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

typedef unsigned char byte;

#include "GE35mapping.h"

#define DUMPVAR(s,v) Serial.print(s); Serial.print(v);

struct rgb {
  byte r;
  byte g;
  byte b;
};

#define FRAMESIZE (2+(26*3))

// Low level Serial Rate -- need to adjust based on uController performance
// tribit - is 1/3rd of a 30us bit time (10us)
// quiettime is 1 bit time (30us)

// !!WARNING!! NOT PORTABLE
// ATMEGA 2560 
#define tribit 8		// # of uSec to delay on ATMEGA2560 (10.04us)
#define quiettime 27	// # of usec quiesce time between frames

// Note: quiettime isn't really nescessary since we're taking a bunch
// of time inbetween sending frames anyhow.

// MAX of 0xff seems to glitch things
#define MAX_INTENSITY 0x0f2

class GE35 {
public:
    byte imgBright;
    rgb out[IMG_HEIGHT][IMG_WIDTH];		// output image buffer
    int strandEnabled[STRAND_COUNT];
    int row[STRAND_COUNT];				// index of LED to display for each strand

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

private:
    void makeFrame(byte index, byte r, byte g, byte b, byte i, byte *buffer);
    void displayTimeSince(unsigned long then, char * desc);
    void setGlobalIntensity(byte val);
	// Low Level I/O
    void setPin(byte pin);
    void clrPin(byte pin);
    byte * getBufferAndMask(byte pin, byte &pinmask);
    void composeAndSendFrame();
    void deferredSendFrame(byte pin, byte *bitbuffer);
    void sendFrame();	// sends 'deffered' serial comm buffer across all 'ports'
    void dumpFrame(byte *buffer);

public:
	// Deferred I/O storage
    // !! Warning this is uController specific !! Not Portable! !!

	// pins 22-29 PORTA
	byte portAframe[FRAMESIZE];	// start and stop frome + 26 bits 
	byte portAmask;			// remember what pins are being set

	// pins 30-37 PORTC
	byte portCframe[FRAMESIZE];	
	byte portCmask;			
};

#endif
