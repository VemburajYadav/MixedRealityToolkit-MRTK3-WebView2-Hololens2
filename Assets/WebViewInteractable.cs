using Microsoft.MixedReality.WebView;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.UX;
using MixedReality.Toolkit.Diagnostics;
using MixedReality.Toolkit.Subsystems;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Unity.Profiling;

public class WebViewInteractable : StatefulInteractable
{
    private StatefulInteractable interactable;
    private WebView webViewComponent;
    private Transform webViewTransform;
    public MeshCollider Collider;
    private IWebView _webView;
    private WebViewMouseEventData mouseEvent;
    private Vector2 webViewCoordNormalized;

    // Start is called before the first frame update
    void Start()
    {
        // Setup the WebView
        webViewComponent = gameObject.GetComponent<WebView>();
        webViewComponent.GetWebViewWhenReady((IWebView webView) =>
        {
            _webView = webView;
        });

        // Get the WebView Transform
        webViewTransform = gameObject.GetComponent<Transform>();
        Debug.Log("Ray Selected: " + IsRaySelected);
        Debug.Log("Ray Hovered: " + IsRayHovered);
        Debug.Log("Poke Selected: " + IsPokeSelected);
        Debug.Log("Poke Hovered: " + IsPokeHovered);
        Debug.Log("Gaze Hovered: " + IsGazeHovered);
        Debug.Log("Gaze Pinch Selected: " + IsGazePinchSelected);
        Debug.Log("Gaze Pinch Hovered: " + IsGazePinchHovered);
        Debug.Log("Active Hovered: " + IsActiveHovered);

        DisableInteractorType(typeof(IGrabInteractor));
        DisableInteractorType(typeof(IGazeInteractor));
        DisableInteractorType(typeof(IGazePinchInteractor));


        // Get the interactable (PressableButton) and set the EndPushPlane to some small value 
        interactable = gameObject.GetComponent<StatefulInteractable>();
       
        // interactable.EndPushPlane = 0.001f;

    }

    void Update()
    {

        // var interactors = FindObjectsOfType<MRTKRayInteractor>();
        /*
        if (IsRaySelected) Debug.Log("Ray Selected: " + IsRaySelected);
        if (IsRayHovered) Debug.Log("Ray Hovered: " + IsRayHovered);
        if (IsPokeHovered) Debug.Log("Poke Hovered: " + IsPokeHovered);
        if (IsPokeSelected) Debug.Log("Poke Selected: " + IsPokeSelected);
        if (IsGazeHovered) Debug.Log("Gaze Hovered: " + IsGazeHovered);
        if (IsGazePinchHovered) Debug.Log("Gaze Pinch Hovered: " + IsGazePinchHovered);
        if (IsGazePinchSelected) Debug.Log("Gaze Pinch Selected: " + IsGazePinchSelected);
        if (IsActiveHovered) Debug.Log("Active Hovered: " + IsActiveHovered);
        */
    }

    /*
    protected override void OnHoverEntering(HoverEnterEventArgs args)
    {
        Debug.Log("Hover status (before entering): " + interactable.isHovered);
        var interactor = args.interactor;
        interactor.allowHover = false;
        Debug.Log("Hover status (after entering): " + interactable.isHovered);
    }
    */


    /*
    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        Debug.Log("Hover status (before entered): " + interactable.isHovered);
        var interactor = args.interactor;
        // interactor.allowHover = false;
        Debug.Log("Hover status (after entered): " + interactable.isHovered);
    }
    */

    /// <inheritdoc />

    /// <summary>
    /// This function determines whether the interactable should fire a click event at a given select event.
    /// Subclasses can override this to add additional requirements for full click/toggle activation,
    /// such as roll-off prevention.
    /// </summary>
    /// <returns><see langword="true"/> if the interactable should fire click or toggle event from this current select event.</returns>
    protected override bool CanClickOnFirstSelectEntered(SelectEnterEventArgs args) => !TriggerOnRelease;

    /// <summary>
    /// This function determines whether the interactable should fire a click event at a given deselect event.
    /// Subclasses can override this to add additional requirements for full click/toggle activation,
    /// such as roll-off prevention.
    /// </summary>
    /// <returns><see langword="true"/> if the interactable should fire click or toggle event from this current deselect event.</returns>
    protected override bool CanClickOnLastSelectExited(SelectExitEventArgs args)
    {
        return TriggerOnRelease && IsRegistered() && IsInteractorTracked() && IsTargetValid();

        // This check will prevent OnClick from firing when the interactable or interactor was unregistered.
        bool IsRegistered()
        {
            return !args.isCanceled;
        }

        // This check will prevent OnClick from firing when the interactor loses tracking.
        // XRI interactor interfaces don't have a good API for "is this interactor tracked?"
        // Hover-active is a good equivalent, though, as MRTK interactors set hoverActive false
        // when their controller loses tracking.
        bool IsInteractorTracked()
        {
            return !(args.interactorObject is IXRHoverInteractor hoverInteractor) ||
                   hoverInteractor.isHoverActive;
        }

        // This check will prevent OnClick from firing when the interactable is not being hovered.
        bool IsTargetValid()
        {
            return !SelectRequiresHover ||
                   !(args.interactableObject is IXRHoverInteractable hoverInteractable) ||
                   hoverInteractable.isHovered;
        }
    }

    protected override void OnFirstSelectEntered(SelectEnterEventArgs args)
    {
        base.OnFirstSelectEntered(args);
        var interactor = args.interactor;
        var worldIntersectionPoint = interactor.attachTransform.position;
        Vector3 localIntersectionPoint = webViewTransform.InverseTransformPoint(worldIntersectionPoint);
        webViewCoordNormalized = new Vector2(localIntersectionPoint.x + 0.5f, 0.5f - localIntersectionPoint.y);

        // interactor.allowSelect = false;
        
        FireMouseDownEvent(webViewCoordNormalized);
        // FireMouseUpEvent(webViewCoordNormalized);

        Debug.Log("On First Select Entered");
        // Debug.Log("Is Selected: " + interactable.isSelected);
        // Debug.Log("Selection Progress: : " + interactable.GetSelectionProgress());
        // Debug.Log("Selecting Interactors: " + interactor);
        // Debug.Log("Interacor select active state: " + interactor.isSelectActive);
        // Debug.Log("Selected Interactable: " + interactable);

        // Debug.Log("Interactor position (Global): " + worldIntersectionPoint);
        // Debug.Log("Interactor position: (Local)" + webViewCoordNormalized);
    }

    /// <inheritdoc />
    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        base.OnSelectExiting(args);
        Debug.Log("On Last Select Exiting");
    }


    /// <inheritdoc />
    protected override void OnLastSelectExited(SelectExitEventArgs args)
    {
        base.OnLastSelectExited(args);
        FireMouseUpEvent(webViewCoordNormalized);
        Debug.Log("On Last Select Exited");
    }


    private void FireMouseDownEvent(Vector2 webViewCoordNormalized)
    {
        // Ensure the WebView supports mouse events
        IWithMouseEvents mouseEventsWebView = _webView as IWithMouseEvents;

        if (mouseEventsWebView == null)
        {
            Debug.LogWarning("WebView does not support mouse events");
            return;
        }

        // Convert coordinates to WebView pixel space
        float pixelX = webViewCoordNormalized.x * (float)(_webView.Width);
        float pixelY = webViewCoordNormalized.y * (float)(_webView.Height);

        // Create WebView mouse event data
        WebViewMouseEventData mouseEvent = new WebViewMouseEventData
        {
            X = (int)pixelX,
            Y = (int)pixelY,
            Device = WebViewMouseEventData.DeviceType.Pointer,
            Type = WebViewMouseEventData.EventType.MouseDown,
            TertiaryAxisDeviceType = WebViewMouseEventData.TertiaryAxisDevice.PointingDevice,
        };

        // Propagate the event to the WebView plugin
        mouseEventsWebView.MouseEvent(mouseEvent);

        // mouseEvent.Type = WebViewMouseEventData.EventType.MouseUp;
        // mouseEventsWebView.MouseEvent(mouseEvent);
    }

    private void FireMouseUpEvent(Vector2 webViewCoordNormalized)
    {
        // Ensure the WebView supports mouse events
        IWithMouseEvents mouseEventsWebView = _webView as IWithMouseEvents;

        if (mouseEventsWebView == null)
        {
            Debug.LogWarning("WebView does not support mouse events");
            return;
        }

        // Convert coordinates to WebView pixel space
        float pixelX = webViewCoordNormalized.x * (float)(_webView.Width);
        float pixelY = webViewCoordNormalized.y * (float)(_webView.Height);

        // Create WebView mouse event data
        WebViewMouseEventData mouseEvent = new WebViewMouseEventData
        {
            X = (int)pixelX,
            Y = (int)pixelY,
            Device = WebViewMouseEventData.DeviceType.Pointer,
            Type = WebViewMouseEventData.EventType.MouseUp,
            TertiaryAxisDeviceType = WebViewMouseEventData.TertiaryAxisDevice.PointingDevice
        };

        // Propagate the event to the WebView plugin
        mouseEventsWebView.MouseEvent(mouseEvent);
    }

    // Update is called once per frame
    void UpdateDummy()
    {
        // if (handsAggregatorSubsystem == null) return;

        // Query pinch characteristics from the left hand.
        // pinchAmount is [0,1], normalized to the open/closed thresholds specified in the Aggregator configuration.
        // "isReadyToPinch" is adjusted with the HandRaiseCameraFOV and HandFacingAwayTolerance settings in the configuration.
        // bool handIsValid = handsAggregatorSubsystem.TryGetPinchProgress(XRNode.LeftHand, out bool isReadyToPinch, out bool isPinching, out float pinchAmount);
        // bool entireHandIsValid = handsAggregatorSubsystem.TryGetEntireHand(XRNode.LeftHand, out IReadOnlyList<HandJointPose> handJointPoses);

        // Debug.Log("Is Hovered: " + interactable.isHovered);
        // Debug.Log("Hovering Interactors: " + interactable.hoveringInteractors);


        // Debug.Log("Selectedness: " + interactable.GetSelectionProgress());
        // Debug.Log("Start Push Plane: " + interactable.StartPushPlane);
        // Debug.Log("End Push Plane: " + interactable.EndPushPlane);

        /*
        if (isPinching)
        {
            Debug.Log("Pinch progress: " + pinchAmount);
            Debug.Log("Ready to pinch: " + isReadyToPinch);
            Debug.Log("Is Pinching: " + isPinching);
        }
        */

        // MRTKRayInteractor[] rayInteractors = FindObjectsOfType<MRTKRayInteractor>();
        // GazePinchInteractor[] gazePinchInteractors = FindObjectsOfType<GazePinchInteractor>();
        // MRTKBaseInteractable[] interactables = FindObjectsOfType<MRTKBaseInteractable>();

        /*
        foreach (GazePinchInteractor gazePinchInteractor in gazePinchInteractors)
        {
            if (gazePinchInteractor is IHandedInteractor handInteractor)
            {
                Debug.Log($"GazePinch Interactor Hand: {handInteractor.Handedness}");
                // bool isHitInfoFound = gazePinchInteractor.TryGetHitInfo(out Vector3 position, out Vector3 normal, out int positionInLine, out bool isValidTarget);
                Debug.Log($"Interactor Hand: {handInteractor.Handedness}");
                Debug.Log($"Interactor Target Hit at position: {gazePinchInteractor.attachTransform.position}");
            }
        }
        */

        /*
        foreach (MRTKRayInteractor rayInteractor in rayInteractors)
        {
            if (rayInteractor is IHandedInteractor handInteractor)
            {
                bool isHitInfoFound = rayInteractor.TryGetHitInfo(out Vector3 position, out Vector3 normal, out int positionInLine, out bool isValidTarget);
                if (isValidTarget && isReadyToPinch)
                {
                    if (true)
                    {
                        Debug.Log($"Interactor Hand: {handInteractor.Handedness}");
                        Debug.Log($"Interactor Target Hit Status: {isHitInfoFound}, Target Valid?: {isValidTarget}, at position: {position}");
                        Debug.Log($"Interactor Target Hit Status: {isHitInfoFound}, Target Valid?: {isValidTarget}, Attach Transform: {rayInteractor.attachTransform.position}");
                        break;
                    }
                }
            }
        }
        */


        //bool isPhysicalData = aggregator.subsystemDescriptor.IsPhysicalData;
        //Debug.Log("Is Physical Data: " + isPhysicalData);
    }
}
