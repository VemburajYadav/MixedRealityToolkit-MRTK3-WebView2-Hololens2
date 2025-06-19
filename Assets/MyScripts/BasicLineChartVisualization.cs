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
using System.IO;
using System.Globalization;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Unity.Profiling;
using System.Threading.Tasks;
using MixedReality.Toolkit.UX.Experimental;
using CodeMonkey.Utils;
using TMPro;
using DataUtils;
using VisualizationUtils;

public class BasicLineChartVisualization : MonoBehaviour
{
    [SerializeField]
    GameObject descriptionGameObject;

    [SerializeField]
    GameObject xLabelGameObject;

    [SerializeField]
    GameObject yLabelGameObject;

    [SerializeField]
    GameObject dataPointTemplate;

    [SerializeField]
    GameObject lineChartTemplate;
    
    [SerializeField]
    GameObject separatorTemplateX;

    [SerializeField]
    GameObject separatorTemplateY;

    // GameObject Transform Variables
    private RectTransform separatorTransformX;
    private RectTransform separatorTransformY;
    private RectTransform dataPointTransform;
    private RectTransform lineChartTransform;
    private RectTransform axisTransformX;
    private RectTransform axisTransformY;

    // Graph Variables
    private Vector2 xRange;
    private Vector2 yRange;

    private int numDataPoints;

    private int numSeparatorsX;
    private int numSeparatorsY;

    private float graphWidth;
    private float graphHeight;

    private float spacingWidth;
    private float spacingHeight;

    private float separatorWidthX;
    private float separatorWidthY;
    private float separatorHeightX;
    private float separatorHeightY;

    private float axisWidthX;
    private float axisHeightX;
    private float axisWidthY;
    private float axisHeightY;

    private Dictionary<string, Vector2> spacingDict = new Dictionary<string, Vector2>();
    private Dictionary<string, Vector2> rangeXDict = new Dictionary<string, Vector2>();
    private Dictionary<string, Vector2> rangeYDict = new Dictionary<string, Vector2>();

    private Vector2 graphCoordRangeX;
    private Vector2 graphCoordRangeY;

    private List<Vector2> dataPointCoords;
    private List<GameObject> dataPointObjects;
    private string filePath;

    // CSV Data Variables
    private CSVDataLoader dataLoader;
    private Dictionary<string, CSVDataSet> datasets;
    private CSVDataSet plotDataset;
    private string plotFileName;
    private List<string> plotHeaders;
    private List<string> plotAttributes;
    private List<string> fileNames;

    private List<float> plotDataPointsX;
    private List<float> plotDataPointsY;

    // Line Chart Variables
    private List<LineChartData> lineChart;
    private Dictionary<string, Dictionary<string, LineChartData>> lineChartDict = new Dictionary<string, Dictionary<string, LineChartData>>();
    private Dictionary<string, Dictionary<string, GameObject>> lineChartGameObjects = new Dictionary<string, Dictionary<string, GameObject>>();
    private Dictionary<string, Dictionary<string, List<GameObject>>> dataPointGameObjects = new Dictionary<string, Dictionary<string, List<GameObject>>>();
    private Dictionary<string, Dictionary<string, List<GameObject>>> dataLineGameObjects = new Dictionary<string, Dictionary<string, List<GameObject>>>();
    private Dictionary<string, Dictionary<string, Color>> colorDict = new Dictionary<string, Dictionary<string, Color>>();
    private Dictionary<string, Dictionary<string, List<GameObject>>> separatorXGameObjects = new Dictionary<string, Dictionary<string, List<GameObject>>>();
    private Dictionary<string, Dictionary<string, List<GameObject>>> separatorYGameObjects = new Dictionary<string, Dictionary<string, List<GameObject>>>();
    private List<string> descriptions = new List<string>();

    // GameObjects being displayed
    private string currentDataset;
    private string currentHeader;

    private void Awake()
    {

        separatorTransformX = separatorTemplateX.GetComponent<RectTransform>();
        separatorTransformY = separatorTemplateY.GetComponent<RectTransform>();
        dataPointTransform = dataPointTemplate.GetComponent<RectTransform>();
        lineChartTransform = lineChartTemplate.GetComponent<RectTransform>();
        axisTransformX = separatorTransformX.parent.gameObject.GetComponent<RectTransform>();
        axisTransformY = separatorTransformY.parent.gameObject.GetComponent<RectTransform>();

        numSeparatorsX = 11;
        numSeparatorsY = 11;

        // Load the data from the csv data files
        dataLoader = new CSVDataLoader();
        dataLoader.LoadAllCSVFiles();
        datasets = dataLoader.dataSets;

        // dataset filenames 
        fileNames = datasets.Keys.ToList();
        int fileCount = fileNames.Count;

        int datasetIndex = UnityEngine.Random.Range(0, fileCount);
        int headerCount = datasets[fileNames[datasetIndex]].headers.Count - 1;
        int headerIndex = UnityEngine.Random.Range(0, headerCount);


        hardCodeDescriptions();
        setTitle(descriptions[datasetIndex]);
        setXLabel(datasets[fileNames[datasetIndex]].headers[0]);
        setYLabel(datasets[fileNames[datasetIndex]].headers[headerIndex + 1]);
        ComputeGraphAttributes();
        SampleColorsForCharts();
        CreateAllLineCharts();
        CreateAllSeparatorXObjects();
        CreateAllSeparatorYObjects();
        CreateAllLineChartGameObjects();
        ShowPointMetadataOnHover();
        ShowLineChart(fileNames[datasetIndex], datasets[fileNames[datasetIndex]].headers[headerIndex + 1]);


    }

    private void hardCodeDescriptions()
    {
        string title1 = "Impact Of Batch Size On Model Latency";
        string title2 = "Impact Of Model Architecture On Classification Accuracy";
        string title3 = "Memory Requirements for Different Model Variants";
        descriptions.Insert(0, title1);
        descriptions.Insert(1, title2);
        descriptions.Insert(2, title3);
    }

    private void setTitle(string text)
    {
        descriptionGameObject.GetComponent<TextMeshProUGUI>().text = text;
    }

    private void setXLabel(string text)
    {
        xLabelGameObject.GetComponent<TextMeshProUGUI>().text = string.Join(" ", text.Split('_').Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
    }

    private void setYLabel(string text)
    {
        yLabelGameObject.GetComponent<TextMeshProUGUI>().text = string.Join(" ", text.Split('_').Select(word => char.ToUpper(word[0]) + word.Substring(1)));
    }

    private void ShowLineChart(string datasetName, string header)
    {
        ActivateSeparators(datasetName, header);
        lineChartGameObjects[datasetName][header].SetActive(true);
    }

    private void SampleColorsForCharts()
    {
        for (int i = 0; i < fileNames.Count; i++)
        {
            Dictionary<string, Color> chartColors = new Dictionary<string, Color>();
            for (int j = 0; j < (datasets[fileNames[i]].headers.Count - 1); j++)
            {
                string header = datasets[fileNames[i]].headers[j + 1];
                Color color = new Color(1f - UnityEngine.Random.value * 0.5f, 1f - UnityEngine.Random.value * 0.5f, 1f - UnityEngine.Random.value * 0.5f, 1f);
                chartColors[header] = color;
            }
            colorDict[fileNames[i]] = chartColors;
        }
    }

    private void ComputeGraphAttributes()
    {
        // Graph size
        graphWidth = lineChartTransform.rect.width;
        graphHeight = lineChartTransform.rect.height;

        // Coordinate ranges in units of graph size
        graphCoordRangeX = new Vector2(spacingWidth / 2, graphWidth - (spacingWidth / 2));
        graphCoordRangeY = new Vector2(-spacingHeight / 2, -(graphHeight - (spacingHeight / 2)));

        // X-Axis and separator attributes 
        RectTransform axisTransformX = separatorTransformX.parent.gameObject.GetComponent<RectTransform>();
        axisWidthX = axisTransformX.rect.width;
        axisHeightX = axisTransformX.rect.height;
        separatorWidthX = axisWidthX / numSeparatorsX;
        separatorHeightX = axisHeightX;
        spacingWidth = separatorWidthX;

        // Y-Axis and separator attributes 
        RectTransform axisTransformY = separatorTransformY.parent.gameObject.GetComponent<RectTransform>();
        axisWidthY = axisTransformY.rect.width;
        axisHeightY = axisTransformY.rect.height;
        separatorWidthY = axisWidthY;
        separatorHeightY = axisHeightY / numSeparatorsY;
        spacingHeight = separatorHeightY;

        // Point Coordinate range
        float minX, maxX, minY, maxY;
        for (int i = 0; i < fileNames.Count; i++)
        {
            (minX, maxX, minY, maxY) = dataLoader.GetDatasetMinMax(fileNames[i]);
            Debug.Log($"{minX}, {maxX}, {minY}, {maxY}");
            (minX, maxX) = dataLoader.RoundDynamically(minX, maxX);
            (minY, maxY) = dataLoader.RoundDynamically(minY, maxY);

            rangeXDict[fileNames[i]] = new Vector2(minX, maxX);
            rangeYDict[fileNames[i]] = new Vector2(minY, maxY);
            spacingDict[fileNames[i]] = new Vector2((maxX - minX) / (numSeparatorsX - 1), (maxY - minY) / (numSeparatorsY - 1));
        }
    }


    private void CreateAllLineCharts()
    {
        for (int i = 0; i < fileNames.Count; i++)
        {
            List<string> headers = datasets[fileNames[i]].headers;
            int numAttributes = headers.Count - 1;
            lineChartDict[fileNames[i]] = new Dictionary<string, LineChartData>();

            for (int j = 0; j < numAttributes; j++)
            {
                CreateLineChart(fileNames[i], headers[j + 1], false);
            }
        }
    }

    private void CreateLineChart(string datasetName, string header, bool globalRange)
    {
        CSVDataSet dataset = datasets[datasetName];
        List<string> headers = dataset.headers;
        LineChartData chart = new LineChartData();
        chart.xLabel = headers[0];
        chart.yLabel = header;
        chart.header = header;

        List<float> coordsX = dataset.columns[headers[0]];
        List<float> coordsY = dataset.columns[header];

        int numPoints = coordsX.Count;

        chart.pointCoordsX = coordsX;
        chart.pointCoordsY = coordsY;
        chart.numPoints = numPoints;

        Vector2 xRange, yRange;

        if (globalRange)
        {
            xRange = rangeXDict[datasetName];
            yRange = rangeYDict[datasetName];
        }
        else
        {
            float minX, maxX, minY, maxY;
            (minX, maxX) = dataLoader.GetColumnRange(datasetName, datasets[datasetName].headers[0]);
            (minX, maxX) = dataLoader.RoundDynamically(minX, maxX);
            (minY, maxY) = dataLoader.GetColumnRange(datasetName, header);
            (minY, maxY) = dataLoader.RoundDynamically(minY, maxY);
            xRange = new Vector2(minX, maxX);
            yRange = new Vector2(minY, maxY);

        }

        for (int i = 0; i < numPoints; i++)
        {
            Vector2 point = new Vector2(coordsX[i], coordsY[i]);
            float graphCoordNormalizedX = (point[0] - xRange[0]) / (xRange[1] - xRange[0]);
            float graphCoordNormalizedY = (point[1] - yRange[0]) / (yRange[1] - yRange[0]);

            float graphCoordX = graphCoordRangeX[0] + graphCoordNormalizedX * (graphCoordRangeX[1] - graphCoordRangeX[0]);
            float graphCoordY = graphCoordRangeY[0] + (1f - graphCoordNormalizedY) * (graphCoordRangeY[1] - graphCoordRangeY[0]);

            chart.graphCoordsX.Insert(i, graphCoordX);
            chart.graphCoordsY.Insert(i, graphCoordY);
        }
        lineChartDict[datasetName][header] = chart;
    }

    private void CreateAllLineChartGameObjects()
    {
        for (int i = 0; i < fileNames.Count; i++)
        {
            List<string> headers = datasets[fileNames[i]].headers;
            int numAttributes = headers.Count - 1;

            dataPointGameObjects[fileNames[i]] = new Dictionary<string, List<GameObject>>();
            dataLineGameObjects[fileNames[i]] = new Dictionary<string, List<GameObject>>();
            lineChartGameObjects[fileNames[i]] = new Dictionary<string, GameObject>();

            for (int j = 0; j < numAttributes; j++)
            {
                CreateLineChartGameObject(fileNames[i], headers[j + 1]);
            }
        }
    }

    private void CreateLineChartGameObject(string datasetName, string header)
    {
        RectTransform graphDisplayTransform = lineChartTransform.parent.gameObject.GetComponent<RectTransform>();

        LineChartData chart = lineChartDict[datasetName][header];
        RectTransform chartTransform = Instantiate(lineChartTransform);
        chartTransform.SetParent(graphDisplayTransform, false);
        GameObject chartObj = chartTransform.gameObject;

        int numPoints = chart.numPoints;
        List<GameObject> pointObjects = new List<GameObject>(numPoints);
        List<GameObject> lineObjects = new List<GameObject>(numPoints - 1);
        Color lineColor = colorDict[datasetName][header];

        for (int i = 0; i < numPoints; i++)
        {
            RectTransform pointTransform = Instantiate(dataPointTransform);
            pointTransform.SetParent(chartTransform, false);

            float graphCoordX = chart.graphCoordsX[i];
            float graphCoordY = chart.graphCoordsY[i];

            pointTransform.anchoredPosition = new Vector2(graphCoordX, graphCoordY);
            GameObject pointObj = pointTransform.gameObject;
            pointObj.SetActive(true);

            pointObjects.Insert(i, pointObj);

            if (i > 0)
            {
                GameObject lineObj = CreateDotConnectionObject(pointObjects[i - 1].GetComponent<RectTransform>().anchoredPosition, pointObjects[i].GetComponent<RectTransform>().anchoredPosition, chartTransform, lineColor);
                lineObj.SetActive(true);
                lineObjects.Insert(i - 1, lineObj);
            }
        }

        dataPointGameObjects[datasetName][header] = pointObjects;
        dataLineGameObjects[datasetName][header] = lineObjects;

        chartObj.SetActive(false);
        lineChartGameObjects[datasetName][header] = chartObj;

    }


    private void ShowPointMetadataOnHover()
    {
        for (int i = 0; i < fileNames.Count; i++)
        {
            List<string> headers = datasets[fileNames[i]].headers;
            for (int j = 0; j < (headers.Count - 1); j++)
            {
                List<GameObject> pointObjects = dataPointGameObjects[fileNames[i]][headers[j + 1]];
                LineChartData chart = lineChartDict[fileNames[i]][headers[j + 1]];

                for (int k = 0; k < pointObjects.Count; k++)
                {
                    GameObject pointObj = pointObjects[k];
                    StatefulInteractable interactable = pointObj.GetComponent<StatefulInteractable>();
                    RectTransform rectTransforn = pointObj.GetComponent<RectTransform>();
                    GameObject metadataTextObj = rectTransforn.GetChild(0).gameObject;
                    RectTransform metadataTextTransform = metadataTextObj.GetComponent<RectTransform>();
                    Vector2 dataPointCoord = new Vector2(chart.pointCoordsX[k], chart.pointCoordsY[k]);

                    // Add Listeners to tthe hoverEntered and hoverExited events
                    interactable.hoverEntered.AddListener(hoverArgs =>
                    {
                        metadataTextObj.SetActive(true);
                        metadataTextObj.GetComponent<TextMeshProUGUI>().text = $"X: {Math.Round(dataPointCoord.x, 2)}{Environment.NewLine}Y: {Math.Round(dataPointCoord.y, 2)}";
                        metadataTextObj.GetComponent<TextMeshProUGUI>().fontSize = 6;
                        Debug.Log("Data Point Select Entered" + metadataTextTransform.anchoredPosition);
                    });

                    interactable.hoverExited.AddListener(hoverArgs =>
                    {
                        metadataTextObj.SetActive(false);
                        Debug.Log("Data Point Select Exited");
                    });
                }
            }
        }
    }


    private void CreateAllSeparatorXObjects()
    {
        for (int i = 0; i < fileNames.Count; i++)
        {
            List<string> headers = datasets[fileNames[i]].headers;
            int numAttributes = headers.Count - 1;

            separatorXGameObjects[fileNames[i]] = new Dictionary<string, List<GameObject>>();

            for (int j = 0; j < numAttributes; j++)
            {
                CreateSeparatorXGameObject(fileNames[i], headers[j + 1], false);
            }
        }
    }

    private void CreateAllSeparatorYObjects()
    {
        for (int i = 0; i < fileNames.Count; i++)
        {
            List<string> headers = datasets[fileNames[i]].headers;
            int numAttributes = headers.Count - 1;

            separatorYGameObjects[fileNames[i]] = new Dictionary<string, List<GameObject>>();

            for (int j = 0; j < numAttributes; j++)
            {
                CreateSeparatorYGameObject(fileNames[i], headers[j + 1], false);
            }
        }
    }

    private void CreateSeparatorXGameObject(string datasetName, string header, bool globalRange)
    {
        float spacingX = spacingDict[datasetName][0];
        float minVal, maxVal;
        if (globalRange)
        {
            minVal = rangeXDict[datasetName][0];
            maxVal = rangeXDict[datasetName][1];
        }
        else
        {
            (minVal, maxVal) = dataLoader.GetColumnRange(datasetName, header);
            (minVal, maxVal) = dataLoader.RoundDynamically(minVal, maxVal);
        }

        List<GameObject> separatorObjects = new List<GameObject>();

        for (int i = 0; i < numSeparatorsX; i++)
        {
            RectTransform separator = Instantiate(separatorTransformX);
            separator.SetParent(axisTransformX, false);
            separator.gameObject.SetActive(false);
            float separatorPosX = (separatorWidthX / 2) + (i * separatorWidthX);
            float separatorPosY = separatorHeightX / 2;
            separator.anchoredPosition = new Vector2(separatorPosX, separatorPosY);
            separator.sizeDelta = new Vector2(separatorWidthX, separatorHeightX);
            float separatorVal = minVal + (maxVal - minVal) * i / (numSeparatorsX - 1);
            separator.GetComponent<TextMeshProUGUI>().text = Math.Round(separatorVal, 2).ToString();
            separator.GetComponent<TextMeshProUGUI>().fontSize = 6;
            separatorObjects.Insert(i, separator.gameObject);
        }
        separatorXGameObjects[datasetName][header] = separatorObjects;
    }

    private void CreateSeparatorYGameObject(string datasetName, string header, bool globalRange)
    {
        float spacingY = spacingDict[datasetName][1];
        float minVal, maxVal;
        if (globalRange)
        {
            minVal = rangeYDict[datasetName][0];
            maxVal = rangeYDict[datasetName][1];
        }
        else
        {
            (minVal, maxVal) = dataLoader.GetColumnRange(datasetName, header);
            (minVal, maxVal) = dataLoader.RoundDynamically(minVal, maxVal);
        }
        List<GameObject> separatorObjects = new List<GameObject>();

        for (int i = 0; i < numSeparatorsY; i++)
        {
            RectTransform separator = Instantiate(separatorTransformY);
            separator.SetParent(axisTransformY, false);
            separator.gameObject.SetActive(false);
            float separatorPosX = separatorWidthY / 2;
            float separatorPosY = (separatorHeightY / 2) + i * separatorHeightY;
            separator.anchoredPosition = new Vector2(separatorPosX, separatorPosY);
            separator.sizeDelta = new Vector2(separatorWidthY, separatorHeightY);
            float separatorVal = maxVal - (maxVal - minVal) * i / (numSeparatorsY - 1);
            separator.GetComponent<TextMeshProUGUI>().text = Math.Round(separatorVal, 2).ToString();
            separator.GetComponent<TextMeshProUGUI>().fontSize = 6;
            separatorObjects.Insert(i, separator.gameObject);
        }
        separatorYGameObjects[datasetName][header] = separatorObjects;
    }

    private void DeactivateSeparators()
    {
        if ((currentDataset == null) || (currentHeader == null))
        {
            return;
        }
        else
        {
            List<GameObject> separatorXObjects = separatorXGameObjects[currentDataset][currentHeader];
            for (int i = 0; i < separatorXObjects.Count; i++)
            {
                separatorXObjects[i].SetActive(false);
            }

            List<GameObject> separatorYObjects = separatorYGameObjects[currentDataset][currentHeader];
            for (int i = 0; i < separatorYObjects.Count; i++)
            {
                separatorYObjects[i].SetActive(false);
            }
        }
    }

    private void ActivateSeparators(string datasetName, string header)
    {
        DeactivateSeparators();

        List<GameObject> separatorXObjects = separatorXGameObjects[datasetName][header];
        for (int i = 0; i < separatorXObjects.Count; i++)
        {
            separatorXObjects[i].SetActive(true);
        }

        List<GameObject> separatorYObjects = separatorYGameObjects[datasetName][header];
        for (int i = 0; i < separatorYObjects.Count; i++)
        {
            separatorYObjects[i].SetActive(true);
        }

        currentDataset = datasetName;
        currentHeader = header;
    }

    private GameObject CreateDotConnectionObject(Vector2 dotPositionA, Vector2 dotPositionB, RectTransform parentTransform, Color color)
    {
        GameObject gameObject = new GameObject("line", typeof(Image));
        gameObject.transform.SetParent(parentTransform, false);
        gameObject.GetComponent<Image>().color = color;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);
        rectTransform.anchoredPosition = dotPositionA + dir * distance * 0.5f;
        rectTransform.sizeDelta = new Vector2(distance, 1f);
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.localEulerAngles = new Vector3(0, 0, UtilsClass.GetAngleFromVectorFloat(dir));
        Vector3 localPos = rectTransform.localPosition;
        rectTransform.localPosition = new Vector3(localPos.x, localPos.y, -4f);
        return gameObject;
    }


    // Start is called before the first frame update
    void Start()
    {

    }


    // Update is called once per frame
    void Update()
    {

    }
}
