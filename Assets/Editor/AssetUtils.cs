#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class AssetUtils
{
    public static void OverwriteAsset<T>(T asset, string path) where T : Object
    {
        var existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);

        if (existingAsset != null)
        {
            EditorUtility.CopySerialized(asset, existingAsset);
            EditorUtility.SetDirty(existingAsset);
        }
        else
        {
            AssetDatabase.CreateAsset(asset, path);
        }
        
        AssetDatabase.SaveAssets();
    }
}
#endif