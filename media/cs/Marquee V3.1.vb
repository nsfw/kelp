'-------------------------------------------------------------------------------
'
'     X     X
'     XX   XX
'     X X X X  XXX  XXXX   XXX  X   X XXXXX XXXXX    X   X XXXX
'     X  X  X X   X X   X X   X X   X X     X        X   X X   X
'     X     X XXXXX XXXX  X   X X   X XXXX  XXXX     X   X XXXX
'     X     X X   X X  X  X X X X   X X     X         X X  X   X
'     X     X X   X X   X  XXX   XXX  XXXXX XXXXX XX   X   XXXX
'                             X
'-------------------------------------------------------------------------------
'
' MARQUEE.VB - a Cubesense VB Script for the eightCubed V2 LED cube
'
' Scroll a string of raster-pattern text characters around the perimeter of an
' eightCubed V2 RGB LED cube, in the manner of a marquee light display. Works
' with any cubic or rectangular lattice having a minimum height (z-dimension)
' of 8 voxels
'
' Author:   Kurt A. Koehler
' Date:     June 2011
'
' CHANGE HISTORY
'
' 07/11/11: version 2.0 - added a new run-time option 'Decay' (True/False) to
'           implement a progressive illumination/decay' technique that can
'           cause the colors of leading and trailing text LEDs to 'fade in' and
'           'fade out' during a delay cycle, helping to create a an illusion
'           of smoother image movement
'
' 08/08/11: version 3.0
'           - added a new run-time option 'FontStyle' to provide the means of
'           selecting from a choice of raster font designs ("Terminal" | "Ozone" |
'           "Gothic")
'           - added a new run time option 'Depth' (1 - 4) causing
'           raster patterns of text to be drawn with a 'thickness' of 'Depth'
'           voxels, giving the block letters a 3D appearance when Depth > 1
'           - added a new run-time option 'Spacing' to define the number of
'           inter-character 'blank' voxels to insert between adjacent letters
'
' 09/23/11: version 3.1
'           - reorganized some code to improve efficiency & performance
'           - corrected problems in raster pattern definitions for comma (",") &
'             lower-case "r" in the 'Gothic' font that resulted in run-time errors
'           - minor revisions to source code indentation, documentation & comments
'           - added vertical (z-dimension) centering of text for larger cubes
'
'-------------------------------------------------------------------------------

Structure RGB
  Dim R As Byte
  Dim G As Byte
  Dim B As Byte
End Structure

Structure ExecParms
  Dim TextString As String  ' text to be scrolled around the LED cube
  Dim FontStyle  As String  ' supported values: "Terminal" | "Ozone" | "Gothic'
  Dim Delay      As Byte    ' number of frames to delay before incrementing text movement
  Dim Decay      As Boolean ' use progressive illumination/decay to smooth movement during delay
  Dim Depth      As Integer ' gives text characters 3D depth if > 1; supported values: 1 - 4
  Dim Cycles     As Integer ' number of complete rotations of text around the cube
  Dim Spacing    As Integer ' number of 'blank' voxels between text characters
  Dim Background As String  ' supported values: "void" | "asis" | "fill" | "edge"
  Dim BackColor  As RGB     ' only required for ExecParms.Background = "fill" | "edge"
  Dim Foreground As String  ' supported values: "rainbow" | "color"
  Dim ForeColor  As RGB     ' only required when ExecParms.Foreground = "color"
End Structure

Public Options                     As ExecParms
Public BlockText (0 To 95, 0 To 7) As String
Public vbTrue                      As Boolean = -1
Public vbFalse                     As Boolean = 0

Public Sub Main() Implements scriptInterface.IScript.Main

'-------------------------------------------------------------------------------
'
' EXECUTION OPTIONS - (values can be set in code immediately below this comment
'                      block)
'
' - Options.TextString is the text value of the marquee message and can contain
'   any combination of upper-case, lower-case, numeric and special text characters
'
' - Options.FontStyle is a text value naming the font style to be used for
'   generating the scrolling text characters. Currently supported values for
'   Options.FontStyle are:
'   "Terminal" - simple block letters of generally uniform width, modelled from
'                the 'Terminal' Windows bitmap font; 1 voxel ascenders
'   "Gothic"   - simple block letters of generally uniform width, modelled from
'                a Letter Gothic Windows bitmap font; 2 voxel ascencers
'   "Ozone"    - stylized bold block letters specifically designed for LED/LCD
'                applications (an emulated dot-matrix raster font pattern),
'                modelled from the Ozone.ttf' Windows font; 2 voxel ascenders
'   Default value of Options.FontStyle is "Ozone"
'
' - Options.Delay is a numeric integer specifying how many times a text pattern
'   is to be repeated in successive frames before the next (incremental) movement
'   of the marquee text is generated; the lower the value, the faster the text moves.
'   Value must be a positive integer or 0; default value of Options.Delay is 6.
'
' - Options.Decay is a boolean value indicating whether a progressive illumination
'   and decay technique is to be used create the illusion of smooth movement during
'   the Options.Delay period. Default value of Options.Decay is True.
'
' - Options.Depth is a positive integer specifying the 3D 'thickness' (in voxels)
'   of text characters displayed on the LED cube. Supported values range from 1 to 4;
'   Default value of Options.Depth is 2 (recommended: 1 for Terminal font, 2 or 3
'   for Ozone & Gothic fonts, depending upon value of Options.Depth)
'
' - Options.Cycles is a positive integer specifying the number of complete scrolling
'   text rotations about the cube this animation is to contain. This option defaults
'   to a value of 1, but higher values can be useful when overlaying an existing
'   ECA animation that is multiple times larger (in frames) than a single cycle of
'   the marquee animation
'
' - Options.Spacing is a numeric integer defining the number of inter-character
'   'blank' voxels to insert between adjacent letters in the text string.
'   Default value of Options.Spacing' is 2 (recommended: 1 for Terminal font, 2 for
'   Ozone font)
'
' - Options.Background is a text value that specifies the preferred method of
'   illuminating lights in the cube 'behind' the text and can have values of:
'   "void"     - clears the color values (to RGB 0,0,0) of all LEDs in each frame
'                before generating the marquee text
'   "asis"     - leaves any exisiting frame content intact but overlays text on
'                the cube perimeter (useful for adding a marquee to another
'                animation already loaded)
'   "fill"     - fills entire cube with a solid color before rendering the text
'                pattern; Options.BackColor R, G and B values must also be supplied
'   "edge"     - fills only the outer layer (Options.Depth voxels deep) of LEDs
'                around the vertical perimeter of the cube with a solid color before
'                rendering the text pattern; Options.BackColor R, G and B values
'                must also be supplied
'   Default value of Options.Background is "void".
'
' - Options.Foreground is a text value that specifies the preferred method of
'   illuminating lights in the text raster pattern and can have values of:
'   "rainbow"  - raster text is presented using a 'rainbow' gradient of color,
'                incrementally changing as it scrolls across the face of the cube
'   "color"    - text is presented using a solid color value;  Options.BackColor
'                R, G and B values must also be supplied
'   Default value of Options.Foreground is "rainbow"
'
' - Options.ForeColor and Options.BackColor. R, G and B variables are integer values
'   in the range of 0 - 255 that specify the intensity of Red, Green and Blue
'   components of the text foreground or cube background colors.
'
'-------------------------------------------------------------------------------
'
' CAUTION: Little to no validation is performed on the values coded in the Options
'          parameters, so please adjust values within recommended ranges; unpredictable
'          results, including script failure, may otherwise occur'

  Options.TextString  = "www.Lumisense.com - Home to eightCubed & Cubesense"
  Options.FontStyle   = "Ozone"      ' "Gothic" | "Terminal" | "Ozone"
  Options.Delay       = 6            ' integer >= 0 ; recommended: 3 - 6
  Options.Decay       = vbTrue       ' vbTrue | vbFalse
  Options.Depth       = 2            ' integer in the range of 1 - 4
  Options.Cycles      = 1            ' integer > 0
  Options.Spacing     = 2            ' integer >= 0; recommended 1 - 3,
                                     ' depending on FontStyle and Depth
  Options.Background  = "void"       ' "void" | "asis" | "fill" | "edge"
  Options.BackColor.R = 0            ' integer 0 - 255, required when Background <> "void"
  Options.BackColor.G = 0            ' integer 0 - 255, required when Background <> "void"
  Options.BackColor.B = 0            ' integer 0 - 255, required when Background <> "void"
  Options.Foreground  = "color"    ' "rainbow" | "color"
  Options.ForeColor.R = 255          ' integer 0 - 255, required only when Foreground = "color"
  Options.ForeColor.G = 0          ' integer 0 - 255, required only when Foreground = "color"
  Options.ForeColor.B = 0          ' integer 0 - 255, required only when Foreground = "color"

' Variables - Declaration & Initialization

  Dim InputString         As String
  Dim Line (7)            As String
  Dim msgText             As String
  Dim r, rr, g, gg, b, bb As Integer
  Dim rX, gX, bX          As Integer
  Dim CrLf                As String = Environment.NewLine
  Dim Spacer              As String = New String(" "c, Options.Spacing)
  Dim i, j, k, l, c, d    As Integer
  Dim xx, yy, zz, dK      As Integer
  Dim pctDecay            As Decimal = 0.000
  Dim I1                  As Decimal = 0.000
  Dim I2                  As Decimal = 0.000
  Dim frameCount          As Integer = cs.getFrameCount() - 1
  Dim totalFrames         As Integer = 0
  Dim response            As Integer = 0
  Dim f                   As Integer = 0
  Dim xSize               As Integer = cs.getSizeX()
  Dim ySize               As Integer = cs.getSizeY()
  Dim zSize               As Integer = cs.getSizeZ()
  Dim xMax                As Integer = xSize - 1
  Dim yMax                As Integer = ySize - 1
  Dim zMax                As Integer = zSize - 1
  Dim xMin                As Integer = 0
  Dim yMin                As Integer = 0
  Dim zMin                As Integer = 0
  Dim x                   As Integer = 0
  Dim y                   As Integer = 0
  Dim z                   As Integer = 0
  Dim centerPadZ          As integer = (zSize - 8) / 2

' Terminate processing if the LED cube is too short to display the Marquee text characters

  If zSize < 8 Then
    System.Windows.Forms.MessageBox.Show ("Error: The Z-dimension of the cube is too small for a Marquee" & _
      " animation - your cube must be at least 8 LEDs in height for the text characters to display" & _
      CrLf & CrLf & "Processing is terminated.", _
      "Marquee.VB Script", _
      System.Windows.Forms.MessageBoxButtons.OK, _
      System.Windows.Forms.MessageBoxIcon.Stop)
    Exit Sub
  End If

' Load BlockText with the raster pattern of the font specified in Options.Fontstyle

 Call RasterFont ' invoke the RasterFont subroutine to load BlockText with the raster
                 ' pattern of the font named in Options.FontStyle

' Prompt the user to enter/modify the text to be presented on the LED cube marquee

 InputString = Microsoft.VisualBasic.Interaction.InputBox _
   ("Enter the text string to be scrolled across the face of the LED cube:", _
   "Marquee.VB Script", _
   Options.TextString)

 If InputString = "" Then Exit Sub

 Options.TextString = InputString

' Initialize TextString to all ASCII text characters (for testing font design changes only)

' Options.TextString = ""
' For i = 32 To 127
'    Options.TextString = Options.TextString & Convert.ToChar(i)
' Next

' Construct the 'Line' array to contain the raster patterns of all characters
' in 'Options.TextString'

  L = Options.TextString.Length - 1

  For j = 0 To L ' for each character in Options.TextString...
    k = Convert.ToInt32 (Convert.ToChar (Options.TextString.Substring (j, 1))) - 32

'   Append the raster pattern of each character in 'Options.TextString' to the 'Line' array

    For i = 0 To 7
      Line (i) = Line (i) + BlockText (k, i) + Spacer
    Next
  Next

' Add a number of blank characters to the beginning and end of the Raster Pattern
' equal to the total number of columns of LEDs around the perimeter of the cube.

  For i = 0 To 7
    Line (i) = String.empty.PadLeft (2 * (xMax + yMax), " "c) + Line (i) + _
               String.empty.PadLeft (2 * (xMax + yMax), " "c)
  Next

' Set the value of L to the total number of columns contained in the raster pattern
' of text to be rotated about the perimeter of the cube, including the leading blanks

  L = Line(0).Length - 2 * (xMax + yMax)

' Calculate the total number of frames that will be required by this animation and compare
' it to the value returned by cs.getFrameCount(), the number of frames defined in the
' 'Frames List'; if they are unequal, display an appropriate warning mesage to the user
' before proceeding to compute the frame content for the marquee display.

  totalFrames = L * Options.Cycles * (Options.Delay + 1)
  frameCount  = cs.getFrameCount()

  If totalFrames <> frameCount Then
    msgText = "This animation requires " & totalFrames.ToString & _
      " frames to complete the marquee display. "
    If totalFrames > frameCount Then
      msgText = msgText & "Not enough frames exist in the 'Frames List' to generate a" & _
        " full rotation... OK to proceed?"
    Else
      msgText = msgText & "More frames exist in the 'Frames List' than are needed to" & _
        " generate a full rotation... OK to proceed?"
    End If

    response = System.Windows.Forms.MessageBox.Show (msgText, _
               "Marquee.VB Script", _
                System.Windows.Forms.MessageBoxButtons.OKCancel, _
                System.Windows.Forms.MessageBoxIcon.Exclamation)
    If response = System.Windows.Forms.DialogResult.Cancel Then Exit Sub

    totalFrames = Math.Min (totalFrames, frameCount)
  End If

' Establish the text foreground color according to the 'ForeColor' option specified

  If Options.Foreground = "color" Then
    r = Options.ForeColor.R ' red component of foreground color
    g = Options.ForeColor.G ' green component of foreground color
    b = Options.ForeColor.B ' blue component of foreground color
  ElseIf Options.Foreground = "rainbow" Then
    Options.ForeColor.R = 0 ' red component of foreground color
    Options.ForeColor.G = 0 ' green component of foreground color
    Options.ForeColor.B = 0 ' blue component of foreground color
  End If

' Establish the background color according to the 'BackColor' option specified

  If Options.Background = "void" Or Options.Background = "asis" Then
    Options.BackColor.R = 0 ' red component of background color
    Options.BackColor.G = 0 ' green component of background color
    Options.BackColor.B = 0 ' blue component of background color
  End If

  rX = Options.BackColor.R ' red component of background color
  gX = Options.BackColor.G ' green component of background color
  bX = Options.BackColor.B ' blue component of background color

' To create an animation occupying a predetermined number of frames, it may be necessary to
' cycle the scrolling text around the LED more than once. Cause the marquee animation
' to be repeated 'Options.Cycles' numer of times.

  For c = 1 To Options.Cycles

'   To implement the marquee animation, incrementally move the text raster pattern text
'   forward by one column about the perimeter of the cube and retain the pattern in
'   that position for 'Delay' number of frames (increasing the value of 'Delay' will
'   slow the movement of the marquee effect).

    For i = 1 To L

      For dK = 0 To Options.Delay ' Repeat same raster text pattern for 'Delay' number of frames

        If Options.Delay > 0 And Options.Decay = vbTrue Then
          pctDecay = dK / Options.Delay
        End If

        I1 = 1.000 - pctDecay    ' primary intensity
        I2 = pctDecay            ' secondary intensity

'     Illuminate the background color according to the options specified

        If Options.Background <> "asis" Then
          If Options.Background = "fill" Or Options.Background = "void" Then
            cs.drawCube (f, xMin, yMin, zMin, xMax, yMax, zMax, rX, gX, bX)
          ElseIf Options.Background = "edge" Then
            cs.drawCube (f, xMin, yMin, zMin, xMax, yMin + Options.Depth - 1, zMax, rX, gX, bX)
            cs.drawCube (f, xMin, yMin, zMin, xMin + Options.Depth - 1, yMax, zMax, rX, gX, bX)
            cs.drawCube (f, xMax - Options.Depth + 1, yMin, zMin, xMax, yMax, zMax, rX, gX, bX)
            cs.drawCube (f, xMin, yMax - Options.Depth + 1, zMin, xMax, yMax, zMax, rX, gX, bX)
          End If
        End If

'       Illuminate a portion of the raster pattern of text characters about the perimeter
'       of the cube

        For zz = 0 To 7 ' each row in raster pattern
          z = zMax - zz - centerPadZ ' cube row is inverse of pattern row

          y = yMin ' paint the West face

          For xx = 0 To xMax - 1 ' xx = column# in x-planes of cube
            If Line (zz).Substring (xx + i + xMax + 2 * yMax, 1) <> " " Then
              x = xMax - xx - 1

              If Options.Foreground = "rainbow" Then
                r = 255 * x / xMax
                g = 255 * y / yMax
                b = 255 * z / zMax
              End If

              If Line (zz).Substring (xx + i + 1 + xMax + 2 * yMax, 1) = " " Then
                rr = r * I1 + Options.BackColor.R * I2
                gg = g * I1 + Options.BackColor.G * I2
                bb = b * I1 + Options.BackColor.B * I2
                For d = 1 To Options.Depth
                  If x >= d - 1 And x <= xMax - d + 1 Then _
                    cs.drawVoxel (f, x, y + d - 1, z, rr, gg, bb)
                Next
              Else
                For d = 1 To Options.Depth
                  If x >= d - 1 And x <= xMax - d + 1 Then _
                    cs.drawVoxel (f, x, y + d - 1, z, r, g, b)
                Next
              End If

              If Line (zz).Substring (xx + i - 1 + xMax + 2 * yMax, 1) = " " Then
                If Options.Foreground = "rainbow" Then r = 255 * (x + 1) / xMax
                rr = r * I2 + Options.BackColor.R * I1
                gg = g * I2 + Options.BackColor.G * I1
                bb = b * I2 + Options.BackColor.B * I1
                For d = 1 To Options.Depth
                  If x >= d - 1 And x <= xMax - d + 1 Then _
                    cs.drawVoxel (f, x + 1, y + d - 1 , z, rr, gg, bb)
                Next
              End If
            End If
          Next ' For xx = 0 To xMax - 1

          x = xMax ' Paint the North face

          For yy = 0 To yMax - 1 ' yy = column# in y-planes of cube
            If Line (zz).Substring (yy + i + xMax + yMax, 1) <> " " Then
              y = yMax - yy - 1

              If Options.Foreground = "rainbow" Then
                r = 255 * x / xMax
                g = 255 * y / yMax
                b = 255 * z / zMax
              End If

              If Line (zz).Substring (yy + i + 1 + xMax + yMax, 1) = " " Then
                rr = r * I1 + Options.BackColor.R * I2
                gg = g * I1 + Options.BackColor.G * I2
                bb = b * I1 + Options.BackColor.B * I2
                For d = 1 To Options.Depth
                  If y >= d - 1 And y <= yMax - d + 1 Then _
                    cs.drawVoxel (f, x - d + 1 , y, z, rr, gg, bb)
                Next
              Else
                For d = 1 To Options.Depth
                  If y >= d - 1 And y <= yMax - d + 1 Then _
                     cs.drawVoxel (f, x - d + 1, y, z, r, g, b)
                Next
              End If

              If Line (zz).Substring (yy + i - 1 + xMax + yMax, 1) = " " Then
                If Options.Foreground = "rainbow" Then g = 255 * (y + 1) / yMax
                rr = r * I2 + Options.BackColor.R * I1
                gg = g * I2 + Options.BackColor.G * I1
                bb = b * I2 + Options.BackColor.B * I1
                For d = 1 To Options.Depth
                  If y >= d - 1 And y <= yMax - d + 1 Then _
                    cs.drawVoxel (f, x - d + 1, y + 1, z, rr, gg, bb)
                Next
              End If
            End If
          Next ' For yy = 0 To yMax - 1

          y = yMax ' Paint the East face

          For xx = 0 To xMax - 1 ' xx = column# in x-planes of cube
            If Line (zz).Substring (xx + i + yMax, 1) <> " " Then
              x = xx + 1

              If Options.Foreground = "rainbow" Then
                r = 255 * x / xMax
                g = 255 * y / yMax
                b = 255 * z / zMax
              End If

              If Line (zz).Substring (xx + i + 1 + yMax, 1) = " " Then
                rr = r * I1 + Options.BackColor.R * I2
                gg = g * I1 + Options.BackColor.G * I2
                bb = b * I1 + Options.BackColor.B * I2
                For d = 1 To Options.Depth
                  If x >= d - 1 And x <= xMax - d + 1 Then _
                    cs.drawVoxel (f, x, y - d + 1, z, rr, gg, bb)
                Next
              Else
                For d = 1 To Options.Depth
                  If x >= d - 1 And x <= xMax - d + 1 Then _
                    cs.drawVoxel (f, x, y - d + 1, z, r, g, b)
                Next
              End If

              If Line (zz).Substring (xx + i - 1 + yMax, 1) = " " Then
                If Options.Foreground = "rainbow" Then r = 255 * (x - 1) / xMax
                rr = r * I2 + Options.BackColor.R * I1
                gg = g * I2 + Options.BackColor.G * I1
                bb = b * I2 + Options.BackColor.B * I1
                For d = 1 To Options.Depth
                  If x >= d - 1 And x <= xMax - d + 1 Then _
                    cs.drawVoxel (f, x - 1, y - d + 1, z,  rr, gg, bb)
                Next
              End If
            End If
          Next ' For xx = 0 To xMax - 1

          x = xMin ' Paint the South face

          For yy = 0 To yMax - 1 ' yy = column# in y-planes of cube
            If Line (zz).Substring (yy + i, 1) <> " " Then
              y = yy + 1

              If Options.Foreground = "rainbow" Then
                r = 255 * x / xMax
                g = 255 * y / yMax
                b = 255 * z / zMax
              End If

              If Line (zz).Substring (yy + i + 1, 1) = " " Then
                rr = r * I1 + Options.BackColor.R * I2
                gg = g * I1 + Options.BackColor.G * I2
                bb = b * I1 + Options.BackColor.B * I2
                For d = 1 To Options.Depth
                  If y >= d - 1 And y <= yMax - d + 1 Then _
                    cs.drawVoxel (f, x + d - 1, y, z, rr, gg, bb)
                Next
              Else
                For d = 1 To Options.Depth
                  If y >= d - 1 And y <= yMax - d + 1 Then _
                    cs.drawVoxel (f, x + d - 1, y, z, r, g, b)
                Next
              End If

              If Line (zz).Substring (yy + i - 1, 1) = " " Then
                If Options.Foreground = "rainbow" Then b = 255 * (y - 1) / yMax
                rr = r * I2 + Options.BackColor.R * I1
                gg = g * I2 + Options.BackColor.G * I1
                bb = b * I2 + Options.BackColor.B * I1
                For d = 1 To Options.Depth
                  If y >= d - 1 And y <= yMax - d + 1 Then _
                    cs.drawVoxel (f, x + d - 1, y - 1, z, rr, gg, bb)
                Next
              End If
            End If
          Next ' For yy = 0 To yMax - 1

        Next ' For zz = 0 To 7

        cs.showProgress(100 * f / totalFrames)
        f = f + 1

        If (cs.checkTermination() = vbTrue Or f > totalFrames) Then Exit For

      Next ' For dK = 0 To Options.Delay
    Next ' For i = 1 To L
  Next ' For c = 1 To Options.Cycles

  System.Windows.Forms.MessageBox.Show ("Marquee display generation created " & _
                                        Math.Min (f, totalFrames).ToString  & " frames.", _
                                        "Marquee.VB Script", _
                                        System.Windows.Forms.MessageBoxButtons.OK, _
                                        System.Windows.Forms.MessageBoxIcon.Asterisk)
End Sub

Public Sub RasterFont ()

' Gothic Font
' Block text character raster pattern definitions
' First ASCII char0: 32, last ASCII char = 127 (total=96)

Dim GothicFont (,) As String = New String(0 To 95, 0 To 7) { _
{     "   ",  _
      "   ",  _
      "   ",  _
      "   ",  _
      "   ",  _
      "   ",  _
      "   ",  _
      "   "}, _
{      "XX",  _
       "XX",  _
       "XX",  _
       "XX",  _
       "XX",  _
       "  ",  _
       "XX",  _
       "  "}, _
{   "XX XX",  _
    "XX XX",  _
    "     ",  _
    "     ",  _
    "     ",  _
    "     ",  _
    "     ",  _
    "     "}, _
{ "       ",  _
  " XX XX ",  _
  "XXXXXXX",  _
  " XX XX ",  _
  " XX XX ",  _
  "XXXXXXX",  _
  " XX XX ",  _
  "       "}, _
{  "  XX  ",  _
   " XXXXX",  _
   "XXXX  ",  _
   " XXXX ",  _
   "  XXXX",  _
   "XXXXX ",  _
   "  XX  ",  _
   "      "}, _
{  "XXX   ",  _
   "XXX XX",  _
   "   XX ",  _
   "  XX  ",  _
   " XX   ",  _
   "XX XXX",  _
   "   XXX",  _
   "      "}, _
{  " XXX  ",  _
   "XX XX ",  _
   "XX XX ",  _
   " XXX  ",  _
   "XX XXX",  _
   "XX XX ",  _
   " XXX X",  _
   "      "}, _
{      "XX",  _
       "XX",  _
       "  ",  _
       "  ",  _
       "  ",  _
       "  ",  _
       "  ",  _
       "  "}, _
{     " XX",  _
      "XX ",  _
      "XX ",  _
      "XX ",  _
      "XX ",  _
      "XX ",  _
      " XX",  _
      "   "}, _
{     "XX ",  _
      " XX",  _
      " XX",  _
      " XX",  _
      " XX",  _
      " XX",  _
      "XX ",  _
      "   "}, _
{  "      ",  _
   "  XX  ",  _
   "XXXXXX",  _
   " XXXX ",  _
   "XXXXXX",  _
   "  XX  ",  _
   "      ",  _
   "      "}, _
{  "      ",  _
   "  XX  ",  _
   "  XX  ",  _
   "XXXXXX",  _
   "  XX  ",  _
   "  XX  ",  _
   "      ",  _
   "      "}, _
{     "   ",  _
      "   ",  _
      "   ",  _
      "   ",  _
      "   ",  _
      "XXX",  _
      " XX",  _
      "XX "}, _
{  "      ",  _
   "      ",  _
   "      ",  _
   "XXXXXX",  _
   "      ",  _
   "      ",  _
   "      ",  _
   "      "}, _
{     "   ",  _
      "   ",  _
      "   ",  _
      "   ",  _
      "   ",  _
      "XXX",  _
      "XXX",  _
      "   "}, _
{"      XX",  _
 "     XX ",  _
 "    XX  ",  _
 "   XX   ",  _
 "  XX    ",  _
 " XX     ",  _
 "XX      ",  _
 "        "}, _
{  " XXXX ",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   " XXXX ",  _
   "      "}, _
{  "  XX  ",  _
   " XXX  ",  _
   "  XX  ",  _
   "  XX  ",  _
   "  XX  ",  _
   "  XX  ",  _
   " XXXX ",  _
   "      "}, _
{  " XXXX ",  _
   "XX  XX",  _
   "    XX",  _
   "   XX ",  _
   "  XX  ",  _
   " XX   ",  _
   "XXXXXX",  _
   "      "}, _
{  " XXXX ",  _
   "XX  XX",  _
   "    XX",  _
   "  XXX ",  _
   "    XX",  _
   "XX  XX",  _
   " XXXX ",  _
   "      "}, _
{  "   XX ",  _
   "  XXX ",  _
   " XXXX ",  _
   "XX XX ",  _
   "XXXXXX",  _
   "   XX ",  _
   "   XX ",  _
   "      "}, _
{  "XXXXXX",  _
   "XX    ",  _
   "XXXXX ",  _
   "    XX",  _
   "    XX",  _
   "XX  XX",  _
   " XXXX ",  _
   "      "}, _
{  " XXXX ",  _
   "XX    ",  _
   "XX    ",  _
   "XXXXX ",  _
   "XX  XX",  _
   "XX  XX",  _
   " XXXX ",  _
   "      "}, _
{  "XXXXXX",  _
   "    XX",  _
   "   XX ",  _
   "  XX  ",  _
   "  XX  ",  _
   "  XX  ",  _
   "  XX  ",  _
   "      "}, _
{  " XXXX ",  _
   "XX  XX",  _
   "XX  XX",  _
   " XXXX ",  _
   "XX  XX",  _
   "XX  XX",  _
   " XXXX ",  _
   "      "}, _
{  " XXXX ",  _
   "XX  XX",  _
   "XX  XX",  _
   " XXXXX",  _
   "    XX",  _
   "    XX",  _
   " XXXX ",  _
   "      "}, _
{     "   ",  _
      "XXX",  _
      "XXX",  _
      "   ",  _
      "   ",  _
      "XXX",  _
      "XXX",  _
      "   "}, _
{     "   ",  _
      "XXX",  _
      "XXX",  _
      "   ",  _
      "   ",  _
      "XXX",  _
      " XX",  _
      "XX "}, _
{  "    XX",  _
   "   XX ",  _
   "  XX  ",  _
   " XX   ",  _
   "  XX  ",  _
   "   XX ",  _
   "    XX",  _
   "      "}, _
{  "      ",  _
   "      ",  _
   "XXXXXX",  _
   "      ",  _
   "XXXXXX",  _
   "      ",  _
   "      ",  _
   "      "}, _
{  "XX    ",  _
   " XX   ",  _
   "  XX  ",  _
   "   XX ",  _
   "  XX  ",  _
   " XX   ",  _
   "XX    ",  _
   "      "}, _
{  " XXXX ",  _
   "XX  XX",  _
   "   XX ",  _
   "  XX  ",  _
   "  XX  ",  _
   "      ",  _
   "  XX  ",  _
   "      "}, _
{" XXXXXX ",  _
 "XX    XX",  _
 "XX XXXXX",  _
 "XX XX XX",  _
 "XX XXXXX",  _
 "XX      ",  _
 " XXXXXX ",  _
 "        "}, _
{  "  XX  ",  _
   " XXXX ",  _
   "XX  XX",  _
   "XX  XX",  _
   "XXXXXX",  _
   "XX  XX",  _
   "XX  XX",  _
   "      "}, _
{  "XXXXX ",  _
   "XX  XX",  _
   "XX  XX",  _
   "XXXXX ",  _
   "XX  XX",  _
   "XX  XX",  _
   "XXXXX ",  _
   "      "}, _
{  " XXXX ",  _
   "XX  XX",  _
   "XX    ",  _
   "XX    ",  _
   "XX    ",  _
   "XX  XX",  _
   " XXXX ",  _
   "      "}, _
{  "XXXX  ",  _
   "XX XX ",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX XX ",  _
   "XXXX  ",  _
   "      "}, _
{  "XXXXXX",  _
   "XX    ",  _
   "XX    ",  _
   "XXXXX ",  _
   "XX    ",  _
   "XX    ",  _
   "XXXXXX",  _
   "      "}, _
{  "XXXXXX",  _
   "XX    ",  _
   "XX    ",  _
   "XXXXX ",  _
   "XX    ",  _
   "XX    ",  _
   "XX    ",  _
   "      "}, _
{  " XXXX ",  _
   "XX  XX",  _
   "XX    ",  _
   "XX XXX",  _
   "XX  XX",  _
   "XX  XX",  _
   " XXXXX",  _
   "      "}, _
{  "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XXXXXX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "      "}, _
{    "XXXX",  _
     " XX ",  _
     " XX ",  _
     " XX ",  _
     " XX ",  _
     " XX ",  _
     "XXXX",  _
     "    "}, _
{  "  XXXX",  _
   "   XX ",  _
   "   XX ",  _
   "   XX ",  _
   "XX XX ",  _
   "XX XX ",  _
   " XXX  ",  _
   "      "}, _
{  "XX  XX",  _
   "XX XX ",  _
   "XXXX  ",  _
   "XXX   ",  _
   "XXXX  ",  _
   "XX XX ",  _
   "XX  XX",  _
   "      "}, _
{  "XX    ",  _
   "XX    ",  _
   "XX    ",  _
   "XX    ",  _
   "XX    ",  _
   "XX    ",  _
   "XXXXXX",  _
   "      "}, _
{ "XX   XX",  _
  "XXX XXX",  _
  "XXXXXXX",  _
  "XX X XX",  _
  "XX   XX",  _
  "XX   XX",  _
  "XX   XX",  _
  "       "}, _
{  "XX  XX",  _
   "XXX XX",  _
   "XXXXXX",  _
   "XX XXX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "      "}, _
{  " XXXX ",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   " XXXX ",  _
   "      "}, _
{  "XXXXX ",  _
   "XX  XX",  _
   "XX  XX",  _
   "XXXXX ",  _
   "XX    ",  _
   "XX    ",  _
   "XX    ",  _
   "      "}, _
{  " XXXX ",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XXXXXX",  _
   " XXXX ",  _
   "    XX"}, _
{  "XXXXX ",  _
   "XX  XX",  _
   "XX  XX",  _
   "XXXXX ",  _
   "XX XX ",  _
   "XX  XX",  _
   "XX  XX",  _
   "      "}, _
{  " XXXX ",  _
   "XX  XX",  _
   "XX    ",  _
   " XXXX ",  _
   "    XX",  _
   "XX  XX",  _
   " XXXX ",  _
   "      "}, _
{  "XXXXXX",  _
   "  XX  ",  _
   "  XX  ",  _
   "  XX  ",  _
   "  XX  ",  _
   "  XX  ",  _
   "  XX  ",  _
   "      "}, _
{  "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   " XXXX ",  _
   "      "}, _
{  "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   " XXXX ",  _
   " XXXX ",  _
   "  XX  ",  _
   "  XX  ",  _
   "      "}, _
{"XX    XX",  _
 "XX    XX",  _
 "XX    XX",  _
 "XX XX XX",  _
 "XX XX XX",  _
 "XXXXXXXX",  _
 " XX  XX ",  _
 "        "}, _
{  "XX  XX",  _
   "XX  XX",  _
   " XXXX ",  _
   "  XX  ",  _
   " XXXX ",  _
   "XX  XX",  _
   "XX  XX",  _
   "      "}, _
{  "XX  XX",  _
   "XX  XX",  _
   " XXXX ",  _
   "  XX  ",  _
   "  XX  ",  _
   "  XX  ",  _
   "  XX  ",  _
   "      "}, _
{  "XXXXXX",  _
   "    XX",  _
   "   XX ",  _
   "  XX  ",  _
   " XX   ",  _
   "XX    ",  _
   "XXXXXX",  _
   "      "}, _
{    "XXXX",  _
     "XX  ",  _
     "XX  ",  _
     "XX  ",  _
     "XX  ",  _
     "XX  ",  _
     "XXXX",  _
     "    "}, _
{"XX      ",  _
 " XX     ",  _
 "  XX    ",  _
 "   XX   ",  _
 "    XX  ",  _
 "     XX ",  _
 "      XX",  _
 "        "}, _
{    "XXXX",  _
     "  XX",  _
     "  XX",  _
     "  XX",  _
     "  XX",  _
     "  XX",  _
     "XXXX",  _
     "    "}, _
{  "  XX  ",  _
   " XXXX ",  _
   "XX  XX",  _
   "      ",  _
   "      ",  _
   "      ",  _
   "      ",  _
   "      "}, _
{  "      ",  _
   "      ",  _
   "      ",  _
   "      ",  _
   "      ",  _
   "      ",  _
   "XXXXXX",  _
   "      "}, _
{  " XX   ",  _
   "  XX  ",  _
   "   XX ",  _
   "      ",  _
   "      ",  _
   "      ",  _
   "      ",  _
   "      "}, _
{  "      ",  _
   "      ",  _
   " XXXX ",  _
   "    XX",  _
   " XXXXX",  _
   "XX  XX",  _
   " XXXXX",  _
   "      "}, _
{  "XX    ",  _
   "XX    ",  _
   "XXXXX ",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XXXXX ",  _
   "      "}, _
{  "      ",  _
   "      ",  _
   " XXXXX",  _
   "XX    ",  _
   "XX    ",  _
   "XX    ",  _
   " XXXXX",  _
   "      "}, _
{  "    XX",  _
   "    XX",  _
   " XXXXX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   " XXXXX",  _
   "      "}, _
{  "      ",  _
   "      ",  _
   " XXXX ",  _
   "XX  XX",  _
   "XXXXXX",  _
   "XX    ",  _
   " XXXX ",  _
   "      "}, _
{  "  XXXX",  _
   " XX   ",  _
   " XX   ",  _
   "XXXXXX",  _
   " XX   ",  _
   " XX   ",  _
   " XX   ",  _
   "      "}, _
{  "      ",  _
   "      ",  _
   " XXXXX",  _
   "XX  XX",  _
   "XX  XX",  _
   " XXXXX",  _
   "    XX",  _
   " XXXX "}, _
{  "XX    ",  _
   "XX    ",  _
   "XXXXX ",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "      "}, _
{    " XX ",  _
     "    ",  _
     "XXX ",  _
     " XX ",  _
     " XX ",  _
     " XX ",  _
     "XXXX",  _
     "    "}, _
{    "  XX",  _
     "    ",  _
     " XXX",  _
     "  XX",  _
     "  XX",  _
     "  XX",  _
     "  XX",  _
     "XXX "}, _
{  "XX    ",  _
   "XX    ",  _
   "XX  XX",  _
   "XX XX ",  _
   "XXXX  ",  _
   "XXXXX ",  _
   "XX  XX",  _
   "      "}, _
{  "XXX   ",  _
   " XX   ",  _
   " XX   ",  _
   " XX   ",  _
   " XX   ",  _
   " XX   ",  _
   "  XXX ",  _
   "      "}, _
{"        ",  _
 "        ",  _
 "XXXXXXX ",  _
 "XX XX XX",  _
 "XX XX XX",  _
 "XX XX XX",  _
 "XX XX XX",  _
 "        "}, _
{  "      ",  _
   "      ",  _
   "XXXXX ",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "      "}, _
{  "      ",  _
   "      ",  _
   " XXXX ",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   " XXXX ",  _
   "      "}, _
{  "      ",  _
   "      ",  _
   "XXXXX ",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XXXXX ",  _
   "XX    "}, _
{  "      ",  _
   "      ",  _
   " XXXXX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   " XXXXX",  _
   "    XX"}, _
{  "      ",  _
   "      ",  _
   "XXXXX ",  _
   "XX  XX",  _
   "XX    ",  _
   "XX    ",  _
   "XX    ",  _
   "      "}, _
{  "      ",  _
   "      ",  _
   " XXXXX",  _
   "XX    ",  _
   " XXXX ",  _
   "    XX",  _
   "XXXXX ",  _
   "      "}, _
{  " XX   ",  _
   " XX   ",  _
   "XXXXX ",  _
   " XX   ",  _
   " XX   ",  _
   " XX   ",  _
   "  XXX ",  _
   "      "}, _
{  "      ",  _
   "      ",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   " XXXXX",  _
   "      "}, _
{  "      ",  _
   "      ",  _
   "XX  XX",  _
   "XX  XX",  _
   " XXXX ",  _
   " XXXX ",  _
   "  XX  ",  _
   "      "}, _
{"        ",  _
 "        ",  _
 "XX    XX",  _
 "XX XX XX",  _
 "XX XX XX",  _
 "XX XX XX",  _
 " XXXXXX ",  _
 "        "}, _
{  "      ",  _
   "      ",  _
   "XX  XX",  _
   " XXXX ",  _
   "  XX  ",  _
   " XXXX ",  _
   "XX  XX",  _
   "      "}, _
{  "      ",  _
   "      ",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   " XXXXX",  _
   "    XX",  _
   "XXXXX "}, _
{  "      ",  _
   "      ",  _
   "XXXXXX",  _
   "   XX ",  _
   "  XX  ",  _
   " XX   ",  _
   "XXXXXX",  _
   "      "}, _
{   "   XX",  _
    "  XX ",  _
    "  XX ",  _
    " XX  ",  _
    "  XX ",  _
    "  XX ",  _
    "   XX",  _
    "     "}, _
{      "XX",  _
       "XX",  _
       "XX",  _
       "XX",  _
       "XX",  _
       "XX",  _
       "XX",  _
       "XX"}, _
{   "XXX  ",  _
    "  XX ",  _
    "  XX ",  _
    "   XX",  _
    "  XX ",  _
    "  XX ",  _
    "XXX  ",  _
    "     "}, _
{ " XXX XX",  _
  "XX XXX ",  _
  "       ",  _
  "       ",  _
  "       ",  _
  "       ",  _
  "       ",  _
  "       "}, _
{       " ",  _
        " ",  _
        " ",  _
        " ",  _
        " ",  _
        " ",  _
        " ",  _
        "  "} _
}

' Terminal Font
' Block text character raster pattern definitions
' First ASCII char0: 32, last ASCII char = 127 (total=96)

  Dim TerminalFont (,) As String = New String(0 To 95, 0 To 7) { _
{      "  ",  _
       "  ",  _
       "  ",  _
       "  ",  _
       "  ",  _
       "  ",  _
       "  ",  _
       "  "}, _
{     " # ",  _
      "###",  _
      "###",  _
      " # ",  _
      " # ",  _
      "   ",  _
      " # ",  _
      "   "}, _
{     "# #",  _
      "# #",  _
      "   ",  _
      "   ",  _
      "   ",  _
      "   ",  _
      "   ",  _
      "   "}, _
{   "     ",  _
    " # # ",  _
    "#####",  _
    " # # ",  _
    " # # ",  _
    "#####",  _
    " # # ",  _
    "     "}, _
{   "  #  ",  _
    " ####",  _
    "# #  ",  _
    " ### ",  _
    "  # #",  _
    "#### ",  _
    "  #  ",  _
    "     "}, _
{   "##   ",  _
    "##  #",  _
    "   # ",  _
    "  #  ",  _
    " #   ",  _
    "#  ##",  _
    "   ##",  _
    "     "}, _
{   " #   ",  _
    "# #  ",  _
    "# #  ",  _
    " #   ",  _
    "# # #",  _
    "#  # ",  _
    " ## #",  _
    "     "}, _
{       "#",  _
        "#",  _
        " ",  _
        " ",  _
        " ",  _
        " ",  _
        " ",  _
        " "}, _
{      " #",  _
       "# ",  _
       "# ",  _
       "# ",  _
       "# ",  _
       "# ",  _
       " #",  _
       "  "}, _
{      "# ",  _
       " #",  _
       " #",  _
       " #",  _
       " #",  _
       " #",  _
       "# ",  _
       "  "}, _
{   "     ",  _
    " # # ",  _
    " ### ",  _
    "#####",  _
    " ### ",  _
    " # # ",  _
    "     ",  _
    "     "}, _
{   "     ",  _
    "  #  ",  _
    "  #  ",  _
    "#####",  _
    "  #  ",  _
    "  #  ",  _
    "     ",  _
    "     "}, _
{      "  ",  _
       "  ",  _
       "  ",  _
       "  ",  _
       "  ",  _
       "  ",  _
       " #",  _
       "# "}, _
{   "     ",  _
    "     ",  _
    "     ",  _
    "#####",  _
    "     ",  _
    "     ",  _
    "     ",  _
    "     "}, _
{       " ",  _
        " ",  _
        " ",  _
        " ",  _
        " ",  _
        " ",  _
        "#",  _
        " "}, _
{   "     ",  _
    "    #",  _
    "   # ",  _
    "  #  ",  _
    " #   ",  _
    "#    ",  _
    "     ",  _
    "     "}, _
{   " ### ",  _
    "#   #",  _
    "#  ##",  _
    "# # #",  _
    "##  #",  _
    "#   #",  _
    " ### ",  _
    "     "}, _
{    "  # ",  _
     " ## ",  _
     "  # ",  _
     "  # ",  _
     "  # ",  _
     "  # ",  _
     " ###",  _
     "    "}, _
{   " ### ",  _
    "#   #",  _
    "    #",  _
    "   # ",  _
    "  #  ",  _
    " #   ",  _
    "#####",  _
    "     "}, _
{   " ### ",  _
    "#   #",  _
    "    #",  _
    " ### ",  _
    "    #",  _
    "#   #",  _
    " ### ",  _
    "     "}, _
{   "   # ",  _
    "  ## ",  _
    " # # ",  _
    "#  # ",  _
    "#####",  _
    "   # ",  _
    "   # ",  _
    "     "}, _
{   "#####",  _
    "#    ",  _
    "#    ",  _
    "#### ",  _
    "    #",  _
    "#   #",  _
    " ### ",  _
    "     "}, _
{   "  ## ",  _
    " #   ",  _
    "#    ",  _
    "#### ",  _
    "#   #",  _
    "#   #",  _
    " ### ",  _
    "     "}, _
{   "#####",  _
    "    #",  _
    "   # ",  _
    "  #  ",  _
    " #   ",  _
    " #   ",  _
    " #   ",  _
    "     "}, _
{   " ### ",  _
    "#   #",  _
    "#   #",  _
    " ### ",  _
    "#   #",  _
    "#   #",  _
    " ### ",  _
    "     "}, _
{   " ### ",  _
    "#   #",  _
    "#   #",  _
    " ####",  _
    "    #",  _
    "   # ",  _
    " ##  ",  _
    "     "}, _
{       " ",  _
        " ",  _
        " ",  _
        "#",  _
        " ",  _
        " ",  _
        "#",  _
        " "}, _
{      "  ",  _
       "  ",  _
       "  ",  _
       " #",  _
       "  ",  _
       "  ",  _
       " #",  _
       "# "}, _
{    "   #",  _
     "  # ",  _
     " #  ",  _
     "#   ",  _
     " #  ",  _
     "  # ",  _
     "   #",  _
     "    "}, _
{   "     ",  _
    "     ",  _
    "#####",  _
    "     ",  _
    "#####",  _
    "     ",  _
    "     ",  _
    "     "}, _
{    "#   ",  _
     " #  ",  _
     "  # ",  _
     "   #",  _
     "  # ",  _
     " #  ",  _
     "#   ",  _
     "    "}, _
{   " ### ",  _
    "#   #",  _
    "    #",  _
    "  ## ",  _
    "  #  ",  _
    "     ",  _
    "  #  ",  _
    "     "}, _
{   " ### ",  _
    "#   #",  _
    "# ###",  _
    "# # #",  _
    "# ###",  _
    "#    ",  _
    " ### ",  _
    "     "}, _
{   " ### ",  _
    "#   #",  _
    "#   #",  _
    "#####",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    "     "}, _
{   "#### ",  _
    "#   #",  _
    "#   #",  _
    "#### ",  _
    "#   #",  _
    "#   #",  _
    "#### ",  _
    "     "}, _
{   " ### ",  _
    "#   #",  _
    "#    ",  _
    "#    ",  _
    "#    ",  _
    "#   #",  _
    " ### ",  _
    "     "}, _
{   "#### ",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    "#### ",  _
    "     "}, _
{   "#####",  _
    "#    ",  _
    "#    ",  _
    "#### ",  _
    "#    ",  _
    "#    ",  _
    "#####",  _
    "     "}, _
{   "#####",  _
    "#    ",  _
    "#    ",  _
    "#### ",  _
    "#    ",  _
    "#    ",  _
    "#    ",  _
    "     "}, _
{   " ### ",  _
    "#   #",  _
    "#    ",  _
    "# ###",  _
    "#   #",  _
    "#   #",  _
    " ### ",  _
    "     "}, _
{   "#   #",  _
    "#   #",  _
    "#   #",  _
    "#####",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    "     "}, _
{   " ### ",  _
    "  #  ",  _
    "  #  ",  _
    "  #  ",  _
    "  #  ",  _
    "  #  ",  _
    " ### ",  _
    "     "}, _
{   "    #",  _
    "    #",  _
    "    #",  _
    "    #",  _
    "#   #",  _
    "#   #",  _
    " ### ",  _
    "     "}, _
{   "#   #",  _
    "#  # ",  _
    "# #  ",  _
    "##   ",  _
    "# #  ",  _
    "#  # ",  _
    "#   #",  _
    "     "}, _
{   "#    ",  _
    "#    ",  _
    "#    ",  _
    "#    ",  _
    "#    ",  _
    "#    ",  _
    "#####",  _
    "     "}, _
{   "#   #",  _
    "## ##",  _
    "# # #",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    "     "}, _
{   "#   #",  _
    "##  #",  _
    "# # #",  _
    "#  ##",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    "     "}, _
{   " ### ",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    " ### ",  _
    "     "}, _
{   "#### ",  _
    "#   #",  _
    "#   #",  _
    "#### ",  _
    "#    ",  _
    "#    ",  _
    "#    ",  _
    "     "}, _
{   " ### ",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    "# # #",  _
    "#  # ",  _
    " ## #",  _
     "     "}, _
{   "#### ",  _
    "#   #",  _
    "#   #",  _
    "#### ",  _
    "#  # ",  _
    "#   #",  _
    "#   #",  _
    "     "}, _
{   " ### ",  _
    "#   #",  _
    "#    ",  _
    " ### ",  _
    "    #",  _
    "#   #",  _
    " ### ",  _
    "     "}, _
{   "#####",  _
    "  #  ",  _
    "  #  ",  _
    "  #  ",  _
    "  #  ",  _
    "  #  ",  _
    "  #  ",  _
    "     "}, _
{   "#   #",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    " ### ",  _
    "     "}, _
{   "#   #",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    " # # ",  _
    "  #  ",  _
    "     "}, _
{   "#   #",  _
    "#   #",  _
    "# # #",  _
    "# # #",  _
    "# # #",  _
    "# # #",  _
    " # # ",  _
    "     "}, _
{   "#   #",  _
    "#   #",  _
    " # # ",  _
    "  #  ",  _
    " # # ",  _
    "#   #",  _
    "#   #",  _
    "     "}, _
{   "#   #",  _
    "#   #",  _
    "#   #",  _
    " # # ",  _
    "  #  ",  _
    "  #  ",  _
    "  #  ",  _
    "     "}, _
{   "#### ",  _
    "   # ",  _
    "  #  ",  _
    " #   ",  _
    "#    ",  _
    "#    ",  _
    "#### ",  _
    "     "}, _
{    " ###",  _
     " #  ",  _
     " #  ",  _
     " #  ",  _
     " #  ",  _
     " #  ",  _
     " ###",  _
     "    "}, _
{   "     ",  _
    "#    ",  _
    " #   ",  _
    "  #  ",  _
    "   # ",  _
    "    #",  _
    "     ",  _
    "     "}, _
{    " ###",  _
     "   #",  _
     "   #",  _
     "   #",  _
     "   #",  _
     "   #",  _
     " ###",  _
     "    "}, _
{   "  #  ",  _
    " # # ",  _
    "#   #",  _
    "     ",  _
    "     ",  _
    "     ",  _
    "     ",  _
    "     "}, _
{   "     ",  _
    "     ",  _
    "     ",  _
    "     ",  _
    "     ",  _
    "     ",  _
    "     ",  _
   "######"}, _
{     "## ",  _
      "## ",  _
      "  #",  _
      "   ",  _
      "   ",  _
      "   ",  _
      "   ",  _
      "   "}, _
{   "     ",  _
    "     ",  _
    " ### ",  _
    "    #",  _
    " ####",  _
    "#   #",  _
    " ####",  _
    "     "}, _
{   "#    ",  _
    "#    ",  _
    "#### ",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    "#### ",  _
    "     "}, _
{   "     ",  _
    "     ",  _
    " ### ",  _
    "#   #",  _
    "#    ",  _
    "#   #",  _
    " ### ",  _
    "     "}, _
{   "    #",  _
    "    #",  _
    " ####",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    " ####",  _
    "     "}, _
{   "     ",  _
    "     ",  _
    " ### ",  _
    "#   #",  _
    "#####",  _
    "#    ",  _
    " ### ",  _
    "     "}, _
{    "  ##",  _
     " #  ",  _
     "####",  _
     " #  ",  _
     " #  ",  _
     " #  ",  _
     " #  ",  _
     "    "}, _
{   "     ",  _
    "     ",  _
    " ### ",  _
    "#   #",  _
    "#   #",  _
    " ####",  _
    "    #",  _
    " ### "}, _
{   "#    ",  _
    "#    ",  _
    "#### ",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    "     "}, _
{       "#",  _
        " ",  _
        "#",  _
        "#",  _
        "#",  _
        "#",  _
        "#",  _
        " "}, _
{    "   #",  _
     "    ",  _
     "   #",  _
     "   #",  _
     "   #",  _
     "   #",  _
     "#  #",  _
     " ## "}, _
{    "#   ",  _
     "#   ",  _
     "#  #",  _
     "# # ",  _
     "##  ",  _
     "# # ",  _
     "#  #",  _
     "    "}, _
{       "#",  _
        "#",  _
        "#",  _
        "#",  _
        "#",  _
        "#",  _
        "#",  _
        " "}, _
{   "     ",  _
    "     ",  _
    "## # ",  _
    "# # #",  _
    "# # #",  _
    "#   #",  _
    "#   #",  _
    "     "}, _
{   "     ",  _
    "     ",  _
    "#### ",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    "     "}, _
{   "     ",  _
    "     ",  _
    " ### ",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    " ### ",  _
    "     "}, _
{   "     ",  _
    "     ",  _
    "#### ",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    "#### ",  _
    "#    "}, _
{   "     ",  _
    "     ",  _
    " ####",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    " ####",  _
    "    #"}, _
{   "     ",  _
    "     ",  _
    "# ## ",  _
    "##  #",  _
    "#    ",  _
    "#    ",  _
    "#    ",  _
    "     "}, _
{   "     ",  _
    "     ",  _
    " ### ",  _
    "#    ",  _
    " ### ",  _
    "    #",  _
    " ### ",  _
    "     "}, _
{    "    ",  _
     " #  ",  _
     "####",  _
     " #  ",  _
     " #  ",  _
     " #  ",  _
     "  ##",  _
     "    "}, _
{   "     ",  _
    "     ",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    " ### ",  _
    "     "}, _
{   "     ",  _
    "     ",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    " # # ",  _
    "  #  ",  _
    "     "}, _
{   "     ",  _
    "     ",  _
    "#   #",  _
    "#   #",  _
    "# # #",  _
    "#####",  _
    " # # ",  _
    "     "}, _
{   "     ",  _
    "     ",  _
    "#   #",  _
    " # # ",  _
    "  #  ",  _
    " # # ",  _
    "#   #",  _
    "     "}, _
{   "     ",  _
    "     ",  _
    "#   #",  _
    "#   #",  _
    "#   #",  _
    " ####",  _
    "    #",  _
    " ### "}, _
{   "     ",  _
    "     ",  _
    "#####",  _
    "   # ",  _
    "  #  ",  _
    " #   ",  _
    "#####",  _
    "     "}, _
{    "  ##",  _
     " #  ",  _
     " #  ",  _
     "##  ",  _
     " #  ",  _
     " #  ",  _
     "  ##",  _
     "    "}, _
 {      "#",  _
        "#",  _
        "#",  _
        " ",  _
        "#",  _
        "#",  _
        "#",  _
        " "}, _
{    "##  ",  _
     "  # ",  _
     "  # ",  _
     "  ##",  _
     "  # ",  _
     "  # ",  _
     "##  ",  _
     "    "}, _
{    " # #",  _
     "# # ",  _
     "    ",  _
     "    ",  _
     "    ",  _
     "    ",  _
     "    ",  _
    "    "},  _
{   "  #  ",  _
    " ### ",  _
    "## ##",  _
    "#   #",  _
    "#   #",  _
    "#####",  _
    "     ",  _
    "     "}  _
 }

' Ozone Font
' Block text character raster pattern definitions
' First ASCII char0: 32, last ASCII char = 127 (total=96)

Dim OzoneFont (,) As String = New String(0 To 95, 0 To 7) { _
{     "   ",  _
      "   ",  _
      "   ",  _
      "   ",  _
      "   ",  _
      "   ",  _
      "   ",  _
      "   "}, _
{      "XX",  _
       "XX",  _
       "XX",  _
       "XX",  _
       "XX",  _
       "  ",  _
       "XX",  _
       "XX"}, _
{   "XX XX",  _
    "XX XX",  _
    "     ",  _
    "     ",  _
    "     ",  _
    "     ",  _
    "     ",  _
    "     "}, _
{ "       ",  _
  "       ",  _
  " XX XX ",  _
  "XXXXXXX",  _
  " XX XX ",  _
  "XXXXXXX",  _
  " XX XX ",  _
  "       "}, _
{  "  XX  ",  _
   " XXXX ",  _
   "XX  XX",  _
   "XXXX  ",  _
   "  XXXX",  _
   "XX  XX",  _
   " XXXX ",  _
   "  XX  "}, _
{  "XX  XX",  _
   "XX  XX",  _
   "   XX ",  _
   "  XX  ",  _
   "  XX  ",  _
   " XX   ",  _
   "XX  XX",  _
   "XX  XX"}, _
{ " XXX   ",  _
  "XXXXX  ",  _
  "XX XX  ",  _
  " XXX XX",  _
  " XXXXX ",  _
  "XX XX  ",  _
  "XXXXXX ",  _
  "XXXX XX"}, _
{       " ",  _
        "X",  _
        "X",  _
        " ",  _
        " ",  _
        " ",  _
        " ",  _
        " "}, _
{    "  XX",  _
     " XX ",  _
     "XX  ",  _
     "XX  ",  _
     "XX  ",  _
     "XX  ",  _
     " XX ",  _
     "  XX"}, _
{    "XX  ",  _
     " XX ",  _
     "  XX",  _
     "  XX",  _
     "  XX",  _
     "  XX",  _
     " XX ",  _
     "XX  "}, _
{   "     ",  _
    "  X  ",  _
    "X X X",  _
    " XXX ",  _
    "X X X",  _
    "  X  ",  _
    "     ",  _
    "     "}, _
{  "      ",  _
   "  XX  ",  _
   "  XX  ",  _
   "XXXXXX",  _
   "XXXXXX",  _
   "  XX  ",  _
   "  XX  ",  _
   "      "}, _
{      "  ",  _
       "  ",  _
       "  ",  _
       "  ",  _
       "  ",  _
       "XX",  _
       "XX",  _
       "X "}, _
{    "    ",  _
     "    ",  _
     "    ",  _
     "XXXX",  _
     "XXXX",  _
     "    ",  _
     "    ",  _
     "    "}, _
{      "  ",  _
       "  ",  _
       "  ",  _
       "  ",  _
       "  ",  _
       "  ",  _
       "XX",  _
       "XX"}, _
{   "   XX",  _
    "   XX",  _
    "  XX ",  _
    "  XX ",  _
    " XX  ",  _
    " XX  ",  _
    "XX   ",  _
    "XX   "}, _
{  " XXXX ",  _
   "XXXXXX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XXXXXX",  _
   " XXXX "}, _
{    " XX ",  _
     "XXX ",  _
     " XX ",  _
     " XX ",  _
     " XX ",  _
     " XX ",  _
     "XXXX",  _
     "XXXX"}, _
{  "XXXXX ",  _
   "XXXXXX",  _
   "    XX",  _
   "  XXX ",  _
   " XXX  ",  _
   "XX    ",  _
   "XXXXXX",  _
   "XXXXXX"}, _
{  "XXXXX ",  _
   "XXXXXX",  _
   "    XX",  _
   "XXXXX ",  _
   "XXXXX ",  _
   "    XX",  _
   "XXXXXX",  _
   "XXXXX "}, _
{  "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XXXXXX",  _
   " XXXXX",  _
   "    XX",  _
   "    XX",  _
   "    XX"}, _
{  "XXXXXX",  _
   "XXXXXX",  _
   "XX    ",  _
   "XXXXX ",  _
   "XXXXXX",  _
   "    XX",  _
   "XXXXXX",  _
   "XXXXX "}, _
{  " XXX  ",  _
   "XXXX  ",  _
   "XX    ",  _
   "XXXXX ",  _
   "XXXXXX",  _
   "XX  XX",  _
   "XXXXXX",  _
   " XXXX "}, _
{  "XXXXXX",  _
   "XXXXXX",  _
   "    XX",  _
   "    XX",  _
   "   XX ",  _
   "   XX ",  _
   "   XX ",  _
   "   XX "}, _
{  " XXXX ",  _
   "XXXXXX",  _
   "XX  XX",  _
   " XXXX ",  _
   " XXXX ",  _
   "XX  XX",  _
   "XXXXXX",  _
   " XXXX "}, _
{  " XXXX ",  _
   "XXXXXX",  _
   "XX  XX",  _
   "XXXXXX",  _
   " XXXXX",  _
   "    XX",  _
   "    XX",  _
   "    XX"}, _
{      "  ",  _
       "  ",  _
       "  ",  _
       "XX",  _
       "XX",  _
       "  ",  _
       "XX",  _
       "XX"}, _
{      "  ",  _
       "  ",  _
       "  ",  _
       "XX",  _
       "XX",  _
       "  ",  _
       "XX",  _
       "X "}, _
{   "   XX",  _
    "  XX ",  _
    " XX  ",  _
    "XX   ",  _
    "XX   ",  _
    " XX  ",  _
    "  XX ",  _
    "   XX"}, _
{    "    ",  _
     "XXXX",  _
     "XXXX",  _
     "    ",  _
     "    ",  _
     "XXXX",  _
     "XXXX",  _
     "    "}, _
{   "XX   ",  _
    " XX  ",  _
    "  XX ",  _
    "   XX",  _
    "   XX",  _
    "  XX ",  _
    " XX  ",  _
    "XX   "}, _
{  " XXXX ",  _
   "XXXXXX",  _
   "X   XX",  _
   "  XXX ",  _
   "  XX  ",  _
   "      ",  _
   "  XX  ",  _
   "  XX  "}, _
{" XXXXXX ",  _
 "X      X",  _
 "X  XXX X",  _
 "X X  X X",  _
 "X X  X X",  _
 "X  XXXXX",  _
 "X       ",  _
 " XXXXX  "}, _
{  " XXXX ",  _
   "XXXXXX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XXXXXX",  _
   "XXXXXX",  _
   "XX  XX",  _
   "XX  XX"}, _
{  "XXXXX ",  _
   "XXXXXX",  _
   "XX  XX",  _
   "XXXXX ",  _
   "XXXXX ",  _
   "XX  XX",  _
   "XXXXXX",  _
   "XXXXX "}, _
{  " XXXXX",  _
   "XXXXXX",  _
   "XX    ",  _
   "XX    ",  _
   "XX    ",  _
   "XX    ",  _
   "XXXXXX",  _
   " XXXXX"}, _
{  "XXXXX ",  _
   "XXXXXX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XXXXXX",  _
   "XXXXX "}, _
{  "XXXXXX",  _
   "XXXXXX",  _
   "XX    ",  _
   "XXXXXX",  _
   "XXXXXX",  _
   "XX    ",  _
   "XXXXXX",  _
   "XXXXXX"}, _
{  "XXXXXX",  _
   "XXXXXX",  _
   "XX    ",  _
   "XXXXXX",  _
   "XXXXXX",  _
   "XX    ",  _
   "XX    ",  _
   "XX    "}, _
{  " XXXX ",  _
   "XXXXXX",  _
   "XX  XX",  _
   "XX    ",  _
   "XX XXX",  _
   "XX  XX",  _
   "XXXXXX",  _
   " XXXX "}, _
{  "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XXXXXX",  _
   "XXXXXX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX"}, _
{      "XX",  _
       "XX",  _
       "XX",  _
       "XX",  _
       "XX",  _
       "XX",  _
       "XX",  _
       "XX"}, _
{  "    XX",  _
   "    XX",  _
   "    XX",  _
   "    XX",  _
   "    XX",  _
   "XX  XX",  _
   "XXXXXX",  _
   " XXXX "}, _
{  "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XXXXX ",  _
   "XXXXX ",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX"}, _
{  "XX    ",  _
   "XX    ",  _
   "XX    ",  _
   "XX    ",  _
   "XX    ",  _
   "XX    ",  _
   "XXXXXX",  _
   "XXXXXX"}, _
{"XX    XX",  _
 "XXX  XXX",  _
 "XXXXXXXX",  _
 "XX XX XX",  _
 "XX    XX",  _
 "XX    XX",  _
 "XX    XX",  _
 "XX    XX"}, _
{  "XX  XX",  _
   "XX  XX",  _
   "XXX XX",  _
   "XXXXXX",  _
   "XX XXX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX"}, _
{  " XXXX ",  _
   "XXXXXX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XXXXXX",  _
   " XXXX "}, _
{  "XXXXX ",  _
   "XXXXXX",  _
   "XX  XX",  _
   "XXXXXX",  _
   "XXXXX ",  _
   "XX    ",  _
   "XX    ",  _
   "XX    "}, _
{  " XXXX ",  _
   "XXXXXX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX XXX",  _
   "XXXXXX",  _
   " XXXXX"}, _
{  "XXXXX ",  _
   "XXXXXX",  _
   "XX  XX",  _
   "XXXXXX",  _
   "XXXXX ",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX"}, _
{  " XXXXX",  _
   "XXXXXX",  _
   "XX    ",  _
   "XXXXX ",  _
   " XXXXX",  _
   "    XX",  _
   "XXXXXX",  _
   "XXXXX "}, _
{  "XXXXXX",  _
   "XXXXXX",  _
   "  XX  ",  _
   "  XX  ",  _
   "  XX  ",  _
   "  XX  ",  _
   "  XX  ",  _
   "  XX  "}, _
{  "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XXXXXX",  _
   " XXXX "}, _
{  "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XXXXXX",  _
   " XXXX ",  _
   "  XX  "}, _
{"XX    XX",  _
 "XX    XX",  _
 "XX    XX",  _
 "XX    XX",  _
 "XX XX XX",  _
 "XXXXXXXX",  _
 "XXX  XXX",  _
 "XX    XX"}, _
{  "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   " XXXX ",  _
   " XXXX ",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX"}, _
{  "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XXXXXX",  _
   " XXXX ",  _
   "  XX  ",  _
   "  XX  ",  _
   "  XX  "}, _
{  "XXXXXX",  _
   "XXXXXX",  _
   "   XX ",  _
   "  XX  ",  _
   " XX   ",  _
   "XX    ",  _
   "XXXXXX",  _
   "XXXXXX"}, _
{    "XXXX",  _
     "XXXX",  _
     "XX  ",  _
     "XX  ",  _
     "XX  ",  _
     "XX  ",  _
     "XXXX",  _
     "XXXX"}, _
{   "XX   ",  _
    "XX   ",  _
    " XX  ",  _
    " XX  ",  _
    "  XX ",  _
    "  XX ",  _
    "   XX",  _
    "   XX"}, _
{    "XXXX",  _
     "XXXX",  _
     "  XX",  _
     "  XX",  _
     "  XX",  _
     "  XX",  _
     "XXXX",  _
     "XXXX"}, _
{   "  X  ",  _
    " XXX ",  _
    "XX XX",  _
    "     ",  _
    "     ",  _
    "     ",  _
    "     ",  _
    "     "}, _
{  "      ",  _
   "      ",  _
   "      ",  _
   "      ",  _
   "      ",  _
   "      ",  _
   "XXXXXX",  _
   "XXXXXX"}, _
{       "X",  _
        "X",  _
        " ",  _
        " ",  _
        " ",  _
        " ",  _
        " ",  _
        " "}, _
{  "XXXXX ",  _
   "XXXXXX",  _
   "    XX",  _
   " XXXXX",  _
   "XXXXXX",  _
   "XX  XX",  _
   "XXXXXX",  _
   " XXXXX"}, _
{  "XXXXX ",  _
   "XXXXXX",  _
   "XX  XX",  _
   "XXXXX ",  _
   "XXXXX ",  _
   "XX  XX",  _
   "XXXXXX",  _
   "XXXXX "}, _
{  " XXXX ",  _
   "XXXXXX",  _
   "XX  XX",  _
   "XX    ",  _
   "XX    ",  _
   "XX  XX",  _
   "XXXXXX",  _
   " XXXX "}, _
{  "XXXXX ",  _
   "XXXXXX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XXXXXX",  _
   "XXXXX "}, _
{  " XXXX ",  _
   "XXXXXX",  _
   "XX  XX",  _
   "XXXXXX",  _
   "XXXXXX",  _
   "XX    ",  _
   "XXXXXX",  _
   " XXXXX"}, _
{  " XXXXX",  _
   "XXXXXX",  _
   "XX    ",  _
   "XXXXX ",  _
   "XXXXX ",  _
   "XX    ",  _
   "XX    ",  _
   "XX    "}, _
{  " XXXX ",  _
   "XXXXXX",  _
   "XX  XX",  _
   "XXXXXX",  _
   " XXXXX",  _
   "    XX",  _
   "XXXXXX",  _
   "XXXXX "}, _
{  "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XXXXXX",  _
   "XXXXXX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX"}, _
{      "XX",  _
       "XX",  _
       "  ",  _
       "XX",  _
       "XX",  _
       "XX",  _
       "XX",  _
       "XX"}, _
{  "    XX",  _
   "    XX",  _
   "    XX",  _
   "    XX",  _
   "    XX",  _
   "XX  XX",  _
   "XXXXXX",  _
   " XXXX "}, _
{  "XX  XX",  _
   "XX  XX",  _
   "XX XX ",  _
   "XXXX  ",  _
   "XXXX  ",  _
   "XX XX ",  _
   "XX  XX",  _
   "XX  XX"}, _
{  "XX    ",  _
   "XX    ",  _
   "XX    ",  _
   "XX    ",  _
   "XX    ",  _
   "XX    ",  _
   "XXXXXX",  _
   " XXXXX"}, _
{" XX  XX ",  _
 "XXXXXXXX",  _
 "XX XX XX",  _
 "XX XX XX",  _
 "XX XX XX",  _
 "XX XX XX",  _
 "XX XX XX",  _
 "XX XX XX"}, _
{  " XXXX ",  _
   "XXXXXX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX"}, _
{  " XXXX ",  _
   "XXXXXX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XXXXXX",  _
   " XXXX "}, _
{  "XXXXX ",  _
   "XXXXXX",  _
   "XX  XX",  _
   "XXXXXX",  _
   "XXXXX ",  _
   "XX    ",  _
   "XX    ",  _
   "XX    "}, _
{  " XXXX ",  _
   "XXXXXX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX XXX",  _
   "XXXXXX",  _
   " XXXXX"}, _
{  " XXXX ",  _
   "XXXXXX",  _
   "XX  XX",  _
   "XX    ",  _
   "XX    ",  _
   "XX    ",  _
   "XX    ",  _
   "XX    "}, _
{  " XXXXX",  _
   "XXXXXX",  _
   "XX    ",  _
   "XXXXX ",  _
   " XXXXX",  _
   "    XX",  _
   "XXXXXX",  _
   "XXXXX "}, _
{  "XX    ",  _
   "XXXXXX",  _
   "XXXXXX",  _
   "XX    ",  _
   "XX    ",  _
   "XX  XX",  _
   "XXXXXX",  _
   " XXXX "}, _
{  "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XXXXXX",  _
   " XXXX "}, _
{  "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XXXXXX",  _
   " XXXX ",  _
   "  XX  "}, _
{"XX XX XX",  _
 "XX XX XX",  _
 "XX XX XX",  _
 "XX XX XX",  _
 "XX XX XX",  _
 "XX XX XX",  _
 "XXXXXXXX",  _
 " XX  XX "}, _
{  "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   " XXXX ",  _
   " XXXX ",  _
   "XX  XX",  _
   "XX  XX",  _
   "XX  XX"}, _
{  "XX  XX",  _
   "XX  XX",  _
   "XX  XX",  _
   "XXXXXX",  _
   " XXXXX",  _
   "    XX",  _
   "XXXXXX",  _
   "XXXXX "}, _
{  "XXXXXX",  _
   "XXXXXX",  _
   "   XX ",  _
   "  XX  ",  _
   " XX   ",  _
   "XX    ",  _
   "XXXXXX",  _
   "XXXXXX"}, _
{    "  XX",  _
     " XXX",  _
     " XX ",  _
     "XXX ",  _
     "XXX ",  _
     " XX ",  _
     " XXX",  _
     " XXX"}, _
{      "XX",  _
       "XX",  _
       "XX",  _
       "XX",  _
       "XX",  _
       "XX",  _
       "XX",  _
       "XX"}, _
{    "XX  ",  _
     "XXX ",  _
     " XX ",  _
     " XXX",  _
     " XXX",  _
     " XX ",  _
     "XXX ",  _
     "XX  "}, _
{  " XX  X",  _
   "XXXXXX",  _
   "X  XX ",  _
   "      ",  _
   "      ",  _
   "      ",  _
   "      ",  _
   "      "}, _
{       " ",  _
        " ",  _
        " ",  _
        " ",  _
        " ",  _
        " ",  _
        " ",  _
        " "}  _
}

If Options.FontStyle.toLower = "terminal" Then
  BlockText = TerminalFont
ElseIf Options.FontStyle.toLower = "gothic" Then
  BlockText = GothicFont
Else
  BlockText = OzoneFont
End If

End Sub