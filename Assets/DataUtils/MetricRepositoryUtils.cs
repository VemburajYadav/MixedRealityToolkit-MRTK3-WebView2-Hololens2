using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

namespace DataUtils
{

    // Define C# classes to match the expected data structure
    [System.Serializable]
    public class MetricResponse
    {
        public string name;
        public string type;
        public string kind;
        public string semantic_type;
        public string parent_type;
        public string parent_id;
        public string experimentId;
        public MetricRecords[] records;
        public MetricAggregation aggregation;
    }

    [System.Serializable]
    public class MetricRecords
    {
        public float value;
    }

    [System.Serializable]
    public class MetricAggregation
    {
        public int count;
        public float average;
        public float min;
        public float max;
        public float median;
    }

    [System.Serializable]
    public class ExperimentResponse
    {
        public ExperimentData experiment;
    }


    [System.Serializable]
    public class ExperimentData
    {
        public string name;
        public List<string> workflow_ids;
    }

    [System.Serializable]
    public class WorkflowResponse
    {
        public WorkflowData workflow;
    }

    [System.Serializable]
    public class WorkflowData
    {
        public string name;
        public string experimentId;
        public List<string> metric_ids;
    }
}




