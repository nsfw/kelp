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

# Auto Detect if running on RVIP computer
from subprocess import *
ifconfig = Popen(['ifconfig'], stdout = PIPE).communicate()[0]
testing = ifconfig.find("192.168.1.11") < 0  # -1 if not found

if not testing:
    normalOperation = [kelp, side]
    insideOnly = [kelp]
    outsideOnly = [side]
else:
    print "Sending to EMULATOR"
    normalOperation = [emulator]
    insideOnly = [emulator]
    outsideOnly = []

sendto = normalOperation

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
    # NOTE: THIS REALLY GLITCHES! YOU NEVER WANT TO USE IT!
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

def sequenceFx(period,colors):
    """ return things (i.e. colors) in sequence over PERIOD seconds """
    def cfx (t):
        t = t % period
        dur = float(period)/len(colors)
        index = int(t/dur)
        return colors[index]
    print "sequence %s - %s" % (period, colors)
    return cfx

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
            left = 1.0 - min((t-hold)/decay,1.0)
            send("/fill",[r*left, g*left*left, b*left])
            # print left
            # bright(left)
    return pulseFx

# use colorSequenceFx for Color
# bigMachines : send("/lights",["sequence",4.0, 27,47,230, 227, 187, 0])
def pulseSequence(hold, decay, period):
    def pulseSeqFx(t):
        # start bright
        color = colorSequenceFx(t)
        t = t % period
        if(t<hold):
            send("/fill", color)
            # bright(1.0)
        else:
            # fraction of decay time
            left = 1.0 - min((t-hold)/decay,1.0)
            send("/fill",[color[0]*left, color[1]*left*left, color[2]*left])
            # print left
            # bright(left)
    return pulseSeqFx

def testPulse(r,g,b, hold, decay, period):
    tt = pulse(r,g,b, hold,decay, period)
    start = time.time()
    while 1:
        tt((time.time()-start))
        time.sleep(1.0/fps)

# testPulse(1.0, 0.0, 0.0, 0.2, 0.8, 1.5)

def colorPulse(r,g,b):
    return pulse(r,g,b, 0.2, 3.0, 3.0)

def solid(r,g,b):
    def solidFx(t):
        send("/fill", [r, g, b])
    return solidFx

def outsideOff ():
    # currently - inside cabin lights are on the same address as outside
    global sendTo
    sendTo = outsideOnly
    solid(0.0, 0.0, 0.0)()	# send black to just the outside
    sendTo = insideOnly     # but behave normally 'inside' (kelp)
    
def outsideOn():
    global sendTo
    sendTo = normalOperation

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

def oscPlayEffect(name,dur=30):
    global quitFlag, jumpTo, jumpToDur
    quitFlag = True
    jumpTo = name
    jumpToDur = dur

colorSequenceFx = False
def playSequenceHandler (msg):
    # setup the color sequence effect
    # Expects payload of: "sequence", "period", r0, g0, b0, r1, g1, b1, etc..
    #
    global colorSequenceFx
    period = msg.data[1]
    num = len(msg.data)-2
    if num%3 != 0:
        print "SEQUENCE expects PERIOD, [R,G,B] 0,255 per componant ..."
        return 
    i = 2
    colors = []
    while num > 0:
        colors.append([msg.data[i+0]/255.0, msg.data[i+1]/255.0, msg.data[i+2]/255.0])
        num -= 3
        i += 3
    colorSequenceFx = sequenceFx(period, colors)
    oscPlayEffect("sequence", 0)

# Handlers expect msg packets
oocpHooks = {
    "red-pulse":(lambda ignore: oscPlayEffect("red-pulse",0)),
    "white":(lambda ignore: oscPlayEffect("white", 0)),
    "black":(lambda ignore: oscPlayEffect("black", 0)),
    "amber":(lambda ignore: oscPlayEffect("amber", 0)),
    "resume":(lambda ignore: oscPlayEffect("", 0)),
    "sequence":playSequenceHandler
}

# Support the OOCP
def oocpHandler(msg):
    # ignore our own kelper messages
    print msg.path
    print msg.data
    effectName = msg.data[0]
    handler = oocpHooks.get(effectName, False)
    # if handler and currentEffect and currentEffect["name"] != effectName :
    if handler:
        print handler
        handler(msg)
    else:
        print "Skipping - already in %s mode" % (effectName)

def dispatch(msg):
    if msg.path in ["/lights"]:
        if msg.path == "/lights":
            oocpHandler(msg)

if oocp:
    oocp.subscribe("*", dispatch)

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
jumpTo = False
jumpToDur = 30

currentEffect = False

defaultXfm = np.array([[-1,0,0],
                       [0,0,1],
                       [0,-1,0]])

def playFx(fx,fps,xfm=defaultXfm,options={},dur=0):
    global quitFlag
    print "Playing "+fx.__name__
    bright(1.0)
    start = time.time()
    playtil = start + dur
    xfmList = None
    if xfm != None:
        xfmList = transformList(xfm)
    quitFlag=False
    while not (quitFlag or (dur and time.time()>playtil)):
        fx(time.time()-start)   # start at t=0
        time.sleep(max((1.0/fps)-interMsgDelay, 0))
        if getKey() or buttonDown():
            quitFlag = True
        if(quitFlag):
            break
    return quitFlag

def playMovie(fn,fps,xfm=defaultXfm,options={},dur=0):
    global quitFlag
    bright(1.0)
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
        if getKey() or buttonDown():
            quitFlag = True
        if(quitFlag):
            break
    return quitFlag

def playEffect(effect, xfm=defaultXfm, options={}, dur=0):
    global currentEffect
    currentEffect = effect
    bright(1.0)
    instance = effect["movie"]
    print "Playing "+ effect.get("name", "un-named")
    # load up any options or xfms associated with clip
    bright(1.0)
    if type(instance)==type(""):
        # handle a 'movie'
        playMovie(instance,fps,xfm,options,dur)
    else:
        # assume it's a function that computes a new frame
        print instance
        playFx(instance, fps, xfm, options, dur)

# Effects by name 

effects=[
    {"name": "hues",        "movie":huePulse(hue,lambda x:x,lambda x:0.3*x,8)}, 
    {"name": "red-pulse",   "movie":colorPulse(1.0, 0.0, 0.0)},
    {"name": "waves",       "movie":"../media/raw888/Waves_8x8x8_color.raw"},
    {"name": "blue-pulse",  "movie":colorPulse(0.0, 0.0, 1.0)},
    {"name": "test-pattern","movie":"../media/raw888/TestXYZ_8x8x8_color.raw"},
    {"name": "two-balls",   "movie":"../media/raw888/TwoBalls_8x8x8_color.raw"},
    {"name": "green-pulse", "movie":colorPulse(0.0, 1.0, 0.0)},
    {"name": "rainbow",     "movie":"../media/raw888/PlaqueRainbowRotation_8x8x8_color.raw"},
    {"name": "explode",     "movie":"../media/cs/explode.eca"},
    {"name": "drape",       "movie":"../media/cs/drape.eca"},
    # solid colors 
    {"name": "white", "movie":solid(1.0, 1.0, 1.0), "auto":False},
    {"name": "black", "movie":solid(0.0, 0.0, 0.0), "auto":False},
    {"name": "amber", "movie":solid(0.8, 0.8, 0.0), "auto":False},
    # sequence of colors - hold, decay, period
    {"name": "sequence", "movie":pulseSequence(0.1, 2.0, 2.0), "auto":False}
]

def findEffect(name):
    for x in effects:
        if x["name"] == name:
            return x
    return False

def playAllEffects():
    # should add a selector for default effects
    n = 0
    while 1:
        for x in effects:
            if x.get("auto", True):
                processJumpTo()
                playEffect(x,dur=30.0)

def playEffectName(name, duration=0):
    effect = findEffect(name)
    if effect:
        playEffect(effect, dur=duration)	# 0=forever

def processJumpTo():
    # if flags have been set, the jump to that effect NOW, and clear jumpFlag
    global jumpTo, jumpToDur
    if jumpTo:
        effectName = jumpTo
        jumpTo = False
        playEffectName(effectName, jumpToDur)

if __name__ == "__main__":
    playAllEffects()
