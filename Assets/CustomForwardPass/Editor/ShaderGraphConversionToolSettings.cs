using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[FilePath("Assets/CustomForwardPass/Editor/ShaderGraphConversionSettings.asset",
    FilePathAttribute.Location.ProjectFolder)]
public class ShaderGraphConversionToolSettings : ScriptableSingleton<ShaderGraphConversionToolSettings>,
    ISerializationCallbackReceiver
{
    readonly Dictionary<GUID, GUID> _shaderPairs = new Dictionary<GUID, GUID>();

    #region Serialization

    [SerializeField, HideInInspector] List<string> _shaderGraphGuids;
    [SerializeField, HideInInspector] List<string> _convertedGraphGuids;

    #endregion


    public void AddFilePair(string shaderGraphPath, string convertedShaderPath)
    {
        var shaderGraphGuid = new GUID(AssetDatabase.AssetPathToGUID(shaderGraphPath));
        var convertedPathGuid = new GUID(AssetDatabase.AssetPathToGUID(convertedShaderPath));

        if (_shaderPairs.ContainsKey(shaderGraphGuid))
        {
            _shaderPairs[shaderGraphGuid] = convertedPathGuid;
        }
        else
        {
            _shaderPairs.Add(shaderGraphGuid, convertedPathGuid);
        }

        Save(true);
    }

    public string GetConvertedShaderPath(string shaderGraphPath)
    {
        var shaderGraphGuid = new GUID(AssetDatabase.AssetPathToGUID(shaderGraphPath));
        if (_shaderPairs.TryGetValue(shaderGraphGuid, out GUID convertedShaderGuid))
        {
            return AssetDatabase.GUIDToAssetPath(convertedShaderGuid);
        }

        return null;
    }

    public void OnBeforeSerialize()
    {
        _shaderGraphGuids = new List<string>();
        _convertedGraphGuids = new List<string>();

        foreach (var shaderPair in _shaderPairs)
        {
            _shaderGraphGuids.Add(shaderPair.Key.ToString());
            _convertedGraphGuids.Add(shaderPair.Value.ToString());
        }
    }

    public void OnAfterDeserialize()
    {
        if (_shaderGraphGuids != null)
        {
            for (int i = 0; i < _shaderGraphGuids.Count; ++i)
            {
                var _shaderGraphGuid = new GUID(_shaderGraphGuids[i]);
                var _convertedGraphGuid = new GUID(_convertedGraphGuids[i]);

                if (_shaderPairs.ContainsKey(_shaderGraphGuid))
                {
                    _shaderPairs[_shaderGraphGuid] = _convertedGraphGuid;
                }
                else
                {
                    _shaderPairs.Add(_shaderGraphGuid, _convertedGraphGuid);
                }
            }
        }

        _shaderGraphGuids = null;
        _convertedGraphGuids = null;
    }
}