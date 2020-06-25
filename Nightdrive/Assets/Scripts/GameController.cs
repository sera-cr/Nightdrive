using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("Buttons")]
    public Button playerButton;
    public Button navmeshButton;
    public Button noAIButton;
    public Button machineLearningButton;
    public Button resetButton;
    public Button dayNightButton;
    public Button enablePathButton;
    public Slider gameSpeedSlider;
    [Header("Cars and Agent")]
    public GameObject playerCar;
    public GameObject navMeshCar;
    public GameObject navMeshAgent;
    public GameObject noAICar;
    public GameObject regressionCar;
    private RegressionCarMovement regressionCarObject;
    public GameObject machineLearningCar;
    [Header("Cameras")]
    public GameObject cenitalCamera;
    [Header("Lighting")]
    public GameObject globalLight;
    public GameObject streetLights;
    public GameObject playerLights;
    public GameObject navMeshLights;
    public GameObject noAILights;
    public GameObject regressionLights;
    public GameObject machineLearningLights;
    [Header("Text")]
    public Text kmhText;
    public Text speedText;
    [Header("Sky")]
    public Material night;
    public Material day;
    private bool nightDay; // true = night, false = day
    private Color dayLight;
    private Color nightLight;
    [Header("Path")]
    public GameObject path1;
    public GameObject path2;
    public Transform[] objectives1;
    public Transform[] objectives2;
    private bool enablePath;
    private bool currentPath; // true = path1, false = path2
    private string enabledPathText;
    private string disabledPathText;

    // Start is called before the first frame update
    void Start()
    {
        playerCar.SetActive(false);
        navMeshCar.SetActive(false);
        navMeshAgent.SetActive(false);
        noAICar.SetActive(false);
        regressionCar.SetActive(false);
        regressionCarObject = regressionCar.GetComponent<RegressionCarMovement>();
        cenitalCamera.SetActive(true);
        machineLearningCar.SetActive(false);
        kmhText.enabled = false;
        speedText.enabled = false;
        nightDay = true;
        streetLights.SetActive(true);
        dayLight = new Color(250f/255f, 244f/255f, 214f/255f, 255f/255f);
        nightLight = new Color(0, 0, 0, 0);
        enablePath = false;
        enabledPathText = "Disable Path";
        disabledPathText = "Enable Path";
        objectives1 = path1.GetComponentsInChildren<Transform>();
        for (int i = 1; i < objectives1.Length; i++)
        {
            objectives1[i].GetComponent<MeshRenderer>().enabled = false;
        }
        objectives2 = path2.GetComponentsInChildren<Transform>();
        for (int i = 1; i < objectives2.Length; i++)
        {
            objectives2[i].GetComponent<MeshRenderer>().enabled = false;
        }
        gameSpeedSlider.minValue = 1f;
        gameSpeedSlider.maxValue = 30f;
    }

    public void PlayerButtonOnClick()
    {
        playerCar.SetActive(true);
        navMeshCar.SetActive(false);
        navMeshAgent.SetActive(false);
        noAICar.SetActive(false);
        regressionCar.SetActive(false);
        machineLearningCar.SetActive(false);
        kmhText.enabled = true;
        speedText.enabled = true;
        cenitalCamera.SetActive(false);
        if (enablePath)
        {
            EnablePathOnClick();
        }
        currentPath = true;
    }

    public void NavMeshButtonOnClick()
    {
        playerCar.SetActive(false);
        navMeshCar.SetActive(true);
        navMeshAgent.SetActive(true);
        navMeshAgent.GetComponent<MeshRenderer>().enabled = false;
        noAICar.SetActive(false);
        regressionCar.SetActive(false);
        machineLearningCar.SetActive(false);
        kmhText.enabled = true;
        speedText.enabled = true;
        cenitalCamera.SetActive(false);
        if (enablePath)
        {
            EnablePathOnClick();
        }
        currentPath = true;
    }

    public void NoAIButtonOnClick()
    {
        playerCar.SetActive(false);
        navMeshCar.SetActive(false);
        navMeshAgent.SetActive(false);
        noAICar.SetActive(true);
        regressionCar.SetActive(false);
        machineLearningCar.SetActive(false);
        kmhText.enabled = true;
        speedText.enabled = true;
        cenitalCamera.SetActive(false);
        if (enablePath)
        {
            EnablePathOnClick();
        }
        currentPath = true;
    }

    public void RegressionTrainingButtonOnClick()
    {
        playerCar.SetActive(false);
        navMeshCar.SetActive(false);
        navMeshAgent.SetActive(false);
        noAICar.SetActive(false);
        regressionCar.SetActive(true);
        machineLearningCar.SetActive(false);
        kmhText.enabled = true;
        speedText.enabled = true;
        cenitalCamera.SetActive(false);
        regressionCarObject.STATE = "No Knowledge";
        regressionCarObject.StartTraining();
        if (enablePath)
        {
            EnablePathOnClick();
        }
        currentPath = false;   
    }

    public void RegressionRunButtonOnClick()
    {
        playerCar.SetActive(false);
        navMeshCar.SetActive(false);
        navMeshAgent.SetActive(false);
        noAICar.SetActive(false);
        regressionCar.SetActive(true);
        machineLearningCar.SetActive(false);
        kmhText.enabled = true;
        speedText.enabled = true;
        cenitalCamera.SetActive(false);
        regressionCarObject.STATE = "With Knowledge";
        if (enablePath)
        {
            EnablePathOnClick();
        }
        currentPath = false;
    }

    public void MachineLearningOnClick()
    {
        playerCar.SetActive(false);
        navMeshCar.SetActive(false);
        navMeshAgent.SetActive(false);
        noAICar.SetActive(false);
        regressionCar.SetActive(false);
        machineLearningCar.SetActive(true);
        kmhText.enabled = true;
        speedText.enabled = true;
        if (enablePath)
        {
            EnablePathOnClick();
        }
        enablePath = true;
        cenitalCamera.SetActive(false);
    }

    public void DayNightOnClick()
    {
        if (nightDay)
        {
            globalLight.GetComponent<Light>().color = dayLight;
            streetLights.SetActive(false);
            playerLights.SetActive(false);
            navMeshLights.SetActive(false);
            noAILights.SetActive(false);
            regressionLights.SetActive(false);
            machineLearningLights.SetActive(false);
            RenderSettings.skybox = day;
            nightDay = false;
        } else
        {
            globalLight.GetComponent<Light>().color = nightLight;
            streetLights.SetActive(true);
            playerLights.SetActive(true);
            navMeshLights.SetActive(true);
            noAILights.SetActive(true);
            regressionLights.SetActive(true);
            machineLearningLights.SetActive(true);
            RenderSettings.skybox = night;
            nightDay = true;
        }
    }

    public void EnablePathOnClick()
    {
        Transform[] objectives;
        if (currentPath)
        {
            objectives = objectives1;
        } else
        {
            objectives = objectives2;
        }
        if (enablePath)
        {
            for (int i = 1; i < objectives.Length; i++)
            {
                objectives[i].GetComponent<MeshRenderer>().enabled = false;
            }
            enablePathButton.GetComponentInChildren<Text>().text = disabledPathText;
            enablePath = false;
        } else
        {
            for (int i = 1; i < objectives.Length; i++)
            {
                objectives[i].GetComponent<MeshRenderer>().enabled = true;
            }
            enablePathButton.GetComponentInChildren<Text>().text = enabledPathText;
            enablePath = true;
        }
    }

    public void RestartOnClick()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void ChangeGameSpeed()
    {
        Time.timeScale = gameSpeedSlider.value;
    }
}
