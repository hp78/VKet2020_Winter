using UnityEngine;

namespace VketTools.Utilities
{
    public class LoginInfo : ScriptableObject
    {
        [SerializeField]
        public Networking.NetworkUtility.VketApiData data;
        [SerializeField]
        public string hashKey;
        [SerializeField]
        public bool vrcLaunchFlag = true;
        [SerializeField]
        public Networking.NetworkUtility.WorldData worldData;
        [SerializeField]
        public bool previewPlaceHopeFlag = false;
        [SerializeField]
        public bool mainSubmissionFlag = false;
        [SerializeField]
        public bool dcThumbnailUpdateFlag = false;
        [SerializeField]
        public Networking.NetworkUtility.DefaultCubeInfoData dcInfoData;
        [SerializeField]
        public Networking.NetworkUtility.SubmissionDateInfo submissionTerm;
    }
}