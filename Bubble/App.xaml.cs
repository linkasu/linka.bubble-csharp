using System;
using System.Threading;
using System.Windows;

namespace Linka.Bubble
{
    public partial class App : Application
    {
        private Mutex _singleInstance;
        private bool _ownsMutex;
        private TrayApplication _tray;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _singleInstance = new Mutex(true, "Local\\LINKa.Bubble", out var isFirst);
            _ownsMutex = isFirst;
            if (!isFirst)
            {
                Shutdown();
                return;
            }

            try
            {
                _tray = new TrayApplication();
            }
            catch (Exception error)
            {
                MessageBox.Show("Не удалось запустить Пузырик: " + error.Message,
                    "Линка.Пузырик", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _tray?.Dispose();
            if (_singleInstance != null)
            {
                if (_ownsMutex) _singleInstance.ReleaseMutex();
                _singleInstance.Dispose();
            }
            base.OnExit(e);
        }
    }
}
