using System;
using System.Collections;
using UnityEngine;

namespace HandyVR.Utility
{
    public class DeferredCall : MonoBehaviour
    {
        // ReSharper disable once InconsistentNaming
        private static DeferredCall caller_DoNotUse;

        private static DeferredCall Caller
        {
            get
            {
                if (caller_DoNotUse) return caller_DoNotUse;

                caller_DoNotUse = new GameObject("DeferredCaller").AddComponent<DeferredCall>();
                caller_DoNotUse.gameObject.hideFlags = HideFlags.HideAndDontSave;

                return caller_DoNotUse;
            }
        }

        public static void Wait(Action callback, YieldInstruction waitFor)
        {
            IEnumerator routine()
            {
                yield return waitFor;
                callback();
            }

            Caller.StartCoroutine(routine());
        }
    }
}