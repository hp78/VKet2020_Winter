using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VketTools.Utilities
{
    public static class AssetUtility
    {
        private static VersionInfo versionInfoData;
        private static LoginInfo loginInfoData;
        private static EditorPlayInfo editorPlayInfoData;
        private static LanguageInfo languageInfoData;
        private static SequenceInfo sequenceInfoData;

        public static T NullCast<T>(this T obj) where T : UnityEngine.Object
        {
            return obj != null ? obj : null;
        }

        public static VersionInfo VersionInfoData
        {
            get
            {
                if (versionInfoData == null)
                {
                    versionInfoData = AssetLoad<VersionInfo>(ConfigFolderPath + "/VersionInfo.asset");
                }
                return versionInfoData;
            }
        }

        public static LoginInfo LoginInfoData
        {
            get
            {
                if (loginInfoData == null)
                {
                    loginInfoData = AssetLoad<LoginInfo>(ConfigFolderPath + "/LoginInfo.asset");
                }
                return loginInfoData;
            }
        }

        public static EditorPlayInfo EditorPlayInfoData
        {
            get
            {
                if (editorPlayInfoData == null)
                {
                    editorPlayInfoData = AssetLoad<EditorPlayInfo>(ConfigFolderPath + "/EditorPlayInfo.asset");
                }
                return editorPlayInfoData;
            }
        }

        public static LanguageInfo LanguageInfoData
        {
            get
            {
                if (languageInfoData == null)
                {
                    languageInfoData = AssetLoad<LanguageInfo>(ConfigFolderPath + "/LanguageInfo.asset");
                }
                return languageInfoData;
            }
        }

        public static SequenceInfo SequenceInfoData
        {
            get
            {
                if (sequenceInfoData == null)
                {
                    sequenceInfoData = AssetLoad<SequenceInfo>(ConfigFolderPath + "/SequenceInfo.asset");
                }
                return sequenceInfoData;
            }
        }

        public static void SaveAsset<T>(T t) where T : UnityEngine.Object
        {
            EditorUtility.SetDirty(AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GetAssetPath(t)));
        }

        public static void SetHideFlags()
        {
            if (LoginInfoData != null && !((LoginInfoData.hideFlags & HideFlags.NotEditable) == HideFlags.NotEditable))
            {
                LoginInfoData.hideFlags |= HideFlags.NotEditable;
            }
            if (EditorPlayInfoData != null && !((EditorPlayInfoData.hideFlags & HideFlags.NotEditable) == HideFlags.NotEditable))
            {
                EditorPlayInfoData.hideFlags |= HideFlags.NotEditable;
            }
            if (VersionInfoData != null && !((VersionInfoData.hideFlags & HideFlags.NotEditable) == HideFlags.NotEditable))
            {
                VersionInfoData.hideFlags |= HideFlags.NotEditable;
            }
            if (LanguageInfoData != null && !((LanguageInfoData.hideFlags & HideFlags.NotEditable) == HideFlags.NotEditable))
            {
                LanguageInfoData.hideFlags |= HideFlags.NotEditable;
            }
            if (SequenceInfoData != null && !((SequenceInfoData.hideFlags & HideFlags.NotEditable) == HideFlags.NotEditable))
            {
                SequenceInfoData.hideFlags |= HideFlags.NotEditable;
            }
        }

        public static GameObject SSCapturePrefab
        {
            get
            {
                return AssetDatabase.LoadAssetAtPath<GameObject>(GetAssetFolderPath("Utilities/SS") + "/SSCapture.prefab");
            }
        }

        public static GameObject SSCameraPrefab
        {
            get
            {
                return AssetDatabase.LoadAssetAtPath<GameObject>(GetAssetFolderPath("Utilities/SS") + "/SSCamera.prefab");
            }
        }

        public static string MainFolderPath
        {
            get
            {
                return AssetDatabase.FindAssets("l:VketToolsMainFolder").Select(guid => AssetDatabase.GUIDToAssetPath(guid)).Where(p => p != null).FirstOrDefault();
            }
        }

        public static string ConfigFolderPath
        {
            get
            {
                return GetAssetFolderPath("Config");
            }
        }

        public static string ImageFolderPath
        {
            get
            {
                return GetAssetFolderPath("Image");
            }
        }

        public static string SSFolderPath
        {
            get
            {
                return GetFolderPath("Assets/Vket_SS");
            }
        }

        public static string MaterialFolderPath
        {
            get
            {
                return GetAssetFolderPath("Material");
            }
        }

        public static Texture2D GetTexture2D(string fileName)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>(ImageFolderPath + "/" + fileName);
        }

        public static Material GetMaterial(string fileName)
        {
            return AssetDatabase.LoadAssetAtPath<Material>(MaterialFolderPath + "/" + fileName);
        }

        public static T AssetLoad<T>(string assetPath) where T : ScriptableObject
        {
            if (!File.Exists(assetPath))
            {
                ScriptableObject scriptableObj = ScriptableObject.CreateInstance(typeof(T));
                AssetDatabase.CreateAsset(scriptableObj, assetPath);
            }
            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }

        public static string GetAssetFolderPath(string folder)
        {
            return GetFolderPath(MainFolderPath + "/" + folder);
        }

        private static string GetFolderPath(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }
            return path;
        }

        public static Texture2D GetIcon(string url)
        {
            var result = GetTexture2D("NoImage.png");
            try
            {
                if (!string.IsNullOrEmpty(url))
                {
                    return Networking.NetworkUtility.GetImage(url);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

            return result;
        }

        public static string SaveSS(Texture2D texture)
        {
            string path = SSFolderPath + "/{SHA-1}.png";
            SaveTexture2DToPNG(path, texture);
            if (!File.Exists(path))
                return null;

            string hash = "";
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                hash = BitConverter.ToString(SHA1.Create().ComputeHash(fs)).Replace("-", "").ToLower();
            string newPath = path.Replace("{SHA-1}", hash);
            if (!File.Exists(newPath))
                File.Move(path, newPath);
            else
                File.Delete(path);
            AssetDatabase.Refresh();
            return newPath;
        }

        public static void SaveTexture2DToPNG(string path, Texture2D texture)
        {
            try
            {
                File.WriteAllBytes(path, texture.EncodeToPNG());
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        public static float ForceRebuild()
        {
            string path = SceneManager.GetActiveScene().path;
            AssetBundleManifest result = null;
            try
            {
                string bundleName = "vketscene.vrcw";
                AssetImporter atPath = AssetImporter.GetAtPath(path);
                if (atPath == null)
                {
                    EditorUtility.DisplayDialog("Error", /* シーンファイルを開いてください。 */GetLabelString(50), "OK");
                    return -1;
                }

                string outPath = Application.temporaryCachePath;
                if (!Directory.Exists(outPath))
                {
                    Directory.CreateDirectory(outPath);
                }

                atPath.assetBundleName = bundleName;
                atPath.SaveAndReimport();
                result = BuildPipeline.BuildAssetBundles(outPath, BuildAssetBundleOptions.ForceRebuildAssetBundle, EditorUserBuildSettings.activeBuildTarget);
                atPath.assetBundleName = string.Empty;
                atPath.SaveAndReimport();
                AssetDatabase.RemoveUnusedAssetBundleNames();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return -1;
            }
            return result != null ? GetFileSize() : -1;
        }

        private static float GetFileSize()
        {
            try
            {
                string path = Application.temporaryCachePath + "/vketscene.vrcw";
                if (!File.Exists(path))
                {
                    return -1;
                }

                FileInfo fileInfo = new FileInfo(path);
                return fileInfo.Length / 1024f / 1024f;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return -1;
            }
        }

        public static void CheckPPSV2()
        {
            var path = "Packages/manifest.json";
            if (File.Exists(path))
            {
                var sr = new StreamReader(path, System.Text.Encoding.GetEncoding("shift_jis"));
                var text = sr.ReadToEnd();
                if (!text.Contains("\"com.unity.postprocessing\": \"2.3.0\","))
                {
                    text = text.Replace("{\n  \"dependencies\": {\n", "{\n  \"dependencies\": {\n    \"com.unity.postprocessing\": \"2.3.0\",\n");
                }
                sr.Close();
                var sw = new StreamWriter(path, false, System.Text.Encoding.GetEncoding("shift_jis"));
                sw.Write(text);
                sw.Close();
            }
            else
            {
                var sw = File.CreateText(path);
                var text = "{\n  \"dependencies\": {\n    \"com.unity.postprocessing\": \"2.3.0\"\n  }\n}";
                sw.Write(text);
                sw.Close();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void UpdateLanguage()
        {
            var result = Networking.NetworkUtility.GetLanguage(versionInfoData.event_version);
            if (result != null)
            {
                LanguageInfoData.languages = result;
            }
            else
            {
                Debug.LogError("言語ファイルのダウンロードに失敗しました。");
                Debug.LogError("Failed to download language file.");
            }
        }

        public static string TimestampToDateString(double timestamp)
        {
            var date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);
            var tzone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            if (Application.systemLanguage == SystemLanguage.Japanese)
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ja-JP");
                return TimeZoneInfo.ConvertTime(date, tzone).ToString("yyyy/MM/dd HH:mm");
            }
            else
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                return TimeZoneInfo.ConvertTime(date, tzone).ToString("MMM dd, yyyy HH:mm");
            }
        }

        public static string GetLabelString(int index)
        {
            if (LanguageInfoData.languages == null || LanguageInfoData.languages.Length == 0)
            {
                UpdateLanguage();
            }

            if (Application.systemLanguage == SystemLanguage.Japanese)
            {
                return LanguageInfoData.languages[index].jp.Replace("{n}", "\r\n");
            }
            else
            {
                return LanguageInfoData.languages[index].en.Replace("{n}", "\r\n");
            }
        }

        public static string GetWorldName()
        {
            var result = "";
            var data = loginInfoData.worldData;

            if (Application.systemLanguage == SystemLanguage.Japanese)
            {
                result = data.world_name;
            }
            else
            {
                result = data.world_name_en;
            }

            return result;
        }
    }
}