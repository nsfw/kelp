###############################################################################
# kelper -- play back files on the kelp!
###############################################################################

import time
import CCore
kelp = CCore.CCore(pubsub="osc-udp:") # use default bidirectional multicasto
# kelp = CCore.CCore(pubsub="osc-udp://139.104.88.199:9999")
# kelp = CCore.CCore(pubsub="osc-udp://198.178.187.122:9999")

def getPixel(mov,frameOffset,x,y,z,options):
    # index into the source movie (which is a one dimensional array)
    # organized as RED[0...63], GRN[0...63], BLU[0...63]
    # support ROTATIONS which map the given x,y,z -> x',y',z'
    # NOTE: turns r,g,b pixels into r,g,b,a pixels
    nx=x		# fake rotation
    ny=y
    nz=z
    pixOff = (nz*8*8) + (ny*8) + nx
    return color(ord(mov[frameOffset+pixOff]),
                 ord(mov[frameOffset+pixOff+0x200]),
                 ord(mov[frameOffset+pixOff+0x400]),200)

def composeFrame(mov, frameNum, base, options):
    # note: though not gorgeous this takes less than a ms my laptop
    # Source movie is RGB, but output is RGBA
    out = []
    frameOffset = frameNum*(8*8*8*3)+base
    # print "Frame Offset = %04x" % (frameOffset)
    for z in range(0,8):
        for y in range(0,8):
            for x in range(0,8):
                out.append(getPixel(mov,frameOffset,x,y,z,options))
    # flatten the list
    return ''.join([item for sublist in out for item in sublist])

def testPattern(colors):
    # will layer colors in Z
    # input colors must be RGBA
    out = []
    for z in range(0,8):
        for y in range(0,8):
            for x in range(0,8):
                out.append(colors[z%len(colors)])
    # flatten the list
    return ''.join([item for sublist in out for item in sublist])

def testXYZ(frame):
    # make a frame of a 'movie' where R=X, G=Y, B=Z
    out = [chr(0) for x in range(8*8*8*3)]
    for z in range(0,8):
        for y in range(0,8):
            for x in range(0,8):
                r=0
                g=0
                b=0
                if(x==frame):
                    r=255
                if(y==frame):
                    g=255
                if(z==frame):
                    b=255
                offset = x+(y*8)+(z*8*8)
                print "%d,%d,%d %d %d,%d,%d\n" % (x,y,z,offset,r,g,b)
                out[offset+0]  = chr(r)
                out[offset+64] = chr(g)
                out[offset+128]= chr(b)
                # out.append([chr(r),chr(g),chr(b)])
    # flatten the list
    return ''.join([item for sublist in out for item in sublist])

def dumpstr(s,start=0,cnt=0):
    if(cnt==0):
        cnt = len(s)
    i=start
    first=1
    for c in s[start:]:
        if(i%16 == 0 or first):
            print "\n"+hex(i)+": ",
        print "%02x"%(ord(c)),
        first=0
        i=i+1
        cnt=cnt-1
        if(cnt<0):
            break

def testMovie():
    out=""
    for i in range(0,8):
        out = out+testXYZ(i)
    return out

def testXYZ_RGBA(frame):
    # make 'movie' frame where R=X, G=Y, B=Z
    out = [chr(0) for x in range(8*8*8*4)]	# full cube rgba output
    # for each vertical layer
    for z in range(0,8):
        for y in range(0,8):
            for x in range(0,8):
                r=0
                g=0
                b=0
                if(x==frame):
                    r=255
                if(y==frame):
                    g=255
                if(z==frame):
                    b=255
                # r=255*x/8
                offset = (x+(y*8)+(z*8*8))*4
                out[offset+0]  = chr(r)
                out[offset+1] = chr(g)
                out[offset+2]= chr(b)
                out[offset+3] = chr(255)
                # print "(%d,%d,%d)=(%d,%d,%d)\n" % (x,y,z,r,g,b)
    # flatten the list
    return ''.join([item for sublist in out for item in sublist])

def sendTest1():
    i=0
    while(1):
        sendFrame(kelp,testXYZ_RGBA(i%8))
        i=i+1
        time.sleep(1.0/fps)

def color(r,g,b,a=200):
    return [chr(r),chr(g),chr(b),chr(a)]

layerCake = testPattern([color(200,0,0),color(0,200,0),color(0,0,200),color(0,200,200)])

interMsgDelay=0.001

def sendFrame(kelp, frame):
    # this is an awful hack to send a blob w/o doing CCoreOSC surgery
    # Further, max Ethernet size is ~1500 bytes, so we need send the frame
    # as two packets...
    m = CCore.OSC.OSCMessage()
    m.setAddress("/screenxy");
    m.append(8)	# image size W, H - 8 by 32
    m.append(8*4)
    m.append(0)	# target X,Y
    m.append(0) # y=top half of image
    m.append(frame[0:(64*4)*4],typehint='b')		# needs blob typehint
    kelp.sender.send(m)
    time.sleep(interMsgDelay)
    # send second half
    m.clear("/screenxy");
    m.append(8)		# image size - 8 by 32
    m.append(8*4)
    m.append(0)		# x=0
    m.append(8*4)	# y=bottom half of image
    m.append(frame[(64*4)*4:],typehint='b')		# needs blob typehint
    kelp.sender.send(m)


# Open file 
# Forever:
#  Compose Frame N (w/ transforms)
#  Send Frame N
#  Delay


movies=[
    "../media/raw888/Waves_8x8x8_color.raw",
    "../media/raw888/TestXYZ_8x8x8_color.raw",
    "../media/raw888/TwoBalls_8x8x8_color.raw",
    "../media/raw888/PlaqueRainbowRotation_8x8x8_color.raw",
    "../media/cs/drape.eca"]

import select
import sys
# Still requires that you 
def getKye():
    if select.select([sys.stdin], [], [], 0) == ([sys.stdin], [], []):
        return sys.stdin.read(1)
    else:
    	return 0

fps = 30
quitFlag = False

def playMovie(fn,fps,options):
    print "Playing "+fn
    f = open(fn, "r")
    movie = f.read()
    csflag = movie[0:2] == "Ls"
    base = 0x100 if csflag else 0
    frames = (len(movie)-base)/(3*8*8*8)
    if int(frames) != frames:
        print "Warning file does not end on an even frame boundry!\n"
    frameNum=0
    quitFlag=False
    # loop
    while not quitFlag:
        frame = composeFrame(movie, frameNum, base, options)
        sendFrame(kelp, frame)
        time.sleep(max((1.0/fps)-interMsgDelay, 0))
        frameNum = (frameNum+1)%frames
        quitFlag = nonBlockingReadChar()
        if(quitFlag):
            break
    return quitFlag

def playN(n,nfps=fps):
    options = {}
    playMovie(movies[n],nfps,options)
#sendFrame(kelp, testPattern([chr(100),chr(0),chr(0),chr(255)]))

def playAll():
    n = 0
    while 1:
        playN(n)
        n=n+1
        n=n%len(movies)


