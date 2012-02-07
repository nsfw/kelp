// KELP! A volumetric* display using GE35 Color Effects LEDs
//
// (*OK, it really depends on how you lay things out, it can totally
//  support 2D or even 1D displays if you desire. And I guess, in some
//  ways 4D displays, or even higher dimensional ones if you get extra
//  clever.)
//
// Put your display details in GE35mapping.h
//
// Scott - alcoholiday googlemailservicedotcom
//
// v1.0 - 02FEB12 - Kind of working!
//

#include "GE35.h"

#include <string.h>

// #define chipKIT 1 -- defined in platforms.txt (should be __PIC32MX__)

#ifdef __AVR__  // ARDUINO

// Ethernet Support
// #include <SPI.h>
// #include <Client.h>
// #include <Ethernet.h>
// #include <Server.h>
// #include <Udp.h>
// // OSC
// #include <ArdOSC.h>

#endif 

#ifdef __PIC32MX__ // -- chipKIT32 --

#include <chipKITEthernet.h>
#include <chipKITOSC.h>
#define strncasecmp strncmp

#endif

OSCServer osc;

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

// #define RVIP
#ifdef RVIP
byte myMac[] = { 0xBE, 0xEF, 0xBE, 0xEF, 0xBE, 0xEF };
byte myIp[]  = { 192, 168, 69, 69 };
#endif

#define CHIPKIT_TEST
#ifdef CHIPKIT_TEST
byte myMac[] = { 0, 0, 0, 0, 0, 0 };	// let ethernet library assign something
byte myIp[]  = { 139, 104, 88, 199 };
#endif

int  serverPort  = 9999;

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


#ifdef __PIC32MX__

void periodic(){
    static int i=0;
    // On chipKIT high volume ethernet activity needs high volume service
    if((i++ & 3)==0)  Ethernet.PeriodicTasks();
}

#include "exceptions.h"

void dumpExceptionInfo(){
    Serial.println("## EXCEPTION ############################################");
    if (_excep_code == EXCEP_Trap) {
        Serial.print("Trap exception: ");
    } else if (_excep_code == EXCEP_DBE) {
        Serial.print("Bus error (load/store) exception: ");
      } else {
        Serial.print("Unknown exception: ");
    }
    Serial.print(_excep_code);
    Serial.println();
    Serial.println("#######################################################");
#endif
}

void setup() {
    Serial.begin(57600);

    Serial.println("Device Start -- ");
    DUMPVAR("IP: ",(int) myIp[0]);
    DUMPVAR(".",(int) myIp[1]);
    DUMPVAR(".",(int) myIp[2]);
    DUMPVAR(".",(int) myIp[3]);
    DUMPVAR(" port: ", serverPort);
    Serial.println("");

    Serial.println("Initing GE35 --");
    ge35.init();

    Ethernet.begin(myMac ,myIp); 

#ifdef __AVR__
    osc.sockOpen(serverPort);
#endif
#ifdef __PIC32MX__
    osc.begin(serverPort);		// newer version of OSC
    osc.addCallback("*",oscDispatch);
#endif

    resetDisplay(0);			// put *something* in the frame buffer
    ge35.sendImage();

    // ge35.setPeriodicFx(periodic);

    debugLevel=0;
    noScroll();

    ge35.setDebugLevel(1);
    ge35.setDebugXY(IMG_WIDTH+1,0);	// disables output

    // debug 
    digitalWrite(22, LOW);
    pinMode(22, OUTPUT);
}

byte noOSC=1;
byte noUpdate=0;		// some OSC commands don't need a graphic refresh!

#ifdef __AVR__
// no try/catch on AVR
#define TRY
#define CATCH if(0)
#endif

void loop(){
    TRY {
        // Serial.println("loop");
        static int dirty=0;
        static int osccnt=0;
        static int loopcnt=0;
        int ret = 0;
        while((ret=osc.availableCheck())>0){	// process all prior to displaying
            // above has side effect of calling handler
            // Serial.print("OSC:"); Serial.println(osccnt++);
            dirty=1;
            if(noOSC){
                resetDisplay(0);	// get back to a known state if someone is talking to us
                noOSC=0;
            }
        }
        if(ret<0) Serial.print("?");	// error in decode
        
#ifdef __PIC32MX__
        Ethernet.PeriodicTasks();	// chipKIT service Ethernet
#endif

        if(noOSC){
            // Serial.println("idle");
            resetDisplay(loopcnt++);
            dirty=1;
        }
        
        if(!noUpdate &&
           (dirty || hueScrollRate || vScrollRate || hScrollRate || displayCurrentColor )){
            prepOutBuffer();	// copies image buffer to OUT (may process)
            ge35.sendImage();	// copy output buffer to LEDS
            // Serial.print(".");
            dirty = 0;
        }

        noUpdate = 0;
        
        // debug - output update rate
        static int tog = 0;
        digitalWrite(22, tog++&0x01);
    } CATCH {
        dumpExceptionInfo();
    }
}

///////////////////////////////////////////////////////////////////////////////
// OSC "handlers"
///////////////////////////////////////////////////////////////////////////////

void oscDispatch(OSCMessage *oscmsg){
    static int resetcount=0;

    char *p = oscmsg->getOSCAddress();

    if(*p != '/'){
        Serial.println("M");
        if(debugLevel) dumpHex(oscmsg, "oscmsg", 4);
        return;
    }

    if(debugLevel){
        Serial.print("osc: ");
        Serial.println(p);
    }

    if(!strncasecmp(p,"/1",2)) p+=2;	// skip page number on TouchOSC

    p++;	    // skip leading slash

    if(!strncasecmp(p,"screenxy",8)){
        copyImageXY(oscmsg);	// copy to screen with x,y offset
        Serial.print("+");
    } else if(!strncasecmp(p,"screen",6)){
        // NOTE: changed to W, H, data... (from H, W)
        copyImage(oscmsg);		// copy to x,y
    } else  if(!strncasecmp(p,"bright",5)){
        brightness(oscmsg->getArgFloat(0)); 
    } else if(!strncasecmp(p,"hscroll",7)){
    } else if(!strncasecmp(p,"vscroll",7)){
        vScrollRate=oscmsg->getArgFloat(0); 
    } else if(!strncasecmp(p,"huescroll",8)){
        hueScrollRate=oscmsg->getArgFloat(0); 
    } else if(!strncasecmp(p,"hvscroll",8)){
        hScrollRate=oscmsg->getArgFloat(1); 
        vScrollRate=oscmsg->getArgFloat(0); 
    } else if(!strncasecmp(p,"fill",4)){
        // fill framebuffer w/ an rgb(float) color
        rgb c;
        if(oscmsg->getArgsNum()==3){
            c.r=oscmsg->getArgFloat(0)*255;
            c.g=oscmsg->getArgFloat(1)*255;
            c.b=oscmsg->getArgFloat(2)*255;
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
            y = oscmsg->getArgInt32(0);
            x = oscmsg->getArgInt32(1);
            c.r=oscmsg->getArgFloat(2)*255;
            c.g=oscmsg->getArgFloat(3)*255;
            c.b=oscmsg->getArgFloat(4)*255;
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
        c[i]=(int) 255*oscmsg->getArgFloat(0);
        if(solidMode){			// set whole screen to this color
            fill(currentColor);
        } else 
            displayCurrentColor=10;		// show current color for this many cycles
    } else if(!strncasecmp(p,"clear",5)){
        fill(black);
    } else if(!strncasecmp(p,"solid",5)){
        solidMode = oscmsg->getArgFloat(0);
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
    } else if(!strncasecmp(p,"debugxy",7)){
        int x = oscmsg->getArgInt32(0);
        int y = oscmsg->getArgInt32(1);
        ge35.setDebugXY(x,y);
        Serial.print("debugXY set to: ");
        Serial.print(x);
        Serial.print(",");
        Serial.println(y);
    } else if(!strncasecmp(p,"debug",5)){
        debugLevel=oscmsg->getArgInt32(0);	// set debug level
    } else {
        Serial.print("Unrecognized Msg: ");
        Serial.println(p);
    }
}


// Legacy Image mode for RV support
void copyImage(OSCMessage *oscmsg){
	//
    // copy image data from OSC to framebuffer
    // 
    int w = oscmsg->getArgInt32(0);
    int h = oscmsg->getArgInt32(1);

    // Inbound Image must at least as big as our measly frame buffer in size
    if(w<IMG_WIDTH || h<IMG_HEIGHT){
        Serial.println("Inbound Image must at least as big as our measly frame buffer in size");
        return;
    }

    byte *data = (byte*) oscmsg->getArg(2)->_argData;

#ifdef DBG
    if(debugLevel==101){
        // pf("Blob Length: %d\n",oscmsg->getBlob(2)->len);
        dumpHex(data, "ScreenData:", 8);
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
    }
}

void copyImageXY(OSCMessage *oscmsg){
	//
    // copy image data from OSC to framebuffer with OFFSET
    // 
    int w = oscmsg->getArgInt32(0);
    int h = oscmsg->getArgInt32(1);
    int baseX = oscmsg->getArgInt32(2);
    int baseY = oscmsg->getArgInt32(3);

    byte *data = (byte*) oscmsg->getArg(4)->_argData;

#ifdef DBG
    if(debugLevel==101){
        dumpHex(data, "ScreenData:", 4);
    }
#endif

    // DUMPVAR("baseX ",baseX);
    // DUMPVAR("baseY ",baseY);
    
    for(int sy=0; sy<h; sy++){
        for(int sx=0; sx<w; sx++){
            int x = baseX+sx;
            int y = baseY+sy;
            rgb *d = &img[y][x];
            byte *s = data + ((sx+(sy*w))<<2);	// src pixels in uint32
            d->r = *s++;
            d->g = *s++;
            d->b = *s++;
            // skip alpha
            if(debugLevel==101){
                DUMPRGB(img[y][x].r,img[y][x].g,img[y][x].b);
                Serial.print(" ");
            }
        }
#ifdef DBG
        if(debugLevel>100) Serial.println("");
#endif
    }
}

// debug
void walkBulbs(){
    static int i = 0;
    static int y = 0;
    if(i++%1 == 0){
        fill(black);
//        DUMPVAR("y= ",y);
        img[y%IMG_HEIGHT][0] = white;
//        Serial.println();
        y++;
    } else {
//        img[0][0] = red;
//        img[1][0] = blue;
//        img[2][0] = green;
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
            z=(y+(i/100))%8;
            img[y][x]= (z==0||z==1)?red:((z==2||z==3)?green:(z==4||z==5)?blue:black);
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
    Serial.println(name);
	while (lineCount < lines) {
        Serial.print(myAddressPointer, HEX);
		Serial.print(": ");
		asciiDump[0] = 0;
		for (ii=0; ii<16; ii++) {
			// theValue	=	pgm_read_byte_near(myAddressPointer);
            theValue = *(byte *)myAddressPointer;
            Serial.print(theValue, HEX);
			sprintf(textString, "%2X ", theValue);
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


