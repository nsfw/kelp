Kelp is a volumetric display that may be hung upside down (from it's
"roots") or immersed in water and floated to the top.

We will make an 8 x 8 x 8 RGB cube (512 addressable pixels) on 10"
spacings.

Fabrication:

Double LED per pixel: Each strand is composed of 'folding over a ge35'
line of lights, either side of the line. Or putting LEDs on either
side on 10" spacing. This allows us to minimize the amount of cable
cutting, etc.  Since we can assign pixels on each strand to the same
address, it doesn't impact our animation bandwidth.

Each strand is inserted in to flexible translucent 'hose' to make it
friendly to swim / walk among.

Programming:

The kelp is controlled by sending an entire frame composed of 8x8
panels(X,Y) representing each layer(Z). (8x8x8=512) * 3(rgb) = 1.5KB bytes

frame:
layer0 (y=0)
layer1 (y=8)
....
layer7 (y=56)

For individual control of 'doubled' LEDs, the width of the frame is doubled.


# LedPerStrand 16
# Total Leds 1024 
# Number of 50light boxes 21
# Bandwidth for RGB8 = 72 KB/sec (or 36KB/sec)


def kelp1():
 # inches
 strandSpacing=10
 strandX=8
 strandY=8
 strandLength=8*10
 ledPerStrand = int(strandLength/5) # same as double sided on 10"
 ledTotal = ledPerStrand*strandX*strandY
 print "LedPerStrand %d" % ledPerStrand
 print "Total Leds %d " % ledTotal
 print "Number of 50light boxes %d" % round((ledTotal/50.0)+0.5)
 print "Bandwidth for RGB8 = %d KBps" % (3*ledTotal*24/1024)

kelp1()

An Arduino Mega has 54 digital IO lines, using 48 allows for 6 per 8x8 panel

8 x 8 panel fab:

128 LEDs doubled. Maximum address is 62 per line, so need to assign AT
LEAST two digital control lines per panel.

When we 'fold them' we could arrange strands like (strand0 and strand1):

|
+ a0  a1  a2  a3  a4  a5  a6  a7  -+		X=0
                                   a7.5 (down/float)
+ a15 a14 a13 a12 a11 a10 a9  a8  -+        X=0
|
a15.5 a 'spare' led that makes wiring easier
|
+ a16 a17 a18 a19 a20 a21 a22 a23 -+	    X=1
                                   a23.5 (down/float)
 -a31 a30 a29 a28 a27 a26 a25 a24 -+        X=1

For 35 LEDs per two strands

Which could use 4 digital per 8x8 panel and allow for full addressability.

Testing:
Water immersion started on 12/29/11
Works with all three wires exposed to water. 

Controllers:
Controlled by an Arduino Mega or chipKit http://www.chipkit.org/forum/
For Chipkit get the NetworkShield
http://www.digilentinc.com/

