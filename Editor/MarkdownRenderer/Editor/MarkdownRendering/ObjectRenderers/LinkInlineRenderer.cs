using System.IO;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax.Inlines;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace UIMarkdownRenderer.ObjectRenderers
{
    
    
    public class LinkInlineRenderer : MarkdownObjectRenderer<UIMarkdownRenderer, LinkInline>
    {
        private static string BetterCombinePaths(string basePath, string relativePath) =>
            Path.Combine(basePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), 
                relativePath.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        

        protected override void Write(UIMarkdownRenderer renderer, LinkInline obj)
        {
            string link = obj.GetDynamicUrl != null ? obj.GetDynamicUrl() ?? obj.Url : obj.Url;

            if (!obj.IsImage)
            {
                renderer.OpenLink(link);
                renderer.WriteChildren(obj);
                renderer.CloseLink();
            }
            else
            {
                link = UIMarkdownRenderer.ResolveLink(link);

                if (!link.StartsWith("http"))
                    // link = "file://" + Path.Combine(renderer.FileFolder, link); 
                    link = Path.Combine(renderer.FileFolder, link); 

                var uwr = new UnityWebRequest(link, UnityWebRequest.kHttpVerbGET);
                // Avoid 403 errors, but does not seem to work on some websites still
                // Todo : Fix All Error 403 errors with HttpWebRequests (Unity WebRequests have limited functionality)
                uwr.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows; U; Windows NT 10.1;; en-US) AppleWebKit/602.42 (KHTML, like Gecko) Chrome/54.0.1859.248 Safari/601");
                
                var imgElem = renderer.AddImage();
                
                var attribute = obj.GetAttributes();
                if (attribute.Classes != null)
                {
                    foreach (var c in attribute.Classes)
                    {
                        imgElem.AddToClassList(c);
                    }
                }
                
                imgElem.tooltip = obj.FirstChild?.ToString();

                uwr.downloadHandler = new DownloadHandlerTexture();
                
                var asyncOp = uwr.SendWebRequest();

                asyncOp.completed += _ =>
                {

                    if (link.StartsWith("http"))
                    {
                        imgElem.image = DownloadHandlerTexture.GetContent(uwr);
                    }
                    else  {
                        var tex = new Texture2D(2, 2);
                        var pth = BetterCombinePaths(renderer.FileFolder, link);
                        tex.LoadImage(File.ReadAllBytes(pth));
                        imgElem.image = tex;
                    }

                    if (imgElem.image == null)
                    {
                        // All this is just to prevent empty images from being added to the hierarchy
                        var parent = imgElem.parent;
                        var previous = imgElem.parent.ElementAt(imgElem.parent.IndexOf(imgElem) - 1);
                        parent.Remove(imgElem);
                        parent.Remove(previous);
                    }
                    else
                    {
                        imgElem.Fit();
                        imgElem.RegisterCallback<GeometryChangedEvent>( _ => imgElem.Fit());
                    }
                    uwr.Dispose();
                };
            }
        }
    }
}

