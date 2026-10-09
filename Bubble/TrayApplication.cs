using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Threading;
using Forms = System.Windows.Forms;

namespace Linka.Bubble
{
    internal sealed class TrayApplication : IDisposable
    {
        private readonly BubbleSettings _settings = SettingsStore.Load();
        private readonly GazeDisplayState _display = new GazeDisplayState();
        private readonly BubbleWindow _bubble = new BubbleWindow();
        private readonly TobiiGazeSource _source = new TobiiGazeSource();
        private readonly DispatcherTimer _frame = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        private readonly DispatcherTimer _save = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        private readonly Forms.NotifyIcon _icon;
        private readonly Forms.ToolStripMenuItem _toggle;
        private readonly Forms.ToolStripMenuItem _status;
        private SettingsWindow _settingsWindow;
        private long _startedAt;
        private long _lastSequence;
        private long _lastRetryAt;
        private string _currentStatus;
        private bool _connectFailed;
        private bool _reconnecting;
        private bool _disposed;

        public TrayApplication()
        {
            var menu = new Forms.ContextMenuStrip();
            _toggle = new Forms.ToolStripMenuItem("Скрыть пузырёк", null, (_, __) => Toggle());
            _status = new Forms.ToolStripMenuItem("Подключаемся") { Enabled = false };
            menu.Items.Add(_toggle);
            menu.Items.Add(new Forms.ToolStripMenuItem("Настройки", null, (_, __) => OpenSettings()));
            menu.Items.Add(_status);
            menu.Items.Add(new Forms.ToolStripSeparator());
            menu.Items.Add(new Forms.ToolStripMenuItem("Выход", null, (_, __) => Application.Current.Shutdown()));

            _icon = new Forms.NotifyIcon
            {
                Icon = SystemIcons.Information,
                Text = "Линка.Пузырик",
                ContextMenuStrip = menu,
                Visible = true
            };
            _icon.DoubleClick += (_, __) => OpenSettings();
            _bubble.Apply(_settings);
            _bubble.Show();
            _bubble.Hide();
            _toggle.Text = _settings.Visible ? "Скрыть пузырёк" : "Показать пузырёк";

            _save.Tick += (_, __) =>
            {
                _save.Stop();
                try { SettingsStore.Save(_settings); }
                catch (Exception error) when (error is System.IO.IOException || error is UnauthorizedAccessException)
                {
                    _icon.ShowBalloonTip(4000, "Линка.Пузырик", "Не удалось сохранить настройки.", Forms.ToolTipIcon.Warning);
                }
            };
            _frame.Tick += (_, __) => RenderFrame();
            _frame.Start();
            Connect();
        }

        private void Connect()
        {
            _reconnecting = _currentStatus == "Нет данных от трекера — проверьте Tobii";
            _display.Reset();
            _bubble.Hide();
            _lastSequence = 0;
            _startedAt = _lastRetryAt = Stopwatch.GetTimestamp();
            _connectFailed = false;
            if (!_reconnecting) SetStatus("Подключаемся");
            try { _source.Start(); }
            catch (Exception)
            {
                _connectFailed = true;
                SetStatus("Нет данных от трекера — проверьте Tobii");
            }
        }

        private void RenderFrame()
        {
            var now = Stopwatch.GetTimestamp();
            if (_connectFailed)
            {
                _bubble.Hide();
                if ((now - _lastRetryAt) / (double)Stopwatch.Frequency > 10) Connect();
                return;
            }
            var bounds = Forms.Screen.PrimaryScreen.Bounds;
            if (_source.TryGetLatest(out var x, out var y, out var received, out var sequence) && sequence != _lastSequence)
            {
                _reconnecting = false;
                _lastSequence = sequence;
                _display.Record(x - bounds.Left, y - bounds.Top, bounds.Width, bounds.Height,
                    received, _settings.Smoothness / 100.0);
            }

            var age = _display.AgeSeconds(now, _startedAt);
            if (!_display.TryGetPosition(now, out var pointX, out var pointY))
            {
                _bubble.Hide();
                if (age > 10)
                {
                    SetStatus("Нет данных от трекера — проверьте Tobii");
                    if ((now - _lastRetryAt) / (double)Stopwatch.Frequency > 10) Connect();
                }
                else SetStatus(_display.HasSamples ? "Взгляд не найден" :
                    _reconnecting ? "Нет данных от трекера — проверьте Tobii" : "Подключаемся");
                return;
            }

            SetStatus(_settings.Visible ? "Взгляд отслеживается" : "Пузырёк скрыт");
            if (!_settings.Visible) { _bubble.Hide(); return; }
            _bubble.MoveToPixels(pointX + bounds.Left, pointY + bounds.Top);
            if (!_bubble.IsVisible) _bubble.Show();
        }

        private void SetStatus(string status)
        {
            if (_currentStatus == status) return;
            _currentStatus = status;
            _status.Text = status;
            _settingsWindow?.SetStatus(status);
        }

        private void Toggle()
        {
            _settings.Visible = !_settings.Visible;
            Changed();
        }

        private void OpenSettings()
        {
            if (_settingsWindow == null)
            {
                _settingsWindow = new SettingsWindow(_settings, Changed);
                _settingsWindow.SetStatus(_currentStatus);
                _settingsWindow.Closed += (_, __) => _settingsWindow = null;
            }
            _settingsWindow.Show();
            _settingsWindow.Activate();
        }

        private void Changed()
        {
            _bubble.Apply(_settings);
            _toggle.Text = _settings.Visible ? "Скрыть пузырёк" : "Показать пузырёк";
            _settingsWindow?.RefreshSettings();
            if (!_settings.Visible) _bubble.Hide();
            _save.Stop();
            _save.Start();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _frame.Stop();
            _save.Stop();
            try { _source.Dispose(); }
            catch (Exception) { /* The SDK must not prevent saved settings and tray cleanup. */ }
            try { SettingsStore.Save(_settings); }
            catch (Exception error) when (error is System.IO.IOException || error is UnauthorizedAccessException) { }
            _icon.Visible = false;
            _icon.ContextMenuStrip.Dispose();
            _icon.Dispose();
            _bubble.Close();
        }
    }
}
