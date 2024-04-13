using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public static class SetCustomFallbackSpriteAsset
{
    public static TextStyleSheet styleSheet;

    static SetCustomFallbackSpriteAsset()
    {
        ModifyStaticTextGenerationSettings(Resources.Load<SpriteAsset>("Sprite Assets/emoji_sheet"));
        Debug.Log("Custom fallback sprite asset set successfully.");
    }

    #if UNITY_EDITOR
    [MenuItem("Tools/Package Manager/Set Custom Emoji Asset")]
    #endif
    public static void SetCustomEmojiAsset()
    {
        ModifyStaticTextGenerationSettings(Resources.Load<SpriteAsset>("Sprite Assets/emoji_sheet"));
    }

    private static void ModifyStaticTextGenerationSettings(SpriteAsset spriteAsset)
    {
        // TextHandle
        // Assume the assembly containing the TextHandle class is not loaded by default and specify its path
        // string assemblyPath = "Path to the assembly containing TextHandle class";
        // Load the assembly
        // Assembly textCoreAssembly = Assembly.LoadFile(assemblyPath);
        var textCoreAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "UnityEngine.TextCoreTextEngineModule");

        // Find the Type of the internal class 'TextHandle' from the loaded assembly
        var textHandleType = textCoreAssembly?.GetType("UnityEngine.TextCore.Text.TextHandle", false);
        if (textHandleType != null)
        {
            // Access the static field 's_LayoutSettings' from the class
            var s_LayoutSettingsField = textHandleType.GetField("s_LayoutSettings", BindingFlags.Static | BindingFlags.NonPublic);

            if (s_LayoutSettingsField != null)
            {
                // Get the value of the static field s_LayoutSettings
                var s_LayoutSettingsValue = s_LayoutSettingsField.GetValue(null); // null because it's a static field

                if (s_LayoutSettingsValue == null) return;

                // Find the property called 'spriteAsset' within s_LayoutSettingsValue
                var spriteAssetProperty = s_LayoutSettingsValue.GetType().GetField("spriteAsset"); // the spriteAsset field in the TextSettings class

                if (spriteAssetProperty == null) return;

                // Set the spriteAsset property to the new value
                // spriteAssetProperty.SetValue(s_LayoutSettingsValue, spriteAsset);

                // Debug.Log(spriteAssetProperty.GetValue(s_LayoutSettingsValue));

                var p_textSettingsField = s_LayoutSettingsValue.GetType().GetField("textSettings");
                var p_textStyleSheetField = s_LayoutSettingsValue.GetType().GetField("styleSheet");

                var textSettingsValue = p_textSettingsField.GetValue(s_LayoutSettingsValue);
                var textStyleSheetValue = p_textStyleSheetField.GetValue(s_LayoutSettingsValue);

                if (textSettingsValue != null && styleSheet != null)
                {
                    s_LayoutSettingsField.SetValue(textStyleSheetValue, styleSheet);
                    // check if property was set successfully
                    Debug.Log(s_LayoutSettingsField.GetValue(textStyleSheetValue));
                }

                if (textSettingsValue == null) return;
                
                var defaultSpriteAssetField = textSettingsValue.GetType().GetField("m_DefaultSpriteAsset", BindingFlags.NonPublic | BindingFlags.Instance);
                // FieldInfo defaultTextStyleSheetField = textSettingsValue.GetType().GetField("m_DefaultStyleSheet", BindingFlags.NonPublic | BindingFlags.Instance);

                if (defaultSpriteAssetField == null) return;
                    
                defaultSpriteAssetField.SetValue(textSettingsValue, spriteAsset);
                // check if property was set successfully
                Debug.Log(defaultSpriteAssetField.GetValue(textSettingsValue));

                // If s_LayoutSettingsValue is a struct, reflect the changes back to the static field
                // Uncomment the line below if necessary.
                // s_LayoutSettingsField.SetValue(null, s_LayoutSettingsValue);
            }
            else
            {
                Debug.Log("Failed to set custom fallback sprite asset. The field might not exist in this Unity version.");
            }
        }
        else
        {
            Debug.Log("Failed to set custom fallback sprite asset. The field might not exist in this Unity version.");
        }
    }
}