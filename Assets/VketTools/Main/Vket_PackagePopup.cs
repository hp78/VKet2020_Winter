using UnityEditor;
using UnityEngine;
using VitDeck.Main;
using VketTools.Utilities;

namespace VketTools.Main
{
    public class Vket_PackagePopup : PopupWindowContent
    {
        private static Utilities.Networking.NetworkUtility.Packages packages;
        private Vector2 scrollPos;

        public override void OnOpen()
        {
            packages = Utilities.Networking.NetworkUtility.GetPackage(AssetUtility.VersionInfoData.event_version, AssetUtility.VersionInfoData.package_type);
        }

        public override void OnClose()
        {
            packages = null;
        }

        public override void OnGUI(Rect rect)
        {
            var box = new GUIStyle(GUI.skin.box);
            var b1 = new GUIStyle(GUI.skin.button);
            var t1 = new GUIStyle(GUI.skin.label);
            float buttonHeight = 30;

            b1.fontSize = 12;
            t1.fontSize = 12;
            t1.alignment = TextAnchor.MiddleCenter;

            if (Event.current.type == EventType.Repaint)
            {
                EditorStyles.toolbar.Draw(new Rect(0, 0, EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight), false, true, true, false);
            }

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Packages", t1, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
            EditorGUILayout.EndHorizontal();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, box);
            {
                EditorGUILayout.Space();

                if (packages == null)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("- No packages -", t1);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    foreach (var package in packages.packages)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            var content = new GUIContent($"{package.package_name}-{package.package_version}");
                            if (GUILayout.Button(content, UIUtility.GetContentSizeFitStyle(content, b1, EditorGUIUtility.currentViewWidth), GUILayout.Height(buttonHeight)))
                            {
                                var downloader = new PackageDownloader();
                                downloader.Download(package.download_url, package.package_name);
                                downloader.Import(package.package_name);
                                downloader.Settlement(package.package_name);
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space();
                    }
                }
            }
            EditorGUILayout.EndScrollView();
        }
    }
}
