using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class ScopedRegistryEditor : EditorWindow
{
    private Vector2 scrollPosition;
    private ReorderableList reorderableList;
    private readonly List<ScopedRegistry> scopedRegistries = new();
    private const string UpmUrl = "https://package.openupm.com";
    private bool selectAllToggleState;

    /// <summary> A list of predefined scoped registries to add to the manifest.json file. </summary>
    private static readonly List<ScopedRegistry> predefinedRegistries = new List<ScopedRegistry>() {
        
        // Author's registry
        new ("ltmx", UpmUrl, "com.ltmx"),
        
        // A curated list of Unity packages
        new ("UnityNuGet", "https://unitynuget-registry.azurewebsites.net", "org.nuget"),
        
        // Other great creator's registries
        new ("acegikmo", UpmUrl, "com.acegikmo"),
        new ("cysharp", UpmUrl, "com.cysharp"),
        new ("neuecc", UpmUrl, "com.neuecc"),
        new ("vrmc", UpmUrl, "com.vrmc"),
        new ("alelievr", UpmUrl, "com.alelievr"),
        new ("dbrizov", UpmUrl, "com.dbrizov"),
        new ("needle", UpmUrl, "com.needle"),
        new ("yasirkula", UpmUrl, "com.yasirkula"),
        new ("keijiro", "https://registry.npmjs.com", "jp.keijiro"),
        new ("NatML", "https://registry.npmjs.com", new [] { "ai.natml", "ai.fxn" }),
        new ("Roy Theunissen", UpmUrl, "com.roytheunissen"),
        new ("mob-sakai", UpmUrl, "com.coffee"),
        
        
        // Company registries
        new ("Unity", UpmUrl, new [] {
            "com.unity.selection-groups", 
            "com.unity.demoteam",
            "com.unity.hlod",
            "com.unity.material-switch",
            // "com.unity.cluster-display",
            "com.unity.vfx-toolbox",

        }),
        new ("Google", UpmUrl, "com.google"),
        new ("MetaXR", "https://npm.developer.oculus.com", "com.meta.xr"),
    };
    
    // Todo : Add hidden unity packages such as : "com.unity.nuget.newtonsoft-json", "com.unity.cluster-display"

    [MenuItem("Tools/Scoped Registry Editor")]
    public static void ShowWindow() => GetWindow<ScopedRegistryEditor>("Scoped Registry Editor").Show();

    private void OnEnable()
    {
        LoadAndUpdateScopedRegistries();
        reorderableList = CreateReorderableList();
    }
    
    /// <summary> Create a reorderable list for the scoped registries. </summary>
    private ReorderableList CreateReorderableList()
    {
        return new ReorderableList(scopedRegistries, typeof(ScopedRegistry), true, true, true, true) {
            drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Scoped Registries"),
            drawElementCallback = (rect, index, isActive, isFocused) => { ScopedRegistryEditorHelper.DrawRegistryElement(scopedRegistries, rect, index); },
            onAddCallback = list =>
            {
                scopedRegistries.Add(new ScopedRegistry("New Registry", UpmUrl, "", true));
                // Select the new element
                reorderableList.Select(scopedRegistries.Count -1);
            },
            onRemoveCallback = list => { scopedRegistries.RemoveAt(list.index); }
        };
    }

    /// <summary> Load and update the scoped registries from the manifest.json file. </summary>
    private void LoadAndUpdateScopedRegistries()
    {
        scopedRegistries.Clear();
        var manifestJson = LoadManifestJson();
        var scopedRegistriesArray = (JArray)manifestJson?["scopedRegistries"] ?? new JArray();
        // Directly convert loaded registries without sorting or adding predefined ones
        scopedRegistries.AddRange(JObjectToScopedRegistries(scopedRegistriesArray));
        // Check if any predefined registries are missing and add them
        var missingPredefinedRegistries = predefinedRegistries.Where(pre => scopedRegistries.All(sr => sr.name != pre.name)).ToList();
        scopedRegistries.AddRange(missingPredefinedRegistries);
    }

    /// <summary> Convert a JArray of scoped registries to a list of ScopedRegistry objects. </summary>
    private IEnumerable<ScopedRegistry> JObjectToScopedRegistries(JArray registriesArray) =>
        registriesArray.Select(registryJson => new ScopedRegistry(
            (string)registryJson["name"],
            (string)registryJson["url"],
            ((JArray)registryJson["scopes"]).Select(s => (string)s).ToArray(),
            predefinedRegistries.Any(predefined => predefined.name == (string)registryJson["name"])
        ));

    /// <summary> Load the manifest.json file as a JObject. </summary>
    private JObject LoadManifestJson()
    {
        var manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
        return File.Exists(manifestPath) ? JObject.Parse(File.ReadAllText(manifestPath)) : null;
    }

    private void OnGUI()
    {
        DrawSelectAllToggle();
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        reorderableList.DoLayoutList();
        EditorGUILayout.EndScrollView();
        if (GUILayout.Button("Update Registries")) SaveRegistries();
    }

    /// <summary> Draw the "All" toggle to enable/disable all scoped registries. </summary>
    private void DrawSelectAllToggle()
    {
        EditorGUI.BeginChangeCheck();
        selectAllToggleState = EditorGUILayout.ToggleLeft("All", selectAllToggleState);
        if (EditorGUI.EndChangeCheck())
            scopedRegistries.ForEach(r => r.enabled = selectAllToggleState);
    }

    /// <summary> Save the scoped registries to the manifest.json file. </summary>
    /// <remarks> This method will also reload the script assemblies to apply the changes. </remarks>
    private void SaveRegistries()
    {
        var manifestJson = LoadManifestJson() ?? new JObject();
        manifestJson["scopedRegistries"] = new JArray(scopedRegistries.Where(r => r.enabled).Select(r => r.ToJObject()));
        SaveManifestJson(manifestJson.ToString());
        EditorUtility.RequestScriptReload();
        EditorUtility.DisplayDialog("Success", "Scoped Registries have been updated successfully.", "Ok");
    }

    /// <summary> Save the manifest.json file with the given content. </summary>
    /// <param name="content"> The content to save to the manifest.json file. </param>
    private void SaveManifestJson(string content)
    {
        var manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
        File.WriteAllText(manifestPath, content);
    }


    /// <summary> Represents a scoped registry in the manifest.json file. </summary>
    /// <param name="name"> The name of the scoped registry. </param>
    /// <param name="url"> The URL of the scoped registry. </param>
    /// <param name="scopes"> The scopes of the scoped registry. </param>
    /// <param name="enabled"> Whether the scoped registry is enabled. </param>
    [Serializable]
    public record ScopedRegistry(string name, string url, string[] scopes, bool enabled = false)
    {
        public string name { get; set; } = name;
        public string url { get; set; } = url;
        public string[] scopes { get; set; } = scopes;
        public bool enabled { get; set; } = enabled;

        public ScopedRegistry(string name, string url, string scope, bool enabled = false) : this(name, url, new[] { scope }, enabled) { }

        /// <summary> Convert the scoped registry to a JObject. </summary>
        public JObject ToJObject() => new() {
            ["name"] = name,
            ["url"] = url,
            ["scopes"] = new JArray(scopes)
        };
    }

    /// <summary> Helper class for drawing scoped registry elements in the editor. </summary>
    private static class ScopedRegistryEditorHelper
    {
        /// <summary> Draw a scoped registry element in the editor. </summary>
        /// <param name="scopedRegistries"> The list of scoped registries. </param>
        /// <param name="rect"> The rect to draw the element in. </param>
        /// <param name="index"> The index of the element to draw. </param>
        public static void DrawRegistryElement(List<ScopedRegistry> scopedRegistries, Rect rect, int index)
        {
            var registry = scopedRegistries[index];
            rect.y += 2;
            registry.enabled = EditorGUI.ToggleLeft(new Rect(rect.x, rect.y, 20, EditorGUIUtility.singleLineHeight), "", registry.enabled);
            registry.name = EditorGUI.TextField(new Rect(rect.x + 25, rect.y, 120, EditorGUIUtility.singleLineHeight), registry.name);
            registry.url = EditorGUI.TextField(new Rect(rect.x + 150, rect.y, rect.width - 245, EditorGUIUtility.singleLineHeight), registry.url);
            var scopesText = EditorGUI.TextField(new Rect(rect.x + rect.width - 90, rect.y, 90, EditorGUIUtility.singleLineHeight),
                string.Join(", ", registry.scopes));
            registry.scopes = scopesText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
        }
    }
}