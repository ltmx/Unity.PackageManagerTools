using UnityEngine;
using UnityEngine.UIElements;

namespace UIMarkdownRenderer
{
    public static class ImageExtension
    {
        public static void Fit(this Image img)
        {
            img.scaleMode = ScaleMode.ScaleToFit;
            img.sourceRect = new Rect(0, 0, img.image.width, img.image.height);
            img.style.height = img.image.height * (img.resolvedStyle.width / img.image.width);
        }
    }
}