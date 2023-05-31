using System;
using UnityEditor;
using UnityEngine;

namespace HandyVR.Editor
{
    [InitializeOnLoad]
    public static class HierarchyColors
    {
        static HierarchyColors()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
        }

        private static void OnHierarchyWindowItemOnGUI(int instanceId, Rect selectionRect)
        {
            var gameObject = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (!gameObject) return;

            if (!gameObject.TryGetComponent(out IHasValidationChecks validationChecks)) return;

            var h = selectionRect.height;
            selectionRect.x += selectionRect.width;
            selectionRect.width = h;
            selectionRect.x += h * 0.1f;
            selectionRect.y += h * 0.1f;
            selectionRect.width *= 0.8f;
            selectionRect.height *= 0.8f;
            
            Texture icon;
            switch (IHasValidationChecks.IsValid(validationChecks))
            {
                default:
                case ValidationLevel.Valid:
                    break;
                case ValidationLevel.Warnings:
                    icon = EditorGUIUtility.IconContent("console.warnicon").image;
                    GUI.DrawTexture(selectionRect, icon);
                    break;
                case ValidationLevel.Errors:
                    icon = EditorGUIUtility.IconContent("console.erroricon").image;
                    GUI.DrawTexture(selectionRect, icon);
                    break;
            }
        }
    }
}