/*
  KELP! A volumetric display using GE35 Color Effects LEDs
*/

// MAPPING - NOTE: currently strand LED mapping is in ge35mapping.h
// Rename your LED mapping file to ge35mapping.h
// Further, there are constants in GE35.h that need to match up with those
// in the mapping file. Sorry!
#include "GE35.h"
#include "GE35mapping.h"

#include <string.h>

// Ethernet Support
#include <SPI.h>
#include <Client.h>
#include <Ethernet.h>
#include <Server.h>
#include <Udp.h>

// rgb <-> hsv
#include "RGBConverter.h"
RGBConverter converter;

// debug 
#define DBG	// conditional DBG code compiled in - small speed penalty
#define DEBUG_TIMING	// may cause significant serial traffic

// remotely settable -- e.g.  osc("/debug",100)
byte debugLevel = 0;	// debugLevel > 100 will print each pixel as sent via /screen

// initialization behavior
#define rgbrgbinit 

// Ethernet - IP ADDRESS
#ifdef DIRECT_CONNECT
byte myMac[] = { 0xDE, 0xAD, 0xBE, 0xEF, 0xFE, 0xED };
byte myIp[]  = { 198, 178, 187, 122 };
#endif
#define RVIP
#ifdef RVIP
byte myMac[] = { 0xBE, 0xEF, 0xBE, 0xEF, 0xBE, 0xEF };
byte myIp[]  = { 192, 168, 69, 69 };
#endif
int  serverPort  = 9999;

// OSC
#include <ArdOSC.h>
OSCServer osc;
OSCMessage *oscmsg;

// FRAME BUFFER
rgb img[IMG_HEIGHT][IMG_WIDTH]={128,0,255};		// source image from controller

rgb white = { 255, 255, 255 };
rgb black = { 0,0,0 };
rgb red = {255, 0, 0};
rgb green = {0, 255, 0};
rgb blue = {0,0,255};

// Setable via OSC
float hScrollRate=0.0;
float vScrollRate=0.0;
float hueScrollRate=0.0;
float hsPos=0;
float vsPos=0;
float huePos=0;

int solidMode = 0;
rgb currentColor={255, 0, 0};
int displayCurrentColor=0;

GE35 ge35;

void setup() {
    Serial.begin(57600);
    Serial.println("Device Start -- ");
    // pf("IP: %d.%d.%d.%d:%d\n", myIp[0], myIp[1], myIp[2], myIp[3], serverPort);

    Serial.println("Initing GE35 --");
    ge35.init();

    Ethernet.begin(myMac ,myIp); 
    osc.sockOpen(serverPort);

    debugLevel=0;
    hueScrollRate=0.00;		// and make it do something while wait for something to do
    vScrollRate=0.8;

    resetDisplay(0);			// put *something* in the frame buffer
}

byte noOSC=1;

void loop(){
    Serial.println("In loop()");
    static int i=0;
    static int dirty=0;
    while(osc.available()){	// process all prior to displaying
        dirty=1;
        if(noOSC){
            resetDisplay(0);	// get back to a known state if someone is talking to us
            noOSC=0;
        }
        oscmsg=osc.getMessage();
        oscDispatch();
    }    
    if(dirty || hueScrollRate || vScrollRate || hScrollRate || displayCurrentColor ){
        prepOutBuffer();	// copies image buffer to OUT (may process)
        ge35.sendImage();	// copy output buffer to LEDS
    }
}

///////////////////////////////////////////////////////////////////////////////
// OSC "handlers"
///////////////////////////////////////////////////////////////////////////////

void panelEnable(int p, int enable){
    // consider losing this, we never use it - turns off one side
    // or the other of the RV.

    int start=0, end=5;
    if(p==1){start=6; end=11;}

    for (int i=start; i<=end; i++){
        ge35.strandEnabled[i]=enable;
    }
}

void oscDispatch(){
    static int resetcount=0;

    char *p = oscmsg->getOSCAddress();

    if(*p != '/'){
        Serial.println("MALFORMED OSC");
        return;
    }

    if(debugLevel){
        Serial.print("osc: ");
        Serial.println(p);
    }

    if(!strncasecmp(p,"/1",2)) p+=2;	// skip page number on TouchOSC

    p++;	    // skip leading slash

    if(!strncasecmp(p,"screen",6)){
        copyImage();
    } else if(!strncasecmp(p,"bright",5)){
        brightness(oscmsg->getFloat(0)); 
    } else if(!strncasecmp(p,"hscroll",7)){
    } else if(!strncasecmp(p,"vscroll",7)){
        vScrollRate=oscmsg->getFloat(0); 
    } else if(!strncasecmp(p,"huescroll",8)){
        hueScrollRate=oscmsg->getFloat(0); 
    } else if(!strncasecmp(p,"hvscroll",8)){
        hScrollRate=oscmsg->getFloat(1); 
        vScrollRate=oscmsg->getFloat(0); 
    } else if(!strncasecmp(p,"fill",4)){
        // fill framebuffer w/ an rgb(float) color
        rgb c;
        if(oscmsg->getArgsNum()==4){
            c.r=oscmsg->getFloat(0)*255;
            c.g=oscmsg->getFloat(1)*255;
            c.b=oscmsg->getFloat(2)*255;
            fill(c);
        } else {
            Serial.println("err: /fill expects 3 floats");
        }
    } else if(!strncasecmp(p,"reset",5)){
        resetDisplay(resetcount++);		// back to a known state
    } else if(!strncasecmp(p,"noScroll",5)){
        noScroll();						// just kill scroll and reset screen position
    } else if(!strncasecmp(p,"setyx",5)){
        // Just set a single pixel!
        int y, x;
        rgb c;
        if(oscmsg->getArgsNum()==6){
            y = oscmsg->getInteger32(0);
            x = oscmsg->getInteger32(1);
            c.r=oscmsg->getFloat(2)*255;
            c.g=oscmsg->getFloat(3)*255;
            c.b=oscmsg->getFloat(4)*255;
            // c.a=0xff;	//  make this optionally settable
            if(x<IMG_WIDTH && y<IMG_HEIGHT){
                img[y][x] = c;
            }
        } else {
            Serial.println("err: /setyx expects i,i,f,f,f");
        }
    } else if(!strncasecmp(p,"rgb",3)){
        // process "/effect/rgb/1..3 [0.0 .. 1.0] messages
        int i = p[4]-'1';		// 1..3
        byte *c = (byte*) &currentColor;
        c[i]=(int) 255*oscmsg->getFloat(0);
        if(solidMode){			// set whole screen to this color
            fill(currentColor);
        } else 
            displayCurrentColor=10;		// show current color for this many cycles
    } else if(!strncasecmp(p,"clear",5)){
        fill(black);
    } else if(!strncasecmp(p,"solid",5)){
        solidMode = oscmsg->getFloat(0);
        Serial.println("Solid Mode");
        Serial.println(solidMode);
    } else if(!strncasecmp(p,"grid",4)){
        // format: grid1/4/1, grid2/5/12
        // grid1/9/1
        int pan = (p[4]=='2');
        int row = 9-(p[6]-'0');	// sends 1-9 (upside down)
        int col = p[8]-'0';
        if(p[9]) col = 10 + p[9]-'0';
        col = col - 1 + pan*15;
        // Serial.println(p);
        // Serial.println(row);
        // Serial.println(col);
        img[row][col] = currentColor;
    } else if(!strncasecmp(p,"panel",5)){
        // enable or disable a panel
        // /panel panel#, mode
        Serial.println("handling panel");
        
        int pan = oscmsg->getInteger32(0);
        int mode = oscmsg->getInteger32(1);
        if(!mode) fill(black);
        panelEnable(pan,mode);
        // } else if(!strncasecmp(p,"datarate",8)){
        // debug method - set serial data rate! tribit quiettime
        // tribit    = oscmsg->getInteger32(0);
        // quiettime = oscmsg->getInteger32(1);
        // pf("tribit = %d quiettime = %d", tribit, quiettime);
    } else if(!strncasecmp(p,"debug",5)){
        debugLevel=oscmsg->getInteger32(0);	// set debug level
    } else {
        Serial.print("Unrecognized Msg: ");
        Serial.println(p);
    }
}

void copyImage(){
	//
    // copy image data from OSC to framebuffer
    // 
    int h = oscmsg->getInteger32(0);
    int w = oscmsg->getInteger32(1);

    // Inbound Image must at least as big as our measly frame buffer in size
    if(w<IMG_WIDTH || h<IMG_HEIGHT){
        Serial.println("Inbound Image must at least as big as our measly frame buffer in size");
        return;
    }

    byte *data = (byte*) oscmsg->getBlob(2)->data;
#ifdef DBG
    if(debugLevel==101){
        // pf("Blob Length: %d\n",oscmsg->getBlob(2)->len);
        dumpHex(data, "ScreenData:", 4);
    }
#endif

    for(byte x=0; x<IMG_WIDTH; x++){
        for(byte y=0; y<IMG_HEIGHT; y++){
            rgb *d = &img[y][x];
            byte *s = data + ((x+(y*w))<<2);	// src pixels in uint32
            d->r = *s++;
            d->g = *s++;
            d->b = *s++;
            // skip alpha
#ifdef DBG
            // if(debugLevel>100) pf("[%d][%d]=%d,%d,%d ",y,x,d->r,d->g,d->b);
#endif
        }
#ifdef DBG
        if(debugLevel>100) Serial.println("/n");
#endif
    }
}


///////////////////////////////////////////////////////////////////////////////
// Initial Frame Buffer functions
///////////////////////////////////////////////////////////////////////////////

#ifdef rgbrgbinit
void initFrameBuffer(int i){
    // just stick some pattern in it for now
    for(byte x=0; x<IMG_WIDTH; x++){
        for(byte y=0; y<IMG_HEIGHT; y++){
            static int z=0;
            z=y%8;
            img[y][x]= (z==0||z==1)?red:((z==2||z==3)?green:(z==4||z==5)?blue:black);
//             img[y][x]= (z==0)?red:((z==1)?green:blue);
//            z=(++z)%3;
        }
    }
}
#endif

#ifdef conf9x10
void initFrameBuffer(int i){
    i=i%3000;
    if(i<1000){
        // just stick some pattern in it for now
        for(byte x=0; x<IMG_WIDTH; x++){
            for(byte y=0; y<IMG_HEIGHT; y++){
                i = ((test9x10[y][x]+i)-1)%3;
                img[y][x]= (i==0)?red:((i==1)?green:blue);
            }
        }
    } else {
        // rows and columns
        for(byte x=0; x<IMG_WIDTH; x++){
            for(byte y=0; y<IMG_HEIGHT; y++){
                int z = (i<2000)? x%3:y%3;
                img[y][x]= (z==0)?red:((z==1)?green:blue);
            }
        }
    }
}
#endif

void noScroll(){
    // stop scroll and reset screen position
    hScrollRate=vScrollRate=hueScrollRate=0.0;	// rates
    hsPos=vsPos=huePos=0;	// and positions
}

void resetDisplay(int i){
    noScroll();
    initFrameBuffer(i);
}

///////////////////////////////////////////////////////////////////////////////
// Library
///////////////////////////////////////////////////////////////////////////////

void fill(struct rgb c){
    // fill the frame buffer with a color
    for(byte x=0; x<IMG_WIDTH; x++){
        for(byte y=0; y<IMG_HEIGHT; y++){
            img[y][x]=c;
        }
    }
}

float bright=1.0;

void brightness(float b){
    bright=b;
    bright = max(0.0, bright);
    bright = min(1.0, bright);
    ge35.imgBright = (float)MAX_INTENSITY*bright;
}

void prepOutBuffer(){
    // copy img[][] -> out[][] w/ possible transforms
    // consider adding a "hue scroll" that cycles colors

    hsPos+=hScrollRate;
    vsPos+=vScrollRate;
    huePos+=hueScrollRate;

    if(displayCurrentColor) --displayCurrentColor;

    for(byte x=0; x<IMG_WIDTH; x++){
        for(byte y=0; y<IMG_HEIGHT; y++){
            int ny = y+vsPos;
            int nx = x+hsPos;
            rgb *s = &img[abs(ny%IMG_HEIGHT)][abs(nx%IMG_WIDTH)];
            if(displayCurrentColor){
                // override output w/ current color
                ge35.out[y][x] = currentColor;
            } else if(hueScrollRate!=0.0) {
                float hsv[3];
                converter.rgbToHsv(s->r, s->g, s->b, hsv);
                converter.hsvToRgb(fabs(fmod(hsv[0]+huePos,1.0)), hsv[1], hsv[2],
                                   (byte*) &ge35.out[y][x]);
            } else {
                ge35.out[y][x] = *s;
            }
        }
    }

}

///////////////////////////////////////////////////////////////////////////////
// debug utils
///////////////////////////////////////////////////////////////////////////////

static void	dumpHex(void * startAddress, char* name, unsigned lines){
    int				ii;
    int				theValue;
    int				lineCount;
    char			textString[16];
    char			asciiDump[24];
    unsigned long	myAddressPointer;
    
	lineCount			=	0;
	myAddressPointer	=	(unsigned long) startAddress;
    sprintf(textString, "%s:\n", name);
    Serial.print(textString);
	while (lineCount < lines) {
		sprintf(textString, "%04X - ", myAddressPointer);
		Serial.print(textString);
		
		asciiDump[0]		=	0;
		for (ii=0; ii<16; ii++) {
			theValue	=	pgm_read_byte_near(myAddressPointer);

			sprintf(textString, "%02X ", theValue);
			Serial.print(textString);
			if ((theValue >= 0x20) && (theValue < 0x7f)) {
				asciiDump[ii % 16]	=	theValue;
            }
			else {
				asciiDump[ii % 16]	=	'.';
			}
			
			myAddressPointer++;
		}
		asciiDump[16]	=	0;
		Serial.println(asciiDump);
	
		lineCount++;
	}
}

void dumpFrame(byte *buffer){
    Serial.print("frame: ");
    for(byte i = 0; i < 26; i++) Serial.print((int) buffer[i]);
}


