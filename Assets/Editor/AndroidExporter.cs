using System.IO;
using UnityEditor;
using UnityEngine;

public class AndroidExporter
{
    [MenuItem("Build/Export Android to React Native")]
    public static void ExportAndroidProject()
    {
        string exportPath = System.Environment.GetEnvironmentVariable("UNITY_ANDROID_EXPORT_PATH")
            ?? Path.GetFullPath("../builds/android");

        string rnAndroidPath = System.Environment.GetEnvironmentVariable("UNITY_RN_PROJECT_PATH")
            ?? "/Users/j/Dev/sultan-azlan-shah-interactives/14.1-weapons/android";

        // Delete existing export if present
        if (Directory.Exists(exportPath))
            Directory.Delete(exportPath, true);

        Directory.CreateDirectory(exportPath);

        // Configure build settings
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;

        // Collect enabled scenes
        var scenes = new System.Collections.Generic.List<string>();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
                scenes.Add(scene.path);
        }

        if (scenes.Count == 0)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets" }))
                scenes.Add(AssetDatabase.GUIDToAssetPath(guid));
        }

        Debug.Log($"Exporting {scenes.Count} scene(s) to {exportPath}");

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes.ToArray(),
            locationPathName = exportPath,
            target = BuildTarget.Android,
            options = BuildOptions.AcceptExternalModificationsToPlayer
        });

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.LogError($"Android export failed: {report.summary.result}");
            if (Application.isBatchMode) EditorApplication.Exit(1);
            return;
        }

        Debug.Log("Export succeeded. Copying unityLibrary to RN project...");
        CopyUnityLibrary(exportPath, rnAndroidPath);
    }

    static void CopyUnityLibrary(string exportPath, string rnAndroidPath)
    {
        string src = Path.Combine(exportPath, "unityLibrary");
        string dst = Path.Combine(rnAndroidPath, "unityLibrary");

        if (!Directory.Exists(src))
        {
            Debug.LogError($"unityLibrary not found at {src}");
            return;
        }

        if (Directory.Exists(dst))
            Directory.Delete(dst, true);

        CopyDirectory(src, dst);
        Debug.Log($"unityLibrary copied to {dst}");
    }

    static void CopyDirectory(string src, string dst)
    {
        Directory.CreateDirectory(dst);
        foreach (var file in Directory.GetFiles(src))
            File.Copy(file, Path.Combine(dst, Path.GetFileName(file)), overwrite: true);
        foreach (var dir in Directory.GetDirectories(src))
            CopyDirectory(dir, Path.Combine(dst, Path.GetFileName(dir)));
    }
}
