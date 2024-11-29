# Unity Package Manager Tools

![LTMX Unity Package Manager Tools Banner Thin](https://github.com/ltmx/Unity.PackageManagerTools/assets/47640688/3677b97d-2bea-44ff-8bb6-7aee4f27ada2)


![GitHub package.json version](https://img.shields.io/github/package-json/v/ltmx/Unity.PackageManagerTools?color=blueviolet)
![GitHub top language](https://img.shields.io/github/languages/top/ltmx/Unity.PackageManagerTools?color=success)
![GitHub](https://img.shields.io/github/license/ltmx/Unity.PackageManagerTools)
[![Made for Unity](https://img.shields.io/badge/Made%20for-Unity-57b9d3.svg?logo=unity&color=blueviolet)](https://unity3d.com)

[![openupm](https://img.shields.io/npm/v/com.ltmx.unity.package-manager.tools?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.ltmx.package-manager.tools)



Enhance your Unity editor experience with the Unity Package Registry Tools, a versatile editor tool designed to seamlessly manage scoped registries in your Unity projects. This powerful editor extension lets you easily add, remove, and manage scoped registries directly from the Unity Editor, providing a streamlined workflow for handling package sources in your `manifest.json` file.

## Features

- #### Enhanced Description *fetched from Documentation files*
- #### Markdown Rendering
- #### Emoji Support
    > ![image](https://github.com/ltmx/Unity.PackageManagerTools/assets/47640688/71953d0c-9985-427a-bef0-b29c986793c5)

- **Predefined Registries**: Comes with a set of predefined scoped registries known for common Unity packages
- #### **Custom Registry Management**: *Add, modify, and remove custom scoped registries*
    > ![Scoped Registry Editor](https://github.com/ltmx/Unity.PackageManagerTools/assets/47640688/55393bf6-0efa-4290-adc6-c7605c4d2cc6)
- **Selective Enable/Disable**: *Conveniently enable or disable individual registries*




## Getting Started

### Installation

To install the Unity Package Registry Tools using the Git URL, follow these simple steps:

1. Open your Unity project and navigate to `Window > Package Manager`.
2. In the Package Manager window, click the `+` button located at the top left and select `Add package from git URL...`.
3. Enter this URL: `https://github.com/ltmx/Unity.PackageRegistryTools.git`
4. Click `Add` and Unity will begin importing the package into your project.

Ensure you have Git installed and properly setup in your system's PATH to use this feature
### Usage

1. Access the tool via `Tools > Scoped Registry Editor` in the Unity main menu.
2. Use the intuitive interface to manage your scoped registries:
    - **Add Registry**: Click on the '+' button to add a new registry entry.
    - **Remove Registry**: Select a registry and click the '-' button to remove the selected registry.
    - **Edit Registry**: Modify the name, URL, or scopes directly in the list.
    - **Enable/Disable Registries**: Toggle the checkbox next to each registry to enable or disable it as required.
    - **Reorder Registries**: The order of the registries are reflected in the package manager (reorderable list feature)
3. After configuring your scoped registries, click `Update Registries` to apply the changes to your `manifest.json` file.
4. A dialog box will confirm the successful update of scoped registries.
5. You can edit the default package list by modifying them from the editor script, here
   
   ```cs
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
        
        // Company registries
        new ("Google", UpmUrl, "com.google"), // h
        new ("MetaXR", "https://npm.developer.oculus.com", "com.meta.xr"),
    };
   ```

## Featured Scoped Registries

This tool includes predefined configurations for essential scoped registries like UnityNuGet, offering a diverse selection of packages, tools, SDKs, and libraries crucial for Unity development.

<details>
<summary><strong>UnityNuGet</strong></summary>

UnityNuGet offers a bridge to NuGet packages, allowing Unity developers to easily integrate thousands of .NET libraries into their projects. It's particularly useful for projects that rely on advanced .NET features or external .NET libraries.

**Registry Details**
- **Name**: UnityNuGet
- **URL**: `https://unitynuget-registry.azurewebsites.net`
- **Scopes**: `org.nuget`

</details>

<details>
<summary><strong>MetaXR</strong></summary>

The MetaXR Scoped Registry is a must-have for developers working on VR and AR applications, especially those targeting Oculus devices. It provides access to Oculus SDKs and tools essential for VR development.

**Registry Details**
- **Name**: MetaXR
- **URL**: `https://npm.developer.oculus.com`
- **Scopes**: `com.meta.xr`

</details>



## Contributing

Contributions are welcome! If you've identified a bug, have an idea for improvement, or want to propose a new feature, feel free to open an issue or submit a pull request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Aknowledgements

The Markdown rendering is made using [UMV](https://github.com/gwaredd/UnityMarkdownViewer) as an embedded package

---

Enjoy a more efficient Unity package management experience with Unity Package
