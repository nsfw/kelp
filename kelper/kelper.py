###############################################################################
# kelper -- play back files on the kelp!
###############################################################################

import time
import CCore
# kelp = CCore.CCore(pubsub="osc-udp:") # use default bidirectional multicasto
kelp = CCore.CCore(pubsub="osc-udp://139.104.88.199:9999")

def getPixel(mov,frameOffset,x,y,z,options):
    # index into the source movie (which is a one dimensional array)
    # organized as RED[0...63], GRN[0...63], BLU[0...63]
    # support ROTATIONS which map the given x,y,z -> x',y',z'
    # NOTE: turns r,g,b pixels into r,g,b,a pixels
    nx=x
    ny=y
    nz=z
    pixOff = (z*8*8) + (y*8) + x
    return [mov[frameOffset+pixOff],
            mov[frameOffset+pixOff+64],
            mov[frameOffset+pixOff+128],
            255]		# alpha or something else?


def composeFrame(mov, frameNum, base, options):
    # note: though not gorgeous this takes less than a ms my laptop
    out = []
    frameOffset = frameNum*(8*8*8*3)+base
    for z in range(0,8):
        for y in range(0,8):
            for x in range(0,8):
                out.append(getPixel(mov,frameOffset,x,y,z,options))
    # flatten the list
    return ''.join([item for sublist in out for item in sublist])

def sendFrame(kelp, frame):
    # this is an awful hack to send a blob w/o doing CCoreOSC surgery
    # Further(!), max Ethernet size is ~1500 bytes, so we need send the frame
    # as two packets...
    m = CCore.OSC.OSCMessage()
    m.setAddress("/screenxy");
    m.append(8)	# image size
    m.append(8)
    m.append(0)	# x=0
    m.append(0) # y=top half of image
    m.append(frame[0:64*3*4],typehint='b')		# needs blob typehint
    kelp.sender.send(m)
    time.sleep(0.001)
    # send second half
    m.clear("/screenxy");
    m.append(8)		# image size
    m.append(8)
    m.append(0)		# x=0
    m.append(8*4)	# y=bottom half of image
    m.append(frame[64*3*4:],typehint='b')		# needs blob typehint
    kelp.sender.send(m)


# Open file 
# Forever:
#  Compose Frame N (w/ transforms)
#  Send Frame N
#  Delay


filenm = "../media/raw888/TestXYZ_8x8x8_color.raw"
f = open(filenm, "r")
movie = f.read()
csflag = movie[0:2] == "Ls"
base = 0x100 if csflag else 0
frames = (len(movie)-base)/(3*8*8*8)
fps = 1

if int(frames) != frames:
    print "Warning file does not end on an even frame boundry!\n"

quitFlag = False
frameNum = 0;
options = {}

print "Playing %s -- %d frames @%dfps" % (filenm, frames, fps)
print ""

while not quitFlag:
    frame = composeFrame(movie, frameNum, base, options)
    # kelp.send("/888RGB",[frame]) -- sending a blob doesn't work w/ current CCore
    sendFrame(kelp, frame)
    time.sleep(1.0/fps)
    frameNum = (frameNum+1)%frames
    print "."
