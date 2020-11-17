using System.IO;
using UnityEditor;
using VitDeck.Main;

namespace VketTools.Utilities
{
    public static class UpdateUtility
    {
        private static string releaseUrl = $"https://v-market.work/api/version_check.json";

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            var path = "Temp/VketToolsInitialization";
            if (!File.Exists(path))
            {
                File.Create(path);
                if (!Hiding.HidingUtil.DebugMode)
                {
                    UpdateCheck();
                    AssetUtility.CheckPPSV2();
                }
            }
        }

        public static bool UpdateCheck()
        {
            VersionInfo versionInfo = AssetUtility.VersionInfoData;
            if (versionInfo == null)
            {
                return false;
            }

            JsonReleaseInfo.FetchInfo($"{releaseUrl}?event_version={AssetUtility.VersionInfoData.event_version}&type={AssetUtility.VersionInfoData.package_type}");
            string latestVersion = JsonReleaseInfo.GetVersion();

            if (latestVersion != versionInfo.version)
            {
                if (!EditorUtility.DisplayDialog("Update", string.Format(AssetUtility.GetLabelString(56), latestVersion), AssetUtility.GetLabelString(15), AssetUtility.GetLabelString(16)))
                {
                    return false;
                }

                VitDeck.Main.UpdateCheck.UpdatePackage(latestVersion);
            }

            return true;
        }
    }
}