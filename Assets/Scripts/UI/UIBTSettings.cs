using System;
using UnityEngine;
using UnityEngine.UI;


public class UIBTSettings : MonoBehaviour
{
    public static UIBTSettings instance = null;
    public static BTSettings settings = new BTSettings("default");
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }
    public BTSettings GetSettings()
    {
        return settings;
    }
    #region Binary tree section
    public void SetSeed(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.seed = value;
    }

    public void ToggleRandomSeed(GameObject inputField)
    {
        bool value = inputField.GetComponent<Toggle>().isOn;
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

    public void SetMinAreaWidth(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.minAreaWidth = Int32.Parse(value);
    }

    public void SetMinAreaHeight(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.minAreaHeight = Int32.Parse(value);
    }

    public void SetOffset(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.separationOffset = Int32.Parse(value);
    }

    public void SetMinRoomWidth(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.minRoomWidth = Int32.Parse(value);
    }

    public void SetMinRoomHeight(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.minRoomHeight = Int32.Parse(value);
    }

    public void SetMaxRoomWidth(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.maxRoomWidth = Int32.Parse(value);
    }

    public void SetMaxRoomHeight(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.maxRoomHeight = Int32.Parse(value);
    }

    public void ToggleORCheck(GameObject inputField)
    {
        bool value = inputField.GetComponent<Toggle>().isOn;
        Debug.Log(value);
        settings.useOrCheck = value;
    }

    public void SetMaxConnectionLenghtX(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.maxConnectionLengthX = Int32.Parse(value);
    }

    public void SetMaxConnectionLenghtY(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.maxConnectionLengthY = Int32.Parse(value);
    }
    #endregion
}