Imports System.IO.Ports

Public Class CLS_COM_SERVICE
    Inherits UserControl
    Private mainForm As Form1

    Public Temp_SV As Double
    Public Temp_Door1 As Integer
    Public Temp_Door2 As Integer
    Public Temp_Crush1 As Integer
    Public Temp_Crush2 As Boolean
    Public Temp_Val1 As Integer
    Public Temp_Val2 As Integer

    Sub New(Form As Form1)
        mainForm = Form
        UICreator()
        InitialPort()
    End Sub


#Region "PortServices"
    Private WithEvents ReportBox As New System.Windows.Forms.GroupBox()
    Private WithEvents ReportList As New System.Windows.Forms.RichTextBox()
    Private WithEvents SensorList As New System.Windows.Forms.Label()
    Private WithEvents StatusPanel As New Panel
    Private Sub UICreator()
        ReportBox.Width = 400
        ReportBox.Height = 130
        ReportBox.Text = "Communications"
        Me.Controls.Add(ReportBox)

        ReportList.Location = New Point(20, 20)
        ReportList.Width = 250
        ReportList.Height = 100
        ReportList.ForeColor = System.Drawing.Color.DarkGray
        ReportBox.Controls.Add(ReportList)

        SensorList.Location = New Point(280, 20)
        SensorList.Width = 100
        SensorList.Height = 80
        SensorList.Text = "Sensor"
        SensorList.ForeColor = System.Drawing.Color.DarkGray
        ReportBox.Controls.Add(SensorList)

        ' Setup the status panel for blinking status indicator
        StatusPanel.Location = New Point(380, 100)
        StatusPanel.Width = 10
        StatusPanel.Height = 10
        StatusPanel.BackColor = System.Drawing.Color.White
        ReportBox.Controls.Add(StatusPanel)

    End Sub

    Private StatusLogic As Boolean
    Private Sub SensorReport(ByVal PV_Re As Double, ByVal doorUp As Integer, ByVal doorDw As Integer)

        If Me.InvokeRequired Then
            Me.Invoke(New Action(Of Double, Integer, Integer)(AddressOf SensorReport), PV_Re, doorUp, doorDw)
        Else
            StatusLogic = Not StatusLogic
            StatusPanel.BackColor = If(StatusLogic, System.Drawing.Color.Green, System.Drawing.Color.White)
            Dim Report As String = "PV : " & PV_Re.ToString("F2")
            Report &= vbLf  ''-------- New line
            Select Case doorUp
                Case 0
                    Report &= ("Top closed")
                Case 1
                    Report &= ("Top slightly")
                Case 2
                    Report &= ("Top open")
                Case Else
                    Report &= ("Invalid")
            End Select
            Report &= vbLf ''-------- New line
            Select Case doorDw
                Case 0
                    Report &= ("Bottom closed")
                Case 1
                    Report &= ("Bottom slightly")
                Case 2
                    Report &= ("Bottom open")
                Case Else
                    Report &= ("Invalid")
            End Select
            'Report &= vbLf ''-------- New line
            'For Each portName As String In initialPortNames ' Repot all name of port is connected
            '    Report &= portName.ToString & vbLf
            'Next
            SensorList.Text = Report
        End If
    End Sub

    Private Sub AddReportText(reportText As String)
        If Me.InvokeRequired Then
            Me.Invoke(New Action(Of String)(AddressOf AddReportText), reportText)
        Else
            ' Append the new text to the RichTextBox
            ReportList.AppendText(reportText & Environment.NewLine)
            ' Scroll to the end of the RichTextBox
            ReportList.SelectionStart = ReportList.Text.Length
            ReportList.ScrollToCaret()
        End If
    End Sub


    Dim serialPorts(4) As SerialPort
    Private initialPortNames As String() ' Store initial port names
    Private WithEvents TimPortChk As New Timer()
    Private Sub UpdatePortName(sender As Object, e As EventArgs) Handles TimPortChk.Tick
        Dim currentPortNames As String() = SerialPort.GetPortNames()
        If Not currentPortNames.SequenceEqual(initialPortNames) Then ' Comparing the port has changed
            For Each portName As String In currentPortNames ' Repot all name of port is connected
                AddReportText(portName)
            Next
            InitialPort()
        End If
    End Sub

    Public Sub InitialPort()
        Shutdown()
        '' WeakUp the Timer---------------
        If TimPortChk.Enabled = False Then
            TimPortChk.Interval = 300
            TimPortChk.Start()
        End If
        If TimTX.Enabled = False Then
            TimTX.Interval = 1000
            TimTX.Start()
        End If
        '-----------------------------------
        Dim N As Integer = 0
        For Each sp As String In SerialPort.GetPortNames()
            serialPorts(N) = New SerialPort With {
                .PortName = sp,
                .BaudRate = 9600,
                .WriteTimeout = 300
            }
            'Console.WriteLine(sp)
            Try
                serialPorts(N).Open()
                AddHandler serialPorts(N).DataReceived, AddressOf SerialDataReceivedHandler ' Add an event handler for the DataReceived event
            Catch ex As Exception
                AddReportText(ex.Message)
            End Try
            N += 1
        Next
        initialPortNames = SerialPort.GetPortNames()
        AddReportText("INITIAL PORT IS COMPLETED")
    End Sub

    Public Sub Shutdown()
        For Each port As SerialPort In serialPorts
            port?.Close()
        Next
        'The If port IsNot Nothing Then check has been replaced with the more succinct port?.Close().
    End Sub
    Private Sub SetFontForControl(ctrl As System.Windows.Forms.Control, fontName As String, fontSize As Single, fontStyle As System.Drawing.FontStyle)
        Dim newFont As New System.Drawing.Font(fontName, fontSize, fontStyle)
        ctrl.Font = newFont
    End Sub
#End Region


#Region "Reciver"

    'Auto Recive data from x4 Serial
    Private Sub SerialDataReceivedHandler(sender As Object, e As SerialDataReceivedEventArgs)
        Dim port As SerialPort = DirectCast(sender, SerialPort)
        Dim InputString As String = port.ReadLine()
        Try
            port.DiscardInBuffer()
        Catch ex As Exception

        End Try
        'Console.WriteLine(InputString)
        Dim cleanedString As String = InputString.Trim("$"c, "#"c) ' Remove the leading "$$" and trailing "##"
        Dim lastHashIndex As Integer = cleanedString.LastIndexOf("#"c)

        If lastHashIndex > -1 Then ' Can read all data / if not maybe  lastHashIndex = -1 error
            cleanedString = cleanedString.Substring(0, lastHashIndex)
            Dim parts() As String = cleanedString.Split(","c) ' Split the cleaned string by the comma

            If parts.Length = 2 Then
                Dim id As String = parts(0)
                Dim value As Double

                'AddReportText(id & "," & value)
                If Double.TryParse(parts(1), value) Then
                    Dim idToProperty As New Dictionary(Of String, Action(Of Double)) From
                   {
                    {"PV", Sub(v) mainForm.MValues.Get_PV = v},
                    {"D1", Sub(v) mainForm.MValues.Get_DrPosUp = CInt(v)},
                    {"D2", Sub(v) mainForm.MValues.Get_DrPosDw = CInt(v)},
                    {"CA", Sub(v) AddReportText("FFFGGG")}
                   }
                    If idToProperty.ContainsKey(id) Then
                        idToProperty(id).Invoke(value)
                    End If
                Else
                    'AddReportText("Log parsing error")
                End If
            Else
                'AddReportText("Invalid input format.")
            End If
        End If
        SensorReport(mainForm.MValues.Get_PV, mainForm.MValues.Get_DrPosUp, mainForm.MValues.Get_DrPosDw)
    End Sub

#End Region


#Region "Transmitter"
    Private WithEvents TimTX As New Timer()
    Private Sub Timer_TX(sender As Object, e As EventArgs) Handles TimTX.Tick
        If mainForm.MValues.Set_SV <> Temp_SV Then
            Temp_SV = mainForm.MValues.Set_SV
            PortSender("$SV," & Temp_SV & "#")
        End If
        If mainForm.MValues.Set_Crush1 <> Temp_Crush1 Then
            Temp_Crush1 = mainForm.MValues.Set_Crush1
            PortSender("$CA," & Temp_Crush1 & "#")
            MsgBox(Temp_Crush1)
        End If
        If mainForm.MValues.Set_Door1 <> Temp_Door1 Then
            Temp_Door1 = mainForm.MValues.Set_Door1
            PortSender("$D1," & Temp_Door1 & "#")
        End If
        If mainForm.MValues.Set_Door2 <> Temp_Door2 Then
            Temp_Door2 = mainForm.MValues.Set_Door2
            PortSender("$D2," & Temp_Door2 & "#")
        End If

        If mainForm.MValues.Set_Val1 <> Temp_Val1 Then
            Temp_Val1 = mainForm.MValues.Set_Val1
            PortSender("$VA," & Temp_Val1 & "#")
        End If
        If mainForm.MValues.Set_Val2 <> Temp_Val2 Then
            Temp_Val2 = mainForm.MValues.Set_Val2
            PortSender("$VB," & Temp_Val2 & "#")
        End If



    End Sub

    Private Sub PortSender(ByVal TX As String)
        For Each port As SerialPort In serialPorts
            If port IsNot Nothing Then
                If port.IsOpen Then
                    Try
                        port.WriteLine(TX)
                        AddReportText(port.PortName & ": " & TX)
                    Catch ex As Exception
                        AddReportText(ex.Message)
                    End Try
                End If
            End If
        Next
    End Sub

#End Region



End Class
