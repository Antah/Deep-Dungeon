using System;
using UnityEngine;
using UnityEngine.UI;

public class UICASettings : MonoBehaviour
{
    public static UICASettings instance = null;
    public static CASettings settings = new CASettings("default");
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }
    public CASettings GetSettings()
    {
        return settings;
    }
    #region Cellular automaton section
    public void SetSeed(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.seed = value;
    }

    public void ToggleRandomSeed(GameObject toggle)
    {
        bool value = toggle.GetComponent<Toggle>().isOn;
        Debug.Log(value);
        settings.useRandomSeed = value;
    }

    public void SetLevelWidth(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.levelWidth = Int32.Parse(value);
    }

    public void SetLevelHeight(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.levelHeight = Int32.Parse(value);
    }

    public void SetWallFillPercent(GameObject slider)
    {
        float value = slider.GetComponent<Slider>().value;
        Debug.Log(value);
        settings.wallFill = (int)value;
    }

    public void ToggleSecondRuleset(GameObject toggle)
    {
        bool value = toggle.GetComponent<Toggle>().isOn;
        Debug.Log(value);
        settings.enableSecondRuleset = value;
    }

    public void SetIterations1(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.iterations1 = Int32.Parse(value);
    }

    public void SetIterations2(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.iterations2 = Int32.Parse(value);
    }

    public void SelectNeighourhoodType1(GameObject dropdown)
    {
        string value = dropdown.GetComponent<Dropdown>().itemText.text;
        Debug.Log(value);
        if (value.Contains("Moore"))
            settings.ruleset1.neighbourhoodType = NeighbourhoodType.Moore;
        else
            settings.ruleset1.neighbourhoodType = NeighbourhoodType.Neuman;
    }

    public void SelectNeighourhoodType2(GameObject dropdown)
    {
        string value = dropdown.GetComponent<Dropdown>().itemText.text;
        Debug.Log(value);
        if (value.Contains("Moore"))
            settings.ruleset2.neighbourhoodType = NeighbourhoodType.Moore;
        else
            settings.ruleset2.neighbourhoodType = NeighbourhoodType.Neuman;
    }

    public void SetMinSurvive1(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.ruleset1.survMin = Int32.Parse(value);
    }

    public void SetMinSurvive2(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.ruleset2.survMin = Int32.Parse(value);
    }

    public void SetMaxSurvive1(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.ruleset1.survMax = Int32.Parse(value);
    }

    public void SetMaxSurvive2(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.ruleset2.survMax = Int32.Parse(value);
    }


    public void SetMinNew1(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.ruleset1.newMin = Int32.Parse(value);
    }

    public void SetMinNew2(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.ruleset2.newMin = Int32.Parse(value);
    }

    public void SetMaxNew1(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.ruleset1.newMax = Int32.Parse(value);
    }

    public void SetMaxNew2(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.ruleset2.newMax = Int32.Parse(value);
    }

    public void SetMinRoomSize(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.minRoomSize = Int32.Parse(value);
    }

    public void SetMaxRoomSize(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.maxRoomSize = Int32.Parse(value);
    }
    public void SetMinWallSize(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.minWallSize = Int32.Parse(value);
    }
    public void SetMaxWallSize(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.maxWallSize = Int32.Parse(value);
    }

    public void ToggleConnections(GameObject toggle)
    {
        bool value = toggle.GetComponent<Toggle>().isOn;
        Debug.Log(value);
        settings.useRandomSeed = value;
    }

    public void SelectConnectionType(GameObject dropdown)
    {
        string value = dropdown.GetComponent<Dropdown>().itemText.text;
        Debug.Log(value);
        if (value.Contains("Direct"))
            settings.useDirectConnections = true;
        else
            settings.useDirectConnections = false;
    }

    public void SetMaxLeavingConnections(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.maxLeavingConnections = Int32.Parse(value);
    }

    public void SetSizeFactor(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.connectionSizeFactor = Int32.Parse(value);
    }

    public void SetMaxConnectionLengthX(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.maxConnectionLengthX = Int32.Parse(value);
    }

    public void SetMaxConnectionLengthY(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.maxConnectionLengthY = Int32.Parse(value);
    }
    #endregion
}