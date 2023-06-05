using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HandyVR.Interfaces
{
    public interface IHasValidationChecks
    {
        public List<ValidationCheck> ValidationList { get; }
        
        public static ValidationLevel IsValid(IHasValidationChecks target)
        {
            var valid = ValidationLevel.Valid;
            foreach (var check in target.ValidationList)
            {
                var localValid = check.checkCallback(target);
                valid = localValid > valid ? localValid : valid;
            }
            return valid;
        }

        public static void DrawGUI(IHasValidationChecks target)
        {
            var valid = ValidationLevel.Valid;
            foreach (var check in target.ValidationList)
            {
                var lValid = check.checkCallback(target);
                valid = lValid > valid ? lValid : valid;
                switch (lValid)
                {
                    default:
                    case ValidationLevel.Valid:
                        break;
                    case ValidationLevel.Warnings:
                        EditorGUILayout.HelpBox(check.failureMessage, MessageType.Warning);
                        break;
                    case ValidationLevel.Errors:
                        EditorGUILayout.HelpBox(check.failureMessage, MessageType.Error);
                        break;
                }
            }

            if (valid == ValidationLevel.Valid)
            {
                EditorGUILayout.HelpBox("No Problems Here :)", MessageType.Info);
            }
        }

        public static ValidationCheck HasComponent(Type type, ValidationLevel failureLevel = ValidationLevel.Errors)
        {
            return new ValidationCheck
            {
                checkCallback = (target) =>
                {
                    var component = target as Component;
                    if (!component) return failureLevel;
                    var valid = component.GetComponent(type) != null;
                    return valid ? ValidationLevel.Valid : failureLevel;
                },
                failureMessage = $"VR Hand must have a component that inherits from {type.Name}",
            };
        }
    }

    public class ValidationCheck
    {
        public Func<IHasValidationChecks, ValidationLevel> checkCallback;
        public string failureMessage;
    }
    
    public enum ValidationLevel
    {
        Valid = 0,
        Warnings = 1,
        Errors = 2,
    }
}