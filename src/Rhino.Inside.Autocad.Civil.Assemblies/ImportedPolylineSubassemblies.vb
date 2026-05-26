Option Explicit On
Option Strict Off

Imports System.Globalization
Imports System.IO

Public Class ImportedPolylineSubassemblies
    Inherits SATemplate

    ' Input parameters (class-level fields matching stock pattern)
    Private Const CsvPathDefault As String = ""
    Private Const PointCodeDefault As String = "Shape"
    Private Const LinkCodeDefault As String = "Top"
    Private Const ClosedDefault As Boolean = False

    Protected Overrides Sub GetInputParametersImplement(ByVal corridorState As CorridorState)
        MyBase.GetInputParametersImplement(corridorState)

        Dim paramsString As ParamStringCollection = corridorState.ParamsString
        Dim paramsLong As ParamLongCollection = corridorState.ParamsLong

        paramsString.Add("CsvPath", CsvPathDefault)
        paramsString.Add("PointCode", PointCodeDefault)
        paramsString.Add("LinkCode", LinkCodeDefault)
        paramsLong.Add("Closed", If(ClosedDefault, 1, 0))
    End Sub

    Protected Overrides Sub DrawImplement(ByVal corridorState As CorridorState)
        Dim paramsString As ParamStringCollection = corridorState.ParamsString
        Dim paramsLong As ParamLongCollection = corridorState.ParamsLong

        ' Get parameters
        Dim csvPath As String = ""
        Dim pointCode As String = PointCodeDefault
        Dim linkCode As String = LinkCodeDefault
        Dim closed As Boolean = ClosedDefault

        Try : csvPath = paramsString.Value("CsvPath") : Catch : End Try
        Try : pointCode = paramsString.Value("PointCode") : Catch : End Try
        Try : linkCode = paramsString.Value("LinkCode") : Catch : End Try
        Try : closed = (paramsLong.Value("Closed") = 1) : Catch : End Try

        ' Validate CSV path
        If String.IsNullOrEmpty(csvPath) OrElse Not File.Exists(csvPath) Then
            Utilities.RecordWarning(corridorState, CorridorError.Failure,
                "CSV file path is empty or file does not exist.", "ImportedPolylineSubassemblies")
            Exit Sub
        End If

        ' Read and parse CSV
        Dim vertices As New List(Of Point2d)
        Dim lines() As String = File.ReadAllLines(csvPath)

        For Each line As String In lines
            Dim trimmed As String = line.Trim()
            If String.IsNullOrEmpty(trimmed) OrElse trimmed.StartsWith("#") Then Continue For

            Dim parts() As String = trimmed.Split(","c)
            If parts.Length < 2 Then Continue For

            Dim x As Double, y As Double
            If Double.TryParse(parts(0).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, x) AndAlso
               Double.TryParse(parts(1).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, y) Then
                vertices.Add(New Point2d(x, y))
            End If
        Next

        ' Validate vertex count
        If vertices.Count < 2 Then
            Utilities.RecordWarning(corridorState, CorridorError.Failure,
                "CSV must contain at least 2 valid vertices.", "ImportedPolylineSubassemblies")
            Exit Sub
        End If

        ' Create points
        Dim corridorPoints As PointCollection = corridorState.Points
        Dim pointList As New List(Of Point)

        For Each vertex As Point2d In vertices
            Dim pt As Point = corridorPoints.Add(vertex.X, vertex.Y, "")
            pt.Codes.TryAdd(pointCode)
            pointList.Add(pt)
        Next

        ' Create links
        Dim corridorLinks As LinkCollection = corridorState.Links
        Dim linkPointArray(1) As Point

        For i As Integer = 0 To pointList.Count - 2
            linkPointArray(0) = pointList(i)
            linkPointArray(1) = pointList(i + 1)
            Dim lnk As Link = corridorLinks.Add(linkPointArray, "")
            lnk.Codes.TryAdd(linkCode)
        Next

        ' Close if requested
        If closed AndAlso pointList.Count >= 3 Then
            linkPointArray(0) = pointList(pointList.Count - 1)
            linkPointArray(1) = pointList(0)
            Dim closingLink As Link = corridorLinks.Add(linkPointArray, "")
            closingLink.Codes.TryAdd(linkCode)
        End If
    End Sub
End Class
