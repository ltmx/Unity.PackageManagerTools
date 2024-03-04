using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;
using static UIMarkdownRenderer.UIMarkdownRenderer;

using PackageInfo = UnityEditor.PackageManager.PackageInfo;

internal class PackageManagerReadmeExtension : IPackageManagerExtension
{
    private TextElement detailContainer;
    public void OnPackageSelected(PackageInfo packageInfo) => EditorApplication.delayCall += () => InjectReadmeIntoPackageManager(packageInfo);

    public void OnPackageSelectionChange(PackageInfo packageInfo) => EditorApplication.delayCall += () => InjectReadmeIntoPackageManager(packageInfo);

    private void InjectReadmeIntoPackageManager(PackageInfo packageInfo)
    {
        // Find the PackageManagerWindow via reflection
        var packageManagerWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.PackageManager.UI.PackageManagerWindow");
        if (packageManagerWindowType == null) return;

        var windows = Resources.FindObjectsOfTypeAll(packageManagerWindowType);
        if (windows == null || windows.Length == 0) return;

        var packageManagerWindow = windows[0]; // Assuming the first found window is the one we want

        // Navigate the visual tree to find the “descriptionTab” or equivalent via reflection
        var rootVisualElementProperty = packageManagerWindowType.GetProperty("rootVisualElement", BindingFlags.Public | BindingFlags.Instance);
        if (rootVisualElementProperty == null) return;

        if (rootVisualElementProperty.GetValue(packageManagerWindow) is not VisualElement rootVisualElement) return;

        detailContainer = rootVisualElement.Query<TextElement>("detailDescription").First(); // this one is a text element
        
        if (detailContainer == null) return;
        if (packageInfo == null) return;
        if (string.IsNullOrEmpty(packageInfo.resolvedPath)) return;
        
        detailContainer.Clear();
        detailContainer.text = "";
        
        var files = Directory.EnumerateFiles(packageInfo.resolvedPath, "*.md", SearchOption.AllDirectories).ToArray();
        var file = files.FirstOrDefault(f => Path.GetFileName(f) is "README.md" or "index.md");
        if (string.IsNullOrEmpty(file)) file = files.FirstOrDefault();
        var description = string.IsNullOrEmpty(file) ? "README.md not found." : File.ReadAllText(file);

        var text = description + "\n\n" + packageInfo.description;

        var x = GenerateVisualElement(text, link => LinkHandler(link, file), true, file);
        detailContainer.Add(x);
    }

    private void LinkHandler(string link, string path)
    {
        if (link.EndsWith(".txt") || link.EndsWith(".md"))
        {
            link = Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, link);
            if (!File.Exists(link)) return;
            // Open file in explorer
            link = Path.GetFullPath(link); // Ensuring the path is absolute
            var args = $"/select, \"{link}\"";
            Process.Start("explorer.exe", args);
        }
        else
        {
            Application.OpenURL(link);
        }
    }

    public VisualElement CreateExtensionUI() => null; // Not used in this workaround


    public void OnPackageAddedOrUpdated(PackageInfo packageInfo) { }

    public void OnPackageRemoved(PackageInfo packageInfo) { }
}

[InitializeOnLoad]
internal static class PackageManagerReadmeExtensionLoader
{
    static PackageManagerReadmeExtensionLoader() => PackageManagerExtensions.RegisterExtension(new PackageManagerReadmeExtension());
}