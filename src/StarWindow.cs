using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using Forms = System.Windows.Forms;

namespace SeptemberCountdown
{
    internal sealed class StarWindow : Window
    {
        private readonly AppSettings _settings;
        private readonly Point _ownerCenterScreen;
        private readonly Color _accent1;
        private readonly Color _accent2;
        private readonly Color _nebula1;
        private readonly Color _nebula2;
        private readonly Color _nebula3;
        private readonly Color _nebula4;
        private readonly TranslateTransform _contentShift = new TranslateTransform();
        private readonly ScaleTransform _contentScale = new ScaleTransform(0.92, 0.92);
        private readonly ScaleTransform _skyScale = new ScaleTransform(0.92, 0.92);
        private TextBlock _nameText;
        private TextBlock _daysText;
        private TextBlock _hoursText;
        private TextBlock _minutesText;
        private TextBlock _secondsText;
        private StackPanel _secondsGroup;
        private TextBlock _hintText;
        private Grid _contentGrid;
        private Starfield _starfield;
        private DispatcherTimer _timer;
        private bool _closed;

        internal StarWindow(AppSettings settings, Window owner, Point ownerCenterScreen,
            Color accent1, Color accent2, Color nebula1, Color nebula2, Color nebula3, Color nebula4)
        {
            _settings = settings;
            _ownerCenterScreen = ownerCenterScreen;
            _accent1 = accent1;
            _accent2 = accent2;
            _nebula1 = nebula1;
            _nebula2 = nebula2;
            _nebula3 = nebula3;
            _nebula4 = nebula4;

            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            Topmost = true;
            Background = Brushes.Black;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Owner = owner;

            PlaceOnOwnerScreen(owner);
            BuildInterface();

            Opacity = 0;
            Loaded += OnLoaded;
            Closed += OnClosed;
            PreviewMouseDown += OnPreviewMouseDown;
            PreviewKeyDown += OnPreviewKeyDown;
        }

        private void PlaceOnOwnerScreen(Window owner)
        {
            Forms.Screen screen = Forms.Screen.FromPoint(
                new System.Drawing.Point((int)_ownerCenterScreen.X, (int)_ownerCenterScreen.Y));
            double scaleX = 1;
            double scaleY = 1;
            PresentationSource source = PresentationSource.FromVisual(owner);
            if (source != null && source.CompositionTarget != null)
            {
                Matrix transform = source.CompositionTarget.TransformToDevice;
                scaleX = transform.M11 <= 0 ? 1 : transform.M11;
                scaleY = transform.M22 <= 0 ? 1 : transform.M22;
            }

            Left = screen.Bounds.Left / scaleX;
            Top = screen.Bounds.Top / scaleY;
            Width = screen.Bounds.Width / scaleX;
            Height = screen.Bounds.Height / scaleY;
        }

        private void BuildInterface()
        {
            Grid root = new Grid();
            _starfield = new Starfield(_accent1, _accent2, _nebula1, _nebula2, _nebula3);
            _starfield.RenderTransform = _skyScale;
            _starfield.RenderTransformOrigin = new Point(0.5, 0.5);
            _starfield.Opacity = 0;
            root.Children.Add(_starfield);

            Grid content = new Grid();
            _contentGrid = content;
            content.Width = 680;
            content.Height = 300;
            content.HorizontalAlignment = HorizontalAlignment.Center;
            content.VerticalAlignment = VerticalAlignment.Center;
            content.RenderTransformOrigin = new Point(0.5, 0.5);
            TransformGroup transforms = new TransformGroup();
            transforms.Children.Add(_contentScale);
            transforms.Children.Add(_contentShift);
            content.RenderTransform = transforms;
            content.Opacity = 0;

            StackPanel stack = new StackPanel();
            stack.HorizontalAlignment = HorizontalAlignment.Center;
            stack.VerticalAlignment = VerticalAlignment.Center;

            _nameText = CreateText(_settings.Name, 20, FontWeights.SemiBold,
                Color.FromRgb(247, 249, 255), 14, 1);
            _nameText.HorizontalAlignment = HorizontalAlignment.Center;
            _nameText.TextAlignment = TextAlignment.Center;
            _nameText.MaxWidth = 620;
            _nameText.TextTrimming = TextTrimming.CharacterEllipsis;
            stack.Children.Add(_nameText);

            StackPanel daysRow = new StackPanel();
            daysRow.Orientation = Orientation.Horizontal;
            daysRow.HorizontalAlignment = HorizontalAlignment.Center;
            daysRow.Margin = new Thickness(0, 6, 0, 0);

            _daysText = CreateText("0", 84, FontWeights.SemiBold,
                Color.FromRgb(248, 250, 255), 24, 2);
            _daysText.LineHeight = 88;
            _daysText.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            Typography.SetNumeralAlignment(_daysText, FontNumeralAlignment.Tabular);
            daysRow.Children.Add(_daysText);

            StackPanel dayUnit = new StackPanel();
            dayUnit.VerticalAlignment = VerticalAlignment.Bottom;
            dayUnit.Margin = new Thickness(12, 0, 0, 17);
            TextBlock day = CreateText("天", 15, FontWeights.SemiBold,
                Color.FromRgb(235, 241, 250), 8, 1);
            TextBlock daysCaption = CreateText("D A Y S", 7.5, FontWeights.Normal,
                Color.FromRgb(197, 210, 228), 6, 1);
            daysCaption.Margin = new Thickness(0, 3, 0, 0);
            dayUnit.Children.Add(day);
            dayUnit.Children.Add(daysCaption);
            daysRow.Children.Add(dayUnit);
            stack.Children.Add(daysRow);

            StackPanel clockRow = new StackPanel();
            clockRow.Orientation = Orientation.Horizontal;
            clockRow.HorizontalAlignment = HorizontalAlignment.Center;
            clockRow.Margin = new Thickness(0, -3, 0, 0);

            _hoursText = CreateText("00", 36, FontWeights.SemiBold,
                Color.FromRgb(235, 241, 250), 14, 1);
            _hoursText.VerticalAlignment = VerticalAlignment.Center;
            clockRow.Children.Add(_hoursText);
            clockRow.Children.Add(CreateColon());
            _minutesText = CreateText("00", 36, FontWeights.SemiBold,
                Color.FromRgb(235, 241, 250), 14, 1);
            _minutesText.VerticalAlignment = VerticalAlignment.Center;
            clockRow.Children.Add(_minutesText);

            _secondsGroup = new StackPanel();
            _secondsGroup.Orientation = Orientation.Horizontal;
            _secondsGroup.Children.Add(CreateColon());
            _secondsText = CreateText("00", 36, FontWeights.SemiBold,
                _accent1, 14, 1);
            _secondsText.VerticalAlignment = VerticalAlignment.Center;
            _secondsGroup.Children.Add(_secondsText);
            clockRow.Children.Add(_secondsGroup);
            stack.Children.Add(clockRow);

            content.Children.Add(stack);

            _hintText = CreateText("点击任意位置退出星空模式", 10, FontWeights.Normal,
                Color.FromRgb(150, 170, 205), 0, 0);
            _hintText.HorizontalAlignment = HorizontalAlignment.Center;
            _hintText.VerticalAlignment = VerticalAlignment.Bottom;
            _hintText.Margin = new Thickness(0, 0, 0, 30);
            _hintText.Opacity = 0;
            content.Children.Add(_hintText);

            root.Children.Add(content);
            Content = root;
        }

        private static TextBlock CreateText(string text, double size, FontWeight weight,
            Color color, double shadowBlur, double shadowDepth)
        {
            TextBlock block = new TextBlock();
            block.Text = text;
            block.FontFamily = new FontFamily("Segoe UI Variable Display, Segoe UI, Microsoft YaHei UI");
            block.FontSize = size;
            block.FontWeight = weight;
            block.Foreground = new SolidColorBrush(color);
            TextOptions.SetTextFormattingMode(block, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(block, TextRenderingMode.Grayscale);
            if (shadowBlur > 0)
            {
                DropShadowEffect shadow = new DropShadowEffect();
                shadow.BlurRadius = shadowBlur;
                shadow.ShadowDepth = shadowDepth;
                shadow.Direction = 270;
                shadow.Color = Color.FromArgb(220, 0, 0, 0);
                shadow.Opacity = 0.85;
                block.Effect = shadow;
            }
            return block;
        }

        private TextBlock CreateColon()
        {
            TextBlock colon = CreateText(":", 30, FontWeights.Normal,
                Color.FromRgb(200, 214, 234), 7, 1);
            colon.Margin = new Thickness(5, 0, 5, 2);
            colon.VerticalAlignment = VerticalAlignment.Center;
            return colon;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateLayout();

            Point start = PointFromScreen(_ownerCenterScreen);
            Point center = new Point(ActualWidth / 2.0, ActualHeight / 2.0);
            _contentShift.X = start.X - center.X;
            _contentShift.Y = start.Y - center.Y;

            DoubleAnimation windowFade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220));
            windowFade.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            BeginAnimation(OpacityProperty, windowFade);

            DoubleAnimation skyFade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(850));
            skyFade.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            _starfield.BeginAnimation(OpacityProperty, skyFade);

            DoubleAnimation skyScaleX = new DoubleAnimation(0.92, 1, TimeSpan.FromMilliseconds(950));
            skyScaleX.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            _skyScale.BeginAnimation(ScaleTransform.ScaleXProperty, skyScaleX);
            _skyScale.BeginAnimation(ScaleTransform.ScaleYProperty, skyScaleX);

            DoubleAnimation contentFade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(700));
            contentFade.BeginTime = TimeSpan.FromMilliseconds(80);
            contentFade.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            _contentGrid.BeginAnimation(OpacityProperty, contentFade);

            DoubleAnimation moveX = new DoubleAnimation(_contentShift.X, 0,
                TimeSpan.FromMilliseconds(900));
            moveX.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            DoubleAnimation moveY = new DoubleAnimation(_contentShift.Y, 0,
                TimeSpan.FromMilliseconds(900));
            moveY.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            _contentShift.BeginAnimation(TranslateTransform.XProperty, moveX);
            _contentShift.BeginAnimation(TranslateTransform.YProperty, moveY);

            DoubleAnimation scaleX = new DoubleAnimation(0.92, 1, TimeSpan.FromMilliseconds(900));
            scaleX.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            _contentScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleX);
            _contentScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleX);

            DoubleAnimation hintFade = new DoubleAnimation(0, 0.7, TimeSpan.FromMilliseconds(500));
            hintFade.BeginTime = TimeSpan.FromMilliseconds(1100);
            _hintText.BeginAnimation(OpacityProperty, hintFade);

            _timer = new DispatcherTimer(DispatcherPriority.Background);
            _timer.Interval = TimeSpan.FromMilliseconds(250);
            _timer.Tick += delegate { UpdateCountdown(); };
            _timer.Start();
            UpdateCountdown();

            Activate();
            Focus();
        }

        private void UpdateCountdown()
        {
            DateTime target = _settings.GetTarget();
            TimeSpan remaining = target - DateTime.Now;
            bool ended = remaining.TotalSeconds <= 0;
            long totalSeconds = ended ? 0 : (long)Math.Floor(remaining.TotalSeconds);
            int days = (int)(totalSeconds / 86400);
            int hours = (int)((totalSeconds % 86400) / 3600);
            int minutes = (int)((totalSeconds % 3600) / 60);
            int seconds = (int)(totalSeconds % 60);

            _daysText.Text = days.ToString(CultureInfo.InvariantCulture);
            _hoursText.Text = hours.ToString("00", CultureInfo.InvariantCulture);
            _minutesText.Text = minutes.ToString("00", CultureInfo.InvariantCulture);
            _secondsText.Text = seconds.ToString("00", CultureInfo.InvariantCulture);
            _secondsGroup.Visibility = _settings.ShowSeconds ? Visibility.Visible : Visibility.Collapsed;
            _nameText.Text = _settings.Name;
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ExitStarMode();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Space || e.Key == Key.Enter)
            {
                ExitStarMode();
                e.Handled = true;
            }
        }

        private void ExitStarMode()
        {
            if (_closed)
            {
                return;
            }
            _closed = true;
            if (_timer != null)
            {
                _timer.Stop();
            }
            DoubleAnimation fade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(180));
            fade.Completed += delegate
            {
                try
                {
                    Close();
                }
                catch
                {
                }
            };
            BeginAnimation(OpacityProperty, fade);
        }

        private void OnClosed(object sender, EventArgs e)
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
        }
    }
}
