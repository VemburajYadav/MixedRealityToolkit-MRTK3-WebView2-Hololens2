using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    // public GameObject datasetBtn;
    // public GameObject modelBtn;
    // public GameObject resourcesBtn;
    // public GameObject metricsBtn;
    // public GameObject evaluationBtn;
    
    public GameObject datasetPage;
    public GameObject modelPage;
    public GameObject parameterPage;    
    public GameObject resourcesPage;
    public GameObject metricsPage;
    public GameObject evaluationPage;
    public GameObject experimentPanelPrefab;
    private int expCount;

    // Start is called before the first frame update
    void Start()
    {
        expCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void newPanel()

    {
        print("button clicked");
        GameObject panel = Instantiate(experimentPanelPrefab, new Vector3(-0.78f, 0.630f, 0.21f), Quaternion.identity);
        // expCount+=1;
        // panel.transform.Rotate(0f, 45f, 0.0f, Space.Self);
        // panel.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
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
    }
}
