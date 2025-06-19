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


public class ClickInteractable : MonoBehaviour
{
    [Header("Click Lag")]
    [SerializeField]
    private float cooldownDuration = 2.0f;

    private float selectEnteredTime;
    private float selectExitedTime;
    private bool isSelectEntered = false;
    private bool isSelectExited = false;
    private bool isClickFired = false;

    public event Action<Vector2> OnClickDetected;

    private bool isInCooldown;
    private Coroutine cooldownCoroutine;

    private WebView webViewComponent;
    private Transform webViewTransform;
    Vector2 currentClickCoord;
    StatefulInteractable interactable;

    // Start is called before the first frame update
    void Start()
    {
        // Get the WebView component attached to the game object
        webViewComponent = gameObject.GetComponent<WebView>();
        webViewTransform = gameObject.GetComponent<Transform>();

        // Grab reference to the interactable component
        interactable = gameObject.GetComponent<StatefulInteractable>();

        // Only subscribe to select events
        interactable.selectEntered.AddListener(OnSelectEntered);
        interactable.selectExited.AddListener(OnSelectExited);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        selectEnteredTime = Time.time;
        isSelectEntered = true;
        var interactor = args.interactor;
        var worldIntersectionPoint = interactor.attachTransform.position;
        Vector3 localIntersectionPoint = webViewTransform.InverseTransformPoint(worldIntersectionPoint);
        currentClickCoord = new Vector2(localIntersectionPoint.x + 0.5f, 0.5f - localIntersectionPoint.y);
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        isSelectExited = true;
        isClickFired = false;
        selectExitedTime = Time.time;
    }

    private void Update()
    {
        CheckPinchGesture();
    }

    private void CheckPinchGesture()
    {
        if ((isSelectEntered && isSelectExited) && !isClickFired)
        {
            // Start or restart the cooldown
            if (cooldownCoroutine != null)
            {
                StopCoroutine(cooldownCoroutine);
            }
            cooldownCoroutine = StartCoroutine(CooldownRoutine());
        }
    }

    private IEnumerator CooldownRoutine()
    {
        isInCooldown = true;

        isSelectEntered = false;
        isSelectExited = false;

        // Wait for the cooldown period
        yield return new WaitForSeconds(cooldownDuration);

        // fire the click event 
        OnClickDetected?.Invoke(currentClickCoord);

        isClickFired = true;

        cooldownCoroutine = null;
    }

    private void OnDisable()
    {
        // Full cleanup only when component is disabled
        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = null;
        }
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
