/*
 * RGBConverter.h - Arduino library for converting between RGB, HSV and HSL
 * 
 * Ported from the Javascript at http://mjijackson.com/2008/02/rgb-to-hsl-and-rgb-to-hsv-color-model-conversion-algorithms-in-javascript
 * The hard work was Michael's, all the bugs are mine.
 *
 * Robert Atkins, December 2010 (ratkins_at_fastmail_dot_fm).
 *
 * https://github.com/ratkins/RGBConverter
 *
 */  
#ifndef RGBConverter_h
#define RGBConverter_h

#if ARDUINO>=100
#include <Arduino.h>	// Arduino 1.0
#else
#include <Wprogram.h>	// Arduino 0022
#endif

#define threeway_max(a, b, c) max(a, max(b, c))
#define threeway_min(a, b, c) min(a, min(b, c))

class RGBConverter {

public:
    /**
     * Converts an RGB color value to HSL. Conversion formula
     * adapted from http://en.wikipedia.org/wiki/HSL_color_space.
     * Assumes r, g, and b are contained in the set [0, 255] and
     * returns h, s, and l in the set [0, 1].
     *
     * @param   byte    r       The red color value
     * @param   byte    g       The green color value
     * @param   byte    b       The blue color value
     * @param   float  hsl[]   The HSL representation
     */
    void rgbToHsl(byte r, byte g, byte b, float hsl[]);
    
    /**
     * Converts an HSL color value to RGB. Conversion formula
     * adapted from http://en.wikipedia.org/wiki/HSL_color_space.
     * Assumes h, s, and l are contained in the set [0, 1] and
     * returns r, g, and b in the set [0, 255].
     *
     * @param   float  h       The hue
     * @param   float  s       The saturation
     * @param   float  l       The lightness
     * @return  byte    rgb[]   The RGB representation
     */
    void hslToRgb(float h, float s, float l, byte rgb[]);

    /**
     * Converts an RGB color value to HSV. Conversion formula
     * adapted from http://en.wikipedia.org/wiki/HSV_color_space.
     * Assumes r, g, and b are contained in the set [0, 255] and
     * returns h, s, and v in the set [0, 1].
     *
     * @param   byte  r       The red color value
     * @param   byte  g       The green color value
     * @param   byte  b       The blue color value
     * @return  float hsv[]  The HSV representation
     */
    void rgbToHsv(byte r, byte g, byte b, float hsv[]);
    /**
     * Converts an HSV color value to RGB. Conversion formula
     * adapted from http://en.wikipedia.org/wiki/HSV_color_space.
     * Assumes h, s, and v are contained in the set [0, 1] and
     * returns r, g, and b in the set [0, 255].
     *
     * @param   float  h       The hue
     * @param   float  s       The saturation
     * @param   float  v       The value
     * @return  byte    rgb[]   The RGB representation
     */
    void hsvToRgb(float h, float s, float v, byte rgb[]);
     
private:
    float hue2rgb(float p, float q, float t);
};

#endif
