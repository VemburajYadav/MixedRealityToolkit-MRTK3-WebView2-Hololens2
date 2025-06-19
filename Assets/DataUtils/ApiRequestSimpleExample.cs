using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

public class ApiRequestSimpleExample : MonoBehaviour
{
    // Url's to the deployed endpoints in the extremeXP server
    private const string baseUrlMetrics = "https://api.expvis.smartarch.cz/api/metrics/";
    private const string baseUrlWorkflows = "https://api.expvis.smartarch.cz/api/workflows/";
    private const string baseUrlExperiments = "https://api.expvis.smartarch.cz/api/experiments/";

    // access token
    private const string accessToken = "859a20f63425a55bb2b924473db1a56bd5374f70";

    // experimentId 
    private const string experimentId = "WDSCpJUBwbSNbg2q54Zd";

    // Workflow and Metric Id's
    private List<string> workflowsId = new List<string>();
    private Dictionary<string, List<string>> metricsId = new Dictionary<string, List<string>>();

    // Url to the experiment
    private string experimentUrl;

    // Variables to store the raw text output from the API's GET Request
    private string jsonExperimentResponse;

    // Variables (Instances of appropriate data structures) to store the parsed respones in JSON format
    public ExperimentResponse experimentResponse;

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
            // response in the form of raw text 
            jsonExperimentResponse = www.downloadHandler.text;

            // Parse the response
            experimentResponse = JsonUtility.FromJson<ExperimentResponse>(jsonExperimentResponse);

            // Extract the Workflow Id's
            int numWorkflows = experimentResponse.experiment.workflow_ids.Count;
            workflowsId = experimentResponse.experiment.workflow_ids;
            Debug.Log("Number of Workflows: " + workflowsId.Count);

            // Extract creator's name and id
            string userName = experimentResponse.experiment.creator.name;
            string userId = experimentResponse.experiment.creator.id;
            Debug.Log($"Username: {userName}, UserID. {userId}");

            // Update the data availability status
            isDataFetched = true;
        }
        else
        {
            // If there's an error, print the error message 
            Debug.LogError("Request failed: " + www.error);
        }


    }

    // Define C# classes to match the expected data structure
    // Doesn't need to have all the attributes (keys in a dictionary) defined in the data stucture
    // For example, in my case, I don't need the uder info (atleast for now,
    // so I neither have the ExperimentCreator class nor the variable creator in the ExperimentData class defined in my script)
    [System.Serializable]
    public class ExperimentResponse
    {
        public ExperimentData experiment;
    }


    [System.Serializable]
    public class ExperimentData
    {
        public string name;
        public ExperimentCreator creator;
        public string status;
        public List<string> workflow_ids;
    }

    [System.Serializable]
    public class ExperimentCreator
    {
        public string name;
        public string id;
    }
}




