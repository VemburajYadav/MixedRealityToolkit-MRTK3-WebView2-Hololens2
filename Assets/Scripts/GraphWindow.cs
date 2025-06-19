using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.UX;
using MixedReality.Toolkit.Diagnostics;
using MixedReality.Toolkit.Subsystems;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Unity.Profiling;
using System.Threading.Tasks;
using MixedReality.Toolkit.UX.Experimental;
using CodeMonkey.Utils;
using TMPro;

public class GraphWindow : MonoBehaviour
{
    [SerializeField]
    private Sprite CircleSprite;

    private RectTransform graphContainer;
    private GameObject[] dataPointObjects;
    private RectTransform labelTemplateX;
    private RectTransform labelTemplateY;

    private int dataPoints = 3;

    StatefulInteractable graphWindowInteractable;

    private void Awake()
    {
        graphContainer = transform.Find("GraphContainer").GetComponent<RectTransform>();
        labelTemplateX = graphContainer.Find("LabelTemplateX").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("LabelTemplateY").GetComponent<RectTransform>();

        Debug.Log("Transform Position: " + graphContainer.anchoredPosition);
        Debug.Log("Transform SizeDelta: " + graphContainer.sizeDelta);
        Debug.Log("Transform AnchorMin: " + graphContainer.anchorMin);
        Debug.Log("Transform AnchorMax: " + graphContainer.anchorMax);

        gameObject.AddComponent<BoxCollider>();

        List<int> valueList = new List<int>() { 5, 98, 56, 45, 30, 22, 17, 15, 13, 17, 25, 37, 40, 36, 33 };
        ShowGraph(valueList);

        /***
        gameObject.AddComponent<StatefulInteractable>();
        graphWindowInteractable = gameObject.GetComponent<StatefulInteractable>();
        graphWindowInteractable.DisableInteractorType(typeof(IGazeInteractor));
        graphWindowInteractable.DisableInteractorType(typeof(IGazePinchInteractor));
        ***/

        /***
        dataPointObjects = new GameObject[dataPoints];
        dataPointObjects[0] = CreateCircle(new Vector2(0, 0));
        dataPointObjects[1] = CreateCircle(new Vector2(150, 0));
        dataPointObjects[2] = CreateCircle(new Vector2(150, 100));
        ***/
    }

    private GameObject CreateCircle(Vector2 anchoredPosition)
    {
        GameObject gameObject = new GameObject("circle", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = CircleSprite;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(5, 5);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        /***
        gameObject.AddComponent<BoxCollider>();
        gameObject.AddComponent<StatefulInteractable>();

        StatefulInteractable interactable = gameObject.GetComponent<StatefulInteractable>();
        interactable.DisableInteractorType(typeof(IGazeInteractor));
        interactable.DisableInteractorType(typeof(IGazePinchInteractor));
        ***/

        return gameObject;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void ShowGraph(List<int> valueList)
    {
        float xSize = graphContainer.sizeDelta.x / (valueList.Count + 1f);
        float graphHeight = graphContainer.sizeDelta.y;
        float yMaximum = 100f;
        GameObject lastDataPointObject = null;
        for (int i = 0; i < valueList.Count; i++)
        {
            float xPosition = (i + 1) * xSize;
            float yPosition = (valueList[i] / yMaximum) * graphHeight;
            GameObject dataPointObject = CreateCircle(new Vector2(xPosition, yPosition));
            if (lastDataPointObject != null)
            {
                CreateDotConnection(lastDataPointObject.GetComponent<RectTransform>().anchoredPosition, dataPointObject.GetComponent<RectTransform>().anchoredPosition);
            }
            lastDataPointObject = dataPointObject;

            RectTransform labelX = Instantiate(labelTemplateX);
            labelX.SetParent(graphContainer, false);
            labelX.gameObject.SetActive(true);
            labelX.anchoredPosition = new Vector2(xPosition, 0);
            printGameObjectComponents(labelX.gameObject);
            labelX.GetComponent<TextMeshProUGUI>().text = i.ToString();
            //Debug.Log("Text: " + labelX.GetComponent<TextMeshProUGUI>().text);
        }

        int separatorCount = 10;

        for (int i = 0; i <= separatorCount; i++)
        {
            RectTransform labelY = Instantiate(labelTemplateY);
            labelY.SetParent(graphContainer, false);
            labelY.gameObject.SetActive(true);
            float normalizedPosition = (1f * i) / separatorCount;
            float yPosition = normalizedPosition * graphHeight;
            labelY.anchoredPosition = new Vector2(-7f, yPosition);
            labelY.GetComponent<TextMeshProUGUI>().text = Mathf.RoundToInt(normalizedPosition * yMaximum).ToString();

        }
    }

    private void printGameObjectComponents(GameObject gameObject)
    {
        // Get all components attached to this GameObject
        Component[] components = gameObject.GetComponents<Component>();

        Debug.Log($"=== Components on {gameObject.name} ===");

        // Loop through and print each component's type
        for (int i = 0; i < components.Length; i++)
        {
            Debug.Log($"{i + 1}. {components[i].GetType()}");
        }

        Debug.Log($"Total components: {components.Length}");
    }

    private void CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB)
    {
        GameObject gameObject = new GameObject("line", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().color = new Color(0,1,0,0.5f);
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();

        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);
        rectTransform.anchoredPosition = dotPositionA + dir * distance * 0.5f;
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, 1);
        rectTransform.localEulerAngles = new Vector3(0, 0, UtilsClass.GetAngleFromVectorFloat(dir));

    }
    // Update is called once per frame
    void Update()
    {
        /***

        for (int i = 0; i < dataPoints; i++)
        {
            GameObject dataPointObj = dataPointObjects[i];

            StatefulInteractable pointInteractable = dataPointObj.GetComponent<StatefulInteractable>();

            var pointColliders = pointInteractable.colliders;

            if (pointInteractable.isSelected)
            {
                Debug.Log("Data Point Collider: " + dataPointObj.GetComponent<BoxCollider>());
                Debug.Log("Data Point Interactable Collider: " + pointColliders[0]);
                Debug.Log("Data Point (Is Selected): " + pointInteractable.isSelected);
                Debug.Log("Data Point (Anchored Position): " + dataPointObj.GetComponent<RectTransform>().anchoredPosition);
            }

        }
        ***/

        /***
        StatefulInteractable graphWindowInteractable = gameObject.GetComponent<StatefulInteractable>();
        var graphWindowColliders = graphWindowInteractable.colliders;

        if (graphWindowInteractable.isHovered)
        {
            Debug.Log("Graph Window Collider: " + gameObject.GetComponent<BoxCollider>());
            Debug.Log("Graph Window Interactable Collider: " + graphWindowColliders[0]);
            Debug.Log("Graph Window (Is Selected): " + graphWindowInteractable.isSelected);
        }
        ***/
    }
}
