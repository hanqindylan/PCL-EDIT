using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PCLInstaller
{
    public partial class MainWindow : Window
    {
        /// <summary>当前步骤：0=欢迎 1=选项 2=进度 3=完成 4=失败。</summary>
        private int _step;

        /// <summary>是否为卸载模式：由 exe 旁存在 uninstall.arg 决定。</summary>
        private readonly bool _uninstallMode;

        /// <summary>默认安装目录：%LOCALAPPDATA%\PCL-EDIT，无需管理员权限。</summary>
        private static string DefaultInstallDir =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PCL-EDIT");

        private string _customDir;

        public MainWindow()
        {
            InitializeComponent();
            _uninstallMode = File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uninstall.arg"));

            LabUserPath.Text = DefaultInstallDir;
            LabCustomPath.Text = "点击「浏览」选择其他文件夹";

            if (_uninstallMode)
            {
                LabCaption.Text = "PCL 3 卸载程序";
                BtnNext.Content = "开始卸载";
                ChkLaunch.Visibility = Visibility.Collapsed;
                ChkLaunch.IsChecked = false;
            }

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // 启动时不用做额外工作，保持极快的打开速度
        }

        #region 窗口交互

        private void CaptionBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            // 安装进行中不允许关闭，避免留下半个目录
            if (_step == 2)
            {
                if (MessageBox.Show("安装正在进行，确定要退出吗？\n退出可能导致安装不完整。", "PCL 3 安装程序",
                        MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            }
            Close();
        }

        #endregion

        #region 页面切换

        private void ShowStep(int step)
        {
            _step = step;
            PageWelcome.Visibility = step == 0 ? Visibility.Visible : Visibility.Collapsed;
            PageOptions.Visibility = step == 1 ? Visibility.Visible : Visibility.Collapsed;
            PageProgress.Visibility = step == 2 ? Visibility.Visible : Visibility.Collapsed;
            PageDone.Visibility = step == 3 ? Visibility.Visible : Visibility.Collapsed;
            PageFailed.Visibility = step == 4 ? Visibility.Visible : Visibility.Collapsed;

            BtnBack.Visibility = step == 1 ? Visibility.Visible : Visibility.Collapsed;
            BtnNext.IsEnabled = true;

            switch (step)
            {
                case 0:
                    BtnNext.Content = "下一步";
                    LabFooterHint.Text = "协议见随附的 LICENCE 文件，上游原文未作改动。";
                    break;
                case 1:
                    BtnNext.Content = _uninstallMode ? "开始卸载" : "开始安装";
                    LabFooterHint.Text = _uninstallMode ? "卸载不会删除你的存档与配置。" : "安装过程全自动，不需要任何额外操作。";
                    break;
                case 2:
                    BtnNext.Content = "请稍候……";
                    BtnNext.IsEnabled = false;
                    BtnBack.Visibility = Visibility.Collapsed;
                    LabFooterHint.Text = string.Empty;
                    break;
                case 3:
                    BtnNext.Content = "完成";
                    BtnBack.Visibility = Visibility.Collapsed;
                    LabFooterHint.Text = string.Empty;
                    break;
                case 4:
                    BtnNext.Content = "关闭";
                    BtnBack.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private async void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            switch (_step)
            {
                case 0:
                    ShowStep(1);
                    break;
                case 1:
                    ShowStep(2);
                    if (_uninstallMode) await RunUninstallAsync();
                    else await RunInstallAsync();
                    break;
                case 3:
                case 4:
                    Close();
                    break;
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (_step == 1) ShowStep(0);
        }

        private void InstallLocation_Changed(object sender, RoutedEventArgs e)
        {
            if (RadInstallCustom == null || LabCustomPath == null) return;
            if (RadInstallCustom.IsChecked == true && _customDir == null)
            {
                // 首次切到自定义位置时直接弹目录选择
                _customDir = PickFolder();
                if (_customDir == null)
                {
                    RadInstallUser.IsChecked = true;
                    return;
                }
                LabCustomPath.Text = _customDir;
            }
        }

        private string PickFolder()
        {
            // 用 WPF 自带的 OpenFileDialog 曲线实现文件夹选择，避免额外依赖 WinForms
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "选择安装文件夹",
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "选择此文件夹"
            };
            return dialog.ShowDialog(this) == true
                ? Path.GetDirectoryName(dialog.FileName)
                : null;
        }

        #endregion

        #region 安装

        private string TargetDir => RadInstallCustom.IsChecked == true && _customDir != null
            ? _customDir
            : DefaultInstallDir;

        private void SetProgress(double value, string title = null, string detail = null)
        {
            BarMain.Value = value;
            if (title != null) LabProgressTitle.Text = title;
            if (detail != null)
            {
                LabProgressDetail.Text = detail;
                AppendLog(detail);
            }
        }

        private void AppendLog(string text)
        {
            LabLog.Text += (LabLog.Text.Length == 0 ? "" : "\n") + text;
            ScrollLog.ScrollToEnd();
        }

        private async Task RunInstallAsync()
        {
            var dir = TargetDir;
            try
            {
                SetProgress(2, "正在安装……", "准备安装目录：" + dir);
                await Task.Run(() => Directory.CreateDirectory(dir));

                // 释放内嵌的 PCL 压缩包
                SetProgress(10, "正在安装……", "正在解压程序文件");
                var tempZip = Path.Combine(Path.GetTempPath(), "PCL-EDIT-" + Guid.NewGuid().ToString("N") + ".zip");
                await Task.Run(() =>
                {
                    using (var input = Assembly.GetExecutingAssembly()
                               .GetManifestResourceStream("PCLInstaller.payload.zip"))
                    {
                        if (input == null) throw new InvalidOperationException("安装包内容缺失：未找到内嵌的程序文件。");
                        using (var output = File.Create(tempZip)) input.CopyTo(output);
                    }
                });

                SetProgress(30, "正在安装……", "正在写入程序文件");
                await Task.Run(() => ExtractPayload(tempZip, dir));
                try { File.Delete(tempZip); } catch { /* 临时文件删不掉不影响安装 */ }

                SetProgress(80, "正在安装……", "正在创建快捷方式");
                await Task.Run(() => CreateShortcuts(dir));

                SetProgress(90, "正在安装……", "正在写入卸载信息");
                await Task.Run(() => RegisterUninstall(dir));

                SetProgress(100, "安装完成", "全部文件都已就位");
                LabDoneDetail.Text = $"已安装到：\n{dir}";

                if (ChkLaunch.IsChecked == true)
                {
                    var exe = Path.Combine(dir, "Plain Craft Launcher 2.exe");
                    if (File.Exists(exe)) Process.Start(new ProcessStartInfo(exe) { WorkingDirectory = dir });
                }

                ShowStep(3);
            }
            catch (Exception ex)
            {
                LabFailedDetail.Text = "安装过程中出现问题：" + ex.Message;
                LabFailedLog.Text = ex.ToString();
                ShowStep(4);
            }
        }

        private void ExtractPayload(string zipPath, string targetDir)
        {
            using (var zip = ZipFile.OpenRead(zipPath))
            {
                var total = zip.Entries.Count;
                var done = 0;
                foreach (var entry in zip.Entries)
                {
                    var dest = Path.Combine(targetDir, entry.FullName.Replace('/', Path.DirectorySeparatorChar));
                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        Directory.CreateDirectory(dest);
                    }
                    else
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(dest));
                        entry.ExtractToFile(dest, true);
                    }
                    done++;
                    if (done % 20 == 0)
                    {
                        var pct = 30 + 50.0 * done / total;
                        Dispatcher.Invoke(() => BarMain.Value = pct);
                    }
                }
            }
        }

        private void CreateShortcuts(string dir)
        {
            var exe = Path.Combine(dir, "Plain Craft Launcher 2.exe");
            if (!File.Exists(exe)) throw new FileNotFoundException("未找到主程序：Plain Craft Launcher 2.exe");

            if (ChkDesktop.IsChecked == true)
            {
                var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                ShortcutHelper.Create(Path.Combine(desktop, "PCL 3.lnk"), exe, dir, "PCL 3（Hanqinchen）");
            }

            var programs = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            var menuDir = Path.Combine(programs, "PCL-EDIT");
            Directory.CreateDirectory(menuDir);
            ShortcutHelper.Create(Path.Combine(menuDir, "PCL 3.lnk"), exe, dir, "PCL 3（Hanqinchen）");
            ShortcutHelper.Create(Path.Combine(menuDir, "卸载 PCL 3.lnk"),
                Path.Combine(dir, "PCL-EDIT 安装程序.exe"), dir, "卸载 PCL-EDIT");
        }

        private void RegisterUninstall(string dir)
        {
            // 复制卸载器并留下标记文件，让它以卸载模式启动
            var installer = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PCL-EDIT 安装程序.exe");
            var uninstaller = Path.Combine(dir, "PCL-EDIT 安装程序.exe");
            if (!string.Equals(installer, uninstaller, StringComparison.OrdinalIgnoreCase))
                File.Copy(installer, uninstaller, true);
            File.WriteAllText(Path.Combine(dir, "uninstall.arg"), dir);

            // 注册到「应用和功能」，可直接卸载
            using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(
                       @"Software\Microsoft\Windows\CurrentVersion\Uninstall\PCL-EDIT"))
            {
                key.SetValue("DisplayName", "PCL 3（Plain Craft Launcher 2 修改版，Hanqinchen 制作）");
                key.SetValue("DisplayVersion", "3.0.0");
                key.SetValue("Publisher", "Hanqinchen");
                key.SetValue("InstallLocation", dir);
                key.SetValue("DisplayIcon", Path.Combine(dir, "Plain Craft Launcher 2.exe"));
                key.SetValue("UninstallString", $"\"{uninstaller}\"");
                key.SetValue("NoModify", 1, Microsoft.Win32.RegistryValueKind.DWord);
                key.SetValue("NoRepair", 1, Microsoft.Win32.RegistryValueKind.DWord);
            }
        }

        #endregion

        #region 卸载

        private async Task RunUninstallAsync()
        {
            var dir = TargetDir;
            try
            {
                SetProgress(5, "正在卸载……", "正在关闭 PCL 3");
                await Task.Run(() =>
                {
                    foreach (var p in Process.GetProcessesByName("Plain Craft Launcher 2"))
                    {
                        try { p.Kill(); } catch { /* 关不掉就继续 */ }
                    }
                });

                SetProgress(25, "正在卸载……", "正在删除快捷方式");
                await Task.Run(() =>
                {
                    TryDelete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "PCL 3.lnk"));
                    TryDelete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "PCL-EDIT", "PCL 3.lnk"));
                });

                SetProgress(60, "正在卸载……", "正在删除程序文件");
                await Task.Run(() => DeleteFilesKeepingUserData(dir));

                SetProgress(90, "正在卸载……", "正在清理注册表项");
                await Task.Run(() =>
                {
                    try
                    {
                        Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(
                            @"Software\Microsoft\Windows\CurrentVersion\Uninstall\PCL-EDIT", false);
                    }
                    catch { /* 键不存在时忽略 */ }
                });

                SetProgress(100, "卸载完成", "PCL 3 已从本机移除");
                LabDoneDetail.Text = "已保留你的存档、Mod 与配置文件。\n如需彻底清理，请手动删除安装文件夹。";
                ShowStep(3);
            }
            catch (Exception ex)
            {
                LabFailedDetail.Text = "卸载过程中出现问题：" + ex.Message;
                LabFailedLog.Text = ex.ToString();
                ShowStep(4);
            }
        }

        /// <summary>删除程序文件，但保留存档、Mod、配置等用户数据。</summary>
        private static void DeleteFilesKeepingUserData(string dir)
        {
            if (!Directory.Exists(dir)) return;

            // 这些目录属于用户数据，卸载时不动
            var keep = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "versions", "mods", "saves", "resourcepacks", "shaderpacks", "config", "logs", "PCL"
            };

            foreach (var file in Directory.GetFiles(dir))
            {
                var name = Path.GetFileName(file);
                // 保留用户配置与存档压缩包
                if (name.Equals("PCL.ini", StringComparison.OrdinalIgnoreCase)) continue;
                TryDelete(file);
            }

            foreach (var sub in Directory.GetDirectories(dir))
            {
                var name = Path.GetFileName(sub);
                if (keep.Contains(name)) continue;
                try { Directory.Delete(sub, true); } catch { /* 被占用就跳过 */ }
            }
        }

        private static void TryDelete(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); } catch { /* 忽略 */ }
        }

        #endregion
    }

    /// <summary>用 PowerShell 创建快捷方式，避免依赖 COM 互操作程序集。</summary>
    internal static class ShortcutHelper
    {
        public static void Create(string lnkPath, string target, string workDir, string description)
        {
            var script =
                "$w = New-Object -ComObject WScript.Shell; " +
                $"$s = $w.CreateShortcut('{Escape(lnkPath)}'); " +
                $"$s.TargetPath = '{Escape(target)}'; " +
                $"$s.WorkingDirectory = '{Escape(workDir)}'; " +
                $"$s.Description = '{Escape(description)}'; " +
                "$s.Save()";

            var psi = new ProcessStartInfo("powershell.exe",
                "-NoProfile -NonInteractive -ExecutionPolicy Bypass -Command \"" + script + "\"")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            using (var p = Process.Start(psi))
            {
                p.WaitForExit(15000);
            }
        }

        private static string Escape(string s) => s.Replace("'", "''");
    }
}
