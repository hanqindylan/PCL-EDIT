Public Class PclLogger
    Inherits FileLogger

    ''' <inheritdoc/>
    Public Overrides Function Format(Text As String, Level As LogLevel, FilePath As String, Ex As Exception) As String
        Text = MyBase.Format(Text, Level, FilePath, Ex)
        Text = FilterUserName(FilterAccessToken(Text, "*"c), "*"c)
        Return Text
    End Function

    ''' <inheritdoc/>
    Public Overrides Sub HandleBehavior(RawMessage As String, FormattedMessage As String, Behavior As LogBehavior, Ex As Exception)
        If IsProgramEnding Then Return
        MyBase.HandleBehavior(RawMessage, FormattedMessage, Behavior, Ex)
        Dim BriefText = If(Ex Is Nothing, RawMessage, If(RawMessage Is Nothing, "", $"{RawMessage}：") & Ex.GetDisplay(False))
        BriefText = FilterUserName(FilterAccessToken(BriefText, "*"c), "*"c)
        Dim DetailText = If(Ex Is Nothing, RawMessage, If(RawMessage Is Nothing, "", $"{RawMessage}：") & Ex.GetDisplay(True))
        DetailText = FilterUserName(FilterAccessToken(DetailText, "*"c), "*"c)
        Select Case Behavior
            Case LogBehavior.None
                '啥也不干
            Case LogBehavior.ToastIfDebug
                If BuildType = BuildTypes.Debug OrElse ModeDebug Then Hint("[调试模式] " & BriefText, HintType.Blue, False)
            Case LogBehavior.Toast
                Hint(BriefText, HintType.Red, False)
            Case LogBehavior.Alert
                MyMsgBox(ShortenErrorText(DetailText), "错误", IsWarn:=True)
            Case LogBehavior.AlertThenFeedback
                If PageSetupSystem.IsLauncherNewest Then
                    If MyMsgBox(ShortenErrorText(DetailText) & vbCrLf & vbCrLf & "是否反馈此问题？如果不反馈，这个问题可能永远无法得到解决！", "错误", "反馈", "取消", IsWarn:=True) = 1 Then Feedback(False, True)
                Else
                    MyMsgBox(ShortenErrorText(DetailText) & vbCrLf & vbCrLf & "将 PCL 更新至最新版或许可以解决这个问题……", "错误", IsWarn:=True)
                End If
            Case LogBehavior.AlertThenCrash
                Static FirstTrigger As Boolean = True
                If FirstTrigger Then
                    '首次触发
                    FirstTrigger = False
                    If PageSetupSystem.IsLauncherNewest Then
                        If MsgBox(DetailText & vbCrLf & vbCrLf & "是否反馈此问题？如果不反馈，这个问题可能永远无法得到解决！", MsgBoxStyle.Critical + MsgBoxStyle.YesNo, "错误") = MsgBoxResult.Yes Then Feedback(False, True)
                    Else
                        MsgBox(DetailText & vbCrLf & vbCrLf & "将 PCL 更新至最新版或许可以解决这个问题……", MsgBoxStyle.Critical, "错误")
                    End If
                Else
                    '多次触发，直接使程序崩溃（这通常代表着在其他线程循环触发严重异常）
                    Thread.Sleep(2000)
                End If
                FormMain.EndProgramForce(ProcessReturnValues.Exception)
        End Select
        '遥测
        If Behavior >= LogBehavior.Toast Then Telemetry("错误日志", "Exception", DetailText)
    End Sub

    ''' <summary>
    ''' 错误弹窗里的文本过长时，WPF 的自动换行文本框在滚动时会明显卡顿（#9325）。
    ''' 弹窗只保留开头一段，完整内容写入临时文件，并通过按钮打开。
    ''' </summary>
    Private Const ErrorDialogMaxLength As Integer = 4000
    Private Shared Function ShortenErrorText(Text As String) As String
        If Text Is Nothing OrElse Text.Length <= ErrorDialogMaxLength Then Return Text
        Try
            Dim FilePath = RequestTaskTempFolder() & "Error.txt"
            File.WriteAllText(FilePath, Text, Encoding.UTF8)
            RunInUi(Sub()
                        If FrmMain Is Nothing OrElse FrmMain.PanMsg Is Nothing Then Return
                        Dim Buttons = FrmMain.PanMsg.Children.OfType(Of MyMsgText).LastOrDefault()
                        If Buttons Is Nothing Then Return
                        Dim OpenButton As New MyButton With {.Text = "查看完整日志", .Margin = New Thickness(12, 0, 0, 0), .Padding = New Thickness(5, 0, 5, 0)}
                        AddHandler OpenButton.Click, Sub() OpenExplorer(FilePath)
                        Buttons.PanBtn.Children.Insert(0, OpenButton)
                    End Sub)
            Logger.Info($"错误信息过长（{Text.Length} 字符），完整内容已写入：{FilePath}")
        Catch ex As Exception
            Logger.Warn(ex, "保存完整错误信息失败")
        End Try
        Return Text.Substring(0, ErrorDialogMaxLength) & vbCrLf & vbCrLf &
            $"（错误信息过长，此处仅显示开头 {ErrorDialogMaxLength} 个字符。完整内容已写入日志，可点击左下角的按钮查看。）"
    End Function

End Class
