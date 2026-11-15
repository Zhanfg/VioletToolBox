using System;
using System.IO;
using System.Globalization;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using Color = System.Windows.Media.Color;
using Application = System.Windows.Application;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;

namespace WpfApp1.Theming;

public enum AccentMode
{
    Violet = 0,
    Windows = 1
}

public enum ThemeMode
{
    System = 0,
    Light = 1,
    Dark = 2
}

/// <summary>
/// Owns the application accent and light/dark palette. The default accent remains Violet.
/// Windows mode reads the selected accent directly from the current user's Windows theme registry.
/// </summary>
public static class ThemeManager
{
    private const int WmSettingChange = 0x001A;
    private const int WmThemeChanged = 0x031A;
    private const int WmDwmColorizationColorChanged = 0x0320;

    private const string PersonalizeKey =
        @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private const string DwmKey = @"Software\Microsoft\Windows\DWM";
    private const string ExplorerAccentKey =
        @"Software\Microsoft\Windows\CurrentVersion\Explorer\Accent";

    private static readonly string PreferencesPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "VioletToolBox",
        "ui-preferences.json");

    private static readonly Color VioletDefault = Color.FromRgb(0x76, 0x56, 0xD6);

    private static Window? _window;
    private static HwndSource? _source;
    private static ResourceDictionary? _palette;
    private static Preferences _preferences = new();

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(ThemeManager),
            new PropertyMetadata(false, OnEnableChanged));

    public static void SetEnable(DependencyObject element, bool value) =>
        element.SetValue(EnableProperty, value);

    public static bool GetEnable(DependencyObject element) =>
        (bool)element.GetValue(EnableProperty);

    public static AccentMode CurrentAccentMode => _preferences.AccentMode;
    public static ThemeMode CurrentThemeMode => _preferences.ThemeMode;

    public static void SetAccentMode(AccentMode mode)
    {
        _preferences.AccentMode = mode;
        SavePreferences();
        QueueRefresh();
    }

    public static void SetThemeMode(ThemeMode mode)
    {
        _preferences.ThemeMode = mode;
        SavePreferences();
        QueueRefresh();
    }

    private static void OnEnableChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs args)
    {
        if (args.NewValue is true && dependencyObject is Window window)
        {
            Initialize(window);
        }
    }

    private static void Initialize(Window window)
    {
        if (ReferenceEquals(_window, window))
        {
            return;
        }

        Detach();
        _window = window;
        LoadPreferences();

        if (Application.Current is not null)
        {
            _palette = new ResourceDictionary();
            Application.Current.Resources.MergedDictionaries.Add(_palette);
            RefreshTheme();
        }

        window.SourceInitialized += OnSourceInitialized;
        window.Closed += OnWindowClosed;
        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
    }

    private static void OnSourceInitialized(object? sender, EventArgs args)
    {
        if (sender is not Window window)
        {
            return;
        }

        _source = HwndSource.FromHwnd(new WindowInteropHelper(window).Handle);
        _source?.AddHook(WindowProc);
    }

    private static void OnWindowClosed(object? sender, EventArgs args) => Detach();

    private static void Detach()
    {
        SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;

        if (_source is not null)
        {
            _source.RemoveHook(WindowProc);
            _source = null;
        }

        if (_window is not null)
        {
            _window.SourceInitialized -= OnSourceInitialized;
            _window.Closed -= OnWindowClosed;
            _window = null;
        }
    }

    private static void OnUserPreferenceChanged(
        object? sender,
        UserPreferenceChangedEventArgs args)
    {
        if (args.Category is UserPreferenceCategory.Color
            or UserPreferenceCategory.General)
        {
            QueueRefresh();
        }
    }

    private static IntPtr WindowProc(
        IntPtr hwnd,
        int message,
        IntPtr wParam,
        IntPtr lParam,
        ref bool handled)
    {
        if (message is WmSettingChange
            or WmThemeChanged
            or WmDwmColorizationColorChanged)
        {
            QueueRefresh();
        }

        handled = false;
        return IntPtr.Zero;
    }

    private static void QueueRefresh()
    {
        Dispatcher? dispatcher = _window?.Dispatcher;
        if (dispatcher is null || dispatcher.HasShutdownStarted)
        {
            return;
        }

        if (dispatcher.CheckAccess())
        {
            RefreshTheme();
        }
        else
        {
            _ = dispatcher.InvokeAsync(RefreshTheme, DispatcherPriority.Background);
        }
    }

    private static void RefreshTheme()
    {
        if (_palette is null)
        {
            return;
        }

        bool isDark = _preferences.ThemeMode switch
        {
            ThemeMode.Dark => true,
            ThemeMode.Light => false,
            _ => !ReadAppsUseLightTheme()
        };

        Color accent = _preferences.AccentMode == AccentMode.Windows
            ? ReadWindowsAccentColor() ?? VioletDefault
            : VioletDefault;

        if (isDark && GetLuminance(accent) < 0.42)
        {
            accent = Blend(accent, Colors.White, 0.24);
        }

        Color background = isDark
            ? Color.FromRgb(0x11, 0x14, 0x1A)
            : Color.FromRgb(0xF4, 0xF6, 0xFA);
        Color surface = isDark
            ? Color.FromRgb(0x1A, 0x1F, 0x28)
            : Colors.White;
        Color surfaceAlt = isDark
            ? Color.FromRgb(0x20, 0x26, 0x31)
            : Color.FromRgb(0xF8, 0xF9, 0xFC);
        Color primaryText = isDark
            ? Color.FromRgb(0xF1, 0xF3, 0xF7)
            : Color.FromRgb(0x20, 0x27, 0x33);
        Color secondaryText = isDark
            ? Color.FromRgb(0xA4, 0xAD, 0xBC)
            : Color.FromRgb(0x6B, 0x75, 0x85);
        Color border = isDark
            ? Color.FromRgb(0x34, 0x3C, 0x49)
            : Color.FromRgb(0xE3, 0xE7, 0xEE);
        Color accentHover = Blend(accent, isDark ? Colors.White : Colors.Black, 0.10);
        Color accentSubtle = Blend(isDark ? surfaceAlt : Colors.White, accent, isDark ? 0.24 : 0.10);
        Color accentSurface = Blend(background, accent, isDark ? 0.16 : 0.055);
        Color accentText = GetLuminance(accent) > 0.58
            ? Color.FromRgb(0x18, 0x1B, 0x22)
            : Colors.White;

        _palette.Clear();
        SetBrush("AppWindowBackgroundBrush", background);
        SetBrush("AppSurfaceBrush", surface);
        SetBrush("AppSurfaceAltBrush", surfaceAlt);
        SetBrush("AppSidebarBackgroundBrush", surfaceAlt);
        SetBrush("AppTitleBarBackgroundBrush", surface);
        SetBrush("AppTitleBarHoverBrush", accentSubtle);
        SetBrush("AppNavSelectedBackgroundBrush", accentSubtle);
        SetBrush("AppNavSelectedForegroundBrush", accent);
        SetBrush("AppStatusBarBackgroundBrush", surfaceAlt);
        SetBrush("AppTextBrush", primaryText);
        SetBrush("AppMutedTextBrush", secondaryText);
        SetBrush("AppBorderBrush", border);
        SetBrush("AppAccentBrush", accent);
        SetBrush("AppAccentHoverBrush", accentHover);
        SetBrush("AppAccentSubtleBrush", accentSubtle);
        SetBrush("AppAccentSurfaceBrush", accentSurface);
        SetBrush("AppAccentTextBrush", accentText);
        SetBrush("AppSuccessBrush", Color.FromRgb(0x16, 0xA3, 0x4A));
        SetBrush("AppWarningBrush", Color.FromRgb(0xD9, 0x77, 0x06));
        SetBrush("AppDangerBrush", Color.FromRgb(0xDC, 0x26, 0x26));
        SetBrush("AppInputBackgroundBrush", isDark ? surfaceAlt : Colors.White);
        // Page-level palette shared by flashing, EDL, screen mirroring and recovery tools.
        // Keep semantic danger/warning colors distinct from the selectable accent.
        SetBrush("AppHoverSurfaceBrush", Blend(surfaceAlt, accent, isDark ? 0.12 : 0.055));
        SetBrush("AppSelectedRowBrush", accentSubtle);
        SetBrush("AppAccentBorderBrush", Blend(border, accent, isDark ? 0.40 : 0.26));
        SetBrush("AppDisabledSurfaceBrush", isDark ? Color.FromRgb(0x30, 0x36, 0x40) : Color.FromRgb(0xE9, 0xEC, 0xF1));
        SetBrush("AppDisabledTextBrush", isDark ? Color.FromRgb(0x87, 0x90, 0x9F) : Color.FromRgb(0x94, 0x9D, 0xAC));
        SetBrush("AppDangerSurfaceBrush", Blend(surface, Color.FromRgb(0xDC, 0x26, 0x26), isDark ? 0.18 : 0.08));
        SetBrush("AppOverlayBrush", isDark ? Color.FromArgb(0xDA, 0x11, 0x14, 0x1A) : Color.FromArgb(0xDD, 0xFF, 0xFF, 0xFF));
        _palette["AppAccentSoftColor"] = Blend(accent, isDark ? Colors.Black : Colors.White, isDark ? 0.42 : 0.66);
        _palette["AppAccentColor"] = accent;
        _palette["AppIsDark"] = isDark;
    }

    private static void SetBrush(string key, Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        _palette![key] = brush;
    }

    private static Color? ReadWindowsAccentColor()
    {
        // Windows stores the DWM AccentColor DWORD as AABBGGRR.
        // AccentColorMenu is used as a fallback on builds/profiles where AccentColor is absent.
        if (TryReadAccentColor(DwmKey, "AccentColor", out Color accent))
        {
            return accent;
        }

        if (TryReadAccentColor(ExplorerAccentKey, "AccentColorMenu", out accent))
        {
            return accent;
        }

        if (TryReadAccentColor(ExplorerAccentKey, "StartColorMenu", out accent))
        {
            return accent;
        }

        return null;
    }

    private static bool TryReadAccentColor(string keyPath, string valueName, out Color color)
    {
        color = default;

        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(keyPath, writable: false);
            object? value = key?.GetValue(valueName);

            uint raw;
            switch (value)
            {
                case int signedValue:
                    raw = unchecked((uint)signedValue);
                    break;
                case long longValue:
                    raw = unchecked((uint)longValue);
                    break;
                case uint unsignedValue:
                    raw = unsignedValue;
                    break;
                case byte[] bytes when bytes.Length >= sizeof(uint):
                    raw = BitConverter.ToUInt32(bytes, 0);
                    break;
                default:
                    return false;
            }

            color = Color.FromRgb(
                (byte)(raw & 0xFF),
                (byte)((raw >> 8) & 0xFF),
                (byte)((raw >> 16) & 0xFF));
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (System.Security.SecurityException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }

    private static bool ReadAppsUseLightTheme()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(PersonalizeKey, writable: false);
            object? value = key?.GetValue("AppsUseLightTheme", 1);
            return Convert.ToInt32(value) != 0;
        }
        catch
        {
            return true;
        }
    }

    private static Color Blend(Color from, Color to, double amount)
    {
        amount = Math.Clamp(amount, 0, 1);
        return Color.FromRgb(
            (byte)Math.Round(from.R + ((to.R - from.R) * amount)),
            (byte)Math.Round(from.G + ((to.G - from.G) * amount)),
            (byte)Math.Round(from.B + ((to.B - from.B) * amount)));
    }

    private static double GetLuminance(Color color)
    {
        static double Linear(byte channel)
        {
            double value = channel / 255d;
            return value <= 0.04045
                ? value / 12.92
                : Math.Pow((value + 0.055) / 1.055, 2.4);
        }

        return (0.2126 * Linear(color.R))
            + (0.7152 * Linear(color.G))
            + (0.0722 * Linear(color.B));
    }

    private static void LoadPreferences()
    {
        try
        {
            if (!File.Exists(PreferencesPath))
            {
                _preferences = new Preferences();
                return;
            }

            _preferences = JsonSerializer.Deserialize<Preferences>(
                File.ReadAllText(PreferencesPath)) ?? new Preferences();
        }
        catch
        {
            _preferences = new Preferences();
        }
    }

    private static void SavePreferences()
    {
        try
        {
            string? directory = Path.GetDirectoryName(PreferencesPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(
                PreferencesPath,
                JsonSerializer.Serialize(_preferences, new JsonSerializerOptions
                {
                    WriteIndented = true
                }));
        }
        catch
        {
            // Theme preferences are optional; a read-only profile must not block the UI.
        }
    }

    public sealed class Preferences
    {
        public Preferences() { }

        public AccentMode AccentMode { get; set; } = AccentMode.Violet;
        public ThemeMode ThemeMode { get; set; } = ThemeMode.System;
    }
}

public static class ThemeCommands
{
    public static ICommand OpenThemeMenuCommand { get; } =
        new DelegateCommand(parameter =>
        {
            if (parameter is FrameworkElement element
                && element.ContextMenu is ContextMenu menu)
            {
                menu.PlacementTarget = element;
                menu.Placement = PlacementMode.Bottom;
                TranslateMenuItems(menu);
                menu.IsOpen = true;
            }
        });

    private static void TranslateMenuItems(ItemsControl parent)
    {
        foreach (object item in parent.Items)
        {
            if (item is not MenuItem menuItem)
            {
                continue;
            }

            if (menuItem.Tag is string sourceText)
            {
                menuItem.Header = TranslateMenuText(sourceText);
            }

            TranslateMenuItems(menuItem);
        }
    }

    private static string TranslateMenuText(string sourceText)
    {
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
        {
            "en" => sourceText switch
            {
                "强调色" => "Accent Color",
                "跟随 Windows 系统强调色" => "Follow Windows accent",
                "固定紫罗兰色" => "Use Violet",
                "界面主题" => "Appearance",
                "跟随系统" => "Follow system",
                "浅色" => "Light",
                "深色" => "Dark",
                _ => sourceText
            },
            "ja" => sourceText switch
            {
                "强调色" => "アクセントカラー",
                "跟随 Windows 系统强调色" => "Windows のアクセントカラーに従う",
                "固定紫罗兰色" => "バイオレットを固定",
                "界面主题" => "外観テーマ",
                "跟随系统" => "システムに従う",
                "浅色" => "ライト",
                "深色" => "ダーク",
                _ => sourceText
            },
            _ => sourceText
        };
    }

    public static ICommand UseWindowsAccentCommand { get; } =
        new DelegateCommand(_ => ThemeManager.SetAccentMode(AccentMode.Windows));

    public static ICommand UseVioletAccentCommand { get; } =
        new DelegateCommand(_ => ThemeManager.SetAccentMode(AccentMode.Violet));

    public static ICommand UseSystemThemeCommand { get; } =
        new DelegateCommand(_ => ThemeManager.SetThemeMode(ThemeMode.System));

    public static ICommand UseLightThemeCommand { get; } =
        new DelegateCommand(_ => ThemeManager.SetThemeMode(ThemeMode.Light));

    public static ICommand UseDarkThemeCommand { get; } =
        new DelegateCommand(_ => ThemeManager.SetThemeMode(ThemeMode.Dark));

    private sealed class DelegateCommand : ICommand
    {
        private readonly Action<object?> _execute;

        public DelegateCommand(Action<object?> execute)
        {
            _execute = execute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => _execute(parameter);
    }
}
