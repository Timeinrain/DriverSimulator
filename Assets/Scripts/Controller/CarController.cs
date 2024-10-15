using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[System.Serializable]
public class AxleInfo {
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor;
    public bool steering;
}

[System.Serializable]
public enum CarType
{
    Manual,
    Automatic
}

public enum CarGearPositionManual
{
    Reverse,
    Neutral,
    First,
    Second,
    Third,
    Fourth,
    Fifth,
    Sixth
}

public enum CarGearPositionAutomatic
{
    Reverse,
    Neutral,
    Low,
    Drive,
    Second,
}

public enum EngineStatus
{
    On,
    Off,
    Stalled,//熄火
}

public enum HandBrakeStatus
{
    On,
    Off
}

public enum ClutchStatus
{
    On,
    HalfOn,
    Off
}
     
public class CarController : MonoBehaviour {
    
    Dictionary<CarGearPositionManual,float> gearSpeed = new Dictionary<CarGearPositionManual, float>()
    {
        {CarGearPositionManual.Reverse, -10},
        {CarGearPositionManual.Neutral, 0},
        {CarGearPositionManual.First, 10},
        {CarGearPositionManual.Second, 20},
        {CarGearPositionManual.Third, 30},
        {CarGearPositionManual.Fourth, 40},
        {CarGearPositionManual.Fifth, 50},
        {CarGearPositionManual.Sixth, 60}
    };
    
    Dictionary<CarGearPositionAutomatic,float> gearSpeedAutomatic = new Dictionary<CarGearPositionAutomatic, float>()
    {
        {CarGearPositionAutomatic.Reverse, -10},
        {CarGearPositionAutomatic.Neutral, 0},
        {CarGearPositionAutomatic.Low, 10},
        {CarGearPositionAutomatic.Drive, 30},
        {CarGearPositionAutomatic.Second, 50}
    };
    
    public List<AxleInfo> axleInfos; 
    public float maxMotorTorque;
    public float curMotorTorque;
    public float brakeMotorTorque;
    public float maxSteeringAngle;
    public float curSteeringAngle;

    public float CarSpeed = 0.0f;

    private float horizontalAdjustValue = 0.0f;
    public float horizontalAdjustFactor = 0.5f;
    
    public Vector2 horizontalInputRange = new Vector2(-90, 90);

    public HandBrakeStatus handBrakeStatus = HandBrakeStatus.On;
    
    public EngineStatus engineStatus = EngineStatus.Off;
    
    public CarType carType = CarType.Manual;

    //初始状态离合抬起
    public ClutchStatus clutchStatus = ClutchStatus.On;
    
    // 0- on 1 halfOn 2 off
    private int clutchValue = 2;

    private const float AcceleratorUnitValue = 100.0f;
    private const float BrakeUnitValue = 1000.0f;
    
    public float AcceleratorValue = 0.0f;
    public float BrakeValue = 0.0f;

    #region ViewLevel

    public float SteeringWheelAngle = 0.0f;
    public GameObject SteeringWheel;

    #endregion

    #region UIManager

    public UIManager uiManager;

    #endregion

    #region CarGear

    public CarGearPositionAutomatic carGearPositionAutomaticCurrent = CarGearPositionAutomatic.Neutral;
    public CarGearPositionManual carGearPositionManualCurrent = CarGearPositionManual.Neutral;

    #endregion

    public void Start()
    {
        uiManager = GetComponent<UIManager>();
        UpdateGearInfo();
        UpdateUIData();
    }
    
    public void UpdateUIData()
    {
        uiManager.UpdateCarTypeText(carType);
        uiManager.UpdateGearPositionText(carGearPositionAutomaticCurrent, carGearPositionManualCurrent);
        uiManager.UpdateHandBrakeText(handBrakeStatus);
        uiManager.UpdateAcceleratorText(AcceleratorValue);
        uiManager.UpdateBrakeText(BrakeValue);
        uiManager.UpdateClutchText(clutchStatus);
        uiManager.UpdateEngineStatus(engineStatus);
    }

    public bool CanSwitchGearCheck()
    {
        if(clutchStatus == ClutchStatus.Off)
        {
            return true;
        }
        return false;
    }
    
    public void DealSwitchGearFailed()
    {
        Debug.Log("熄火了！");
        engineStatus = EngineStatus.Stalled;
        UpdateUIData();
        //熄火
    }

    public void SwitchEngineState(EngineStatus engineStatus)
    {
        this.engineStatus = engineStatus;
    }

    // 查找相应的可视车轮
    // 正确应用变换
    public void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        if (collider.transform.childCount == 0) {
            return;
        }
     
        Transform visualWheel = collider.transform.GetChild(0);
     
        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);
     
        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }

    public void UpdateGearInfo()
    {
        var currentMinSpeedManual = gearSpeed[carGearPositionManualCurrent];
        var currentMinSpeedAuto = gearSpeedAutomatic[carGearPositionAutomaticCurrent];
        if (carType == CarType.Automatic)
        {
            if (carGearPositionAutomaticCurrent == CarGearPositionAutomatic.Reverse)
            {
                curMotorTorque = - MathF.Max(-gearSpeedAutomatic[CarGearPositionAutomatic.Reverse], 0);
            }
            else
            {
                curMotorTorque = MathF.Max(currentMinSpeedAuto, 0);
            }
        }
        else
        {
            if (carGearPositionManualCurrent == CarGearPositionManual.Reverse)
            {
                curMotorTorque = - MathF.Max(-gearSpeed[CarGearPositionManual.Reverse], 0);
            }
            else
            {
                curMotorTorque = MathF.Max(currentMinSpeedManual, 0);
            }
        }
        
        UpdateUIData();
    }
    
    // 等接入线性控制再直接替换
    private void OnTurn(InputValue value)
    {
        var inputValue = value.Get<float>();
        horizontalAdjustValue = inputValue;
    }
    
    private void OnEngine()
    {
        if (engineStatus == EngineStatus.Off)
        {
            engineStatus = EngineStatus.On;
        }
        else if (engineStatus == EngineStatus.Stalled)
        {
            engineStatus = EngineStatus.Off;
        }
        else if (engineStatus == EngineStatus.On)
        {
            engineStatus = EngineStatus.Off;
        }
        
        UpdateUIData();
    }
    
    private void OnHandBrake()
    {
        handBrakeStatus = handBrakeStatus == HandBrakeStatus.On ? HandBrakeStatus.Off : HandBrakeStatus.On;
        
        UpdateUIData();
    }
    
    private void OnBrake(InputValue value)
    {
        var inputValue = value.Get<float>();
        BrakeValue = BrakeUnitValue * inputValue;
        BrakeValue = Mathf.Max(BrakeValue, 0);
        
        UpdateUIData();
    }
    
    private void OnAccelerator(InputValue value)
    {
        var inputValue = value.Get<float>();
        AcceleratorValue = AcceleratorUnitValue * inputValue;
        AcceleratorValue = Mathf.Max(AcceleratorValue, 0);
        
        UpdateUIData();
    }
    
    private void OnSwitchCarType()
    {
        carType = carType == CarType.Automatic ? CarType.Manual : CarType.Automatic;
        
        UpdateUIData();
    }

    private void OnClutch(InputValue value)
    {
        var inputValue = value.Get<float>();
        Debug.Log("Clutch" + inputValue);
        clutchValue += (int) inputValue;
        clutchValue = Mathf.Clamp(clutchValue, 0, 2);
        switch (clutchValue)
        {
            case 0:
                clutchStatus = ClutchStatus.Off;
                break;
            case 1:
                clutchStatus = ClutchStatus.HalfOn;
                break;
            case 2:
                clutchStatus = ClutchStatus.On;
                break;
        }
        UpdateUIData();
    }

    #region GearPosition

    private void OnSwitchGearPositionManualGear1()
    {
        if (carType == CarType.Automatic)
        {
            return;
        }

        if (!CanSwitchGearCheck())
        {
            Debug.Log("Can't switch gear now" + "CurrentGearPosition" + carGearPositionManualCurrent + "TargetGearPosition" + CarGearPositionManual.First);
            DealSwitchGearFailed();
            return;
        }

        carGearPositionManualCurrent = CarGearPositionManual.First;
        curMotorTorque = gearSpeed[carGearPositionManualCurrent];
        curMotorTorque = MathF.Min(curMotorTorque, maxMotorTorque);
        
        UpdateGearInfo();
    }
    
    private void OnSwitchGearPositionManualGear2()
    {
        if (carType == CarType.Automatic)
        {
            return;
        }
        
        if (!CanSwitchGearCheck())
        {
            Debug.Log("Can't switch gear now" + "CurrentGearPosition" + carGearPositionManualCurrent + "TargetGearPosition" + CarGearPositionManual.First);
            DealSwitchGearFailed();
            return;
        }

        carGearPositionManualCurrent = CarGearPositionManual.Second;
        curMotorTorque = gearSpeed[carGearPositionManualCurrent];
        curMotorTorque = MathF.Min(curMotorTorque, maxMotorTorque);
        
        UpdateGearInfo();
    }
    
    private void OnSwitchGearPositionManualGear3()
    {
        if (carType == CarType.Automatic)
        {
            return;
        }
        
        if (!CanSwitchGearCheck())
        {
            Debug.Log("Can't switch gear now" + "CurrentGearPosition" + carGearPositionManualCurrent + "TargetGearPosition" + CarGearPositionManual.First);
            DealSwitchGearFailed();
            return;
        }

        carGearPositionManualCurrent = CarGearPositionManual.Third;
        curMotorTorque = gearSpeed[carGearPositionManualCurrent];
        curMotorTorque = MathF.Min(curMotorTorque, maxMotorTorque);
        
        UpdateGearInfo();
    }
    
    private void OnSwitchGearPositionManualGear4()
    {
        if (carType == CarType.Automatic)
        {
            return;
        }
        
        if (!CanSwitchGearCheck())
        {
            Debug.Log("Can't switch gear now" + "CurrentGearPosition" + carGearPositionManualCurrent + "TargetGearPosition" + CarGearPositionManual.First);
            DealSwitchGearFailed();
            return;
        }

        carGearPositionManualCurrent = CarGearPositionManual.Fourth;
        curMotorTorque = gearSpeed[carGearPositionManualCurrent];
        curMotorTorque = MathF.Min(curMotorTorque, maxMotorTorque);
        
        UpdateGearInfo();
    }
    
    private void OnSwitchGearPositionManualGear5()
    {
        if (carType == CarType.Automatic)
        {
            return;
        }
        
        if (!CanSwitchGearCheck())
        {
            Debug.Log("Can't switch gear now" + "CurrentGearPosition" + carGearPositionManualCurrent + "TargetGearPosition" + CarGearPositionManual.First);
            DealSwitchGearFailed();
            return;
        }

        carGearPositionManualCurrent = CarGearPositionManual.Fifth;
        curMotorTorque = gearSpeed[carGearPositionManualCurrent];
        curMotorTorque = MathF.Min(curMotorTorque, maxMotorTorque);
        
        UpdateGearInfo();
    }
    
    private void OnSwitchGearPositionManualGear6()
    {
        if (carType == CarType.Automatic)
        {
            return;
        }
        
        if (!CanSwitchGearCheck())
        {
            Debug.Log("Can't switch gear now" + "CurrentGearPosition" + carGearPositionManualCurrent + "TargetGearPosition" + CarGearPositionManual.First);
            DealSwitchGearFailed();
            return;
        }

        carGearPositionManualCurrent = CarGearPositionManual.Sixth;
        curMotorTorque = gearSpeed[carGearPositionManualCurrent];
        curMotorTorque = MathF.Min(curMotorTorque, maxMotorTorque);
        
        UpdateGearInfo();
    }
    
    private void OnSwitchGearPositionManualNeutral()
    {
        if (carType == CarType.Automatic)
        {
            return;
        }
        
        if (!CanSwitchGearCheck())
        {
            Debug.Log("Can't switch gear now" + "CurrentGearPosition" + carGearPositionManualCurrent + "TargetGearPosition" + CarGearPositionManual.First);
            DealSwitchGearFailed();
            return;
        }

        carGearPositionManualCurrent = CarGearPositionManual.Neutral;
        curMotorTorque = gearSpeed[carGearPositionManualCurrent];
        curMotorTorque = MathF.Min(curMotorTorque, maxMotorTorque);
        
        UpdateGearInfo();
    }
    
    private void OnSwitchGearPositionManualReverse()
    {
        if (carType == CarType.Automatic)
        {
            return;
        }
        
        if (!CanSwitchGearCheck())
        {
            Debug.Log("Can't switch gear now" + "CurrentGearPosition" + carGearPositionManualCurrent + "TargetGearPosition" + CarGearPositionManual.First);
            DealSwitchGearFailed();
            return;
        }

        carGearPositionManualCurrent = CarGearPositionManual.Reverse;
        curMotorTorque = gearSpeed[carGearPositionManualCurrent];
        curMotorTorque = MathF.Min(curMotorTorque, maxMotorTorque);
        
        UpdateGearInfo();
    }
    
    private void OnSwitchGearPositionAutoGearReverse()
    {
        if (carType == CarType.Manual)
        {
            return;
        }

        carGearPositionAutomaticCurrent = CarGearPositionAutomatic.Reverse;
        curMotorTorque = gearSpeedAutomatic[carGearPositionAutomaticCurrent];
        curMotorTorque = MathF.Min(curMotorTorque, maxMotorTorque);
        
        UpdateGearInfo();
    }
    
    private void OnSwitchGearPositionAutoGearNeutral()
    {
        if (carType == CarType.Manual)
        {
            return;
        }

        carGearPositionAutomaticCurrent = CarGearPositionAutomatic.Neutral;
        curMotorTorque = gearSpeedAutomatic[carGearPositionAutomaticCurrent];
        curMotorTorque = MathF.Min(curMotorTorque, maxMotorTorque);
        
        UpdateGearInfo();
    }
    
    private void OnSwitchGearPositionAutoGear1()
    {
        if (carType == CarType.Manual)
        {
            return;
        }

        carGearPositionAutomaticCurrent = CarGearPositionAutomatic.Low;
        curMotorTorque = gearSpeedAutomatic[carGearPositionAutomaticCurrent];
        curMotorTorque = MathF.Min(curMotorTorque, maxMotorTorque);
        
        UpdateGearInfo();
    }
    
    private void OnSwitchGearPositionAutoGear2()
    {
        if (carType == CarType.Manual)
        {
            return;
        }

        carGearPositionAutomaticCurrent = CarGearPositionAutomatic.Drive;
        curMotorTorque = gearSpeedAutomatic[carGearPositionAutomaticCurrent];
        curMotorTorque = MathF.Min(curMotorTorque, maxMotorTorque);
        
        UpdateGearInfo();
    }
    
    private void OnSwitchGearPositionAutoGear3()
    {
        if (carType == CarType.Manual)
        {
            return;
        }

        carGearPositionAutomaticCurrent = CarGearPositionAutomatic.Second;
        curMotorTorque = gearSpeedAutomatic[carGearPositionAutomaticCurrent];
        curMotorTorque = MathF.Min(curMotorTorque, maxMotorTorque);
        
        UpdateGearInfo();
    }

    #endregion
    
    public void FixedUpdate()
    {

        // ==============================
        
        curSteeringAngle += horizontalAdjustValue * horizontalAdjustFactor;
        curSteeringAngle = Mathf.Clamp(curSteeringAngle, horizontalInputRange.x, horizontalInputRange.y);
        
        //Debug.Log("curSteeringAngle" + curSteeringAngle);
        
        // ======== 方向盘表现层 =========
        SteeringWheelAngle = - curSteeringAngle * 6;
        var steeringWheelRotation = SteeringWheel.transform.rotation;
        var targetRotation = Quaternion.Euler(steeringWheelRotation.eulerAngles.x, steeringWheelRotation.eulerAngles.y,
            SteeringWheelAngle);
        SteeringWheel.transform.rotation = targetRotation;
        // ==============================
        
        // 等接入轮盘和油门 再更换为 curxxxx
        if(handBrakeStatus == HandBrakeStatus.On)
        {
            foreach (AxleInfo axleInfo in axleInfos) {
                axleInfo.leftWheel.brakeTorque = 100000;
                axleInfo.rightWheel.brakeTorque = 100000;
            }
        }
        else
        {
            foreach (AxleInfo axleInfo in axleInfos) {
                axleInfo.leftWheel.brakeTorque = 0;
                axleInfo.rightWheel.brakeTorque = 0;
            }
        }

        // 油门的倒挡处理
        if(carGearPositionAutomaticCurrent == CarGearPositionAutomatic.Reverse || carGearPositionManualCurrent == CarGearPositionManual.Reverse)
        {
            AcceleratorValue = - AcceleratorValue;
        }
        
        float motor = curMotorTorque + AcceleratorValue;
        motor = Mathf.Clamp(motor, -maxMotorTorque, maxMotorTorque);
        if(BrakeValue > 0)
        {
            motor = 0;
        }
        //Debug.Log("motor = " + motor + "AcceleratorValue = " + AcceleratorValue + "BrakeValue = " + BrakeValue + "curMotorTorque = " + curMotorTorque);
        float steering = curSteeringAngle;
     
        // ======== 引擎状态检测 =========
        if(engineStatus == EngineStatus.Off || engineStatus == EngineStatus.Stalled)
        {
            motor = 0;
        }
        
        // ======== 离合状态检测 =========
        //离合处于踩下状态
        if(clutchStatus == ClutchStatus.Off)
        {
            motor = 0;
        }
        
        // ==============================
        
        foreach (AxleInfo axleInfo in axleInfos) {
            if (axleInfo.steering) {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if (axleInfo.motor) {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
            
            if(BrakeValue > 0)
            {
                axleInfo.leftWheel.brakeTorque = BrakeValue;
                axleInfo.rightWheel.brakeTorque = BrakeValue;
            }
            
            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }
    }
}