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
            var shaderGraphWindowAssembly = AppDomain.CurrentDomain.GetAssemblies().First(assembly =>
            {
                return assembly.GetType("UnityEditor.ShaderGraph.ShaderGraphImporterEditor") != null;
            });
            var shaderGraphImporterEditorType =
                shaderGraphWindowAssembly.GetType("UnityEditor.ShaderGraph.ShaderGraphImporterEditor");

            var getGraphDataMethod = shaderGraphImporterEditorType
                .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .First(info => { return info.Name.Contains("GetGraphData"); });

            var graphData = getGraphDataMethod.Invoke(null, new object[] {assetImporter});
            var generatorType = shaderGraphWindowAssembly.GetType("UnityEditor.ShaderGraph.Generator");
            var generatorConstructor = generatorType.GetConstructors().First();
            var generator = generatorConstructor.Invoke(new object[]
                {graphData, null, 1, "Custom Lighting Shader", null});

            var generatedShaderMethod = generator.GetType().GetMethod("get_generatedShader",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var generatedShader = generatedShaderMethod.Invoke(generator, new object[] { });
            GUIUtility.systemCopyBuffer = (string) generatedShader;
            ConvertShaderInBuffer();
            break;
        }
    }

#if false
    [MenuItem("Shader Tool/Create Custom Lighting Shader")]
    static void CreateCustomLightingShader()
    {
        var shaderGraphWindowAssembly = AppDomain.CurrentDomain.GetAssemblies().First(assembly =>
        {
            return assembly.GetType("UnityEditor.ShaderGraph.Drawing.MaterialGraphEditWindow") != null;
        });
        var shaderGraphWindowType =
            shaderGraphWindowAssembly.GetType("UnityEditor.ShaderGraph.Drawing.MaterialGraphEditWindow");

        if (shaderGraphWindowType != null)
        {
            var shaderGraphWindow = EditorWindow.GetWindow(shaderGraphWindowType);
            var graphView = shaderGraphWindow.rootVisualElement.Query("GraphView").First(); /* as MaterialGraphView */
            var graphViewType = graphView.GetType();
            var getSelectionProperty = graphViewType.GetProperty("GetSelection");

            var selection = getSelectionProperty.GetValue(graphView);
            var getCountMethod =
                selection.GetType().GetMethod("get_Count", BindingFlags.Instance | BindingFlags.Public);
            var getItemMethod = selection.GetType().GetMethod("get_Item", BindingFlags.Instance | BindingFlags.Public);

            var count = (int) getCountMethod.Invoke(selection, new object[] { });
            if (count > 0)
            {
                var firstItem = getItemMethod.Invoke(selection, new object[] {0});

                var getNodeMethod = firstItem.GetType().GetMethod("get_node",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var node = getNodeMethod.Invoke(firstItem, new object[] { });
                var graphDataMethod = node.GetType().GetMethod("get_owner",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var graphData = graphDataMethod.Invoke(node, new object[] { });
                var generatorType = shaderGraphWindowAssembly.GetType("UnityEditor.ShaderGraph.Generator");
                var generatorConstructor = generatorType.GetConstructors().First();
                var generator = generatorConstructor.Invoke(new object[]
                    {graphData, node, 1, "Custom Lighting Shader", null, true});

                var generatedShaderMethod = generator.GetType().GetMethod("get_generatedShader",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var generatedShader = generatedShaderMethod.Invoke(generator, new object[] { });
                GUIUtility.systemCopyBuffer = (string) generatedShader;
            }
        }
    }
#endif
}