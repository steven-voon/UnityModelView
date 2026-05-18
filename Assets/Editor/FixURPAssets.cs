using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FixURPAssets
{
    [MenuItem("Tools/Fix URP Assets")]
    static void Fix()
    {
        FixPipelineAsset();
        FixGlobalSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Done — try exporting again.");
    }

    static void FixPipelineAsset()
    {
        var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/Mobile_RPAsset.asset");
        if (asset == null) { Debug.LogError("Mobile_RPAsset not found"); return; }

        var so = new SerializedObject(asset);
        // k_LastVersion = 12 in URP 17.4.0
        var versionProp = so.FindProperty("k_AssetVersion");
        var prevVersionProp = so.FindProperty("k_AssetPreviousVersion");
        if (versionProp != null) { versionProp.intValue = 12; Debug.Log("Set k_AssetVersion = 12"); }
        if (prevVersionProp != null) { prevVersionProp.intValue = 12; }
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
    }

    static void FixGlobalSettings()
    {
        var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/Settings/UniversalRenderPipelineGlobalSettings.asset");
        if (asset == null) { Debug.LogError("UniversalRenderPipelineGlobalSettings not found"); return; }

        var so = new SerializedObject(asset);
        // k_LastVersion = 8 in URP 17.4.0
        var versionProp = so.FindProperty("m_AssetVersion");
        if (versionProp != null) { versionProp.intValue = 8; Debug.Log("Set m_AssetVersion = 8"); }
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
    }
}
