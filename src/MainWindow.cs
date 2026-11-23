using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Forms = System.Windows.Forms;

namespace SeptemberCountdown
{
    public class MainWindow : Window
    {
        private AppSettings _settings;
        private Grid _root;
        private bool _settingsOpen;
        private bool _syncing;
        private bool _noAcrylic;
        private bool _closing;
        private bool _switchingMode;
        private bool _settingsExpanded;
        private bool _shownTrayTip;
        private bool _previewShimmer;
        private bool _previewShimmerGradient;
        private bool _previewGlassDigits;
        private bool _previewSandDigits;
        private bool _syncingDigitToggles;
        private bool _eggMenuOpen;
        private bool _checkingUpdate;
        private int _versionClickCount;
        private DateTime _lastVersionClick;
        private string _displayMode;
        private string _previewTheme;
        private string _previewDisplayMode;
        private string _lastSecondText;
        private DateTime _lastTarget;
        private double _compactLeft;
        private double _compactTop;
        private double _compactWidth;
        private double _compactHeight;
        private DispatcherTimer _timer;

        private TextBlock _nameText;
        private TextBlock _daysText;
        private TextBlock _hoursText;
        private TextBlock _minutesText;
        private TextBlock _secondsText;
        private TextBlock _statusText;
        private TextBlock _targetText;
        private TextBlock _progressText;
        private TextBlock _remainingHint;
        private TextBlock _validationText;
        private TextBlock _pinGlyph;
        private TextBlock _dayUnitText;
        private TextBlock _dayUnitCaption;
        private TextBlock _colon1;
        private TextBlock _colon2;
        private StackPanel _secondsGroup;
        private StackPanel _nameStack;
        private StackPanel _dayUnitPanel;
        private StackPanel _topRightInfo;
        private Border _countdownBadge;
        private Border _statusPill;
        private Ellipse _statusDot;
        private ProgressBar _progress;
        private ScrollViewer _settingsScroll;

        private TextBlock _daysRim;
        private TextBlock _daysSheen;
        private TextBlock _hoursRim;
        private TextBlock _hoursSheen;
        private TextBlock _minutesRim;
        private TextBlock _minutesSheen;
        private TextBlock _secondsRim;
        private TextBlock _secondsSheen;
        private Rectangle _daysSand;
        private Rectangle _daysSandFine;
        private Rectangle _daysSandFlow;
        private Rectangle _hoursSand;
        private Rectangle _hoursSandFine;
        private Rectangle _hoursSandFlow;
        private Rectangle _minutesSand;
        private Rectangle _minutesSandFine;
        private Rectangle _minutesSandFlow;
        private Rectangle _secondsSand;
        private Rectangle _secondsSandFine;
        private Rectangle _secondsSandFlow;
        private Grid _secondsHost;

        private TextBox _nameBox;
        private TextBox _dateBox;
        private TextBox _timeBox;
        private ToggleButton _topmostToggle;
        private ToggleButton _secondsToggle;
        private ToggleButton _autoStartToggle;
        private ToggleButton _shimmerToggle;
        private ToggleButton _shimmerGradientToggle;
        private ToggleButton _glassDigitsToggle;
        private ToggleButton _sandToggle;
        private ToggleButton _starModeToggle;
        private ToggleButton _pinButton;
        private Button _settingsButton;
        private Button _minButton;
        private Button _closeButton;
        private Button _settingsCloseButton;
        private Button _saveButton;
        private Button _cancelButton;
        private Button _resetButton;
        private Button _presetHourButton;
        private Button _presetTomorrowButton;
        private Button _presetNewYearButton;
        private Button _versionButton;
        private Button _eggCloseButton;
        private Button _eggAuthorButton;
        private Button _eggChallengeButton;
        private Button _updateCheckButton;
        private TextBlock _updateStatusText;
        private TextBox _updateFeedBox;
        private TextBox _starIdleBox;
        private StackPanel _starIdlePanel;
        private Button _starPreviewButton;
        private Button _themeAurora;
        private Button _themeOcean;
        private Button _themeSunset;
        private Button _themeMint;
        private Button _themeSakura;
        private Button _themeEmber;
        private Button _themeForest;
        private Button _themeNebula;
        private Button _effectGlass;
        private Button _effectLiquid;
        private Button _effectClear;

        private Grid _settingsOverlay;
        private Border _settingsPanel;
        private Border _shimmerGradientPanel;
        private Border _mainCard;
        private Grid _topBar;
        private Grid _cardContent;
        private Grid _footer;
        private Grid _progressArea;
        private Border _cardShadow;
        private Border _edgeShade;
        private Border _noiseLayer;
        private Grid _minimalGlow;
        private TranslateTransform _settingsSlide;
        private TranslateTransform _secondsTransform;
        private Canvas _blobCanvas;
        private Ellipse _blobA;
        private Ellipse _blobB;
        private Ellipse _blobC;
        private Ellipse _blobD;
        private FrameworkElement _cardVioletGlow;
        private FrameworkElement _cardCyanGlow;
        private FrameworkElement _cardSpecularGlow;
        private FrameworkElement _cardPurpleGlow;
        private FrameworkElement _cardTopHighlight;

        private Border _eggToast;
        private Grid _eggOverlay;
        private Border _eggPanel;
        private DispatcherTimer _eggTimer;
        private Popup _countdownPopup;
        private Button _menuTopmostButton;
        private Button _menuUpdateButton;
        private DispatcherTimer _starIdleTimer;
        private StarWindow _starWindow;

        private Forms.NotifyIcon _tray;
        private Forms.ToolStripMenuItem _trayTopmostItem;
        private System.Drawing.Icon _trayIcon;
        private bool _openSettingsOnLoad;

        private sealed class ThemePalette
        {
            public Color A;
            public Color B;
            public Color C;
            public Color D;
            public Color Accent1;
            public Color Accent2;
        }

        public MainWindow(bool openSettings, bool noAcrylic)
        {
            _openSettingsOnLoad = openSettings;
            _noAcrylic = noAcrylic;
            _settings = AppSettings.Load();
            StartupRegistration.SetEnabled(_settings.AutoStart);
            _displayMode = _settings.DisplayMode;
            _previewTheme = _settings.Theme;
            _previewDisplayMode = _displayMode;
            _previewShimmer = _settings.Shimmer;
            _previewShimmerGradient = _settings.ShimmerGradient;
            _previewGlassDigits = _settings.GlassDigits;
            _previewSandDigits = _settings.SandDigits;
            _lastTarget = _settings.GetTarget();

            LoadInterface();
            ConfigureWindow();
            BindControls();
            ApplyDisplayModeLayout();
            ApplyModelToWindow();
            ApplyTheme(_settings.Theme, false);
            UpdateCountdown(true);

            _timer = new DispatcherTimer(DispatcherPriority.Background);
            _timer.Interval = TimeSpan.FromMilliseconds(180);
            _timer.Tick += OnTimerTick;
            _timer.Start();

            _starIdleTimer = new DispatcherTimer(DispatcherPriority.Background);
            _starIdleTimer.Interval = TimeSpan.FromSeconds(1);
            _starIdleTimer.Tick += OnStarIdleTick;
            _starIdleTimer.Start();

            CreateTray();
        }

        private void LoadInterface()
        {
            string xaml = ReadTextResource("SeptemberCountdown.Ui.xaml");
            _root = (Grid)XamlReader.Parse(xaml);

            Stream noiseStream = GetResourceStream("SeptemberCountdown.Assets.noise.png");
            if (noiseStream != null)
            {
                BitmapImage noise = new BitmapImage();
                noise.BeginInit();
                noise.CacheOption = BitmapCacheOption.OnLoad;
                noise.StreamSource = noiseStream;
                noise.EndInit();
                noise.Freeze();
                ImageBrush noiseBrush = _root.FindName("NoiseBrush") as ImageBrush;
                if (noiseBrush != null)
                {
                    noiseBrush.ImageSource = noise;
                }
                noiseStream.Dispose();
            }

            Stream iconStream = GetResourceStream("SeptemberCountdown.Assets.app.ico");
            if (iconStream != null)
            {
                Icon = BitmapFrame.Create(iconStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
        }

        private void ConfigureWindow()
        {
            bool transparent = IsTransparentWindow();
            Title = "九月倒计时";
            Width = 680;
            Height = 300;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            AllowsTransparency = transparent;
            Background = transparent
                ? Brushes.Transparent
                : new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
            ShowInTaskbar = true;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Content = _root;
            Topmost = _settings.Topmost;

            RestorePosition();

            SourceInitialized += OnSourceInitialized;
            Loaded += OnWindowLoaded;
            Closing += OnWindowClosing;
            LocationChanged += OnWindowLocationChanged;
            PreviewKeyDown += OnWindowPreviewKeyDown;
        }

        private void BindControls()
        {
            _nameText = Find<TextBlock>("NameText");
            _daysText = Find<TextBlock>("DaysText");
            _hoursText = Find<TextBlock>("HoursText");
            _minutesText = Find<TextBlock>("MinutesText");
            _secondsText = Find<TextBlock>("SecondsText");
            _statusText = Find<TextBlock>("StatusText");
            _targetText = Find<TextBlock>("TargetText");
            _progressText = Find<TextBlock>("ProgressText");
            _remainingHint = Find<TextBlock>("RemainingHint");
            _validationText = Find<TextBlock>("ValidationText");
            _pinGlyph = Find<TextBlock>("PinGlyph");
            _dayUnitText = Find<TextBlock>("DayUnitText");
            _dayUnitCaption = Find<TextBlock>("DayUnitCaption");
            _colon1 = Find<TextBlock>("Colon1");
            _colon2 = Find<TextBlock>("Colon2");
            _secondsGroup = Find<StackPanel>("SecondsGroup");
            _secondsHost = Find<Grid>("SecondsHost");
            _nameStack = Find<StackPanel>("NameStack");
            _dayUnitPanel = Find<StackPanel>("DayUnitPanel");
            _topRightInfo = Find<StackPanel>("TopRightInfo");
            _countdownBadge = Find<Border>("CountdownBadge");
            _statusPill = Find<Border>("StatusPill");
            _statusDot = Find<Ellipse>("StatusDot");
            _progress = Find<ProgressBar>("Progress");
            _settingsScroll = Find<ScrollViewer>("SettingsScroll");

            _daysRim = Find<TextBlock>("DaysRim");
            _daysSheen = Find<TextBlock>("DaysSheen");
            _hoursRim = Find<TextBlock>("HoursRim");
            _hoursSheen = Find<TextBlock>("HoursSheen");
            _minutesRim = Find<TextBlock>("MinutesRim");
            _minutesSheen = Find<TextBlock>("MinutesSheen");
            _secondsRim = Find<TextBlock>("SecondsRim");
            _secondsSheen = Find<TextBlock>("SecondsSheen");
            _daysSand = Find<Rectangle>("DaysSand");
            _daysSandFine = Find<Rectangle>("DaysSandFine");
            _daysSandFlow = Find<Rectangle>("DaysSandFlow");
            _hoursSand = Find<Rectangle>("HoursSand");
            _hoursSandFine = Find<Rectangle>("HoursSandFine");
            _hoursSandFlow = Find<Rectangle>("HoursSandFlow");
            _minutesSand = Find<Rectangle>("MinutesSand");
            _minutesSandFine = Find<Rectangle>("MinutesSandFine");
            _minutesSandFlow = Find<Rectangle>("MinutesSandFlow");
            _secondsSand = Find<Rectangle>("SecondsSand");
            _secondsSandFine = Find<Rectangle>("SecondsSandFine");
            _secondsSandFlow = Find<Rectangle>("SecondsSandFlow");

            _nameBox = Find<TextBox>("NameBox");
            _dateBox = Find<TextBox>("DateBox");
            _timeBox = Find<TextBox>("TimeBox");
            _topmostToggle = Find<ToggleButton>("TopmostToggle");
            _secondsToggle = Find<ToggleButton>("SecondsToggle");
            _autoStartToggle = Find<ToggleButton>("AutoStartToggle");
            _shimmerToggle = Find<ToggleButton>("ShimmerToggle");
            _shimmerGradientToggle = Find<ToggleButton>("ShimmerGradientToggle");
            _glassDigitsToggle = Find<ToggleButton>("GlassDigitsToggle");
            _sandToggle = Find<ToggleButton>("SandToggle");
            _starModeToggle = Find<ToggleButton>("StarModeToggle");
            _starIdleBox = Find<TextBox>("StarIdleBox");
            _starIdlePanel = Find<StackPanel>("StarIdlePanel");
            _starPreviewButton = Find<Button>("StarPreviewButton");
            _pinButton = Find<ToggleButton>("PinButton");
            _settingsButton = Find<Button>("SettingsButton");
            _minButton = Find<Button>("MinButton");
            _closeButton = Find<Button>("CloseButton");
            _settingsCloseButton = Find<Button>("SettingsCloseButton");
            _saveButton = Find<Button>("SaveButton");
            _cancelButton = Find<Button>("CancelButton");
            _resetButton = Find<Button>("ResetButton");
            _presetHourButton = Find<Button>("PresetHourButton");
            _presetTomorrowButton = Find<Button>("PresetTomorrowButton");
            _presetNewYearButton = Find<Button>("PresetNewYearButton");
            _versionButton = Find<Button>("VersionButton");
            _eggCloseButton = Find<Button>("EggCloseButton");
            _eggAuthorButton = Find<Button>("EggAuthorButton");
            _eggChallengeButton = Find<Button>("EggChallengeButton");
            _updateCheckButton = Find<Button>("UpdateCheckButton");
            _updateStatusText = Find<TextBlock>("UpdateStatusText");
            _updateFeedBox = Find<TextBox>("UpdateFeedBox");
            _themeAurora = Find<Button>("ThemeAurora");
            _themeOcean = Find<Button>("ThemeOcean");
            _themeSunset = Find<Button>("ThemeSunset");
            _themeMint = Find<Button>("ThemeMint");
            _themeSakura = Find<Button>("ThemeSakura");
            _themeEmber = Find<Button>("ThemeEmber");
            _themeForest = Find<Button>("ThemeForest");
            _themeNebula = Find<Button>("ThemeNebula");
            _effectGlass = Find<Button>("EffectGlass");
            _effectLiquid = Find<Button>("EffectLiquid");
            _effectClear = Find<Button>("EffectClear");

            _settingsOverlay = Find<Grid>("SettingsOverlay");
            _settingsPanel = Find<Border>("SettingsPanel");
            _shimmerGradientPanel = Find<Border>("ShimmerGradientPanel");
            _mainCard = Find<Border>("MainCard");
            _topBar = Find<Grid>("TopBar");
            _cardContent = Find<Grid>("CardContent");
            _footer = Find<Grid>("Footer");
            _progressArea = Find<Grid>("ProgressArea");
            _cardShadow = Find<Border>("CardShadow");
            _edgeShade = Find<Border>("EdgeShade");
            _noiseLayer = Find<Border>("NoiseLayer");
            _minimalGlow = Find<Grid>("MinimalGlow");
            _settingsSlide = Find<TranslateTransform>("SettingsSlide");
            _secondsTransform = Find<TranslateTransform>("SecondsTransform");
            _blobCanvas = Find<Canvas>("BlobCanvas");
            _blobA = Find<Ellipse>("BlobA");
            _blobB = Find<Ellipse>("BlobB");
            _blobC = Find<Ellipse>("BlobC");
            _blobD = Find<Ellipse>("BlobD");
            _cardVioletGlow = Find<FrameworkElement>("CardVioletGlow");
            _cardCyanGlow = Find<FrameworkElement>("CardCyanGlow");
            _cardSpecularGlow = Find<FrameworkElement>("CardSpecularGlow");
            _cardPurpleGlow = Find<FrameworkElement>("CardPurpleGlow");
            _cardTopHighlight = Find<FrameworkElement>("CardTopHighlight");

            _eggToast = Find<Border>("EggToast");
            _eggOverlay = Find<Grid>("EggOverlay");
            _eggPanel = Find<Border>("EggPanel");

            _countdownPopup = Find<Popup>("CountdownPopup");
            Button menuSettingsButton = Find<Button>("MenuSettingsButton");
            _menuTopmostButton = Find<Button>("MenuTopmostButton");
            _menuUpdateButton = Find<Button>("MenuUpdateButton");
            Button menuExitButton = Find<Button>("MenuExitButton");
            if (menuSettingsButton != null)
            {
                menuSettingsButton.Click += delegate { OpenSettings(); };
            }
            if (_menuTopmostButton != null)
            {
                _menuTopmostButton.Click += delegate { ToggleTopmostFromTray(); };
            }
            if (_menuUpdateButton != null)
            {
                _menuUpdateButton.Click += delegate { CheckForUpdates(); };
            }
            if (menuExitButton != null)
            {
                menuExitButton.Click += delegate { Close(); };
            }

            _pinButton.Checked += OnPinChanged;
            _pinButton.Unchecked += OnPinChanged;
            _settingsButton.Click += OnSettingsButtonClick;
            _minButton.Click += OnMinButtonClick;
            _closeButton.Click += OnCloseButtonClick;

            _settingsCloseButton.Click += OnCancelSettingsClick;
            _cancelButton.Click += OnCancelSettingsClick;
            _saveButton.Click += OnSaveSettingsClick;
            _resetButton.Click += OnResetSettingsClick;
            _settingsOverlay.MouseLeftButtonDown += OnSettingsOverlayMouseDown;

            _presetHourButton.Click += OnPresetClick;
            _presetTomorrowButton.Click += OnPresetClick;
            _presetNewYearButton.Click += OnPresetClick;

            _themeAurora.Click += OnThemeClick;
            _themeOcean.Click += OnThemeClick;
            _themeSunset.Click += OnThemeClick;
            _themeMint.Click += OnThemeClick;
            _themeSakura.Click += OnThemeClick;
            _themeEmber.Click += OnThemeClick;
            _themeForest.Click += OnThemeClick;
            _themeNebula.Click += OnThemeClick;

            _effectGlass.Click += OnEffectClick;
            _effectLiquid.Click += OnEffectClick;
            _effectClear.Click += OnEffectClick;

            _shimmerToggle.Checked += OnDigitEffectToggleChanged;
            _shimmerToggle.Unchecked += OnDigitEffectToggleChanged;
            _shimmerGradientToggle.Checked += OnDigitEffectToggleChanged;
            _shimmerGradientToggle.Unchecked += OnDigitEffectToggleChanged;
            _glassDigitsToggle.Checked += OnDigitEffectToggleChanged;
            _glassDigitsToggle.Unchecked += OnDigitEffectToggleChanged;
            _sandToggle.Checked += OnDigitEffectToggleChanged;
            _sandToggle.Unchecked += OnDigitEffectToggleChanged;

            _starModeToggle.Checked += OnStarModeToggleChanged;
            _starModeToggle.Unchecked += OnStarModeToggleChanged;
            _starPreviewButton.Click += OnStarPreviewClick;

            _versionButton.Click += OnVersionButtonClick;
            _updateCheckButton.Click += OnUpdateCheckClick;
            _eggCloseButton.Click += OnEggCloseButtonClick;
            _eggAuthorButton.Click += OnEggAuthorButtonClick;
            _eggChallengeButton.Click += OnEggChallengeButtonClick;
            _eggOverlay.MouseLeftButtonDown += OnEggOverlayMouseDown;

            _nameBox.TextChanged += OnDraftTextChanged;
            _dateBox.TextChanged += OnDraftTextChanged;
            _timeBox.TextChanged += OnDraftTextChanged;
            _nameBox.KeyDown += OnEditorKeyDown;
            _dateBox.KeyDown += OnEditorKeyDown;
            _timeBox.KeyDown += OnEditorKeyDown;

            _topBar.MouseLeftButtonDown += OnDragAreaMouseDown;
            _mainCard.MouseLeftButtonDown += OnDragAreaMouseDown;
            _mainCard.MouseRightButtonUp += OnDragAreaMouseRightButtonUp;
        }

        private T Find<T>(string name) where T : class
        {
            return _root.FindName(name) as T;
        }

        private bool IsGlassMode()
        {
            return _displayMode == "glass";
        }

        private bool IsLiquidMode()
        {
            return _displayMode == "liquid";
        }

        private bool IsTransparentWindow()
        {
            return _displayMode != "glass" || _noAcrylic;
        }

        private bool UsesAcrylicGlass()
        {
            return _displayMode == "glass" && !_noAcrylic;
        }

        private void ApplyDisplayModeLayout()
        {
            bool glass = IsGlassMode();
            bool liquid = IsLiquidMode();

            _root.Background = UsesAcrylicGlass()
                ? new SolidColorBrush(Color.FromArgb(1, 0, 0, 0))
                : Brushes.Transparent;

            _topBar.Visibility = Visibility.Collapsed;
            _footer.Visibility = Visibility.Collapsed;
            _cardShadow.Visibility = Visibility.Collapsed;
            _edgeShade.Visibility = Visibility.Collapsed;
            _noiseLayer.Visibility = Visibility.Collapsed;
            _progressArea.Visibility = Visibility.Collapsed;
            _topRightInfo.Visibility = Visibility.Collapsed;
            _countdownBadge.Visibility = Visibility.Collapsed;
            _blobCanvas.Visibility = glass ? Visibility.Visible : Visibility.Collapsed;
            _minimalGlow.Visibility = (glass || liquid) ? Visibility.Visible : Visibility.Collapsed;
            _minimalGlow.Opacity = glass ? 0.7 : 1.0;

            _cardVioletGlow.Visibility = Visibility.Collapsed;
            _cardCyanGlow.Visibility = Visibility.Collapsed;
            _cardSpecularGlow.Visibility = Visibility.Collapsed;
            _cardPurpleGlow.Visibility = Visibility.Collapsed;
            _cardTopHighlight.Visibility = Visibility.Collapsed;

            _mainCard.Margin = new Thickness(0);
            _mainCard.CornerRadius = new CornerRadius(0);
            _mainCard.BorderThickness = new Thickness(0);
            _mainCard.BorderBrush = Brushes.Transparent;
            _mainCard.Background = Brushes.Transparent;
            _mainCard.Effect = null;
            _cardContent.Margin = new Thickness(24, 18, 24, 18);

            _nameStack.HorizontalAlignment = HorizontalAlignment.Center;
            _nameStack.Margin = new Thickness(0);
            _nameText.HorizontalAlignment = HorizontalAlignment.Center;
            _nameText.TextAlignment = TextAlignment.Center;
            _nameText.FontSize = 20;
            _nameText.Margin = new Thickness(0);
            _nameText.MaxWidth = 610;
            _nameText.Foreground = new SolidColorBrush(Color.FromRgb(247, 249, 255));

            _daysText.FontSize = 84;
            _daysText.LineHeight = 88;
            _daysText.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            _hoursText.FontSize = 36;
            _minutesText.FontSize = 36;
            _secondsText.FontSize = 36;
            _colon1.FontSize = 30;
            _colon2.FontSize = 30;
            _colon1.Margin = new Thickness(5, 0, 5, 2);
            _colon2.Margin = new Thickness(5, 0, 5, 2);
            _dayUnitPanel.Margin = new Thickness(10, 0, 0, 17);
            _dayUnitText.FontSize = 15;
            _dayUnitCaption.FontSize = 7.5;
            _dayUnitText.Foreground = new SolidColorBrush(Color.FromRgb(235, 241, 250));
            _dayUnitCaption.Foreground = new SolidColorBrush(Color.FromRgb(197, 210, 228));
            _colon1.Foreground = new SolidColorBrush(Color.FromRgb(190, 203, 221));
            _colon2.Foreground = new SolidColorBrush(Color.FromRgb(190, 203, 221));

            Color shadowColor = Color.FromArgb(225, 0, 0, 0);
            _nameText.Effect = CreateTextShadow(shadowColor, 12, 1);
            _dayUnitText.Effect = CreateTextShadow(shadowColor, 9, 1);
            _dayUnitCaption.Effect = CreateTextShadow(shadowColor, 8, 1);

            SyncDigitLayerGeometry();
            ApplyDigitEffects();
        }

        private static DropShadowEffect CreateTextShadow(Color color, double blur, double depth)
        {
            DropShadowEffect effect = new DropShadowEffect();
            effect.Color = color;
            effect.BlurRadius = blur;
            effect.ShadowDepth = depth;
            effect.Direction = 270;
            effect.Opacity = 0.85;
            return effect;
        }

        private static DropShadowEffect CreateTextGlow(Color color, double blur, double opacity)
        {
            DropShadowEffect effect = new DropShadowEffect();
            effect.Color = color;
            effect.BlurRadius = blur;
            effect.ShadowDepth = 0;
            effect.Direction = 270;
            effect.Opacity = opacity;
            return effect;
        }

        private void SyncDigitLayerGeometry()
        {
            SyncDigitLayerStyle(_daysText, _daysRim);
            SyncDigitLayerStyle(_daysText, _daysSheen);
            SyncDigitLayerStyle(_hoursText, _hoursRim);
            SyncDigitLayerStyle(_hoursText, _hoursSheen);
            SyncDigitLayerStyle(_minutesText, _minutesRim);
            SyncDigitLayerStyle(_minutesText, _minutesSheen);
            SyncDigitLayerStyle(_secondsText, _secondsRim);
            SyncDigitLayerStyle(_secondsText, _secondsSheen);
        }

        private static void SyncDigitLayerStyle(TextBlock source, TextBlock target)
        {
            if (source == null || target == null)
            {
                return;
            }
            target.Text = source.Text;
            target.FontFamily = source.FontFamily;
            target.FontSize = source.FontSize;
            target.FontWeight = source.FontWeight;
            target.FontStyle = source.FontStyle;
            target.FontStretch = source.FontStretch;
            target.LineHeight = source.LineHeight;
            target.LineStackingStrategy = source.LineStackingStrategy;
            target.TextAlignment = source.TextAlignment;
            Typography.SetNumeralAlignment(target, Typography.GetNumeralAlignment(source));
        }

        private void SyncDigitLayerText()
        {
            if (_daysText == null)
            {
                return;
            }
            if (_daysRim != null) _daysRim.Text = _daysText.Text;
            if (_daysSheen != null) _daysSheen.Text = _daysText.Text;
            if (_hoursRim != null) _hoursRim.Text = _hoursText.Text;
            if (_hoursSheen != null) _hoursSheen.Text = _hoursText.Text;
            if (_minutesRim != null) _minutesRim.Text = _minutesText.Text;
            if (_minutesSheen != null) _minutesSheen.Text = _minutesText.Text;
            if (_secondsRim != null) _secondsRim.Text = _secondsText.Text;
            if (_secondsSheen != null) _secondsSheen.Text = _secondsText.Text;
        }

        private void ApplyDigitEffects()
        {
            if (_daysText == null || _root == null)
            {
                return;
            }

            ThemePalette palette = GetPalette(_previewTheme);
            Color accent1 = palette.Accent1;
            Color accent2 = palette.Accent2;
            bool shimmer = _previewShimmer;
            bool shimmerGradient = _previewShimmerGradient;
            bool glass = _previewGlassDigits;
            bool sand = _previewSandDigits;
            Color darkShadow = Color.FromArgb(225, 0, 0, 0);

            LinearGradientBrush glassBase = BuildGlassBaseBrush(palette);
            LinearGradientBrush glassRim = BuildGlassRimBrush(palette);
            LinearGradientBrush shimmerSingle = BuildShimmerSingleBrush(palette);
            LinearGradientBrush shimmerBand = BuildShimmerBandBrush(palette);
            LinearGradientBrush sandBase = BuildSandBaseBrush();
            DrawingBrush sandGrain = sand ? BuildSandBrush(false, 52, 86) : null;

            LinearGradientBrush[] shimmerRandom = null;
            TranslateTransform[] shimmerRandomShifts = null;
            if (shimmer && shimmerGradient)
            {
                shimmerRandom = new LinearGradientBrush[4];
                shimmerRandomShifts = new TranslateTransform[4];
                Random shimmerRandomSource = new Random();
                for (int i = 0; i < shimmerRandom.Length; i++)
                {
                    shimmerRandom[i] = BuildShimmerGradientBrush(
                        palette, shimmerRandomSource, out shimmerRandomShifts[i]);
                }
            }

            Brush daysNormal = new SolidColorBrush(Color.FromRgb(248, 250, 255));
            Brush clockNormal = new SolidColorBrush(Color.FromRgb(223, 231, 248));
            Brush secondsNormal = new SolidColorBrush(Lighten(accent1, 0.42));

            TextBlock[] bases = new TextBlock[] { _daysText, _hoursText, _minutesText, _secondsText };
            TextBlock[] rims = new TextBlock[] { _daysRim, _hoursRim, _minutesRim, _secondsRim };
            TextBlock[] sheens = new TextBlock[] { _daysSheen, _hoursSheen, _minutesSheen, _secondsSheen };
            Rectangle[] sandLayers = new Rectangle[] { _daysSand, _hoursSand, _minutesSand, _secondsSand };
            Rectangle[] sandFineLayers = new Rectangle[] { _daysSandFine, _hoursSandFine, _minutesSandFine, _secondsSandFine };
            Rectangle[] sandFlowLayers = new Rectangle[] { _daysSandFlow, _hoursSandFlow, _minutesSandFlow, _secondsSandFlow };            Brush[] normals = new Brush[] { daysNormal, clockNormal, clockNormal, secondsNormal };
            double[] shadowBlurs = new double[] { 20, 13, 13, 13 };

            for (int i = 0; i < bases.Length; i++)
            {
                TextBlock baseBlock = bases[i];
                if (baseBlock == null)
                {
                    continue;
                }

                if (sand)
                {
                    baseBlock.Foreground = sandBase;
                }
                else if (glass)
                {
                    baseBlock.Foreground = glassBase;
                }
                else if (shimmer)
                {
                    baseBlock.Foreground = shimmerGradient ? shimmerRandom[i] : shimmerSingle;
                }
                else
                {
                    baseBlock.Foreground = normals[i];
                }
                baseBlock.Effect = CreateTextShadow(darkShadow, sand ? 14 : shadowBlurs[i], i == 0 ? 2 : 1);

                TextBlock rim = rims[i];
                if (rim != null)
                {
                    rim.Foreground = glassRim;
                    rim.Visibility = glass ? Visibility.Visible : Visibility.Collapsed;
                    rim.Effect = glass
                        ? CreateTextGlow(Lighten(accent2, 0.55), 15, 0.5)
                        : null;
                }

                TextBlock sheen = sheens[i];
                if (sheen != null)
                {
                    sheen.Foreground = shimmerBand;
                    sheen.Visibility = shimmer ? Visibility.Visible : Visibility.Collapsed;
                    sheen.Effect = shimmer
                        ? CreateTextGlow(Lighten(accent2, 0.62), 17, 0.55)
                        : null;
                }

                Rectangle sandLayer = sandLayers[i];
                Rectangle sandFineLayer = sandFineLayers[i];
                Rectangle sandFlowLayer = sandFlowLayers[i];
                if (sand)
                {
                    ApplySandLayer(sandLayer, baseBlock, sandGrain, 0.62);
                    ClearSandLayer(sandFineLayer);
                    ClearSandLayer(sandFlowLayer);
                }
                else
                {
                    ClearSandLayer(sandLayer);
                    ClearSandLayer(sandFineLayer);
                    ClearSandLayer(sandFlowLayer);
                }
            }

            if (shimmer)
            {
                if (!glass && !sand)
                {
                    if (shimmerGradient)
                    {
                        for (int i = 0; i < shimmerRandomShifts.Length; i++)
                        {
                            StartShimmerGradientAnimation(shimmerRandomShifts[i], 4.6 + i * 0.55);
                        }
                    }
                    else
                    {
                        StartFlowAnimation(shimmerSingle, 8.5);
                    }
                }
                StartFlowAnimation(shimmerBand, 2.9);
            }

            if (sand)
            {
                StartSandBandAnimation(sandBase, 7.6);
                StartSandFlow(sandGrain, 52, 6.5, 0, 1);
            }

            Brush punctuation;
            if (sand)
            {
                punctuation = sandBase;
            }
            else if (glass)
            {
                punctuation = glassBase;
            }
            else if (shimmer)
            {
                punctuation = shimmerGradient ? shimmerRandom[0] : shimmerSingle;
            }
            else
            {
                punctuation = new SolidColorBrush(Color.FromRgb(190, 203, 221));
            }
            _colon1.Foreground = punctuation;
            _colon2.Foreground = punctuation;
            _colon1.Effect = CreateTextShadow(darkShadow, 9, 1);
            _colon2.Effect = CreateTextShadow(darkShadow, 9, 1);

            if (sand)
            {
                _dayUnitText.Foreground = new SolidColorBrush(Color.FromRgb(255, 231, 188));
                _dayUnitCaption.Foreground = new SolidColorBrush(Color.FromRgb(226, 189, 133));
            }
            else if (glass)
            {
                _dayUnitText.Foreground = new SolidColorBrush(Color.FromArgb(240, 236, 246, 255));
                _dayUnitCaption.Foreground = new SolidColorBrush(Color.FromArgb(195, 203, 224, 244));
            }
            else if (shimmer)
            {
                _dayUnitText.Foreground = new SolidColorBrush(Lighten(accent1, 0.55));
                _dayUnitCaption.Foreground = new SolidColorBrush(Lighten(accent2, 0.55));
            }
            else
            {
                _dayUnitText.Foreground = new SolidColorBrush(Color.FromRgb(235, 241, 250));
                _dayUnitCaption.Foreground = new SolidColorBrush(Color.FromRgb(197, 210, 228));
            }
            _dayUnitText.Effect = CreateTextShadow(darkShadow, 9, 1);
            _dayUnitCaption.Effect = CreateTextShadow(darkShadow, 8, 1);
        }

        private static LinearGradientBrush BuildGlassBaseBrush(ThemePalette palette)
        {
            LinearGradientBrush brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0, 0);
            brush.EndPoint = new Point(0, 1);
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xE6, 255, 255, 255), 0.0));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xB3, 214, 238, 255), 0.07));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x5C, 138, 190, 226), 0.20));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x2A, 96, 150, 200), 0.38));
            brush.GradientStops.Add(new GradientStop(WithAlpha(Lighten(palette.Accent2, 0.6), 0xBE), 0.47));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x33, 96, 150, 200), 0.56));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x4D, 130, 180, 220), 0.76));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x99, 200, 226, 250), 0.90));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xE6, 240, 248, 255), 0.98));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xF5, 255, 255, 255), 1.0));
            return brush;
        }

        private static LinearGradientBrush BuildGlassRimBrush(ThemePalette palette)
        {
            LinearGradientBrush brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0, 0);
            brush.EndPoint = new Point(0, 1);
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 255, 255, 255), 0.0));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x8C, 255, 255, 255), 0.035));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x00, 255, 255, 255), 0.11));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x00, 255, 255, 255), 0.89));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x8C, 255, 255, 255), 0.965));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 255, 255, 255), 1.0));
            return brush;
        }

        private static LinearGradientBrush BuildShimmerSingleBrush(ThemePalette palette)
        {
            LinearGradientBrush brush = new LinearGradientBrush();
            brush.StartPoint = new Point(-1, 0);
            brush.EndPoint = new Point(0, 0);
            brush.SpreadMethod = GradientSpreadMethod.Repeat;
            brush.GradientStops.Add(new GradientStop(Lighten(palette.Accent1, 0.42), 0.0));
            brush.GradientStops.Add(new GradientStop(Lighten(palette.Accent1, 0.82), 0.22));
            brush.GradientStops.Add(new GradientStop(Lighten(palette.Accent1, 0.28), 0.45));
            brush.GradientStops.Add(new GradientStop(Lighten(palette.Accent1, 0.68), 0.68));
            brush.GradientStops.Add(new GradientStop(Lighten(palette.Accent1, 0.42), 1.0));
            return brush;
        }

        private static LinearGradientBrush BuildShimmerGradientBrush(
            ThemePalette palette, Random random, out TranslateTransform shift)
        {
            Color[] colors = new Color[]
            {
                Color.FromRgb(139, 92, 246),
                Color.FromRgb(34, 211, 238),
                Color.FromRgb(255, 95, 162),
                Color.FromRgb(255, 184, 107),
                Color.FromRgb(74, 222, 128),
                Color.FromRgb(245, 250, 255)
            };

            int offset = random.Next(colors.Length);
            LinearGradientBrush brush = new LinearGradientBrush();
            brush.StartPoint = new Point(-1, 0);
            brush.EndPoint = new Point(0, 0);
            brush.SpreadMethod = GradientSpreadMethod.Repeat;
            for (int i = 0; i < colors.Length; i++)
            {
                brush.GradientStops.Add(new GradientStop(
                    colors[(i + offset) % colors.Length], i / (double)colors.Length));
            }
            brush.GradientStops.Add(new GradientStop(colors[offset], 1.0));

            double angle = random.NextDouble() * 360.0;
            shift = new TranslateTransform(0, 0);
            TransformGroup group = new TransformGroup();
            group.Children.Add(shift);
            group.Children.Add(new RotateTransform(angle, 0.5, 0.5));
            brush.RelativeTransform = group;
            return brush;
        }

        private static void StartShimmerGradientAnimation(TranslateTransform shift, double seconds)
        {
            if (shift == null)
            {
                return;
            }
            DoubleAnimation animation = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(seconds));
            animation.RepeatBehavior = RepeatBehavior.Forever;
            Timeline.SetDesiredFrameRate(animation, 30);
            shift.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private static LinearGradientBrush BuildShimmerBandBrush(ThemePalette palette)
        {
            LinearGradientBrush brush = new LinearGradientBrush();
            brush.StartPoint = new Point(-1, 0);
            brush.EndPoint = new Point(0, 0);
            brush.SpreadMethod = GradientSpreadMethod.Repeat;
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x00, 255, 255, 255), 0.0));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x00, 255, 255, 255), 0.37));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x33, 255, 255, 255), 0.44));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xD9, 245, 252, 255), 0.50));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x33, 255, 255, 255), 0.56));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x00, 255, 255, 255), 0.63));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x00, 255, 255, 255), 1.0));
            return brush;
        }

        private static void StartFlowAnimation(LinearGradientBrush brush, double seconds)
        {
            TranslateTransform shift = new TranslateTransform(0, 0);
            brush.RelativeTransform = shift;
            DoubleAnimation animation = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(seconds));
            animation.RepeatBehavior = RepeatBehavior.Forever;
            Timeline.SetDesiredFrameRate(animation, 30);
            shift.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private static void ApplySandLayer(Rectangle layer, TextBlock maskSource, Brush fill, double opacity)
        {
            if (layer == null || maskSource == null || fill == null)
            {
                return;
            }
            VisualBrush mask = new VisualBrush();
            mask.Visual = maskSource;
            mask.Stretch = Stretch.Fill;
            mask.ViewboxUnits = BrushMappingMode.RelativeToBoundingBox;
            mask.Viewbox = new Rect(0, 0, 1, 1);
            layer.Fill = fill;
            layer.OpacityMask = mask;
            layer.Opacity = opacity;
            layer.Visibility = Visibility.Visible;
        }

        private static void ClearSandLayer(Rectangle layer)
        {
            if (layer == null)
            {
                return;
            }
            layer.Visibility = Visibility.Collapsed;
            layer.Fill = null;
            layer.OpacityMask = null;
        }

        private static void StartSandFlow(DrawingBrush brush, double tile, double seconds, int dirX, int dirY)
        {
            if (brush == null)
            {
                return;
            }
            TranslateTransform shift = new TranslateTransform(0, 0);
            brush.Transform = shift;
            Duration duration = new Duration(TimeSpan.FromSeconds(seconds));
            DoubleAnimation moveX = new DoubleAnimation(0, dirX * tile, duration);
            moveX.RepeatBehavior = RepeatBehavior.Forever;
            DoubleAnimation moveY = new DoubleAnimation(0, dirY * tile, duration);
            moveY.RepeatBehavior = RepeatBehavior.Forever;
            Timeline.SetDesiredFrameRate(moveX, 18);
            Timeline.SetDesiredFrameRate(moveY, 18);
            shift.BeginAnimation(TranslateTransform.XProperty, moveX);
            shift.BeginAnimation(TranslateTransform.YProperty, moveY);
        }

        private static LinearGradientBrush BuildSandBaseBrush()
        {
            LinearGradientBrush brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0, -1);
            brush.EndPoint = new Point(0, 0);
            brush.SpreadMethod = GradientSpreadMethod.Repeat;
            brush.GradientStops.Add(new GradientStop(Color.FromRgb(255, 246, 224), 0.0));
            brush.GradientStops.Add(new GradientStop(Color.FromRgb(245, 208, 148), 0.12));
            brush.GradientStops.Add(new GradientStop(Color.FromRgb(224, 172, 104), 0.26));
            brush.GradientStops.Add(new GradientStop(Color.FromRgb(201, 136, 72), 0.40));
            brush.GradientStops.Add(new GradientStop(Color.FromRgb(168, 102, 42), 0.52));
            brush.GradientStops.Add(new GradientStop(Color.FromRgb(201, 136, 72), 0.64));
            brush.GradientStops.Add(new GradientStop(Color.FromRgb(232, 188, 124), 0.78));
            brush.GradientStops.Add(new GradientStop(Color.FromRgb(255, 240, 200), 0.90));
            brush.GradientStops.Add(new GradientStop(Color.FromRgb(255, 246, 224), 1.0));
            return brush;
        }

        private static LinearGradientBrush BuildSandFlowBrush()
        {
            LinearGradientBrush brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0, -1);
            brush.EndPoint = new Point(0, 0);
            brush.SpreadMethod = GradientSpreadMethod.Repeat;
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x00, 255, 236, 190), 0.0));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x00, 255, 236, 190), 0.28));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x36, 255, 236, 190), 0.40));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xBC, 255, 248, 226), 0.50));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x36, 255, 236, 190), 0.60));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x00, 255, 236, 190), 0.72));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x00, 255, 236, 190), 1.0));
            return brush;
        }

        private static void StartSandBandAnimation(LinearGradientBrush brush, double seconds)
        {
            if (brush == null)
            {
                return;
            }
            TranslateTransform shift = new TranslateTransform(0, 0);
            brush.RelativeTransform = shift;
            DoubleAnimation animation = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(seconds));
            animation.RepeatBehavior = RepeatBehavior.Forever;
            Timeline.SetDesiredFrameRate(animation, 18);
            shift.BeginAnimation(TranslateTransform.YProperty, animation);
        }

        private static DrawingBrush BuildSandBrush(bool fine, int tile, int count)
        {
            Color[] colors = new Color[]
            {
                Color.FromRgb(255, 244, 220),
                Color.FromRgb(255, 228, 178),
                Color.FromRgb(246, 202, 132),
                Color.FromRgb(230, 168, 92),
                Color.FromRgb(198, 128, 56),
                Color.FromRgb(255, 249, 232)
            };

            DrawingGroup group = new DrawingGroup();
            Random random = new Random(fine ? 9173 : 5171);
            using (DrawingContext context = group.Open())
            {
                for (int i = 0; i < count; i++)
                {
                    double x = random.NextDouble() * tile;
                    double y = random.NextDouble() * tile;
                    bool large = random.Next(100) < 22;
                    double radius = large
                        ? 1.3 + random.NextDouble() * 1.2
                        : 0.5 + random.NextDouble() * 0.5;
                    int colorIndex = random.Next(colors.Length);
                    int alpha = large ? 125 + random.Next(75) : 115 + random.Next(75);
                    if (random.Next(100) < 3)
                    {
                        colorIndex = colors.Length - 1;
                        alpha = 205;
                    }
                    Color color = colors[colorIndex];
                    SolidColorBrush dotBrush = new SolidColorBrush(
                        Color.FromArgb((byte)alpha, color.R, color.G, color.B));
                    dotBrush.Freeze();
                    DrawSandDot(context, dotBrush, x, y, radius, tile);
                }
            }
            group.Freeze();

            DrawingBrush brush = new DrawingBrush(group);
            brush.TileMode = TileMode.Tile;
            brush.ViewportUnits = BrushMappingMode.Absolute;
            brush.Viewport = new Rect(0, 0, tile, tile);
            brush.ViewboxUnits = BrushMappingMode.Absolute;
            brush.Viewbox = new Rect(0, 0, tile, tile);
            brush.Stretch = Stretch.Fill;
            return brush;
        }

        private static void DrawSandDot(DrawingContext context, Brush brush, double x, double y,
            double radius, int tile)
        {
            double radiusY = radius * 0.82;
            context.DrawEllipse(brush, null, new Point(x, y), radius, radiusY);
            bool left = x < radius;
            bool right = x > tile - radius;
            bool top = y < radiusY;
            bool bottom = y > tile - radiusY;
            if (left)
            {
                context.DrawEllipse(brush, null, new Point(x + tile, y), radius, radiusY);
            }
            if (right)
            {
                context.DrawEllipse(brush, null, new Point(x - tile, y), radius, radiusY);
            }
            if (top)
            {
                context.DrawEllipse(brush, null, new Point(x, y + tile), radius, radiusY);
            }
            if (bottom)
            {
                context.DrawEllipse(brush, null, new Point(x, y - tile), radius, radiusY);
            }
            if (left && top)
            {
                context.DrawEllipse(brush, null, new Point(x + tile, y + tile), radius, radiusY);
            }
            if (right && top)
            {
                context.DrawEllipse(brush, null, new Point(x - tile, y + tile), radius, radiusY);
            }
            if (left && bottom)
            {
                context.DrawEllipse(brush, null, new Point(x + tile, y - tile), radius, radiusY);
            }
            if (right && bottom)
            {
                context.DrawEllipse(brush, null, new Point(x - tile, y - tile), radius, radiusY);
            }
        }

        private void RestorePosition()
        {
            double left;
            double top;
            if (_settings.HasPosition)
            {
                left = _settings.Left;
                top = _settings.Top;
            }
            else
            {
                Rect work = SystemParameters.WorkArea;
                left = work.Left + (work.Width - Width) / 2.0;
                top = work.Top + (work.Height - Height) / 2.0 - 18;
            }

            double virtualLeft = SystemParameters.VirtualScreenLeft;
            double virtualTop = SystemParameters.VirtualScreenTop;
            double virtualWidth = SystemParameters.VirtualScreenWidth;
            double virtualHeight = SystemParameters.VirtualScreenHeight;

            left = Math.Max(virtualLeft + 8, Math.Min(left, virtualLeft + virtualWidth - Width - 8));
            top = Math.Max(virtualTop + 8, Math.Min(top, virtualTop + virtualHeight - Height - 8));
            Left = left;
            Top = top;
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            if (UsesAcrylicGlass())
            {
                NativeMethods.ApplyWindowEffects(hwnd);
            }
            NativeMethods.SetTopmost(hwnd, _settings.Topmost);
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            _root.Opacity = 0;
            DoubleAnimation fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(420));
            fade.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            _root.BeginAnimation(OpacityProperty, fade);

            TranslateTransform cardShift = new TranslateTransform(0, 10);
            _mainCard.RenderTransform = cardShift;
            DoubleAnimation shift = new DoubleAnimation(10, 0, TimeSpan.FromMilliseconds(520));
            shift.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            cardShift.BeginAnimation(TranslateTransform.YProperty, shift);

            if (_openSettingsOnLoad)
            {
                Dispatcher.BeginInvoke(new Action(OpenSettings), DispatcherPriority.ApplicationIdle);
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(Keyboard.ClearFocus), DispatcherPriority.Input);
            }

            if (!_settings.HasPosition)
            {
                _settings.HasPosition = true;
                _settings.Left = Left;
                _settings.Top = Top;
                _settings.Save();
            }
        }

        private void OnWindowLocationChanged(object sender, EventArgs e)
        {
            if (_switchingMode || _settingsExpanded || !IsLoaded)
            {
                return;
            }
            _settings.HasPosition = true;
            _settings.Left = Left;
            _settings.Top = Top;
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_closing)
            {
                return;
            }
            _closing = true;
            try
            {
                _timer.Stop();
                if (_starIdleTimer != null)
                {
                    _starIdleTimer.Stop();
                }
                StopEggTimer();
                if (_starWindow != null)
                {
                    try
                    {
                        _starWindow.Close();
                    }
                    catch
                    {
                    }
                    _starWindow = null;
                }
                if (!_switchingMode)
                {
                    _settings.HasPosition = true;
                    if (_settingsExpanded)
                    {
                        _settings.Left = _compactLeft;
                        _settings.Top = _compactTop;
                    }
                    else
                    {
                        _settings.Left = Left;
                        _settings.Top = Top;
                    }
                }
                _settings.Save();
            }
            catch
            {
            }

            if (_tray != null)
            {
                _tray.Visible = false;
                _tray.Dispose();
                _tray = null;
            }
            if (_trayIcon != null)
            {
                _trayIcon.Dispose();
                _trayIcon = null;
            }

            if (!_switchingMode)
            {
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    Application.Current.Shutdown(0);
                }), DispatcherPriority.Background);
            }
        }

        private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_eggMenuOpen && e.Key == Key.Escape)
            {
                HideEggMenu();
                e.Handled = true;
                return;
            }
            if (!_settingsOpen)
            {
                return;
            }
            if (e.Key == Key.Escape)
            {
                CancelSettings();
                e.Handled = true;
            }
            else if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                SaveSettings();
                e.Handled = true;
            }
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            UpdateCountdown(false);
        }

        private void UpdateCountdown(bool force)
        {
            DateTime now = DateTime.Now;
            DateTime target = _settings.GetTarget();
            DateTime start = _settings.GetStartAt();
            TimeSpan remaining = target - now;
            bool ended = remaining.TotalSeconds <= 0;

            long totalSeconds = ended ? 0 : (long)Math.Floor(remaining.TotalSeconds);
            int days = (int)(totalSeconds / 86400);
            int hours = (int)((totalSeconds % 86400) / 3600);
            int minutes = (int)((totalSeconds % 3600) / 60);
            int seconds = (int)(totalSeconds % 60);

            _daysText.Text = days.ToString(CultureInfo.InvariantCulture);
            _hoursText.Text = hours.ToString("00", CultureInfo.InvariantCulture);
            _minutesText.Text = minutes.ToString("00", CultureInfo.InvariantCulture);
            string secondText = seconds.ToString("00", CultureInfo.InvariantCulture);
            if (force || secondText != _lastSecondText)
            {
                _secondsText.Text = secondText;
                if (!force && !ended)
                {
                    AnimateSecond();
                }
                _lastSecondText = secondText;
            }

            _nameText.Text = _settings.Name;
            _secondsGroup.Visibility = _settings.ShowSeconds ? Visibility.Visible : Visibility.Collapsed;
            _targetText.Text = "目标 · " + target.ToString("yyyy.MM.dd HH:mm", CultureInfo.InvariantCulture);
            SyncDigitLayerText();

            double progress = 0;
            double span = (target - start).TotalSeconds;
            if (span > 1)
            {
                progress = (now - start).TotalSeconds / span;
            }
            if (ended)
            {
                progress = 1;
            }
            progress = Math.Max(0, Math.Min(1, progress));
            _progress.Value = progress * 100;
            _progressText.Text = "时间进度 " + (progress * 100).ToString("0.0", CultureInfo.InvariantCulture) + "%";

            if (ended)
            {
                _remainingHint.Text = "已经到达目标时间";
                SetStatus("时间到", Color.FromRgb(251, 191, 36), Color.FromRgb(255, 224, 155),
                    Color.FromArgb(28, 251, 191, 36), Color.FromArgb(62, 251, 191, 36));
            }
            else if (days == 0 && hours == 0)
            {
                _remainingHint.Text = "还剩 " + minutes + " 分 " + seconds + " 秒";
                SetStatus("即将到达", Color.FromRgb(255, 111, 174), Color.FromRgb(255, 179, 211),
                    Color.FromArgb(26, 255, 95, 162), Color.FromArgb(60, 255, 95, 162));
            }
            else
            {
                if (days > 0)
                {
                    _remainingHint.Text = "还剩 " + days + " 天";
                }
                else if (hours > 0)
                {
                    _remainingHint.Text = "还剩 " + hours + " 小时";
                }
                else
                {
                    _remainingHint.Text = "还剩 " + minutes + " 分钟";
                }
                SetStatus("进行中", Color.FromRgb(74, 222, 128), Color.FromRgb(159, 233, 199),
                    Color.FromArgb(20, 34, 197, 94), Color.FromArgb(51, 34, 197, 94));
            }

            if (force || _lastTarget != target)
            {
                _lastTarget = target;
            }
        }

        private void SetStatus(string text, Color dotColor, Color textColor, Color backgroundColor, Color borderColor)
        {
            if (_statusText.Text != text)
            {
                _statusText.Text = text;
                _statusText.Foreground = new SolidColorBrush(textColor);
                _statusDot.Fill = new SolidColorBrush(dotColor);
                _statusPill.Background = new SolidColorBrush(backgroundColor);
                _statusPill.BorderBrush = new SolidColorBrush(borderColor);
                DropShadowEffect glow = new DropShadowEffect();
                glow.BlurRadius = 7;
                glow.ShadowDepth = 0;
                glow.Color = dotColor;
                glow.Opacity = 0.9;
                _statusDot.Effect = glow;
            }
        }

        private void AnimateSecond()
        {
            DoubleAnimation fade = new DoubleAnimation(0.52, 1, TimeSpan.FromMilliseconds(220));
            fade.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            _secondsHost.BeginAnimation(OpacityProperty, fade);

            DoubleAnimation slide = new DoubleAnimation(3.2, 0, TimeSpan.FromMilliseconds(240));
            slide.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            _secondsTransform.BeginAnimation(TranslateTransform.YProperty, slide);
        }

        private void ApplyModelToWindow()
        {
            _nameText.Text = _settings.Name;
            _secondsGroup.Visibility = _settings.ShowSeconds ? Visibility.Visible : Visibility.Collapsed;

            _syncing = true;
            _pinButton.IsChecked = _settings.Topmost;
            _syncing = false;
            UpdatePinVisual();

            ApplyTopmost(_settings.Topmost);
            UpdateCountdown(true);
        }

        private void UpdatePinVisual()
        {
            if (_pinGlyph != null)
            {
                _pinGlyph.Text = _settings.Topmost ? "\uE840" : "\uE718";
            }
            if (_pinButton != null)
            {
                _pinButton.ToolTip = _settings.Topmost ? "取消桌面置顶" : "桌面置顶";
            }
        }

        private void ApplyTopmost(bool topmost)
        {
            Topmost = topmost;
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd != IntPtr.Zero)
            {
                NativeMethods.SetTopmost(hwnd, topmost);
            }
        }

        private void OnPinChanged(object sender, RoutedEventArgs e)
        {
            if (_syncing)
            {
                return;
            }
            _settings.Topmost = _pinButton.IsChecked == true;
            UpdatePinVisual();
            ApplyTopmost(_settings.Topmost);
            _settings.Save();
            if (_settingsOpen)
            {
                _syncing = true;
                _topmostToggle.IsChecked = _settings.Topmost;
                _syncing = false;
            }
            if (_trayTopmostItem != null)
            {
                _trayTopmostItem.Checked = _settings.Topmost;
            }
        }

        private void OnDragAreaMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_settingsOpen || e.ChangedButton != MouseButton.Left || IsInsideInteractiveControl(e.OriginalSource))
            {
                return;
            }
            if (e.ClickCount == 2)
            {
                OpenSettings();
                e.Handled = true;
                return;
            }
            try
            {
                DragMove();
            }
            catch
            {
            }
            _settings.HasPosition = true;
            _settings.Left = Left;
            _settings.Top = Top;
            _settings.Save();
        }

        private static bool IsInsideInteractiveControl(object source)
        {
            DependencyObject current = source as DependencyObject;
            while (current != null)
            {
                if (current is ButtonBase || current is TextBox || current is ScrollBar)
                {
                    return true;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return false;
        }

        private void OnSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            OpenSettings();
        }

        private void OnMinButtonClick(object sender, RoutedEventArgs e)
        {
            HideToTray();
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ExpandForSettings()
        {
            _compactLeft = Left;
            _compactTop = Top;
            _compactWidth = Width;
            _compactHeight = Height;
            _settingsExpanded = true;

            double centerX = Left + Width / 2.0;
            double centerY = Top + Height / 2.0;
            Width = 780;
            Height = 580;
            MoveWindowClamped(centerX - Width / 2.0, centerY - Height / 2.0);
        }

        private void RestoreCompactWindow()
        {
            if (!_settingsExpanded)
            {
                return;
            }
            _settingsExpanded = false;
            Width = _compactWidth;
            Height = _compactHeight;
            MoveWindowClamped(_compactLeft, _compactTop);
        }

        private void MoveWindowClamped(double left, double top)
        {
            double virtualLeft = SystemParameters.VirtualScreenLeft;
            double virtualTop = SystemParameters.VirtualScreenTop;
            double virtualWidth = SystemParameters.VirtualScreenWidth;
            double virtualHeight = SystemParameters.VirtualScreenHeight;
            if (Width > virtualWidth)
            {
                Width = virtualWidth;
            }
            if (Height > virtualHeight)
            {
                Height = virtualHeight;
            }
            left = Math.Max(virtualLeft + 8, Math.Min(left, virtualLeft + virtualWidth - Width - 8));
            top = Math.Max(virtualTop + 8, Math.Min(top, virtualTop + virtualHeight - Height - 8));
            Left = left;
            Top = top;
        }

        private void OpenSettings()
        {
            if (_settingsOpen)
            {
                return;
            }
            _settingsOpen = true;
            if (_countdownPopup != null)
            {
                _countdownPopup.IsOpen = false;
            }
            _previewTheme = _settings.Theme;
            _previewDisplayMode = _displayMode;
            _previewShimmer = _settings.Shimmer;
            _previewGlassDigits = _settings.GlassDigits;
            _previewSandDigits = _settings.SandDigits;
            if (!_settingsExpanded)
            {
                ExpandForSettings();
            }
            PopulateSettingsForm();
            UpdateThemeSelection(_previewTheme);
            UpdateEffectSelection(_previewDisplayMode);
            ApplyDigitEffects();
            _settingsOverlay.Background = IsTransparentWindow()
                ? new SolidColorBrush(Color.FromArgb(85, 3, 9, 18))
                : new SolidColorBrush(Color.FromArgb(119, 3, 9, 18));

            _settingsOverlay.Visibility = Visibility.Visible;
            _settingsOverlay.Opacity = 0;
            _settingsSlide.X = 430;

            DoubleAnimation fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(170));
            _settingsOverlay.BeginAnimation(OpacityProperty, fade);

            DoubleAnimation slide = new DoubleAnimation(430, 0, TimeSpan.FromMilliseconds(300));
            slide.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            _settingsSlide.BeginAnimation(TranslateTransform.XProperty, slide);

            Dispatcher.BeginInvoke(new Action(delegate
            {
                _nameBox.Focus();
                _nameBox.CaretIndex = _nameBox.Text == null ? 0 : _nameBox.Text.Length;
            }), DispatcherPriority.Input);
        }

        private void PopulateSettingsForm()
        {
            DateTime target = _settings.GetTarget();
            _nameBox.Text = _settings.Name;
            _dateBox.Text = target.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            _timeBox.Text = target.ToString("HH:mm", CultureInfo.InvariantCulture);
            _topmostToggle.IsChecked = _settings.Topmost;
            _secondsToggle.IsChecked = _settings.ShowSeconds;
            _autoStartToggle.IsChecked = _settings.AutoStart;
            _starModeToggle.IsChecked = _settings.StarMode;
            _starIdleBox.Text = _settings.StarIdleMinutes.ToString(CultureInfo.InvariantCulture);
            UpdateStarIdlePanelVisibility();
            _syncingDigitToggles = true;
            _shimmerToggle.IsChecked = _settings.Shimmer;
            _shimmerGradientToggle.IsChecked = _settings.ShimmerGradient;
            _glassDigitsToggle.IsChecked = _settings.GlassDigits;
            _sandToggle.IsChecked = _settings.SandDigits;
            _syncingDigitToggles = false;
            _previewShimmer = _settings.Shimmer;
            _previewShimmerGradient = _settings.ShimmerGradient;
            _previewGlassDigits = _settings.GlassDigits;
            _previewSandDigits = _settings.SandDigits;
            UpdateShimmerGradientVisibility();
            _previewDisplayMode = _displayMode;
            UpdateEffectSelection(_previewDisplayMode);
            _updateFeedBox.Text = _settings.UpdateFeed;
            if (!_checkingUpdate)
            {
                _updateStatusText.Text = "当前版本 v" + UpdateService.CurrentVersion.ToString(3);
                _updateStatusText.Foreground = new SolidColorBrush(Color.FromRgb(138, 153, 175));
                _updateCheckButton.IsEnabled = true;
            }
            _validationText.Text = "";
            if (_settingsScroll != null)
            {
                _settingsScroll.ScrollToTop();
            }
        }

        private void CloseSettings(bool keepTheme)
        {
            if (!_settingsOpen)
            {
                return;
            }
            _settingsOpen = false;

            StopEggTimer();
            HideEggMenu(true);
            HideEggToast(true);

            if (!keepTheme)
            {
                _previewTheme = _settings.Theme;
                ApplyTheme(_settings.Theme, true);
            }

            _previewShimmer = _settings.Shimmer;
            _previewShimmerGradient = _settings.ShimmerGradient;
            _previewGlassDigits = _settings.GlassDigits;
            _previewSandDigits = _settings.SandDigits;
            ApplyDigitEffects();

            bool restoreCompact = _settingsExpanded && !_switchingMode;
            DoubleAnimation fade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150));
            fade.Completed += delegate
            {
                _settingsOverlay.Visibility = Visibility.Collapsed;
                _settingsOverlay.BeginAnimation(OpacityProperty, null);
                if (restoreCompact)
                {
                    RestoreCompactWindow();
                }
            };
            _settingsOverlay.BeginAnimation(OpacityProperty, fade);

            DoubleAnimation slide = new DoubleAnimation(0, 430, TimeSpan.FromMilliseconds(220));
            slide.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn };
            _settingsSlide.BeginAnimation(TranslateTransform.XProperty, slide);
        }

        private void CancelSettings()
        {
            if (!_settingsOpen)
            {
                return;
            }
            _previewTheme = _settings.Theme;
            ApplyTheme(_settings.Theme, true);
            CloseSettings(true);
        }

        private void OnCancelSettingsClick(object sender, RoutedEventArgs e)
        {
            CancelSettings();
        }

        private void OnSaveSettingsClick(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            if (!_settingsOpen)
            {
                return;
            }
            string name = _nameBox.Text == null ? "" : _nameBox.Text.Trim();
            if (name.Length == 0)
            {
                ShowValidation("请先写下倒计时的文本。");
                _nameBox.Focus();
                return;
            }

            DateTime target;
            if (!TryBuildTarget(_dateBox.Text, _timeBox.Text, out target))
            {
                ShowValidation("日期或时间格式不正确，试试 2027-01-01 和 00:00。");
                return;
            }
            if (target <= DateTime.Now)
            {
                ShowValidation("目标时间需要晚于现在，换一个未来的时刻吧。");
                return;
            }

            DateTime oldTarget = _settings.GetTarget();
            bool targetChanged = Math.Abs((target - oldTarget).TotalSeconds) > 1;
            if (targetChanged)
            {
                _settings.StartAt = DateTime.Now.ToString("o", CultureInfo.InvariantCulture);
            }

            string updateFeed = _updateFeedBox.Text == null ? "" : _updateFeedBox.Text.Trim();
            if (updateFeed.Length > 0 && !IsHttpUrl(updateFeed))
            {
                ShowValidation("更新源地址需要以 http:// 或 https:// 开头。");
                _updateFeedBox.Focus();
                return;
            }

            int starIdleMinutes;
            if (!int.TryParse(_starIdleBox.Text == null ? "" : _starIdleBox.Text.Trim(),
                    NumberStyles.Integer, CultureInfo.InvariantCulture, out starIdleMinutes) ||
                starIdleMinutes < 1 || starIdleMinutes > 120)
            {
                ShowValidation("星空待机时间需要是 1-120 之间的整数分钟。");
                _starIdleBox.Focus();
                return;
            }

            _settings.Name = name;
            _settings.Target = target.ToString("o", CultureInfo.InvariantCulture);
            bool autoStart = _autoStartToggle.IsChecked == true;
            bool oldAutoStart = _settings.AutoStart;
            if (!StartupRegistration.SetEnabled(autoStart))
            {
                _autoStartToggle.IsChecked = oldAutoStart;
                ShowValidation("开机自启动设置失败，请检查当前用户注册表权限后重试。");
                return;
            }

            bool glassDigits = _glassDigitsToggle.IsChecked == true;
            bool sandDigits = _sandToggle.IsChecked == true;
            if (glassDigits && sandDigits)
            {
                sandDigits = false;
            }

            _settings.Topmost = _topmostToggle.IsChecked == true;
            _settings.ShowSeconds = _secondsToggle.IsChecked == true;
            _settings.AutoStart = autoStart;
            _settings.Shimmer = _shimmerToggle.IsChecked == true;
            _settings.ShimmerGradient = _shimmerGradientToggle.IsChecked == true;
            _settings.GlassDigits = glassDigits;
            _settings.SandDigits = sandDigits;
            _settings.UpdateFeed = updateFeed;
            _settings.StarMode = _starModeToggle.IsChecked == true;
            _settings.StarIdleMinutes = starIdleMinutes;
            _settings.Theme = _previewTheme;
            string newDisplayMode = _previewDisplayMode;
            if (newDisplayMode != "glass" && newDisplayMode != "liquid" && newDisplayMode != "clear")
            {
                newDisplayMode = "glass";
            }
            _settings.DisplayMode = newDisplayMode;

            if (!_settings.Save())
            {
                _settings.AutoStart = oldAutoStart;
                StartupRegistration.SetEnabled(oldAutoStart);
                ShowValidation("设置保存失败，请检查文件权限后重试。");
                return;
            }

            _lastTarget = target;
            _lastSecondText = null;
            _previewShimmer = _settings.Shimmer;
            _previewShimmerGradient = _settings.ShimmerGradient;
            _previewGlassDigits = _settings.GlassDigits;
            _previewSandDigits = _settings.SandDigits;

            if (newDisplayMode != _displayMode)
            {
                SwitchDisplayMode(newDisplayMode);
                return;
            }

            ApplyTheme(_settings.Theme, true);
            ApplyModelToWindow();
            CloseSettings(true);
        }

        private void SwitchDisplayMode(string mode)
        {
            double keepLeft = _settingsExpanded ? _compactLeft : Left;
            double keepTop = _settingsExpanded ? _compactTop : Top;

            _settings.DisplayMode = mode;
            _settings.HasPosition = true;
            _settings.Left = keepLeft;
            _settings.Top = keepTop;
            _settings.Save();

            _switchingMode = true;
            MainWindow next = null;
            try
            {
                next = new MainWindow(false, _noAcrylic);
                Application.Current.MainWindow = next;
                next.Show();
                Close();
            }
            catch (Exception ex)
            {
                _switchingMode = false;
                Application.Current.MainWindow = this;
                _settings.DisplayMode = _displayMode;
                _settings.Left = keepLeft;
                _settings.Top = keepTop;
                _settings.Save();
                if (next != null)
                {
                    try
                    {
                        next.Close();
                    }
                    catch
                    {
                    }
                }
                ShowValidation("切换显示效果失败：" + ex.Message);
            }
        }

        private bool TryBuildTarget(string dateText, string timeText, out DateTime target)
        {
            target = DateTime.Now;
            if (string.IsNullOrWhiteSpace(dateText) || string.IsNullOrWhiteSpace(timeText))
            {
                return false;
            }

            DateTime date;
            string[] dateFormats = new string[]
            {
                "yyyy-M-d", "yyyy/M/d", "yyyy.M.d", "yyyyMMdd",
                "yyyy年M月d日", "yyyy年MM月dd日"
            };
            if (!DateTime.TryParseExact(dateText.Trim(), dateFormats, CultureInfo.CurrentCulture,
                    DateTimeStyles.None, out date))
            {
                if (!DateTime.TryParse(dateText.Trim(), CultureInfo.CurrentCulture,
                        DateTimeStyles.None, out date))
                {
                    return false;
                }
            }

            TimeSpan time;
            string[] timeFormats = new string[]
            {
                "H:mm", "HH:mm", "H:mm:ss", "HH:mm:ss", "H时mm分", "H时mm分ss秒"
            };
            if (!TimeSpan.TryParseExact(timeText.Trim(), timeFormats, CultureInfo.CurrentCulture, out time))
            {
                if (!TimeSpan.TryParse(timeText.Trim(), CultureInfo.CurrentCulture, out time))
                {
                    return false;
                }
            }
            if (time < TimeSpan.Zero || time >= TimeSpan.FromDays(1))
            {
                return false;
            }

            target = new DateTime(date.Year, date.Month, date.Day, time.Hours, time.Minutes,
                time.Seconds, DateTimeKind.Local);
            return true;
        }

        private void OnDraftTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_validationText != null && _validationText.Text.Length > 0)
            {
                _validationText.Text = "";
            }
        }

        private void OnEditorKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SaveSettings();
                e.Handled = true;
            }
        }

        private void ShowValidation(string message)
        {
            _validationText.Foreground = new SolidColorBrush(Color.FromRgb(255, 138, 155));
            _validationText.Text = message;
            _validationText.BringIntoView();
        }

        private void OnPresetClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null)
            {
                return;
            }
            string tag = button.Tag == null ? "" : button.Tag.ToString();
            DateTime now = DateTime.Now;
            DateTime value;
            if (tag == "hour")
            {
                value = now.AddHours(1);
            }
            else if (tag == "tomorrow")
            {
                value = now.AddDays(1);
            }
            else
            {
                value = new DateTime(now.Year + 1, 1, 1, 0, 0, 0, DateTimeKind.Local);
            }

            _dateBox.Text = value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            _timeBox.Text = value.ToString("HH:mm", CultureInfo.InvariantCulture);
            _validationText.Text = "";
        }

        private void OnResetSettingsClick(object sender, RoutedEventArgs e)
        {
            AppSettings defaults = AppSettings.CreateDefault();
            DateTime target = defaults.GetTarget();
            _nameBox.Text = defaults.Name;
            _dateBox.Text = target.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            _timeBox.Text = target.ToString("HH:mm", CultureInfo.InvariantCulture);
            _topmostToggle.IsChecked = defaults.Topmost;
            _secondsToggle.IsChecked = defaults.ShowSeconds;
            _autoStartToggle.IsChecked = defaults.AutoStart;
            _starModeToggle.IsChecked = defaults.StarMode;
            _starIdleBox.Text = defaults.StarIdleMinutes.ToString(CultureInfo.InvariantCulture);
            UpdateStarIdlePanelVisibility();
            _syncingDigitToggles = true;
            _shimmerToggle.IsChecked = defaults.Shimmer;
            _shimmerGradientToggle.IsChecked = defaults.ShimmerGradient;
            _glassDigitsToggle.IsChecked = defaults.GlassDigits;
            _sandToggle.IsChecked = defaults.SandDigits;
            _syncingDigitToggles = false;
            _previewTheme = defaults.Theme;
            _previewDisplayMode = defaults.DisplayMode;
            _previewShimmer = defaults.Shimmer;
            _previewShimmerGradient = defaults.ShimmerGradient;
            _previewGlassDigits = defaults.GlassDigits;
            _previewSandDigits = defaults.SandDigits;
            UpdateShimmerGradientVisibility();
            _updateFeedBox.Text = defaults.UpdateFeed;
            _updateStatusText.Text = "当前版本 v" + UpdateService.CurrentVersion.ToString(3);
            _updateStatusText.Foreground = new SolidColorBrush(Color.FromRgb(138, 153, 175));
            ApplyTheme(_previewTheme, true);
            UpdateThemeSelection(_previewTheme);
            UpdateEffectSelection(_previewDisplayMode);
            ApplyDigitEffects();
            _validationText.Foreground = new SolidColorBrush(Color.FromRgb(140, 153, 175));
            _validationText.Text = "已恢复默认值，点击保存后生效。";
        }

        private void OnSettingsOverlayMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_settingsPanel != null && !_settingsPanel.IsMouseOver)
            {
                CancelSettings();
                e.Handled = true;
            }
        }

        private void OnThemeClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null || button.Tag == null)
            {
                return;
            }
            _previewTheme = button.Tag.ToString();
            ApplyTheme(_previewTheme, true);
            UpdateThemeSelection(_previewTheme);
            _validationText.Text = "";
        }

        private void UpdateThemeSelection(string theme)
        {
            Button[] buttons = new Button[]
            {
                _themeAurora, _themeOcean, _themeSunset, _themeMint,
                _themeSakura, _themeEmber, _themeForest, _themeNebula
            };
            Color normalBorder = Color.FromArgb(24, 255, 255, 255);
            Color normalBackground = Color.FromArgb(11, 255, 255, 255);
            Brush accent = _root.Resources.Contains("AccentBrush")
                ? (Brush)_root.Resources["AccentBrush"]
                : Brushes.White;
            Color accentText = ((SolidColorBrush)_root.Resources["AccentColor"]).Color;

            for (int i = 0; i < buttons.Length; i++)
            {
                Button b = buttons[i];
                if (b == null)
                {
                    continue;
                }
                bool selected = b.Tag != null && b.Tag.ToString() == theme;
                b.BorderThickness = new Thickness(selected ? 1.5 : 1);
                b.BorderBrush = selected ? accent : new SolidColorBrush(normalBorder);
                b.Background = selected
                    ? new SolidColorBrush(Color.FromArgb(30, 255, 255, 255))
                    : new SolidColorBrush(normalBackground);
                StackPanel panel = b.Content as StackPanel;
                if (panel != null && panel.Children.Count > 1)
                {
                    TextBlock label = panel.Children[1] as TextBlock;
                    if (label != null)
                    {
                        label.Foreground = new SolidColorBrush(selected ? accentText : Color.FromRgb(185, 197, 216));
                    }
                }
            }
        }

        private void OnEffectClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null || button.Tag == null)
            {
                return;
            }
            _previewDisplayMode = button.Tag.ToString();
            UpdateEffectSelection(_previewDisplayMode);
            _validationText.Text = "";
        }

        private void UpdateEffectSelection(string mode)
        {
            Button[] buttons = new Button[] { _effectGlass, _effectLiquid, _effectClear };
            Color normalBorder = Color.FromArgb(24, 255, 255, 255);
            Color normalBackground = Color.FromArgb(11, 255, 255, 255);
            Brush accent = _root.Resources.Contains("AccentBrush")
                ? (Brush)_root.Resources["AccentBrush"]
                : Brushes.White;
            Color accentText = ((SolidColorBrush)_root.Resources["AccentColor"]).Color;

            for (int i = 0; i < buttons.Length; i++)
            {
                Button b = buttons[i];
                if (b == null)
                {
                    continue;
                }
                bool selected = b.Tag != null && b.Tag.ToString() == mode;
                b.BorderThickness = new Thickness(selected ? 1.5 : 1);
                b.BorderBrush = selected ? accent : new SolidColorBrush(normalBorder);
                b.Background = selected
                    ? new SolidColorBrush(Color.FromArgb(30, 255, 255, 255))
                    : new SolidColorBrush(normalBackground);
                StackPanel panel = b.Content as StackPanel;
                if (panel != null && panel.Children.Count > 1)
                {
                    TextBlock label = panel.Children[1] as TextBlock;
                    if (label != null)
                    {
                        label.Foreground = new SolidColorBrush(selected ? accentText : Color.FromRgb(185, 197, 216));
                    }
                }
            }
        }

        private void ApplyTheme(string theme, bool animate)
        {
            if (theme != "ocean" && theme != "sunset" && theme != "mint" &&
                theme != "sakura" && theme != "ember" && theme != "forest" && theme != "nebula")
            {
                theme = "aurora";
            }
            ThemePalette palette = GetPalette(theme);

            SetBlobBrush(_blobA, palette.A);
            SetBlobBrush(_blobB, palette.B);
            SetBlobBrush(_blobC, palette.C);
            SetBlobBrush(_blobD, palette.D);

            LinearGradientBrush accent = new LinearGradientBrush(palette.Accent1, palette.Accent2,
                new Point(0, 0), new Point(1, 1));
            _root.Resources["AccentBrush"] = accent;
            _root.Resources["AccentColor"] = new SolidColorBrush(Lighten(palette.Accent1, 0.42));
            _root.Resources["AccentSoft"] = new SolidColorBrush(WithAlpha(palette.Accent1, 48));

            if (animate && _blobCanvas != null)
            {
                DoubleAnimation fade = new DoubleAnimation(0.62, 1, TimeSpan.FromMilliseconds(420));
                fade.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
                _blobCanvas.BeginAnimation(OpacityProperty, fade);
            }
            if (_settingsOpen || _settings != null)
            {
                UpdateThemeSelection(_previewTheme);
                if (_effectGlass != null)
                {
                    UpdateEffectSelection(_previewDisplayMode);
                }
            }
            ApplyDigitEffects();
        }

        private static ThemePalette GetPalette(string theme)
        {
            ThemePalette p = new ThemePalette();
            if (theme == "ocean")
            {
                p.A = Color.FromRgb(47, 128, 255);
                p.B = Color.FromRgb(0, 229, 195);
                p.C = Color.FromRgb(110, 208, 255);
                p.D = Color.FromRgb(167, 139, 250);
                p.Accent1 = Color.FromRgb(47, 128, 255);
                p.Accent2 = Color.FromRgb(0, 229, 195);
            }
            else if (theme == "sunset")
            {
                p.A = Color.FromRgb(255, 122, 89);
                p.B = Color.FromRgb(255, 61, 129);
                p.C = Color.FromRgb(255, 196, 107);
                p.D = Color.FromRgb(168, 85, 247);
                p.Accent1 = Color.FromRgb(255, 122, 89);
                p.Accent2 = Color.FromRgb(255, 61, 129);
            }
            else if (theme == "mint")
            {
                p.A = Color.FromRgb(52, 211, 153);
                p.B = Color.FromRgb(34, 211, 238);
                p.C = Color.FromRgb(163, 230, 53);
                p.D = Color.FromRgb(110, 231, 183);
                p.Accent1 = Color.FromRgb(52, 211, 153);
                p.Accent2 = Color.FromRgb(34, 211, 238);
            }
            else if (theme == "sakura")
            {
                p.A = Color.FromRgb(255, 126, 182);
                p.B = Color.FromRgb(192, 132, 252);
                p.C = Color.FromRgb(255, 179, 209);
                p.D = Color.FromRgb(249, 168, 212);
                p.Accent1 = Color.FromRgb(255, 126, 182);
                p.Accent2 = Color.FromRgb(192, 132, 252);
            }
            else if (theme == "ember")
            {
                p.A = Color.FromRgb(255, 92, 56);
                p.B = Color.FromRgb(255, 176, 32);
                p.C = Color.FromRgb(255, 138, 61);
                p.D = Color.FromRgb(185, 28, 28);
                p.Accent1 = Color.FromRgb(255, 92, 56);
                p.Accent2 = Color.FromRgb(255, 176, 32);
            }
            else if (theme == "forest")
            {
                p.A = Color.FromRgb(22, 163, 74);
                p.B = Color.FromRgb(132, 204, 22);
                p.C = Color.FromRgb(52, 211, 153);
                p.D = Color.FromRgb(13, 148, 136);
                p.Accent1 = Color.FromRgb(22, 163, 74);
                p.Accent2 = Color.FromRgb(132, 204, 22);
            }
            else if (theme == "nebula")
            {
                p.A = Color.FromRgb(99, 102, 241);
                p.B = Color.FromRgb(236, 72, 153);
                p.C = Color.FromRgb(139, 92, 246);
                p.D = Color.FromRgb(34, 211, 238);
                p.Accent1 = Color.FromRgb(99, 102, 241);
                p.Accent2 = Color.FromRgb(236, 72, 153);
            }
            else
            {
                p.A = Color.FromRgb(124, 92, 255);
                p.B = Color.FromRgb(19, 217, 245);
                p.C = Color.FromRgb(255, 95, 162);
                p.D = Color.FromRgb(255, 184, 107);
                p.Accent1 = Color.FromRgb(139, 92, 246);
                p.Accent2 = Color.FromRgb(34, 211, 238);
            }
            return p;
        }

        private static void SetBlobBrush(Ellipse blob, Color color)
        {
            if (blob == null)
            {
                return;
            }
            RadialGradientBrush brush = new RadialGradientBrush();
            brush.GradientOrigin = new Point(0.42, 0.4);
            brush.Center = new Point(0.5, 0.5);
            brush.RadiusX = 0.5;
            brush.RadiusY = 0.5;
            brush.GradientStops.Add(new GradientStop(WithAlpha(color, 210), 0));
            brush.GradientStops.Add(new GradientStop(WithAlpha(color, 132), 0.36));
            brush.GradientStops.Add(new GradientStop(WithAlpha(color, 40), 0.68));
            brush.GradientStops.Add(new GradientStop(WithAlpha(color, 0), 1));
            blob.Fill = brush;
        }

        private static Color WithAlpha(Color color, byte alpha)
        {
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }

        private static Color Lighten(Color color, double amount)
        {
            int r = (int)Math.Min(255, color.R + (255 - color.R) * amount);
            int g = (int)Math.Min(255, color.G + (255 - color.G) * amount);
            int b = (int)Math.Min(255, color.B + (255 - color.B) * amount);
            return Color.FromRgb((byte)r, (byte)g, (byte)b);
        }

        private void OnDigitEffectToggleChanged(object sender, RoutedEventArgs e)
        {
            if (_syncingDigitToggles)
            {
                return;
            }

            bool glass = _glassDigitsToggle != null && _glassDigitsToggle.IsChecked == true;
            bool sand = _sandToggle != null && _sandToggle.IsChecked == true;
            if (glass && sand)
            {
                _syncingDigitToggles = true;
                if (sender == _glassDigitsToggle)
                {
                    _sandToggle.IsChecked = false;
                    sand = false;
                }
                else
                {
                    _glassDigitsToggle.IsChecked = false;
                    glass = false;
                }
                _syncingDigitToggles = false;
            }

            _previewShimmer = _shimmerToggle != null && _shimmerToggle.IsChecked == true;
            _previewShimmerGradient = _shimmerGradientToggle != null && _shimmerGradientToggle.IsChecked == true;
            _previewGlassDigits = glass;
            _previewSandDigits = sand;
            UpdateShimmerGradientVisibility();
            ApplyDigitEffects();
            if (_validationText != null)
            {
                _validationText.Text = "";
            }
        }

        private void UpdateShimmerGradientVisibility()
        {
            if (_shimmerGradientPanel == null)
            {
                return;
            }
            bool visible = _shimmerToggle != null && _shimmerToggle.IsChecked == true;
            _shimmerGradientPanel.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnDragAreaMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_settingsOpen || _eggMenuOpen)
            {
                return;
            }
            ShowCountdownMenu();
            e.Handled = true;
        }

        private void ShowCountdownMenu()
        {
            if (_countdownPopup == null)
            {
                return;
            }
            if (_menuTopmostButton != null)
            {
                _menuTopmostButton.Content = _settings.Topmost ? "取消桌面置顶" : "桌面置顶";
            }
            Point mouse = Mouse.GetPosition(_mainCard);
            Point screenPoint = _mainCard.PointToScreen(mouse);
            _countdownPopup.PlacementTarget = null;
            _countdownPopup.Placement = PlacementMode.Absolute;
            _countdownPopup.HorizontalOffset = screenPoint.X + 4;
            _countdownPopup.VerticalOffset = screenPoint.Y + 4;
            _countdownPopup.IsOpen = true;
        }

        private void OnStarModeToggleChanged(object sender, RoutedEventArgs e)
        {
            UpdateStarIdlePanelVisibility();
            if (_validationText != null)
            {
                _validationText.Text = "";
            }
        }

        private void UpdateStarIdlePanelVisibility()
        {
            if (_starIdlePanel == null)
            {
                return;
            }
            _starIdlePanel.Visibility = _starModeToggle != null && _starModeToggle.IsChecked == true
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void OnStarPreviewClick(object sender, RoutedEventArgs e)
        {
            EnterStarMode();
        }

        private void OnStarIdleTick(object sender, EventArgs e)
        {
            if (!_settings.StarMode || _settingsOpen || _eggMenuOpen || _closing)
            {
                return;
            }
            if (_starWindow != null || !IsVisible || WindowState == WindowState.Minimized)
            {
                return;
            }
            int minutes = Math.Max(1, _settings.StarIdleMinutes);
            if (NativeMethods.GetSystemIdleTime().TotalMinutes >= minutes)
            {
                EnterStarMode();
            }
        }

        private void EnterStarMode()
        {
            if (_starWindow != null || _closing)
            {
                return;
            }

            ThemePalette palette = GetPalette(_settings.Theme);
            Point center = PointToScreen(new Point(Width / 2.0, Height / 2.0));
            StarWindow star = new StarWindow(_settings, this, center,
                palette.Accent1, palette.Accent2, palette.A, palette.B, palette.C, palette.D);
            _starWindow = star;
            star.Closed += delegate
            {
                _starWindow = null;
                if (_closing)
                {
                    return;
                }
                try
                {
                    Show();
                    Activate();
                    ApplyTopmost(_settings.Topmost);
                }
                catch
                {
                }
            };
            star.Show();
            star.Activate();
        }

        private void OnUpdateCheckClick(object sender, RoutedEventArgs e)
        {
            CheckForUpdates();
        }

        private void CheckForUpdates()
        {
            if (_checkingUpdate)
            {
                return;
            }

            string feed = _settings.UpdateFeed;
            if (_settingsOpen && _updateFeedBox != null)
            {
                feed = _updateFeedBox.Text == null ? "" : _updateFeedBox.Text.Trim();
                if (feed.Length > 0 && !IsHttpUrl(feed))
                {
                    ShowValidation("更新源地址需要以 http:// 或 https:// 开头。");
                    _updateFeedBox.Focus();
                    return;
                }
            }

            _checkingUpdate = true;
            if (_updateCheckButton != null)
            {
                _updateCheckButton.IsEnabled = false;
            }
            SetUpdateStatus("正在检查更新…", GetAccentColor());
            if (_validationText != null)
            {
                _validationText.Text = "";
            }

            string feedCopy = feed;
            ThreadPool.QueueUserWorkItem(delegate
            {
                UpdateInfo info = null;
                string error = null;
                try
                {
                    info = UpdateService.Check(feedCopy);
                }
                catch (Exception ex)
                {
                    error = ex.GetBaseException().Message;
                }
                try
                {
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        FinishUpdateCheck(info, error);
                    }));
                }
                catch
                {
                }
            });
        }

        private void FinishUpdateCheck(UpdateInfo info, string error)
        {
            _checkingUpdate = false;
            if (_closing)
            {
                return;
            }
            if (_updateCheckButton != null)
            {
                _updateCheckButton.IsEnabled = true;
            }

            Version current = UpdateService.CurrentVersion;
            if (info == null)
            {
                SetUpdateStatus("检查更新失败", Color.FromRgb(255, 138, 155));
                string feed = _settings.UpdateFeed;
                string source = string.IsNullOrWhiteSpace(feed)
                    ? "默认会尝试 GitHub、jsDelivr、国内镜像和 Gitee。"
                    : "更新源：" + feed;
                string message = "无法连接更新服务器。\n\n" + (error ?? "") +
                    "\n\n" + source +
                    "\n\n国内网络访问 GitHub 可能不稳定，可以稍后重试，或在设置里填写可访问的更新源。" +
                    "\n\n是否打开作者主页？";
                if (MessageBox.Show(message, "检查更新", MessageBoxButton.YesNo,
                        MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    OpenExternalUrl("https://github.com/wyft2");
                }
                return;
            }

            Version latest = UpdateService.ParseVersion(info.Version);
            if (latest == null)
            {
                SetUpdateStatus("更新源版本号无效", Color.FromRgb(255, 138, 155));
                MessageBox.Show("更新源返回的版本号无法识别。", "检查更新",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (latest > current)
            {
                SetUpdateStatus("发现新版本 v" + latest.ToString(3), GetAccentColor());
                string notes = string.IsNullOrWhiteSpace(info.Notes) ? "" : "\n\n" + info.Notes;
                string notice = string.IsNullOrWhiteSpace(info.Notice) ? "" : "\n\n" + info.Notice;
                string target = !string.IsNullOrWhiteSpace(info.Url)
                    ? info.Url
                    : (!string.IsNullOrWhiteSpace(info.Page)
                        ? info.Page
                        : "https://github.com/wyft2/SeptemberCountdown/releases/latest");
                string message = "发现新版本 v" + latest.ToString(3) +
                    "（当前 v" + current.ToString(3) + "）" + notes + notice +
                    "\n\n是否打开下载页？";
                if (MessageBox.Show(message, "检查更新", MessageBoxButton.YesNo,
                        MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    OpenExternalUrl(target);
                }
                return;
            }

            if (latest == current)
            {
                SetUpdateStatus("已是最新版本 v" + current.ToString(3) +
                    (info.FromLocal ? "（离线）" : ""), Color.FromRgb(74, 222, 128));
                string extra = info.FromLocal
                    ? info.Notice + "\n\n当前版本：v" + current.ToString(3) +
                      "\n更新源：程序目录的 version.json" +
                      "\n\n如果要在线检查，请在设置里填写可访问的 version.json 地址。"
                    : "当前版本：v" + current.ToString(3) + "。";
                MessageBox.Show(extra, "检查更新", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SetUpdateStatus("当前版本 v" + current.ToString(3) + " 比更新源还新",
                Color.FromRgb(74, 222, 128));
            MessageBox.Show("当前版本 v" + current.ToString(3) +
                " 比更新源里的 v" + latest.ToString(3) + " 还要新。",
                "检查更新", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SetUpdateStatus(string text, Color color)
        {
            if (_updateStatusText == null)
            {
                return;
            }
            _updateStatusText.Text = text;
            _updateStatusText.Foreground = new SolidColorBrush(color);
        }

        private Color GetAccentColor()
        {
            SolidColorBrush brush = _root.Resources.Contains("AccentColor")
                ? _root.Resources["AccentColor"] as SolidColorBrush
                : null;
            return brush != null ? brush.Color : Color.FromRgb(167, 139, 250);
        }

        private static bool IsHttpUrl(string value)
        {
            return value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                   value.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }

        private void OnVersionButtonClick(object sender, RoutedEventArgs e)
        {
            DateTime now = DateTime.Now;
            if ((now - _lastVersionClick).TotalMilliseconds > 1300)
            {
                _versionClickCount = 0;
            }
            _lastVersionClick = now;
            _versionClickCount++;

            if (_versionClickCount >= 7)
            {
                _versionClickCount = 0;
                EnterEasterEgg();
            }
        }

        private void EnterEasterEgg()
        {
            HideEggMenu(true);
            ShowEggToast();
            StopEggTimer();
            _eggTimer = new DispatcherTimer();
            _eggTimer.Interval = TimeSpan.FromMilliseconds(1250);
            _eggTimer.Tick += delegate
            {
                StopEggTimer();
                HideEggToast(false);
                ShowEggMenu();
            };
            _eggTimer.Start();
        }

        private void StopEggTimer()
        {
            if (_eggTimer != null)
            {
                _eggTimer.Stop();
                _eggTimer = null;
            }
        }

        private void ShowEggToast()
        {
            if (_eggToast == null)
            {
                return;
            }
            _eggToast.Visibility = Visibility.Visible;
            DoubleAnimation fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            fade.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            _eggToast.BeginAnimation(UIElement.OpacityProperty, fade);
        }

        private void HideEggToast(bool immediate)
        {
            if (_eggToast == null)
            {
                return;
            }
            if (immediate)
            {
                _eggToast.BeginAnimation(UIElement.OpacityProperty, null);
                _eggToast.Visibility = Visibility.Collapsed;
                return;
            }
            DoubleAnimation fade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(220));
            fade.Completed += delegate
            {
                _eggToast.Visibility = Visibility.Collapsed;
                _eggToast.BeginAnimation(UIElement.OpacityProperty, null);
            };
            _eggToast.BeginAnimation(UIElement.OpacityProperty, fade);
        }

        private void ShowEggMenu()
        {
            if (_eggOverlay == null)
            {
                return;
            }
            _eggMenuOpen = true;
            _eggOverlay.Visibility = Visibility.Visible;
            _eggOverlay.Opacity = 0;
            DoubleAnimation fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220));
            _eggOverlay.BeginAnimation(UIElement.OpacityProperty, fade);

            if (_eggPanel != null)
            {
                _eggPanel.RenderTransformOrigin = new Point(0.5, 0.5);
                ScaleTransform scale = new ScaleTransform(0.94, 0.94);
                _eggPanel.RenderTransform = scale;
                DoubleAnimation pop = new DoubleAnimation(0.94, 1, TimeSpan.FromMilliseconds(320));
                pop.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, pop);
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, pop);
            }
        }

        private void HideEggMenu()
        {
            HideEggMenu(false);
        }

        private void HideEggMenu(bool immediate)
        {
            if (_eggOverlay == null)
            {
                return;
            }
            _eggMenuOpen = false;
            if (immediate)
            {
                _eggOverlay.BeginAnimation(UIElement.OpacityProperty, null);
                _eggOverlay.Visibility = Visibility.Collapsed;
                return;
            }
            DoubleAnimation fade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(170));
            fade.Completed += delegate
            {
                _eggOverlay.Visibility = Visibility.Collapsed;
                _eggOverlay.BeginAnimation(UIElement.OpacityProperty, null);
            };
            _eggOverlay.BeginAnimation(UIElement.OpacityProperty, fade);
        }

        private void OnEggCloseButtonClick(object sender, RoutedEventArgs e)
        {
            HideEggMenu();
        }

        private void OnEggOverlayMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_eggPanel != null && !_eggPanel.IsMouseOver)
            {
                HideEggMenu();
                e.Handled = true;
            }
        }

        private void OnEggAuthorButtonClick(object sender, RoutedEventArgs e)
        {
            OpenExternalUrl("https://github.com/wyft2");
        }

        private void OnEggChallengeButtonClick(object sender, RoutedEventArgs e)
        {
            OpenExternalUrl("https://wyft2.github.io/Echo-Layer-Page-7/");
        }

        private void OpenExternalUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("链接打开失败：" + ex.Message, "九月倒计时",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void HideToTray()
        {
            if (_tray == null)
            {
                WindowState = WindowState.Minimized;
                return;
            }

            Hide();
            if (!_shownTrayTip && _tray != null)
            {
                _shownTrayTip = true;
                try
                {
                    _tray.ShowBalloonTip(2600, "九月倒计时",
                        "已经收起到系统托盘，双击图标即可召回。", Forms.ToolTipIcon.None);
                }
                catch
                {
                }
            }
        }

        private void CreateTray()
        {
            try
            {
                _tray = new Forms.NotifyIcon();
                string exe = Assembly.GetExecutingAssembly().Location;
                _trayIcon = System.Drawing.Icon.ExtractAssociatedIcon(exe);
                if (_trayIcon == null)
                {
                    using (Stream iconStream = GetResourceStream("SeptemberCountdown.Assets.app.ico"))
                    {
                        if (iconStream != null)
                        {
                            _trayIcon = new System.Drawing.Icon(iconStream);
                        }
                    }
                }
                if (_trayIcon != null)
                {
                    _tray.Icon = _trayIcon;
                }
                _tray.Text = "九月倒计时";
                _tray.Visible = true;

                Forms.ContextMenuStrip menu = new Forms.ContextMenuStrip();
                menu.ShowImageMargin = false;
                menu.BackColor = System.Drawing.Color.FromArgb(15, 19, 29);
                menu.ForeColor = System.Drawing.Color.FromArgb(232, 238, 248);
                menu.Font = new System.Drawing.Font("Microsoft YaHei UI", 9f);
                menu.Renderer = new Forms.ToolStripProfessionalRenderer();

                Forms.ToolStripMenuItem showItem = new Forms.ToolStripMenuItem("显示 / 隐藏");
                showItem.Click += delegate { Dispatcher.BeginInvoke(new Action(ToggleWindowVisibility)); };

                Forms.ToolStripMenuItem settingsItem = new Forms.ToolStripMenuItem("设置…");
                settingsItem.Click += delegate
                {
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        ShowFromTray();
                        OpenSettings();
                    }));
                };

                Forms.ToolStripMenuItem updateItem = new Forms.ToolStripMenuItem("检查更新…");
                updateItem.Click += delegate
                {
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        ShowFromTray();
                        CheckForUpdates();
                    }));
                };

                _trayTopmostItem = new Forms.ToolStripMenuItem("桌面置顶");
                _trayTopmostItem.CheckOnClick = false;
                _trayTopmostItem.Click += delegate
                {
                    Dispatcher.BeginInvoke(new Action(ToggleTopmostFromTray));
                };

                Forms.ToolStripMenuItem exitItem = new Forms.ToolStripMenuItem("退出");
                exitItem.Click += delegate { Dispatcher.BeginInvoke(new Action(Close)); };

                menu.Items.Add(showItem);
                menu.Items.Add(settingsItem);
                menu.Items.Add(updateItem);
                menu.Items.Add(new Forms.ToolStripSeparator());
                menu.Items.Add(_trayTopmostItem);
                menu.Items.Add(new Forms.ToolStripSeparator());
                menu.Items.Add(exitItem);
                menu.Opening += delegate { _trayTopmostItem.Checked = _settings.Topmost; };

                _tray.ContextMenuStrip = menu;
                _tray.DoubleClick += delegate
                {
                    Dispatcher.BeginInvoke(new Action(ShowFromTray));
                };
            }
            catch
            {
                if (_tray != null)
                {
                    _tray.Dispose();
                    _tray = null;
                }
            }
        }

        private void ToggleWindowVisibility()
        {
            if (IsVisible)
            {
                HideToTray();
            }
            else
            {
                ShowFromTray();
            }
        }

        private void ShowFromTray()
        {
            Show();
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
            Activate();
            ApplyTopmost(_settings.Topmost);
        }

        private void ToggleTopmostFromTray()
        {
            _settings.Topmost = !_settings.Topmost;
            ApplyTopmost(_settings.Topmost);
            _syncing = true;
            _pinButton.IsChecked = _settings.Topmost;
            _syncing = false;
            UpdatePinVisual();
            _settings.Save();
            if (_trayTopmostItem != null)
            {
                _trayTopmostItem.Checked = _settings.Topmost;
            }
        }

        private static Stream GetResourceStream(string name)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
        }

        private static string ReadTextResource(string name)
        {
            using (Stream stream = GetResourceStream(name))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException("界面资源缺失：" + name);
                }
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}

