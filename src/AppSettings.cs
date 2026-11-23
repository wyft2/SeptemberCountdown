using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace SeptemberCountdown
{
    public class AppSettings
    {
        public string Name { get; set; }
        public string Target { get; set; }
        public string StartAt { get; set; }
        public bool Topmost { get; set; }
        public bool ShowSeconds { get; set; }
        public bool AutoStart { get; set; }
        public string Theme { get; set; }
        public string DisplayMode { get; set; }
        public bool Shimmer { get; set; }
        public bool ShimmerGradient { get; set; }
        public bool GlassDigits { get; set; }
        public bool SandDigits { get; set; }
        public string UpdateFeed { get; set; }
        public bool StarMode { get; set; }
        public int StarIdleMinutes { get; set; }
        public bool HasPosition { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public int Version { get; set; }

        public AppSettings()
        {
            Name = "新年倒计时";
            DateTime now = DateTime.Now;
            DateTime target = new DateTime(now.Year + 1, 1, 1, 0, 0, 0, DateTimeKind.Local);
            Target = target.ToString("o", CultureInfo.InvariantCulture);
            StartAt = now.ToString("o", CultureInfo.InvariantCulture);
            Topmost = true;
            ShowSeconds = true;
            AutoStart = false;
            Theme = "aurora";
            DisplayMode = "glass";
            Shimmer = false;
            ShimmerGradient = false;
            GlassDigits = false;
            SandDigits = false;
            UpdateFeed = "";
            StarMode = false;
            StarIdleMinutes = 1;
            HasPosition = false;
            Left = 0;
            Top = 0;
            Version = 7;
        }

        public static string SettingsDirectory
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SeptemberCountdown");
            }
        }

        public static string SettingsPath
        {
            get { return Path.Combine(SettingsDirectory, "settings.json"); }
        }

        public static AppSettings CreateDefault()
        {
            return new AppSettings();
        }

        public DateTime GetTarget()
        {
            return ParseDate(Target, new DateTime(DateTime.Now.Year + 1, 1, 1, 0, 0, 0, DateTimeKind.Local));
        }

        public DateTime GetStartAt()
        {
            return ParseDate(StartAt, DateTime.Now);
        }

        private static DateTime ParseDate(string value, DateTime fallback)
        {
            DateTime parsed;
            if (!string.IsNullOrEmpty(value) &&
                DateTime.TryParse(value, CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind, out parsed))
            {
                if (parsed.Kind == DateTimeKind.Utc)
                {
                    parsed = parsed.ToLocalTime();
                }
                return parsed;
            }
            return fallback;
        }

        public AppSettings Clone()
        {
            AppSettings copy = new AppSettings();
            copy.Name = Name;
            copy.Target = Target;
            copy.StartAt = StartAt;
            copy.Topmost = Topmost;
            copy.ShowSeconds = ShowSeconds;
            copy.AutoStart = AutoStart;
            copy.Theme = Theme;
            copy.DisplayMode = DisplayMode;
            copy.Shimmer = Shimmer;
            copy.ShimmerGradient = ShimmerGradient;
            copy.GlassDigits = GlassDigits;
            copy.SandDigits = SandDigits;
            copy.UpdateFeed = UpdateFeed;
            copy.StarMode = StarMode;
            copy.StarIdleMinutes = StarIdleMinutes;
            copy.HasPosition = HasPosition;
            copy.Left = Left;
            copy.Top = Top;
            copy.Version = Version;
            return copy;
        }

        public static AppSettings Load()
        {
            try
            {
                string path = SettingsPath;
                if (!File.Exists(path))
                {
                    return CreateDefault();
                }

                string json = File.ReadAllText(path, Encoding.UTF8);
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                AppSettings loaded = serializer.Deserialize<AppSettings>(json);
                if (loaded == null)
                {
                    return CreateDefault();
                }

                AppSettings fallback = CreateDefault();
                if (string.IsNullOrWhiteSpace(loaded.Name))
                {
                    loaded.Name = fallback.Name;
                }
                loaded.Name = loaded.Name.Trim();
                if (loaded.Name.Length > 40)
                {
                    loaded.Name = loaded.Name.Substring(0, 40);
                }
                if (string.IsNullOrEmpty(loaded.Target))
                {
                    loaded.Target = fallback.Target;
                }
                if (string.IsNullOrEmpty(loaded.StartAt))
                {
                    loaded.StartAt = fallback.StartAt;
                }
                if (string.IsNullOrEmpty(loaded.Theme) ||
                    (loaded.Theme != "aurora" && loaded.Theme != "ocean" &&
                     loaded.Theme != "sunset" && loaded.Theme != "mint" &&
                     loaded.Theme != "sakura" && loaded.Theme != "ember" &&
                     loaded.Theme != "forest" && loaded.Theme != "nebula"))
                {
                    loaded.Theme = fallback.Theme;
                }
                if (string.IsNullOrEmpty(loaded.DisplayMode) ||
                    (loaded.DisplayMode != "glass" && loaded.DisplayMode != "liquid" &&
                     loaded.DisplayMode != "clear"))
                {
                    loaded.DisplayMode = fallback.DisplayMode;
                }
                if (loaded.GlassDigits && loaded.SandDigits)
                {
                    loaded.SandDigits = false;
                }
                if (loaded.StarIdleMinutes < 1 || loaded.StarIdleMinutes > 120)
                {
                    loaded.StarIdleMinutes = 1;
                }
                if (loaded.UpdateFeed == null)
                {
                    loaded.UpdateFeed = "";
                }
                loaded.UpdateFeed = loaded.UpdateFeed.Trim();
                loaded.Version = 7;

                DateTime target = loaded.GetTarget();
                DateTime start = loaded.GetStartAt();
                if (start >= target)
                {
                    start = target.AddDays(-30);
                    loaded.StartAt = start.ToString("o", CultureInfo.InvariantCulture);
                }
                return loaded;
            }
            catch
            {
                return CreateDefault();
            }
        }

        public bool Save()
        {
            try
            {
                Directory.CreateDirectory(SettingsDirectory);
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string json = serializer.Serialize(this);
                string path = SettingsPath;
                string temp = path + ".tmp";
                File.WriteAllText(temp, json, new UTF8Encoding(false));
                if (File.Exists(path))
                {
                    try
                    {
                        File.Replace(temp, path, null);
                    }
                    catch
                    {
                        File.Copy(temp, path, true);
                        File.Delete(temp);
                    }
                }
                else
                {
                    File.Move(temp, path);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void DeleteSaved()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    File.Delete(SettingsPath);
                }
            }
            catch
            {
            }
        }
    }
}
