Public Class Form1
    Public Structure MainValues
        ' SET_VALUE
        Public Set_SV As Double
        Public Set_Door1 As Integer
        Public Set_Door2 As Integer
        Public Set_Crush1 As Integer
        Public Set_Crush2 As Boolean
        Public Set_Val1 As Integer
        Public Set_Val2 As Integer
        ' GET_VALUE
        Public Get_PV As Double
        Public Get_DrPosUp As Integer
        Public Get_DrPosDw As Integer
        ' PROCESS_VALUE
        Public Now_TotalTime As TimeSpan
        Public Now_CurrentTime As TimeSpan
        Public Now_node(,) As Double
        Public OpenVA As TimeSpan
        Public OpenVB As TimeSpan
        Public CloseVA As TimeSpan
        Public CloseVB As TimeSpan
    End Structure
    Public MValues As New MainValues()

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        newComm()
    End Sub

    Private Sub newComm()
        Dim TXRX As New CLS_COM_SERVICE(Me)
        TXRX.Location = New Point(20, 20)
        TXRX.Width = 410
        TXRX.Height = 150
        Me.Controls.Add(TXRX)
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        MValues.Set_Crush2 = 1
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        MValues.Set_Crush2 = 0
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        MValues.Set_Crush2 = 2
    End Sub
End Class
