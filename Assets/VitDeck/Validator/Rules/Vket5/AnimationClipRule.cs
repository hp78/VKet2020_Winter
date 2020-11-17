using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VitDeck.Language;

namespace VitDeck.Validator
{
    public class AnimationClipRule : BaseRule
    {
        public AnimationClipRule(string name) : base(name)
        {
        }

        protected override void Logic(ValidationTarget target)
        {
            foreach (var asset in target.GetAllAssets())
            {
                var clip = asset as AnimationClip;
                if (clip == null)
                {
                    continue;
                }

                LogicForAnimationClip(clip);
            }
        }

        private void LogicForAnimationClip(AnimationClip clip)
        {
            var objectBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            foreach (var binding in objectBindings)
            {
                var keyFrames = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                foreach (var curve in keyFrames)
                {
                    if (curve.value is Material)
                    {
                        AddIssue(new Issue(
                            clip,
                            IssueLevel.Error,
                            LocalizedMessage.Get("AnimationClipRule.DontChangeMaterialInAnimation"),
                            LocalizedMessage.Get("AnimationClipRule.DontChangeMaterialInAnimation.Solution")
                            ));
                    }
                    // エラーは1個出せば十分なのでbreakでループを抜ける
                    break;
                }

                LogicForBinding(clip, binding);
            }
            var curveBindings = AnimationUtility.GetCurveBindings(clip);
            foreach (var binding in curveBindings)
            {
                LogicForBinding(clip, binding);
            }
        }

        private void LogicForBinding(AnimationClip clip, EditorCurveBinding binding)
        {
            if (binding.path.Contains("../"))
            {
                AddIssue(new Issue(
                    clip,
                    IssueLevel.Error,
                    LocalizedMessage.Get("AnimationClipRule.DontAccessParentObject"),
                    LocalizedMessage.Get("AnimationClipRule.DontAccessParentObject.Solution")
                    ));
            }
        }
    }
}