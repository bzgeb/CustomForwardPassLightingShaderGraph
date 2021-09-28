using UnityEditor;
using System.IO;
using UnityEngine;

public class ShaderGraphConversionTool : Editor
{
    const string PBRForwardPassInclude = "#include \"Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl\"";
    const string CustomInclude = "#include \"CustomForwardPass.hlsl\"";

    [MenuItem("Tools/Convert Shader in CopyPaste Buffer")]
    public static void ConvertShaderInBuffer()
    {
        var shader = GUIUtility.systemCopyBuffer;
        var convertedShader = shader.Replace(PBRForwardPassInclude, CustomInclude);
        GUIUtility.systemCopyBuffer = convertedShader;

        var filePath = EditorUtility.SaveFilePanel("Converted Shader", "Assets", "ConvertedShader", "shader");
        if (!string.IsNullOrEmpty(filePath))
        {
            File.WriteAllText(filePath, convertedShader);
            AssetDatabase.Refresh();
        }
    }
}