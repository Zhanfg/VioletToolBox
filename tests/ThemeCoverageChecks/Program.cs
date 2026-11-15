using System.Text.RegularExpressions;
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
            "OugaFlashStopPanel_PreviewMouseLeftButtonUp" })
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

    string pageXaml = page.ToString(SaveOptions.DisableFormatting);
    var uses = Regex.Matches(pageXaml, Regex.Escape("{DynamicResource ") + @"(?<key>App[A-Za-z0-9]+)" + Regex.Escape("}"));
    Check(uses.Count >= 18,
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

Check(totalTokens >= 300, "Core pages should share at least 300 palette references.");
Console.WriteLine($"PASS P3 theme coverage: {pages.Length} core pages, {totalTokens} token references");

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

record PageCheck(string RootName, string ControlName, string[] EventHandlers);

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
