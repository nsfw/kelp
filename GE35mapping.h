///////////////////////////////////////////////////////////////////////////////
// KELP - Configuration Information - TEST MAP!!!
///////////////////////////////////////////////////////////////////////////////
//
// KELP is 8 panels displays, each panel is made up of 4 strands, where each
// strand is setup like:
//
// |
// + a0  a1  a2  a3  a4  a5  a6  a7  -+
//                                    a7.5 (down/float)
// + a15 a14 a13 a12 a11 a10 a9  a8  -+
// |
// a15.5 a 'spare' led that makes wiring easier
// |
// + a16 a17 a18 a19 a20 a21 a22 a23 -+
//                                    a23.5 (down/float)
//  -a31 a30 a29 a28 a27 a26 a25 a24 -+
//
// They are configured with strand "a" led "0" mapped to [x,y] of [0,0]
// strand "b" led "0" mapped to [2,0], "c0" = [4,0], "d0" = [7,0]
//
// The entire cube is mapped to an 8 x 64
// Each PANEL is mapped to a different base X coordinate:
// 
///////////////////////////////////////////////////////////////////////////////
#ifndef GE35mapping_h
#define GE35mapping_h

// CONFIGURATION INFORMATION 
#define MAX_STRAND_LEN 35	// Should be the ACTUAL LENGTH OF LONGEST STRAND - electrical max is 62
#define STRAND_COUNT (8*4)		// Needs to be ACTUAL NUMBER OF DATA LINES IN USE
// #define STRAND_COUNT (4)		// Needs to be ACTUAL NUMBER OF DATA LINES IN USE

// storage for 8x8x8 - mapped as an 8 x 64 tall image
#define IMG_WIDTH (8)			
#define IMG_HEIGHT (8*8)		// 

typedef struct a_strand {
    byte len;		// length of this strand
    byte pin;		// digital out pin associated w/ this strand
    byte x[MAX_STRAND_LEN];		// source X and Y from Image
    byte y[MAX_STRAND_LEN];
} strand;

// !!NOTE!! PANEL MACRO assumes that PINS are CONTIGUOUS and INCREASEd

// Need 32 pins 
// J3 (chipKIT) 78-85 -- upper left dual HDI
#define PIN_PANEL_0_STRAND_A 78
#define PIN_PANEL_0_STRAND_B 79
#define PIN_PANEL_0_STRAND_C 80
#define PIN_PANEL_0_STRAND_D 81

#define PIN_PANEL_1_STRAND_A 82
#define PIN_PANEL_1_STRAND_B 83
#define PIN_PANEL_1_STRAND_C 84
#define PIN_PANEL_1_STRAND_D 85

#define PIN_PANEL_2_STRAND_A 8
#define PIN_PANEL_2_STRAND_B 9
#define PIN_PANEL_2_STRAND_C 10
#define PIN_PANEL_2_STRAND_D 11

// J14 upper middle dual HDI

#define PIN_PANEL_3_STRAND_A 70
#define PIN_PANEL_3_STRAND_B 71
#define PIN_PANEL_3_STRAND_C 72
#define PIN_PANEL_3_STRAND_D 73

#define PIN_PANEL_4_STRAND_A 2
#define PIN_PANEL_4_STRAND_B 3
#define PIN_PANEL_4_STRAND_C 4
#define PIN_PANEL_4_STRAND_D 5

// J4 single row HDI

#define PIN_PANEL_5_STRAND_A 14
#define PIN_PANEL_5_STRAND_B 15
#define PIN_PANEL_5_STRAND_C 16
#define PIN_PANEL_5_STRAND_D 17

#define PIN_PANEL_6_STRAND_A 18	
#define PIN_PANEL_6_STRAND_B 19
#define PIN_PANEL_6_STRAND_C 20
#define PIN_PANEL_6_STRAND_D 21

// J9 big dual HDI on right 

#define PIN_PANEL_7_STRAND_A 30
#define PIN_PANEL_7_STRAND_B 32
#define PIN_PANEL_7_STRAND_C 34
#define PIN_PANEL_7_STRAND_D 36

#ifndef GE35_NO_DATA

// MACROS to condense mapping 

#define UP_DOWN_X(a,b) \
    {\
          a,a,a,a,a,a,a,a,\
                        a,	/* down */\
          a,a,a,a,a,a,a,a,\
          b,	/* spare */\
          b,b,b,b,b,b,b,b,\
                        b,	/* down */\
          b,b,b,b,b,b,b,b,\
     }


#define UP_DOWN_Y(y) \
    {\
          0+y,1+y,2+y,3+y,4+y,5+y,6+y,7+y,                                \
			                          7+y,	/* down */ \
          7+y,6+y,5+y,4+y,3+y,2+y,1+y,0+y, \
          0+y,    /* spare */ \
          0+y,1+y,2+y,3+y,4+y,5+y,6+y,7+y, \
			                          7+y,	/* down */\
          7+y,6+y,5+y,4+y,3+y,2+y,1+y,0+y \
    }

#define STRANDS(PIN,X,BASE_Y) \
    { /*len*/ 35,	\
      /*pin*/ PIN,  \
      UP_DOWN_X(X,X+1), \
      UP_DOWN_Y(BASE_Y) \
    }

// !!NOTE!! Macro assumes that PINS are CONTIGUOUS and INCREASE
#define PANEL(PIN,Z) \
    STRANDS(PIN+0,0,Z*8),	\
    STRANDS(PIN+1,2,Z*8),	\
    STRANDS(PIN+2,4,Z*8),	\
    STRANDS(PIN+3,6,Z*8)

// !!NOTE!! Macro assumes that PINS are CONTIGUOUS and INCREASE
// This one uses every other pin, which is useful on J9
#define PANEL_ON2S(PIN,Z) \
    STRANDS(PIN+0,0,Z*8),	\
    STRANDS(PIN+2,2,Z*8),	\
    STRANDS(PIN+4,4,Z*8),	\
    STRANDS(PIN+6,6,Z*8)

// Each panel layer is mapped in the Y direction in the image buffer
strand strands[]={
// len, pin, {x-coords}{y-coords}, initial color
    PANEL(PIN_PANEL_0_STRAND_A, 0),	// z=0 0,0 - 7,7
    PANEL(PIN_PANEL_1_STRAND_A, 1),	// z=1 0,8 - 7,15
    PANEL(PIN_PANEL_2_STRAND_A, 2), // 
    PANEL(PIN_PANEL_3_STRAND_A, 3),
    PANEL(PIN_PANEL_4_STRAND_A, 4),
    PANEL(PIN_PANEL_5_STRAND_A, 5),
    PANEL(PIN_PANEL_6_STRAND_A, 6),
    PANEL_ON2S(PIN_PANEL_7_STRAND_A, 7)
};

#endif	// GE35_NO_DATA
#endif
