using System.Windows;

namespace PCLInstaller
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 全局异常兜底，避免安装器静默崩溃让用户不知所措
            DispatcherUnhandledException += (s, args) =>
            {
                MessageBox.Show("安装程序遇到未处理的问题：\n\n" + args.Exception.Message,
                    "PCL-EDIT 安装程序", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };

            new MainWindow().Show();
        }
    }
}
