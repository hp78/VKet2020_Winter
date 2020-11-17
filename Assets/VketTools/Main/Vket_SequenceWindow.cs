using System;
using UnityEditor;
using UnityEngine;
using VketTools.Utilities;

namespace VketTools.Main
{
    public class Vket_SequenceWindow : EditorWindow
    {
        private static Vket_SequenceWindow window;
        private static float time;

        public enum Sequence
        {
            BakeCheck,
            VRChatCheck,
            RuleCheck,
            BuildSizeCheck,
            ScreenShotCheck,
            SetPassCheck,
            UploadCheck,
            PostUploadCheck
        }

        public static Vket_SequenceWindow Window
        {
            get
            {
                Vket_SequenceWindow win = null;
                try
                {
                    win = GetWindow<Vket_SequenceWindow>();
                }
                catch { }
                return win;
            }
        }

        public static void ShowSequenceWindow()
        {
            try
            {
                if (Window != null)
                {
                    Window.Close();
                }
            }
            catch { }

            window = GetWindow<Vket_SequenceWindow>(false, "Sequence - VketTools -");
            window.minSize = new Vector2(280, 320);
            window.maxSize = new Vector2(280.184f, 320.184f);
            Init();
            window.Show();
        }

        public static string GetResultLog()
        {
            var result = "";
            var info = AssetUtility.SequenceInfoData;
            for (int i = 0; i < info.sequenceStatus.Length; ++i)
            {
                result += $"{Enum.ToObject(typeof(Sequence), i)}:{info.sequenceStatus[i].status},{info.sequenceStatus[i].desc}\r\n";
            }
            return result;
        }

        private static void Init()
        {
            AssetUtility.SequenceInfoData.sequenceStatus = new SequenceStatus[8];
            for (int i = 0; i < AssetUtility.SequenceInfoData.sequenceStatus.Length; ++i)
            {
                AssetUtility.SequenceInfoData.sequenceStatus[i] = new SequenceStatus();
                AssetUtility.SequenceInfoData.sequenceStatus[i].status = 0;
                AssetUtility.SequenceInfoData.sequenceStatus[i].desc = "";
            }
        }

        private void Update()
        {
            time -= Time.deltaTime;
            if (time < 0)
            {
                time = Application.isPlaying ? 0.184f : 2.74f;
                var editorEvent = EditorGUIUtility.CommandEvent("OnGUI");
                editorEvent.type = EventType.Used;
                SendEvent(editorEvent);
            }
        }

        public new void Close()
        {
            Init();
            base.Close();
        }

        private void OnGUI()
        {
            GUIStyle box = new GUIStyle(GUI.skin.box);
            GUIStyle l1 = new GUIStyle(GUI.skin.label);
            GUIStyle l2 = new GUIStyle(GUI.skin.label);
            l1.fontSize = 13;
            l1.fixedHeight = 30;
            l2.fontSize = 10;
            l2.fixedHeight = 25;

            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(box);
            {
                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(20);
                    if (AssetUtility.SequenceInfoData.sequenceStatus[(int)Sequence.BakeCheck].status == 1)
                    {
                        UIUtility.EditorGUIWaitSpin();
                    }
                    else if (AssetUtility.SequenceInfoData.sequenceStatus[(int)Sequence.BakeCheck].status == 2)
                    {
                        var icon = EditorGUIUtility.IconContent("toggle on");
                        GUILayout.Box(icon, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16));
                    }
                    else
                    {
                        GUILayout.Space(16);
                    }
                    EditorGUILayout.Space();
                    var content = new GUIContent(/* VRChat内での確認 */AssetUtility.GetLabelString(57));
                    EditorGUILayout.LabelField(content, UIUtility.GetContentSizeFitStyle(content, l1, position.width));
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(36);
                    EditorGUILayout.Space();
                    var content = new GUIContent(AssetUtility.SequenceInfoData.sequenceStatus[(int)Sequence.BakeCheck].desc);
                    EditorGUILayout.LabelField(content, UIUtility.GetContentSizeFitStyle(content, l2, position.width));
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(20);
                    if (AssetUtility.SequenceInfoData.sequenceStatus[(int)Sequence.VRChatCheck].status == 1)
                    {
                        UIUtility.EditorGUIWaitSpin();
                    }
                    else if (AssetUtility.SequenceInfoData.sequenceStatus[(int)Sequence.VRChatCheck].status == 2)
                    {
                        var icon = EditorGUIUtility.IconContent("toggle on");
                        GUILayout.Box(icon, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16));
                    }
                    else
                    {
                        GUILayout.Space(16);
                    }
                    EditorGUILayout.Space();
                    var content = new GUIContent(/* VRChat内での確認 */AssetUtility.GetLabelString(58));
                    EditorGUILayout.LabelField(content, UIUtility.GetContentSizeFitStyle(content, l1, position.width));
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(36);
                    EditorGUILayout.Space();
                    var content = new GUIContent(AssetUtility.SequenceInfoData.sequenceStatus[(int)Sequence.VRChatCheck].desc);
                    EditorGUILayout.LabelField(content, UIUtility.GetContentSizeFitStyle(content, l2, position.width));
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(20);
                    if (AssetUtility.SequenceInfoData.sequenceStatus[(int)Sequence.RuleCheck].status == 1)
                    {
                        UIUtility.EditorGUIWaitSpin();
                    }
                    else if (AssetUtility.SequenceInfoData.sequenceStatus[(int)Sequence.RuleCheck].status == 2)
                    {
                        var icon = EditorGUIUtility.IconContent("toggle on");
                        GUILayout.Box(icon, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16));
                    }
                    else
                    {
                        GUILayout.Space(16);
                    }
                    EditorGUILayout.Space();
                    var content = new GUIContent(/* ルールチェック */AssetUtility.GetLabelString(59));
                    EditorGUILayout.LabelField(content, UIUtility.GetContentSizeFitStyle(content, l1, position.width));
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(36);
                    EditorGUILayout.Space();
                    var content = new GUIContent(AssetUtility.SequenceInfoData.sequenceStatus[(int)Sequence.RuleCheck].desc);
                    EditorGUILayout.LabelField(content, UIUtility.GetContentSizeFitStyle(content, l2, position.width));
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(20);
                    if (AssetUtility.SequenceInfoData.sequenceStatus[(int)Sequence.BuildSizeCheck].status == 1)
                    {
                        UIUtility.EditorGUIWaitSpin();
                    }
                    else if (AssetUtility.SequenceInfoData.sequenceStatus[(int)Sequence.BuildSizeCheck].status == 2)
                    {
                        var icon = EditorGUIUtility.IconContent("toggle on");
                        GUILayout.Box(icon, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16));
                    }
                    else
                    {
                        GUILayout.Space(16);
                    }
                    EditorGUILayout.Space();
                    var content = new GUIContent(/* 容量チェック */AssetUtility.GetLabelString(60));
                    EditorGUILayout.LabelField(content, UIUtility.GetContentSizeFitStyle(content, l1, position.width));
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(36);
                    EditorGUILayout.Space();
                    var content = new GUIContent(AssetUtility.SequenceInfoData.sequenceStatus[(int)Sequence.BuildSizeCheck].desc);
                    EditorGUILayout.LabelField(content, UIUtility.GetContentSizeFitStyle(content, l2, position.width));
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(20);
                    if (AssetUtility.SequenceInfoData.sequenceStatus[(int)Sequence.ScreenShotCheck].status == 1)
                    {
                        UIUtility.EditorGUIWaitSpin();
                    }
                    else if (AssetUtility.SequenceInfoData.sequenceStatus[(int)Sequence.ScreenShotCheck].status == 2)
                    {
                        var icon = EditorGUIUtility.IconContent("toggle on");
                        GUILayout.Box(icon, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16));
                    }
                    else
                    {
                        GUILayout.Space(16);
                    }
                    EditorGUILayout.Space();
                    var content = new GUIContent(/* スクリーンショット撮影 */AssetUtility.GetLabelString(61));
                    EditorGUILayout.LabelField(content, UIUtility.GetContentSizeFitStyle(content, l1, position.width));
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(36);
                    EditorGUILayout.Space();
                    var content = new GUIContent(AssetUtility.SequenceInfoData.sequenceStatus[(int)Sequence.ScreenShotCheck].desc);
                    EditorGUILayout.LabelField(content, UIUtility.GetContentSizeFitStyle(content, l2, position.width));
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(20);
                    if (AssetUtility.SequenceInfoData.sequenceStatus[(int)Sequence.SetPassCheck].status == 1)
                    {
                        UIUtility.EditorGUIWaitSpin();
                    }
                    else if (AssetUtility.SequenceInfoData.sequenceStatus[(int)Sequence.SetPassCheck].status == 2)
                    {
                        var icon = EditorGUIUtility.IconContent("toggle on");
                        GUILayout.Box(icon, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16));
                    }
                    else
                    {
                        GUILayout.Space(16);
                    }
                    EditorGUILayout.Space();
                    var content = new GUIContent(/* SetPassチェック */AssetUtility.GetLabelString(62));
                    EditorGUILayout.LabelField(content, UIUtility.GetContentSizeFitStyle(content, l1, position.width));
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(36);
                    EditorGUILayout.Space();
                    var content = new GUIContent(AssetUtility.SequenceInfoData.sequenceStatus[(int)Sequence.SetPassCheck].desc);
                    EditorGUILayout.LabelField(content, UIUtility.GetContentSizeFitStyle(content, l2, position.width));
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(20);
                    if (AssetUtility.SequenceInfoData.sequenceStatus[(int)Sequence.UploadCheck].status == 1)
                    {
                        UIUtility.EditorGUIWaitSpin();
                    }
                    else if (AssetUtility.SequenceInfoData.sequenceStatus[(int)Sequence.UploadCheck].status == 2)
                    {
                        var icon = EditorGUIUtility.IconContent("toggle on");
                        GUILayout.Box(icon, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16));
                    }
                    else
                    {
                        GUILayout.Space(16);
                    }
                    EditorGUILayout.Space();
                    var content = new GUIContent(/* アップロード */AssetUtility.GetLabelString(63));
                    EditorGUILayout.LabelField(content, UIUtility.GetContentSizeFitStyle(content, l1, position.width));
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(36);
                    EditorGUILayout.Space();
                    var content = new GUIContent(AssetUtility.SequenceInfoData.sequenceStatus[(int)Sequence.UploadCheck].desc);
                    EditorGUILayout.LabelField(content, UIUtility.GetContentSizeFitStyle(content, l2, position.width));
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(20);
                    if (AssetUtility.SequenceInfoData.sequenceStatus[(int)Sequence.PostUploadCheck].status == 1)
                    {
                        UIUtility.EditorGUIWaitSpin();
                    }
                    else if (AssetUtility.SequenceInfoData.sequenceStatus[(int)Sequence.PostUploadCheck].status == 2)
                    {
                        var icon = EditorGUIUtility.IconContent("toggle on");
                        GUILayout.Box(icon, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(16));
                    }
                    else
                    {
                        GUILayout.Space(16);
                    }
                    EditorGUILayout.Space();
                    var content = new GUIContent(/* アップロード後処理 */AssetUtility.GetLabelString(64));
                    EditorGUILayout.LabelField(content, UIUtility.GetContentSizeFitStyle(content, l1, position.width));
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(36);
                    EditorGUILayout.Space();
                    var content = new GUIContent(AssetUtility.SequenceInfoData.sequenceStatus[(int)Sequence.PostUploadCheck].desc);
                    EditorGUILayout.LabelField(content, UIUtility.GetContentSizeFitStyle(content, l2, position.width));
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
    }
}