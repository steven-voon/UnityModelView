# Android Export Guide

## Menu Options

### Build → Export Android to React Native
Exports the Unity project **and automatically copies** `unityLibrary` into the React Native project. No manual steps needed.

### Build → Export Android Only
Exports the Unity project to the local builds folder only. Use this if you want to inspect the output or copy it manually.

Default output path: `../builds/android/` (one level above this Unity project folder)

---

## Manually Copying to React Native

After running **Export Android Only**, copy the `unityLibrary` folder to the React Native project.
Replace `<EXPORT_PATH>` and `<RN_PROJECT_PATH>` with your actual paths (see [Changing the Paths](#changing-the-paths) below).

### macOS / Linux
```bash
rm -rf "<RN_PROJECT_PATH>/android/unityLibrary"
cp -R "<EXPORT_PATH>/unityLibrary" "<RN_PROJECT_PATH>/android/unityLibrary"
```

### Windows
```bat
rmdir /s /q "<RN_PROJECT_PATH>\android\unityLibrary"
xcopy /e /i "<EXPORT_PATH>\unityLibrary" "<RN_PROJECT_PATH>\android\unityLibrary"
```

---

## Changing the Paths

Both paths are configured in `Assets/Editor/AndroidExporter.cs` and can be overridden with environment variables — no code change needed.

### Option 1: Environment Variables (recommended)

| Variable | Description | Default |
|---|---|---|
| `UNITY_ANDROID_EXPORT_PATH` | Where Unity exports the Gradle project | `../builds/android` (relative to Unity project) |
| `UNITY_RN_PROJECT_PATH` | The `android/` folder of the React Native project | *(hardcoded in script)* |

Set them in your shell before opening Unity:
```bash
export UNITY_ANDROID_EXPORT_PATH="/your/export/path"
export UNITY_RN_PROJECT_PATH="/your/rn/project/android"
```

### Option 2: Edit the script directly

Open `Assets/Editor/AndroidExporter.cs` and update the fallback paths in both `ExportAndroidOnly()` and `ExportAndroidProject()`:

```csharp
string exportPath = System.Environment.GetEnvironmentVariable("UNITY_ANDROID_EXPORT_PATH")
    ?? Path.GetFullPath("../builds/android"); // <-- change this

string rnAndroidPath = System.Environment.GetEnvironmentVariable("UNITY_RN_PROJECT_PATH")
    ?? "/path/to/your/rn-project/android"; // <-- change this
```
