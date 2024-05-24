using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using static UIMarkdownRenderer.UIMarkdownRenderer;
using Debug = UnityEngine.Debug;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using TextElement = UnityEngine.UIElements.TextElement;

internal class PackageManagerReadmeExtension : IPackageManagerExtension
{
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

        // Navigate the visual tree to find the "detailDescription" via reflection
        var rootVisualElementProperty = packageManagerWindowType.GetProperty("rootVisualElement", BindingFlags.Public | BindingFlags.Instance);
        if (rootVisualElementProperty == null) return;

        if (rootVisualElementProperty.GetValue(packageManagerWindow) is not VisualElement rootVisualElement) return;

        var detailContainer = rootVisualElement.Q<TextElement>("detailDescription");

        if (detailContainer == null) return;

        var parentElement = detailContainer.parent;
        var markdownContainer = parentElement.Q<VisualElement>(null, "md-container");

        // Remove existing markdown container if it exists
        markdownContainer?.RemoveFromHierarchy();

        // If packageInfo is null or the resolved path is not set, exit
        if (packageInfo == null || string.IsNullOrEmpty(packageInfo.resolvedPath)) return;

        // Find markdown files in the package's resolved path
        var markdownFiles = Directory.EnumerateFiles(packageInfo.resolvedPath, "*.md", SearchOption.AllDirectories).ToArray();
        var readmeFilePath = markdownFiles.FirstOrDefault(f => Path.GetFileName(f) == "README.md" || Path.GetFileName(f) == "index.md")
                             ?? markdownFiles.FirstOrDefault();

        // Set the content of the detail container
        var readmeContent = string.IsNullOrEmpty(readmeFilePath) ? "README.md not found." : File.ReadAllText(readmeFilePath);
        // string combinedText = $"{readmeContent}\n\n{packageInfo.description}";
        var combinedText = $"{readmeContent}\n\n"; // Only show the README.md content since the package description is still existing

        // Possibly do some preprocessing on the text here
        combinedText = ReplaceUnicodeBeforeFE0FWithMap(combinedText);

        // Generate a new VisualElement with the markdown and add it to the parent element
        var markdownVisualElement = GenerateVisualElement(combinedText, link => LinkHandler(link, readmeFilePath), true, readmeFilePath);
        markdownVisualElement.AddToClassList("md-container");

        // Insert the new VisualElement before the detailContainer
        parentElement.Insert(parentElement.IndexOf(detailContainer), markdownVisualElement);
    }

    private void LogUnicodePoints(string input)
    {
        foreach (var c in input)
        {
            if ($"{(int)c:X4}" == "FE0F")
            {
                break;
                // Debug.Log(((int)c).ToString("X"));
            }

            Debug.Log($"Char: {c}, Unicode: {(int)c:X4}");
        }
    }

    public static string RemoveUnicodeFE0F(string input) => input.Replace("\uFE0F", string.Empty);

    public static string ReplaceUnicode(string input)
    {
        var output = string.Empty;
        var previous = char.MinValue;
        foreach (var c in input)
        {
            if (c == '\uFE0F')
            {
                var code = char.ConvertToUtf32(previous.ToString(), 0).ToString("X");
                code = code.Replace("B", "b");
                var prefix = "";
                for (var i = 0; i < 4 - code.Length; i++)
                    prefix += 0;

                code = prefix + code;
                code += "-fe0f";
                var tag = $"<sprite name=\"{code}\">";
                tag = "<sprite name=\"0023\">";
                tag = @"\u20E3";
                output += tag;
                Debug.Log(tag);
            }
            else
                output += c;

            previous = c;
        }

        return output;
    }

    public static string ReplaceUnicodeBeforeFE0F(string input)
    {
        // Pattern to match any character followed by the Unicode FE0F
        // Using positive lookahead to not consume the FE0F in the match, enabling overlapping matches
        var pattern = ".(?=\uFE0F)";

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
        var pattern = ".(?=\uFE0F)";

        return Regex.Replace(input, pattern, match =>
        {
            // Convert the matched character to its Unicode code point in hexadecimal format
            var codePointHex = char.ConvertToUtf32(match.Value, 0).ToString("X4");

            // Check if the map contains the code point and use the mapped value if available
            if (map.TryGetValue(codePointHex, out var spriteIndex))
                return $"<sprite={spriteIndex}>";

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
            Application.OpenURL(link);
    }

    public VisualElement CreateExtensionUI() => null; // Not used in this workaround


    public void OnPackageAddedOrUpdated(PackageInfo packageInfo)
    {
    }

    public void OnPackageRemoved(PackageInfo packageInfo)
    {
    }


    public static string ParseUnicodeToEmojiTag(string input)
    {
        var processedInput = string.Empty;
        foreach (var unicode in ExtractUnicodeCodes(input))
            if (map.TryGetValue(unicode, out var spriteIndex))
                processedInput += $"<sprite={spriteIndex}>";

        return processedInput;
    }

    private static IEnumerable<string> ExtractUnicodeCodes(string text)
    {
        var unicodeSymbols = text.Split('-').Select(code => code.Length > 4 ? code.Substring(0, 4) : code);

        return unicodeSymbols.Where(code => map.ContainsKey(code));
    }

    private static readonly Dictionary<string, int> map = new()
    {
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

        { "265F", 3169 }, //

        { "2663", 3171 }, //
        { "2665", 3172 }, //
        { "2666", 3673 }, //
        { "2668", 3174 }, //
        { "267B", 3175 }, //

        { "267E", 3176 }, //

        { "2692", 3178 }, //

        { "2694", 3180 }, //
        { "2695", 3181 }, //
        { "2696", 3182 }, //
        { "2697", 3183 }, //
        { "2699", 3184 }, //
        { "269B", 3185 }, //

        { "269C", 3186 }, //
        { "26A0", 3187 }, //
        { "26A7", 3189 }, //

        { "26B0", 3192 }, //
        { "26B1", 3193 }, //


        { "26C8", 3198 }, //

        { "26CF", 3200 }, //
        { "26D1", 3201 }, //
        { "26D3", 3202 }, //
        { "26E9", 3204 }, //
        { "26F0", 3206 }, //
        { "26F1", 3207 }, //

        { "26F4", 3210 }, //
        { "26F7", 3212 }, //
        { "26F8", 3213 }, //
        { "26F9", 3214 }, // has a lot of variants

        { "2708", 3236 }, //
        { "2709", 3237 }, //

        { "270C", 3250 }, // has a lot of variants
        { "270D", 3256 }, // has a lot of variants

        { "270F", 3262 }, //
        { "2712", 3263 }, //
        { "2714", 3264 }, //
        { "2716", 3265 }, //
        { "271D", 3266 }, //
        { "2721", 3267 }, //

        { "2733", 3269 }, //
        { "2734", 3770 }, //
        { "2744", 3271 }, //
        { "2747", 3272 }, //

        { "2763", 3279 }, //

        { "2764", 3280 }, //
        { "27A1", 3284 }, //

        { "2934", 3287 }, //
        { "2935", 3288 }, //
        { "2B05", 3289 }, //
        { "2B06", 3290 }, //
        { "2B07", 3291 }, //

        { "3030", 3296 }, //
        { "303D", 3297 }, //
        { "3297", 3298 }, //
        { "3299", 3299 }, //

        { "2660", 3645 } //
    };
}

[InitializeOnLoad]
internal static class PackageManagerReadmeExtensionLoader
{
    static PackageManagerReadmeExtensionLoader()
    {
        PackageManagerExtensions.RegisterExtension(new PackageManagerReadmeExtension());
    }
}