using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[FilePath("Assets/CustomForwardPass/Editor/ShaderGraphConversionSettings.asset",
    FilePathAttribute.Location.ProjectFolder)]
public class ShaderGraphConversionToolSettings : ScriptableSingleton<ShaderGraphConversionToolSettings>
{
    [Serializable]
    public class FilePair
    {
        [SerializeField] public string ShaderGraphGuid;
        [SerializeField] public string ConvertedShaderGuid;
    }

    public List<FilePair> ShaderPairs = new List<FilePair>();

    public void AddFilePair(string shaderGraphPath, string convertedShaderPath)
    {
        var filePair = new FilePair
        {
            ShaderGraphGuid = AssetDatabase.AssetPathToGUID(shaderGraphPath),
            ConvertedShaderGuid = AssetDatabase.AssetPathToGUID(convertedShaderPath)
        };
        ShaderPairs.Add(filePair);

        Save(true);
    }

    public string GetConvertedShaderPath(string shaderGraphPath)
    {
        var guid = AssetDatabase.AssetPathToGUID(shaderGraphPath);
        foreach (var filePair in ShaderPairs)
        {
            if (filePair.ShaderGraphGuid == guid)
            {
                return AssetDatabase.GUIDToAssetPath(filePair.ConvertedShaderGuid);
            }
        }

        return null;
    }

    void OnDestroy()
    {
        ShaderPairs.RemoveAll(pair =>
            string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(pair.ShaderGraphGuid)) ||
            string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(pair.ConvertedShaderGuid)));

        Save(true);
    }
}