import oscP5.*;
import netP5.*;

/*
 * Emulate Kelp (8x8x8) under OSC control
 */

float boxSize = 40;
float margin = boxSize*2;
float depth = 8*boxSize/2;
color boxFill;
color black;
int cubeSize=8;
float bright=1.0;

// color[][] img = new color[cubeSize][cubeSize*cubeSize];

PImage img = createImage(cubeSize,cubeSize*cubeSize,ARGB);

OscP5 oscP5;
void setup() {
  size(640, 360, P3D);
  noStroke();
  black = color(0,0,0);
  // listen for OSC on Communicore default 239.192.192.192:9192
  oscP5 = new OscP5(this,"239.192.192.192",9192);
}

void draw() {
    // background(200);
    background(30);
    img.loadPixels();
    image(img,0,0,cubeSize*3,cubeSize*cubeSize*3);

    // print("Hello?"+frameCount);
    float bound = (cubeSize-1)*boxSize/2;
    
    // Center and spin grid
    translate(width/2, height/2, -depth);
    rotateY(frameCount * 0.001);
    rotateX(frameCount * 0.001);

    // Build grid using multiple translations 
    for (int z = 0; z<cubeSize; ++z){
        pushMatrix();
        for (int y = 0; y<cubeSize; ++y){
            pushMatrix();
            for (int x = 0; x<cubeSize; ++x){
                // Base fill color on counter values, abs function 
                // ensures values stay within legal range
                boxFill = img.pixels[x+(y*cubeSize)+(z*cubeSize*cubeSize)];
                boxFill = lerpColor(boxFill, black, (1.0 - bright));
                // boxFill = color(x*255/cubeSize, y*255/cubeSize, z*255/cubeSize, 200);
                pushMatrix();
                translate(-bound + (x*boxSize),
                          -bound + (y*boxSize),
                          -bound + (z*boxSize));
                fill(boxFill);
                box(boxSize/5, boxSize/5, boxSize/5);
                popMatrix();
            }
            popMatrix();
        }
        popMatrix();
  }
}

void fill(OscMessage oscmsg){
    int r = (int)(oscmsg.get(0).floatValue()*255);
    int g = (int)(oscmsg.get(1).floatValue()*255);
    int b = (int)(oscmsg.get(2).floatValue()*255);
    int a = 255;
         
    for(int y=0; y<(cubeSize*cubeSize); y++){
        for(int x=0; x<cubeSize; x++){
            img.pixels[x+y*cubeSize] = color(r, g, b, a);
            // println("x="+x+" y="+y+" p="+p+" color="+r+","+g+","+b);
        }
    }
}

void bright(OscMessage oscmsg){
    bright = oscmsg.get(0).floatValue();
}

void copyImageXY(OscMessage oscmsg){
    // Treats the cube as a tall image 
    int w = oscmsg.get(0).intValue();
    int h = oscmsg.get(1).intValue();
    int baseX = oscmsg.get(2).intValue();
    int baseY = oscmsg.get(3).intValue();
    byte[] data = oscmsg.get(4).bytesValue();

    // brightness is precomputed for movies
    bright = 1.0;

    for(int sy=0; sy<h; sy++){
        for(int sx=0; sx<w; sx++){
            int x = baseX+sx;
            int y = baseY+sy;
            // index into the array of 4byte pixels
            int p = (sx+(sy*w))*4;
            int r = data[p+0] & 0xff;
            int g = data[p+1] & 0xff;
            int b = data[p+2] & 0xff;
            int a = data[p+3] & 0xff;
            img.pixels[x+y*cubeSize] = color(r, g, b, a);
            // println("x="+x+" y="+y+" p="+p+" color="+r+","+g+","+b);
        }
    }
}

void oscEvent(OscMessage oscmsg) {
  /* print the address pattern and the typetag of the received OscMessage */
  // print("### received an osc message.");
  // print(" addrpattern: "+oscmsg.addrPattern());
  // println(" typetag: "+oscmsg.typetag());

  if(oscmsg.checkAddrPattern("/screenxy")){
      copyImageXY(oscmsg);
  } else if(oscmsg.checkAddrPattern("/fill")) {
      fill(oscmsg);
  } else if(oscmsg.checkAddrPattern("/bright")) {
      bright(oscmsg);
  } else {
      println("unknown OSC message: "+oscmsg.addrPattern());
  }
}

