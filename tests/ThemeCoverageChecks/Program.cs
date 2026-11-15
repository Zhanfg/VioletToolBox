using System.Text.RegularExpressions;
using System.Text.Json;
using System.Xml.Linq;

// This test is intentionally device-free: it guards the XAML surface and handler
// wiring, but does not claim Windows DPI, hardware, or live accent validation.
string repoRoot = Directory.GetCurrentDirectory();
string xamlPath = Path.Combine(repoRoot, "VioletToolBox", "MainWindow.xaml");
string themePath = Path.Combine(repoRoot, "VioletToolBox", "Theming", "ThemeManager.cs");
if (!File.Exists(xamlPath) || !File.Exists(themePath))
{
    throw new FileNotFoundException(
        "Run ThemeCoverageChecks from the repository root so source files are available.");
}

var doc = XDocument.Load(xamlPath, LoadOptions.PreserveWhitespace);
string themeSource = File.ReadAllText(themePath);
var definedTokens = Regex.Matches(
        themeSource,
        "(?:SetBrush\\(|_palette\\[)\\s*\\\"(?<key>App[A-Za-z0-9]+)\\\"")
    .Select(match => match.Groups["key"].Value)
    .ToHashSet(StringComparer.Ordinal);

var pages = new[]
{
    new PageCheck("ScreenMirrorView", "ScreenMirrorLogTextBox",
        new[] { "StartMirrorButton_Click", "StopMirrorButton_Click", "BitrateSlider_ValueChanged" }),
    new PageCheck("BasicFlashView", "FlashLogTextBox",
        new[] { "FlashBootButton_Click", "StartXiaomiFlashButton_Click", "LockBLButton_Click" }),
    new PageCheck("FastbootVisualizationView", "PartitionTableDataGrid",
        new[] { "ReadPartitionTableButton_Click", "WritePartitionButton_Click", "ErasePartitionButton_Click" }),
    new PageCheck("EdlFlashView", "EdlPartitionDataGrid",
        new[] { "EdlReadGptButton_Click", "EdlWritePartButton_Click", "EdlErasePartButton_Click" }),
    new PageCheck("ColorOSAssistantView", "PackagesGrid",
        new[] { "StartBtn_Click", "BtnQueryOnly_Click", "BtnDownloadApk_Click" }),
    new PageCheck("OujiaFlashView", "OugaPartitionTableDataGrid",
        new[] { "StartFlashButton_Click", "AfterSalesStartFlashButton_Click",
            "OugaFlashStopPanel_PreviewMouseLeftButtonUp" }),
    // P4 management and download pages: these have fewer custom controls than P3.
    new PageCheck("SystemZoneView", "FileListTextBox",
        new[] { "LoadSystemZoneLocationButton_Click", "FileListTextBox_Drop",
            "CreateSystemZoneFolderMenuItem_Click" }, 9, true),
    new PageCheck("AppManagementView", "AppListDataGrid",
        new[] { "ReadAppListButton_Click", "FreezeSelectedAppsButton_Click",
            "UninstallSelectedAppsButton_Click" }, 12, true),
    new PageCheck("VioletDownloadView", "VioletDownloadUrlTextBox",
        new[] { "VioletDownloadStartButton_Click", "VioletDownloadPauseButton_Click",
            "VioletDownloadResumeButton_Click" }, 18, true),
    new PageCheck("DownloadView", "DownloadZoneContainer",
        new[] { "DriverTile_MouseLeftButtonDown",
            "BackToDriverTileButton_Click" }, 8, true),
    new PageCheck("RomDownloadview", "RomBrandComboBox",
        new[] { "RomDownloadStartButton_Click",
            "RomSelectCopyLinksButton_Click" }, 16, true),
    new PageCheck("BackupAssistantView", "photoGallery",
        new[] { "BtnLoadImages_Click", "BtnBackup_Click",
            "BtnRestoreContacts_Click" }, 6, true)
};

int totalTokens = 0;
foreach (PageCheck check in pages)
{
    XElement page = doc.Descendants()
        .FirstOrDefault(element =>
            element.Name.LocalName == "Grid" &&
            element.Attributes().Any(attr => attr.Name.LocalName == "Name"
                && attr.Value == check.RootName))
        ?? throw new InvalidOperationException($"Missing page root: {check.RootName}");

    Check(
        HasAttribute(page, "Background", "{DynamicResource AppWindowBackgroundBrush}"),
        $"{check.RootName} must use the runtime background token");
    Check(
        HasAttribute(page, "TextElement.Foreground", "{DynamicResource AppTextBrush}"),
        $"{check.RootName} must inherit the runtime foreground token");

    XElement[] descendants = page.DescendantsAndSelf().ToArray();
    Check(descendants.Any(element => element.Attributes().Any(attr =>
        attr.Name.LocalName == "Name" && attr.Value == check.ControlName)),
        $"{check.RootName} lost critical named control: {check.ControlName}");

    foreach (string handler in check.EventHandlers)
    {
        Check(descendants.Any(element => element.Attributes().Any(attr =>
            attr.Value == handler && EventAttributeNames.Contains(attr.Name.LocalName))),
            $"{check.RootName} lost critical event handler: {handler}");
    }

    if (check.CheckScopedControls)
    {
        int scopedStyles = descendants.Count(el =>
            el.Name.LocalName == "Style" &&
            el.Attributes().Any(attr => attr.Name.LocalName == "BasedOn" &&
                attr.Value.StartsWith("{StaticResource CorePage", StringComparison.Ordinal)));
        Check(scopedStyles >= 12,
            $"{check.RootName} is missing its scoped P4 control palette: {scopedStyles}");
    }

    string pageXaml = page.ToString(SaveOptions.DisableFormatting);
    var uses = Regex.Matches(pageXaml, Regex.Escape("{DynamicResource ") + @"(?<key>App[A-Za-z0-9]+)" + Regex.Escape("}"));
    Check(uses.Count >= check.MinimumTokens,
        $"{check.RootName} still lacks page-level theme coverage: {uses.Count} tokens");

    foreach (Match match in uses)
    {
        string token = match.Groups["key"].Value;
        Check(definedTokens.Contains(token),
            $"{check.RootName} references an undefined runtime palette token: {token}");
    }

    totalTokens += uses.Count;
    Console.WriteLine($"PASS {check.RootName}: {uses.Count} runtime token references, handlers preserved");
}

Check(totalTokens >= 500, "P3 and P4 pages should share at least 500 palette references.");
Console.WriteLine($"PASS P3/P4 theme coverage: {pages.Length} pages, {totalTokens} token references");


// Reproducible baseline from P3 head e0a5cde8: every named XAML control and
// event assignment must survive P4/theme/responsive changes (new names are allowed).
string baselinePath = Path.Combine(repoRoot, "tests", "ThemeCoverageChecks", "ui-parity-baseline.json");
using var baseline = JsonDocument.Parse(File.ReadAllText(baselinePath));
string xamlSource = File.ReadAllText(xamlPath);
ValidateMultiset(
    baseline.RootElement.GetProperty("named_controls").EnumerateArray().Select(el => el.GetString()!).ToArray(),
    Regex.Matches(xamlSource, @"\b(?:x:Name|Name)=""[^""]+""").Select(match => match.Value).ToArray(),
    "named control");
ValidateMultiset(
    baseline.RootElement.GetProperty("event_bindings").EnumerateArray().Select(el => el.GetString()!).ToArray(),
    Regex.Matches(xamlSource,
        @"\b(?:Click|Checked|Unchecked|SelectionChanged|TextChanged|Loaded|Drop|DragOver|MouseDoubleClick|PreviewMouseLeftButtonUp|MouseLeftButtonDown|KeyDown|LostFocus|GotFocus|ValueChanged|ContextMenuOpening|PreviewDragOver|PreviewKeyDown|PreviewMouseWheel)=""[^""]+""")
        .Select(match => match.Value).ToArray(),
    "event binding");
Console.WriteLine("PASS P3/P4 parity: all 536 named controls and 377 event bindings preserved");

// A deterministic structure test cannot prove behavior at each Windows DPI;
// it guards the no-Viewbox architecture and scroll fallbacks.
XElement root = doc.Root ?? throw new InvalidOperationException("XAML window root missing");
Check(root.Attribute("ResizeMode")?.Value == "CanResize",
    "Main window must support resizing");
Check(!root.Elements().Any(element => element.Name.LocalName == "Viewbox"),
    "Main window must not uniformly scale its entire content in a Viewbox");

XElement mainBorder = root.Elements()
    .FirstOrDefault(el => el.Name.LocalName == "Border")
    ?? throw new InvalidOperationException("Main window border missing");
Check(mainBorder.Attribute("Width") is null && mainBorder.Attribute("Height") is null,
    "Main border must size with its window rather than be fixed at 1000x800");

XElement viewport = doc.Descendants()
    .FirstOrDefault(el => el.Name.LocalName == "ScrollViewer" &&
        el.Attributes().Any(a => a.Name.LocalName == "Name" && a.Value == "PageViewport"))
    ?? throw new InvalidOperationException("PageViewport missing");
Check(viewport.Attribute("HorizontalScrollBarVisibility")?.Value == "Auto" &&
      viewport.Attribute("VerticalScrollBarVisibility")?.Value == "Auto",
    "PageViewport must make overflow reachable on narrow/short windows");

XElement canvas = viewport.Descendants()
    .FirstOrDefault(el => el.Name.LocalName == "Grid" &&
        el.Attributes().Any(a => a.Name.LocalName == "Name" && a.Value == "ResponsivePageCanvas"))
    ?? throw new InvalidOperationException("ResponsivePageCanvas missing");
double[] pageMinWidths = canvas.Descendants()
    .Where(el => el.Name.LocalName == "Setter" &&
        el.Attribute("Property")?.Value == "MinWidth")
    .Select(el => double.TryParse(el.Attribute("Value")?.Value, out double width) ? width : -1)
    .ToArray();
Check(pageMinWidths.Contains(900) && pageMinWidths.Contains(700) &&
      double.TryParse(canvas.Attribute("MinHeight")?.Value, out double canvasHeight) &&
      canvasHeight >= 650,
    "Each responsive page needs a scrollable minimum width/height policy");
Check(canvas.Descendants().Any(el => el.Name.LocalName == "DataTrigger" &&
    (el.Attribute("Binding")?.Value.Contains("BackupAssistantView") ?? false)),
    "Backup page must be able to use its narrower layout minimum");

XElement grip = doc.Descendants()
    .FirstOrDefault(el => el.Name.LocalName == "Thumb" &&
        el.Attributes().Any(a => a.Name.LocalName == "Name" && a.Value == "WindowResizeThumb"))
    ?? throw new InvalidOperationException("WindowResizeThumb missing");
Check(grip.Attribute("DragDelta")?.Value == "WindowResizeThumb_DragDelta",
    "Transparent borderless window must support drag resizing");

string sizingSource = File.ReadAllText(Path.Combine(repoRoot, "VioletToolBox", "MainWindow.xaml.cs"));
string layoutSource = File.ReadAllText(Path.Combine(repoRoot, "VioletToolBox", "MainWindow.Layout.cs"));
Check(sizingSource.Contains("ConfigureResponsiveWindowBounds();", StringComparison.Ordinal) &&
      !sizingSource.Contains("Math.Min(ratioH, ratioW)", StringComparison.Ordinal) &&
      layoutSource.Contains("private void WindowResizeThumb_DragDelta", StringComparison.Ordinal),
    "Original global scaling must be replaced with native window bounds");
Console.WriteLine("PASS responsive shell structure: native size, scroll overflow, resize grip");

static void ValidateMultiset(string[] baseline, string[] current, string type)
{
    var frequencies = current
        .GroupBy(entry => entry, StringComparer.Ordinal)
        .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
    foreach (string entry in baseline)
    {
        if (!frequencies.TryGetValue(entry, out int remaining) || remaining <= 0)
        {
            throw new InvalidOperationException($"Missing P3 baseline {type}: {entry}");
        }
        frequencies[entry] = remaining - 1;
    }
}

static bool HasAttribute(XElement element, string localName, string value) =>
    element.Attributes().Any(attr =>
        attr.Name.LocalName == localName && attr.Value == value);

static void Check(bool ok, string message)
{
    if (!ok)
    {
        throw new InvalidOperationException(message);
    }
}

record PageCheck(string RootName, string ControlName, string[] EventHandlers,
    int MinimumTokens = 18, bool CheckScopedControls = false);

static class EventAttributeNames
{
    private static readonly HashSet<string> KnownNames = new(StringComparer.Ordinal)
    {
        "Click", "Checked", "Unchecked", "SelectionChanged", "TextChanged",
        "ValueChanged", "Loaded", "Drop", "DragOver", "MouseDoubleClick",
        "PreviewMouseLeftButtonUp", "MouseLeftButtonDown", "KeyDown",
        "LostFocus", "GotFocus", "ContextMenuOpening", "PreviewDragOver",
        "PreviewKeyDown", "PreviewMouseWheel"
    };

    public static bool Contains(string value) => KnownNames.Contains(value);
}
