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
using System.Threading.Tasks;
using MixedReality.Toolkit.UX.Experimental;
//using UnityEngine.XR.Hands;


//public interface IWithMouseEvents : IWithInputEvents
//{
//    void MouseEvent(WebViewMouseEventData mouseEvent);
//}


public class WebViewBrowser : MonoBehaviour, IPointerClickHandler, IScrollHandler
{
    // Declare UI elements: Back button, Go button, and URL input field
    public Button BackButton;
    public Button GoButton;
    public TMP_InputField URLField;
    private WebView webViewComponent;
    private Transform webViewTransform;
    private Plane viewportPlane;
    private IWebView _webView;
    private float iWebViewHeight;
    private float iWebViewWidth;
    private StatefulInteractable interactable;
    private Vector2 webViewCoordNormalized;
    private IWithMouseEvents mouseEventsWebView;
    private bool isSelectEntered = false;
    private bool isSelectExited = false;
    private bool isClickFired = false;
    private float lastClickTime;
    private float timeElapsed;
    private float selectEnteredTime;
    private float selectExitedTime;
    private float timeDelayClickEvent = 1.0f;
    private TouchScreenKeyboard keyboard;

    private BoxCollider collider;

    [SerializeField]
    public float scrollMultiplier = 50f;

    [SerializeField]
    public bool EnableMouseClicks = true;

    [SerializeField]
    public bool EnableHandInteractions = true;

    [SerializeField]
    public bool EnableMouseScrolls = false;

    private void Start()
    {
        // Get the WebView component attached to the game object
        webViewComponent = gameObject.GetComponent<WebView>();
        webViewComponent.GetWebViewWhenReady((IWebView webView) =>
        {
            _webView = webView;

            // Get bWebView's properties
            iWebViewHeight = _webView.Height;
            iWebViewWidth = _webView.Width;

            // Ensure the WebView supports mouse events
            IWithMouseEvents mouseEventsWebView = webView as IWithMouseEvents;
            if (mouseEventsWebView == null)
            {
                Debug.LogWarning("WebView does not support mouse events");
                return;
            }

            // If the WebView supports browser history, enable the Back button
            if (webView is IWithBrowserHistory history)
            {
                // Add an event listener for the Back button to navigate back in history
                BackButton.onClick.AddListener(() => history.GoBack());

                // Update the Back button's enabled state based on whether there's any history to go back to
                history.CanGoBackUpdated += CanGoBack;
            }

            // Add an event listener for the Go button to load the URL that was entered in the input field
            GoButton.onClick.AddListener(() => webView.Load(new Uri(URLField.text)));

            // Subscribe to the Navigated event to update the URL input field whenever a navigation occurs
            webView.Navigated += OnNavigated;

            // Set the initial value of the URL input field to the current URL of the WebView
            if (webView.Page != null)
            {
                URLField.text = webView.Page.AbsoluteUri;
            }
        });

        // Get the plane of the WebView
        webViewTransform = gameObject.GetComponent<Transform>();
        viewportPlane = new Plane(webViewTransform.forward, webViewTransform.position);
        collider = gameObject.GetComponent<BoxCollider>();

        // Disable the Grab, Gaze and GazePinch interaction modes 
        interactable = gameObject.GetComponent<StatefulInteractable>();
        interactable.DisableInteractorType(typeof(IGrabInteractor));
        interactable.DisableInteractorType(typeof(IGazeInteractor));
        interactable.DisableInteractorType(typeof(IGazePinchInteractor));
        var hoveringInteractors = interactable.hoveringInteractors;

        foreach (var interactor in hoveringInteractors)
        {
            if (interactor != null)
            {
                interactor.allowHover = false;
            }
        }

        // List of colliders for the statefulInteractable
        List<Collider> colliders = interactable.colliders;

        keyboard = TouchScreenKeyboard.Open("text to edit", TouchScreenKeyboardType.Default);

        // Add Listeners to tthe selectEntered and selectEntered events
        interactable.selectEntered.AddListener(selectArgs =>
        {
            selectEnteredTime = Time.time;
            isSelectEntered = true;
            var interactor = selectArgs.interactor;
            var worldIntersectionPoint = interactor.attachTransform.position;
            Vector3 localIntersectionPoint = webViewTransform.InverseTransformPoint(worldIntersectionPoint);
            webViewCoordNormalized = new Vector2(localIntersectionPoint.x + 0.5f, 0.5f - localIntersectionPoint.y);
            Debug.Log("Select Entered" + interactor);
            Debug.Log("Select Entered Interactor Select Active: " + interactor.isSelectActive);
            Debug.Log("Select Entered Interactor Hover Active: " + interactor.isHoverActive);
            Debug.Log("Select Entered Time: " + selectEnteredTime);
            // Debug.Log("World Intersection Point: " + worldIntersectionPoint);
        });

        interactable.selectExited.AddListener(selectArgs =>
        {
            var interactor = selectArgs.interactor;
            // var colliderObject = gameObject.GetComponent<BoxCollider>();
            // Destroy(colliderObject);
            isSelectExited = true;
            isClickFired = false;
            lastClickTime = Time.time;
            selectExitedTime = Time.time;

            Debug.Log("Select Exited" + interactor);
            Debug.Log("Select Exited Interactor Select Active: " + interactor.isSelectActive);
            Debug.Log("Select Exited Interactor Hover Active: " + interactor.isHoverActive);
            Debug.Log("Select Exited Time: " + selectExitedTime);

        });

        interactable.hoverEntered.AddListener(selectArgs =>
        {
            var interactor = selectArgs.interactor;
            Debug.Log("Hover Entered" + interactor);
            Debug.Log("Hover Entered Interactor Select Active: " + interactor.isSelectActive);
            Debug.Log("Hover Entered Interactor Hover Active: " + interactor.isHoverActive);
            Debug.Log("Hover Entered Time: " + Time.time);

        });

        interactable.hoverExited.AddListener(selectArgs =>
        {
            var interactor = selectArgs.interactor;
            Debug.Log("Hover Exited" + interactor);
            Debug.Log("Hover Exited Interactor Select Active: " + interactor.isSelectActive);
            Debug.Log("Hover Exited Interactor Hover Active: " + interactor.isHoverActive);
            Debug.Log("Hover Exited Time: " + Time.time);
        });
    }

    void Awake()
    {
    }

    void Update()
    {

        /*
        if (interactable.IsRaySelected) Debug.Log("Ray Selected: " + interactable.IsRaySelected);
        if (interactable.IsRayHovered) Debug.Log("Ray Hovered: " + interactable.IsRayHovered);
        if (interactable.IsPokeHovered) Debug.Log("Poke Hovered: " + interactable.IsPokeHovered);
        if (interactable.IsPokeSelected) Debug.Log("Poke Selected: " + interactable.IsPokeSelected);
        if (interactable.IsGazeHovered) Debug.Log("Gaze Hovered: " + interactable.IsGazeHovered);
        if (interactable.IsGazePinchHovered) Debug.Log("Gaze Pinch Hovered: " + interactable.IsGazePinchHovered);
        if (interactable.IsGazePinchSelected) Debug.Log("Gaze Pinch Selected: " + interactable.IsGazePinchSelected);
        if (interactable.IsActiveHovered) Debug.Log("Active Hovered: " + interactable.IsActiveHovered);
        */


        timeElapsed = Time.time - lastClickTime;

        if (timeElapsed > timeDelayClickEvent)
        {

            if ((isSelectEntered && isSelectExited) && !isClickFired)
            {
                if (EnableHandInteractions)
                {
                    FireMouseDownEvent(webViewCoordNormalized);
                    Debug.Log("Click fired");
                }

                isClickFired = true;
                isSelectEntered = false;
                isSelectExited = false;
            }
        }
    }


    // Update the URL input field with the new path after navigation
    private void OnNavigated(string path)
    {
        URLField.text = path;
    }

    // Enable or disable the Back button based on whether there's any history to go back to
    private void CanGoBack(bool value)
    {
        BackButton.enabled = value;
    }

    // Conversion method to translate Unity screen coordinates to WebView coordinate space
    private Vector2 ConvertToWebViewSpace(float screenX, float screenY)
    {
        //Transform transform = _webView.GetComponent<Transform>();

        // Cast a ray from the camera passing through the world point corresponding to the point clicked on the screen
        Vector2 screenPosition = new Vector2(screenX, screenY);
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);

        // Find the ray's intersection point on the webview plane
        if (viewportPlane.Raycast(ray, out float distance))
        {
            // Get the world point where the ray intersects the plane
            Vector3 worldPoint = ray.GetPoint(distance);
            Debug.Log("Clicked Point (Global): " + worldPoint);

            // Convert world space point to local space of the WebView
            Vector3 localPoint = webViewTransform.InverseTransformPoint(worldPoint);
            Debug.Log("Clicked Point (Local): " + localPoint);

            // Get the collider dimensions
            // var boundsSize = collider.sharedMesh.bounds.size;
            // var boundsExtents = collider.sharedMesh.bounds.max;

            // Debug.Log("bounds size: " + boundsSize);
            // Debug.Log("bounds extents: " + boundsExtents);

            // float pixelX = localPointNormalized.x;
            // float pixelY = -1.0f * localPointNormalized.y;

            // float pixelX = (localPoint.x + boundsExtents.x) / boundsSize.x;
            // float pixelY = (-1.0f * (localPoint.y - boundsExtents.y)) / boundsSize.y;
            // pixelX = Mathf.Clamp(pixelX, 0, webViewWidth);
            // pixelY = Mathf.Clamp(pixelY, 0, webViewHeight);

            float pixelX = localPoint.x + 0.5f;
            float pixelY = 0.5f - localPoint.y;

            Vector2 webViewCoord = new Vector2(pixelX, pixelY);
            Debug.Log("Web View Coordinates (Normalized): " + webViewCoord);

            return webViewCoord;
        }

        Debug.LogWarning("Could not convert screen point to WebView pixel coordinates");
        return Vector2.zero;
    }

    private void FireMouseDownEvent(Vector2 webViewCoordNormalized)
    {
        // Convert coordinates to WebView pixel space
        float pixelX = webViewCoordNormalized.x * (float)(_webView.Width);
        float pixelY = webViewCoordNormalized.y * (float)(_webView.Height);

        IWithMouseEvents mouseEventsWebView = _webView as IWithMouseEvents;

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

        mouseEvent.Type = WebViewMouseEventData.EventType.MouseUp;
        mouseEventsWebView.MouseEvent(mouseEvent);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("IWebView (PointerDown) Width: " + _webView.Width);
        Debug.Log("IWebView (PointerDown) Height: " + _webView.Height);

        IWithMouseEvents mouseEventsWebView = _webView as IWithMouseEvents;

        // Convert screen coordinates to WebView space
        var hitCoord = ConvertToWebViewSpace(eventData.position.x, eventData.position.y);

        float pixelX = hitCoord.x * (float)(_webView.Width);
        float pixelY = hitCoord.y * (float)(_webView.Height);
        Debug.Log("Web View Coordinates (Pixels): " + new Vector2(pixelX, pixelY));

        // Create WebView mouse event data
        WebViewMouseEventData mouseEvent = new WebViewMouseEventData
        {
            X = (int)pixelX,
            Y = (int)pixelY,
            Device = WebViewMouseEventData.DeviceType.Pointer,
            Type = WebViewMouseEventData.EventType.MouseDown,
            Button = WebViewMouseEventData.MouseButton.ButtonLeft,
            TertiaryAxisDeviceType = WebViewMouseEventData.TertiaryAxisDevice.PointingDevice
        };

        // Propagate the event to the WebView plugin
        if (EnableMouseClicks)
        {
            mouseEventsWebView.MouseEvent(mouseEvent);
        }

        // To register as a click, the WebView needs to be a mouse-up event.
        mouseEvent.Type = WebViewMouseEventData.EventType.MouseUp;
        if (EnableMouseClicks)
        {
            mouseEventsWebView.MouseEvent(mouseEvent);
        }
    }

    public void OnScroll(PointerEventData eventData)
    {
        Debug.Log("Scroll Event with Delta: " + eventData.scrollDelta);

        IWithMouseEvents mouseEventsWebView = _webView as IWithMouseEvents;

        float scrollX = eventData.scrollDelta.x;
        float scrollY = eventData.scrollDelta.y;

        scrollX = 0f;
        scrollY = -0.5f;
        // Create WebView mouse event data
        WebViewMouseEventData mouseEvent = new WebViewMouseEventData
        {
            WheelX = scrollX * scrollMultiplier,
            WheelY = scrollY * scrollMultiplier,
            Type = WebViewMouseEventData.EventType.MouseWheel,
            Button = WebViewMouseEventData.MouseButton.ButtonMiddle,
            TertiaryAxisDeviceType = WebViewMouseEventData.TertiaryAxisDevice.PointingDevice
        };

        // Propagate the event to the WebView plugin
        if (EnableMouseScrolls)
        {
            mouseEventsWebView.MouseEvent(mouseEvent);
        }
    }

    public void OnPointerDragged(PointerEventData eventData) { }
}
