using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public CarController carController;

    private bool isReadMeOpen = true;
    
    public GameObject readMeCanvas;

    public GameObject CarTypeText;
    
    public GameObject GearPositionText;
    
    public GameObject HandBrakeText;
    
    public GameObject AcceleratorText;
    
    public GameObject BrakeText;
    
    public GameObject ClutchText;

    public GameObject EngineText;
    
    private void OnUICallReadMe()
    {
        isReadMeOpen = !isReadMeOpen;
        readMeCanvas.SetActive(isReadMeOpen);
    }
    
    public void UpdateCarTypeText(CarType carType)
    {
        var carTypeTMP = CarTypeText.GetComponent<TMP_Text>();
        carTypeTMP.text = "车类型: " + carType.ToString();
    }
    
    public void UpdateGearPositionText(CarGearPositionAutomatic gearPosition, CarGearPositionManual gearPositionManual)
    {
        if (carController.carType == CarType.Automatic)
        {
            GearPositionText.GetComponent<TMP_Text>().text = "档位: " + gearPosition;
        }
        else
        {
            GearPositionText.GetComponent<TMP_Text>().text = "档位: " + gearPositionManual;
        }
    }
    
    public void UpdateHandBrakeText(HandBrakeStatus isHandBrakeOn)
    {
        HandBrakeText.GetComponent<TMP_Text>().text = "手刹: " + isHandBrakeOn.ToString();
    }

    public void UpdateAcceleratorText(float acceleration)
    {
        AcceleratorText.GetComponent<TMP_Text>().text = "油门大小：" + acceleration;
    }
    
    public void UpdateBrakeText(float brake)
    {
        BrakeText.GetComponent<TMP_Text>().text = "刹车：" + (brake != 0);
    }
    
    public void UpdateClutchText(ClutchStatus clutch)
    {
        string[] clutchStatus = {"On","HalfOn" , "OFF"};
        switch (clutch)
        {
            case ClutchStatus.On:
                ClutchText.GetComponent<TMP_Text>().text = "离合：" + clutchStatus[0];
                break;
            case ClutchStatus.HalfOn:
                ClutchText.GetComponent<TMP_Text>().text = "离合：" + clutchStatus[1];
                break;
            case ClutchStatus.Off:
                ClutchText.GetComponent<TMP_Text>().text = "离合：" + clutchStatus[2];
                break;
        }
    }
    
    public void UpdateEngineStatus(EngineStatus engineStatus)
    {
        string[] engineStatusString = {"ON","OFF","STALLED"};
        if (engineStatus == EngineStatus.Stalled)
        {
            EngineText.GetComponent<TMP_Text>().color = Color.red;
        }
        else
        {
            EngineText.GetComponent<TMP_Text>().color = Color.white;
        }
        EngineText.GetComponent<TMP_Text>().text = "引擎：" + engineStatusString[(int)engineStatus];
    }
}
