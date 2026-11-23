using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace SeptemberCountdown
{
    internal sealed class Starfield : FrameworkElement
    {
        private sealed class Star
        {
            public double X;
            public double Y;
            public double Radius;
            public double BaseAlpha;
            public double Phase;
            public double Speed;
            public Color Color;
        }

        private sealed class Dust
        {
            public double X;
            public double Y;
            public double VX;
            public double VY;
            public double Radius;
            public double Alpha;
            public Color Color;
        }

        private sealed class Shooter
        {
            public double X;
            public double Y;
            public double VX;
            public double VY;
            public double Life;
            public double MaxLife;
            public double Length;
        }

        private readonly Random _random = new Random();
        private readonly Star[] _stars = new Star[190];
        private readonly Dust[] _dust = new Dust[46];
        private readonly Shooter[] _shooters = new Shooter[3];
        private readonly Color _accent1;
        private readonly Color _accent2;
        private readonly Color _nebula1;
        private readonly Color _nebula2;
        private readonly Color _nebula3;
        private RadialGradientBrush _nebulaA;
        private RadialGradientBrush _nebulaB;
        private RadialGradientBrush _nebulaC;
        private RadialGradientBrush _nebulaD;
        private LinearGradientBrush _milkyWay;
        private LinearGradientBrush _background;
        private DispatcherTimer _timer;
        private double _time;

        internal Starfield(Color accent1, Color accent2, Color nebula1, Color nebula2, Color nebula3)
        {
            _accent1 = accent1;
            _accent2 = accent2;
            _nebula1 = nebula1;
            _nebula2 = nebula2;
            _nebula3 = nebula3;
            IsHitTestVisible = false;
            SnapsToDevicePixels = false;

            Color[] starColors = new Color[]
            {
                Color.FromRgb(255, 255, 255),
                Color.FromRgb(214, 232, 255),
                Lighten(accent1, 0.55),
                Lighten(accent2, 0.6),
                Color.FromRgb(255, 235, 206)
            };

            for (int i = 0; i < _stars.Length; i++)
            {
                Star star = new Star();
                star.X = _random.NextDouble();
                star.Y = _random.NextDouble();
                star.Radius = 0.35 + _random.NextDouble() * 1.45;
                star.BaseAlpha = 0.28 + _random.NextDouble() * 0.72;
                star.Phase = _random.NextDouble() * Math.PI * 2;
                star.Speed = 0.35 + _random.NextDouble() * 1.5;
                star.Color = starColors[_random.Next(starColors.Length)];
                _stars[i] = star;
            }

            for (int i = 0; i < _dust.Length; i++)
            {
                Dust dust = new Dust();
                dust.X = _random.NextDouble();
                dust.Y = _random.NextDouble();
                dust.VX = (_random.NextDouble() - 0.5) * 0.012;
                dust.VY = (_random.NextDouble() - 0.5) * 0.01;
                dust.Radius = 0.5 + _random.NextDouble() * 1.3;
                dust.Alpha = 0.12 + _random.NextDouble() * 0.3;
                dust.Color = i % 3 == 0 ? Lighten(accent1, 0.6) : Lighten(accent2, 0.65);
                _dust[i] = dust;
            }

            for (int i = 0; i < _shooters.Length; i++)
            {
                _shooters[i] = NewShooter(true);
            }

            _nebulaA = CreateNebula(_nebula1, 0.55);
            _nebulaB = CreateNebula(_nebula2, 0.5);
            _nebulaC = CreateNebula(_nebula3, 0.45);
            _nebulaD = CreateNebula(_accent2, 0.38);
            _milkyWay = CreateMilkyWay();
            _background = CreateBackground();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private static LinearGradientBrush CreateBackground()
        {
            LinearGradientBrush brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0, 0);
            brush.EndPoint = new Point(0, 1);
            brush.GradientStops.Add(new GradientStop(Color.FromRgb(4, 7, 17), 0.0));
            brush.GradientStops.Add(new GradientStop(Color.FromRgb(8, 12, 30), 0.45));
            brush.GradientStops.Add(new GradientStop(Color.FromRgb(2, 4, 11), 1.0));
            brush.Freeze();
            return brush;
        }

        private static RadialGradientBrush CreateNebula(Color color, double alpha)
        {
            RadialGradientBrush brush = new RadialGradientBrush();
            brush.Center = new Point(0.5, 0.5);
            brush.GradientOrigin = new Point(0.45, 0.42);
            brush.RadiusX = 0.5;
            brush.RadiusY = 0.5;
            brush.GradientStops.Add(new GradientStop(WithAlpha(color, (byte)(alpha * 255)), 0.0));
            brush.GradientStops.Add(new GradientStop(WithAlpha(color, (byte)(alpha * 150)), 0.45));
            brush.GradientStops.Add(new GradientStop(WithAlpha(color, 18), 0.78));
            brush.GradientStops.Add(new GradientStop(WithAlpha(color, 0), 1.0));
            brush.Freeze();
            return brush;
        }

        private LinearGradientBrush CreateMilkyWay()
        {
            LinearGradientBrush brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0, 0);
            brush.EndPoint = new Point(0, 1);
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0, 255, 255, 255), 0.0));
            brush.GradientStops.Add(new GradientStop(WithAlpha(_accent1, 34), 0.3));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(34, 255, 255, 255), 0.48));
            brush.GradientStops.Add(new GradientStop(WithAlpha(_accent2, 28), 0.7));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0, 255, 255, 255), 1.0));
            brush.Freeze();
            return brush;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_timer == null)
            {
                _timer = new DispatcherTimer(DispatcherPriority.Render);
                _timer.Interval = TimeSpan.FromMilliseconds(33);
                _timer.Tick += OnTick;
            }
            _timer.Start();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_timer != null)
            {
                _timer.Stop();
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            _time += 0.033;
            for (int i = 0; i < _dust.Length; i++)
            {
                Dust dust = _dust[i];
                dust.X += dust.VX * 0.033;
                dust.Y += dust.VY * 0.033;
                if (dust.X < -0.02) dust.X = 1.02;
                if (dust.X > 1.02) dust.X = -0.02;
                if (dust.Y < -0.02) dust.Y = 1.02;
                if (dust.Y > 1.02) dust.Y = -0.02;
            }

            for (int i = 0; i < _shooters.Length; i++)
            {
                Shooter shooter = _shooters[i];
                shooter.X += shooter.VX * 0.033;
                shooter.Y += shooter.VY * 0.033;
                shooter.Life += 0.033;
                if (shooter.Life > shooter.MaxLife ||
                    shooter.X < -0.2 || shooter.X > 1.2 ||
                    shooter.Y < -0.2 || shooter.Y > 1.2)
                {
                    _shooters[i] = NewShooter(false);
                }
            }
            InvalidateVisual();
        }

        private Shooter NewShooter(bool spread)
        {
            Shooter shooter = new Shooter();
            shooter.X = _random.NextDouble() * 0.8 + 0.1;
            shooter.Y = _random.NextDouble() * 0.55 + 0.02;
            double speed = 0.22 + _random.NextDouble() * 0.28;
            double angle = 0.55 + _random.NextDouble() * 0.35;
            shooter.VX = speed * Math.Cos(angle);
            shooter.VY = speed * Math.Sin(angle);
            shooter.MaxLife = spread ? 2.2 + _random.NextDouble() * 4.5 : 1.8 + _random.NextDouble() * 3.5;
            shooter.Life = spread ? _random.NextDouble() * shooter.MaxLife : 0;
            shooter.Length = 0.06 + _random.NextDouble() * 0.1;
            return shooter;
        }

        protected override void OnRender(DrawingContext context)
        {
            double width = ActualWidth;
            double height = ActualHeight;
            if (width < 8 || height < 8)
            {
                return;
            }

            context.DrawRectangle(_background, null, new Rect(0, 0, width, height));

            // 银河光带
            context.PushClip(new RectangleGeometry(new Rect(0, 0, width, height)));
            context.PushTransform(new RotateTransform(-22, width * 0.5, height * 0.5));
            Rect band = new Rect(-width * 0.2, height * 0.32, width * 1.4, height * 0.3);
            context.DrawRectangle(_milkyWay, null, band);
            context.Pop();
            context.Pop();

            DrawNebula(context, _nebulaA, width * 0.24, height * 0.3, width * 0.58, height * 0.5);
            DrawNebula(context, _nebulaB, width * 0.78, height * 0.58, width * 0.62, height * 0.6);
            DrawNebula(context, _nebulaC, width * 0.56, height * 0.14, width * 0.42, height * 0.34);
            DrawNebula(context, _nebulaD, width * 0.12, height * 0.82, width * 0.38, height * 0.3);

            for (int i = 0; i < _dust.Length; i++)
            {
                Dust dust = _dust[i];
                SolidColorBrush brush = new SolidColorBrush(WithAlpha(dust.Color,
                    (byte)(dust.Alpha * 255)));
                brush.Freeze();
                context.DrawEllipse(brush, null,
                    new Point(dust.X * width, dust.Y * height), dust.Radius, dust.Radius);
            }

            for (int i = 0; i < _stars.Length; i++)
            {
                Star star = _stars[i];
                double twinkle = 0.45 + 0.55 * (0.5 + 0.5 * Math.Sin(_time * star.Speed + star.Phase));
                byte alpha = (byte)Math.Max(0, Math.Min(255, star.BaseAlpha * twinkle * 255));
                SolidColorBrush brush = new SolidColorBrush(WithAlpha(star.Color, alpha));
                brush.Freeze();
                double x = star.X * width;
                double y = star.Y * height;
                if (star.Radius > 1.25)
                {
                    SolidColorBrush glow = new SolidColorBrush(WithAlpha(star.Color, (byte)(alpha / 4)));
                    glow.Freeze();
                    context.DrawEllipse(glow, null, new Point(x, y), star.Radius * 3.4, star.Radius * 3.4);
                }
                context.DrawEllipse(brush, null, new Point(x, y), star.Radius, star.Radius);
            }

            for (int i = 0; i < _shooters.Length; i++)
            {
                Shooter shooter = _shooters[i];
                double fade = 1.0 - Math.Abs((shooter.Life / shooter.MaxLife) - 0.5) * 1.2;
                if (fade <= 0) continue;
                Point head = new Point(shooter.X * width, shooter.Y * height);
                Point tail = new Point(
                    (shooter.X - shooter.VX * shooter.Length * 4) * width,
                    (shooter.Y - shooter.VY * shooter.Length * 4) * height);
                LinearGradientBrush trail = new LinearGradientBrush();
                trail.StartPoint = new Point(0, 0);
                trail.EndPoint = new Point(1, 0);
                trail.GradientStops.Add(new GradientStop(Color.FromArgb(0, 255, 255, 255), 0));
                trail.GradientStops.Add(new GradientStop(Color.FromArgb((byte)(210 * fade), 220, 240, 255), 0.75));
                trail.GradientStops.Add(new GradientStop(Color.FromArgb((byte)(240 * fade), 255, 255, 255), 1));
                trail.Freeze();
                Pen pen = new Pen(trail, 1.3);
                pen.Freeze();
                context.DrawLine(pen, tail, head);
            }
        }

        private static void DrawNebula(DrawingContext context, Brush brush,
            double x, double y, double width, double height)
        {
            context.DrawEllipse(brush, null, new Point(x, y), width * 0.5, height * 0.5);
        }

        private static Color Lighten(Color color, double amount)
        {
            int r = (int)Math.Min(255, color.R + (255 - color.R) * amount);
            int g = (int)Math.Min(255, color.G + (255 - color.G) * amount);
            int b = (int)Math.Min(255, color.B + (255 - color.B) * amount);
            return Color.FromRgb((byte)r, (byte)g, (byte)b);
        }

        private static Color WithAlpha(Color color, byte alpha)
        {
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }
    }
}
