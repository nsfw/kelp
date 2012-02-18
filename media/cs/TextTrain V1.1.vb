'-------------------------------------------------------------------------------
'
'  XXXXXXX                   XXXXXXX
'     X                         X
'     X    XXXXX X   X XXXXX    X    XXXX   XXX  XXXXX X   X    X   X  XXXX
'     X    X      X X    X      X    X   X X   X   X   XX  X    X   X  X   X
'     X    XXXX    X     X      X    XXXX  XXXXX   X   X X X    X   X  XXXX
'     X    X      X X    X      X    X  X  X   X   X   X  XX     X X   X   X
'     X    XXXXX X   X   X      X    X   X X   X XXXXX X   X XX   X    XXXX
'
'-------------------------------------------------------------------------------
'
' TEXTTRAIN.VB - a Cubesense VB Script for the eightCubed V2 LED cube
'
' Display a text message in animated block letters, each of which moves
' through the inside of an eightCubed V2 RGB LED cube from the back to the front.
' Works with any cubic or rectangular lattice having a minimum height (z-dimension)
' and width (x-dimension) of 8 voxels
'
' Author:   Kurt A. Koehler
' Date:     August 2011
'
' CHANGE HISTORY
'
' 08/11/11: version 1.0 - Initial release
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
  Dim Delay      As Integer ' number of frames to delay before incrementing text movement
  Dim Decay      As Boolean ' use progressive illumination/decay to smooth movement during delay
  Dim Depth      As Integer ' gives text characters 3D depth if > 1; supported values: 1 - 3
  Dim Trail      As Integer ' leaves a residual 'ghosting' or 'comet tail' "Trail" voxels behind
  Dim Cycles     As Integer ' number of complete rotations of text around the cube
  Dim Spacing    As Integer ' number of 'blank' voxels between text characters
  Dim Background As String  ' supported values: "void" | "asis" | "fill" | "edge"
  Dim BackColor  As RGB     ' only required for ExecParms.Background="fill" | "edge"
  Dim Foreground As String  ' supported values: "rainbow" | "color"
  Dim ForeColor  As RGB     ' only required when ExecParms.Foreground="color"
End Structure

Public Options                     As ExecParms
Public BlockText (0 To 95, 0 To 7) As String
Public vbTrue                      As Boolean = -1
Public vbFalse                     As Boolean = 0

Public Sub Main() implements scriptInterface.IScript.Main
Try

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
' - Options.Delay is a positive integer specifying how many times a text pattern
'   is to be repeated in successive frames before the next (incremental) movement
'   of the displayed text is generated; the lower the value, the faster the text moves.
'   Value must be a positive integer or 0; default value of Options.Delay is 3.
'
' - Options.Decay is a boolean value indicating whether a progressive illumination
'   and decay technique is to be used create the illusion of smooth movement during
'   the Options.Delay period. Default value of Options.Decay is vbTrue.
'
' - Options.Depth is a positive integer specifying the 3D 'thickness' (in voxels)
'   of text characters displayed on the LED cube. Default value of Options.Depth is 2
'
' - Options.Trail is an integer value (>= 0) specifying how long (in voxels) a
'   'trail' of lights of diminishing intensity is to lag behind the moving characters.
'   This option produces a 'comet tail' or 'ghosting' effect when used.
'
' - Options.Cycles is a positive integer specifying the number of complete scrolling
'   text rotations about the cube this animation is to contain. This option defaults
'   to a value of 1, but higher values can be useful when overlaying an existing
'   ECA animation that is multiple times larger (in frames) than a single cycle of
'   the display animation
'
' - Options.Spacing is an integer value defining a number of additional empty
'   frames to display between the disappearance of one text character and the
'   beginning presentation of the next one. Default value of Options.Spacing is 0.
'
' - Options.Background is a text value that specifies the preferred method of
'   illuminating lights in the cube 'behind' the text and can have values of:
'   "void"     - clears the color values (to RGB 0,0,0) of all LEDs in each frame
'                before generating the display text
'   "asis"     - leaves any exisiting frame content intact but overlays existing
'                content already loaded in the 'Frames List'(useful for adding a
'                TextTrain display to another ECA animation)
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
'   "color"    - text is presented using a solid color value;  Options.ForeColor
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

  Options.TextString  = "www.Lumisense.com "
  Options.FontStyle   = "Ozone"      ' "Gothic" | "Terminal" | "Ozone"
  Options.Delay       = 3            ' integer >= 0 ; recommended: 3 - 6
  Options.Decay       = vbTrue       ' vbTrue | vbFalse
  Options.Depth       = 2            ' integer > 0
  Options.Trail       = 0            ' integer >= 0
  Options.Cycles      = 1            ' integer > 0
  Options.Spacing     = 0            ' integer >= 0
  Options.Background  = "void"       ' "void" | "asis" | "fill" | "edge"
  Options.BackColor.R = 0            ' integer 0 - 255, required when Background <> "void"
  Options.BackColor.G = 0            ' integer 0 - 255, required when Background <> "void"
  Options.BackColor.B = 0            ' integer 0 - 255, required when Background <> "void"
  Options.Foreground  = "color"    ' "rainbow" | "color"
  Options.ForeColor.R = 255          ' integer 0 - 255, required only when Foreground = "color"
  Options.ForeColor.G = 0          ' integer 0 - 255, required only when Foreground = "color"
  Options.ForeColor.B = 0          ' integer 0 - 255, required only when Foreground = "color"

'   Variable declarations & initialization

  Dim InputString         As String
  Dim CrLf                As String = Environment.NewLine
  Dim msgText             As String
  Dim H                   As Double  = 0
  Dim I1                  As Decimal = 0.000
  Dim I2                  As Decimal = 0.000
  Dim pctDecay            As Decimal = 0.000
  Dim br                  As Decimal
  Dim frameCount          As Integer = cs.getFrameCount()
  Dim totalFrames         As Integer = 0
  Dim response            As Integer = 0
  Dim dK                  As Integer = 0
  Dim frameNum            As Integer = 0
  Dim f                   As Integer = 0
  Dim j, k, l, s, t, c, d As Integer
  Dim x, y, z, xx, yy, zz As Integer
  Dim x1, x2              As Integer
  Dim rX, gX, bX          As Integer
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
  Dim charWidth           As Integer = 0
  Dim widthIndex          As integer = 0
  Dim centerPadX          As integer = 0
  Dim centerPadZ          As integer = (zSize - 8) / 2

' Terminate processing if the LED cube is too short to display the TextTrain characters

  If zSize < 8 Then
    System.Windows.Forms.MessageBox.Show ("Error: The Z-dimension of the cube is too small for a TextTrain" & _
      " animation - your cube must be at least 8 LEDs in height for the text characters to display" & _
      CrLf & CrLf & "Processing is terminated.", _
      "TextTrain.VB Script", _
      System.Windows.Forms.MessageBoxButtons.OK, _
      System.Windows.Forms.MessageBoxIcon.Stop)
    Exit Sub
  End If

' Load BlockText with the raster pattern of the font specified in Options.Fontstyle

 Call RasterFont ' invoke the RasterFont subroutine to load BlockText with the raster
                 ' pattern of the font named in Options.FontStyle

' Prompt the user to enter/modify the text to be presented on the LED cube TextTrain display

 InputString = Microsoft.VisualBasic.Interaction.InputBox _
   ("Enter message to be presented as 3D text moving across the LED cube:", _
   "TextTrain.VB Script", _
   Options.TextString)

 If InputString = "" Then Exit Sub

 Options.TextString = InputString

' Initialize TextString to all ASCII text characters (for testing font design changes only)

' Options.TextString = ""
' For i = 32 to 127
'    Options.TextString = Options.TextString & Convert.ToChar(i)
' Next

' Calculate the total number of frames that will be required by this animation and compare
' it to the value returned by cs.getFrameCount(), the number of frames defined in the
' 'Frames List'; if they are unequal, display an appropriate warning mesage to the user
' before proceeding to compute the frame content for the TextTrain display.

  totalFrames =  Options.Cycles * (yMax + 1 + Options.Depth + Options.Trail) * _
                 Options.TextString.Length * (Options.Delay + 1) + _
                 Options.TextString.Length * Options.Spacing

  If totalFrames <> frameCount Then
    msgText = "This animation requires " & totalFrames.ToString & _
      " frames to complete the TextTrain display. "
    If totalFrames > cs.getFrameCount() Then
      msgText = msgText & "Not enough frames exist in the 'Frames List' to generate a" & _
        " complete animation... OK to proceed?"
    Else
      msgText = msgText & "More frames exist in the 'Frames List' than are needed to" & _
        " generate a complete animation... OK to proceed?"
    End If

    response = System.Windows.Forms.MessageBox.Show (msgText, _
               "TextTrain.VB Script", _
                System.Windows.Forms.MessageBoxButtons.OKCancel, _
                System.Windows.Forms.MessageBoxIcon.Exclamation)
    If response = System.Windows.Forms.DialogResult.Cancel Then Exit Sub

    totalFrames = Math.Min (totalFrames, frameCount)
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

' Establish the background color according to the 'BackColor' option specified

  If Options.Background = "void" Or Options.Background = "asis" Then
    Options.BackColor.R = 0 ' red component of background color
    Options.BackColor.G = 0 ' green component of background color
    Options.BackColor.B = 0 ' blue component of background color
  End If

  rX = Options.BackColor.R ' red component of background color
  gX = Options.BackColor.G ' green component of background color
  bX = Options.BackColor.B ' blue component of background color

  l = Options.TextString.Length - 1

' To create an animation occupying a predetermined number of frames, it may be necessary to
' cycle the scrolling text around the cube more than once. Cause the marquee animation
' to be repeated 'Options.Cycles' numer of times.

  For c = 1 To Options.Cycles

  For j = 0 to l ' for each character in Options.TextString...
    k = Convert.ToInt32 (Convert.ToChar (Options.TextString.Substring (j, 1))) - 32

'   Move text through the cube from the north side to the south side

    t = Options.Trail

    For y = yMin - Options.Depth To yMax + t

'     If progressive illumination/decay is requested, calculate the intensity of
'     colors for lights that are 'moving' during the delay cycles

      For dK = 0 To Options.Delay

        If Options.Delay > 0 And Options.Decay = vbTrue Then
          pctDecay = dK / Options.Delay
        End If

        I1 = 1.000 - pctDecay    ' relative intensity of current position
        I2 = pctDecay            ' relative intensity of next position

'       Illuminate the background color according to the options specified

        Call PaintBack (f, xMax, yMax, zMax, rX, gX, bX)

'       Illuminate each voxel in the raster pattern of the text character

        charWidth    = BlockText (k, 0).Length
        widthIndex   = charWidth - 1
        centerPadX   = (xSize - charWidth) / 2

        For z = 0 To 7
          zz = 7 - z
          For x = 0 To widthIndex
            If BlockText (k, z).Substring (x, 1) <> " " Then

'             Calculate coordinates of the voxels to illuminate for each point in
'             the raster pattern of the current character position

              yy = y
              xx = x + centerPadX

'             If rainbow text has been selected, calculate the colors of the lights
'             based upon voxel position within the cube

              If Options.Foreground = "rainbow" Then
                r1 = 255 * xx / xMax
                g1 = 255 * yy / yMax
                b1 = 255 * zz / zMax
                r2 = 255 * xx / xMax
                g2 = 255 * yy / yMax
                b2 = 255 * zz / zMax
              End If

'             If trailing is requested, generate a 'comet tail' following the character

              If Options.Trail > 0 Then
                For t = 0 To Options.Trail
                  yy = y - t
                  If yy >= yMin And yy <= yMax Then
                    br = .5 * (Options.Trail - t - I2) / Options.Trail
                    If Options.Foreground = "rainbow" Then g1 = 255 * yy / yMax
                    If br > 0 Then cs.drawVoxel (f, xx, yy, zz + centerPadZ, br * r1, br * g1, br * b1)
                  End If
                Next

'             otherwise, use current position decay intensity on trailing edge of current text

              ElseIf yy >= 0 And yy <= xMax Then
                cs.drawVoxel (f, xx, yy, zz + centerPadZ, r1 * I1, g1 * I1, b1 * I1)
              End If

'             use full intensity on body and leading edge of current text

              For d = 1 To Options.Depth - 1
                yy = y + d
                If yy >= yMin And yy <= yMax Then
                  If Options.Foreground = "rainbow" Then g2 = 255 * yy / yMax
                  cs.drawVoxel (f, xx, yy, zz + centerPadZ, r2, g2, b2)
                End If
              Next

'             Calculate coordinates of the voxels to illuminate for each point in
'             the raster pattern of the NEXT character position

              yy = y + Options.Depth

              If yy >= yMin And yy <= yMax Then

'               If rainbow text has been selected, calculate the colors of the lights
'               in the next position based upon voxel position within the cube

                If Options.Foreground = "rainbow" Then
                  rr2 = 255 * xx / xMax
                  gg2 = 255 * yy / yMax
                  bb2 = 255 * zz / zMax
                End if

'               display the 'next position intensity' ahead of the leading edge

                cs.drawVoxel (f, xx, yy, zz + centerPadZ, rr2 * I2, gg2 * I2, bb2 * I2)

              End If ' yy >= yMin And yy <= yMax
            End If ' If BlockText (k, z).Substring (x, 1) <> " "
          Next ' For x = 0 To widthIndex
        Next ' For z = 0 To 7

        cs.showProgress (100 * f / totalFrames)
        f = f + 1

        If (cs.checkTermination() = vbTrue Or f > totalFrames) Then Exit For

      Next ' For dK = 0 To Options.Delay
    Next ' For y = yMin - Options.Depth To yMax + t

'  delay the presentation of the next text character by 'Options.Spacing' frames

    For s = 1 to Options.Spacing
      Call PaintBack (f, xMax, yMax, zMax, rX, gX, bX)
      f = f + 1
    Next

  Next ' For j = 0 to l

  Next ' For c = 1 To Options.Cycles

  System.Windows.Forms.MessageBox.Show ("TextTrain display generation created " & _
                                        Math.Min (f, totalFrames).ToString & " frames.", _
                                        "TextTrain.VB Script", _
                                        System.Windows.Forms.MessageBoxButtons.OK, _
                                        System.Windows.Forms.MessageBoxIcon.Asterisk)
Catch ex as Exception
  System.Windows.Forms.MessageBox.Show (ex.ToString, _
                                        "TextTrain.VB Script", _
                                        System.Windows.Forms.MessageBoxButtons.OK, _
                                        System.Windows.Forms.MessageBoxIcon.Stop)
End Try

End Sub

Public Sub PaintBack (f    As Integer, _
                      xMax As Integer, _
                      yMax As Integer, _
                      zMax As Integer, _
                      r    As Integer, _
                      g    As Integer, _
                      b    As Integer)

' Illuminate the background color according to the options specified

  If Options.Background <> "asis" Then
    If Options.Background = "fill" Or Options.Background = "void" Then
      cs.drawCube (f, 0, 0, 0, xMax, yMax, zMax, r, g, b)
    ElseIf Options.Background = "edge" Then
      cs.drawCube (f, 0, 0, 0, xMax, Options.Depth - 1, zMax, r, g, b)
      cs.drawCube (f, 0, 0, 0, Options.Depth - 1, yMax, zMax, r, g, b)
      cs.drawCube (f, xMax - Options.Depth + 1, 0, 0, xMax, yMax, zMax, r, g, b)
      cs.drawCube (f, 0, yMax - Options.Depth + 1, 0, xMax, yMax, zMax, r, g, b)
    End If
  End If

End Sub

Public Sub RasterFont ()
Try

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
     " XXX",  _
     " XXX",  _
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

Catch ex as Exception
  System.Windows.Forms.MessageBox.Show (ex.ToString, _
                                        "TextTrain.VB Script", _
                                        System.Windows.Forms.MessageBoxButtons.OK, _
                                        System.Windows.Forms.MessageBoxIcon.Stop)
End Try

End Sub