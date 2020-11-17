using UnityEngine;

namespace VketTools.Utilities
{
    public class VersionInfo : ScriptableObject
    {
        [SerializeField]
        public string event_version = "";
        [SerializeField]
        public string version = "";
        [SerializeField]
        public string package_type = "";
    }
}