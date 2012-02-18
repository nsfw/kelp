
Public paramStepCount            As Integer = 8

Public positionOriginal ()       As Integer
Public positionCurrent ()        As Integer
Public positionTarget ()         As Integer
Public activeCube                As Integer
Public activeCoordinate          As Integer
Public latticeSize (2)           As Integer

Public Sub Main() implements scriptInterface.IScript.Main

  Dim f                          As Integer

  ReDim positionOriginal (0 To 8)
  ReDim positionCurrent (0 To 8)
  ReDim positionTarget (0 To 8)

  latticeSize (0) = cs.getSizeX ()
  latticeSize (1) = cs.getSizeY ()
  latticeSize (2) = cs.getSizeZ ()

  initialyze

  For f = 0 To cs.getFrameCount () - 1

    cs.drawCube (f, 0, 0, 0, latticeSize (0), latticeSize (1), latticeSize (2), 0, 0, 0)
    If (f Mod paramStepCount = 0) Then

      positionOriginal (activeCube * 3 + activeCoordinate) = _
        positionTarget (activeCube * 3 + activeCoordinate)
      positionCurrent (activeCube * 3 + activeCoordinate) = _
        positionTarget (activeCube * 3 + activeCoordinate)
      findNewMove

    Else

      positionCurrent (activeCube * 3 + activeCoordinate) = _
        positionOriginal (activeCube * 3 + activeCoordinate) + _
        (positionTarget (activeCube * 3 + activeCoordinate) - _
        positionOriginal (activeCube * 3 + activeCoordinate)) * _
        (f Mod paramStepCount) / paramStepCount

    End If

    displayCube (f, 0)
    displayCube (f, 1)
    displayCube (f, 2)
    cs.showProgress ((f + 1) * 100 / cs.getFrameCount())
  Next

End Sub

Public Sub initialyze() ' "initialize" has special meaning in VB, so subroutine name is different than CS version

  Dim i                   As Integer
  positionOriginal (0) = 0
  positionOriginal (1) = 0
  positionOriginal (2) = 0
  positionOriginal (3) = latticeSize (0) / 2
  positionOriginal (4) = 0
  positionOriginal (5) = latticeSize (2) / 2
  positionOriginal (6) = 0
  positionOriginal (7) = latticeSize (1) / 2
  positionOriginal (8) = latticeSize (2) / 2

  For i = 0 To 8

    positionCurrent (i) = positionOriginal (i)
    positionTarget (i)  = positionOriginal (i)

  Next

  activeCube       = 0
  activeCoordinate = 0

End Sub

Public Sub displayCube (f As Integer, cubeId As Integer)

  Dim r, g, b            As Integer
  r = 0
  b = 0
  g = 0

  If cubeId = 0 Then
    r = 255
  Else If cubeId = 1 Then
    g = 255
  ElseIf cubeId = 2 Then
    b = 255
  End If

  cs.drawCube (f, _
               positionCurrent (cubeId * 3 + 0), _
               positionCurrent (cubeId * 3 + 1), _
               positionCurrent (cubeId * 3 + 2), _
               positionCurrent (cubeId * 3 + 0) + latticeSize (0) / 2 - 1, _
               positionCurrent (cubeId * 3 + 1) + latticeSize (1) / 2 - 1, _
               positionCurrent (cubeId * 3 + 2) + latticeSize (2) / 2 - 1, _
               r, g, b)

End Sub

Public Sub findNewMove ()

  Dim done                As Boolean
  Dim i                   As Integer

  done =  False

  Do While Not done
    If (cs.checkTermination ()) Then _
      Exit Sub

    activeCube       = cs.rnd (3)
    activeCoordinate = cs.rnd (3)

    For i = 0 To 2
      positionTarget (activeCube * 3 + i) = positionOriginal (activeCube * 3 + i)
    Next

    If (positionOriginal (activeCube * 3 + activeCoordinate) = 0) Then
      positionTarget (activeCube * 3 + activeCoordinate) = latticeSize (activeCoordinate) / 2
    Else
      positionTarget (activeCube * 3 + activeCoordinate) = 0
    End If

    done = Not checkIfNoCubeIsNotThere (positionTarget (activeCube * 3 + 0), _
                                        positionTarget (activeCube * 3 + 1), _
                                        positionTarget (activeCube * 3 + 2))
  Loop

End Sub

Public Function checkIfNoCubeIsNotThere (x As Integer, y As Integer, z As Integer) as Boolean

  Dim i                   As Integer
  For i = 0 To 2
    If positionOriginal (i * 3 + 0) = x And _
       positionOriginal (i * 3 + 1) = y And _
       positionOriginal (i * 3 + 2) = z Then
      checkIfNoCubeIsNotThere =  True
      Exit Function
    End If
  Next
  checkIfNoCubeIsNotThere =  False
End Function