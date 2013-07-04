#!/usr/local/bin/python
###############################################################################
# kelper -- play back files on the kelp!
###############################################################################

import numpy as np
import time
import colorsys
import CCore

kelp = CCore.CCore(pubsub="osc-udp://192.168.1.69:9999")
side = CCore.CCore(pubsub="osc-udp://192.168.1.99:9999")
emulator = CCore.CCore(pubsub="osc-udp:") # use default bidirectional multicasto
oocp = emulator	# OOCP talks on default multicast too

#sendto = [emulator]
#sendto = [kelp, emulator]
sendto = [kelp,side]

def getPixel(mov,frameOffset,x,y,z):
    # index into the source movie (which is a one dimensional array)
    # organized as RED[0...63], GRN[0...63], BLU[0...63]
    # support ROTATIONS which map the given x,y,z -> x',y',z'
    # NOTE: turns r,g,b pixels into r,g,b,a pixels
    pixOff = (z*8*8) + (y*8) + x
    return color(ord(mov[frameOffset+pixOff]),
                 ord(mov[frameOffset+pixOff+0x200]),
                 ord(mov[frameOffset+pixOff+0x400]),200)

def composeFrame(mov, frameNum, base, xfmList, options):
    # note: though not gorgeous this takes less than a ms my laptop
    # Source movie is RGB, but output is RGBA
    out = []
    frameOffset = frameNum*(8*8*8*3)+base
    # print "Frame Offset = %04x" % (frameOffset)
    i = 0
    for z in range(0,8):
        for y in range(0,8):
            for x in range(0,8):
                if xfmList != None:
                    pt = xfmList[i]
                    i+=1
                    out.append(getPixel(mov,frameOffset,pt[0],pt[1],pt[2]))
                else:
                    out.append(getPixel(mov,frameOffset,x,y,z))
    # flatten the list
    return ''.join([item for sublist in out for item in sublist])

# create a transformation array that for an inital x,y,z returns a new x,y,z
def transformList(xfm):
    out = []
    for z in range(0,8):
        for y in range(0,8):
            for x in range(0,8):
                out.append(transformPoint([x,y,z],xfm))
    return out

def transformPoint(pt, xfm):
    # e.g. "swap x and z"
    # xfm = array([[0, 0, 1],
    #              [0, 1, 0],
    #              [1, 0, 0]])
    flip = xfm.dot([1,1,1])
    tmp = xfm.dot(pt)
    # handles Flipping around an axis [0->7, 7->0]
    for i in range(3):
        if(flip[i]<0):
            tmp[i] += 7
    return tmp.tolist()

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
    out = [chr(0) for x in range(8*8*8*4)] # full cube rgba output
    # for each vertical layer
    for z in range(0,8):
        for y in range(0,8):
            for x in range(0,8):
                r=0
                g=0
                b=0
                # if(x==frame):
                #     r=255
                if(y==frame):
                    g=240
                # if(y==(frame+1)):
                #     g=10
                # if(y==frame-1):
                #     g=20
                # if(z==frame):
                #     b=255
                # r=255*x/8
                offset = (x+(y*8)+(z*8*8))*4
                out[offset+0]  = chr(r)
                out[offset+1] = chr(g)
                out[offset+2]= chr(b)
                out[offset+3] = chr(255)
                # print "(%d,%d,%d)=(%d,%d,%d)\n" % (x,y,z,r,g,b)
    # flatten the list
    return ''.join([item for sublist in out for item in sublist])

def testXYZ_INDEX(frame):
    # light each pixel up once - 512 frames long
    out = [chr(0) for x in range(8*8*8*4)]  # full cube rgba output
    # for each panel
    for z in range(0,8):
        for y in range(0,8):
            for x in range(0,8):
                offset = (x+(y*8)+(z*8*8))*4
                if(offset == frame%512):
                    out[offset+0] = chr(240)
                    out[offset+1] = chr(240)
                    out[offset+2] = chr(240)
                    out[offset+3] = chr(255)
                    print "(%d,%d,%d)\n" % (x,y,z)
    # flatten the list
    return ''.join([item for sublist in out for item in sublist])

def sendTest1():
    i=0
    while(1):
        sendFrame(testXYZ_RGBA(i%8))
        print i
        i=i+1
        getKeyBlock()
        # time.sleep(1.0/fps)

def sendTest2():
    i=0
    while(1):
        sendFrame(testPattern([color(i%240,0,0)]))
        i=i+1
        time.sleep(1.0/fps)

def sendTest3(r,g,b):
    i=0
    sendFrame(testPattern([color(r,g,b)]))
    while(1):
        bright(1.0-(i%255)/255.0)
        i=i+4
        time.sleep(1.0/fps)

def sendTestIndex():
    i=0
    while(1):
        sendFrame(testXYZ_INDEX(i))
        i=i+1
        getKeyBlock()
        # time.sleep(1.0/fps)

def bright(b):
    # brightness from 0.0 -> 1.0
    send("/bright",[float(b)])

def color(r,g,b,a=200):
    return [chr(r),chr(g),chr(b),chr(a)]

layerCake = testPattern([color(200,0,0),color(0,200,0),color(0,0,200),color(0,200,200)])

interMsgDelay=0.001

def rawSendFrame(target, frame):
    # this is an awful hack to send a blob w/o doing CCoreOSC surgery
    # Further, max Ethernet size is ~1500 bytes, so we need send the frame
    # as two packets...
    m = CCore.OSC.OSCMessage()
    m.setAddress("/screenxy");
    m.append(8) # image size W, H - 8 by 32
    m.append(8*4)
    m.append(0) # target X,Y
    m.append(0) # y=top half of image
    m.append(frame[0:(64*4)*4],typehint='b')        # needs blob typehint
    target.sender.send(m)
    time.sleep(interMsgDelay)
    # send second half
    m.clear("/screenxy");
    m.append(8)     # image size - 8 by 32
    m.append(8*4)
    m.append(0)     # x=0
    m.append(8*4)   # y=bottom half of image
    m.append(frame[(64*4)*4:],typehint='b')     # needs blob typehint
    target.sender.send(m)

def sendFrame(frame):
    for i in sendto:
        if i : rawSendFrame(i,frame)
             
def send(path,msg):
    for i in sendto:
        if i : i.send(path,msg)

def bright(bf):
    send("/bright",[bf])

def fill(rf,gf,bf):
    send("/fill",[float(rf),float(gf),float(bf)]);

# apply a function to all pixels
# def makeFrame(picFx, src):
#   out = []
#     i = 0
#     for z in range(0,8):
#         for y in range(0,8):
#             for x in range(0,8):
#               out.append(picFx,x,y,z,src)

#   return ''.join([item for sublist in out for item in sublist])

###############################################################################
# Effects fx
###############################################################################

t0 = 0.0
t = 0.0

# effect functions initialize themselves and then return a function
# to call each frame.

def const(k):
    return lambda(t): k
    
def linfx(k):
    return lambda(t): k*t

def hue(t):
    return np.array(colorsys.hsv_to_rgb(t%1.0,1.0,1.0))

def modfx(v):
    return lambda(t): t%v

def scalergb(rgb,bright):
    hsv = np.array(colorsys.rgb_to_hsv(rgb[0], rgb[1], rgb[2]))
    return np.array(colorsys.hsv_to_rgb(hsv[0], hsv[1], bright))

def huePulse(colorfx, scalefx, ratefx, dur):
    def huePulse_update(t):
        t = ratefx(t % dur)
        # f = abs(scalefx(t))
        f = t
        color = colorfx(t)
        scalergb(color,f );
        nc = np.array(color) * f;
        # bright(f)
        fill(nc[0],nc[1],nc[2])
        time.sleep(0.0001)
    return huePulse_update

def testHuePulse(colorfx,scalefx,ratefx, dur):
    tt = pulse(colorfx,scalefx,ratefx,dur)
    while 1:
        tt(time.time())
        time.sleep(1.0/fps)


def pulse(r,g,b, hold, decay, period):
    def pulseFx(t):
        t = t % period
        # start bright
        if(t<hold):
            send("/fill", [r, g, b])
        else:
            # fraction of decay time
            left = 1 - max((t-hold)/decay,0)
            send("/fill",[r*left, g*left*left, b*left])
    return pulseFx

def testPulse(r,g,b, hold, decay, period):
    tt = pulse(r,g,b, hold,decay, period)
    start = time.time()
    while 1:
        tt((time.time()-start))
        time.sleep(1.0/fps)

# testPulse(1.0, 0.0, 0.0, 0.2, 0.8, 1.5)

def colorPulse(r,g,b):
    return pulse(r,g,b, 0.2, 0.8, 1.5)

# Best Buy Function

# Color until Interrupted


###############################################################################
# Main
###############################################################################

buttonPressed = 0
buttonDownEvent = 0

def buttonHandler(msg):
    global buttonPressed, buttonDownEvent
    val = msg.data[0]
    buttonDownEvent = val and (buttonPressed != val)
    buttonPressed = val
    print msg.data 

def buttonDown():
    # current value
    global buttonPressed
    return buttonPressed

def buttonDownEvent():
    global buttonDownEvent
    # just return once
    tmp = buttonDownEvent
    buttonDownEvent = False
    return tmp

if(kelp):
    kelp.subscribe("/button",buttonHandler)

# Support the OOCP
def oocpHandler(msg):
    pass

movies=[
    "../media/cs/CUBES.eca",
    huePulse(hue,lambda x:x,lambda x:0.3*x,8),
    colorPulse(1.0, 0.0, 0.0),
#    "../media/cs/TED ACTIVE RAINBOW TRAIN.eca",
#    "../media/cs/RubiCube - PlummersCross 8cube.eca",
#    "../media/cs/TED RAINBOW ROTO.eca",
    "../media/raw888/Waves_8x8x8_color.raw",
    colorPulse(0.0, 0.0, 1.0),
    "../media/raw888/TestXYZ_8x8x8_color.raw",
#    "../media/cs/TED ACTIVE MARQUEE.eca",
    "../media/raw888/TwoBalls_8x8x8_color.raw",
    colorPulse(0.0, 1.0, 0.0),
    "../media/raw888/PlaqueRainbowRotation_8x8x8_color.raw",
    "../media/cs/explode.eca",
    "../media/cs/drape.eca"]

import select
import sys

def getKeyBlock():
    return sys.stdin.read(1)

# requires use to hit return!
def getKey():
    if select.select([sys.stdin], [], [], 0) == ([sys.stdin], [], []):
        return sys.stdin.read(1)
    else:
        return 0

fps = 40
quitFlag = False

defaultXfm = np.array([[-1,0,0],
                       [0,0,1],
                       [0,-1,0]])

def playFx(fx,fps,xfm=defaultXfm,options={},dur=0):
    print "Playing "+fx.__name__
    start = time.time()
    playtil = start + dur
    xfmList = None
    if xfm != None:
        xfmList = transformList(xfm)
    quitFlag=False
    while not (quitFlag or (dur and time.time()>playtil)):
        fx(time.time()-start)   # start at t=0
        time.sleep(max((1.0/fps)-interMsgDelay, 0))
        quitFlag = getKey() or buttonDown()
        if(quitFlag):
            break
    return quitFlag


def playMovie(fn,fps,xfm=defaultXfm,options={},dur=0):
    print "Playing "+fn
    if dur:
        print "For",dur,"seconds "
    playtil = time.time() + dur
    f = open(fn, "r")
    movie = f.read()
    csflag = movie[0:2] == "Ls"
    base = 0x100 if csflag else 0
    frames = (len(movie)-base)/(3*8*8*8)
    if int(frames) != frames:
        print "Warning file does not end on an even frame boundry!\n"
    frameNum=0
    quitFlag=False
    # is there a transformation List?
    xfmList = None
    if xfm != None:
        print "XFM = "
        print xfm
        xfmList = transformList(xfm)
    # loop
    while not (quitFlag or (dur and time.time()>playtil)):
        frame = composeFrame(movie, frameNum, base, xfmList, options)
        sendFrame(frame)
        time.sleep(max((1.0/fps)-interMsgDelay, 0))
        frameNum = (frameNum+1)%frames
        quitFlag = getKey() or buttonDown()
        if(quitFlag):
            break
    return quitFlag

def playN(n,xfm=defaultXfm,options={},dur=0):
    instance = movies[n]
    # load up any options or xfms associated with clip
    if type(instance)==type(""):
        # handle a 'movie'
        playMovie(instance,fps,xfm,options,dur)
    else:
        # assume it's a function that computes a new frame
        print instance
        playFx(instance, fps, xfm, options, dur)
        

def playAll():
    n = 0
    while 1:
        playN(n,dur=30.0)
        n=n+1
        n=n%len(movies)

playAll()
