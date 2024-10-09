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
    Stalled,
}

public enum HandBrakeStatus
{
    On,
    Off
}
     
public class CarController : MonoBehaviour {
    
    Dictionary<CarGearPositionManual,float> gearSpeed = new Dictionary<CarGearPositionManual, float>()
    {
        {CarGearPositionManual.Reverse, -30},
        {CarGearPositionManual.Neutral, 0},
        {CarGearPositionManual.First, 30},
        {CarGearPositionManual.Second, 60},
        {CarGearPositionManual.Third, 90},
        {CarGearPositionManual.Fourth, 120},
        {CarGearPositionManual.Fifth, 150},
        {CarGearPositionManual.Sixth, 180}
    };
    
    Dictionary<CarGearPositionAutomatic,float> gearSpeedAutomatic = new Dictionary<CarGearPositionAutomatic, float>()
    {
        {CarGearPositionAutomatic.Reverse, -30},
        {CarGearPositionAutomatic.Neutral, 0},
        {CarGearPositionAutomatic.Low, 30},
        {CarGearPositionAutomatic.Drive, 75},
        {CarGearPositionAutomatic.Second, 120}
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

    private const float AcceleratorUnitValue = 100.0f;
    private const float BrakeUnitValue = 1000.0f;
    public float AcceleratorValue = 0.0f;
    public float BrakeValue = 0.0f;

    #region ViewLevel

    public float SteeringWheelAngle = 0.0f;
    public GameObject SteeringWheel;

    #endregion

    #region CarGear

    public CarGearPositionAutomatic carGearPositionAutomaticCurrent = CarGearPositionAutomatic.Neutral;
    public CarGearPositionManual carGearPositionManualCurrent = CarGearPositionManual.Neutral;

    #endregion


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
    }
    
    // 等接入线性控制再直接替换
    private void OnTurn(InputValue value)
    {
        var inputValue = value.Get<float>();
        horizontalAdjustValue = inputValue;
    }
    
    private void OnHandBrake()
    {
        handBrakeStatus = handBrakeStatus == HandBrakeStatus.On ? HandBrakeStatus.Off : HandBrakeStatus.On;
        Debug.Log("handBrakeStatus" + handBrakeStatus);
    }
    
    private void OnBrake(InputValue value)
    {
        var inputValue = value.Get<float>();
        BrakeValue = BrakeUnitValue * inputValue;
        BrakeValue = Mathf.Max(BrakeValue, 0);
        Debug.Log("BrakeValue" + BrakeValue);
    }
    
    private void OnAccelerator(InputValue value)
    {
        var inputValue = value.Get<float>();
        AcceleratorValue = AcceleratorUnitValue * inputValue;
        AcceleratorValue = Mathf.Max(AcceleratorValue, 0);
        Debug.Log("Accelorator = " + AcceleratorValue);
    }
    
    private void OnSwitchCarType()
    {
        carType = carType == CarType.Automatic ? CarType.Manual : CarType.Automatic;
    }

    #region GearPosition

    private void OnSwitchGearPositionManualGear1()
    {
        if (carType == CarType.Automatic)
        {
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
        curSteeringAngle += horizontalAdjustValue * horizontalAdjustFactor;
        curSteeringAngle = Mathf.Clamp(curSteeringAngle, horizontalInputRange.x, horizontalInputRange.y);
        
        Debug.Log("curSteeringAngle" + curSteeringAngle);
        
        // var wheelsSpeeds = new List<float>(4);
        // wheelsSpeeds[0] = axleInfos[0].leftWheel.rpm;
        // wheelsSpeeds[1] = axleInfos[0].rightWheel.rpm;
        // wheelsSpeeds[2] = axleInfos[1].leftWheel.rpm;
        // wheelsSpeeds[3] = axleInfos[1].rightWheel.rpm;
        // CarSpeed = Mathf.Max(wheelsSpeeds.ToArray());
        
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
        Debug.Log("motor = " + motor + "AcceleratorValue = " + AcceleratorValue + "BrakeValue = " + BrakeValue + "curMotorTorque = " + curMotorTorque);
        float steering = curSteeringAngle;
     
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