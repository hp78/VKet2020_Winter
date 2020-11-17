using UnityEngine;

namespace VketTools.Utilities
{
    public class EditorPlayInfo : ScriptableObject
    {
        [SerializeField]
        public bool isVketEditorPlay = false;
        [SerializeField]
        public bool isSetPassCheckOnly = false;
        [SerializeField]
        public bool buildSizeSuccessFlag = false;
        [SerializeField]
        public bool setPassSuccessFlag = false;
        [SerializeField]
        public bool ssSuccessFlag = false;
        [SerializeField]
        public bool forceExportFlag = false;
        [SerializeField]
        public int setPassCalls = 0;
        [SerializeField]
        public int batches = 0;
        [SerializeField]
        public string ssPath;
    }
}