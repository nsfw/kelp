///////////////////////////////////////////////////////////////////////////////
// Configuration Information
///////////////////////////////////////////////////////////////////////////////
//
// TEXT READABLE / SIDES MAPPED TOGETHER
//
// NOTE: This file maps the right and left sides together, with the
// backmost strand of PASSENGER SIDE mapped to the frontmost strand of
// the DRIVER SIDE. This will retain readability for text.
//
///////////////////////////////////////////////////////////////////////////////


// Three strands on the back half of RV - upper back corner is 0,0 on PASSENGER SIDE
#define confRV 1
#define F(ox) ox

strand strands[]={
// len, pin, {x-coords}{y-coords}, initial color
// R1
    { 36, 22, 			// 45 leds as 5 columns of 9 LEDs
      {
          3,3,3,3,3,3,3,3,3,	// strand starts to the FRONT and goes BACKWARDS
          2,2,2,2,2,2,2,2,2,
          1,1,1,1,1,1,1,1,1,
          0,0,0,0,0,0,0,0,0
      },
      {
          0,1,2,3,4,5,6,7,8,
          8,7,6,5,4,3,2,1,0,
          0,1,2,3,4,5,6,7,8,
          8,7,6,5,4,3,2,1,0
      }},
// R2
    { 36, 23, 
      {
          7,7,7,7,7,7,7,7,7,
          6,6,6,6,6,6,6,6,6,
          5,5,5,5,5,5,5,5,5,
          4,4,4,4,4,4,4,4,4
      },
      {
          0,1,2,3,4,5,6,7,8,
          8,7,6,5,4,3,2,1,0,
          0,1,2,3,4,5,6,7,8,
          8,7,6,5,4,3,2,1,0
      }},
// R3
    { 35, 24, 
      {
          12,							// lost duplicate
          11,11,11,11,11,11,11,11,		// 8 high
          10,10,10,10,10,10,10,10,		// "" 
          9, 9, 9, 9, 9, 9, 9, 9, 9,	// 9 high
          8, 8, 8, 8, 8, 8, 8, 8, 8
      },
      {
          0,
          0,1,2,3,4,5,6,7,				// 8 high
          7,6,5,4,3,2,1,0,
          0,1,2,3,4,5,6,7,8,
          8,7,6,5,4,3,2,1,0
      }},

// R4
    { 34, 25, 		// above door 
      {
          13,14,15,16,17,18,19,20,21,22,23,24,25,
          25,24,23,22,21,20,19,18,17,16,15,14,13,
          12,	// gympy double LED
          12,12,12,12,12,12,12
      },
      {
          0,0,0,0,0,0,0,0,0,0,0,0,0,
          1,1,1,1,1,1,1,1,1,1,1,1,1,
          1,	// gympy?
          2,3,4,5,6,7,8
      }},
// R5 
    { 36, 26, 		
      {
          16,17,18,19,20,21,22,
          22,21,20,19,18,17,16,
          16,17,18,19,20,21,22,
          22,21,20,19,18,17,16,
          16,17,18,19,20,21,22,
          22
      },
      {
          4,4,4,4,4,4,4,
          5,5,5,5,5,5,5,
          6,6,6,6,6,6,6,
          7,7,7,7,7,7,7,
          8,8,8,8,8,8,8,
          8
      }},
    
// R6 - right side only 
    { 17, 31, 			// front run that goes in front and over to left side!
      {
          16,16,
          17,17,
          17,			// duplicate
          18,19,20,21,
          21,22,
          22,23,24,25,26,27
      },
      {
          2,3,
          3,2,
          2,		// duplicate
          2,2,2,2,
          3,3,
          2,2,2,2,2,2
      }},

// L1 -- mapped to the same pixels as passenger side
    { 36, 32,
      {
          F(3),F(3),F(3),F(3),F(3),F(3),F(3),F(3),F(3),
          F(2),F(1),
          F(0),F(0),F(0),F(0),F(0),F(0),F(0),F(0),F(0),
          F(1),F(1),F(1),F(1),F(1),F(1),F(1),
          F(2),F(2),F(2),F(2),F(2),F(2),F(2),
      },
      {
          0,1,2,3,4,5,6,7,8,
          8,8,
          8,7,6,5,4,3,2,1,0,
          0,1,2,3,4,5,6,
          6,5,4,3,2,1,0
      }},
// L2
    { 36, 33,
      {
          F(7),F(7),F(7),F(7),F(7),F(7),F(7),F(7),F(7),
          F(6),F(6),F(6),F(6),F(6),F(6),F(6),F(6),F(6),
          F(5),F(5),F(5),F(5),F(5),F(5),F(5),F(5),F(5),
          F(4),F(4),F(4),F(4),F(4),F(4),F(4),F(4),F(4)
      },
      {
          0,1,2,3,4,5,6,7,8,
          8,7,6,5,4,3,2,1,0,
          0,1,2,3,4,5,6,7,8,
          8,7,6,5,4,3,2,1,0
      }},
// L3
    { 32, 34,
      {
          F(11),F(11),F(11),F(11),F(11),F(11),F(11),F(11),		// 8 high
          F(10),F(10),F(10),F(10),F(10),F(10),F(10),F(10),	
          F(9), F(9), F(9), F(9), F(9), F(9), F(9), F(9),
          F(8), F(8), F(8), F(8), F(8), F(8), F(8), F(8)
      },
      {
          0,1,2,3,4,5,6,7,
          7,6,5,4,3,2,1,0,
          0,1,2,3,4,5,6,7,
          7,6,5,4,3,2,1,0
      }},
// L4
    { 34, 27,
      {
          F(15),F(15),F(15),F(15),F(15),F(15),F(15),F(15),F(15),	// 9 high
          F(14),F(14),F(14),
          F(13),F(13),F(13),
          F(12),F(12),F(12),F(12),F(12),F(12),F(12),F(12),F(12),
          F(13),F(13),F(13),F(13),F(13),
          F(14),F(14),F(14),F(14),F(14)
      },
      {
          0,1,2,3,4,5,6,7,8,
          8,7,6,
          6,7,8,
          8,7,6,5,4,3,2,1,0,
          0,1,2,3,4,
          4,3,2,1,0
      }},
// L5 -- funky!
    { 34, 35,
      {
          F(16),F(16),F(16),F(16),F(16),F(16),F(16),
          F(17),F(17),F(17),F(17),F(17),F(17),F(17),
          F(18),F(18),F(18),F(18),F(18),F(18),F(18),F(18),
          F(17),
          F(17),F(18),
          F(19),F(19),F(19),F(19),F(19),F(19),F(19),F(19),F(19)
      },
      {
          0,1,2,3,4,5,6,
          6,5,4,3,2,1,0,
          0,1,2,3,4,5,6,7,
          7,
          8,8,
          8,7,6,5,4,3,2,1,0
      }},
// L6 
    { 32, 28,
      {
          F(20),F(20),F(20),F(20),F(20),F(20),F(20),F(20),F(20),
          F(21),F(21),F(21),F(21),F(21),F(21),F(21),F(21),F(21),
          F(22),F(23),F(24),F(25),
          F(25),F(24),F(23),F(22),
          F(22),F(23),F(24),F(25),F(26),F(27)
      },
      {
          0,1,2,3,4,5,6,7,8,
          8,7,6,5,4,3,2,1,0,
          0,0,0,0,
          1,1,1,1,
          2,2,2,2,2,2
      }}
};

