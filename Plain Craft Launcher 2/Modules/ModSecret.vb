'由于包含加解密等安全信息，本文件中的部分代码已被删除

Friend Module ModSecret

    '标注 PCL 的不同分支，仅用于替换标记
    Public Const VersionBranchMain As String = "OpenSource"
    '在开源版的注册表与常规版的注册表隔离，以防数据冲突
    Public Const RegFolder As String = "PCLDebug"
    '用于微软登录的 ClientId
    Public OAuthClientId As String = If(Environment.GetEnvironmentVariable("PCL_MS_CLIENT_ID"), "")
    'CurseForge API Key
    Public CurseForgeAPIKey As String = If(Environment.GetEnvironmentVariable("PCL_CURSEFORGE_API_KEY"), "")
    '用于匿名数据收集的腾讯云日志服务上报 URL，形如 https://{region}.cls.tencentcs.com/track?topic_id={topic_id}
    Public Const ClsBaseUrl As String = ""

#Region "网络鉴权"

    'PCL-EDIT：恢复 MC 下载功能（开源版出于合规考虑默认阉割，本 Fork 将其恢复）
    Public Const RestrictMcDownload As Boolean = False
    Friend Function SecretCdnSign(UrlWithMark As String) As String
        If Not UrlWithMark.EndsWithF("{CDN}") Then Return UrlWithMark
        Return UrlWithMark.Replace("{CDN}", "").Replace(" ", "%20")
    End Function
    ''' <summary>
    ''' 设置 Headers 的 UA、Referer。
    ''' </summary>
    Friend Sub SecretHeadersSign(Url As String, ByRef Req As HttpRequestMessage, Optional SimulateBrowserHeaders As Boolean = False)
        If Not Req.Headers.UserAgent.Any Then
            If Url.Contains("baidupcs.com") OrElse Url.Contains("baidu.com") Then
                Req.Headers.Add("User-Agent", "LogStatistic")  '#4951
            ElseIf SimulateBrowserHeaders Then
                Req.Headers.Add("User-Agent", $"PCL2/{VersionBaseName}.{CInt(BuildType)} Mozilla/5.0 AppleWebKit/537.36 Chrome/63.0.3239.132 Safari/537.36")
            Else
                Req.Headers.Add("User-Agent", $"PCL2/{VersionBaseName}.{CInt(BuildType)}")
            End If
        End If
        If Not SimulateBrowserHeaders Then Req.Headers.Add("Referer", $"http://{VersionCode}.open.pcl2.server/")
        If Url.Contains("api.curseforge.com") OrElse Url.Contains("forgecdn.net") Then Req.Headers.Add("x-api-key", CurseForgeAPIKey)
    End Sub

#End Region

#Region "主题"

    Public Color1 As New MyColor(52, 61, 74)
    Public Color2 As New MyColor(11, 91, 203)
    Public Color3 As New MyColor(19, 112, 243)
    Public Color4 As New MyColor(72, 144, 245)
    Public Color5 As New MyColor(150, 192, 249)
    Public Color6 As New MyColor(213, 230, 253)
    Public Color7 As New MyColor(222, 236, 253)
    Public Color8 As New MyColor(234, 242, 254)
    Public ColorBg0 As New MyColor(150, 192, 249)
    Public ColorBg1 As New MyColor(190, Color7)
    Public ColorGray1 As New MyColor(64, 64, 64)
    Public ColorGray2 As New MyColor(115, 115, 115)
    Public ColorGray3 As New MyColor(140, 140, 140)
    Public ColorGray4 As New MyColor(166, 166, 166)
    Public ColorGray5 As New MyColor(204, 204, 204)
    Public ColorGray6 As New MyColor(235, 235, 235)
    Public ColorGray7 As New MyColor(240, 240, 240)
    Public ColorGray8 As New MyColor(245, 245, 245)
    Public ColorSemiTransparent As New MyColor(1, Color8)

    Public ThemeNow As Integer = -1
    Public ColorHue As Integer = 210, ColorSat As Integer = 85, ColorLightAdjust As Integer = 0, ColorHueTopbarDelta As OneOf(Of Integer, Integer()) = 0
    Public ThemeDontClick As Integer = 0

    Public Sub ThemeRefresh(Optional NewTheme As Integer = -1)
        Try
            If ThemeNow = NewTheme AndAlso NewTheme >= 0 Then Return
            If NewTheme >= 0 Then ThemeNow = NewTheme

            Color1 = New MyColor().FromHSL2(ColorHue, ColorSat * 0.2, 25 + ColorLightAdjust * 0.3)
            Color2 = New MyColor().FromHSL2(ColorHue, ColorSat, 45 + ColorLightAdjust)
            Color3 = New MyColor().FromHSL2(ColorHue, ColorSat, 55 + ColorLightAdjust)
            Color4 = New MyColor().FromHSL2(ColorHue, ColorSat, 65 + ColorLightAdjust)
            Color5 = New MyColor().FromHSL2(ColorHue, ColorSat, 80 + ColorLightAdjust * 0.4)
            Color6 = New MyColor().FromHSL2(ColorHue, ColorSat, 91 + ColorLightAdjust * 0.1)
            Color7 = New MyColor().FromHSL2(ColorHue, ColorSat, 95)
            Color8 = New MyColor().FromHSL2(ColorHue, ColorSat, 97)
            ColorBg0 = Color4 * 0.4 + Color5 * 0.4 + ColorGray4 * 0.2
            ColorBg1 = New MyColor(190, Color7)

            ColorSemiTransparent = New MyColor(1, Color8)
            Application.Current.Resources("ColorBrush1") = New SolidColorBrush(Color1)
            Application.Current.Resources("ColorBrush2") = New SolidColorBrush(Color2)
            Application.Current.Resources("ColorBrush3") = New SolidColorBrush(Color3)
            Application.Current.Resources("ColorBrush4") = New SolidColorBrush(Color4)
            Application.Current.Resources("ColorBrush5") = New SolidColorBrush(Color5)
            Application.Current.Resources("ColorBrush6") = New SolidColorBrush(Color6)
            Application.Current.Resources("ColorBrush7") = New SolidColorBrush(Color7)
            Application.Current.Resources("ColorBrush8") = New SolidColorBrush(Color8)
            Application.Current.Resources("ColorBrushBg0") = New SolidColorBrush(ColorBg0)
            Application.Current.Resources("ColorBrushBg1") = New SolidColorBrush(ColorBg1)
            Application.Current.Resources("ColorObject1") = CType(Color1, Color)
            Application.Current.Resources("ColorObject2") = CType(Color2, Color)
            Application.Current.Resources("ColorObject3") = CType(Color3, Color)
            Application.Current.Resources("ColorObject4") = CType(Color4, Color)
            Application.Current.Resources("ColorObject5") = CType(Color5, Color)
            Application.Current.Resources("ColorObject6") = CType(Color6, Color)
            Application.Current.Resources("ColorObject7") = CType(Color7, Color)
            Application.Current.Resources("ColorObject8") = CType(Color8, Color)
            Application.Current.Resources("ColorObjectBg0") = CType(ColorBg0, Color)
            Application.Current.Resources("ColorObjectBg1") = CType(ColorBg1, Color)
            ThemeRefreshMain()
        Catch ex As Exception
            Logger.Error(ex, "刷新主题颜色失败", LogBehavior.Toast)
        End Try
    End Sub
    Public Sub ThemeRefreshMain()
        RunInUi(
        Sub()
            If Not FrmMain.IsLoaded Then Return
            '顶部条背景
            Dim Brush = New LinearGradientBrush With {.EndPoint = New Point(1, 0), .StartPoint = New Point(0, 0)}
            Dim Deltas = ColorHueTopbarDelta.Switch(Function(d) New Integer() {-d, 0, d}, Function(d) d)
            Brush.GradientStops.Add(New GradientStop With {.Offset = 0, .Color = New MyColor().FromHSL2(ColorHue + Deltas(0), ColorSat, 48 + ColorLightAdjust)})
            Brush.GradientStops.Add(New GradientStop With {.Offset = 0.5, .Color = New MyColor().FromHSL2(ColorHue + Deltas(1), ColorSat, 54 + ColorLightAdjust)})
            Brush.GradientStops.Add(New GradientStop With {.Offset = 1, .Color = New MyColor().FromHSL2(ColorHue + Deltas(2), ColorSat, 48 + ColorLightAdjust)})
            FrmMain.PanTitle.Background = Brush
            FrmMain.PanTitle.Background.Freeze()
            '主页面背景
            If Settings.Get(Of Boolean)("UiBackgroundColorful") Then
                Brush = New LinearGradientBrush With {.EndPoint = New Point(0.1, 1), .StartPoint = New Point(0.9, 0)}
                Brush.GradientStops.Add(New GradientStop With {.Offset = -0.1, .Color = New MyColor().FromHSL2(ColorHue - 15, ColorSat * 0.8, 91)})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 0.4, .Color = New MyColor().FromHSL2(ColorHue, ColorSat * 0.8, 91)})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 1.1, .Color = New MyColor().FromHSL2(ColorHue + 15, ColorSat * 0.8, 91)})
                FrmMain.PanForm.Background = Brush
            Else
                FrmMain.PanForm.Background = New MyColor(245, 245, 245)
            End If
            FrmMain.PanForm.Background.Freeze()
        End Sub)
    End Sub
    Friend Sub ThemeCheckAll(EffectSetup As Boolean)
    End Sub
    Friend Function ThemeCheckOne(Id As Integer) As Boolean
        Return True
    End Function
    Friend Function ThemeUnlock(Id As Integer, Optional ShowDoubleHint As Boolean = True, Optional UnlockHint As String = Nothing) As Boolean
        Return False
    End Function

#End Region

#Region "更新"

    'PCL-EDIT 更新功能（#9553）
    '官方 notice.cgi 已对旧版客户端返回 403，无法再作为更新来源，故改用 GitHub Releases 作为更新源
    Public Const EditUpdateRepo As String = "hanqindylan/PCL-EDIT"

    ''' <summary>
    ''' 获取 PCL-EDIT 在 GitHub 上的最新 Release 信息。失败时返回 Nothing。
    ''' </summary>
    Public Function GetEditLatestRelease() As JObject
        Try
            Dim Json As JArray = NetRequestByClientRetry($"https://api.github.com/repos/{EditUpdateRepo}/releases?per_page=1", RequireJson:=True).DeserializeJson()
            If Json Is Nothing OrElse Not Json.Any() Then Return Nothing
            Return CType(Json(0), JObject)
        Catch ex As Exception
            Logger.Warn(ex, "获取 PCL-EDIT 最新版本失败")
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' 将版本名解析为可比较的整型数组，例如 2.99.0.1 → {2, 99, 0, 1}。
    ''' </summary>
    Public Function ParseEditVersion(VersionName As String) As Integer()
        If String.IsNullOrWhiteSpace(VersionName) Then Return {0, 0, 0, 0}
        Dim Parts As New List(Of Integer)
        For Each Part In VersionName.Trim().TrimStart("v"c).Split("."c)
            Dim Value As Integer = 0
            Integer.TryParse(Part, Value)
            Parts.Add(Value)
        Next
        While Parts.Count < 4
            Parts.Add(0)
        End While
        Return Parts.Take(4).ToArray()
    End Function
    ''' <summary>
    ''' 比较两个版本名。返回正数表示 A 更新，返回负数表示 B 更新，0 表示相同。
    ''' </summary>
    Public Function CompareEditVersion(A As String, B As String) As Integer
        Dim PartsA = ParseEditVersion(A), PartsB = ParseEditVersion(B)
        For i = 0 To 3
            If PartsA(i) <> PartsB(i) Then Return PartsA(i).CompareTo(PartsB(i))
        Next
        Return 0
    End Function

    ''' <summary>
    ''' 手动检查更新。
    ''' </summary>
    Friend Sub UpdateCheckByButton()
        RunInThread(Sub() UpdateCheckInternal(True))
    End Sub
    ''' <summary>
    ''' 启动时自动检查更新，遵循 启动器更新 设置（SystemSystemUpdate）。
    ''' </summary>
    Friend Sub UpdateCheckAuto()
        If Settings.Get(Of Integer)("SystemSystemUpdate") = 3 Then Return '关闭更新提示
        RunInThread(Sub() UpdateCheckInternal(False))
    End Sub

    ''' <summary>
    ''' 检查 PCL-EDIT 的更新。
    ''' IsFromButton 为真时，无论结果如何都会给出提示。
    ''' </summary>
    Private Sub UpdateCheckInternal(IsFromButton As Boolean)
        Dim Latest As JObject = Nothing
        Try
            Latest = GetEditLatestRelease()
        Catch ex As Exception
            Logger.Warn(ex, "获取 PCL-EDIT 最新版本失败")
        End Try
        If Latest Is Nothing Then
            If IsFromButton Then RunInUi(Sub() Hint("检查更新失败：无法连接更新服务器", HintType.Red))
            Return
        End If
        Dim LatestName As String = Latest("tag_name").ToString().TrimStart("v"c)
        Dim LatestUrl As String = If(Latest("html_url") IsNot Nothing, Latest("html_url").ToString(), $"https://github.com/{EditUpdateRepo}/releases")
        '写入缓存，供 IsLauncherNewest 等处使用
        Try
            DirectoryUtils.Create(PathTemp & "Cache\")
            FileUtils.Write(PathTemp & "Cache\EditUpdate.cfg", LatestName & "|" & LatestUrl, Encoding:=Encoding.UTF8)
        Catch ex As Exception
            Logger.Warn(ex, "写入更新缓存失败")
        End Try
        Logger.Info($"PCL-EDIT 最新版本：{LatestName}，当前版本：{VersionBaseName}")
        If CompareEditVersion(LatestName, VersionBaseName) <= 0 Then
            If IsFromButton Then RunInUi(Sub() Hint("当前已是最新版本"))
            Return
        End If
        '选择了自动下载时直接下载，否则弹窗提示
        If Settings.Get(Of Integer)("SystemSystemUpdate") = 0 AndAlso Not IsFromButton Then
            DownloadEditUpdate(Latest, LatestName)
            Return
        End If
        RunInUi(Sub() ShowEditUpdateDialog(LatestName, LatestUrl))
    End Sub

    ''' <summary>
    ''' 显示更新提示窗口。
    ''' </summary>
    Private Sub ShowEditUpdateDialog(LatestName As String, LatestUrl As String)
        Try
            Select Case MyMsgBox(
                $"发现新版本：{LatestName}{vbCrLf}当前版本：{VersionBaseName}{vbCrLf}{vbCrLf}是否现在下载并更新？",
                "启动器更新", "立即更新", "查看详情", "取消")
                Case 1
                    RunInThread(Sub() DownloadEditUpdate(Nothing, LatestName))
                Case 2
                    OpenWebsite(LatestUrl)
            End Select
        Catch ex As Exception
            Logger.Error(ex, "显示更新提示失败")
        End Try
    End Sub

    ''' <summary>
    ''' 下载 PCL-EDIT 的最新版本并执行替换更新。
    ''' Latest 为 Nothing 时会重新获取。必须在工作线程执行。
    ''' </summary>
    Private Sub DownloadEditUpdate(Optional Latest As JObject = Nothing, Optional LatestName As String = Nothing)
        RunInUi(Sub() Hint("正在下载新版本……", HintType.Blue))
        Try
            If Latest Is Nothing Then
                Latest = GetEditLatestRelease()
                If Latest Is Nothing Then Throw New Exception("无法获取最新版本信息")
                LatestName = Latest("tag_name").ToString().TrimStart("v"c)
            End If
            If String.IsNullOrEmpty(LatestName) Then LatestName = VersionBaseName
            If CompareEditVersion(LatestName, VersionBaseName) <= 0 Then
                RunInUi(Sub() Hint("当前已是最新版本"))
                Return
            End If
            '确定下载地址，优先 exe，其次 zip
            Dim DownloadUrl As String = Nothing
            Dim IsZip As Boolean = False
            Dim Assets As JArray = TryCast(Latest("assets"), JArray)
            If Assets IsNot Nothing Then
                For Each Asset As JObject In Assets
                    Dim AssetName As String = Asset("name").ToString().ToLower()
                    If AssetName.EndsWith(".exe") Then
                        DownloadUrl = Asset("browser_download_url").ToString()
                        IsZip = False
                        Exit For
                    ElseIf AssetName.EndsWith(".zip") AndAlso DownloadUrl Is Nothing Then
                        DownloadUrl = Asset("browser_download_url").ToString()
                        IsZip = True
                    End If
                Next
            End If
            If DownloadUrl Is Nothing Then
                '没有可用的附件，打开网页供手动下载
                RunInUi(Sub() OpenWebsite(Latest("html_url").ToString()))
                Return
            End If
            '下载
            Dim UpdateFolder As String = PathTemp & "PCL-EDIT-Update\"
            DirectoryUtils.Create(UpdateFolder)
            Dim DownloadPath As String = UpdateFolder & If(IsZip, "Update.zip", "Update.exe")
            NetDownloadByLoader(DownloadUrl, DownloadPath)
            '获取新 exe 的路径
            Dim NewExe As String = DownloadPath
            If IsZip Then
                Dim ExtractDir As String = UpdateFolder & "Extract\"
                FileUtils.ExtractToDirectory(DownloadPath, ExtractDir)
                FileUtils.Delete(DownloadPath)
                Dim ExeFiles = Directory.GetFiles(ExtractDir, "*.exe", SearchOption.AllDirectories)
                If Not ExeFiles.Any() Then Throw New Exception("下载的压缩包中不包含可执行文件")
                NewExe = ExeFiles.OrderByDescending(Function(f) New FileInfo(f).Length).First()
            End If
            '启动替换流程：以 --update 参数启动新版本，由它完成文件替换与重启
            IsUpdateWaitingRestart = True
            Logger.Info($"开始更新：{NewExe} → {PathExe}")
            StartProcess(NewExe, $"--update {Process.GetCurrentProcess().Id} ""{PathExe}"" ""{NewExe}"" True")
            '结束当前程序，等待替换完成
            IsUpdateWaitingRestart = False
            RunInUi(Sub() FormMain.EndProgramForce(ProcessReturnValues.TaskDone))
        Catch ex As Exception
            ex.ThrowIfCanceled()
            Logger.Error(ex, "下载更新失败")
            RunInUi(Sub() Hint("下载更新失败：" & ex.Message, HintType.Red))
        End Try
    End Sub

    Friend IsUpdateWaitingRestart As Boolean = False
    ''' <summary>
    ''' 重启 PCL。若 TriggerRestartAndByEnd 为真，则立即重启并结束当前程序。
    ''' </summary>
    Public Sub UpdateRestart(TriggerRestartAndByEnd As Boolean)
        IsUpdateWaitingRestart = False
        Try
            StartProcess(PathExe)
        Catch ex As Exception
            Logger.Error(ex, "重启 PCL 失败")
            Return
        End Try
        If TriggerRestartAndByEnd Then
            FormMain.EndProgramForce(ProcessReturnValues.TaskDone)
        End If
    End Sub
    ''' <summary>
    ''' 以 --update 模式启动时，由新版本进程调用，用于替换旧版文件并重启。
    ''' </summary>
    Public Sub UpdateReplace(ProcessId As Integer, OldFileName As String, NewFileName As String, TriggerRestart As Boolean)
        Logger.Info($"更新替换：{NewFileName} → {OldFileName}")
        '等待旧进程退出
        Try
            Dim OldProcess As Process = Process.GetProcessById(ProcessId)
            If Not OldProcess.HasExited Then OldProcess.WaitForExit(30000)
        Catch ex As Exception
            Logger.Warn(ex, "等待旧进程退出失败，直接尝试替换")
        End Try
        '替换文件
        Try
            FileUtils.Copy(NewFileName, OldFileName)
            Logger.Info("更新替换完成")
        Catch ex As Exception
            Logger.Error(ex, "更新替换失败")
            Environment.Exit(ProcessReturnValues.Fail)
        End Try
        '清理更新用的临时文件
        Try
            Dim UpdateFolder As String = PathTemp & "PCL-EDIT-Update\"
            If FileUtils.Exists(UpdateFolder) Then FileUtils.Delete(UpdateFolder, True)
        Catch ex As Exception
            Logger.Warn(ex, "清理更新临时文件失败")
        End Try
        If TriggerRestart Then
            Try
                StartProcess(OldFileName)
            Catch ex As Exception
                Logger.Error(ex, "更新后重启失败")
            End Try
        End If
    End Sub

    ''' <summary>
    ''' 确保 PathTemp/Latest.exe 是最新正式版的 PCL，它会被用于整合包打包。
    ''' 如果不是，则下载一个。
    ''' </summary>
    Friend Sub DownloadLatestPCL(Optional LoaderToSyncProgress As LoaderBase = Nothing)
        '注意：使用了独立的文件路径，以免与官方版本冲突
        Try
            Dim Latest = GetEditLatestRelease()
            If Latest Is Nothing Then Throw New Exception("无法获取 PCL-EDIT 的最新版本信息")
            Dim DownloadUrl As String = Nothing
            Dim Assets As JArray = TryCast(Latest("assets"), JArray)
            If Assets IsNot Nothing Then
                For Each Asset As JObject In Assets
                    Dim AssetName As String = Asset("name").ToString().ToLower()
                    If AssetName.EndsWith(".exe") OrElse AssetName.EndsWith(".zip") Then
                        DownloadUrl = Asset("browser_download_url").ToString()
                        Exit For
                    End If
                Next
            End If
            If DownloadUrl Is Nothing Then Throw New Exception("最新版本中没有可用的下载文件")
            Dim IsZip As Boolean = DownloadUrl.ToLower().EndsWith(".zip")
            Dim DownloadPath As String = PathTemp & If(IsZip, "Latest.zip", "Latest.exe")
            NetDownloadByLoader(DownloadUrl, DownloadPath, LoaderToSyncProgress)
            If IsZip Then
                Dim ExtractDir As String = PathTemp & "Latest\"
                FileUtils.ExtractToDirectory(DownloadPath, ExtractDir)
                FileUtils.Delete(DownloadPath)
                Dim ExeFiles = Directory.GetFiles(ExtractDir, "*.exe", SearchOption.AllDirectories)
                If Not ExeFiles.Any() Then Throw New Exception("下载的压缩包中不包含可执行文件")
                FileUtils.Copy(ExeFiles.OrderByDescending(Function(f) New FileInfo(f).Length).First(), PathTemp & "Latest.exe")
            End If
        Catch ex As Exception
            Logger.Error(ex, "下载最新版 PCL 失败")
            Throw
        End Try
    End Sub

#End Region

#Region "联网配置"

    ''' <summary>
    ''' 联网获取的配置信息。
    ''' 若获取失败或仍在获取中，可能为 Nothing。
    ''' </summary>
    Public ServerConfig As JObject

    Public ServerLoader As New LoaderTask(Of Integer, Integer)("PCL 配置更新", Sub() Logger.Info("该版本中不包含更新通知功能……"), Priority:=ThreadPriority.BelowNormal) With
        {.ReloadTimeout = 1000 * 60 * 60} '超时 1 小时

#End Region

#Region "赞助等级"

    Public ReadOnly Property CurrentRank As DonationRank
        Get
            Return DonationRank.None
        End Get
    End Property

    Public Sub InputPotatoCode(IsUpdating As Boolean)
    End Sub
    Friend Sub GeneratePotatoCode()
    End Sub

    ''' <summary>
    ''' 获取设备识别码。
    ''' </summary>
    Friend Function GetIdentify() As String
        Return "0000-0000-0000-0000"
    End Function

#End Region

End Module
