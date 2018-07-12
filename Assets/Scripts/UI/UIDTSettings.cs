using System;
using UnityEngine;
using UnityEngine.UI;


public class UIDTSettings : MonoBehaviour
{
    public static UIDTSettings instance = null;
    public static DTSettings settings = new DTSettings("default");
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public DTSettings GetSettings()
    {
        return settings;
    }
    #region Delaunay Triangulation section
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

    public void SetInitialRoomsNumber(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.initialRooms = Int32.Parse(value);
    }

    public void SetInitialAreaWidth(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.initialAreaWidth = Int32.Parse(value);
    }

    public void SetInitialAreaHeight(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.initialAreaHeight = Int32.Parse(value);
    }

    public void SetInitialMinRoomWidth(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.initialRoomMinWidth = Int32.Parse(value);
    }

    public void SetInitialMinRoomHeight(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.initialRoomMinHeight = Int32.Parse(value);
    }

    public void SetInitialMaxRoomWidth(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.initialRoomMaxWidth = Int32.Parse(value);
    }

    public void SetInitialMaxRoomHeight(GameObject inputField)
    {
        string value = inputField.GetComponent<InputField>().text;
        Debug.Log(value);
        settings.initialRoomMaxHeight = Int32.Parse(value);
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

    public void ToggleORCheck(GameObject inputField)
    {
        bool value = inputField.GetComponent<Toggle>().isOn;
        Debug.Log(value);
        settings.useOrCheck = value;
    }

    public void SetAdditionalConnectionsPercent(GameObject slider)
    {
        float value = slider.GetComponent<Slider>().value;
        Debug.Log(value);
        settings.additionalConnections = (int)value;
    }
    #endregion
}