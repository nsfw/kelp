///////////////////////////////////////////////////////////////////////////////
// BIGGIE Configuration
///////////////////////////////////////////////////////////////////////////////
//
// BIGGIE is 4 rows, a gap, and 4 rows.
// Each row conists of a 9 + 2*15 + 14 on the slide out
//
// Image size = 53 x 8 (39 on side + 14 on slide out, 8 rows)
//
///////////////////////////////////////////////////////////////////////////////
#ifndef GE35mapping_h
#define GE35mapping_h

// MONOLITHIC - CONFIGURATION INFORMATION 
// full row = 9 + 16 + 16 + 16 = 57
#define MAX_STRAND_LEN 56   // ACTUAL LENGTH OF LONGEST STRAND - max is 62
#define STRAND_COUNT (8)    // ACTUAL NUMBER OF DATA LINES IN USE

// Mapped as a rectangle 53x8 (make this 64 for easier math?)
#define IMG_WIDTH (53)
#define IMG_HEIGHT (8)

typedef struct a_strand {
    byte len;       // length of this strand
    byte pin;       // digital out pin associated w/ this strand
    byte x[MAX_STRAND_LEN];     // source X and Y from Image
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

#define PIN_PANEL_5_STRAND_A 31 // was 14-17, but moved to free up serial
#define PIN_PANEL_5_STRAND_B 33
#define PIN_PANEL_5_STRAND_C 35
#define PIN_PANEL_5_STRAND_D 37

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

// Large Rows in front of vehicle 
// 9 + 15 + 15 = 39 
#define ROW_39_X() \
    { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9,\
     10, 11, 12, 13, 14, 15, 16, 17, 18, 19,\
     20, 21, 22, 23, 24, 25, 26, 27, 28, 29,\
     30, 31, 32, 33, 34, 35, 36, 37, 38     }

#define ROW_39_Y(y) \
    { y, y, y, y, y, y, y, y, y, y,\
      y, y, y, y, y, y, y, y, y, y,\
      y, y, y, y, y, y, y, y, y, y,\
      y, y, y, y, y, y, y, y, y    }

#define ROW_39(PIN, Y)\
    { /*len*/ 39,\
      /*pin*/ PIN,\
      ROW_39_X(),\
      ROW_39_Y(Y),\
    }

// Short Zig Zag Rows on Slideout
// Four shorter (14) sticks per strand

#define ROWS_4_of_14_X() \
    { (39+0),(39+1),(39+2), (39+3), (39+4), (39+5), (39+6), \
      (39+7),(39+8),(39+9),(39+10),(39+11),(39+12),(39+13), \
      (39+0),(39+1),(39+2), (39+3), (39+4), (39+5), (39+6), \
      (39+7),(39+8),(39+9),(39+10),(39+11),(39+12),(39+13), \
      (39+0),(39+1),(39+2), (39+3), (39+4), (39+5), (39+6), \
      (39+7),(39+8),(39+9),(39+10),(39+11),(39+12),(39+13), \
      (39+0),(39+1),(39+2), (39+3), (39+4), (39+5), (39+6), \
      (39+7),(39+8),(39+9),(39+10),(39+11),(39+12),(39+13) }

#define ROWS_4_of_14_Y(y,i) \
    { y, y, y, y, y, y, y, \
      y, y, y, y, y, y, y, \
      y+(1*i),y+(1*i),y+(1*i),y+(1*i),y+(1*i),y+(1*i),y+(1*i),\
      y+(1*i),y+(1*i),y+(1*i),y+(1*i),y+(1*i),y+(1*i),y+(1*i),\
      y+(2*i),y+(2*i),y+(2*i),y+(2*i),y+(2*i),y+(2*i),y+(2*i),\
      y+(2*i),y+(2*i),y+(2*i),y+(2*i),y+(2*i),y+(2*i),y+(2*i),\
      y+(3*i),y+(3*i),y+(3*i),y+(3*i),y+(3*i),y+(3*i),y+(3*i),\
      y+(3*i),y+(3*i),y+(3*i),y+(3*i),y+(3*i),y+(3*i),y+(3*i) }

#define ROWS_4_of_14(PIN, Y, INC)                   \
    { /*len*/ 56,\
      /*pin*/ PIN,\
      ROWS_4_of_14_X(),\
      ROWS_4_of_14_Y(Y,INC)                  \
    }

strand strands[]={ // top down
    ROW_39(PIN_PANEL_0_STRAND_A, 0),
    ROW_39(PIN_PANEL_1_STRAND_A, 1),
    ROW_39(PIN_PANEL_2_STRAND_A, 2),
    ROW_39(PIN_PANEL_3_STRAND_A, 3),
    ROW_39(PIN_PANEL_4_STRAND_A, 4),
    ROW_39(PIN_PANEL_5_STRAND_A, 5),
    ROW_39(PIN_PANEL_6_STRAND_A, 6),
    ROW_39(PIN_PANEL_7_STRAND_A, 7),
    // rows on slideout 
    // ROWS_4_of_14(PIN_PANEL_0_STRAND_B,7,-1),// start at row 7 and go up
    // ROWS_4_of_14(PIN_PANEL_1_STRAND_B,3,-1) // start at row 3 and go up
};

#endif  // GE35_NO_DATA
#endif
