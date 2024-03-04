using System.Text.RegularExpressions;
using UnityEngine;

public static class MarkdownToUnityRichTextConverter
{
    public static string Convert(string markdown)
    {
        // Remove comments first
        markdown = Regex.Replace(markdown, @"<!--(.*?)-->", "", RegexOptions.Singleline);
        // Handle code blocks
        // Handle code blocks first
        markdown = Regex.Replace(markdown, @"```(.*?)```", "<color=#CCCCCC><line-height=150%><indent=20px><i>$1</i></indent></line-height</color>", RegexOptions.Singleline);
        
        // Convert headers (Unity doesn't support different sizes, so we use <b> and <size> to differentiate)
        markdown = Regex.Replace(markdown, @"^# (.*)", "<b><size=24>$1</size></b>\n", RegexOptions.Multiline);
        markdown = Regex.Replace(markdown, @"^## (.*)", "<b><size=18>$1</size></b>\n", RegexOptions.Multiline);
        markdown = Regex.Replace(markdown, @"^### (.*)", "<b><size=16>$1</size></b>\n", RegexOptions.Multiline);
        markdown = Regex.Replace(markdown, @"^#### (.*)", "<b><size=14>$1</size></b>\n", RegexOptions.Multiline);
        
        // Convert strong emphasis (bold)
        markdown = Regex.Replace(markdown, @"\*\*(.*?)\*\*", "<b>$1</b>");
        
        // Convert emphasis (italic)
        markdown = Regex.Replace(markdown, @"\*(.*?)\*", "<i>$1</i>");
        markdown = ConvertLinks(markdown);
        
        // Convert hyperlinks
        // markdown = Regex.Replace(markdown,  @"\[!\[([^\]]+)\]\(([^)]+)\)\]\(([^)]+)\)", "<a href=\"$3\">$1</a> "); // nested image link inside link
        // markdown = Regex.Replace(markdown, @"!\[([^\]]*)\]\(([^)]+)\)", "<a href=\"$2\">$1</a> "); // image links
        // markdown = Regex.Replace(markdown, @"\[([^\]]*)\]\(([^)]+)\)", "<a href=\"$2\">$1</a> "); // image links
        
        // markdown = Regex.Replace(markdown, @"\[(?<text>[^\]]+)]\((?<url>https?://[^)]+)\)", "<a href=\"$2\">$1</a>" );
        // markdown = Regex.Replace(markdown, @"!\[([^\]]*)\]\(([^)]+)\)", "$1");
        
        // markdown = Regex.Replace(markdown, @"\[([^\]]+)\]\(([^)]+)\)", "<a href=\"$2\">$1</a>");
        // markdown = Regex.Replace(markdown, @"!\[([^\]]*)\]\(([^)]+)\)", "<img src=\"$2\" alt=\"$1\" />");
        // Convert hyperlinks
        // markdown = Regex.Replace(markdown, @"(?<!\!)\[([^\]]*)\]\(([^)]+)\)", "<a href=\"$2\">$1</a> ");
        
        // If clickable links are needed, a more specialized UI element would be required.
        // markdown = Regex.Replace(markdown, @"\!\[([^\]]+)\]\(([^)]+)\)", "");
        // markdown = Regex.Replace(markdown, @"\[([^\]]+)\]\(([^)]+)\)", "$1");
        
        // markdown = Regex.Replace(markdown, @"(?<!\!)\[([^\]]+)\]\(([^)]+)\)", "<a href=\"$2\">$1</a>");
        // Process Markdown links to HTML links
        

        

        // Convert bullet points (- or *)
        markdown = Regex.Replace(markdown, @"^- (.*)", "• $1", RegexOptions.Multiline);
        markdown = Regex.Replace(markdown, @"^\* (.*)", "• $1", RegexOptions.Multiline);
        return markdown;
    }
    
    public static string ConvertLinks(string markdown)
    {
        // Convert image links nested into regular links
        markdown = Regex.Replace(markdown, @"\[(!\[[^\]]*\]\([^\)]+\))\]\(([^\)]+)\)", m =>
            $"<a href=\"{ensureURLDoesNotInterfereWithXML(m.Groups[2].Value)}\">{m.Groups[3].Value}</a>");
        // Convert image links
        markdown = Regex.Replace(markdown, @"!\[([^\]]*)\]\(([^\)]+)\)", m =>
            $"<a href=\"{ensureURLDoesNotInterfereWithXML(m.Groups[2].Value)}\">{m.Groups[1].Value}</a>");
        Debug.Log(markdown);
        // Convert regular links
        markdown = Regex.Replace(markdown, @"\[([^\]]+)\]\(([^\)]+)\)", m =>
            $"<a href=\"{ensureURLDoesNotInterfereWithXML(m.Groups[2].Value)}\">{m.Groups[1].Value}</a>");
        return markdown;
    }
    
    public static string ensureURLDoesNotInterfereWithXML(string url)
    {
        url = url.Replace("&", "&amp;");
        url = url.Replace("<", "&lt;");
        url = url.Replace(">", "&gt;");
        url = url.Replace("\"", "&quot;");
        url = url.Replace("'", "&apos;");
        return url;
    }
}