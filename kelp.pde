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

#include <GE35.h>
#include <string.h>

#ifdef __AVR__  // ARDUINO

// Ethernet Support
// #include <SPI.h>
// #include <Client.h>
// #include <Ethernet.h>
// #include <Server.h>
// #include <Udp.h>
// // OSC
// #include <ArdOSC.h>
// OSCServer udpserver;	// need to fix this for AVR again
// OSCClient udpclient;

#endif 

#ifdef __PIC32MX__ // chipKIT32

#include <DNETcK.h>
#include <chipKITOSC.h>
#define strncasecmp strncmp

#endif
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
// IPv4 myIp = { 139, 104, 88, 199 };
IPv4 myIp = { 192, 168, 1, 69 };	// via wifi
#endif

int  serverPort  = 9999;

// FRAME BUFFER
rgb img[IMG_HEIGHT][IMG_WIDTH]={128,0,255};		// source image from controller

rgb white = {255, 255, 255 };
rgb black = {0, 0, 0 };
rgb red =   {255, 0, 0};
rgb green = {0, 255, 0};
rgb blue =  {0, 0, 255};

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
}

///////////////////////////////////////////////////////////////////////////////
// DNETcK Storage
///////////////////////////////////////////////////////////////////////////////
const int cPending = 1;	 // number of clients the server will hold until accepted

byte rgbUDPClientCache[8096];
UdpClient udpClient(rgbUDPClientCache, sizeof(rgbUDPClientCache));

byte rgbUDPServerCache[cPending * sizeof(rgbUDPClientCache)];
UdpServer udpServer(rgbUDPServerCache, sizeof(rgbUDPServerCache), cPending);

#endif

#define RED_BUTTON_PIN 38		// on J9
#define RED_BUTTON_LIGHT 39		// on J9

void setup() {
    Serial.begin(57600);
    
    Serial.println("Device Start -- ");
    DUMPVAR("IP: ",(int) myIp.rgbIP[0]);
    DUMPVAR(".",(int) myIp.rgbIP[1]);
    DUMPVAR(".",(int) myIp.rgbIP[2]);
    DUMPVAR(".",(int) myIp.rgbIP[3]);
    DUMPVAR(" port: ", serverPort);
    Serial.println("");

	Serial2.begin(9600);
	Serial2.println("HiPehr!");

    delay(100);
    Serial.println("Initing GE35 --");
    ge35.init();

#ifdef __PIC32MX__
    DNETcK::begin(myIp);
#endif

#ifdef __AVR__
    Ethernet.begin(myMac ,myIp); 
    osc.sockOpen(serverPort);
#endif
    
    resetDisplay(0);			// put *something* in the frame buffer
    ge35.sendImage();

    debugLevel=0;
    noScroll();

    ge35.setDebugLevel(1);
    ge35.setDebugXY(IMG_WIDTH+1,0);	// disables output

    // debug 
    digitalWrite(22, LOW);
    pinMode(22, OUTPUT);

	// Button 
	pinMode(RED_BUTTON_PIN, INPUT);
	pinMode(RED_BUTTON_LIGHT, OUTPUT);
	digitalWrite(RED_BUTTON_LIGHT, HIGH);
}

byte noOSC=1;
byte noUpdate=0;		// some OSC commands don't need a graphic refresh!

#ifdef __AVR__
// no try/catch on AVR
#define TRY
#define CATCH if(0)
#endif

#ifdef __PIC32MX__
typedef enum
{
    NONE = 0,
    LISTEN,
    ISLISTENING,
    AVAILABLECLIENT,
    ACCEPTCLIENT,
    READ,
    WRITE,
    CLOSE,
    EXIT,
    DONE
} STATE;

STATE state = LISTEN;

void writeOSC_i(char *path, int32_t val){
	OSCMessage msg;
	msg.beginMessage(path);
	msg.addArgInt32(val);

	byte *sendData=(uint8_t*)calloc( msg.getMessageSize() ,1 );
	OSCEncoder encoder;
	OSCEncoder::encode(&msg,sendData);
	udpClient.writeDatagram(sendData,msg.getMessageSize());
	free(sendData);
}

int buttonState;             // the current reading from the input pin
int lastButtonState = LOW;   // the previous reading from the input pin
long lastDebounceTime = 0;

bool pollButton(){
	int reading = digitalRead(RED_BUTTON_PIN);
	if (reading != lastButtonState) {
		lastDebounceTime = millis();
	} 
	if (buttonState != reading &&
		(millis() - lastDebounceTime) > 50) {	// 50ms
		buttonState = reading;
		Serial.println(buttonState);
		writeOSC_i("/button",buttonState);
	}
	lastButtonState = reading;
}

int readOSC(){
	// returns:
    //  1 if sucessfully processed an OSC packet
    // -1 if error
    //  0 if nothing actionable happened

    static unsigned tStart = 0;
    unsigned int tWait = 0*1000;	// connection timeout, 0 = no timeout, 
    int cbRead = 0;
    int count = 0;
    int retVal = 0;

    byte rgbRead[kMaxRecieveData];		// from OSC library
    OSCMessage msg;

    // manage connection
    switch(state)
    {
        // wait for a new 'client'
    case LISTEN:
        if(udpServer.startListening(serverPort)){
            Serial.println("Listening...");
            state = ISLISTENING;
        } else {
            state = EXIT;
        }
        break;
        // could skip this state... just nice to show what's up!
    case ISLISTENING:
        if(udpServer.isListening()) {
            Serial.print("... on port: ");
            Serial.println(serverPort, DEC);
            state = AVAILABLECLIENT;
        } else {
            state = EXIT;
        }
        break;
        // wait for a connection
    case AVAILABLECLIENT:
        if((count = udpServer.availableClients()) > 0){
            Serial.print(count, DEC);
            Serial.println(" clients pending");
            state = ACCEPTCLIENT;
        }
        break;
        // accept the connection
    case ACCEPTCLIENT:
        udpClient.close(); 	// make sure it's "just constructed"
        if(udpServer.acceptClient(&udpClient)){
            Serial.println("Connected");
            state = READ;
            tStart = (unsigned) millis();
        } else {
            state = CLOSE;	// release the connection if error
        }
        break;

    case READ:
        // will wait tWait ms (if non-zero) for a connection,
        // otherwise will just go back to "listening"

        if((cbRead = udpClient.available()) > 0) {
            cbRead = cbRead < sizeof(rgbRead) ? cbRead : sizeof(rgbRead);
            cbRead = udpClient.readDatagram(rgbRead, cbRead);
            tStart = (unsigned) millis();

            // OSC decode and dispatch
            retVal = 1;
            if( OSCDecoder::decode( &msg ,rgbRead ) < 0 )
                retVal = -1;
            else
                oscDispatch(&msg);
        } else if( tWait && (((unsigned) millis()) - tStart) > tWait ) {
            state = CLOSE;
        }
        break;
        
        // close our udpClient and go back to listening
    case CLOSE:
		udpClient.close();	// close when we get a new connection
        Serial.println("Closed UdpClient");
        Serial.println("");
        state = ISLISTENING;
        break;

        // something bad happen, just exit out of the program
    case EXIT:
        udpClient.close();
        state = LISTEN;
        // udpServer.close();
        // Serial.println("Something went wrong, sketch is done.");  
        // state = DONE;
        break;

        // do nothing in the loop
    case DONE:
    default:
        break;
    }

    // Make sure that the Ethernet stack runs
    DNETcK::periodicTasks();

    return retVal;
}
#endif
#ifdef __AVR__
int readOSC(){
    // implement
}
#endif


void doTerry(){
	Serial.print("Terry!");
	Serial2.print("[");
	for(int i=0; i<10; i++){
		rgb *d = &img[(4*64)+(i%8)][4];
		// Serial2.write((byte*) d,3);
		byte x[]={255,0,255};
		Serial2.write(x,3);
		Serial2.print(",");
	}
	Serial2.print("]");
}

void loop(){
    TRY {
        static int dirty=0;
        static int loopcnt=0;
        int ret = 0;

		static unsigned int terryCount = 0;

		if((++terryCount%1000) == 0){
			terryCount=0;
			doTerry();
		}

        while((ret=readOSC())>0){	// process all queued messages
            dirty=1;
            if(noOSC){
                resetDisplay(0);	// get back to a known state if someone is talking to us
                noOSC=0;
            }
        }
        if(ret<0) Serial.print("?");	// error in decode

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
        
		pollButton();

        // output update rate on a pin
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
            //            z=(y+(i/100))%8;
            z=(y+(i/5))%8;
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
            if(theValue<0x10) Serial.print("0");
            Serial.print(theValue, HEX);
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


