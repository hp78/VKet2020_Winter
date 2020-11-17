using System.IO;
using UnityEditor;
using UnityEngine;
using VketBeta;

namespace VketBetaTools
{
    [InitializeOnLoad]
    public class VketBetaToolsLanguageInitializer : ScriptableObject
    {
        [SerializeField]
        TextAsset _messageCsv = null;

        static VketBetaToolsLanguageInitializer()
        {
            EditorApplication.delayCall += Initialize;
        }

        static void Initialize()
        {
            var initializer = CreateInstance<VketBetaToolsLanguageInitializer>();
            var messagesCsvPath = GetAssetFilePath(initializer._messageCsv);
            VketBetaGateway.SetMessages(messagesCsvPath);

            EditorApplication.delayCall -= Initialize;
        }

        static string GetAssetFilePath(Object obj)
        {
            return Path.GetFullPath(AssetDatabase.GetAssetPath(obj));
        }
    }
}