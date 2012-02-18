'-------------------------------------------------------------------------------
'
'     XXXXXX                    XXXXXXX
'     X     X                      X
'     X     X  XXX  XXXXX  XXX     X    XXXXX X   X XXXXX    X   X XXXX
'     XXXXXX  X   X   X   X   X    X    X      X X    X      X   X X   X
'     X   X   X   X   X   X   X    X    XXXX    X     X      X   X XXXX
'     X    X  X   X   X   X   X    X    X      X X    X       X X  X   X
'     X     X  XXX    X    XXX     X    XXXXX X   X   X   XX   X   XXXX
'
'-------------------------------------------------------------------------------
'
' ROTOTEXT.VB - a Cubesense VB Script for the eightCubed V2 LED cube
'
' Display a text message in animated block letters, each of which rotates
' at the center of an eightCubed V2 RGB LED cube. Works with any cubic or
' rectangular lattice having a minimum height (z-dimension), width (x-dimension)
' and depth (y-dimension) of 8 voxels
'
' Author:   Kurt A. Koehler
' Date:     August 2011
'
' CHANGE HISTORY
'
' 08/16/11: version 1.0 - Initial release
'
' 09/23/11: version 1.1
'           - corrected problems in raster pattern definitions for comma (",") &
'             lower-case "r" in the 'Gothic' font that resulted in run-time errors
'           - minor revisions to indentation, documentation & comments
'
'-------------------------------------------------------------------------------

Structure RGB
  Dim R As Byte
  Dim G As Byte
  Dim B As Byte
End Structure

Structure ExecParms
  Dim TextString As String  ' text to be moved through the LED cube
  Dim FontStyle  As String  ' supported values: "Terminal" | "Ozone"
  Dim StepAngle  As Integer ' Angle of incremental change between movements
  Dim Delay      As Integer ' number of frames to delay before incrementing text movement
  Dim Decay      As Boolean ' use progressive illumination/decay to smooth movement during delay
  Dim Cycles     As Integer ' number of complete rotations of text around the cube
  Dim Background As String  ' supported values: "void" | "asis"
  Dim Foreground As String  ' supported values: "rainbow" | "color"
  Dim ForeColor  As RGB     ' only required when ExecParms.Foreground="color"
End Structure

Public Options                     As ExecParms
Public BlockText (0 To 95, 0 To 7) As String
Public vbTrue                      As Boolean = -1
Public vbFalse                     As Boolean = 0

Public Sub Main() implements scriptInterface.IScript.Main

'-------------------------------------------------------------------------------
'
' EXECUTION OPTIONS - (values can be set in code immediately below this comment
'                      block)
'
' - Options.TextString is the text value of the displayed message and can contain
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
' - Options.StepAngle is an integer value defining the number of degrees of rotation
'   that is to occur in each incremental movement of the text character. Positive
'   values generate a counter-clockewise rotation; negative values generate a clockwise
'   rotation. This number should always be a value that is evenly divisible into 360
'   so that all rotational movement is uniform. Recommended values are in the range
'   of 1 to 30; Default value of Options.StepAngle is -10.
'
' - Options.Delay is a positive integer specifying how many times a text pattern
'   is to be repeated in successive frames before the next (incremental) movement
'   of the displayed text is generated; the lower the value, the faster the text moves.
'   Value must be a positive integer or 0; default value of Options.Delay is 1.
'
' - Options.Decay is a boolean value indicating whether a progressive illumination
'   and decay technique is to be used create the illusion of smooth movement during
'   the Options.Delay period. Default value of Options.Decay is vbTrue.
'
' - Options.Cycles is a positive integer specifying the number of complete message
'   text rotations about the cube this animation is to contain. This option defaults
'   to a value of 1, but higher values can be useful when overlaying an existing
'   ECA animation that is multiple times larger (in frames) than a single cycle of
'   the display animation
'
' - Options.Background is a text value that specifies the preferred method of
'   illuminating lights in the cube 'around' the text and can have values of:
'   "void"     - clears the color values (to RGB 0,0,0) of all LEDs in each frame
'                before generating the display text
'   "asis"     - leaves any exisiting frame content intact but overlays existing
'                content already loaded in the 'Frames List'(useful for adding a
'                RotoText display to another ECA animation)
'   Default value of Options.Background is "void".
'
' - Options.Foreground is a text value that specifies the preferred method of
'   illuminating lights in the text raster pattern and can have values of:
'   "rainbow"  - raster text is presented using a 'rainbow' gradient of color,
'                incrementally changing as it scrolls across the face of the cube
'   "color"    - text is presented using a solid color value;  Options.ForeColor
'                R, G and B values must also be supplied
'   Default value of Options.Foreground is "rainbow"
'
' - Options.ForeColor R, G and B variables are integer values in the range of
'   0 - 255 that specify the intensityI of Red, Green and Blue components of the
'   text foreground colors.
'
'-------------------------------------------------------------------------------
'
' CAUTION: Little to no validation is performed on the values coded in the Options
'          parameters, so please adjust values within recommended ranges; unpredictable
'          results, including script failure, may otherwise occur'

  Options.TextString  = " ROTOTEXT"
  Options.FontStyle   = "Ozone"      ' "Gothic" | "Terminal" | "Ozone"
  Options.StepAngle   = -5           ' Angle of incremental movement
  Options.Delay       = 1            ' integer >= 0 ; recommended: 3 - 6
  Options.Decay       = vbTrue       ' vbTrue | vbFalse
  Options.Cycles      = 1            ' integer > 0
  Options.Background  = "void"       ' "void" | "asis"
  Options.Foreground  = "rainbow"    ' "rainbow" | "color"
  Options.ForeColor.R = 255          ' integer 0 - 255, required only when Foreground = "color"
  Options.ForeColor.G = 255          ' integer 0 - 255, required only when Foreground = "color"
  Options.ForeColor.B = 255          ' integer 0 - 255, required only when Foreground = "color"

'   Variable declarations & initialization

  Dim Terminated          As Boolean = vbFalse
  Dim InputString         As String
  Dim CrLf                As String = Environment.NewLine
  Dim msgText             As String
  Dim H                   As Double  = 0
  Dim I                   As Integer = Options.StepAngle ' Angle increment between movements
  Dim I1, I2, I3, I4, IX  As Decimal
  Dim pctDecay            As Decimal = 0.000
  Dim frameCount          As Integer = cs.getFrameCount()
  Dim totalFrames         As Integer = 0
  Dim response            As Integer = 0
  Dim dK                  As Integer
  Dim frameNum            As Integer = 0
  Dim f                   As Integer = 0
  Dim j, k, kk, l, s, c   As Integer
  Dim z, zz               As Integer
  Dim x1, x2, xx1, xx2    As Integer
  Dim y1, y2, yy1, yy2    As Integer
  Dim x, dx, xx           As Double
  Dim dy, yy              As Double
  Dim Angle               As Double
  Dim Radian1, Radian2    As Double
  Dim Cosine1, Cosine2    As Double
  Dim Sine1, Sine2        As Double
  Dim StartAngle          As Integer = 0
  Dim EndAngle            As Integer = 360
  Dim r1, r2, rr1, rr2    As Integer
  Dim g1, g2, gg1, gg2    As Integer
  Dim b1, b2, bb1, bb2    As Integer
  Dim xSize               As Integer = cs.getSizeX ()
  Dim ySize               As Integer = cs.getSizeY ()
  Dim zSize               As Integer = cs.getSizeZ ()
  Dim xMax                As Integer = xSize - 1
  Dim yMax                As Integer = ySize - 1
  Dim zMax                As Integer = zSize - 1
  Dim xMin                As Integer = 0
  Dim yMin                As Integer = 0
  Dim zMin                As Integer = 0
  Dim xPad                As Integer = (xSize - 8) / 2
  Dim yPad                As Integer = (ySize - 8) / 2
  Dim zPad                As Integer = (zSize - 8) / 2
  Dim charWidth           As Integer = 0
  Dim centerPadX          As Integer = 0
  Dim thisText (0 To 7)   As String
  Dim nextText (0 to 7)   As String
  Dim nullText ()         As String = New String (0 To 7) _
  {"        ",  _
   "        ",  _
   "        ",  _
   "        ",  _
   "        ",  _
   "        ",  _
   "        ",  _
   "        "}

' Terminate processing if the LED cube is too short to display the RotoText characters

  If xSize < 8 Or ySize < 8 Or zSize < 8 Then
    System.Windows.Forms.MessageBox.Show ("Error: At least one dimension of the cube is too small for a RotoText" & _
      " animation - your cube must contain at least 8 LEDs in all planes for the text characters to display" & _
      CrLf & CrLf & "Processing is terminated.", _
      "RotoText.VB Script", _
      System.Windows.Forms.MessageBoxButtons.OK, _
      System.Windows.Forms.MessageBoxIcon.Stop)
    Exit Sub
  End If

' Load BlockText with the raster pattern of the font specified in Options.Fontstyle

 Call RasterFont ' invoke the RasterFont subroutine to load BlockText with the raster
                 ' pattern of the font named in Options.FontStyle

' Prompt the user to enter/modify the text to be presented on the LED cube RotoText display

 InputString = Microsoft.VisualBasic.Interaction.InputBox _
   ("Enter the message to be presented as 3D text rotating inside the LED cube:", _
   "RotoText.VB Script", _
   Options.TextString)

 If InputString = "" Then Exit Sub

 Options.TextString = InputString

' Initialize TextString to all ASCII text characters (for testing font design changes only)

' Options.TextString = ""
' For i = 32 to 127
'    Options.TextString = Options.TextString & Convert.ToChar(i)
' Next

' Establish clockwise or counter-clockwise rotation criteria based on whether the
' Options.StepAngle is negative or positive

  If Options.StepAngle < 0 Then
    EndAngle   = -360
    Options.StepAngle  = -Options.StepAngle
  End If

' Calculate the total number of frames that will be required by this animation and compare
' it to the value returned by cs.getFrameCount(), the number of frames defined in the
' 'Frames List'; if they are unequal, display an appropriate warning mesage to the user
' before proceeding to compute the frame content for the RotoText display.

  totalFrames =  2 * Options.Cycles * (360 / Options.StepAngle) * _
                 Options.TextString.Length * (Options.Delay + 1)

  If totalFrames <> frameCount Then
    msgText = "This animation requires " & totalFrames.ToString & _
      " frames to complete the RotoText display. "
    If totalFrames > cs.getFrameCount() Then
      msgText = msgText & "Not enough frames exist in the 'Frames List' to generate a" & _
        " complete animation... OK to proceed?"
    Else
      msgText = msgText & "More frames exist in the 'Frames List' than are needed to" & _
        " generate a complete animation... OK to proceed?"
    End If

    response = System.Windows.Forms.MessageBox.Show (msgText, _
               "RotoText.VB Script", _
                System.Windows.Forms.MessageBoxButtons.OKCancel, _
                System.Windows.Forms.MessageBoxIcon.Exclamation)
    If response = System.Windows.Forms.DialogResult.Cancel Then Exit Sub

    totalFrames = System.Math.Min (totalFrames, frameCount)
  End If

' Establish the text foreground colors according to the 'Options.ForeColor' values specified

  If Options.Foreground = "color" Then
    r1  = Options.ForeColor.R
    g1  = Options.ForeColor.G
    b1  = Options.ForeColor.B
    r2  = Options.ForeColor.R
    g2  = Options.ForeColor.G
    b2  = Options.ForeColor.B
    rr1 = Options.ForeColor.R
    gg1 = Options.ForeColor.G
    bb1 = Options.ForeColor.B
    rr2 = Options.ForeColor.R
    gg2 = Options.ForeColor.G
    bb2 = Options.ForeColor.B
  End If

  l = Options.TextString.Length - 1

' To create an animation occupying a predetermined number of frames, it may be necessary to
' cycle the scrolling text around the cube more than once. Cause the marquee animation
' to be repeated 'Options.Cycles' numer of times.

  For c = 1 To Options.Cycles

    For j = 0 to l ' for each character in Options.TextString...

'     Assign the ASCII value of the current text character to the integer variable 'k',
'     reducing the value by 32 to account for the offset of corresponding character
'     patterns in the BlockText Array

      k = Convert.ToInt32 (Convert.ToChar (Options.TextString.Substring (j, 1))) - 32

'     Calculate the amount of left padding required to center the character raster pattern
'     in an 8-byte string array

      charWidth  = BlockText (k, 0).Length
      centerPadX = (8 - charWidth) / 2

'     Copy the raster pattern for the current text symbol into the 'thisText' array
'     of 8-byte strings, centering the pattern in the 8-byte field

      For z = 0 to 7
        thisText (z) = BlockText (k, z).PadLeft (charWidth + centerPadX, " "c)
        thisText (z) = thisText (z).PadRight (8, " "c)
      Next

'     Rotate text character 360 Degrees

      For Angle = 0 To (EndAngle - I) Step I

'       Calculate trigonometry of the current text position and the next text movement

        Radian1 = Angle       * System.Math.PI / 180 ' Angle of current position
        Radian2 = (Angle + I) * System.Math.PI / 180 ' Angle of next position
        Sine1   = System.Math.Sin (Radian1)          ' Sine of current angle
        Cosine1 = System.Math.Cos (Radian1)          ' Cosine of current angle
        Sine2   = System.Math.Sin (Radian2)          ' Sine of next angle
        Cosine2 = System.Math.Cos (Radian2)          ' Cosine of next angle

'       If progressive illumination/decay is requested, calculate the intensity of
'       colors for LEDs that are 'transitioning' during the delay cycles

        For dK = 0 To Options.Delay

          If Options.Delay > 0 And Options.Decay = vbTrue Then
            pctDecay = dK / Options.Delay
          End If

          I1 = 1.000 - pctDecay    ' relative intensity of current position
          I2 = pctDecay            ' relative intensity of next position

'         Illuminate the background color according to the options specified

          If Options.Background = "void" Then _
            cs.drawCube (f, 0, 0, 0, xMax, yMax, zMax, 0, 0, 0)

'         Illuminate each voxel in the raster pattern of the text character

          For z = 0 To 7
            zz = 7 - z + zPad
            For x = 0 To 7
              If thisText (z).Substring (x, 1) <> " " Then

'               Calculate coordinates of the voxels to illuminate for each point in
'               the raster pattern of the current character position

                xx  = (x - 3.5) * Sine1   + 3.5 + xPad
                yy  = (x - 3.5) * Cosine1 + 3.5 + ypad

                dx = Cosine1 * .5
                dy = Sine1   * .5

                x1 = RoundNum (xx + dx)
                x2 = RoundNum (xx - dx)

                y1 = RoundNum (yy - dy)
                y2 = RoundNum (yy + dy)

'               If rainbow text has been selected, calculate the colors of the lights
'               based upon voxel position within the cube

                If Options.Foreground = "rainbow" Then
                  r1 = 255 * x1 / xMax
                  g1 = 255 * y1 / yMax
                  b1 = 255 * zz / zMax
                  r2 = 255 * x2 / xMax
                  g2 = 255 * y2 / yMax
                  b2 = 255 * zz / zMax
                End If

'               use current position intensity on trailing edge of current text

                If x1 >= 0 And x1 <= 7 And y1 >= 0 And y1 <= 7 Then _
                  cs.drawVoxel (f, x1, y1, zz, r1 * I1, g1 * I1, b1 * I1)

'               use full intensity on leading edge of current text

                If x2 >= 0 And x2 <= 7 And y2 >= 0 And y2 <= 7 Then _
                  cs.drawVoxel (f, x2, y2, zz, r2, g2, b2)

'               Calculate coordinates of the voxels to illuminate for each point in
'               the raster pattern of the next character position

                xx = (x - 3.5) * Sine2   + 3.5 + xPad
                yy = (x - 3.5) * Cosine2 + 3.5 + yPad

                dx = Cosine2 * .5
                dy = Sine2   * .5

                xx1 = RoundNum (xx + dx)
                xx2 = RoundNum (xx - dx)

                yy1 = RoundNum (yy - dy)
                yy2 = RoundNum (yy + dy)

'               If rainbow text has been selected, calculate the colors of the lights
'               based upon voxel position in the cube

                If Options.Foreground = "rainbow" Then
                  rr1 = 255 * xx1 / xMax
                  gg1 = 255 * yy1 / yMax
                  bb1 = 255 * zz  / zMax
                  rr2 = 255 * xx2 / xMax
                  gg2 = 255 * yy2 / yMax
                  bb2 = 255 * zz  / zMax
                End if

'               If a voxel from the next movement position occupies the same coordinates
'               as a voxel from the current character position, determine whether to replace
'               the existing color value based on relative intensities - use the brighter
'               value

                If xx1 = x1 And yy1 = y1 Then
                  If I2 > I1 Then _
                    cs.drawVoxel (f, xx1, yy1, zz, rr1 * I2, gg1 * I2, bb1 * I2)
                ElseIf xx1 <> x2 Or yy1 <> y2 Then
                  If xx1 >= 0 And xx1 <= 7 And yy1 >= 0 And yy1 <= 7 Then _
                    cs.drawVoxel (f, xx1, yy1, zz, rr1 * I2, gg1 * I2, bb1 * I2)
                End If

                If xx2 = x1 And yy2 = y1 Then
                  If I2 > I1 Then _
                    cs.drawVoxel (f, xx2, yy2, zz, rr2 * I2, gg2 * I2, bb2 * I2)
                ElseIf xx2 <> x2 Or yy2 <> y2 Then
                  If xx2 >= 0 And xx2 <= 7 And yy2 >= 0 And yy2 <= 7 Then _
                    cs.drawVoxel (f, xx2, yy2, zz, rr2 * I2, gg2 * I2, bb2 * I2)
                End If
              End If ' If thisText (z).Substring (x, 1) <> " "
            Next ' For x = 0 To 7
          Next ' For z = 0 To 7

          cs.showProgress (100 * f / totalFrames)

          If cs.checkTermination() = vbTrue Or f = TotalFrames Then
            dK    = Options.Delay
            Angle = EndAngle
            If cs.checkTermination() = vbTrue Then
              j          = l
              c          = Options.Cycles
              Terminated = vbTrue
            End If
'           Exit For
          Else
            f = f + 1
          End If

        Next ' For dK = 0 To Options.Delay
      Next ' For Angle = 0 to 360 Step I

      If Not Terminated Then

'     If the end of the text string has not yet been reached...

      If j < l Then

'       Assign the ASCII value of the next text character to the integer variable 'kk',
'       reducing the value by 32 to account for the offset of corresponding character
'       patterns in the BlockText Array

        kk = Convert.ToInt32 (Convert.ToChar (Options.TextString.Substring (j + 1, 1))) - 32

'       Calculate the amount of left padding required to center the character raster pattern
'       in an 8-byte string array

        charWidth  = BlockText (kk, 0).Length
        centerPadX = (8 - charWidth) / 2

'       Copy the raster pattern for the text symbol into the 'nextText' array of 8-byte strings,
'       centering the pattern in the 8-byte field

        For z = 0 to 7
          nextText (z) = BlockText(kk, z).PadLeft (charWidth + centerPadX, " "c)
          nextText (z) = nextText (z).PadRight (8, " "c)
        Next
      Else ' the last character has been reached... use a blank character for the 'next text'
        kk = 0
        nextText = nullText
      End If

'     Rotate text character 360 Degrees, progressively diminishing the brightness of the 'current'
'     text character as the brightness of the 'next' character is progressively increased

      For Angle = 0 To (EndAngle - I) Step I
        I3 = (360 - System.Math.Abs (Angle)) / 360 ' I3 = illumination % of 'current' character (diminishing)
        I4 = System.Math.Abs (Angle) / 360         ' I4 = illumination % of 'next' character (brightening)

'       calculate trigonometry of the current and next text positions

        Radian1 = Angle       * System.Math.PI / 180 ' Angle of current position
        Radian2 = (Angle + I) * System.Math.PI / 180 ' Angle of next position
        Sine1   = System.Math.Sin (Radian1)          ' Sine of current angle
        Cosine1 = System.Math.Cos (Radian1)          ' Cosine of current angle
        Sine2   = System.Math.Sin (Radian2)          ' Sine of next angle
        Cosine2 = System.Math.Cos (Radian2)          ' Cosine of next angle

'       If progressive illumination/decay is requested, calculate the intensity of
'       colors for LEDs that are 'transitioning' during the delay cycles

        For dK = 0 To Options.Delay

          If Options.Delay > 0 And Options.Decay = vbTrue Then
            pctDecay = dK / Options.Delay
          End If

          I1 = 1.000 - pctDecay  ' relative intensity of current position
          I2 = pctDecay          ' relative intensity of next position

'         Illuminate the background color according to the options specified

          If Options.Background = "void" Then _
            cs.drawCube (f, 0, 0, 0, xMax, yMax, zMax, 0, 0, 0)

'         Illuminate each voxel in the raster pattern of the text character

          For z = 0 To 7
            zz = 7 - z + zPad
            For x = 0 To 7
              If thisText (z).Substring (x, 1) <> " " Or _
                 nextText (z).Substring (x, 1) <> " " Then

'               If both the current and next letters have the same voxel illuminated,
'               use the illumination setting of the brighter one

                If thisText (z).Substring (x, 1) <> " " And _
                   nextText (z).Substring (x, 1) <> " " Then
                  If System.Math.Abs (Angle) < 180 Then
                    IX = I3
                  Else IX = I4
                  End If
                ElseIf thisText (z).Substring (x, 1) <> " " Then
                  IX = I3
                Else
                  IX = I4
                End If

'               Calculate coordinates of the voxels to illuminate for each point in
'               the raster pattern of the current character position

                xx  = (x - 3.5) * Sine1   + 3.5 + xPad
                yy  = (x - 3.5) * Cosine1 + 3.5 + yPad

                dx = Cosine1 * .5
                dy = Sine1   * .5

                x1 = RoundNum (xx + dx)
                x2 = RoundNum (xx - dx)

                y1 = RoundNum (yy - dy)
                y2 = RoundNum (yy + dy)

'               If rainbow text has been selected, calculate the colors of the lights
'               based upon voxel position within the cube

                If Options.Foreground = "rainbow" Then
                  r1 = 255 * x1 / xMax
                  g1 = 255 * y1 / yMax
                  b1 = 255 * zz / zMax
                  r2 = 255 * x2 / xMax
                  g2 = 255 * y2 / yMax
                  b2 = 255 * zz / zMax
                End If

'               use current position intensity on trailing edge of current text

                If x1 >= 0 And x1 <= 7 And y1 >= 0 And y1 <= 7 Then _
                  cs.drawVoxel (f, x1, y1, zz, r1 * I1 * IX, g1 * I1 * IX, b1 * I1 * IX)

'               use full intensity on leading edge of current text

                If x2 >= 0 And x2 <= 7 And y2 >= 0 And y2 <= 7 Then _
                  cs.drawVoxel (f, x2, y2, zz, r2 * IX, g2 * IX, b2 * IX)

'               Calculate coordinates of the voxels to illuminate for each point in
'               the raster pattern of the next character position

                xx = (x - 3.5) * Sine2   + 3.5 + xPad
                yy = (x - 3.5) * Cosine2 + 3.5 + yPad

                dx = Cosine2 * .5
                dy = Sine2   * .5

                xx1 = RoundNum (xx + dx)
                xx2 = RoundNum (xx - dx)

                yy1 = RoundNum (yy - dy)
                yy2 = RoundNum (yy + dy)

'               If rainbow text has been selected, calculate the colors of the lights
'               based upon voxel position in the cube

                If Options.Foreground = "rainbow" Then
                  rr1 = 255 * xx1 / xMax
                  gg1 = 255 * yy1 / yMax
                  bb1 = 255 * zz  / zMax
                  rr2 = 255 * xx2 / xMax
                  gg2 = 255 * yy2 / yMax
                  bb2 = 255 * zz  / zMax
                End if

'               If a voxel from the next movement position occupies the same coordinates
'               as a voxel from the current character position, determine whether to replace
'               the existing color value based on relative intensity - use the brighter
'               value

                If xx1 = x1 And yy1 = y1 Then
                  If I2 > I1 Then _
                    cs.drawVoxel (f, xx1, yy1, zz, rr1 * I2 * IX, gg1 * I2 * IX, bb1 * I2 * IX)
                ElseIf xx1 <> x2 Or yy1 <> y2 Then
                  If xx1 >= 0 And xx1 <= 7 And yy1 >= 0 And yy1 <= 7 Then _
                    cs.drawVoxel (f, xx1, yy1, zz, rr1 * I2 * IX, gg1 * I2 * IX, bb1 * I2 * IX)
                End If

                If xx2 = x1 And yy2 = y1 Then
                  If I2 > I1 Then _
                    cs.drawVoxel (f, xx2, yy2, zz, rr2 * I2 * IX, gg2 * I2 * IX, bb2 * I2 * IX)
                ElseIf xx2 <> x2 Or yy2 <> y2 Then
                  If xx2 >= 0 And xx2 <= 7 And yy2 >= 0 And yy2 <= 7 Then _
                    cs.drawVoxel (f, xx2, yy2, zz, rr2 * I2 * IX, gg2 * I2 * IX, bb2 * I2 * IX)
                End If
              End If ' If BlockText (k, z).Substring (x, 1) <> " "
            Next ' For x = 0 To 7
          Next ' For z = 0 To 7

          cs.showProgress (100 * f / totalFrames)

          If cs.checkTermination() = vbTrue Or f = TotalFrames Then
            dK     = Options.Delay
            Angle  = EndAngle
            j      = l
            c      = Options.Cycles
'           Exit For
          Else
            f = f + 1
          End If

        Next ' For dK = 0 To Options.Delay
      Next '  For Angle = I To EndAngle - I Step I
      End If ' Not Terminated
    Next ' For j = 0 to l
  Next ' For c = 1 To Options.Cycles

  System.Windows.Forms.MessageBox.Show ("RotoText display generation created " & _
                                        System.Math.Min (f, totalFrames).ToString & " frames.", _
                                        "RotoText.VB Script", _
                                        System.Windows.Forms.MessageBoxButtons.OK, _
                                        System.Windows.Forms.MessageBoxIcon.Asterisk)
End Sub

Public Function RoundNum (Number as Double) As Double

' Calculate the rounded integer value of any Number

  If Number < 0 Then
    RoundNum = System.Math.Ceiling (Number - .5)
  Else
    RoundNum = System.Math.Floor (Number + .5)
  End If

End Function

Public Sub RasterFont ()

' Assign the raster patterns of the font named in Options.FontStyle to the BlockText string array

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
{    "    ",  _
     "    ",  _
     "    ",  _
     "    ",  _
     "    ",  _
     " XXX",  _
     "  XX",  _
     "XX  "}, _
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
{    "    ",  _
     " XXX",  _
     " XXX",  _
     "    ",  _
     "    ",  _
     " XXX",  _
     "  XX",  _
     "XX  "}, _
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