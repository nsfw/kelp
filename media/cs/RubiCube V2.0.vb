'-------------------------------------------------------------------------------
'
'     XXXXXX                     XXXXX
'     X     X                   X     X
'     X     X X   X XXXX  XXXXX X       X   X XXXX  XXXXX    X   X XXXX
'     XXXXXX  X   X X   X   X   X       X   X X   X X        X   X X   X
'     X   X   X   X XXXX    X   X       X   X XXXX  XXXX     X   X XXXX
'     X    X  X   X X   X   X   X     X X   X X   X X         X X  X   X
'     X     X  XXX  XXXX  XXXXX  XXXXX   XXX  XXXX  XXXXX XX   X   XXXX
'
'-------------------------------------------------------------------------------
'
' RUBICUBE.VB - a Cubesense VB Script for the eightCubed V2 LED cube
'
' Display a Rubik's Cube on the eightCubed LED lattice and execute pre-defined
' 'slice' movements and cube rotations to create a variety of interesting and colorful
' patterns. Movements are coded using the official WCA notation from Article 12 of
' the World Cube Association Regulations (visit http://www.cubewhiz.com/notation.html
' to view graphic illustrations of notations for slice moves and cube rotations)
'
' USAGE NOTES
'
' To change the Cube pattern being generated, edit the declaration statement
' for the 'Pattern' string variable in the code to equate it with pre-defined string
' variable containing a different maneuver, e.g.,
'
' Dim Pattern           As String = PlummersCross
'
' ... or initialize its value to a text string containing a new maneuver in WCA
' notation:
'
' Dim Pattern           As String = "L U F2 R L' U2 B' U D B2 L F B' R' L F' R"
'
' Hint: for the best CubeSense 'Preview' presentation, it is suggested that you
' increase the 'LED size:' option value equal to approximately half the 'LED spacing:'
' option value and choose the 'Invisible' selection of the 'Black LEDs as:" option
' in the 'View' tab
'
'
' Author:   Kurt A. Koehler
' Date:     October 2011
'
' CHANGE HISTORY
'
' 10/13/11: version 1.0 - Initial release, limited to the display of an 8x8x8 Rubik's
'           cube design only - future versions being planned will support bigger cubes
'           when presented on lattices having larger dimensions
'
' 10/18/11: version 2.0 - Added automatic sizing of the Rubik's cube up to 23x23x23
'           dimension based on the CubeSense lattice size.
'           - Added suppoort for the CubeSense progress bar display
'
'-------------------------------------------------------------------------------

Structure RGB
  Dim R(,,,) As Byte
  Dim G(,,,) As Byte
  Dim B(,,,) As Byte
End Structure

Structure ExecParms
  Dim Delay      As Byte    ' number of frames to delay before displaying the next movement
  Dim Pause      As Integer ' number of frames to repeat cube pattern after maneuver is
                            ' completed so that solution can be reviewed before the
                            ' maneuver is reversed
End Structure

' Public Variables

Public csLattice        As RGB
Public Options          As ExecParms
Public xSizeCS          As Integer
Public ySizeCS          As Integer
Public zSizeCS          As Integer
Public xMaxCS           As Integer
Public yMaxCS           As Integer
Public zMaxCS           As Integer
Public currentFrame     As Integer
Public previousFrame    As Integer
Public latticeSize      As Integer
Public frameCount       As Integer
Public totalFrames      As Integer
Public frameMaxCS       As Integer
Public Lo, Hi           As Integer
Public FwdStart, FwdEnd As Integer
Public RevStart, RevEnd As Integer
Public OutSideStart     As Integer
Public OutSideEnd       As Integer
Public Delta            As Integer
Public MoveIndex        As Integer
Public f, i, j, x, y, z As Integer
Public xx, yy, zz       As Integer
Public Cycles           As Integer
Public cubeSize         As Integer
Public sliceWidth       As Integer
Public innerMargin      As Integer
Public cubeHiBound      As Integer
Public x1, y1, z1       As Double
Public a1, a2, l        As Double
Public Center           As Double
Public Movement         As String
Public Direction        As String
Public msgText          As String
Public msgTitle         As String = "RubiCube.VB CubeSense Animation Script"
Public ValidMoves       As String = "LDFMESRUBxyz"

Public Sub Main() Implements scriptInterface.IScript.Main
Try
' ==============================================================================
' Edit run-time options below to change animation display characteristics
' ==============================================================================

  Options.Delay = 3       ' Set value (1 or greater) of Options.Delay here
  Options.Pause = 120     ' Set value (1 or greater) of Options.Pause here

' ==============================================================================
' Edit run-time options above to change animation display characteristics
' ==============================================================================

  xSizeCS = cs.getSizeX()
  ySizeCS = cs.getSizeY()
  zSizeCS = cs.getSizeZ()

' Scale the Rubik's Cube dimensions to closely match the size of the CubeSense lattice

  If xSizeCS >= 8 And ySizeCS >= 8 And zSizeCS >= 8 Then ' OK to proceed... lattice is sufficiently large
    If xSizeCS >= 23 And ySizeCS >= 23 And zSizeCS >= 23 Then
      cubeSize = 10
    ElseIf xSizeCS >= 22 And ySizeCS >= 22 And zSizeCS >= 22 Then
      cubeSize = 9
    ElseIf xSizeCS >= 20 And ySizeCS >= 20 And zSizeCS >= 20 Then
      cubeSize = 8
    ElseIf xSizeCS >= 19 And ySizeCS >= 19 And zSizeCS >= 19 Then
      cubeSize = 7
    ElseIf xSizeCS >= 17 And ySizeCS >= 17 And zSizeCS >= 17 Then
      cubeSize = 6
    ElseIf xSizeCS >= 16 And ySizeCS >= 16 And zSizeCS >= 16 Then
      cubeSize = 5
    ElseIf xSizeCS >= 14 And ySizeCS >= 14 And zSizeCS >= 14 Then
      cubeSize = 4
    ElseIf xSizeCS >= 13 And ySizeCS >= 13 And zSizeCS >= 13 Then
      cubeSize = 3
    ElseIf xSizeCS >= 11 And ySizeCS >= 11 And zSizeCS >= 11 Then
      cubeSize = 2
    ElseIf xSizeCS >= 10 And ySizeCS >= 10 And zSizeCS >= 10 Then
      cubeSize = 1
    Else 'xSizeCS >= 8 And ySizeCS >= 8 And zSizeCS >= 8
      cubeSize = 0
    End If
  Else
    msgText = "This script cannot generate Rubik's Cube animation for lattices smaller than " & _
              "8 x 8 x 8 in (X x Y x Z) dimensions. Change your CubeSense lattice dimensions (" & _
              xSizeCS.ToString & " x " & ySizeCS.ToString & " x " & zSizeCS.ToString & _
              ") to a supported size and re-run this script."
    System.Windows.Forms.MessageBox.Show (System.Windows.Forms.Form.ActiveForm, _
                                          msgText, msgTitle, _
                                          System.Windows.Forms.MessageBoxButtons.OK, _
                                          System.Windows.Forms.MessageBoxIcon.Stop)
    Return
  End If

  sliceWidth  = System.Math.Floor(cubeSize / 2) + 2
  innerMargin = cubeSize Mod 2
  cubeHiBound = 1 + 3 * sliceWidth + 2 * innerMargin
  Center      = cubeHiBound / 2
  xMaxCS      = xSizeCS - 1
  yMaxCS      = ySizeCS - 1
  zMaxCS      = zSizeCS - 1
  frameCount  = cs.getFrameCount()
  frameMaxCS  = frameCount - 1

' Private Variables - Declaration & Initialization

  Dim m, response       As Integer
  Dim Move              As String

' Rubik's Cube Patterns - Visit http://www.math.ucf.edu/~reid/Rubik/patterns.html
' to view graphic ilustrations of the following patterns and more.

  Dim PonsAsinorum      As String = "F2 B2 R2 L2 U2 D2"
  Dim Checkerboard3     As String = "F B2 R' D2 B R U D' R L' D' F' R2 D F2 B'"
  Dim Checkerboard6     As String = "R' D' F' D L F U2 B' L U D' R' D' L F L2 U F'"
  Dim Stripes           As String = "F U F R L2 B D' R D2 L D' B R2 L F U F"
  Dim CubeInCube        As String = "F L F U' R U F2 L2 U' L' B D' B' L2 U"
  Dim CubeInCubeInCube  As String = "U' L' U' F' R2 B' R F U B2 U B' L U' F U R F'"
  Dim ChristmansCross   As String = "U F B' L2 U2 L2 F' B U2 L2 U"
  Dim PlummersCross     As String = "R2 L' D F2 R' D' R' L U' D R D B2 R' U D2"
  Dim Anaconda          As String = "L U B' U' R L' B R' F B' D R D' F'"
  Dim Python            As String = "F2 R' B' U R' L F' L F' B D' R B L2"
  Dim BlackMamba        As String = "R D L F' R L' D R' U D' B U' R' D'"
  Dim GreenMamba        As String = "R D R F R' F' B D R' U' B' U D2"
  Dim FemaleRattlesnale As String = "U2 D' L2 D B U B' R' L2 U2 F U' F R"
  Dim MaleRattlesnake   As String = "R' F' U F' U2 R L2 B U' B' D' L2 U2 D"
  Dim FemaleBoa         As String = "R U' R2 U2 F D2 R2 U' D' R D' F'"
  Dim FourSpot          As String = "F2 B2 U D' R2 L2 U D'"
  Dim SixSpot           As String = "U D' R L' F B' U D'"
  Dim OrthogonalBars    As String = "F R' U L F' L' F U' R U L' U' L F'"
  Dim SixTs             As String = "F2 R2 U2 F' B D2 L2 F B"
  Dim SixTwoOne         As String = "U B2 D2 L B' L' U' L' B D2 B2"
  Dim ExchangedPeaks    As String = "F U2 L F L' B L U B' R' L' U R' D' F' B R2"
  Dim TwoTwistedPeaks   As String = "F B' U F U F U L B L2 B' U F' L U L' B"
  Dim FourTwistedPeaks  As String = "U' D B R' F R B' L' F' B L F R' B' R F' U' D"

'===============================================================================
' Edit the 'Pattern' declaration statement below to change the cube pattern
'===============================================================================

  Dim Pattern           As String = PlummersCross

'===============================================================================
' Edit the 'Pattern' declaration statement above to change the cube pattern
'===============================================================================

  Dim Maneuver          As String() = Pattern.Split(New Char() {" "c})
  Dim mMax              As Integer = Maneuver.GetUpperBound(0)

' Calculate the total number of frames that will be required by this animation and compare
' it to the value returned by cs.getFrameCount(), the number of frames defined in the
' 'Frames List'; if they are unequal, display an appropriate warning mesage to the user
' before proceeding to compute the frame content for the Rubik's Cube animation.

  totalFrames = 1 + 2 * Options.Pause

  For m = 0 to mMax
    Move = Maneuver(m)
    Movement  = Move.Substring (0, 1) ' first position defines the face turn, slice move or cube rotation
    If ValidMoves.IndexOf(Movement) >= 0 Then
      If Move.IndexOf("2") > 0 Then
        totalFrames = totalFrames + 4 * cubeHiBound * Options.Delay
      Else
        totalFrames = totalFrames + 2 * cubeHiBound * Options.Delay
      End If
    End If
  Next

  If totalFrames <> frameCount Then
    msgText = "This animation requires " & totalFrames.ToString & _
      " frames to complete the Rubik's Cube display. "
    If totalFrames > frameCount Then
      msgText = msgText & "Not enough frames exist in the 'Frames List' to generate a" & _
        " complete animation... OK to proceed?"
    Else
      msgText = msgText & "More frames exist in the 'Frames List' than are needed to" & _
        " generate a complete animation... OK to proceed?"
    End If

    response = System.Windows.Forms.MessageBox.Show (System.Windows.Forms.Form.ActiveForm, _
                msgText, msgTitle, _
                System.Windows.Forms.MessageBoxButtons.OKCancel, _
                System.Windows.Forms.MessageBoxIcon.Exclamation)
    If response = System.Windows.Forms.DialogResult.Cancel Then Exit Sub
  End If

  frameMaxCS = Math.Min (totalFrames - 1, frameCount - 1)

' Allocate the 'csLattice' array dimensions large enough to contain color data for all voxels
' in all frames of the animation

  ReDim csLattice.R(frameMaxCS, xMaxCS, yMaxCS, zMaxCS)
  ReDim csLattice.G(frameMaxCS, xMaxCS, yMaxCS, zMaxCS)
  ReDim csLattice.B(frameMaxCS, xMaxCS, yMaxCS, zMaxCS)

' Draw the outer faces of a Rubik's Cube in the first frame of the animation
  m = cubeHiBound
  i = m - 1

  drawCube (0,   0, 0, 0,   xMaxCS, yMaxCS, zMaxCS,     0,   0,   0) ' void entire lattice

  drawCube (0,   1, 0, 1,   i, 0, i,     0,   0, 255) ' blue face
  drawCube (0,   0, 1, 1,   0, i, i,   255,   0,   0) ' red face
  drawCube (0,   1, 1, 0,   i, i, 0,   255, 255, 255) ' white face
  drawCube (0,   m, 1, 1,   m, i, i,   255, 165,   0) ' orange face
  drawCube (0,   1, m, 1,   i, m, i,     0, 255,   0) ' green face
  drawCube (0,   1, 1, m,   i, i, m,   255, 255,   0) ' yellow face

  If innerMargin = 1 Then
    drawCube (0,       sliceWidth + 1, 0, 0,        sliceWidth + 1, m, m,   0, 0, 0)
    drawCube (0,   2 * sliceWidth + 2, 0, 0,    2 * sliceWidth + 2, m, m,   0, 0, 0)
    drawCube (0,   0,     sliceWidth + 1, 0,    m,     sliceWidth + 1, m,   0, 0, 0)
    drawCube (0,   0, 2 * sliceWidth + 2, 0,    m, 2 * sliceWidth + 2, m,   0, 0, 0)
    drawCube (0,   0, 0,     sliceWidth + 1,    m, m,     sliceWidth + 1,   0, 0, 0)
    drawCube (0,   0, 0, 2 * sliceWidth + 2,    m, m, 2 * sliceWidth + 2,   0, 0, 0)
  End If

' Execute all slice movements and cube rotations coded in the maneuver

  For m = 0 to mMax
    makeMove(Maneuver(m))
  Next

' Pause for an interval to permit the solution to be reviewed

  copyFrame(Options.Pause)

  If currentFrame = frameMaxCS Or cs.checkTermination() = True Then Return

' Reverse the maneuver to restore the Rubik Cube to its starting configuration

  For m = mMax to 0 Step -1
    Move = Maneuver(m)
    i = Move.IndexOf("'")

'   If the move contains a 'reverse' notation, remove it to undo the move...

    If i >= 0 Then
      Move = Move.Replace("'", "")

'   otherwise add a 'reverse' notation to undo the move

    Else
      Move = Move & "'"
    End If

    makeMove(Move)

    If currentFrame = frameMaxCS Or cs.checkTermination() = True Then Return

  Next

' Pause for an interval to permit the solution to be reviewed

  copyFrame(Options.Pause)

Catch ex as Exception

  System.Windows.Forms.MessageBox.Show (System.Windows.Forms.Form.ActiveForm,      _
                                        ex.ToString,                               _
                                        "RubiCube.VB CubeSense Animation Script",  _
                                        System.Windows.Forms.MessageBoxButtons.OK, _
                                        System.Windows.Forms.MessageBoxIcon.Stop)
End Try

End Sub

'-------------------------------------------------------------------------------

Private Sub makeMove(Move As String)

' Validate the Move parameter passed to the subroutine and rotate cube slice(s) accordingly

  If Move.Length > 0 Then ' determine the slice(s) and direction of the move
    Movement  = Move.Substring (0, 1) ' first position defines the face turn, slice move or cube rotation
    MoveIndex = ValidMoves.IndexOf(Movement)

    If MoveIndex > 8 Then        ' rotate entire cube about axis
      Lo = 1
      Hi = cubeHiBound - 1
    ElseIf MoveIndex > 5 Then    ' rotate a slice farthest from the origin
      Lo = cubeHiBound - sliceWidth
      Hi = cubeHiBound - 1
    ElseIf MoveIndex > 2 Then    ' rotate a middle slice
      Lo = 1 + sliceWidth + innerMargin
      Hi = Lo + sliceWidth - 1
    ElseIf MoveIndex >= 0 Then   ' rotate a slice closest to the origin
      Lo = 1
      Hi = Lo + sliceWidth - 1
    Else                         ' an invalid Move parameter was passed - ignore
      Return
    End If

'   If the movement parameter contains additional notation(s), interpret them and apply
'   their function to the movement of the slice(s)

    If Move.Length > 1 Then

'     If an anti-clockwise rotation is indicated, set the Direction value accordingly ("-"),
'     otherwise use the default value of clockwise ("+")

      If Move.IndexOf("'") >= 0 Then
        Direction = "-" ' counter-clockwise
      Else
        Direction = "+" ' clockwise (default)
      End If

'     If a 180 degree rotation is indicated, set the Cycles value accordingly (2),
'     otherwise use the default value of clockwise (1)

      If Move.IndexOf("2") >= 0 Then
        Cycles = 2 ' 180 degree rotation (2 x default)
      Else
        Cycles = 1 ' 90 degree rotation (default)
      End If

'     If the adjacent middle layer is also to be rotated in addition to an outside slice,
'     extend the range of voxels defining the slice width

      If Move.IndexOf("w") >= 0 Then
        If MoveIndex < 3 Then ' a slice closest to the origin
          Hi = 4
        ElseIf MoveIndex > 5 And  MoveIndex < 9 Then ' a slice farthest from the origin
          Lo = 3
        End If
      End If

'   If the movement parameter contains no additional notation(s), apply the default
'   actions to the movement of the slice(s)

    Else
      Direction = "+"
      Cycles = 1
    End If

' If no parameter string was passed to the subroutine, then return without performing
' any action.

  Else
    Return
  End If

' When the movement parameters indicate a clockwise rotation (from the front-right
' face perspective), set key processing variables so as to produce that result

  If "RxDEFSz".IndexOf(Movement) >= 0 And Direction = "+" Or _
     "LMUyB".IndexOf(Movement) >= 0 And Direction = "-" Then

    FwdStart = cubeHiBound
    FwdEnd   = 1
    RevStart = 0
    RevEnd   = cubeHiBound - 1
    Delta    = 1

' When the movement parameters indicate a counter-clockwise rotation (from the front-
' right face perspective), set key processing variables so as to produce that result

  ElseIf "RxDEFSz".IndexOf(Movement) >= 0 And Direction = "-" Or _
         "LMUyB".IndexOf(Movement) >= 0 And Direction = "+" Then

    FwdStart = 0
    FwdEnd   = cubeHiBound - 1
    RevStart = cubeHiBound
    RevEnd   = 1
    Delta    = -1
  End If

' Set processing parameters to indicate which, if any, of the outer side faces
' are to be rotated in addition to the slice(s) on each of the 4 cube faces

  If MoveIndex < 3 Or MoveIndex > 8 Then
    OutSideStart = 0           ' one side face is closest to origin
  Else
    OutSideStart = cubeHiBound ' one side face may be farthest from origin
  End If

  If MoveIndex > 5 Then
    OutSideEnd = cubeHiBound   ' one side face is farthest from origin
  Else
    OutSideEnd = 0             ' one side face may be closest to origin
  End If

' Invoke the appropriate subroutine to process the type of movement required

  If MoveIndex Mod 3 = 0 Then
     LMRx ' Process L, M, R and x movements
  ElseIf (MoveIndex - 1) Mod 3 = 0 Then
     DEUy ' Process D, E, U and y movements
  ElseIf (MoveIndex - 2) Mod 3 = 0 Then
     FSBz ' Process F, S, B and z movements
  End If

End Sub

'-------------------------------------------------------------------------------

Private Sub copyFrame(copyCount As Integer)

' copyFrame: subroutine to 'clone' the current CubeSense frame as a new frame
'            'copyCount' number of times and set the 'currentFrame' variable
'            to point at the most recent copy

Dim x, y, z, i           As Integer

For i = 1 To copyCount
  If currentFrame = frameMaxCS Or cs.checkTermination() = True Then Return

  previousFrame = currentFrame
  currentFrame  = currentFrame + 1

  cs.showProgress(CInt(100 * currentFrame / frameMaxCS))

  For z = 0 To zMaxCS
    For y = 0 To yMaxCS
      For x = 0 To xMaxCS
        drawVoxel(currentFrame, x, y, z, _
                  csLattice.R(previousFrame, x, y, z), _
                  csLattice.G(previousFrame, x, y, z), _
                  csLattice.B(previousFrame, x, y, z))
      Next
    Next
  Next
Next

End Sub

'-------------------------------------------------------------------------------

Private Sub LMRx

' Generate the CubeSense frames reflecting the movement requested by the parameter
' string passed in the invoking call.

  For j = 1 to Cycles
    f = currentFrame

    For i = 1 To cubeHiBound ' incremental steps to produce a 90-degree rotation

'     Repeat the previously generated frame a number of times equal to the 'Delay'
'     value set in the execution options

      copyFrame(Options.Delay)
      If currentFrame = frameMaxCS Then Return

'
'     Left & Right Side(s)
'
'     Generate incremental movement for either outer side that needs to be rotated

      For y = OutSideStart to OutSideEnd Step cubeHiBound

'       Erase the entire side face before placing new content on it

        drawCube(currentFrame, 1, y, 1, cubeHiBound - 1, y, cubeHiBound - 1, 0, 0, 0)

'       Calculate the angle (in radians) of the next rotational movement of the side face

        a2 = Delta * .5 * System.Math.PI * i / cubeHiBound

'       Illuminate voxels on the side face to reflect the next incremental rotation of the
'       face content

        For z1 = 0 To cubeHiBound
          For x1 = 0 To cubeHiBound
            a1 = Math.Atan2(x1 - Center, z1 - Center)
            If x1 <> Center Then
              l = (x1 - Center) / Math.Sin(a1)
            Else
              l = (z1 - Center) / Math.Cos(a1)
            End If
            xx = RoundNum(l * Math.Sin (a1 + a2) + Center)
            zz = RoundNum(l * Math.Cos (a1 + a2) + Center)
            If xx >= 0 And xx <= cubeHiBound And zz >= 0 And zz <= cubeHiBound Then
              x = CInt(x1)
              z = CInt(z1)
              drawVoxel(currentFrame, xx, y, zz, _
                        csLattice.R(f, x, y, z), _
                        csLattice.G(f, x, y, z), _
                        csLattice.B(f, x, y, z))
            End If
          Next ' For x1 = 0 To cubeHiBound
        Next ' For z1 = 0 To cubeHiBound
      Next ' For y = OutSideStart to OutSideEnd Step cubeHiBound

'
'     Front Face
'
'     Move the slice colors on this face forward one unit in the direction indicated
'     by the movement parameter

      x = 0
      For z = FwdStart To FwdEnd Step -Delta
        For y = Lo To Hi
          drawVoxel(currentFrame, x, y, z, _
                    csLattice.R(previousFrame, x, y, z - Delta), _
                    csLattice.G(previousFrame, x, y, z - Delta), _
                    csLattice.B(previousFrame, x, y, z - Delta))
        Next
      Next

'     Move the slice colors from the adjacent face forward to this face one unit in
'     the direction indicated by the movement parameter

      For y = Lo to Hi
        drawVoxel(currentFrame, x, y, z, _
                  csLattice.R(previousFrame, x + 1, y, z), _
                  csLattice.G(previousFrame, x + 1, y, z), _
                  csLattice.B(previousFrame, x + 1, y, z))
      Next

'
'     Top Face
'
'     Move the slice colors on this face forward one unit in the direction indicated
'     by the movement parameter

      z = cubeHiBound
      For x = FwdStart To FwdEnd Step -Delta
        For y = Lo to Hi
          drawVoxel(currentFrame, x, y, z, _
                    csLattice.R(previousFrame, x - Delta, y, z), _
                    csLattice.G(previousFrame, x - Delta, y, z), _
                    csLattice.B(previousFrame, x - Delta, y, z))
        Next
      Next

'     Move the slice colors from the adjacent face forward to this face one unit in
'     the direction indicated by the movement parameter

      For y = Lo to Hi
        drawVoxel(currentFrame, x, y, z, _
                  csLattice.R(previousFrame, x, y, z - 1), _
                  csLattice.G(previousFrame, x, y, z - 1), _
                  csLattice.B(previousFrame, x, y, z - 1))
      Next

'
'     Back Face
'
'     Move the slice colors on this face forward one unit in the direction indicated
'     by the movement parameter

      x = cubeHiBound
      For z = RevStart To RevEnd Step Delta
        For y = Lo To Hi
          drawVoxel(currentFrame, x, y, z, _
                    csLattice.R(previousFrame, x, y, z + Delta), _
                    csLattice.G(previousFrame, x, y, z + Delta), _
                    csLattice.B(previousFrame, x, y, z + Delta))
        Next
      Next

'     Move the slice colors from the adjacent face forward to this face one unit in
'     the direction indicated by the movement parameter

      For y = Lo to Hi
        drawVoxel(currentFrame, x, y, z, _
                  csLattice.R(previousFrame, x - 1, y, z), _
                  csLattice.G(previousFrame, x - 1, y, z), _
                  csLattice.B(previousFrame, x - 1, y, z))
      Next

'
'     Bottom Face
'
'     Move the slice colors on this face forward one unit in the direction indicated
'     by the movement parameter

      z = 0
      For x = RevStart To RevEnd Step Delta
        For y = Lo to Hi
          drawVoxel(currentFrame, x, y, z, _
                    csLattice.R(previousFrame, x + Delta, y, z), _
                    csLattice.G(previousFrame, x + Delta, y, z), _
                    csLattice.B(previousFrame, x + Delta, y, z))
         Next
      Next

'     Move the slice colors from the adjacent face forward to this face one unit in
'     the direction indicated by the movement parameter

      For y = Lo to Hi
        drawVoxel(currentFrame, x, y, z, _
                  csLattice.R(previousFrame, x, y, z + 1), _
                  csLattice.G(previousFrame, x, y, z + 1), _
                  csLattice.B(previousFrame, x, y, z + 1))
      Next

    Next ' for i = 1 to cubeHiBound
  Next ' for j = 1 to Cycles

End Sub

'-------------------------------------------------------------------------------

Private Sub DEUy

' Generate the CubeSense frames reflecting the movement requested by the parameter
' string passed in the invoking call.

  For j = 1 to Cycles
    f = currentFrame

    For i = 1 To cubeHiBound ' incremental steps to produce a 90-degree rotation

'     Repeat the previously generated frame a number of times equal to the 'Delay'
'     value set in the execution options

      copyFrame(Options.Delay)
      If currentFrame = frameMaxCS Then Return

'
'     Top & Bottom Side(s)
'
'     Generate incremental movement for either outer side that needs to be rotated

      For z = OutSideStart to OutSideEnd Step cubeHiBound

'       Erase the entire side before placing new content on it

        drawCube(currentFrame, 1, 1, z, cubeHiBound - 1, cubeHiBound - 1, z, 0, 0, 0)

'       Calculate the angle (in radians) of the next rotational movement of the side face

        a2 = Delta * .5 * System.Math.PI * i / cubeHiBound

'       Illuminate voxels on the side face to reflect the next incremental rotation of the
'       face content

        For x1 = 0 To cubeHiBound
          For y1 = 0 To cubeHiBound
            a1 = Math.Atan2(x1 - Center, y1 - Center)
            If x1 <> Center Then
              l = (x1 - Center) / Math.Sin(a1)
            Else
              l = (y1 - Center) / Math.Cos(a1)
            End If
            xx = RoundNum(l * Math.Sin (a1 + a2) + Center)
            yy = RoundNum(l * Math.Cos (a1 + a2) + Center)
            If xx >= 0 And xx <= cubeHiBound And yy >= 0 And yy <= cubeHiBound Then
              x = CInt(x1)
              y = CInt(y1)
              drawVoxel(currentFrame, xx, yy, z, _
                        csLattice.R(f, x, y, z), _
                        csLattice.G(f, x, y, z), _
                        csLattice.B(f, x, y, z))
            End If
          Next ' For x1 = 0 To cubeHiBound
        Next ' For y1 = 0 To cubeHiBound
      Next ' For z = OutSideStart to OutSideEnd Step cubeHiBound

'
'     Front Face
'
'     Move the slice colors on this face forward one unit in the direction indicated
'     by the movement parameter

      x = 0
      For y = FwdStart To FwdEnd Step -Delta
        For z = Lo To Hi
          drawVoxel(currentFrame, x, y, z, _
                    csLattice.R(previousFrame, x, y - Delta, z), _
                    csLattice.G(previousFrame, x, y - Delta, z), _
                    csLattice.B(previousFrame, x, y - Delta, z))
        Next
      Next

'     Move the slice colors from the adjacent face forward to this face one unit in
'     the direction indicated by the movement parameter

      For z = Lo to Hi
        drawVoxel(currentFrame, x, y, z, _
                  csLattice.R(previousFrame, x + 1, y, z), _
                  csLattice.G(previousFrame, x + 1, y, z), _
                  csLattice.B(previousFrame, x + 1, y, z))
      Next

'
'     Right Face
'
'     Move the slice colors on this face forward one unit in the direction indicated
'     by the movement parameter

      y = cubeHiBound
      For x = FwdStart To FwdEnd Step -Delta
        For z = Lo to Hi
          drawVoxel(currentFrame, x, y, z, _
                    csLattice.R(previousFrame, x - Delta, y, z), _
                    csLattice.G(previousFrame, x - Delta, y, z), _
                    csLattice.B(previousFrame, x - Delta, y, z))
        Next
      Next

'     Move the slice colors from the adjacent face forward to this face one unit in
'     the direction indicated by the movement parameter

      For z = Lo to Hi
        drawVoxel(currentFrame, x, y, z, _
                  csLattice.R(previousFrame, x, y - 1, z), _
                  csLattice.G(previousFrame, x, y - 1, z), _
                  csLattice.B(previousFrame, x, y - 1, z))
      Next

'
'     Back Face
'
'     Move the slice colors on this face forward one unit in the direction indicated
'     by the movement parameter

      x = cubeHiBound
      For y = RevStart To RevEnd Step Delta
        For z = Lo To Hi
          drawVoxel(currentFrame, x, y, z, _
                    csLattice.R(previousFrame, x, y + Delta, z), _
                    csLattice.G(previousFrame, x, y + Delta, z), _
                    csLattice.B(previousFrame, x, y + Delta, z))
        Next
      Next

'     Move the slice colors from the adjacent face forward to this face one unit in
'     the direction indicated by the movement parameter

      For z = Lo to Hi
        drawVoxel(currentFrame, x, y, z, _
                  csLattice.R(previousFrame, x - 1, y, z), _
                  csLattice.G(previousFrame, x - 1, y, z), _
                  csLattice.B(previousFrame, x - 1, y, z))
      Next

'
'     Left Face
'
'     Move the slice colors on this face forward one unit in the direction indicated
'     by the movement parameter

      y = 0
      For x = RevStart To RevEnd Step Delta
        For z = Lo to Hi
          drawVoxel(currentFrame, x, y, z, _
                    csLattice.R(previousFrame, x + Delta, y, z), _
                    csLattice.G(previousFrame, x + Delta, y, z), _
                    csLattice.B(previousFrame, x + Delta, y, z))
         Next
      Next

'     Move the slice colors from the adjacent face forward to this face one unit in
'     the direction indicated by the movement parameter

      For z = Lo to Hi
        drawVoxel(currentFrame, x, y, z, _
                  csLattice.R(previousFrame, x, y + 1, z), _
                  csLattice.G(previousFrame, x, y + 1, z), _
                  csLattice.B(previousFrame, x, y + 1, z))
      Next

    Next ' for i = 1 to cubeHiBound
  Next ' for j = 1 to Cycles

End Sub

'-------------------------------------------------------------------------------

Private Sub FSBz

' Generate the CubeSense frames reflecting the movement requested by the parameter
' string passed in the invoking call.

  For j = 1 to Cycles
    f = currentFrame

    For i = 1 To cubeHiBound ' incremental steps to produce a 90-degree rotation

'     Repeat the previously generated frame a number of times equal to the 'Delay'
'     value set in the execution options

      copyFrame(Options.Delay)
      If currentFrame = frameMaxCS Then Return

'
'     Front and Back Side(s)
'
'     Generate incremental movement for either outer side that needs to be rotated

      For x = OutSideStart to OutSideEnd Step cubeHiBound

'       Erase the entire side before placing new content on it

        drawCube(currentFrame, x, 1, 1, x, cubeHiBound - 1, cubeHiBound - 1, 0, 0, 0)

'       Calculate the angle (in radians) of the next rotational movement of the side face

        a2 = Delta * .5 * System.Math.PI * i / cubeHiBound

'       Illuminate voxels on the side face to reflect the next incremental rotation of the
'       face content

        For z1 = 0 To cubeHiBound
          For y1 = 0 To cubeHiBound
            a1 = Math.Atan2(y1 - Center, z1 - Center)
            If y1 <> Center Then
              l = (y1 - Center) / Math.Sin(a1)
            Else
              l = (z1 - Center) / Math.Cos(a1)
            End If
            yy = RoundNum(l * Math.Sin (a1 + a2) + Center)
            zz = RoundNum(l * Math.Cos (a1 + a2) + Center)
            If yy >= 0 And yy <= cubeHiBound And zz >= 0 And zz <= cubeHiBound Then
              y = CInt(y1)
              z = CInt(z1)
              drawVoxel(currentFrame, x, yy, zz, _
                        csLattice.R(f, x, y, z), _
                        csLattice.G(f, x, y, z), _
                        csLattice.B(f, x, y, z))
            End If
          Next ' For y1 = 0 To cubeHiBound
        Next ' For z1 = 0 To cubeHiBound
      Next ' For x = OutSideStart to OutSideEnd Step cubeHiBound

'
'     Left Face
'
'     Move the slice colors on this face forward one unit in the direction indicated
'     by the movement parameter

      y = 0
      For z = FwdStart To FwdEnd Step -Delta
        For x = Lo To Hi
          drawVoxel(currentFrame, x, y, z, _
                    csLattice.R(previousFrame, x, y, z - Delta), _
                    csLattice.G(previousFrame, x, y, z - Delta), _
                    csLattice.B(previousFrame, x, y, z - Delta))
        Next
      Next

'     Move the slice colors from the adjacent face forward to this face one unit in
'     the direction indicated by the movement parameter

      For x = Lo to Hi
        drawVoxel(currentFrame, x, y, z, _
                  csLattice.R(previousFrame, x, y + 1, z), _
                  csLattice.G(previousFrame, x, y + 1, z), _
                  csLattice.B(previousFrame, x, y + 1, z))
      Next

'
'     Top Face
'
'     Move the slice colors on this face forward one unit in the direction indicated
'     by the movement parameter

      z = cubeHiBound
      For y = FwdStart To FwdEnd Step -Delta
        For x = Lo to Hi
          drawVoxel(currentFrame, x, y, z, _
                    csLattice.R(previousFrame, x, y - Delta, z), _
                    csLattice.G(previousFrame, x, y - Delta, z), _
                    csLattice.B(previousFrame, x, y - Delta, z))
        Next
      Next

'     Move the slice colors from the adjacent face forward to this face one unit in
'     the direction indicated by the movement parameter

      For x = Lo to Hi
        drawVoxel(currentFrame, x, y, z, _
                  csLattice.R(previousFrame, x, y, z - 1), _
                  csLattice.G(previousFrame, x, y, z - 1), _
                  csLattice.B(previousFrame, x, y, z - 1))
      Next

'
'     Right Face
'
'     Move the slice colors on this face forward one unit in the direction indicated
'     by the movement parameter

      y = cubeHiBound
      For z = RevStart To RevEnd Step Delta
        For x = Lo To Hi
          drawVoxel(currentFrame, x, y, z, _
                    csLattice.R(previousFrame, x, y, z + Delta), _
                    csLattice.G(previousFrame, x, y, z + Delta), _
                    csLattice.B(previousFrame, x, y, z + Delta))
        Next
      Next

'     Move the slice colors from the adjacent face forward to this face one unit in
'     the direction indicated by the movement parameter

      For x = Lo to Hi
        drawVoxel(currentFrame, x, y, z, _
                  csLattice.R(previousFrame, x, y - 1, z), _
                  csLattice.G(previousFrame, x, y - 1, z), _
                  csLattice.B(previousFrame, x, y - 1, z))
      Next

'
'     Bottom Face
'
'     Move the slice colors on this face forward one unit in the direction indicated
'     by the movement parameter

      z = 0
      For y = RevStart To RevEnd Step Delta
        For x = Lo to Hi
          drawVoxel(currentFrame, x, y, z, _
                    csLattice.R(previousFrame, x, y + Delta, z), _
                    csLattice.G(previousFrame, x, y + Delta, z), _
                    csLattice.B(previousFrame, x, y + Delta, z))
         Next
      Next

'     Move the slice colors from the adjacent face forward to this face one unit in
'     the direction indicated by the movement parameter

      For x = Lo to Hi
        drawVoxel(currentFrame, x, y, z, _
                  csLattice.R(previousFrame, x, y, z + 1), _
                  csLattice.G(previousFrame, x, y, z + 1), _
                  csLattice.B(previousFrame, x, y, z + 1))
      Next

    Next ' for i = 1 to cubeHiBound
  Next ' for j = 1 to Cycles

End Sub

'-------------------------------------------------------------------------------

Private Sub drawVoxel(f As Integer, _
                      x As Integer, _
                      y As Integer, _
                      z As Integer, _
                      r As Integer, _
                      g As Integer, _
                      b As Integer)

  csLattice.R(f, x, y, z) = r
  csLattice.G(f, x, y, z) = g
  csLattice.B(f, x, y, z) = b

  cs.drawVoxel(f, x, y, z, r, g, b)

End Sub

'-------------------------------------------------------------------------------

Private Sub drawCube(f  As Integer, _
                     x1 As Integer, _
                     y1 As Integer, _
                     z1 As Integer, _
                     x2 As Integer, _
                     y2 As Integer, _
                     z2 As Integer, _
                     r  As Integer, _
                     g  As Integer, _
                     b  As Integer)

  Dim x, y, z           As Integer
  Dim xIncr             As Integer = 1
  Dim yIncr             As Integer = 1
  Dim zIncr             As Integer = 1

  If x2 < x1 Then xIncr = -1
  If y2 < y1 Then yIncr = -1
  If z2 < z1 Then zIncr = -1

  For z = z1 To z2 Step zIncr
    For y = y1 To y2 Step yIncr
      For x = x1 To x2 Step xIncr
        drawVoxel (f, x, y, z, r, g, b)
      Next
    Next
  Next

End Sub

'-------------------------------------------------------------------------------

Private Function RoundNum (Number as Double) As Integer

' Calculate the rounded integer value of any Number

  If Number < 0 Then
    RoundNum = System.Math.Ceiling (Number - .5)
  Else
    RoundNum = System.Math.Floor (Number + .5)
  End If

End Function