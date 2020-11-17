using UnityEngine;

namespace VketTools.Utilities
{
    public class LanguageInfo : ScriptableObject
    {
        [SerializeField]
        public Networking.NetworkUtility.LanguageItem[] languages;
    }
}