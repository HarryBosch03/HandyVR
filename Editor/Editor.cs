using System;
using UnityEditor;
using UnityEngine;

namespace HandyVR.Editor
{
    public class Editor<T> : UnityEditor.Editor where T : UnityEngine.Object
    {
        public T Target => (T)base.target;

        public static float LineHeight => EditorGUIUtility.singleLineHeight;

        protected string KeyFromName(string name)
        {
            return $"{target.GetInstanceID()}.{name}";
        }

        protected void Section(string name, Action body, bool fallback = false)
        {
            var foldout = EditorPrefs.GetBool(KeyFromName(name), fallback);
            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, name);
            EditorPrefs.SetBool(KeyFromName(name), foldout);

            if (foldout)
                using (new EditorGUI.IndentLevelScope())
                {
                    body();
                }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        protected static void Div()
        {
            var rect = EditorGUILayout.GetControlRect();
            rect.y += rect.height / 2.0f;
            rect.height = 1.0f;
            EditorGUI.DrawRect(rect, new Color(1.0f, 1.0f, 1.0f, 0.1f));
        }

        protected virtual void Space() => EditorGUILayout.Space(LineHeight / 2.0f);
    }
}