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

public class LineChartVisualization : MonoBehaviour
{
    [SerializeField]
    GameObject dataPointTemplate;

    [SerializeField]
    GameObject lineChartTemplate;

    [SerializeField]
    GameObject toggleLineChartTemplate;

    [SerializeField]
    GameObject toggleDatasetTemplate;
    
    [SerializeField]
    GameObject separatorTemplateX;

    [SerializeField]
    GameObject separatorTemplateY;

    // GameObject Transform Variables
    private RectTransform separatorTransformX;
    private RectTransform separatorTransformY;
    private RectTransform dataPointTransform;
    private RectTransform toggleLineChartTransform;
    private RectTransform toggleDatasetTransform;
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

    // Options Layout Game Objects
    private Dictionary<string, GameObject> datasetToggleGameObjects = new Dictionary<string, GameObject>();
    private Dictionary<string, Dictionary<string, GameObject>> chartToggleGameObjects = new Dictionary<string, Dictionary<string, GameObject>>();

    // Interactables
    private Dictionary<string, Dictionary<string, StatefulInteractable>> interactablesDict = new Dictionary<string, Dictionary<string, StatefulInteractable>>();

    // toggle Collections
    private ToggleCollection datasetToggles = new ToggleCollection();

    // GameObjects being displayed
    private string currentDataset;
    private string prevDataset;
    private string datasetOnDisplay;

    private void Awake()
    {

        separatorTransformX = separatorTemplateX.GetComponent<RectTransform>();
        separatorTransformY = separatorTemplateY.GetComponent<RectTransform>();
        dataPointTransform = dataPointTemplate.GetComponent<RectTransform>();
        toggleLineChartTransform = toggleLineChartTemplate.GetComponent<RectTransform>();
        toggleDatasetTransform = toggleDatasetTemplate.GetComponent<RectTransform>();
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

        ComputeGraphAttributes();
        SampleColorsForCharts();
        CreateAllLineCharts();
        CreateAllSeparatorXObjects();
        CreateAllSeparatorYObjects();
        CreateAllLineChartGameObjects();
        ShowPointMetadataOnHover();
        CreateDatasetToggles(fileNames);
        CreateLineChartToggles(fileNames);
        ShowDatasetToggles();

        AddListenersToDatasetToggles();
        AddListenersToChartToggles();
        // ShowLineChart(fileNames[1], datasets[fileNames[1]].headers[1]);
        // ShowLineChart(fileNames[1], datasets[fileNames[1]].headers[2]);
        // ShowLineChart(fileNames[1], datasets[fileNames[1]].headers[3]);
    }

    private void AddListenersToDatasetToggles()
    {
        for (int i = 0; i < fileNames.Count; i++)
        {
            GameObject gameObject = datasetToggleGameObjects[fileNames[i]];
            StatefulInteractable interactable = gameObject.GetComponent<StatefulInteractable>();

            string datasetName = fileNames[i];
            // Add Listeners to the hoverEntered and hoverExited events
            interactable.selectExited.AddListener(selectArgs =>
            {
                bool toggleState = interactable.IsToggled;

                currentDataset = datasetName;

                if (toggleState)
                {
                    if (prevDataset != null)
                    {
                        RemoveChartToggles(prevDataset);
                    }
                    ShowChartToggles(currentDataset);
                    prevDataset = currentDataset;
                }
                else
                {
                    RemoveChartToggles(currentDataset);
                    RemoveAllLineCharts();
                    prevDataset = null;
                }
            });
        }
    }

    private void AddListenersToChartToggles()
    {
        for (int i = 0; i < fileNames.Count; i++)
        {
            string datasetName = fileNames[i];
            List<string> headers = datasets[fileNames[i]].headers;
            int numAttributes = headers.Count - 1;

            for (int j = 0; j < numAttributes; j++)
            {
                string header = headers[j + 1];

                GameObject gameObject = chartToggleGameObjects[datasetName][header];
                StatefulInteractable interactable = gameObject.GetComponent<StatefulInteractable>();

                interactable.selectExited.AddListener(selectArgs =>
                {
                    bool toggleState = interactable.IsToggled;

                    if (toggleState)
                    {
                        ShowLineChart(datasetName, header);
                    }
                    else
                    {
                        RemoveLineChart(datasetName, header);
                    }
                });
            }
        }
    }


    private void ShowLineChart(string datasetName, string header)
    {
        /***
        if (datasetOnDisplay != datasetName)
        {
            if (datasetOnDisplay != null)
            {
                DeactivateSeparators(datasetOnDisplay);
            }
            ActivateSeparators(datasetName);
            datasetOnDisplay = datasetName;
        }
        ***/
        if (datasetOnDisplay != null)
        {
            DeactivateSeparators(datasetOnDisplay);
        }
        ActivateSeparators(datasetName);
        datasetOnDisplay = datasetName;
        lineChartGameObjects[datasetName][header].SetActive(true);
    }

    private void RemoveLineChart(string datasetName, string header)
    {
        lineChartGameObjects[datasetName][header].SetActive(false);
    }

    private void RemoveAllLineCharts()
    {
        for (int i = 0; i < fileNames.Count; i++)
        {
            string datasetName = fileNames[i];
            List<string> headers = datasets[datasetName].headers;
            int numAttributes = headers.Count - 1;

            for (int j = 0; j < numAttributes; j++)
            {
                string header = headers[j + 1];
                RemoveLineChart(datasetName, header);
            }
        }
        if (datasetOnDisplay != null)
        {
            DeactivateSeparators(datasetOnDisplay);
        }
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
                CreateLineChart(fileNames[i], headers[j + 1]);
            }
        }
    }

    private void CreateLineChart(string datasetName, string header)
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
        xRange = rangeXDict[datasetName];
        yRange = rangeYDict[datasetName];

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

    private void CreateDatasetToggles(List<string> datasetNames)
    {
        RectTransform optionsLayoutTransform = toggleDatasetTransform.parent.gameObject.GetComponent<RectTransform>();

        for (int i = 0; i < datasetNames.Count; i++)
        {
            RectTransform datasetOption = Instantiate(toggleDatasetTransform);
            datasetOption.SetParent(optionsLayoutTransform, false);
            datasetOption.gameObject.SetActive(false);
            GameObject textObj = datasetOption.Find("Frontplate/AnimatedContent/Text").gameObject;
            textObj.GetComponent<TextMeshProUGUI>().text = string.Join(" ", datasetNames[i].Split('_').Select(word => char.ToUpper(word[0]) + word.Substring(1).Split('.')[0]));
            datasetToggleGameObjects[datasetNames[i]] = datasetOption.gameObject;
        }
    }

    private void CreateLineChartToggles(List<string> datasetNames)
    {
        RectTransform optionsLayoutTransform = toggleLineChartTransform.parent.gameObject.GetComponent<RectTransform>();

        for (int i = 0; i < datasetNames.Count; i++)
        {
            List<string> headers = datasets[datasetNames[i]].headers.Skip(1).ToList();
            Dictionary<string, GameObject> toggleObjects = new Dictionary<string, GameObject>();
            for (int j = 0; j < headers.Count; j++)
            {
                RectTransform lineChartOption = Instantiate(toggleLineChartTransform);
                lineChartOption.SetParent(optionsLayoutTransform, false);
                lineChartOption.gameObject.SetActive(false);
                GameObject textObj = lineChartOption.Find("Frontplate/AnimatedContent/Text").gameObject;
                textObj.GetComponent<TextMeshProUGUI>().text = string.Join(" ", headers[j].Split('_').Select(word => char.ToUpper(word[0]) + word.Substring(1)));
                toggleObjects[headers[j]] = lineChartOption.gameObject;
            }
            chartToggleGameObjects[datasetNames[i]] = toggleObjects;
        }
    }

    private void ShowChartToggles(string datasetName)
    {
        List<string> headers = datasets[datasetName].headers.Skip(1).ToList();
        for (int i = 0; i < headers.Count; i++)
        {
            GameObject gameObject = chartToggleGameObjects[datasetName][headers[i]];
            gameObject.SetActive(true);
        }
    }

    private void RemoveChartToggles(string datasetName)
    {
        List<string> headers = datasets[datasetName].headers.Skip(1).ToList();
        for (int i = 0; i < headers.Count; i++)
        {
            GameObject gameObject = chartToggleGameObjects[datasetName][headers[i]];
            gameObject.SetActive(false);
            StatefulInteractable interactable = gameObject.GetComponent<StatefulInteractable>();
            interactable.ForceSetToggled(false);
        }
        RemoveAllLineCharts();
    }

    private void ShowDatasetToggles()
    {
        for (int i = 0; i < fileNames.Count; i++)
        {
            GameObject gameObject = datasetToggleGameObjects[fileNames[i]];
            gameObject.SetActive(true);
        }

        datasetToggles = toggleDatasetTransform.parent.gameObject.GetComponent<ToggleCollection>();
        datasetToggles.enabled = true;
    }

    private void ShowLineChartOptions(List<string> headers)
    {
        RectTransform optionsLayoutTransform = toggleLineChartTransform.parent.gameObject.GetComponent<RectTransform>();

        for (int i = 0; i < headers.Count; i++)
        {
            RectTransform lineChartOption = Instantiate(toggleLineChartTransform);
            lineChartOption.SetParent(optionsLayoutTransform, false);
            lineChartOption.gameObject.SetActive(true);
            GameObject textObj = lineChartOption.Find("Frontplate/AnimatedContent/Text").gameObject;
            textObj.GetComponent<TextMeshProUGUI>().text = string.Join(" ", headers[i].Split('_').Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
        }
    }

    private void ShowDatasetOptions(List<string> datasetNames)
    {
        RectTransform optionsLayoutTransform = toggleDatasetTransform.parent.gameObject.GetComponent<RectTransform>();

        for (int i = 0; i < datasetNames.Count; i++)
        {
            RectTransform datasetOption = Instantiate(toggleDatasetTransform);
            datasetOption.SetParent(optionsLayoutTransform, false);
            datasetOption.gameObject.SetActive(true);
            GameObject textObj = datasetOption.Find("Frontplate/AnimatedContent/Text").gameObject;
            textObj.GetComponent<TextMeshProUGUI>().text = string.Join(" ", datasetNames[i].Split('_').Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower().Split('.')[0]));
        }
    }

    private GameObject CreateDataPoint(Vector2 point)
    {
        RectTransform graphDisplayTransform = dataPointTransform.parent.gameObject.GetComponent<RectTransform>();
        graphWidth = graphDisplayTransform.rect.width;
        graphHeight = graphDisplayTransform.rect.height;
        graphCoordRangeX = new Vector2(spacingWidth / 2, graphWidth - (spacingWidth / 2));
        graphCoordRangeY = new Vector2(-spacingHeight / 2, -(graphHeight - (spacingHeight / 2)));

        float graphCoordNormalizedX = (point[0] - xRange[0]) / (xRange[1] - xRange[0]);
        float graphCoordNormalizedY = (point[1] - yRange[0]) / (yRange[1] - yRange[0]);

        float graphCoordX = graphCoordRangeX[0] + graphCoordNormalizedX * (graphCoordRangeX[1] - graphCoordRangeX[0]);
        float graphCoordY = graphCoordRangeY[0] + (1f - graphCoordNormalizedY) * (graphCoordRangeY[1] - graphCoordRangeY[0]);

        RectTransform pointObj = Instantiate(dataPointTransform);
        pointObj.SetParent(graphDisplayTransform, false);
        pointObj.gameObject.SetActive(true);
        pointObj.anchoredPosition = new Vector2(graphCoordX, graphCoordY);

        return pointObj.gameObject;
    }

    private void ShowDataPoints(List<Vector2> points)
    {
        int numPoints = points.Count;

        for (int i = 0; i < numPoints; i++)
        {
            dataPointObjects.Insert(i, CreateDataPoint(points[i]));
        }
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
                Color metadataColor = colorDict[fileNames[i]][headers[j + 1]];
                for (int k = 0; k < pointObjects.Count; k++)
                {
                    GameObject pointObj = pointObjects[k];
                    StatefulInteractable interactable = pointObj.GetComponent<StatefulInteractable>();
                    RectTransform rectTransforn = pointObj.GetComponent<RectTransform>();
                    GameObject metadataTextObj = rectTransforn.GetChild(0).gameObject;
                    RectTransform metadataTextTransform = metadataTextObj.GetComponent<RectTransform>();
                    Vector2 dataPointCoord = new Vector2(chart.pointCoordsX[k], chart.pointCoordsY[k]);

                    // Add Listeners to the hoverEntered and hoverExited events
                    interactable.hoverEntered.AddListener(hoverArgs =>
                    {
                        metadataTextObj.SetActive(true);
                        metadataTextObj.GetComponent<TextMeshProUGUI>().text = $"X: {Math.Round(dataPointCoord.x, 2)}{Environment.NewLine}Y: {Math.Round(dataPointCoord.y, 2)}";
                        metadataTextObj.GetComponent<TextMeshProUGUI>().color = metadataColor;
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
                CreateSeparatorXGameObject(fileNames[i], headers[j + 1]);
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
                CreateSeparatorYGameObject(fileNames[i], headers[j + 1]);
            }
        }
    }

    private void CreateSeparatorXGameObject(string datasetName, string header)
    {
        float spacingX = spacingDict[datasetName][0];
        float minVal = rangeXDict[datasetName][0];
        float maxVal = rangeXDict[datasetName][1];

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

    private void CreateSeparatorYGameObject(string datasetName, string header)
    {
        float spacingY = spacingDict[datasetName][1];
        float minVal = rangeYDict[datasetName][0];
        float maxVal = rangeYDict[datasetName][1];

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

    private void DeactivateSeparators(string datasetName)
    {
        List<string> headers = datasets[datasetName].headers;

        List<GameObject> separatorXObjects = separatorXGameObjects[datasetName][headers[1]];
        for (int i = 0; i < separatorXObjects.Count; i++)
        {
            separatorXObjects[i].SetActive(false);
        }

        List<GameObject> separatorYObjects = separatorYGameObjects[datasetName][headers[1]];
        for (int i = 0; i < separatorYObjects.Count; i++)
        {
            separatorYObjects[i].SetActive(false);
        }

    }

    private void ActivateSeparators(string datasetName)
    {
        // DeactivateSeparators();
        List<string> headers = datasets[datasetName].headers;

        List<GameObject> separatorXObjects = separatorXGameObjects[datasetName][headers[1]];
        for (int i = 0; i < separatorXObjects.Count; i++)
        {
            separatorXObjects[i].SetActive(true);
        }

        List<GameObject> separatorYObjects = separatorYGameObjects[datasetName][headers[1]];
        for (int i = 0; i < separatorYObjects.Count; i++)
        {
            separatorYObjects[i].SetActive(true);
        }

        // currentDataset = datasetName;
        // currentHeader = header;
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
        rectTransform.localPosition = new Vector3(localPos.x,  localPos.y, -4f);
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
