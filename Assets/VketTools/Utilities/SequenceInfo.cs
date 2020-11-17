using System;
using UnityEngine;

namespace VketTools.Utilities
{
    public class SequenceInfo : ScriptableObject
    {
        public SequenceStatus[] sequenceStatus;
    }

    [Serializable]
    public class SequenceStatus
    {
        public int status = 0;
        public string desc = "";
    }
}