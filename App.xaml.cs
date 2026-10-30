using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;

namespace AuthWin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private OtpSignalServer otpSignalServer;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            otpSignalServer = new OtpSignalServer();
            otpSignalServer.ShowRequested += () =>
            {
                Dispatcher.Invoke(() => (MainWindow as MainWindow)?.BringToFront());
            };

            try
            {
                otpSignalServer.Start();
            }
            catch (SocketException)
            {
                // Another AuthWin instance already owns the port; nothing to do.
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            otpSignalServer?.Stop();
            base.OnExit(e);
        }
    }
}
