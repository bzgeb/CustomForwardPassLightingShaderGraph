using System.IO;
using UnityEditor;

public class ShaderGraphModificationProcessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        foreach (var importedAsset in importedAssets)
        {
            if (Path.GetExtension(importedAsset).ToLower() == ".shadergraph")
            {
                var guid = new GUID(AssetDatabase.AssetPathToGUID(importedAsset));
                ShaderGraphConversionTool.ConvertShaderGraphWithGuid(guid);
            }
        }
    }
}