
![Scoped Registry Editor Effect](https://github.com/ltmx/Unity.PackageRegistryTools/assets/47640688/55393bf6-0efa-4290-adc6-c7605c4d2cc6)

# Unity Package Registry Tools

Enhance your Unity editor experience with the Unity Package Registry Tools, a versatile editor tool designed to seamlessly manage scoped registries in your Unity projects. This powerful editor extension lets you easily add, remove, and manage scoped registries directly from the Unity Editor, providing a streamlined workflow for handling package sources in your `manifest.json` file.

## Features

- **Predefined Registries**: Comes with a set of predefined scoped registries known for common Unity packages
- **Custom Registry Management**: Add, modify, and remove custom scoped registries
- **Selective Enable/Disable**: Conveniently enable or disable individual registries

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
3. After configuring your scoped registries, click `Update Registries` to apply the changes to your `manifest.json` file.
4. A dialog box will confirm the successful update of scoped registries.
5. You can edit the default package list by modifying them from the editor script, here
   
   ```cs
    /// <summary> A list of predefined scoped registries to add to the manifest.json file. </summary>
    private static readonly List<ScopedRegistry> predefinedRegistries = new List<ScopedRegistry>() {
        new ("acegikmo", UpmUrl, "com.acegikmo"),
        new ("ltmx", UpmUrl, "com.ltmx"),
        new ("cysharp", UpmUrl, "com.cysharp"),
        new ("neuecc", UpmUrl, "com.neuecc"),
        new ("vrmc", UpmUrl, "com.vrmc"),
        new ("alelievr", UpmUrl, "com.alelievr"),
        new ("dbrizov", UpmUrl, "com.dbrizov"),
        new ("needle", UpmUrl, "com.needle"),
        new ("yasirkula", UpmUrl, "com.yasirkula"),
        new ("Google", UpmUrl, "com.google"),

        new ("MetaXR", "https://npm.developer.oculus.com", "com.meta.xr"),
        new ("UnityNuGet", "https://unitynuget-registry.azurewebsites.net", "org.nuget"),
        new ("keijiro", "https://registry.npmjs.com", "jp.keijiro"),
    };
   ```

## Contributing

Contributions are welcome! If you've identified a bug, have an idea for improvement, or want to propose a new feature, feel free to open an issue or submit a pull request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

Enjoy a more efficient Unity package management experience with Unity Package