using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using System.Windows;

namespace redmine_notifier.Views
{
    public class MainWindow : Avalonia.Controls.Window
    {

        private static readonly int height = 300;

        private static readonly int width = 280;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            Screen primaryScreen = Screens.Primary;

            PixelPoint bottomRight = primaryScreen.WorkingArea.BottomRight;

            var windowX = bottomRight.X - width - bottomRight.X / 50;

            var windowY = bottomRight.Y - height - 5;

            var windowPosition = new PixelPoint(windowX, windowY);

            Position = windowPosition;
        }
    }
}