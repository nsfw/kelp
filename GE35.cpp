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
       sendFrame       
*/

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
}

extern strand strands[];
void GE35::init() {
    int i=0;
    while (i < STRAND_COUNT) {
        strandEnabled[i]=1;
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

    Serial.println("Initializing Strands");

    rgb initColor = {128,0,255};
    fill(out,initColor);	// put something in the buffer

    // NOTE: First time since power up, will assign addresses.
    // If image buffer doesn't match strand config, interesting things
    // will happen!

    // NOTE: this will assign unique addresses for each LED on a
    // strand We can optimize by assigning the same address for LEDs
    // on the SAME STRAND that reference the same pixel

    // sendImageSerial();
    delay(1000);
    sendImagePara();

    Serial.println(" -- done");
}


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
//        blockingReadChar();
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
        return portAframe;
    } else if(30 <= pin && pin <= 38){
        pinmask = (0x80>>(pin-30));	// bit 0 = pin 37
        portCmask |= pinmask;	// remember we're using this output pin
        return portCframe;
    }
}

void GE35::composeAndSendFrame(){
    // Compose bit pattern to send for a particular LED across all active strands
    // and then send it in one bollus

    // TIMING ANALYSIS: This is called once per MAX LED per Strand
    // (~36 times for an image on RV) This is where we're spending
    // most of our time, and have been in the <4fps range... should be
    // in the ~10fps range now.p
    
    byte buffer[26];

    // Accumulate bit streams for ALL strands in portAframe[], portCframe[], etc...
    unsigned long composeLoop = millis();
    for (byte s=0; s<STRAND_COUNT; s++){
        int index = row[s];
        if ((index != -1) && (strandEnabled[s]!=0)){
            byte x = strands[s].x[index];
            byte y = strands[s].y[index];
            rgb *pix = &out[y][x];
            // unsigned long makeFrameTime = millis();
            makeFrame(index, pix->r, pix->g, pix->b, imgBright, buffer);
            // displayTimeSince(makeFrameTime,"makeFrame");
            // unsigned long defferedSendFrameTime = millis();
            deferredSendFrame(strands[s].pin, buffer);
            // displayTimeSince(defferedSendFrameTime, "defferedSendFrame");
        }
    }
    // displayTimeSince(composeLoop,"composeLoop");
    // unsigned long sendFrameTime = millis();

    // sends accumulated bitstreams out at max serial rate
    sendFrame();
    // displayTimeSince(sendFrameTime, "sendFrame");
}

#define sliceSet(s) *s |= pinmask;
#define sliceClr(s) *s &= ~pinmask;

void GE35::deferredSendFrame(byte pin, byte *bitbuffer){
    // toggle associated bit in associated port buffer array based on data in
    // bitbuffer, representing the 26bit pattern to send on this pin

    byte pinmask;

    // points at appropriate buffer for this pin and sets pinmask
    byte *slicePtr = getBufferAndMask(pin, pinmask);

    sliceSet(slicePtr++);	// start bit
    for(byte i=0; i<26; i++){
        if(bitbuffer[i]){	// send a 1 : L L H
            sliceClr(slicePtr++);
            sliceClr(slicePtr++);
            sliceSet(slicePtr++);
//            Serial.print("1");
        } else {			// send a 0: L H H
            sliceClr(slicePtr++);
            sliceSet(slicePtr++);
            sliceSet(slicePtr++);
//            Serial.print("0");
        }
    }
    sliceClr(slicePtr++);	// back to LOW inter frame
//    Serial.println("");
}

void GE35::sendFrame(){
    // Say it in one precise parallel blast for all strands
    for(byte i = 0; i<FRAMESIZE; i++){
        PORTA = (PORTA & ~portAmask) | (portAframe[i] & portAmask);	// selectively set bits
        PORTC = (PORTC & ~portCmask) | (portCframe[i] & portCmask);	// selectively set bits
        delayMicroseconds(tribit);
    }
    delayMicroseconds(quiettime);	// 30us quiesce
}

void GE35::dumpFrame(byte *buffer){
    Serial.print("frame: ");
    for(byte i = 0; i < 26; i++) Serial.print((int) buffer[i]);
}
