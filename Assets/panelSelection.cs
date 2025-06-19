using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class panelSelection : MonoBehaviour
{

    public GameObject datasetPage;
    public GameObject modelPage;
    public GameObject parameterPage;    
    public GameObject resourcesPage;
    public GameObject metricsPage;
    public GameObject evaluationPage;
    public GameObject currPanel;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void pageSelection(string button)
    {
        if (button == "DatasetButton")
        {
            datasetPage.SetActive(true);
            modelPage.SetActive(false);
            parameterPage.SetActive(false);
            resourcesPage.SetActive(false);
            metricsPage.SetActive(false);
            evaluationPage.SetActive(false);
        }
        else if (button == "ModelButton")
        {
            datasetPage.SetActive(false);
            modelPage.SetActive(true);
            parameterPage.SetActive(true);
            resourcesPage.SetActive(false);
            metricsPage.SetActive(false);
            evaluationPage.SetActive(false);
        }
        else if (button == "ResourcesButton")
        {
            datasetPage.SetActive(false);
            modelPage.SetActive(false);
            parameterPage.SetActive(false);
            resourcesPage.SetActive(true);
            metricsPage.SetActive(false);
            evaluationPage.SetActive(false);
        }
        else if (button == "MetricsButton")
        {
            datasetPage.SetActive(false);
            modelPage.SetActive(false);
            parameterPage.SetActive(false);
            resourcesPage.SetActive(false);
            metricsPage.SetActive(true);
            evaluationPage.SetActive(false);
        }
        else if (button == "EvaluationButton")
        {
            datasetPage.SetActive(false);
            modelPage.SetActive(false);
            parameterPage.SetActive(false);
            resourcesPage.SetActive(false);
            metricsPage.SetActive(false);
            evaluationPage.SetActive(true);
        }
        else if (button == "RunExperiment")
        {
            print("run");
            // currPanel.SetActive(false);
            datasetPage.SetActive(false);
            modelPage.SetActive(false);
            parameterPage.SetActive(false);
            resourcesPage.SetActive(false);
            metricsPage.SetActive(false);
            evaluationPage.SetActive(true);
        }
    }
}
