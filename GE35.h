// GE35.h - Drive GE35 LED lights
//
// NOTE: You need to include the MAPPING and CONFIGURATION file
// PRIOR to including this file, for example:
//
// #include "GE35mapping.h"
// #include "GE35.h"
//

#ifndef GE35_h
#define GE35_h

#define OPT_BOARD_INTERNAL	// let us see pin/port details

#if ARDUINO>=100
#include <Arduino.h>	// Arduino 1.0
#else
#include <Wprogram.h>	// Pre 1.0
#endif

#include <stdint.h>
typedef uint8_t byte;

#include "GE35mapping.h"

#define DUMPVAR(s,v) Serial.print(s); Serial.print("=");Serial.println((int)v);
#define DUMPVARH(s,v) Serial.print(s); Serial.print(" 0x");Serial.println((int) v,HEX);
#define DUMPPTR(s,v) Serial.print(s); Serial.print("= ");Serial.println((uint32_t) v, HEX);
#define DUMPRGB(r,g,b) Serial.print((int)r); Serial.print(","); Serial.print((int)g); Serial.print(","); Serial.println((int)b); 

struct rgb {
  byte r;
  byte g;
  byte b;
};

#define FRAMESIZE (2+(26*3))

// Low level Serial Rate -- need to adjust based on uController performance
// tribit - is 1/3rd of a 30us bit time (10us)
// quiettime is 1 bit time (30us)
// Lights seem to be happy running faster (like 7us) too

// !!WARNING!! NOT PORTABLE
// ATMEGA 2560 
#define tribit 8		// # of uSec to delay on ATMEGA2560 (10.04us)
#define quiettime 27	// # of usec quiesce time between frames

// Note: quiettime isn't really nescessary since we're taking a bunch
// of time inbetween sending frames anyhow.

// MAX of 0xff seems to glitch things
#define MAX_INTENSITY 0x0f2

#ifdef __PIC32MX__
#define MAXPORT _IOPORT_PG
extern const uint16_t PROGMEM digital_pin_to_bit_mask_PGM[];
extern const uint8_t digital_pin_to_port_PGM[];
extern const uint32_t port_to_output_PGM[];
#endif

class GE35 {
public:
    byte imgBright;
    rgb out[IMG_HEIGHT][IMG_WIDTH];		// output image buffer
    int row[STRAND_COUNT];				// index of LED to display for each strand

    // debug
    int debugLevel;
    int debugX;
    int debugY;

    // Interrupt 
    /* static volatile bool sendFrameFlag;			// flags that there is data to send */
    /* static volatile int tribitTimerValue; */
    void sendFrameISR();

    // methods
    GE35();
    void init();

    // xmit the OUT buffer to the LEDS
    void sendImage(){ sendImagePara(); };
    void sendImagePara();
    void sendImageSerial();
    void sendSingleLED(byte address, int pin, byte r, byte g, byte b, byte i);

    // util
    void fill(rgb [][IMG_WIDTH], rgb c);

    void setDebugLevel(int level){ debugLevel=level; };
    void setDebugXY(int x, int y){ debugX = x; debugY = y; };

private:
    void makeFrame(byte index, byte r, byte g, byte b, byte i, byte *buffer);
    void displayTimeSince(unsigned long then, char * desc);
    void setGlobalIntensity(byte val);
	// Low Level I/O
    void setPin(byte pin);
    void clrPin(byte pin);
    void clearPortMasks();
    uint16_t *getBufferAndMask(byte pin, uint16_t &pinmask);
    void deferredSendFrame(byte pin, byte *bitbuffer);
    void composeAndSendFrame();
    void sendFrame();	// sends 'deffered' serial comm buffer across all 'ports'
    void dumpFrame(byte *buffer);

public:
	// Deferred I/O storage

#ifdef __AVR__
    // !! Warning this is uController specific !! Not Portable! !!

	// pins 22-29 PORTA - ping / pong frame buffers
	byte portAframe[2][FRAMESIZE];	// start and stop frome + 26 bits 
	byte portAmask;			// remember what pins are being set

	// pins 30-37 PORTC
	byte portCframe[2][FRAMESIZE];	
	byte portCmask;			
#endif

#ifdef __PIC32MX__
// Define "Frame buffers" for PortsA(1) through PortG(7)
// NOTE: PORT0 is not defined!
uint16_t portMasks[2][MAXPORT+1];			// remember what pins are being set - 0 = none
uint16_t portFrames[2][MAXPORT+1][FRAMESIZE];
#endif
};

#endif
