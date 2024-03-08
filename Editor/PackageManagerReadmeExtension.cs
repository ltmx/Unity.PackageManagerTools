using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Swan;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UIMarkdownRenderer.UIMarkdownRenderer;
using Debug = UnityEngine.Debug;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using TextElement = UnityEngine.UIElements.TextElement;

internal class PackageManagerReadmeExtension : IPackageManagerExtension
{
    private TextElement detailContainer;
    private TextSettings textSettings;
    
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
        // LogUnicodePoints(text);
        text = ReplaceUnicodeBeforeFE0FWithMap(text);

        var x = GenerateVisualElement(text, link => LinkHandler(link, file), true, file);
        // document = rootVisualElement.visualTreeAssetSource.stylesheets.FirstOrDefault().
        // document.panelSettings = Resources.Load<PanelSettings>("Panel Settings");
        // Debug.Log(document.panelSettings != null);
        // document.panelSettings.textSettings.fallbackSpriteAssets = new List<SpriteAsset> { Resources.Load<SpriteAsset>("Sprite Assets/emoji_sheet") };
        // Debug.Log("UI Document: " + document);
        textSettings = Resources.Load<TextSettings>("Text Settings");
        // textSettings.set
        // detailContainer.
        detailContainer.Add(x);
    }
    
    private void LogUnicodePoints(string input)
    {
        foreach (char c in input)
        {
            if ($"{(int)c:X4}" == "FE0F")
            {
                break;
                // Debug.Log(((int)c).ToString("X"));
            }
            Debug.Log($"Char: {c}, Unicode: {(int)c:X4}");
        }
    }
    
    public static string RemoveUnicodeFE0F(string input)
    {
        return input.Replace("\uFE0F", string.Empty);
    }

    public static string ReplaceUnicode(string input)
    {
        string output = string.Empty;
        char previous = char.MinValue;
        foreach (char c in input)
        {
            if (c == '\uFE0F')
            {
                var code = char.ConvertToUtf32(previous.ToString(), 0).ToString("X");
                code = code.Replace("B", "b");
                string prefix = "";
                for (int i = 0; i < 4 - code.Length ; i++)
                {
                    prefix += 0;
                }

                code = prefix + code;
                code += "-fe0f";
                var tag = $"<sprite name=\"{code}\">";
                tag = $"<sprite name=\"0023\">";
                tag = @"\u20E3";
                    output += tag;
                Debug.Log(tag);
            }
            else
            {
                output += c;
            }
            previous = c;
        }

        return output;
    }
    
    public static string ReplaceUnicodeBeforeFE0F(string input)
    {
        // Pattern to match any character followed by the Unicode FE0F
        // Using positive lookahead to not consume the FE0F in the match, enabling overlapping matches
        string pattern = ".(?=\uFE0F)";

        return Regex.Replace(input, pattern, match =>
        {
            // Convert the matched character to its Unicode code point
            var codePoint = char.ConvertToUtf32(match.Value, 0).ToString("X");
            // Construct the replacement string with the Unicode value in the desired format
            return $"<sprite={codePoint}>";
        }).Replace("\uFE0F", ""); // Finally, remove all the FE0F occurrences
    }
    
    public static string ReplaceUnicodeBeforeFE0FWithMap(string input)
    {
        string pattern = ".(?=\uFE0F)";
    
        return Regex.Replace(input, pattern, match =>
        {
            // Convert the matched character to its Unicode code point in hexadecimal format
            var codePointHex = char.ConvertToUtf32(match.Value, 0).ToString("X4");
            // Check if the map contains the code point and use the mapped value if available
            if (map.TryGetValue(codePointHex, out var spriteIndex)) 
            {
                return $"<sprite={spriteIndex}>";
            }
            // Fallback to the original match if no mapping exists
            return match.Value;
        }).Replace("\uFE0F", ""); // Finally, remove all the FE0F occurrences
    }

    // public static string ReplaceUnicodeBeforeFE0FWithMap(string input)
    // {
    //     // This regex pattern matches Unicode scalar values, considering potential surrogate pairs
    //     // It looks ahead for the \uFE0F variant selector, indicating an emoji presentation
    //     var pattern = @"[\uD800-\uDBFF][\uDC00-\uDFFF]|[\u0000-\uFFFF](?=\uFE0F)";
    //
    //     return Regex.Replace(input, pattern, match =>
    //     {
    //         // Handling surrogate pairs to get the correct code point
    //         var codePoint = char.IsHighSurrogate(match.Value[0])
    //             ? char.ConvertToUtf32(match.Value[0], match.Value[1])
    //             : match.Value[0];
    //         var codePointHex = codePoint.ToString("X4");
    //
    //         if (map.TryGetValue(codePointHex, out var spriteIndex)) return $"<sprite={spriteIndex}>";
    //         return match.Value; // Fallback to the original match if no mapping exists
    //     }).Replace("\uFE0F", ""); // Remove all occurrences of the FE0F variant selector
    // }


    // public static string ReplaceUnicodeBeforeFE0FWithMap(string input)
    // {
    //     var pattern = @"([\uD800-\uDBFF][\uDC00-\uDFFF]|[\u0000-\uFFFF])\uFE0F?";
    //
    //     var result = Regex.Replace(input, pattern, match =>
    //     {
    //         var matchedString = match.Groups[1].Value; // Get the matched character or surrogate pair
    //         var codePoint = char.IsHighSurrogate(matchedString[0]) ? char.ConvertToUtf32(matchedString[0], matchedString[1]) : matchedString[0];
    //         var codePointHex = codePoint.ToString("X4");
    //
    //         // Checking and replacing based on dictionary mapping
    //         if (map.TryGetValue(codePointHex, out var spriteIndex)) return $"<sprite={spriteIndex}>";
    //
    //         // Returning the original matched string if no mapping exists
    //         // This ensures we do not lose characters unintentionally
    //         return match.Value;
    //     });
    //
    //     // Since the variant selector (\uFE0F) is integral to certain emoji displays,
    //     // we only remove it if it's not followed by a valid emoji match in your dictionary.
    //     // This nuanced approach aims to balance between correct display and clean text.
    //     return result;
    // }
    
    

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
    
    
    
    public static string ParseUnicodeToEmojiTag(string input)
    {
        string processedInput = string.Empty;
        foreach (string unicode in ExtractUnicodeCodes(input))
        {
            if (map.TryGetValue(unicode, out int spriteIndex))
            {
                processedInput += $"<sprite={spriteIndex}>";
            }
        }
        return processedInput;
    }
    private static IEnumerable<string> ExtractUnicodeCodes(string text)
    {
        var unicodeSymbols = text.Split('-').Select(code => code.Length > 4 ? code.Substring(0, 4) : code);
        return unicodeSymbols.Where(code => map.ContainsKey(code));
    }

    private static Dictionary<string, int> map = new () {
        { "0023", 0 },
        { "002A", 1 },
        { "0030", 2 },
        { "0031", 3 },
        { "0032", 4 },
        { "0033", 5 },
        { "0034", 6 },
        { "0035", 7 },
        { "0036", 8 },
        { "0037", 9 },
        { "0038", 10 },
        { "0039", 11 },
        { "00A9", 12 },
        { "00AE", 13 },
        { "2934", 3765 },
        { "2935", 3766 },
        { "2B05", 3767 },
        { "2B06", 3768 },
        { "2B07", 3769 },
        { "3030", 3774 },
        { "303D", 3775 },
        { "3297", 3776 },
        { "3299", 3777 },
        { "27A1", 3762 },
        { "2764", 3758 },
        { "2763", 3755 },
        { "2733", 3745 },
        { "2734", 3746 },
        { "2744", 3747 },
        { "2747", 3748 },
        { "270F", 3738 },
        { "2712", 3739 },
        { "2714", 3740 },
        { "2716", 3741 },
        { "271D", 3742 },
        { "2721", 3743 },
        { "270D", 3732 },
        { "270C", 3726 },
        { "2708", 3712 },
        { "2709", 3713 },
        { "26F9", 3702 },
        { "26F7", 3688 },
        { "26F8", 3689 },
        { "26F4", 3686 },
        { "26F0", 3682 },
        { "26F1", 3683 },
        { "26E9", 3680 },
        { "26D3", 3678 },
        { "26CF", 3675 },
        { "26D1", 3676 },
        { "26C8", 3673 },
        { "26B0", 3667 },
        { "26B1", 3668 },
        { "26A7", 3664 },
        { "269C", 3661 },
        { "26A0", 3662 },
        { "2694", 3655 },
        { "2695", 3656 },
        { "2696", 3657 },
        { "2697", 3658 },
        { "2699", 3659 },
        { "269B", 3660 },
        { "2692", 3653 },
        { "265F", 3644 },
        { "2660", 3645 },
        { "2663", 3646 },
        { "2665", 3647 },
        { "2666", 3648 },
        { "2668", 3649 },
        { "267B", 3650 },
        { "267E", 3651 },
    };
}

[InitializeOnLoad]
internal static class PackageManagerReadmeExtensionLoader
{
    static PackageManagerReadmeExtensionLoader() => PackageManagerExtensions.RegisterExtension(new PackageManagerReadmeExtension());
}