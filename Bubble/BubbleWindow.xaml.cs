using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Linka.Bubble
{
    public partial class BubbleWindow : Window
    {
        private const int GwlExstyle = -20;
        private const int WsExTransparent = 0x20;
        private const int WsExToolwindow = 0x80;
        private const int WsExNoactivate = 0x08000000;

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr(IntPtr window, int index);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr window, int index, IntPtr value);

        public BubbleWindow()
        {
            InitializeComponent();
            SourceInitialized += (_, __) =>
            {
                var window = new WindowInteropHelper(this).Handle;
                var style = GetWindowLongPtr(window, GwlExstyle).ToInt64();
                SetWindowLongPtr(window, GwlExstyle,
                    new IntPtr(style | WsExTransparent | WsExToolwindow | WsExNoactivate));
            };
        }

        internal void Apply(BubbleSettings settings)
        {
            Width = Height = settings.Diameter;
            Opacity = settings.Brightness / 100.0;
        }

        public void MoveToPixels(double x, double y)
        {
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget == null) return;
            var point = source.CompositionTarget.TransformFromDevice.Transform(new Point(x, y));
            Left = point.X - Width / 2;
            Top = point.Y - Height / 2;
        }
    }
}
