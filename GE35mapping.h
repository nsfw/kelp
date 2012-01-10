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
///////////////////////////////////////////////////////////////////////////////
#ifndef GE35mapping_h
#define GE35mapping_h

// CONFIGURATION INFORMATION 
#define MAX_STRAND_LEN 35	// Should be the ACTUAL LENGTH OF LONGEST STRAND - electrical max is 62
#define STRAND_COUNT 2		// Needs to be ACTUAL NUMBER OF DATA LINES IN USE
#define IMG_WIDTH 4			
#define IMG_HEIGHT 8		// 

typedef struct a_strand {
    byte len;		// length of this strand
    byte pin;		// digital out pin associated w/ this strand
    byte x[MAX_STRAND_LEN];		// source X and Y from Image
    byte y[MAX_STRAND_LEN];
} strand;

#define PIN_PANEL_0_STRAND_A 22
#define PIN_PANEL_0_STRAND_B 23

#ifndef GE35_NO_DATA

strand strands[]={
// len, pin, {x-coords}{y-coords}, initial color
    /* panel 0 strand A */
    { /*len*/ 35,
      /*pin*/ PIN_PANEL_0_STRAND_A,
      {
          0,0,0,0,0,0,0,0,
                        0,	/* down */
          0,0,0,0,0,0,0,0,
          1,	/* spare */
          1,1,1,1,1,1,1,1,
                        1,	/* down */
          1,1,1,1,1,1,1,1,
      },
      {
          0,1,2,3,4,5,6,7,
                        7,	/* down */
          7,6,5,4,3,2,1,0,
          0,    /* spare */
          0,1,2,3,4,5,6,7,
                        7,	/* down */
          7,6,5,4,3,2,1,0
      }},
    /* panel 0 strand B */
    { /*len*/ 35,
      /*pin*/ PIN_PANEL_0_STRAND_B,
      {
          2,2,2,2,2,2,2,2,
                        2,	/* down */
          2,2,2,2,2,2,2,2,
          3,	/* spare */
          3,3,3,3,3,3,3,3,
                        3,	/* down */
          3,3,3,3,3,3,3,3,
      },
      {
          0,1,2,3,4,5,6,7,
                        7,	/* down */
          7,6,5,4,3,2,1,0,
          0,    /* spare */
          0,1,2,3,4,5,6,7,
                        7,	/* down */
          7,6,5,4,3,2,1,0
      }},
};

#endif	// GE35_NO_DATA
#endif
