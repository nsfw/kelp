/*
  Drive a matrix of GE35 Color Effects LEDs 

  These routines are designed to accept RECTANGULAR matrix of pixel
  values and a map of strands/lights to x,y.

  The Strand/Light map consists of STRAND_COUNT length array of
  arrays of Lights and associated an associated x,y source pixel.

  Note: The output display is most likely NOT Rectangular and there is
  no correlation between input row/column and
  "strand"/"light". Further, it's often desirable to map the same
  source pixel to multiple output lights.

  All LEDs are updated each frame, in strand/index order. 

  Scott -- alcoholiday at gmail
*/

/* Perfomance Issues and Improvements -

   Much of the time is taken in the CPU BOUND composeAndSendFrame function,
   which is called once per LED on the longest strand. It can
   definately use some optimization.

   sendIMGPara()
    foreach LED per strand:	// x36 times
     foreach STRAND:		// x11 times
      composeAndSendFrame()
         makeFrame()		    // encodes data for an LED into a byte buffer
         defferredSendFrame()	// sets bits for serial stream
       sendFrame (enable ISR to shift out 'frame' for all strands)

Current performance w/ ISR:
 2 x 34 long strand, < 1ms between frames 8ms between sendImage yielding 66ms, or
 roughly 15hz. 
           
*/

// #define DEBUG_TIMING

#ifdef __PIC32MX__
#define OPT_BOARD_INTERNAL
#undef USE_ISR
#else
#define USE_ISR
#endif

#if ARDUINO>=100
#include <Arduino.h>	// Arduino 1.0
#else
#include <Wprogram.h>	// Arduino 0022
#endif

#define GE35_NO_DATA	// don't instantiate the 'strand' structure
#include "GE35.h"		// includes configuration information (e.g. mapping)

GE35::GE35(){
    rgb black = {0,0,0};
    portAmask = 0;		// will accumulate output bits for pins we're driving
    portCmask = 0;
    imgBright = MAX_INTENSITY; 
    fill(out,black);

    debugLevel=0;
    debugX=0;
    debugY=0;
}

extern strand strands[];

// ISR related variables
volatile bool sendFrameFlag;			// flags that there is data to send
volatile byte sendISRPingPong = 0;
byte tribitTimerValue;
byte sendFramePingPong = 0;	// one to send, one to update
GE35 *myGE35 = 0;

void GE35::init() {
    int i=0;
    myGE35 = this;			// used in ISR 

    while (i < STRAND_COUNT) {
        digitalWrite(strands[i].pin, LOW);
        pinMode(strands[i].pin, OUTPUT);
        Serial.print("Configured strand ");
        Serial.print(i, DEC);
        Serial.print(" on output pin ");
        Serial.print(strands[i].pin, DEC);
        Serial.print("\n");
        i++;
    }
    Serial.println("Output Pins Configured");

    // init data sending interrupt routine on TIMER2
    sendFrameFlag = 0; 		// nothing to send

#ifdef USE_ISR
	// no interrupt during setup
    TIMSK2 &= ~(1<<TOIE2);	// clear timer2 interrupt enable

	//timer2 in counter mode
    TCCR2A &= ~((1<<WGM21) | (1<<WGM20));
    TCCR2B &= ~(1<<WGM22);

	// internal clock
    ASSR &= ~(1<<AS2);

	// Compare Match A interruption disabled : we only need overflow
	TIMSK2 &= ~(1<<OCIE2A);

	// Prescaler setup to divide CPU clock by 1
	TCCR2B |= (1<<CS20);
	TCCR2B &= ~((1<<CS21)|(1<<CS22));

	// Compute timer value for 10us 
	// ( CPU Frequency ) / (prescaler value) = 16,000,000 Hz.
	// 10us / 1/16us = 160.
	// 256 – 160 = 96;
    tribitTimerValue = 96 + 46;		// AT MEGA overhead compensation (10.06us)

	// TCNT2 value loaded and interrupt enabled
	TCNT2 = tribitTimerValue;
	TIMSK2 |= (1<<TOIE2);	// enable interrupt
#endif

    Serial.println("Initializing Strands");

    rgb initColor = {128,0,255};
    fill(out,initColor);	// put something in the buffer

    // NOTE: First time since power up, will assign addresses.
    // If image buffer doesn't match strand config, interesting things
    // will happen!

    // NOTE: this will assign unique addresses for each LED on a
    // strand. We can optimize by assigning the same address for LEDs
    // on the SAME STRAND that reference the same pixel

    // sendImageSerial();
    delay(1000);
    sendImagePara();

    Serial.println(" -- done");
}

#ifdef USE_ISR
ISR(TIMER2_OVF_vect)
{
    TCNT2 = tribitTimerValue;		// reload timer
    if(myGE35) myGE35->sendFrameISR();
}
#endif

///////////////////////////////////////////////////////////////////////////////
// Library
///////////////////////////////////////////////////////////////////////////////

void GE35::fill(rgb buf[][IMG_WIDTH], rgb c){
    // fill the frame buffer with a color
    for(byte x=0; x<IMG_WIDTH; x++){
        for(byte y=0; y<IMG_HEIGHT; y++){
            buf[y][x]=c;
        }
    }
}

char blockingReadChar(){
    while(!Serial.available());	// wait for it
    return Serial.read();
}

void GE35::sendSingleLED(byte address, int pin, byte r, byte g, byte b, byte i) {
    byte buffer[26];
    makeFrame(address, r, g, b, i, buffer);
    deferredSendFrame(pin, buffer);
    sendFrame();
}

void GE35::makeFrame(byte index, byte r, byte g, byte b, byte i, byte *buffer){
    // creates 26 byte version of 26bit payload
    int bufferPos = 0;
    int bitPos;
    int data;

    while (bufferPos < 26) {
        switch (bufferPos) {
        case 0:			// first 6 bits are INDEX of LED
            bitPos = 6;
            data = index;
            break;
        case 6:			// 8 bits for INTENSITY
            bitPos = 8;
            data = i;	
            break;
        case 14:		// 4 bits of BLUE
            bitPos = 4;
            data = b>>4;
            break;
        case 18:		// 4 bits of GREEN
            bitPos = 4;
            data = g>>4;
            break;
        case 22:		// 4 bits of RED
            bitPos = 4;
            data = r>>4;
            break;
        default:
            break;
        } 
        
        buffer[bufferPos] = ( (data & (1 << (bitPos - 1))) != 0) ? 1:0;
        bitPos--;
        bufferPos++;
    }
}

void GE35::displayTimeSince(unsigned long then, char * desc){
#ifdef DEBUG_TIMING
     unsigned long diff = millis() - then;
     Serial.print(desc);
     Serial.print(": ");
     Serial.print(diff);
     Serial.print("ms\n");
#endif
     }

void GE35::sendImagePara(){
    unsigned long sendIMGParaEntry = millis();

    // walk the strand length ~230ms
    for(byte i=0; i < MAX_STRAND_LEN; i++ ){
        // compute what index each strand should send
        for( byte j=0; j < STRAND_COUNT; j++)
            row[j] = (i < strands[j].len )? i: -1;
        composeAndSendFrame();
    }
    displayTimeSince(sendIMGParaEntry, "sendIMGPara");
}

void GE35::setGlobalIntensity(byte val){
    byte buffer[26];
    makeFrame(0xff, 0x80,0x80,0x00, val, buffer);
    // collect bit streams for ALL strands
    for (byte s=0; s<STRAND_COUNT; s++)
        deferredSendFrame(strands[s].pin, buffer);
    sendFrame();
}


///////////////////////////////////////////////////////////////////////////////
// Low Level I/O
///////////////////////////////////////////////////////////////////////////////
//
// Serial Protocol:
//
// Idle bus state: Low
// Start Bit: High for 10µSeconds
// 0 Bit: Low for 10µSeconds, High for 20µSeconds
// 1 Bit: Low for 20µSeconds, High for 10µSeconds
// Minimum quiet-time between frames: 30µSeconds
//
// Phase 1 2 3 
// 0 =   L H H 
// 1 =   L L H

#ifndef __PIC32MX__	
// ARDUINO SPECIFIC
// !!WARNING!! THIS IS NON PORTABLE!! !!WARNING!!

void GE35::setPin(byte pin){
    if(pin<30) PORTA |= (1<<(pin-22));
    else PORTC |= (1<<(pin-22));
}
void GE35::clrPin(byte pin){
    if(pin<30) PORTA &= ~(1<<(pin-22));
    else PORTC &= ~(1<<(pin-22));
}

byte * GE35::getBufferAndMask(byte pin, byte &pinmask){
    if(22 <= pin && pin <= 30){	// PORTA
        pinmask = (1<<(pin-22));
        portAmask |= pinmask;	// remember we're using this output pin
        return portAframe[sendFramePingPong];
    } else if(30 <= pin && pin <= 38){
        pinmask = (0x80>>(pin-30));	// bit 0 = pin 37
        portCmask |= pinmask;	// remember we're using this output pin
        return portCframe[sendFramePingPong];
    }
}

#else	// chipKIT (pic32)

#define MAXPORT _IOPORT_PG

// Define "Frame buffers" for PortsA(1) through PortG(7)
// NOTE: PORT0 is not defined!
uint16_t portMasks[2][MAXPORT+1];			// remember what pins are being set - 0 = none
uint16_t portFrames[2][MAXPORT+1][FRAMESIZE];

extern const uint16_t PROGMEM digital_pin_to_bit_mask_PGM[];
extern const uint8_t digital_pin_to_port_PGM[];
extern const uint32_t port_to_output_PGM[];

void GE35::clearPortMasks(){
    for(int i=0; i<=MAXPORT; i++)
        portMasks[sendFramePingPong][i]=0;
}

uint16_t * GE35::getBufferAndMask(byte pin, uint16_t &pinmask){
    int port = digital_pin_to_port_PGM[pin];
    if(!port){
        Serial.print("INVALID PORT FOR PIN ");
        Serial.println(pin);
        return 0;
    }
    pinmask = digital_pin_to_bit_mask_PGM[pin];
    portMasks[sendFramePingPong][port] |= pinmask;	// remember we're writing this pin
    // DUMPVAR("gb port", port);
    // DUMPVAR(" gb pinmask", pinmask);
    return portFrames[sendFramePingPong][port];		// return pointer to port buffer
}


#endif

void GE35::composeAndSendFrame(){
    // Compose bit pattern to send for a particular LED across all active strands
    // and then send it in one bollus

    // TIMING ANALYSIS: This is called once per MAX LED per Strand
    // (~36 times for an image on RV) This is where we're spending
    // most of our time, and have been in the <4fps range... should be
    // in the ~10fps range now.p
    
    byte buffer[26];

    clearPortMasks();	// keep track of pins we actually xmit on

    // Accumulate bit streams for ALL strands in portAframe[], portCframe[], etc...
    unsigned long composeLoop = millis();
    for (byte s=0; s<STRAND_COUNT; s++){
        int index = row[s];
        if (index != -1){
            int x = strands[s].x[index];
            int y = strands[s].y[index];
            rgb *pix = &out[y][x];
            if(debugLevel==1 && x==debugX && y==debugY){
                Serial.print("out["); Serial.print(x); Serial.print(",");
                Serial.print(y); Serial.print("]=");
                DUMPRGB(pix->r,pix->g,pix->b);
            }
                
            // unsigned long makeFrameTime = millis();
            makeFrame(index, pix->r, pix->g, pix->b, imgBright, buffer);
            // displayTimeSince(makeFrameTime,"makeFrame");
            // unsigned long defferedSendFrameTime = millis();
            deferredSendFrame(strands[s].pin, buffer);
            // displayTimeSince(defferedSendFrameTime, "defferedSendFrame");
            // if (periodic) (*periodic)();
        }
    }
    // displayTimeSince(composeLoop,"composeLoop");
    unsigned long sendFrameTime = millis();

    // sends accumulated bitstreams out at max serial rate
    sendFrame();
    // displayTimeSince(sendFrameTime, "sendFrame");
}

#define sliceSet(s) *s |= pinmask;
#define sliceClr(s) *s &= ~pinmask;

void GE35::deferredSendFrame(byte pin, byte *bitbuffer){
    // toggle associated bit in associated port buffer array based on data in
    // bitbuffer, representing the 26bit pattern to send on this pin

    uint16_t pinmask;

    // points at appropriate buffer (respects ping-pong) for this pin and sets pinmask
    uint16_t *slicePtr = getBufferAndMask(pin, pinmask);

    sliceSet(slicePtr++);   // start bit
    for(byte i=0; i<26; i++){
        if(bitbuffer[i]){   // send a 1 : L L H
            sliceClr(slicePtr++);
            sliceClr(slicePtr++);
            sliceSet(slicePtr++);
        } else {            // send a 0: L H H
            sliceClr(slicePtr++);
            sliceSet(slicePtr++);
            sliceSet(slicePtr++);
        }
    }
    sliceClr(slicePtr++);   // back to LOW inter frame
}

void GE35::sendFrame(){

#ifdef USE_ISR
    while(sendFrameFlag);   // wait if we're still processing last buffer
#endif

    sendISRPingPong = sendFramePingPong;        // send current buffer
    sendFramePingPong=(sendFramePingPong+1)&1;	// toggle buffers
    sendFrameFlag = 1;

#ifndef USE_ISR
// Just delay instead of using ISR
    while(sendFrameFlag){
        sendFrameISR();
        delayMicroseconds(6);	
    }
#endif
}


void GE35::sendFrameISR(){
    // Say it in one precise parallel blast for all strands
    // Send the buffer selected by sendISRPingPong

    static byte i = 0;	// current 'tribit'

    volatile uint32_t	*latchPort;
    uint16_t mask;
    uint16_t current;
    uint16_t *portFrame;
    
    if(!sendFrameFlag) return;

    // selectively set bits
    for(int port=1; port <= MAXPORT; port++){
        mask = portMasks[sendISRPingPong][port];
        // DUMPVAR("port", port);
        // DUMPVAR(" mask", mask);
        if(mask){	// only process if activity on this port
            latchPort = portOutputRegister(port);
            current = *portInputRegister(port);
            *latchPort = (current & ~mask) | (portFrames[sendISRPingPong][port][i] & mask);
            // DUMPVAR(" latchPort",(uint32_t) latchPort);
            // DUMPVAR(" current", current);
            // DUMPVAR(" rb:",*portInputRegister(port));
        }
    }

    i++;
    
    if(i>=FRAMESIZE){
        i=0;				// reset
        sendFrameFlag = 0;	// done sending
    }
}

void GE35::dumpFrame(byte *buffer){
    Serial.print("frame: ");
    for(byte i = 0; i < 26; i++) Serial.print((int) buffer[i]);
}
