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
using System.Collections;


public class FarRaySwipeDetector : MonoBehaviour
{
    [Header("Swipe Parameters")]
    [SerializeField]
    private float minVerticalDelta = 0.2f;
    [SerializeField]
    private float maxVerticalDelta = 0.7f;
    [SerializeField]
    private float maxHorizontalDelta = 0.05f;
    [SerializeField]
    private float minSwipeTime = 0.05f;
    [SerializeField]
    private float maxSwipeTime = 1.0f;
    [SerializeField]
    private float velocityThreshold = 1.0f; // Helps distinguish swipes from casual movements

    [Header("Cooldown Settings")]
    [SerializeField]
    private float cooldownDuration = 2.0f;

    public event Action<SwipeDirection, float, Vector2> OnSwipeDetected;

    private Vector2 lastPosition;
    private float moveStartTime;
    private bool isTracking;
    private bool isSwiping;
    private Vector2 startPosition;
    private Vector2 velocity;
    private float lastUpdateTime;
    private bool isInCooldown;
    private Coroutine cooldownCoroutine;

    private WebView webViewComponent;
    private Transform webViewTransform;
    private MRTKRayInteractor hoverInteractor;

    // Separate struct to hold pending swipe data
    private struct PendingSwipe
    {
        public SwipeDirection Direction;
        public bool IsValid;
        public float distance;
        public Vector2 position;
    }
    private PendingSwipe pendingSwipe;

    public enum SwipeDirection
    {
        Up,
        Down
    }

    StatefulInteractable interactable;
    SwipeDirection direction;

    // Start is called before the first frame update
    void Start()
    {
        // Get the WebView component attached to the game object
        webViewComponent = gameObject.GetComponent<WebView>();
        webViewTransform = gameObject.GetComponent<Transform>();

        // Grab reference to the interactable component
        interactable = gameObject.GetComponent<StatefulInteractable>();

        // Only subscribe to hover events
        interactable.hoverEntered.AddListener(OnHoverEntered);
        interactable.hoverExited.AddListener(OnHoverExited);
    }

    private Vector2 GetRayInteractorAttachTransform(MRTKRayInteractor rayInteractor)
    {
        Vector3 worldIntersectionPoint = GetRayInteractorIntersection(rayInteractor);
        Vector3 localIntersectionPoint = webViewTransform.InverseTransformPoint(worldIntersectionPoint);
        Vector2 attachTransform = new Vector2(localIntersectionPoint.x + 0.5f, 0.5f - localIntersectionPoint.y);
        return attachTransform;
    }

    private Vector3 GetRayInteractorIntersection(MRTKRayInteractor rayInteractor)
    {
        Vector3 worldPosition = Vector3.zero;
        RaycastHit hitInfo;

        // Try to get hit information
        if (rayInteractor.TryGetCurrent3DRaycastHit(out hitInfo))
        {
            worldPosition = hitInfo.point;
        }
        else
        {
            Debug.Log("Failed to get hit information");
        }

        return worldPosition;
    }

    private bool isHoveringInsideWebView(Vector2 position)
    {
        float posX = position.x;
        float posY = position.y;
        bool isInside = false;
        if ((posX > 0f) && (posX < 1f) && (posY > 0f) && (posY < 1f))
        {
            isInside = true;
        }
        return isInside;
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        try
        {
            hoverInteractor = (MRTKRayInteractor)args.interactor;
            Vector2 hitPoint = GetRayInteractorAttachTransform(hoverInteractor);

            if (isHoveringInsideWebView(hitPoint))
            {
                // Initialize tracking
                startPosition = hitPoint;
                lastPosition = hitPoint;
                moveStartTime = Time.time;
                lastUpdateTime = Time.time;
                isTracking = true;
            }
        }
        catch (InvalidCastException ex)
        {
            // Log the exception details for debugging
            Debug.LogWarning($"Attempted to process non-ray interactor of type: {args.interactor.GetType().Name}");
            return; // Exit the method or continue with alternative logic
        }
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        // Only reset tracking-related variables
        isTracking = false;
        velocity = Vector2.zero;
        // Note: We don't reset the cooldown or pending swipe here    }
    }

    private void Update()
    {
        if (!isTracking) return;
        if (hoverInteractor == null) return;

        Vector2 currentHitPoint = GetRayInteractorAttachTransform(hoverInteractor);

        if (isHoveringInsideWebView(currentHitPoint))
        {
            float deltaTime = Time.time - lastUpdateTime;
            if (deltaTime > 0)
            {
                // Calculate velocity
                velocity = (currentHitPoint - lastPosition) / deltaTime;

                // Check for swipe
                CheckSwipe(currentHitPoint);

                lastPosition = currentHitPoint;
                lastUpdateTime = Time.time;
            }
        }
    }

    private void CheckSwipe(Vector2 currentPosition)
    {
        float deltaTime = Time.time - moveStartTime;

        // Reset tracking if we've exceeded the max time
        if (deltaTime > maxSwipeTime)
        {
            StartNewTracking(currentPosition);
            return;
        }

        // Calculate movement deltas
        float deltaX = Mathf.Abs(currentPosition.x - startPosition.x);
        float deltaY = currentPosition.y - startPosition.y;
        float absDeltaY = Mathf.Abs(deltaY);

        // Check if the movement meets swipe criteria
        if (deltaTime >= minSwipeTime &&
            absDeltaY >= minVerticalDelta &&
            deltaX <= maxHorizontalDelta &&
            Mathf.Abs(velocity.y) >= velocityThreshold)
        {
            if (!isSwiping)
            {
                direction = deltaY > 0 ? SwipeDirection.Down : SwipeDirection.Up;
                isSwiping = true;
            }
        }
        else if (deltaTime >= minSwipeTime &&
                 absDeltaY >= minVerticalDelta &&
                 isSwiping)
        {
            float distance = Math.Min(absDeltaY - minVerticalDelta, Math.Abs(maxVerticalDelta - minVerticalDelta)) / (maxVerticalDelta - minVerticalDelta);
            QueueSwipe(direction, distance, startPosition);

            // Start tracking new potential swipe
            StartNewTracking(currentPosition);
        }
        // If we've exceeded the horizontal threshold, start tracking a new potential swipe
        else if (deltaX > maxHorizontalDelta)
        {
            StartNewTracking(currentPosition);
        }
    }

    private void QueueSwipe(SwipeDirection direction, float swipeDelta, Vector2 swipePosition)
    {
        // Store the swipe information
        pendingSwipe = new PendingSwipe
        {
            Direction = direction,
            IsValid = true,
            distance = swipeDelta,
            position = swipePosition
        };

        // Start or restart the cooldown
        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
        }
        cooldownCoroutine = StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        isInCooldown = true;

        // Wait for the cooldown period
        yield return new WaitForSeconds(cooldownDuration);

        // If we have a valid pending swipe, fire the event
        if (pendingSwipe.IsValid)
        {
            OnSwipeDetected?.Invoke(pendingSwipe.Direction, pendingSwipe.distance, pendingSwipe.position);
            pendingSwipe.IsValid = false; // Clear the pending swipe
        }

        cooldownCoroutine = null;
    }

    private void StartNewTracking(Vector2 position)
    {
        startPosition = position;
        lastPosition = position;
        moveStartTime = Time.time;
        lastUpdateTime = Time.time;
        isSwiping = false;
    }


    private void OnDisable()
    {
        // Full cleanup only when component is disabled
        isTracking = false;
        isSwiping = false;
        velocity = Vector2.zero;

        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = null;
        }

        pendingSwipe = new PendingSwipe { IsValid = false };
    }

    private void OnDestroy()
    {
        // Ensure we clean up all coroutines
        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
        }
    }

}
