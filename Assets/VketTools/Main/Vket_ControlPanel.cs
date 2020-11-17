using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VitDeck.Main.ValidatedExporter;
using VitDeck.Validator;
using VketTools.Utilities;
using VketTools.Utilities.SS;

#if VRC_SDK_VRCSDK3
using VRC.SDK3.Editor;
using VRC.SDKBase;
using VRC.SDKBase.Editor;
#elif VRC_SDK_VRCSDK2
using VRCSDK2;
#endif

namespace VketTools.Main
{
    public class Vket_ControlPanel : EditorWindow
    {
        private static string hashKey;
        private static Texture2D circleIcon;
        private static Dictionary<EnumeratorType, IEnumerator> enumerators = new Dictionary<EnumeratorType, IEnumerator>();
        private bool loginModeFlg = false;

        public static bool IsLogin { get; private set; }

        private enum EnumeratorType
        {
            BoothCheck,
            BuildSizeCheck,
            SetPassCheck,
            EditorPlay,
            Submission,
            Export
        }

        private enum MaxSizeType
        {
            SetPass,
            Batches,
            Build
        }

        [MenuItem("VketTools/Control Panel")]
        public static void ShowControlPanelWindow()
        {
            var window = GetWindow<Vket_ControlPanel>(false, "Control Panel - VketTools -");
            window.minSize = new Vector2(230, 385);
            window.Show();
        }

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Split(';').ToList();
            if (!Utilities.Hiding.HidingUtil.DebugMode)
            {
                if (!symbols.Contains("VITDECK_HIDE_MENUITEM"))
                {
                    symbols.Add("VITDECK_HIDE_MENUITEM");
                }
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, string.Join(";", symbols));

            EditorApplication.playModeStateChanged += Vket_OnChangedPlayMode;
            AssetUtility.SetHideFlags();

            LoginInfo info = AssetUtility.LoginInfoData;
            if (IsLogin && !CircleNullOrEmptyCheck())
            {
                VitDeck.Utilities.UserSettings userSettings = VitDeck.Utilities.UserSettingUtility.GetUserSettings();
                if (string.IsNullOrEmpty(userSettings.validatorRuleSetType))
                {
                    userSettings.validatorFolderPath = "Assets/" + info.data.circle.id;
                    userSettings.validatorRuleSetType = GetValidatorRule();
                }
            }

            if (!EditorApplication.isPlaying && info != null && info.data != null && !string.IsNullOrEmpty(info.hashKey))
            {
                Login(info.hashKey);
            }

            EditorSceneManager.sceneOpened += OnSceneOpened;
            DrawBoundsLimitGizmos(SceneManager.GetActiveScene());

            AssetUtility.UpdateLanguage();
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            DrawBoundsLimitGizmos(scene);
        }

        private static void DrawBoundsLimitGizmos(Scene scene)
        {
            var info = AssetUtility.LoginInfoData;
            var userSettings = VitDeck.Utilities.UserSettingUtility.GetUserSettings();
            if (info == null || info.data == null || info.data.circle == null || SceneManager.GetActiveScene() != scene || scene.name != info.data.circle.id.ToString() || Path.GetDirectoryName(scene.path).Replace("/", @"\") != userSettings.validatorFolderPath.Replace("/", @"\"))
            {
                return;
            }

            var ruleSets = Validator.GetRuleSets().Where(a => a.GetType().Name == userSettings.validatorRuleSetType);
            if (ruleSets.Count() > 0)
            {
                var selectedRuleSet = ruleSets.First();
                var baseFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(userSettings.validatorFolderPath);
                var target = selectedRuleSet.TargetFinder.Find(AssetDatabase.GetAssetPath(baseFolder), false);
                var type = typeof(BoothBoundsRule);
                foreach (var rule in selectedRuleSet.GetRules())
                {
                    if (rule.GetType() == type)
                    {
                        ReflectionUtility.InvokeMethod(type, "Logic", rule, new object[] { target });
                    }
                }
            }
        }

        private void OnEnable()
        {
            LoginInfo info = AssetUtility.LoginInfoData;
            if (info == null || info.data == null || string.IsNullOrEmpty(info.hashKey))
            {
                IsLogin = false;
                return;
            }

            if (!IsLogin)
            {
                string key = info.hashKey;
                if (!string.IsNullOrEmpty(key))
                {
                    hashKey = key;
                    IsLogin = true;
                }
            }
        }

        private void OnGUI()
        {
            if (AssetUtility.LoginInfoData == null || AssetUtility.LoginInfoData.data == null || string.IsNullOrEmpty(AssetUtility.LoginInfoData.hashKey))
            {
                IsLogin = false;
            }

            AssetUtility.SetHideFlags();

            GUILayout.Space(5);

            if (IsLogin)
            {
                ControlPanelWindow();
            }
            else
            {
                LoginWindow();
            }

            GUILayout.Space(5);
        }

        private void Update()
        {
            var tmp = new Dictionary<EnumeratorType, IEnumerator>(enumerators);
            foreach (var pair in tmp)
            {
                if (pair.Value != null)
                {
                    pair.Value.MoveNext();
                }
            }
        }

        private void LoginWindow()
        {
            GUIStyle box = new GUIStyle(GUI.skin.box);
            GUIStyle l1 = new GUIStyle(GUI.skin.label);
            GUIStyle l2 = new GUIStyle(GUI.skin.label);

            l1.fontSize = 25;
            l1.fixedHeight = 30;
            l2.fontSize = 15;
            l2.fixedHeight = 23;

            float controlWidth = position.width - position.width / 3;

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                GUIContent content = new GUIContent(/* ログイン */AssetUtility.GetLabelString(12));
                EditorGUILayout.LabelField(content, l1, GUILayout.Width(l1.CalcSize(content).x));
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(30);

            EditorGUILayout.BeginVertical(box);
            {
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUIContent content = new GUIContent(!loginModeFlg ? /* Vketβでログイン */AssetUtility.GetLabelString(13) : /* ログインキーでログイン */AssetUtility.GetLabelString(14));
                    EditorGUILayout.LabelField(content, l2, GUILayout.Width(l2.CalcSize(content).x));
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(30);

                if (loginModeFlg)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        hashKey = EditorGUILayout.PasswordField(hashKey, GUILayout.Width(controlWidth));
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(30);
                }

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button(/* ログイン */AssetUtility.GetLabelString(12), GUILayout.Width(controlWidth), GUILayout.Height(30)))
                    {
                        if (!loginModeFlg)
                        {
                            VketBeta.VketBetaGateway.Login();
                            hashKey = VketBeta.VketBetaGateway.GetUserHash();
                        }

                        Login(hashKey);
                    }

                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                GUIStyle l3 = new GUIStyle(GUI.skin.label);
                l3.fontSize = 11;
                l3.fixedHeight = 16;

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUIContent content = new GUIContent(/* ログインキーでログイン */AssetUtility.GetLabelString(14));
                    loginModeFlg = EditorGUILayout.ToggleLeft(content, loginModeFlg, l3, GUILayout.Width(l3.CalcSize(content).x + 15));
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    UIUtility.EditorGUILink("https://www.v-market.work/", l3);
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndVertical();
        }

        private void ControlPanelWindow()
        {
            LoginInfo info = AssetUtility.LoginInfoData;
            circleIcon = circleIcon.NullCast() ?? AssetUtility.GetIcon(info.data.user_icon);

            GUIStyle box = new GUIStyle(GUI.skin.box);
            GUIStyle l2 = new GUIStyle(GUI.skin.label);
            GUIStyle l3 = new GUIStyle(GUI.skin.label);
            GUIStyle b1 = new GUIStyle(GUI.skin.button);
            GUIStyle b2 = new GUIStyle(GUI.skin.button);

            EditorGUILayout.BeginVertical(box);
            {
                float minSize = GetWindowMinSize(position.width, position.height - 54);
                float buttonHeight1 = 35;
                float buttonHeight2 = 30;

                l2.alignment = TextAnchor.MiddleCenter;
                l2.fontSize = 15;
                l2.fixedHeight = 25;
                l2.padding = new RectOffset(0, 0, 0, 0);
                l3.alignment = TextAnchor.MiddleCenter;
                l3.fontSize = 11;
                l3.fixedHeight = 16;
                l3.padding = new RectOffset(0, 0, 0, 0);

                b1.fontSize = 12;
                b2.fontSize = 12;

                float labelAndButtonsHeight = l2.fixedHeight * 3 + l3.fixedHeight * 3 + buttonHeight2 * 2 + 10;
                float thumbnailSize = minSize - labelAndButtonsHeight;
                float toolButtonSize = position.width - thumbnailSize - 40;

                bool controlButtonFlag = position.height - 54 - thumbnailSize - labelAndButtonsHeight - buttonHeight1 * 4 - 10 - 10 > buttonHeight1 * 4;

                if (controlButtonFlag)
                {
                    thumbnailSize += toolButtonSize;
                }

                EditorGUILayout.BeginHorizontal();
                {
                    var content1 = new GUIContent($"{/* サークル名 */AssetUtility.GetLabelString(7)}:");
                    var content2 = new GUIContent(info.data.circle.name, info.data.circle.name);
                    var content3 = new GUIContent(/* ダッシュボード */AssetUtility.GetLabelString(82));
                    var width = position.width - 10 - 10 - l2.CalcSize(content1).x - b1.CalcSize(content3).x;
                    var style = UIUtility.GetContentSizeFitStyle(content2, l2, width);
                    EditorGUILayout.LabelField(content1, l2, GUILayout.Width(l2.CalcSize(content1).x), GUILayout.Height(l2.CalcSize(content1).y));
                    EditorGUILayout.LabelField(content2, style, GUILayout.Width(style.CalcSize(content2).x), GUILayout.Height(style.CalcSize(content2).y));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(content3, b1, GUILayout.Width(b1.CalcSize(content3).x), GUILayout.Height(b1.CalcSize(content3).y)))
                    {
                        Application.OpenURL($"https://www.v-market.work/v{AssetUtility.VersionInfoData.event_version}/user/dashboard");
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.DrawPreviewTexture(new Rect(18.4f, 5 + l2.fixedHeight, thumbnailSize, thumbnailSize), circleIcon);

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (!controlButtonFlag)
                    {
                        DrawControlButtons(b1, toolButtonSize, buttonHeight1);
                        EditorGUILayout.BeginVertical();
                        {
                            GUILayout.Space(thumbnailSize);
                        }
                        EditorGUILayout.EndVertical();
                    }
                    else
                    {
                        EditorGUILayout.BeginVertical();
                        {
                            GUILayout.Space(thumbnailSize);
                        }
                        EditorGUILayout.EndVertical();
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    var circlePos = info.data.is_owner ? /* 代表 */AssetUtility.GetLabelString(72) : /* メンバー */AssetUtility.GetLabelString(73);
                    GUIContent content = new GUIContent($"{/* 名前 */AssetUtility.GetLabelString(8)}:{info.data.name}({circlePos})", $"{info.data.name}({circlePos})");
                    EditorGUILayout.LabelField(content, l2, GUILayout.Width(l2.CalcSize(content).x > position.width - 10 ? position.width - 10 : l2.CalcSize(content).x), GUILayout.Height(l2.CalcSize(content).y));
                }
                EditorGUILayout.EndHorizontal();

                if (!CircleNullOrEmptyCheck())
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        var content1 = new GUIContent($"{/* 配置ワールド */AssetUtility.GetLabelString(9)}:{AssetUtility.GetWorldName()}", AssetUtility.GetWorldName());
                        var content2 = new GUIContent(/* 入稿ルール */AssetUtility.GetLabelString(74));
                        var style = UIUtility.GetContentSizeFitStyle(content1, l3, position.width - 10 - b1.CalcSize(content2).x - 20);
                        EditorGUILayout.LabelField(content1, style, GUILayout.Width(style.CalcSize(content1).x > position.width - 10 ? position.width - 10 : style.CalcSize(content1).x), GUILayout.Height(style.CalcSize(content1).y));
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(content2, b1, GUILayout.Width(b1.CalcSize(content2).x), GUILayout.Height(b1.CalcSize(content2).y)))
                        {
                            Application.OpenURL(GetRuleURL());
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        GUIContent content = new GUIContent($"{/* VirtualMarket */AssetUtility.GetLabelString(11)}{info.data.circle.event_version}");
                        EditorGUILayout.LabelField(content, l2, GUILayout.Width(l2.CalcSize(content).x), GUILayout.Height(l2.CalcSize(content).y));
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        GUIContent content = new GUIContent($"{/* 入稿期限(JST) */AssetUtility.GetLabelString(78)}:");
                        EditorGUILayout.LabelField(content, l3, GUILayout.Width(l3.CalcSize(content).x), GUILayout.Height(l3.CalcSize(content).y));
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(5);
                        var s1 = /* 一次 */AssetUtility.GetLabelString(79);
                        var s2 = /* 二次 */AssetUtility.GetLabelString(80);
                        var s3 = /* 最終 */AssetUtility.GetLabelString(81);
                        var d1s = AssetUtility.TimestampToDateString(info.submissionTerm.once_time.start);
                        var d2s = AssetUtility.TimestampToDateString(info.submissionTerm.second_time.start);
                        var d3s = AssetUtility.TimestampToDateString(info.submissionTerm.last_time.start);
                        var d1e = AssetUtility.TimestampToDateString(info.submissionTerm.once_time.end);
                        var d2e = AssetUtility.TimestampToDateString(info.submissionTerm.second_time.end);
                        var d3e = AssetUtility.TimestampToDateString(info.submissionTerm.last_time.end);

                        var dateStyle = UIUtility.GetContentSizeFitStyle(new GUIContent($"{s1} {d1s}-,{s2} {d2s}-,{s3} {d3s}"), l3, position.width - 70);
                        GUIContent content = new GUIContent($"{s1} ");
                        EditorGUILayout.LabelField(content, dateStyle, GUILayout.Width(dateStyle.CalcSize(content).x), GUILayout.Height(dateStyle.CalcSize(content).y));

                        EditorGUILayout.BeginVertical();
                        {
                            var tooltip = $"{s1} {d1s} - {d1e}";
                            content = new GUIContent($"{d1s}-,", tooltip);
                            EditorGUILayout.LabelField(content, dateStyle, GUILayout.Width(dateStyle.CalcSize(content).x), GUILayout.Height(dateStyle.CalcSize(content).y));
                            content = new GUIContent($"{d1e}  ", tooltip);
                            EditorGUILayout.LabelField(content, dateStyle, GUILayout.Width(dateStyle.CalcSize(content).x), GUILayout.Height(dateStyle.CalcSize(content).y));
                        }
                        EditorGUILayout.EndVertical();

                        content = new GUIContent($"{s2} ");
                        EditorGUILayout.LabelField(content, dateStyle, GUILayout.Width(dateStyle.CalcSize(content).x), GUILayout.Height(dateStyle.CalcSize(content).y));

                        EditorGUILayout.BeginVertical();
                        {
                            var tooltip = $"{s2} {d2s} - {d2e}";
                            content = new GUIContent($"{d2s}-,", tooltip);
                            EditorGUILayout.LabelField(content, dateStyle, GUILayout.Width(dateStyle.CalcSize(content).x), GUILayout.Height(dateStyle.CalcSize(content).y));
                            content = new GUIContent($"{d2e}  ", tooltip);
                            EditorGUILayout.LabelField(content, dateStyle, GUILayout.Width(dateStyle.CalcSize(content).x), GUILayout.Height(dateStyle.CalcSize(content).y));
                        }
                        EditorGUILayout.EndVertical();

                        content = new GUIContent($"{s3} ");
                        EditorGUILayout.LabelField(content, dateStyle, GUILayout.Width(dateStyle.CalcSize(content).x), GUILayout.Height(dateStyle.CalcSize(content).y));

                        EditorGUILayout.BeginVertical();
                        {
                            var tooltip = $"{s3} {d3s} - {d3e}";
                            content = new GUIContent($"{d3s}-,", tooltip);
                            EditorGUILayout.LabelField(content, dateStyle, GUILayout.Width(dateStyle.CalcSize(content).x), GUILayout.Height(dateStyle.CalcSize(content).y));
                            content = new GUIContent($"{d3e}  ", tooltip);
                            EditorGUILayout.LabelField(content, dateStyle, GUILayout.Width(dateStyle.CalcSize(content).x), GUILayout.Height(dateStyle.CalcSize(content).y));
                        }
                        EditorGUILayout.EndVertical();

                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(10);
                }

                if (controlButtonFlag)
                {
                    DrawControlButtons(b1, position.width - 15, buttonHeight1);
                }

                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginHorizontal();
                {
                    GUIContent content = new GUIContent(/* ログアウト */AssetUtility.GetLabelString(5));
                    if (GUILayout.Button(content, UIUtility.GetContentSizeFitStyle(content, b2, position.width * 0.7f - 10), GUILayout.Width(position.width * 0.7f - 10), GUILayout.Height(buttonHeight2)))
                    {
                        Logout();
                    }

                    GUILayout.FlexibleSpace();

                    content = new GUIContent("Packages");
                    if (GUILayout.Button(content, UIUtility.GetContentSizeFitStyle(content, b2, position.width * 0.3f - 10), GUILayout.Width(position.width * 0.3f - 10), GUILayout.Height(buttonHeight2)))
                    {
                        PopupWindow.Show(new Rect(Event.current.mousePosition, Vector2.one), new Vket_PackagePopup());
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginDisabledGroup(CircleNullOrEmptyCheck());
                    {
                        if (GUILayout.Button(/* 入稿 */AssetUtility.GetLabelString(6), b2, GUILayout.Height(buttonHeight2)))
                        {
                            DraftButton_Click();
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2);
            }
            EditorGUILayout.EndVertical();

            float GetWindowMinSize(float width, float height) => width < height ? width : height;
        }

        private void DrawControlButtons(GUIStyle style, float width, float height)
        {
            EditorGUILayout.BeginVertical();
            {
                GUILayout.Space(-5);

                GUIContent content = new GUIContent(/* 入稿用シーン作成 */AssetUtility.GetLabelString(0));
                if (GUILayout.Button(content, UIUtility.GetContentSizeFitStyle(content, style, width), GUILayout.Width(width), GUILayout.Height(height)))
                {
                    LoadTemplateButton_Click();
                }

                content = new GUIContent(/* ブースチェック */AssetUtility.GetLabelString(1));
                if (GUILayout.Button(content, UIUtility.GetContentSizeFitStyle(content, style, width), GUILayout.Width(width), GUILayout.Height(height)))
                {
                    BoothCheckButton_Click();
                }

                content = new GUIContent(/* 容量チェック */AssetUtility.GetLabelString(2));
                if (GUILayout.Button(content, UIUtility.GetContentSizeFitStyle(content, style, width), GUILayout.Width(width), GUILayout.Height(height)))
                {
                    BuildSizeCheckButton_Click();
                }

                content = new GUIContent(/* SetPassチェック */AssetUtility.GetLabelString(3));
                if (GUILayout.Button(content, UIUtility.GetContentSizeFitStyle(content, style, width), GUILayout.Width(width), GUILayout.Height(height)))
                {
                    SetPassCheckButton_Click();
                }

                GUILayout.Space(-5);
            }
            EditorGUILayout.EndVertical();
        }

        private static void LoadTemplateButton_Click()
        {
            Login(hashKey);

            if (EditorPlayCheck() || !UpdateCheck() || !AuthCheck())
            {
                return;
            }

            Type type = typeof(VitDeck.TemplateLoader.GUI.TemplateLoaderWindow);
            ReflectionUtility.InvokeMethod(type, "Open", null, null);
            LoginInfo info = AssetUtility.LoginInfoData;
            var templateIndex = (info.data.circle.event_version == 5 && info.worldData.world_num == 8) ? 6 : info.worldData.world_num - 1; // UdonCube English の例外処理
            if (!CircleNullOrEmptyCheck())
            {
                ReflectionUtility.SetField(type, "popupIndex", null, templateIndex);
                ReflectionUtility.SetField(type, "templateProperty", null, Assembly.Load("VitDeck.TemplateLoader").GetType("VitDeck.TemplateLoader.TemplateLoader").GetMethod("GetTemplateProperty", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { ((string[])ReflectionUtility.GetField(type, "templateFolders", null))[templateIndex] }));
                ReflectionUtility.SetField(type, "replaceStringList", null, new Dictionary<string, string> { { "CIRCLEID", info.data.circle.id.ToString() } });
            }
        }

        private static void BoothCheckButton_Click()
        {
            Login(hashKey);

            if (EditorPlayCheck() || !UpdateCheck() || !AuthCheck())
            {
                return;
            }

            enumerators[EnumeratorType.BoothCheck] = BoothCheckFunc();
        }

        private static void BuildSizeCheckButton_Click()
        {
            if (EditorPlayCheck() || !UpdateCheck() || !AuthCheck())
            {
                return;
            }

            enumerators[EnumeratorType.BuildSizeCheck] = BuildSizeCheckFunc();
        }

        private static void SetPassCheckButton_Click()
        {
            Login(hashKey);

            if (EditorPlayCheck() || !UpdateCheck() || !AuthCheck())
            {
                return;
            }

            enumerators[EnumeratorType.SetPassCheck] = SetPassCheckFunc();
        }

        private static void DraftButton_Click()
        {
            Login(hashKey);

            if (EditorPlayCheck() || !UpdateCheck() || !AuthCheck())
            {
                return;
            }

            if (!Utilities.Hiding.HidingUtil.DebugMode && !Utilities.Networking.NetworkUtility.TermCheck(AssetUtility.VersionInfoData.event_version))
            {
                EditorUtility.DisplayDialog("Error", /* 入稿期間外です。 */AssetUtility.GetLabelString(28), "OK");
                return;
            }

            if (!AssetUtility.LoginInfoData.data.is_owner)
            {
                if (!EditorUtility.DisplayDialog("Error", /* 入稿は代表者のみが可能です。{n}入稿されませんが手順の確認はできます。続行しますか？ */AssetUtility.GetLabelString(70), "Yes", "Cancel"))
                {
                    return;
                }
            }

            enumerators[EnumeratorType.Submission] = DraftFunc();
        }

        private static IEnumerator BoothCheckFunc()
        {
            IEnumerator bakeCheck = BakeCheckAndRun(false);
            while (bakeCheck.MoveNext())
            {
                yield return null;
            }
            if (!(bool)bakeCheck.Current)
            {
                EditorUtility.DisplayDialog("Error", /* ブースチェックを中断しました。 */AssetUtility.GetLabelString(19), "OK");
                yield break;
            }

            LoginInfo info = AssetUtility.LoginInfoData;

            var userSettings = VitDeck.Utilities.UserSettingUtility.GetUserSettings();
            userSettings.validatorRuleSetType = GetValidatorRule();
            Type validatorWindowType = typeof(VitDeck.Validator.GUI.ValidatorWindow);
            ReflectionUtility.InvokeMethod(validatorWindowType, "Open", null, null);
            object window = ReflectionUtility.GetField(validatorWindowType, "window", null);
            if (!CircleNullOrEmptyCheck())
            {
                userSettings.validatorFolderPath = "Assets/" + info.data.circle.id;
            }
            VitDeck.Utilities.UserSettingUtility.SaveUserSettings(userSettings);

            if (string.IsNullOrEmpty(userSettings.validatorFolderPath) || (!string.IsNullOrEmpty(userSettings.validatorFolderPath) && !Directory.Exists(userSettings.validatorFolderPath)))
            {
                if (!EditorUtility.DisplayDialog("Warning", /* 入稿フォルダが見つかりませんでした。\r\n現在開いているシーンをチェックしますか？ */AssetUtility.GetLabelString(20), /* はい */AssetUtility.GetLabelString(15), /* いいえ */AssetUtility.GetLabelString(16)))
                {
                    yield break;
                }

                string baseFolderPath = SceneManager.GetActiveScene().path;
                if (string.IsNullOrEmpty(baseFolderPath))
                {
                    EditorUtility.DisplayDialog("Error", /* シーンが見つかりませんでした。 */AssetUtility.GetLabelString(21), "OK");
                    yield break;
                }
                ReflectionUtility.SetField(validatorWindowType, "baseFolder", window, AssetDatabase.LoadAssetAtPath<DefaultAsset>(Path.GetDirectoryName(baseFolderPath)));
            }

            ReflectionUtility.InvokeMethod(validatorWindowType, "OnValidate", window, null);

            enumerators[EnumeratorType.BoothCheck] = null;
            yield return null;
        }

        private static IEnumerator BuildSizeCheckFunc()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.isDirty)
            {
                if (!EditorUtility.DisplayDialog("Warning", /* シーンが変更されています。\r\n保存して実行しますか？ */AssetUtility.GetLabelString(22), /* はい */AssetUtility.GetLabelString(15), /* いいえ */AssetUtility.GetLabelString(16)))
                {
                    yield break;
                }
            }

            EditorSceneManager.SaveScene(scene);

            IEnumerator bakeCheck = BakeCheckAndRun(false);
            while (bakeCheck.MoveNext())
            {
                yield return null;
            }
            if (!(bool)bakeCheck.Current)
            {
                EditorUtility.DisplayDialog("Error", /* 容量チェックを中断しました。 */AssetUtility.GetLabelString(23), "OK");
                yield break;
            }

            LoginInfo info = AssetUtility.LoginInfoData;
            string rootObjectName = CircleNullOrEmptyCheck() ? scene.name : info.data.circle.id.ToString();

            foreach (var obj in Array.FindAll(Resources.FindObjectsOfTypeAll<GameObject>(), (item) => item.transform.parent == null))
            {
                if (obj.name != rootObjectName && AssetDatabase.GetAssetOrScenePath(obj).Contains(".unity"))
                {
                    DestroyImmediate(obj);
                }
            }

            float cmpSize = AssetUtility.ForceRebuild();
            EditorSceneManager.OpenScene(SceneManager.GetActiveScene().path);
            if (BuildSizeCheck(cmpSize))
            {
                if (EditorUtility.DisplayDialog("Error", $"{/* 容量がオーバーしています。 */AssetUtility.GetLabelString(24)}\r\n{cmpSize}MB\r\nRegulation {GetMaxSize(MaxSizeType.Build)}MB", /*入稿ルール*/AssetUtility.GetLabelString(74), "OK"))
                {
                    Application.OpenURL(GetRuleURL());
                }
                GUIUtility.ExitGUI();
                yield break;
            }
            else if (cmpSize == -1)
            {
                EditorUtility.DisplayDialog("Error", /* ビルドに失敗しました。 */AssetUtility.GetLabelString(25), "OK");
            }
            else
            {
                if (EditorUtility.DisplayDialog("Build", $"{/* ビルド完了！ */AssetUtility.GetLabelString(26)}\r\nCompressed Size: {cmpSize}MB\r\nRegulation {GetMaxSize(MaxSizeType.Build)}MB", /*入稿ルール*/AssetUtility.GetLabelString(74), "OK"))
                {
                    Application.OpenURL(GetRuleURL());
                }
            }

            enumerators[EnumeratorType.BuildSizeCheck] = null;
            yield return null;
        }

        private static IEnumerator SetPassCheckFunc()
        {
            IEnumerator bakeCheck = BakeCheckAndRun(false);
            while (bakeCheck.MoveNext())
            {
                yield return null;
            }

            if ((bool)bakeCheck.Current)
            {
                ShowControlPanelWindow();
                EditorUtility.DisplayProgressBar("SetPassCalls and Batches Check.", "Start up...", 0);
                AssetUtility.EditorPlayInfoData.isVketEditorPlay = true;
                AssetUtility.EditorPlayInfoData.isSetPassCheckOnly = true;
                AssetUtility.SaveAsset(AssetUtility.EditorPlayInfoData);
                EditorApplication.isPlaying = true;
            }
            else
            {
                EditorUtility.DisplayDialog("Error", /* SetPassCallsのチェックを中断しました。 */AssetUtility.GetLabelString(27), "OK");
            }

            enumerators[EnumeratorType.SetPassCheck] = null;
            yield return null;
        }

        private static IEnumerator DraftFunc()
        {
            LoginInfo info = AssetUtility.LoginInfoData;
            Scene scene = SceneManager.GetActiveScene();
            string rootObjectName = CircleNullOrEmptyCheck() ? scene.name : info.data.circle.id.ToString();

            Vket_SequenceWindow.ShowSequenceWindow();
            AssetUtility.SequenceInfoData.sequenceStatus[(int)Vket_SequenceWindow.Sequence.BakeCheck].status = 1;
            Vket_SequenceWindow.Window.Repaint();
            {
                double timeSinceStartup = EditorApplication.timeSinceStartup;
                while (EditorApplication.timeSinceStartup - timeSinceStartup < 1)
                {
                    yield return null;
                }
            }

            IEnumerator bakeCheck = BakeCheckAndRun(true);
            while (bakeCheck.MoveNext())
            {
                yield return null;
            }
            if (!(bool)bakeCheck.Current && EditorUtility.DisplayDialog("Error", /* 入稿を中断しました。 */AssetUtility.GetLabelString(29), "OK"))
            {
                Debug.Log(Vket_SequenceWindow.GetResultLog());
                Vket_SequenceWindow.Window.Close();
                enumerators[EnumeratorType.Submission] = null;
                yield break;
            }

            AssetUtility.SequenceInfoData.sequenceStatus[(int)Vket_SequenceWindow.Sequence.BakeCheck].status = 2;
            Vket_SequenceWindow.Window.Repaint();
            yield return null;

            AssetUtility.SequenceInfoData.sequenceStatus[(int)Vket_SequenceWindow.Sequence.VRChatCheck].status = 1;
            Vket_SequenceWindow.Window.Repaint();
            yield return null;

            if (info.vrcLaunchFlag)
            {
                var objList = new Dictionary<string, bool>();
                if (info.data.circle != null)
                {
                    foreach (GameObject obj in Array.FindAll(FindObjectsOfType<GameObject>(), (item) => item.transform.parent == null))
                    {
                        if (obj.name != rootObjectName && obj.name != "Reference Object")
                        {
                            objList.Add(obj.name, obj.activeSelf);
                            obj.SetActive(false);
                        }
                    }
                }

                if (!VRC_LocalTestLaunch())
                {
                    Debug.Log(Vket_SequenceWindow.GetResultLog());
                    Vket_SequenceWindow.Window.Close();
                    enumerators[EnumeratorType.Submission] = null;
                    yield break;
                }
                if (!EditorUtility.DisplayDialog("VRChat Client Check.", /* VRChat内での確認を待っています。 */AssetUtility.GetLabelString(30), /* 確認しました */AssetUtility.GetLabelString(18), /* キャンセル */AssetUtility.GetLabelString(17)))
                {
                    Debug.Log(Vket_SequenceWindow.GetResultLog());
                    Vket_SequenceWindow.Window.Close();
                    enumerators[EnumeratorType.Submission] = null;
                    yield break;
                }

                foreach (var obj in Array.FindAll(Resources.FindObjectsOfTypeAll<GameObject>(), (item) => item.transform.parent == null))
                {
                    if (obj.name != rootObjectName && obj.name != "Reference Object" && AssetDatabase.GetAssetOrScenePath(obj).Contains(".unity"))
                    {
                        foreach (var pair in objList)
                        {
                            if (obj.name == pair.Key)
                            {
                                obj.SetActive(pair.Value);
                                break;
                            }
                        }
                    }
                }
            }

            AssetUtility.SequenceInfoData.sequenceStatus[(int)Vket_SequenceWindow.Sequence.VRChatCheck].status = 2;
            Vket_SequenceWindow.Window.Repaint();
            yield return null;

            AssetUtility.EditorPlayInfoData.buildSizeSuccessFlag = false;
            AssetUtility.EditorPlayInfoData.setPassSuccessFlag = false;
            AssetUtility.EditorPlayInfoData.ssSuccessFlag = false;
            AssetUtility.EditorPlayInfoData.forceExportFlag = false;
            AssetUtility.SaveAsset(AssetUtility.EditorPlayInfoData);

            AssetUtility.SequenceInfoData.sequenceStatus[(int)Vket_SequenceWindow.Sequence.RuleCheck].status = 1;
            Vket_SequenceWindow.Window.Repaint();
            {
                double timeSinceStartup = EditorApplication.timeSinceStartup;
                while (EditorApplication.timeSinceStartup - timeSinceStartup < 1)
                {
                    yield return null;
                }
            }

            bakeCheck = BakeCheckAndRun(false);
            while (bakeCheck.MoveNext())
            {
                yield return null;
            }
            if (!(bool)bakeCheck.Current && EditorUtility.DisplayDialog("Error", /* 入稿を中断しました。 */AssetUtility.GetLabelString(29), "OK"))
            {
                Debug.Log(Vket_SequenceWindow.GetResultLog());
                Vket_SequenceWindow.Window.Close();
                enumerators[EnumeratorType.Submission] = null;
                yield break;
            }

            VitDeck.Utilities.UserSettings userSettings = VitDeck.Utilities.UserSettingUtility.GetUserSettings();
            userSettings.validatorRuleSetType = GetValidatorRule();
            VitDeck.Utilities.UserSettingUtility.SaveUserSettings(userSettings);
            DefaultAsset baseFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(userSettings.validatorFolderPath);
            string baseFolderPath = AssetDatabase.GetAssetPath(baseFolder);
            var results = Validator.Validate(Validator.GetRuleSet(userSettings.validatorRuleSetType), baseFolderPath, false);

            AssetDatabase.Refresh();

            if (results == null)
            {
                Debug.LogError(/* ルールチェックが正常に終了しませんでした。 */AssetUtility.GetLabelString(31));
                Debug.Log(Vket_SequenceWindow.GetResultLog());
                Vket_SequenceWindow.Window.Close();
                enumerators[EnumeratorType.Submission] = null;
                yield break;
            }

            bool errorFlag = results.Where(w => w.Issues.Where(w2 => w2.level == VitDeck.Validator.IssueLevel.Error).Any()).Any();

            if (errorFlag)
            {
                Type validatorWindowType = typeof(VitDeck.Validator.GUI.ValidatorWindow);
                ReflectionUtility.InvokeMethod(validatorWindowType, "Open", null, null);
                object window = ReflectionUtility.GetField(validatorWindowType, "window", null);
                SetValidateLog(validatorWindowType, window, results, baseFolderPath, userSettings.validatorRuleSetType);
                EditorUtility.DisplayDialog("Error", /* ルールチェックで問題が発見されました。 */AssetUtility.GetLabelString(32), "OK");
                Debug.Log(Vket_SequenceWindow.GetResultLog());
                Vket_SequenceWindow.Window.Close();
                enumerators[EnumeratorType.Submission] = null;
                yield break;
            }

            AssetUtility.SequenceInfoData.sequenceStatus[(int)Vket_SequenceWindow.Sequence.RuleCheck].status = 2;
            Vket_SequenceWindow.Window.Repaint();
            yield return null;

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

            foreach (var obj in Array.FindAll(Resources.FindObjectsOfTypeAll<GameObject>(), (item) => item.transform.parent == null))
            {
                if (obj.name != rootObjectName && AssetDatabase.GetAssetOrScenePath(obj).Contains(".unity"))
                {
                    DestroyImmediate(obj);
                }
            }

            AssetUtility.SequenceInfoData.sequenceStatus[(int)Vket_SequenceWindow.Sequence.BuildSizeCheck].status = 1;
            Vket_SequenceWindow.Window.Repaint();
            {
                double timeSinceStartup = EditorApplication.timeSinceStartup;
                while (EditorApplication.timeSinceStartup - timeSinceStartup < 1)
                {
                    yield return null;
                }
            }

            float cmpSize = AssetUtility.ForceRebuild();
            EditorSceneManager.OpenScene(scene.path);
            if (BuildSizeCheck(cmpSize))
            {
                EditorUtility.DisplayDialog("Error", $"{/* 容量がオーバーしています。 */AssetUtility.GetLabelString(24)}\r\n{cmpSize}MB\r\nRegulation {GetMaxSize(MaxSizeType.Build)}MB", "OK");
                Debug.Log(Vket_SequenceWindow.GetResultLog());
                Vket_SequenceWindow.Window.Close();
                enumerators[EnumeratorType.Submission] = null;
                yield break;
            }
            else if (cmpSize == -1)
            {
                EditorUtility.DisplayDialog("Error", /* 容量チェックに失敗しました。 */AssetUtility.GetLabelString(33), "OK");
                Debug.Log(Vket_SequenceWindow.GetResultLog());
                Vket_SequenceWindow.Window.Close();
                enumerators[EnumeratorType.Submission] = null;
                yield break;
            }

            AssetUtility.SequenceInfoData.sequenceStatus[(int)Vket_SequenceWindow.Sequence.BuildSizeCheck].status = 2;
            AssetUtility.SequenceInfoData.sequenceStatus[(int)Vket_SequenceWindow.Sequence.BuildSizeCheck].desc = $"{cmpSize}MB";
            Vket_SequenceWindow.Window.Repaint();
            yield return null;

            AssetUtility.SequenceInfoData.sequenceStatus[(int)Vket_SequenceWindow.Sequence.ScreenShotCheck].status = 1;
            Vket_SequenceWindow.Window.Repaint();
            {
                double timeSinceStartup = EditorApplication.timeSinceStartup;
                while (EditorApplication.timeSinceStartup - timeSinceStartup < 1)
                {
                    yield return null;
                }
            }

            AssetUtility.EditorPlayInfoData.buildSizeSuccessFlag = true;
            AssetUtility.EditorPlayInfoData.isVketEditorPlay = true;
            AssetUtility.EditorPlayInfoData.isSetPassCheckOnly = false;
            AssetUtility.EditorPlayInfoData.ssPath = null;
            AssetUtility.SaveAsset(AssetUtility.EditorPlayInfoData);
            EditorApplication.isPlaying = true;

            enumerators[EnumeratorType.Submission] = null;
        }

        private static IEnumerator BakeCheckAndRun(bool isVRChatCheck)
        {
            var objList = new Dictionary<GameObject, bool>();

            if (!isVRChatCheck)
            {
                LoginInfo info = AssetUtility.LoginInfoData;
                Scene scene = SceneManager.GetActiveScene();
                string rootObjectName = CircleNullOrEmptyCheck() ? scene.name : info.data.circle.id.ToString();
                if (info.data.circle != null)
                {
                    foreach (GameObject obj in Array.FindAll(FindObjectsOfType<GameObject>(), (item) => item.transform.parent == null))
                    {
                        if (obj.name != rootObjectName)
                        {
                            objList.Add(obj, obj.activeSelf);
                            obj.SetActive(false);
                        }
                    }
                }
            }

            EditorUtility.DisplayProgressBar("Bake", "Baking...", 0);
            bool bakeFlag = Lightmapping.BakeAsync();
            if (!bakeFlag)
            {
                EditorUtility.DisplayDialog("Error", /* Light Bakeに失敗しました。 */AssetUtility.GetLabelString(34), "OK");
            }

            while (Lightmapping.isRunning)
            {
                EditorUtility.DisplayProgressBar("Bake", "Baking...", Lightmapping.buildProgress);
                yield return null;
            }

            EditorUtility.ClearProgressBar();

            if (!isVRChatCheck)
            {
                foreach (var pair in objList)
                {
                    pair.Key.SetActive(pair.Value);
                }
            }

            yield return bakeFlag;
        }

        private static IEnumerator EditorPlay()
        {
            ShowControlPanelWindow();

            if (EditorApplication.isPaused)
            {
                EditorApplication.isPaused = false;
            }

            bool isSetPassOnly = AssetUtility.EditorPlayInfoData.isSetPassCheckOnly;
            string progressTitle = "SetPassCalls and Batches Check.";
            float progress = 0;
            EditorUtility.DisplayProgressBar(progressTitle, "Initializing...", progress);
            double time = EditorApplication.timeSinceStartup;
            while (EditorApplication.timeSinceStartup - time < 1.84f) yield return null;

            if (!EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Error", /* Editorが再生中になっていません。\r\nもう一度お試しください。 */AssetUtility.GetLabelString(46), "OK");
                EditorPlayClose();
                if (!isSetPassOnly)
                {
                    Debug.Log(Vket_SequenceWindow.GetResultLog());
                    Vket_SequenceWindow.Window.Close();
                }
                yield break;
            }

            bool flag = false;
            Scene scene = SceneManager.GetActiveScene();
            LoginInfo info = AssetUtility.LoginInfoData;
            string rootObjectName = CircleNullOrEmptyCheck() ? scene.name : info.data.circle.id.ToString();

            if (EditorApplication.isPaused)
            {
                EditorApplication.isPaused = false;
            }

            if (info.data.circle != null)
            {
                foreach (var obj in Array.FindAll(Resources.FindObjectsOfTypeAll<GameObject>(), (item) => item.transform.parent == null))
                {
                    if (obj.name != rootObjectName && AssetDatabase.GetAssetOrScenePath(obj).Contains(".unity"))
                    {
                        DestroyImmediate(obj);
                    }
                    else
                    {
                        flag = true;
                    }
                }
            }
            else
            {
                if (EditorUtility.DisplayDialog("Warning", /* サークルIDを取得できませんでした。\r\n正常にチェックできない可能性がありますが実行しますか？ */AssetUtility.GetLabelString(47), /* はい */AssetUtility.GetLabelString(15), /* いいえ */AssetUtility.GetLabelString(16)))
                {
                    flag = true;
                }
                else
                {
                    EditorPlayClose();
                    if (!isSetPassOnly)
                    {
                        Debug.Log(Vket_SequenceWindow.GetResultLog());
                        Vket_SequenceWindow.Window.Close();
                    }
                    yield break;
                }
            }

            if (!flag)
            {
                EditorUtility.DisplayDialog("Error", /* ブースオブジェクトが見つかりませんでした。\r\nブースチェックを行ってください。 */AssetUtility.GetLabelString(48), "OK");
                EditorPlayClose();
                if (!isSetPassOnly)
                {
                    Debug.Log(Vket_SequenceWindow.GetResultLog());
                    Vket_SequenceWindow.Window.Close();
                }
                yield break;
            }

            if (!isSetPassOnly)
            {
                AssetUtility.SequenceInfoData.sequenceStatus[(int)Vket_SequenceWindow.Sequence.SetPassCheck].status = 1;
                Vket_SequenceWindow.Window.Repaint();
                yield return null;
            }

            if (EditorApplication.isPaused)
            {
                EditorApplication.isPaused = false;
            }

            foreach (Camera cam in Camera.allCameras.Union(SceneView.sceneViews.ToArray().Select(s => ((SceneView)s).camera)))
            {
                cam.enabled = false;
            }

            int space = info.worldData.world_num == 1 ? -4 : info.worldData.world_num == 2 ? 8 : 0;

            GameObject checkParentObj = new GameObject("SetPassCheck");
            checkParentObj.transform.position = new Vector3(0, 0, 0);
            GameObject cameraObj = new GameObject("CheckCamera");
            Camera checkCam = cameraObj.AddComponent<Camera>();
            cameraObj.AddComponent<AudioListener>();
            checkCam.depth = 100;
            cameraObj.transform.SetParent(checkParentObj.transform);
            float rate = info.worldData.world_num == 1 ? 0.5f : info.worldData.world_num == 2 ? 2 : 1;
            cameraObj.transform.position = new Vector3(0, 2.5f * rate, -(10 + space));
#if VRC_SDK_VRCSDK3
            scene.GetRootGameObjects()[0]?.transform.Find("Dynamic")?.gameObject.SetActive(true);
#endif

            List<int> setPassCallsList = new List<int>();
            List<int> batchesList = new List<int>();
            float rotation = 0;
            for (int i = 0; i < 360; i++)
            {
                if (EditorApplication.isPaused)
                {
                    EditorApplication.isPaused = false;
                }
                progress = (float)i / 360;
                if (EditorUtility.DisplayCancelableProgressBar(progressTitle, (progress * 100).ToString("F2") + "%", progress))
                {
                    EditorPlayClose();
                    if (!isSetPassOnly)
                    {
                        Debug.Log(Vket_SequenceWindow.GetResultLog());
                        Vket_SequenceWindow.Window.Close();
                    }
                    yield break;
                }
                checkParentObj.transform.rotation = Quaternion.Euler(0, rotation, 0);
                setPassCallsList.Add(UnityStats.setPassCalls);
                batchesList.Add(UnityStats.batches);
                rotation++;
                yield return null;
            }

            DestroyImmediate(cameraObj);
            DestroyImmediate(checkParentObj);

            int setPassCalls = AssetUtility.EditorPlayInfoData.setPassCalls = (int)setPassCallsList.Average();
            int batches = AssetUtility.EditorPlayInfoData.batches = (int)batchesList.Average();
            AssetUtility.SaveAsset(AssetUtility.EditorPlayInfoData);

            yield return null;

            EditorUtility.ClearProgressBar();

            if (AssetUtility.EditorPlayInfoData.isSetPassCheckOnly)
            {
                if (EditorUtility.DisplayDialog("Complete!", $"SetPass Calls {setPassCalls}, Batches {batches}\r\nRegulation: SetPass Calls {GetMaxSize(MaxSizeType.SetPass)}, Batches {GetMaxSize(MaxSizeType.Batches)}", /*入稿ルール*/AssetUtility.GetLabelString(74), "OK"))
                {
                    Application.OpenURL(GetRuleURL());
                }
                EditorApplication.isPlaying = false;
            }
            else
            {
                GameObject SSCaptureObj = Instantiate(AssetUtility.SSCapturePrefab);
                GameObject SSCameraObj = Instantiate(AssetUtility.SSCameraPrefab);
                SSManager ssManager = SSCaptureObj.GetComponent<SSManager>();
                ssManager.capCamera = SSCameraObj.GetComponent<Camera>();
                SSCameraObj.transform.position = new Vector3(0, 2.5f * rate, 8 + space);
                ssManager.previewPlaceHopeToggle.isOn = info.previewPlaceHopeFlag;
                ssManager.mainSubmissionToggle.isOn = info.mainSubmissionFlag;
                var dcThumbnailUpdateFlag = string.IsNullOrEmpty(info.dcInfoData.image.url);
                ssManager.dcThumbnailChangeToggle.isOn = dcThumbnailUpdateFlag;
                ssManager.dcThumbnailChangeToggle.interactable = !dcThumbnailUpdateFlag;
                ssManager.previewPlaceHopeToggle.transform.Find("Label").GetComponent<Text>().text = /* 下見時に配置されていた場所を希望する */AssetUtility.GetLabelString(65);
                ssManager.mainSubmissionToggle.transform.Find("Label").GetComponent<Text>().text = /* この入稿を本入稿とする */AssetUtility.GetLabelString(66);
                ssManager.screenshotMessageText.text = /* ※ 入稿確認用であり、カタログには乗りません。ユーザーにも見えません。 */AssetUtility.GetLabelString(67);
                ssManager.dcThumbnailChangeToggle.gameObject.SetActive(true);
                ssManager.dcThumbnailChangeToggle.transform.Find("Label").GetComponent<Text>().text = /* サムネイルを変更する */AssetUtility.GetLabelString(68);

                if (Utilities.Networking.NetworkUtility.TermCheck(info.data.circle.event_version.ToString(), Utilities.Networking.NetworkUtility.TermType.Final))
                {
                    ssManager.mainSubmissionToggle.isOn = true;
                    ssManager.mainSubmissionToggle.interactable = false;
                }
            }

            yield return null;
            enumerators[EnumeratorType.EditorPlay] = null;

            void EditorPlayClose()
            {
                AssetUtility.EditorPlayInfoData.isVketEditorPlay = false;
                AssetUtility.EditorPlayInfoData.isSetPassCheckOnly = false;
                AssetUtility.SaveAsset(AssetUtility.EditorPlayInfoData);
                EditorApplication.isPlaying = false;
                EditorUtility.ClearProgressBar();
            }
        }

        private static IEnumerator ExportAndUpload()
        {
            ShowControlPanelWindow();

            AssetUtility.SequenceInfoData.sequenceStatus[(int)Vket_SequenceWindow.Sequence.ScreenShotCheck].status = 2;
            Vket_SequenceWindow.Window.Repaint();
            {
                double timeSinceStartup = EditorApplication.timeSinceStartup;
                while (EditorApplication.timeSinceStartup - timeSinceStartup < 1)
                {
                    yield return null;
                }
            }

            LoginInfo loginInfo = AssetUtility.LoginInfoData;
            if (CircleNullOrEmptyCheck())
            {
                Debug.Log(Vket_SequenceWindow.GetResultLog());
                Vket_SequenceWindow.Window.Close();
                enumerators[EnumeratorType.Export] = null;
                yield break;
            }

            EditorPlayInfo editorPlayInfo = AssetUtility.EditorPlayInfoData;

            int setPassCalls = editorPlayInfo.setPassCalls;
            int batches = editorPlayInfo.batches;
            int maxSetPass = GetMaxSize(MaxSizeType.SetPass);
            int maxBatches = GetMaxSize(MaxSizeType.Batches);

            if (setPassCalls > maxSetPass || batches > maxBatches)
            {
                if (EditorUtility.DisplayDialog("Error", $"{/* SetPassCalls/Batchesが規定をオーバーしています。 */AssetUtility.GetLabelString(41)}\r\nSetPass Calls {setPassCalls}, Batches {batches}\r\nRegulation: SetPass Calls {maxSetPass}, Batches {maxBatches}", /*入稿ルール*/AssetUtility.GetLabelString(74), "OK"))
                {
                    Application.OpenURL(GetRuleURL());
                }
                Debug.LogError($"SetPass Calls {setPassCalls}, Batches {batches}\r\nRegulation: SetPass Calls {maxSetPass}, Batches {maxBatches}");
                Debug.Log(Vket_SequenceWindow.GetResultLog());
                Vket_SequenceWindow.Window.Close();
                enumerators[EnumeratorType.Export] = null;
                yield break;
            }

            AssetUtility.SequenceInfoData.sequenceStatus[(int)Vket_SequenceWindow.Sequence.SetPassCheck].status = 2;
            AssetUtility.SequenceInfoData.sequenceStatus[(int)Vket_SequenceWindow.Sequence.SetPassCheck].desc = $"SetPass Calls {setPassCalls}, Batches {batches}";
            Vket_SequenceWindow.Window.Repaint();
            yield return null;

            AssetUtility.SequenceInfoData.sequenceStatus[(int)Vket_SequenceWindow.Sequence.UploadCheck].status = 1;
            Vket_SequenceWindow.Window.Repaint();
            {
                double timeSinceStartup = EditorApplication.timeSinceStartup;
                while (EditorApplication.timeSinceStartup - timeSinceStartup < 1.84f)
                {
                    yield return null;
                }
            }

            editorPlayInfo.setPassSuccessFlag = true;
            AssetUtility.SaveAsset(editorPlayInfo);

            Type type = typeof(VitDeck.Main.ValidatedExporter.GUI.ValidatedExporterWindow);
            var instance = new VitDeck.Main.ValidatedExporter.GUI.ValidatedExporterWindow();
            ReflectionUtility.InvokeMethod(type, "Init", instance, new object[] { });
            DefaultAsset baseFolder = (DefaultAsset)ReflectionUtility.GetField(type, "baseFolder", instance);
            VitDeck.Utilities.UserSettings userSettings = VitDeck.Utilities.UserSettingUtility.GetUserSettings();
            if (string.IsNullOrEmpty(userSettings.exporterSettingFileName))
            {
                userSettings.exporterSettingFileName = GetExportSetting();
            }
            VitDeck.Exporter.ExportSetting selectedSetting = VitDeck.Exporter.Exporter.GetExportSettings().Where(w => w.name == userSettings.exporterSettingFileName).First();
            string baseFolderPath = AssetDatabase.GetAssetPath(baseFolder);
            ValidatedExportResult result = ValidatedExporter.ValidatedExport(baseFolderPath, selectedSetting, false);
            SetExportValidateLog(type, instance, result, baseFolderPath, selectedSetting.ruleSetName);

            string ssPath = editorPlayInfo.ssPath;

            if (result == null || string.IsNullOrEmpty(ssPath))
            {
                EditorUtility.DisplayDialog("Error", /* 問題が発生した為、入稿を中断しました。 */AssetUtility.GetLabelString(42), "OK");
                Debug.Log(Vket_SequenceWindow.GetResultLog());
                Vket_SequenceWindow.Window.Close();
                enumerators[EnumeratorType.Export] = null;
                yield break;
            }

            if (!result.IsExportSuccess)
            {
                EditorUtility.DisplayDialog("Error", /* ルールチェックで問題が発見された為、入稿を中断しました。 */AssetUtility.GetLabelString(43), "OK");
                Debug.Log(Vket_SequenceWindow.GetResultLog());
                Vket_SequenceWindow.Window.Close();
                enumerators[EnumeratorType.Export] = null;
                yield break;
            }

            if (!AssetUtility.LoginInfoData.data.is_owner)
            {
                EditorUtility.DisplayDialog("Error", /* 入稿は代表者のみが可能です。{n}入稿を中断します。 */AssetUtility.GetLabelString(71), "OK");
            }
            else
            {
                if (editorPlayInfo.buildSizeSuccessFlag && editorPlayInfo.setPassSuccessFlag && editorPlayInfo.ssSuccessFlag)
                {
                    string path = result.exportResult.exportFilePath;
                    string fileName = Path.GetFileName(path);
                    Regex regex = new Regex(@"https:\/\/drive\.google\.com\/drive\/folders\/(?<folderId>.*)\?");
                    Match match = regex.Match(AssetUtility.LoginInfoData.data.space.share_url);
                    string folderId = match.Groups["folderId"].Value;

                    string response = Utilities.Networking.NetworkUtility.FileUpload(fileName, folderId, File.ReadAllBytes(path), AssetUtility.VersionInfoData.event_version);
                    string resMes = "";
                    string uploadFileId = "";

                    if (response != null)
                    {
                        string[] responses = response.Split(',');
                        if (responses.Length > 1)
                        {
                            resMes = responses[0];
                            uploadFileId = responses[1];
                        }
                    }

                    if (resMes == "Success")
                    {
                        AssetUtility.SequenceInfoData.sequenceStatus[(int)Vket_SequenceWindow.Sequence.UploadCheck].status = 2;
                        Vket_SequenceWindow.Window.Repaint();
                        yield return null;

                        AssetUtility.SequenceInfoData.sequenceStatus[(int)Vket_SequenceWindow.Sequence.PostUploadCheck].status = 1;
                        Vket_SequenceWindow.Window.Repaint();
                        {
                            double timeSinceStartup = EditorApplication.timeSinceStartup;
                            while (EditorApplication.timeSinceStartup - timeSinceStartup < 1)
                            {
                                yield return null;
                            }
                        }

                        Utilities.Networking.NetworkUtility.UploadResult uploadResult = new Utilities.Networking.NetworkUtility.UploadResult();
                        uploadResult.package = Path.GetFileNameWithoutExtension(result.exportResult.exportFilePath);
                        uploadResult.setPassCalls = setPassCalls;
                        uploadResult.batches = batches;
                        Utilities.Networking.NetworkUtility.PostUploadResponse resultResponse = Utilities.Networking.NetworkUtility.PostUploadResult(loginInfo.data.space.id, AssetUtility.VersionInfoData.event_version, loginInfo.previewPlaceHopeFlag, loginInfo.mainSubmissionFlag, !loginInfo.dcThumbnailUpdateFlag, uploadResult, ssPath);

                        if (resultResponse == null)
                        {
                            EditorUtility.DisplayProgressBar("Error!", "Initializing...", 0);
                            Vket_GoogleDrive.FileDelete(uploadFileId, folderId);
                            EditorUtility.ClearProgressBar();
                            EditorUtility.DisplayDialog(/*入稿*/AssetUtility.GetLabelString(6), /* 入稿に失敗しました。\r\n入稿データを確認してからもう一度お試しください。 */AssetUtility.GetLabelString(44), "OK");
                            Vket_SequenceWindow.Window.Close();
                            enumerators[EnumeratorType.Export] = null;
                            yield break;
                        }

                        AssetUtility.SequenceInfoData.sequenceStatus[(int)Vket_SequenceWindow.Sequence.PostUploadCheck].status = 2;
                        {
                            double timeSinceStartup = EditorApplication.timeSinceStartup;
                            while (EditorApplication.timeSinceStartup - timeSinceStartup < 0.5f)
                            {
                                yield return null;
                            }
                        }

                        if (EditorUtility.DisplayDialog(/*入稿*/AssetUtility.GetLabelString(6), /* 入稿完了しました。\r\n公式サイトのマイページから「入稿管理ページ」をご確認ください。 */AssetUtility.GetLabelString(45), /* 管理画面へ */AssetUtility.GetLabelString(69), "OK"))
                        {
                            Application.OpenURL("https://www.v-market.work/v5/user/upload");
                        }
                    }
                    else
                    {
                        Debug.LogError(response);
                        Messenger.Messenger.ErrorMessage("Upload Failed.\r\n" + response, "#00-0005");
                        Vket_SequenceWindow.Window.Close();
                        enumerators[EnumeratorType.Export] = null;
                        yield break;
                    }
                }
            }

            {
                double timeSinceStartup = EditorApplication.timeSinceStartup;
                while (EditorApplication.timeSinceStartup - timeSinceStartup < 1)
                {
                    yield return null;
                }
            }

            Debug.Log(Vket_SequenceWindow.GetResultLog());

            Vket_SequenceWindow.Window.Close();

            AssetUtility.EditorPlayInfoData.buildSizeSuccessFlag = false;
            AssetUtility.EditorPlayInfoData.setPassSuccessFlag = false;
            AssetUtility.EditorPlayInfoData.ssSuccessFlag = false;
            AssetUtility.EditorPlayInfoData.forceExportFlag = false;
            AssetUtility.SaveAsset(AssetUtility.EditorPlayInfoData);
        }

        private static string GetValidatorRule()
        {
            string result = "ConceptWorldRuleSet";
            if (CircleNullOrEmptyCheck())
            {
                return result;
            }
            LoginInfo info = AssetUtility.LoginInfoData;

            if (info.worldData.world_num == 1)
            {
                result = "AvatarShowcaseRuleSet";
            }
            else if (info.worldData.world_num == 2)
            {
                result = "DefaultCubeRuleSet";
            }
            else if (info.worldData.world_num == 7 || info.worldData.world_num == 8)
            {
                result = "UdonCubeRuleSet";
            }

            return result;
        }

        private static string GetExportSetting()
        {
            string result = "VketExportSetting";
            if (CircleNullOrEmptyCheck())
            {
                return result;
            }
            LoginInfo info = AssetUtility.LoginInfoData;

            if (info.worldData.world_num == 1)
            {
                result = "VketExportSettingAvatarShowcase";
            }
            else if (info.worldData.world_num == 2)
            {
                result = "VketExportSettingDefaultCube";
            }
            else if (info.worldData.world_num == 7 || info.worldData.world_num == 8)
            {
                result = "VketExportSettingUdonCube";
            }

            return result;
        }

        private static void Login(string hash_key)
        {
            if (EditorPlayCheck())
            {
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (string.IsNullOrEmpty(hash_key))
            {
                if (IsLogin && AssetUtility.LoginInfoData != null && AssetUtility.LoginInfoData.data != null && !string.IsNullOrEmpty(AssetUtility.LoginInfoData.hashKey))
                    hash_key = hashKey = AssetUtility.LoginInfoData.hashKey;
                else
                {
                    EditorUtility.DisplayDialog("Error", /* ログインキーを入力してください。 */AssetUtility.GetLabelString(35), "OK");
                    return;
                }
            }

            LoginInfo info = AssetUtility.LoginInfoData;
            VitDeck.Utilities.UserSettings userSettings = VitDeck.Utilities.UserSettingUtility.GetUserSettings();

            try
            {
                Type type = typeof(Vket_GoogleDrive);
                MethodInfo methodInfo = type.GetMethod("OAuth", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static);
                methodInfo.Invoke(null, null);

                var data = Utilities.Networking.NetworkUtility.GetVketApiData(hash_key, AssetUtility.VersionInfoData.event_version);
                if (data == null || data.status != "200")
                {
                    Logout();
                    Debug.LogError(data);
                    Debug.LogError(data.status);
                    return;
                }

                info.hashKey = hash_key;
                info.data = data;
                if (data.circle != null && data.space != null)
                {
                    info.worldData = Utilities.Networking.NetworkUtility.GetWorldName(info.data.space.sel_world_1);
                    userSettings.validatorFolderPath = "Assets/" + info.data.circle.id;
                    userSettings.validatorRuleSetType = GetValidatorRule();
                    userSettings.exporterSettingFileName = GetExportSetting();
                    info.dcInfoData = Utilities.Networking.NetworkUtility.GetDefaultCubeInfo(info.data.space.id);
                    info.submissionTerm = Utilities.Networking.NetworkUtility.GetTerm(info.data.circle.event_version.ToString());
                }
                AssetUtility.SaveAsset(info);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                if (Vket_GoogleDrive.isServerRun && !Vket_GoogleDrive.stopFlag)
                {
                    Vket_GoogleDrive.stopFlag = true;
                }
                Debug.LogError(/* ログインに失敗しました。お問い合わせにて表示されたエラーコードをお伝えください。 */AssetUtility.GetLabelString(36));
                Logout();
                return;
            }

            if (CircleNullOrEmptyCheck())
            {
                userSettings.validatorRuleSetType = "ConceptWorldRuleSet";
            }
            VitDeck.Utilities.UserSettingUtility.SaveUserSettings(userSettings);

            circleIcon = AssetUtility.GetIcon(info.data.user_icon);

            if (!AuthCheck())
            {
                return;
            }

            AssetDatabase.SaveAssets();
            IsLogin = true;
            AssetUtility.UpdateLanguage();
        }

        private static void Logout()
        {
            if (EditorPlayCheck())
            {
                return;
            }

            if (string.IsNullOrEmpty(hashKey))
            {
                hashKey = AssetUtility.LoginInfoData.hashKey;
            }
            Utilities.Networking.NetworkUtility.VketApiData data = new Utilities.Networking.NetworkUtility.VketApiData();
            LoginInfo info = AssetUtility.LoginInfoData;
            info.data = data;
            info.worldData = null;
            info.hashKey = "";
            VitDeck.Utilities.UserSettings userSettings = VitDeck.Utilities.UserSettingUtility.GetUserSettings();
            userSettings.validatorFolderPath = "";
            userSettings.validatorRuleSetType = "";
            userSettings.exporterSettingFileName = "";
            VitDeck.Utilities.UserSettingUtility.SaveUserSettings(userSettings);
            AssetUtility.SaveAsset(info);
            AssetDatabase.SaveAssets();
            IsLogin = false;
        }

        private static bool AuthCheck()
        {
            if (!Vket_GoogleDrive.isAuth())
            {
                Messenger.Messenger.ErrorMessage(/* ログインに失敗しました。 */AssetUtility.GetLabelString(37), "#00-0003");
                Logout();
                return false;
            }

            return true;
        }

        private static bool UpdateCheck()
        {
            if (!Utilities.Hiding.HidingUtil.DebugMode && !UpdateUtility.UpdateCheck())
            {
                EditorUtility.DisplayDialog("Error", /* アップデートを行ってから実行してください。 */AssetUtility.GetLabelString(38), "OK");
                return false;
            }

            return true;
        }

        private static bool EditorPlayCheck()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Error", /* Editorを再生中は実行できません。 */AssetUtility.GetLabelString(39), "OK");
                return true;
            }

            return false;
        }

        private static bool BuildSizeCheck(float cmpSize)
        {
            return cmpSize > GetMaxSize(MaxSizeType.Build);
        }

        private static bool CircleNullOrEmptyCheck()
        {
            LoginInfo info = AssetUtility.LoginInfoData;
            return info.data == null || info.data.circle == null || info.data.space == null;
        }

        private static bool VRC_LocalTestLaunch()
        {
#if VRC_SDK_VRCSDK3
            bool flg = false;
            foreach (VRC_SceneDescriptor sd in FindObjectsOfType<VRC_SceneDescriptor>())
            {
                flg = true;
                break;
            }
            if (!flg)
            {
                EditorUtility.DisplayDialog("Error", /* VRC_SceneDescriptorが見つかりませんでした。\r\nVRCWorldをシーンに追加してからもう一度実行してください。 */AssetUtility.GetLabelString(40), "OK");
                return false;
            }

            VRC_SdkBuilder.shouldBuildUnityPackage = false;
            AssetExporter.CleanupUnityPackageExport();
            VRC_SdkBuilder.SetNumClients(1);
            VRC_SdkBuilder.PreBuildBehaviourPackaging();
            VRC_SdkBuilder.ExportSceneResourceAndRun();
            return true;
#elif VRC_SDK_VRCSDK2
            bool flg = false;
            foreach (VRC_SceneDescriptor sd in FindObjectsOfType<VRC_SceneDescriptor>())
            {
                flg = true;
                break;
            }
            if (!flg)
            {
                EditorUtility.DisplayDialog("Error", /* VRC_SceneDescriptorが見つかりませんでした。\r\nVRCWorldをシーンに追加してからもう一度実行してください。 */AssetUtility.GetLabelString(40), "OK");
                return false;
            }

            VRC_SdkBuilder.shouldBuildUnityPackage = false;
            VRC.AssetExporter.CleanupUnityPackageExport();
            VRC_SdkBuilder.numClientsToLaunch = 1;
            VRC_SdkBuilder.PreBuildBehaviourPackaging();
            VRC_SdkBuilder.ExportSceneResourceAndRun();
            return true;
#else
            throw new Exception("VACHAT SDK is not imported!");
#endif
        }

        private static void SetExportValidateLog(Type type, object instance, ValidatedExportResult result, string baseFolderPath, string ruleSetName)
        {
            string header = string.Format("- version:{0}", VitDeck.Utilities.ProductInfoUtility.GetVersion()) + Environment.NewLine;
            header += string.Format("- Rule set:{0}", ruleSetName) + Environment.NewLine;
            header += string.Format("- Base folder:{0}", baseFolderPath) + Environment.NewLine;
            string log = header + result.GetValidationLog() + result.GetExportLog() + result.log;
            ReflectionUtility.InvokeMethod(type, "SetMessages", instance, new object[] { header, result });
            ReflectionUtility.InvokeMethod(type, "OutLog", instance, new object[] { log });
            ReflectionUtility.InvokeMethod(type, "OutLog", instance, new object[] { "Export completed." });
        }

        private static void SetValidateLog(Type type, object instance, VitDeck.Validator.ValidationResult[] results, string baseFolderPath, string ruleSetName)
        {
            string header = string.Format("- version:{0}", VitDeck.Utilities.ProductInfoUtility.GetVersion()) + Environment.NewLine;
            header += string.Format("- Rule set:{0}", ruleSetName) + Environment.NewLine;
            header += string.Format("- Base folder:{0}", baseFolderPath) + Environment.NewLine;
            bool isHideInfoMessage = (bool) ReflectionUtility.GetField(type, "isHideInfoMessage", instance);
            string log = header + (string) ReflectionUtility.InvokeMethod(type, "GetResultLog", instance, new object[] { results, isHideInfoMessage ? VitDeck.Validator.IssueLevel.Warning : VitDeck.Validator.IssueLevel.Info });
            ReflectionUtility.InvokeMethod(type, "SetMessages", instance, new object[] { header, results });
            ReflectionUtility.InvokeMethod(type, "OutLog", instance, new object[] { log });
            ReflectionUtility.InvokeMethod(type, "OutLog", instance, new object[] { "Export completed." });
        }

        private static int GetMaxSize(MaxSizeType type)
        {
            var info = AssetUtility.LoginInfoData;
            switch (type)
            {
                case MaxSizeType.SetPass:
                    return (info.worldData.world_num == 2 || info.worldData.world_num == 7 || info.worldData.world_num == 8) ? 120 : info.worldData.world_num == 1 ? 12 : 20;
                case MaxSizeType.Batches:
                    return (info.worldData.world_num == 2 || info.worldData.world_num == 7 || info.worldData.world_num == 8) ? 180 : info.worldData.world_num == 1 ? 18 : 30;
                case MaxSizeType.Build:
                    return (info.worldData.world_num == 2 || info.worldData.world_num == 7 || info.worldData.world_num == 8) ? 20 : info.worldData.world_num == 1 ? 6 : 10;
            }

            return -1;
        }

        private static string GetRuleURL()
        {
            var info = AssetUtility.LoginInfoData;
            switch (info.worldData.world_num)
            {
                case 1:
                    return $"https://{AssetUtility.GetLabelString(75)}";
                case 2:
                    return $"https://{AssetUtility.GetLabelString(76)}";
                case 7:
                case 8:
                    return $"https://{AssetUtility.GetLabelString(84)}";
                default:
                    return $"https://{AssetUtility.GetLabelString(77)}";
            }
        }

        private static void Vket_OnChangedPlayMode(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                if (AssetUtility.EditorPlayInfoData.isVketEditorPlay)
                {
                    ShowControlPanelWindow();
                    enumerators[EnumeratorType.EditorPlay] = EditorPlay();
                }
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                if (AssetUtility.EditorPlayInfoData.isVketEditorPlay)
                {
                    AssetUtility.EditorPlayInfoData.isVketEditorPlay = false;
                    AssetUtility.SaveAsset(AssetUtility.EditorPlayInfoData);

                    if (!AssetUtility.EditorPlayInfoData.isSetPassCheckOnly && !AssetUtility.EditorPlayInfoData.ssSuccessFlag)
                    {
                        Debug.Log(Vket_SequenceWindow.GetResultLog());
                        Vket_SequenceWindow.Window.Close();
                    }

                    if (!AssetUtility.EditorPlayInfoData.isSetPassCheckOnly && AssetUtility.EditorPlayInfoData.ssSuccessFlag)
                    {
                        ShowControlPanelWindow();
                        enumerators[EnumeratorType.Export] = ExportAndUpload();
                    }
                    else
                    {
                        AssetUtility.EditorPlayInfoData.isSetPassCheckOnly = false;
                        AssetUtility.SaveAsset(AssetUtility.EditorPlayInfoData);
                    }
                }
            }
        }
    }
}