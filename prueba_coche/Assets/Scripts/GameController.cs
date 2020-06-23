using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    [Header("Cars and Agent")]
    public GameObject playerCar;
    public GameObject navMeshCar;
    public GameObject navMeshAgent;
    public GameObject noAICar;
    [Header("Cameras")]
    public GameObject cenitalCamera;
    [Header("Lighting")]
    public GameObject globalLight;
    public GameObject streetLights;
    public GameObject playerLights;
    public GameObject navMeshLights;
    public GameObject noAILights;
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
    public GameObject path;
    public Transform[] objectives;
    private bool enablePath;
    private string enabledPathText;
    private string disabledPathText;

    // Start is called before the first frame update
    void Start()
    {
        playerCar.SetActive(false);
        navMeshCar.SetActive(false);
        navMeshAgent.SetActive(false);
        noAICar.SetActive(false);
        kmhText.enabled = false;
        speedText.enabled = false;
        nightDay = true;
        streetLights.SetActive(true);
        dayLight = new Color(250f/255f, 244f/255f, 214f/255f, 255f/255f);
        nightLight = new Color(0, 0, 0, 0);
        enablePath = false;
        enabledPathText = "Disable Path";
        disabledPathText = "Enable Path";
        objectives = path.GetComponentsInChildren<Transform>();
        for (int i = 1; i < objectives.Length; i++)
        {
            objectives[i].GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public void PlayerButtonOnClick()
    {
        playerCar.SetActive(true);
        navMeshCar.SetActive(false);
        navMeshAgent.SetActive(false);
        noAICar.SetActive(false);
        kmhText.enabled = true;
        speedText.enabled = true;
        cenitalCamera.SetActive(false);
    }

    public void NavMeshButtonOnClick()
    {
        playerCar.SetActive(false);
        navMeshCar.SetActive(true);
        navMeshAgent.SetActive(true);
        navMeshAgent.GetComponent<MeshRenderer>().enabled = false;
        noAICar.SetActive(false);
        kmhText.enabled = true;
        speedText.enabled = true;
        cenitalCamera.SetActive(false);
    }

    public void NoAIButtonOnClick()
    {
        playerCar.SetActive(false);
        navMeshCar.SetActive(false);
        navMeshAgent.SetActive(false);
        noAICar.SetActive(true);
        kmhText.enabled = true;
        speedText.enabled = true;
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
            RenderSettings.skybox = day;
            nightDay = false;
        } else
        {
            globalLight.GetComponent<Light>().color = nightLight;
            streetLights.SetActive(true);
            playerLights.SetActive(true);
            navMeshLights.SetActive(true);
            noAILights.SetActive(true);
            RenderSettings.skybox = night;
            nightDay = true;
        }
    }

    public void EnablePathOnClick()
    {
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

}
