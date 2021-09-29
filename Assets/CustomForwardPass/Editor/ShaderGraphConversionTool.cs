using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class ShaderGraphConversionTool : Editor
{
    const string PBRForwardPassInclude =
        "#include \"Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl\"";

    const string CustomInclude = "#include \"CustomForwardPass.hlsl\"";

    [MenuItem("Tools/Convert Shader in CopyPaste Buffer")]
    static void ConvertShaderInBuffer()
    {
        var shader = GUIUtility.systemCopyBuffer;
        var convertedShader = ConvertShader(shader);
        GUIUtility.systemCopyBuffer = convertedShader;
        WriteShaderToFile(convertedShader);
    }

    [MenuItem("Tools/Convert Shader Graph Asset")]
    static void ConvertShaderGraphAsset()
    {
        var selection = Selection.activeObject;
        Debug.Log($"{selection}, {selection.GetType()}");

        var assetGUIDs = Selection.assetGUIDs;
        foreach (var assetGUID in assetGUIDs)
        {
            var path = AssetDatabase.GUIDToAssetPath(assetGUID);
            var assetImporter = AssetImporter.GetAtPath(path);
            if (assetImporter.GetType().FullName != "UnityEditor.ShaderGraph.ShaderGraphImporter")
            {
                Debug.Log("Not a shader graph importer");
                continue;
            }

            var shaderGraphName = Path.GetFileNameWithoutExtension(path);

            var shaderGraphImporterAssembly = AppDomain.CurrentDomain.GetAssemblies().First(assembly =>
            {
                return assembly.GetType("UnityEditor.ShaderGraph.ShaderGraphImporterEditor") != null;
            });
            var shaderGraphImporterEditorType =
                shaderGraphImporterAssembly.GetType("UnityEditor.ShaderGraph.ShaderGraphImporterEditor");

            var getGraphDataMethod = shaderGraphImporterEditorType
                .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .First(info => { return info.Name.Contains("GetGraphData"); });

            var graphData = getGraphDataMethod.Invoke(null, new object[] {assetImporter});
            var generatorType = shaderGraphImporterAssembly.GetType("UnityEditor.ShaderGraph.Generator");
            var generatorConstructor = generatorType.GetConstructors().First();
            var generator = generatorConstructor.Invoke(new object[]
                {graphData, null, 1, $"Converted/{shaderGraphName}", null});

            var generatedShaderMethod = generator.GetType().GetMethod("get_generatedShader",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var generatedShader = generatedShaderMethod.Invoke(generator, new object[] { });
            //GUIUtility.systemCopyBuffer = (string) generatedShader;
            WriteShaderToFile(ConvertShader((string) generatedShader), shaderGraphName);
            break;
        }
    }

    static string ConvertShader(string shader)
    {
        return shader.Replace(PBRForwardPassInclude, CustomInclude);
    }

    static void WriteShaderToFile(string shader, string defaultFileName = "ConvertedShader")
    {
        var filePath = EditorUtility.SaveFilePanel("Converted Shader", "Assets", defaultFileName, "shader");
        if (!string.IsNullOrEmpty(filePath))
        {
            File.WriteAllText(filePath, shader);
            AssetDatabase.ImportAsset(filePath, ImportAssetOptions.Default);
            AssetDatabase.Refresh();
        }
    }
}