using System;
using System.Windows;

namespace Linka.Bubble
{
    public partial class SettingsWindow : Window
    {
        private readonly BubbleSettings _settings;
        private readonly Action _changed;
        private bool _ready;

        internal SettingsWindow(BubbleSettings settings, Action changed)
        {
            InitializeComponent();
            _settings = settings;
            _changed = changed;
            ReadSettings();
            _ready = true;
        }

        private void ReadSettings()
        {
            ShowBubble.IsChecked = _settings.Visible;
            SizeSlider.Value = _settings.Diameter;
            BrightnessSlider.Value = _settings.Brightness;
            SmoothnessSlider.Value = _settings.Smoothness;
        }

        public void RefreshSettings()
        {
            _ready = false;
            ReadSettings();
            _ready = true;
        }

        private void OnChanged(object sender, RoutedEventArgs e)
        {
            if (!_ready) return;
            _settings.Visible = ShowBubble.IsChecked == true;
            _settings.Diameter = (int)SizeSlider.Value;
            _settings.Brightness = (int)BrightnessSlider.Value;
            _settings.Smoothness = (int)SmoothnessSlider.Value;
            _changed();
        }

        private void ResetAppearance(object sender, RoutedEventArgs e)
        {
            var defaults = new BubbleSettings();
            _ready = false;
            _settings.Diameter = defaults.Diameter;
            _settings.Brightness = defaults.Brightness;
            _settings.Smoothness = defaults.Smoothness;
            ReadSettings();
            _ready = true;
            _changed();
        }

        private void CloseSettings(object sender, RoutedEventArgs e) => Close();

        public void SetStatus(string message) => StatusText.Text = message;
    }
}
