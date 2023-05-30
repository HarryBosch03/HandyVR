using System;
using HandyVR.Bindables;
using HandyVR.Core;
using HandyVR.Interfaces;
using HandyVR.Switches;
using TMPro;
using UnityEditor.MemoryProfiler;
using UnityEngine;

namespace HandyVR.Player.Hands
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public class VRHandDebugger : MonoBehaviour, IVRHandModule
    {
        [SerializeField] private FloatDriverData floatDriver;

        private VRHand hand;

        public void Init(VRHand hand)
        {
            this.hand = hand;

            floatDriver.Init(hand);
        }

        private void Update()
        {
            floatDriver.Update();
        }

        [Serializable]
        public abstract class WorldCanvasDebugger
        {
            public GameObject root;
            public Vector3 offset = new(-0.1f, 0.2f, 0.0f);
            public float animateSpeed = 0.1f;
            
            [Space]
            public LineRenderer lines;

            protected VRHand hand;
            
            private float scale;
            private bool open;
            private Vector3 anchor;
            private Camera camera;

            private Vector3 position;
            private Quaternion rotation;

            public virtual void Init(VRHand hand)
            {
                if (!lines) lines = root.GetComponentInChildren<LineRenderer>();

                this.hand = hand;
                camera = Camera.main;
            }

            public virtual void Update()
            {
                root.transform.position = position;
                root.transform.rotation = rotation;
                
                root.transform.localScale = Vector3.one * scale;
                Utility.Animation.SimpleMoveTowards(ref scale, open ? 1.0f : 0.0f, animateSpeed);

                if (open)
                {
                    position = anchor + camera.transform.rotation * offset;
                    var face = camera.transform.position - position;
                    rotation = Quaternion.LookRotation(-face, Vector3.up);
                    
                    lines.SetLine(anchor, position);
                }
            }

            public void Hide()
            {
                open = false;
            }

            public void Show(Vector3 anchor)
            {
                this.anchor = anchor;
                open = true;
            }
        }
        
        [Serializable]
        public class FloatDriverData : WorldCanvasDebugger
        {
            public TMP_Text text;
            
            [Space]
            public string template;
 
            public override void Init(VRHand hand)
            {
                base.Init(hand);
                
                if (!text) text = root.GetComponentInChildren<TMP_Text>();
            }

            public override void Update()
            {
                base.Update();
                
                Hide();
                
                var binding = hand.ActiveBinding;
                if (!binding) return;
                var driver = binding.bindable as FloatDriver;
                if (!driver) return;
                
                Show(driver.transform.position);
                
                text.text = string.Format(template, Mathf.Round(driver.Value * 20.0f) / 20.0f);
            }
        }
    }
}