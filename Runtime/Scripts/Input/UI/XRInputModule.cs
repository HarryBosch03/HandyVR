using System.Collections.Generic;
using HandyVR.Player.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace HandyVR.Input.UI
{
    /// <summary>
    /// Custom input module added to the event system object, allows VR hands to work with UI.
    /// </summary>
    [AddComponentMenu("HandyVR/XRInputModule", Reference.AddComponentMenuOrder.Components)]
    public class XRInputModule : BaseInputModule
    {
        // Speed used for """double""" clicks
        private const float ClickSpeed = 0.3f;
        // Threshold for the *screen position* to move before dragging is called.
        private const float TrackedDeviceDragThresholdMultiplier = 1.4f;
        
        public override void Process()
        {
            var pointers = XRPointer.All;
            
            foreach (var pointer in pointers)
            {
                // Gets tracked device information from current pointer.
                var data = pointer.GetData();

                // cache and clear position as to not cloud the raycast results.
                var savedPosition = data.position;
                data.position = new Vector2(float.MaxValue, float.MaxValue);
                
                // Raycast with the event system.
                var res = new List<RaycastResult>();
                eventSystem.RaycastAll(data, res);
                
                // Restore the screen position and set the raycast.
                data.position = savedPosition;
                data.pointerCurrentRaycast = FindFirstRaycast(res);
 
                var cam = Camera.main;
                if (!cam) return;
                
                // Cast the raycast result into a screen position for use by the base event system.
                var screenPos = data.position;
                if (data.pointerCurrentRaycast.isValid)
                {
                    screenPos = cam.WorldToScreenPoint(data.pointerCurrentRaycast.worldPosition);
                }

                // Calculate deltas.
                data.delta = screenPos - data.position;
                data.position = screenPos;

                // Update Event System stuff, most of this was directly lifted from Unity's Input System Input Module.
                ProcessPointerButton(pointer);
                ProcessPointerMovement(pointer);
                ProcessScrollWheel(pointer);
                ProcessPointerButtonDrag(pointer);
            }
        }

        private void ProcessPointerButton(XRPointer pointer)
        {
            var data = pointer.GetData();
            var hoverTarget = data.pointerCurrentRaycast.gameObject;

            if (pointer.TriggerAction.State == HandInput.InputWrapper.InputState.PressedThisFrame)
            {
                data.eligibleForClick = true;
                data.delta = Vector2.zero;
                data.dragging = false;
                data.pressPosition = data.position;
                data.pointerPressRaycast = data.pointerCurrentRaycast;
                data.useDragThreshold = true;

                var selectHandler = ExecuteEvents.GetEventHandler<ISelectHandler>(hoverTarget);

                // If we have clicked something new, deselect the old thing
                // and leave 'selection handling' up to the press event.
                if (selectHandler != eventSystem.currentSelectedGameObject)
                    eventSystem.SetSelectedGameObject(null, data);

                // search for the control that will receive the press.
                // if we can't find a press handler set the press
                // handler to be what would receive a click.

                var newPressed = ExecuteEvents.ExecuteHierarchy(hoverTarget, data, ExecuteEvents.pointerDownHandler);

                // We didn't find a press handler, so we search for a click handler.
                if (newPressed == null)
                    newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(hoverTarget);

                var time = Time.unscaledTime;

                if (newPressed == data.lastPress && ((time - data.clickTime) < ClickSpeed))
                    ++data.clickCount;
                else
                    data.clickCount = 1;

                data.clickTime = time;

                data.pointerPress = newPressed;
                data.rawPointerPress = hoverTarget;

                // Save the drag handler for drag events during this mouse down.
                var dragObject = ExecuteEvents.GetEventHandler<IDragHandler>(hoverTarget);
                data.pointerDrag = dragObject;

                if (dragObject != null)
                {
                    ExecuteEvents.Execute(dragObject, data, ExecuteEvents.initializePotentialDrag);
                }
            }

            if (pointer.TriggerAction.State == HandInput.InputWrapper.InputState.ReleasedThisFrame)
            {
                var target = data.pointerPress;
                ExecuteEvents.Execute(target, data, ExecuteEvents.pointerUpHandler);

                var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(hoverTarget);
                var pointerDrag = data.pointerDrag;
                if (target == pointerUpHandler && data.eligibleForClick)
                {
                    ExecuteEvents.Execute(target, data, ExecuteEvents.pointerClickHandler);
                }
                else if (data.dragging && pointerDrag != null)
                {
                    ExecuteEvents.ExecuteHierarchy(hoverTarget, data, ExecuteEvents.dropHandler);
                }

                data.eligibleForClick = false;
                data.pointerPress = null;
                data.rawPointerPress = null;

                if (data.dragging && pointerDrag != null)
                {
                    ExecuteEvents.Execute(pointerDrag, data, ExecuteEvents.endDragHandler);
                }

                data.dragging = false;
                data.pointerDrag = null;
            }
        }

        private void ProcessPointerMovement(XRPointer pointer)
        {
            var data = pointer.GetData();
            var currentPointerTarget = data.pointerCurrentRaycast.gameObject;

            var wasMoved = data.IsPointerMoving();
            if (wasMoved)
            {
                foreach (var hovered in data.hovered)
                {
                    ExecuteEvents.Execute(hovered, data, ExecuteEvents.pointerMoveHandler);
                }
            }

            // If we have no target or pointerEnter has been deleted,
            // we just send exit events to anything we are tracking
            // and then exit.
            if (currentPointerTarget == null || data.pointerEnter == null)
            {
                foreach (var hovered in data.hovered)
                {
                    ExecuteEvents.Execute(hovered, data, ExecuteEvents.pointerExitHandler);
                }

                data.hovered.Clear();

                if (currentPointerTarget == null)
                {
                    data.pointerEnter = null;
                    return;
                }
            }

            if (data.pointerEnter == currentPointerTarget)
                return;

            var commonRoot = FindCommonRoot(data.pointerEnter, currentPointerTarget);

            // We walk up the tree until a common root and the last entered and current entered object is found.
            // Then send exit and enter events up to, but not including, the common root.
            if (data.pointerEnter != null)
            {
                var target = data.pointerEnter.transform;

                while (target != null)
                {
                    if (commonRoot != null && commonRoot.transform == target)
                        break;

                    var targetGameObject = target.gameObject;
                    ExecuteEvents.Execute(targetGameObject, data, ExecuteEvents.pointerExitHandler);

                    data.hovered.Remove(targetGameObject);

                    target = target.parent;
                }
            }

            data.pointerEnter = currentPointerTarget;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse -- Could be null if it was destroyed immediately after executing above
            if (currentPointerTarget != null)
            {
                var target = currentPointerTarget.transform;

                while (target != null && target.gameObject != commonRoot)
                {
                    var targetGameObject = target.gameObject;
                    ExecuteEvents.Execute(targetGameObject, data, ExecuteEvents.pointerEnterHandler);
                    if (wasMoved)
                    {
                        ExecuteEvents.Execute(targetGameObject, data, ExecuteEvents.pointerMoveHandler);
                    }
                    data.hovered.Add(targetGameObject);

                    target = target.parent;
                }
            }
        }

        private void ProcessScrollWheel(XRPointer pointer)
        {
            var data = pointer.GetData();
            var scrollDelta = data.scrollDelta;
            if (Mathf.Approximately(scrollDelta.sqrMagnitude, 0f)) return;
            
            var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(data.pointerEnter);
            ExecuteEvents.ExecuteHierarchy(scrollHandler, data, ExecuteEvents.scrollHandler);
        }

        private void ProcessPointerButtonDrag(XRPointer pointer)
        {
            var data = pointer.GetData();
            var pointerType = data.pointerType;
            
            if (!data.IsPointerMoving() ||
                (pointerType == UIPointerType.MouseOrPen && Cursor.lockState == CursorLockMode.Locked) ||
                data.pointerDrag == null)
            {
                return;
            }

            if (!data.dragging)
            {
                var threshold = eventSystem.pixelDragThreshold * TrackedDeviceDragThresholdMultiplier;
                if (!data.useDragThreshold || (data.pressPosition - data.position).sqrMagnitude >= (threshold * threshold))
                {
                    var target = data.pointerDrag;
                    ExecuteEvents.Execute(target, data, ExecuteEvents.beginDragHandler);
                    data.dragging = true;
                }
            }

            if (data.dragging)
            {
                // If we moved from our initial press object, process an up for that object.
                var target = data.pointerPress;
                if (target != data.pointerDrag)
                {
                    ExecuteEvents.Execute(target, data, ExecuteEvents.pointerUpHandler);

                    data.eligibleForClick = false;
                    data.pointerPress = null;
                    data.rawPointerPress = null;
                }

                ExecuteEvents.Execute(data.pointerDrag, data, ExecuteEvents.dragHandler);
            }
        }
    }
}