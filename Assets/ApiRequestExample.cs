using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using DataUtils;

public class ApiRequestExample : MonoBehaviour
{
    // Url's to the deployed endpoints in the extremeXP server
    private const string baseUrlMetrics = "https://api.expvis.smartarch.cz/api/metrics/";
    private const string baseUrlWorkflows = "https://api.expvis.smartarch.cz/api/workflows/";
    private const string baseUrlExperiments = "https://api.expvis.smartarch.cz/api/experiments/";

    // access token
    private const string accessToken = "859a20f63425a55bb2b924473db1a56bd5374f70";

    // Script has experimentId as the only parameter. The workflows and associated metrics will be automatically inferred for the Vizualition Dashboard
    private const string experimentId = "TTS7gJUBwbSNbg2qDIOf";

    // Workflow and Metric Id's
    private List<string> workflowsId = new List<string>();
    private Dictionary<string, List<string>> metricsId = new Dictionary<string, List<string>>();

    // Url to the experiment
    private string experimentUrl;

    // Variables to store the raw text output from the API's GET Request
    private string jsonExperimentResponse;
    private Dictionary<string, string> jsonWorkflowsResponse = new Dictionary<string, string>();
    private Dictionary<string, Dictionary<string, string>> jsonMetricsResponse = new Dictionary<string, Dictionary<string, string>>();

    // Variables (Instances of appropriate data structures) to store the parsed respones in JSON format
    public ExperimentResponse experimentResponse;
    public Dictionary<string, WorkflowResponse> workflowsResponse = new Dictionary<string, WorkflowResponse>();
    public Dictionary<string, Dictionary<string, MetricResponse>> metricsResponse = new Dictionary<string, Dictionary<string, MetricResponse>>();

    // Variables to store the processed api responses
    public List<string> uniqueMetricNames;
    public Dictionary<string, Dictionary<string, List<float>>> groupedMetricsData = new Dictionary<string, Dictionary<string, List<float>>>();
    public Dictionary<string, string> semanticTypes = new Dictionary<string, string>();

    // Check if the data has been fetched from the metric repsoitory
    public bool isDataFetched = false;


    // Start is called before the first frame update 
    void Start()
    {
        experimentUrl = baseUrlExperiments + experimentId;
        StartCoroutine(GetApiData());
    }

    public IEnumerator GetApiData()
    {
        UnityWebRequest www = UnityWebRequest.Get(experimentUrl);

        // Add the access token in the request header 
        www.SetRequestHeader("access-token", accessToken);

        // Send the request and wait for the response 
        yield return www.SendWebRequest();

        // Check if the request completed without errors 
        if (www.result == UnityWebRequest.Result.Success)
        {
            // If successful, parse the JSON response 
            jsonExperimentResponse = www.downloadHandler.text;

            // Parse the response
            experimentResponse = JsonUtility.FromJson<ExperimentResponse>(jsonExperimentResponse);

            // Extract the Workflow Id's
            int numWorkflows = experimentResponse.experiment.workflow_ids.Count;
            workflowsId = experimentResponse.experiment.workflow_ids;
            Debug.Log("Number of Workflows: " + workflowsId.Count);

            // Extract the Metric Id's from each of the workflows
            int numMetrics;
            string currWorkflowUrl, currMetricUrl;
            string currWorkflowId, currMetricId;
            WorkflowResponse currWorkflowResponse;
            MetricResponse currMetricResponse;

            for (int i = 0; i < workflowsId.Count; i++)
            {
                // Url to the workflow
                currWorkflowId = workflowsId[i];
                currWorkflowUrl = baseUrlWorkflows + currWorkflowId;
                www = UnityWebRequest.Get(currWorkflowUrl);

                // Add the access token in the request header 
                www.SetRequestHeader("access-token", accessToken);

                // Send the request and wait for the response 
                yield return www.SendWebRequest();

                // Check if the request completed without errors 
                if (www.result == UnityWebRequest.Result.Success)
                {
                    // If successful, parse the JSON response 
                    jsonWorkflowsResponse[currWorkflowId] = www.downloadHandler.text;
                    // Debug.Log("Workflow Responser: " + jsonWorkflowsResponse[currWorkflowId]);

                    // Parse the response
                    currWorkflowResponse = JsonUtility.FromJson<WorkflowResponse>(jsonWorkflowsResponse[currWorkflowId]);
                    workflowsResponse[currWorkflowId] = currWorkflowResponse;
                    // Debug.Log("Workflow Name: " + currWorkflowResponse.workflow.name);

                    // Extract the Metric Id's
                    numMetrics = currWorkflowResponse.workflow.metric_ids.Count;
                    metricsId[currWorkflowId] = currWorkflowResponse.workflow.metric_ids;
                    Debug.Log($"Number of Metrics in workflow {currWorkflowId}: {metricsId[currWorkflowId].Count}");


                    // GET the metris data
                    jsonMetricsResponse[currWorkflowId] = new Dictionary<string, string>();
                    metricsResponse[currWorkflowId] = new Dictionary<string, MetricResponse>();

                    for (int j = 0; j < numMetrics; j++)
                    {
                        // Url to the metric
                        currMetricId = metricsId[currWorkflowId][j];
                        currMetricUrl = baseUrlMetrics + currMetricId;
                        www = UnityWebRequest.Get(currMetricUrl);

                        // Add the access token in the request header 
                        www.SetRequestHeader("access-token", accessToken);

                        // Send the request and wait for the response 
                        yield return www.SendWebRequest();

                        // Check if the request completed without errors 
                        if (www.result == UnityWebRequest.Result.Success)
                        {
                            // If successful, parse the JSON response 
                            jsonMetricsResponse[currWorkflowId][currMetricId] = www.downloadHandler.text;
                            // Debug.Log("Metric Responser: " + jsonMetricsResponse[currWorkflowId][currMetricId]);

                            // Parse the response
                            currMetricResponse = JsonUtility.FromJson<MetricResponse>(jsonMetricsResponse[currWorkflowId][currMetricId]);
                            metricsResponse[currWorkflowId][currMetricId] = currMetricResponse;
                            // Debug.Log("Metric Name: " + currMetricResponse.name);


                        }
                        else
                        {
                            // If there's an error, print the error message 
                            Debug.LogError("Request failed: " + www.error);
                        }

                    }
                }
                else
                {
                    // If there's an error, print the error message 
                    Debug.LogError("Request failed: " + www.error);
                }
            }
        }
        else
        {
            // If there's an error, print the error message 
            Debug.LogError("Request failed: " + www.error);
        }

        // Update the data availability status
        isDataFetched = true;
    }

    public void FindUniqueMetrics()
    {
        // Variable declaration
        string metricName;
        List<string> availableMetricNames = new List<string>();

        // Gather the names of all the available metrics from each of the workflows 
        for (int i = 0; i < workflowsId.Count; i++)
        {
            List<string> metricsList = metricsId[workflowsId[i]];
            for (int j = 0; j < metricsList.Count; j++)
            {
                metricName = metricsResponse[workflowsId[i]][metricsList[j]].name;
                availableMetricNames.Insert(i, metricName);
            }
        }

        // Form a list with unique metric names
        uniqueMetricNames = new List<string>(new HashSet<string>(availableMetricNames));

        /***
        string[] keysArray = new string[metricsId.Count];
        metricsId.Keys.CopyTo(keysArray, 0);

        for (int i = 0; i < availableMetricNames.Count; i++)
        {
            Debug.Log($"Available Metrics: {i}, {availableMetricNames[i]}");
        }
        ***/

        for (int i = 0; i < uniqueMetricNames.Count; i++)
        {
            Debug.Log($"Unique Metrics: {i}, {uniqueMetricNames[i]}");
        }
    }

    public void GroupMetricsData()
    {
        string queryMetricName, currMetricName, currWorkflowName, currWorkflowId, currMetricId;
        bool isCurrMetricValid;
        string querySemanticType;

        for (int i = 0; i < uniqueMetricNames.Count; i++)
        {
            isCurrMetricValid = false;
            queryMetricName = uniqueMetricNames[i];
            Dictionary<string, List<float>> metricData = new Dictionary<string, List<float>>();

            for (int j = 0; j < workflowsId.Count; j++)
            {
                currWorkflowId = workflowsId[j];
                currWorkflowName = workflowsResponse[currWorkflowId].workflow.name;
                List<string> metricsList = metricsId[currWorkflowId];

                for (int k = 0; k < metricsList.Count; k++)
                {
                    currMetricId = metricsList[k];
                    currMetricName = metricsResponse[currWorkflowId][currMetricId].name;
                    bool isMetricAvailable = string.Equals(currMetricName, queryMetricName, StringComparison.OrdinalIgnoreCase);
                    if (isMetricAvailable)
                    {
                        MetricResponse currMetricResponse = metricsResponse[currWorkflowId][currMetricId];
                        MetricRecords[] records = currMetricResponse.records;
                        if (records != null)
                        {
                            if (records.Length > 0)
                            {
                                Debug.Log($"Number of records for the metric {queryMetricName} in Workflow {currWorkflowName}: {currMetricResponse.records.Length}");
                                metricData[currWorkflowName] = ConvertRecordsToList(records);
                                querySemanticType = currMetricResponse.semantic_type;
                                semanticTypes[queryMetricName] = querySemanticType;
                                isCurrMetricValid = true;   
                            }
                        }
                    }
                }
            }

            if (isCurrMetricValid)
            {
                groupedMetricsData[queryMetricName] = metricData;
            }
        }
    }

    public List<float> ConvertRecordsToList(MetricRecords[] records)
    {
        int numRecords = records.Length;
        List<float> valueList = new List<float>();

        for (int i = 0; i < numRecords; i++)
        {
            valueList.Insert(i, records[i].value);
        }
        return valueList;
    }
}




