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


public class WebViewBrowser : MonoBehaviour
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

    private int numClicksMin = 5;
    private int numClicksMax = 40;
    private TouchScreenKeyboard keyboard;

    private BoxCollider collider;

    [SerializeField]
    public bool EnableHandGestureClicks = true;

    [SerializeField]
    public bool EnableHandGestureSwipes = true;


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

        // List of colliders for the statefulInteractable
        // List<Collider> colliders = interactable.colliders;

        var swipeInteractable = GetComponent<FarRaySwipeInteractable>();
        if (swipeInteractable != null)
        {
            swipeInteractable.OnSwipeDetected += HandleSwipe;
        }

        var clickInteractable = GetComponent<ClickInteractable>();
        if (clickInteractable != null)
        {
            clickInteractable.OnClickDetected += HandleClick;
        }
    }

    private void HandleSwipe(FarRaySwipeInteractable.SwipeDirection direction, float distance, Vector2 startPosition)
    {
        Debug.Log("start position: " + startPosition);

        if (EnableHandGestureSwipes)
        {
            if (direction == FarRaySwipeInteractable.SwipeDirection.Up)
            {
                // Fire scroll up event
                Debug.Log("Scroll Down: " + distance);
                ScrollDown(distance);
            }
            else
            {
                // Fire scroll down event
                Debug.Log("Scroll Up: " + distance);
                ScrollUp(distance);
            }
        }
    }

    private void HandleClick(Vector2 webviewCoord)
    {
        if (EnableHandGestureClicks)
        {
            FireClick(webviewCoord);
            Debug.Log("Click fired");
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
        // Cast a ray from the camera passing through the world point corresponding to the point clicked on the screen
        Vector2 screenPosition = new Vector2(screenX, screenY);
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);

        // Find the ray's intersection point on the webview plane
        if (viewportPlane.Raycast(ray, out float distance))
        {
            // Get the world point where the ray intersects the plane
            Vector3 worldPoint = ray.GetPoint(distance);

            // Convert world space point to local space of the WebView
            Vector3 localPoint = webViewTransform.InverseTransformPoint(worldPoint);

            float pixelX = localPoint.x + 0.5f;
            float pixelY = 0.5f - localPoint.y;

            Vector2 webViewCoord = new Vector2(pixelX, pixelY);

            return webViewCoord;
        }

        Debug.LogWarning("Could not convert screen point to WebView pixel coordinates");
        return Vector2.zero;
    }

    private void FireClick(Vector2 webViewCoordNormalized)
    {
        webViewComponent = gameObject.GetComponent<WebView>();
        webViewComponent.GetWebViewWhenReady((IWebView webView) =>
        {
            // Get bWebView's properties
            iWebViewHeight = webView.Height;
            iWebViewWidth = webView.Width;

            // Ensure the WebView supports mouse events
            IWithMouseEvents mouseEventsWebView = webView as IWithMouseEvents;
            if (mouseEventsWebView == null)
            {
                Debug.LogWarning("WebView does not support mouse events");
                return;
            }

            // Convert coordinates to WebView pixel space
            float pixelX = webViewCoordNormalized.x * (float)(iWebViewWidth);
            float pixelY = webViewCoordNormalized.y * (float)(iWebViewHeight);

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

        });

    }


    public void ScrollDown(float distance)
    {
        IWithMouseEvents mouseEventsWebView = _webView as IWithMouseEvents;

        float pixelX = 0.991f * (float)(_webView.Width);
        float pixelY = 0.989f * (float)(_webView.Height);

        int numClicks = (int)((float)numClicksMin + (float)numClicksMax * distance);
        Debug.Log("Num Clicks (Scroll Down): " + numClicks);

        for (int i = 0; i < numClicks; i++)
        {
            // Create WebView mouse event data
            WebViewMouseEventData mouseEvent = new WebViewMouseEventData
            {
                X = (int)pixelX,
                Y = (int)pixelY,
                Device = WebViewMouseEventData.DeviceType.Pointer,
                Type = WebViewMouseEventData.EventType.MouseDown,
                Button = WebViewMouseEventData.MouseButton.ButtonLeft,
                TertiaryAxisDeviceType = WebViewMouseEventData.TertiaryAxisDevice.PointingDevice,
            };

            // Propagate the event to the WebView plugin
            mouseEventsWebView.MouseEvent(mouseEvent);

            // To register as a click, the WebView needs to be a mouse-up event.
            mouseEvent.Type = WebViewMouseEventData.EventType.MouseUp;
            mouseEventsWebView.MouseEvent(mouseEvent);
        }
    }

    public void ScrollUp(float distance)
    {
        IWithMouseEvents mouseEventsWebView = _webView as IWithMouseEvents;

        float pixelX = 0.991f * (float)(_webView.Width);
        float pixelY = 0.0131f * (float)(_webView.Height);

        int numClicks = (int)((float)numClicksMin + (float)numClicksMax * distance);
        Debug.Log("Num Clicks (Scroll Up): " + numClicks);

        for (int i = 0; i < numClicks; i++)
        {
            // Create WebView mouse event data
            WebViewMouseEventData mouseEvent = new WebViewMouseEventData
            {
                X = (int)pixelX,
                Y = (int)pixelY,
                Device = WebViewMouseEventData.DeviceType.Pointer,
                Type = WebViewMouseEventData.EventType.MouseDown,
                Button = WebViewMouseEventData.MouseButton.ButtonLeft,
                TertiaryAxisDeviceType = WebViewMouseEventData.TertiaryAxisDevice.PointingDevice,
            };

            // Propagate the event to the WebView plugin
            mouseEventsWebView.MouseEvent(mouseEvent);

            // To register as a click, the WebView needs to be a mouse-up event.
            mouseEvent.Type = WebViewMouseEventData.EventType.MouseUp;
            mouseEventsWebView.MouseEvent(mouseEvent);
        }
    }

    /***
    public void ScrollUpWheel(Vector2 position)
    {

        IWithMouseEvents mouseEventsWebView = _webView as IWithMouseEvents;

        // Convert coordinates to WebView pixel space
        float pixelX = position.x * (float)(iWebViewWidth);
        float pixelY = position.y * (float)(iWebViewHeight);

        // Create WebView mouse event data
        WebViewMouseEventData mouseEvent = new WebViewMouseEventData
        {
            WheelX = 0,
            WheelY = 0,
            Device = WebViewMouseEventData.DeviceType.Pointer,
            Type = WebViewMouseEventData.EventType.MouseWheel,
            Button = WebViewMouseEventData.MouseButton.ButtonMiddle,
            TertiaryAxisDeviceType = WebViewMouseEventData.TertiaryAxisDevice.PointingDevice,
            X = (int)pixelX,
            Y = (int)pixelY
        };

        // Propagate the event to the WebView plugin
        if (EnableMouseScrolls)
        {
            mouseEventsWebView.MouseEvent(mouseEvent);
        }

    }

    public void ScrollDownWheel(Vector2 position)
    {
        IWithMouseEvents mouseEventsWebView = _webView as IWithMouseEvents;

        // Convert coordinates to WebView pixel space
        float pixelX = position.x * (float)(iWebViewWidth);
        float pixelY = position.y * (float)(iWebViewHeight);

        // Create WebView mouse event data
        WebViewMouseEventData mouseEvent = new WebViewMouseEventData
        {
            WheelX = 0,
            WheelY = 0,
            Device = WebViewMouseEventData.DeviceType.Pointer,
            Type = WebViewMouseEventData.EventType.MouseWheel,
            Button = WebViewMouseEventData.MouseButton.ButtonMiddle,
            TertiaryAxisDeviceType = WebViewMouseEventData.TertiaryAxisDevice.PointingDevice,
            X = (int)pixelX,
            Y = (int)pixelY,
        };

        // Propagate the event to the WebView plugin
        if (EnableMouseScrolls)
        {
            mouseEventsWebView.MouseEvent(mouseEvent);
        }
 
    }
    ***/

    public void OnPointerDragged(PointerEventData eventData) { }

    void Update() { }
}
