using System;
using UnityEngine;
using UnityEngine.UI;

public class UIBaseInterface : MonoBehaviour {
    public static UIBaseInterface instance = null;
    private static int generatorType = 0; //0 = DT, 1 = BT, 2 = CA, 3 = SG
    private static int previousGeneratorType;

    private static GameObject ScrollViewDT, ScrollViewBT, ScrollViewCA, ScrollViewSG;
    private static GameObject AdvSettings;

    // Use this for initialization
    void Start () {

    }
	
	void Awake () {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        SetupUI();
    }

    public void SetupUI()
    {
        ScrollViewDT = GameObject.Find("ScrollViewDT");
        ScrollViewBT = GameObject.Find("ScrollViewBT");
        ScrollViewCA = GameObject.Find("ScrollViewCA");
        ScrollViewSG = GameObject.Find("ScrollViewSG");
        ScrollViewDT.SetActive(true);
        ScrollViewBT.SetActive(false);
        ScrollViewCA.SetActive(false);
        ScrollViewSG.SetActive(false);

        AdvSettings = GameObject.Find("AdvSettings");
        AdvSettings.SetActive(false);

        generatorType = 0;
    }

    public void SetLevelGenerator(string generator)
    {
        GameManager.instance.SetLevelGenerator(generator);
    }

    public void NextLevel()
    {
        GameManager.instance.NextLevel();
    }

    public void ExitToMenu()
    {
        GameManager.instance.GameOver();
    }

    public void ToggleWindow(GameObject window)
    {
        if (window.active)
            window.SetActive(false);
        else
            window.SetActive(true);
    }

    public void ChangeGeneratorRight(GameObject genName)
    {
        previousGeneratorType = generatorType;
        generatorType++;
        if (generatorType > 3)
            generatorType = 0;
        UpdateGeneratorType(genName);
    }

    public void ChangeGeneratorLeft(GameObject genName)
    {
        previousGeneratorType = generatorType;
        generatorType--;
        if (generatorType < 0)
            generatorType = 3;
        UpdateGeneratorType(genName);
    }

    private void UpdateGeneratorType(GameObject genName)
    {
        switch (previousGeneratorType)
        {
            case 0:
                ScrollViewDT.SetActive(false);
                break;
            case 1:
                ScrollViewBT.SetActive(false);
                break;
            case 2:
                ScrollViewCA.SetActive(false);
                break;
            case 3:
                ScrollViewSG.SetActive(false);
                break;
            default:
                generatorType = 0;
                ScrollViewDT.SetActive(false);
                UpdateGeneratorType(genName);
                break;
        }

        switch (generatorType)
        {
            case 0:
                genName.GetComponent<Text>().text = "Delaunay triangulation";
                ScrollViewDT.SetActive(true);
                break;
            case 1:
                genName.GetComponent<Text>().text = "Binary tree";
                ScrollViewBT.SetActive(true);
                break;
            case 2:
                genName.GetComponent<Text>().text = "Cellular automaton";
                ScrollViewCA.SetActive(true);
                break;
            case 3:
                genName.GetComponent<Text>().text = "Simple generator";
                ScrollViewSG.SetActive(true);
                break;
            default:
                generatorType = 0;
                genName.GetComponent<Text>().text = "Delaunay triangulation";
                ScrollViewDT.SetActive(true);
                UpdateGeneratorType(genName);
                break;
        }
    }
}
