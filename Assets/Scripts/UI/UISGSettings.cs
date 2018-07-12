using System;
using UnityEngine;
using UnityEngine.UI;


public class UISGSettings : MonoBehaviour
{
    public static UISGSettings instance = null;
    public static SGSettings settings = new SGSettings("default");
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }
    public SGSettings GetSettings()
    {
        return settings;
    }
    #region Simple generator section
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

    public void SetMinRoomNumber(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.roomNumberMin = Int32.Parse(value);
    }

    public void SetMaxRoomNumber(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.roomNumberMax = Int32.Parse(value);
    }

    public void SetMinRoomWidth(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.roomWidthMin = Int32.Parse(value);
    }

    public void SetMaxRoomWidth(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.roomWidthMax = Int32.Parse(value);
    }

    public void SetMinRoomHeight(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.roomHeightMin = Int32.Parse(value);
    }

    public void SetMaxRoomHeight(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.roomHeightMax = Int32.Parse(value);
    }

    public void SetMinConnectionLength(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.connectionLengthMin = Int32.Parse(value);
    }

    public void SetMaxConnectionLength(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.connectionLengthMax = Int32.Parse(value);
    }
    #endregion
}