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
     
public class CarController : MonoBehaviour {
    
    Dictionary<CarGearPositionManual,float> gearSpeed = new Dictionary<CarGearPositionManual, float>()
    {
        {CarGearPositionManual.Reverse, -100},
        {CarGearPositionManual.Neutral, 0},
        {CarGearPositionManual.First, 100},
        {CarGearPositionManual.Second, 200},
        {CarGearPositionManual.Third, 300},
        {CarGearPositionManual.Fourth, 400},
        {CarGearPositionManual.Fifth, 500},
        {CarGearPositionManual.Sixth, 600}
    };
    
    Dictionary<CarGearPositionAutomatic,float> gearSpeedAutomatic = new Dictionary<CarGearPositionAutomatic, float>()
    {
        {CarGearPositionAutomatic.Reverse, -100},
        {CarGearPositionAutomatic.Neutral, 0},
        {CarGearPositionAutomatic.Low, 100},
        {CarGearPositionAutomatic.Drive, 300},
        {CarGearPositionAutomatic.Second, 600}
    };
    
    public List<AxleInfo> axleInfos; 
    public float maxMotorTorque;
    public float curMotorTorque;
    public float maxSteeringAngle;
    public float curSteeringAngle;
    
    public CarType carType = CarType.Manual;

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

    // 现在只是简单地用后退来刹车，真实刹车逻辑应该是减速到0
    private void OnMove(InputValue value)
    {
        var inputVector = value.Get<Vector2>();
        Debug.Log(inputVector);
        var currentMinSpeedManual = gearSpeed[carGearPositionManualCurrent];
        var currentMinSpeedAuto = gearSpeedAutomatic[carGearPositionAutomaticCurrent];
        if (carType == CarType.Automatic)
        {
            if (carGearPositionAutomaticCurrent == CarGearPositionAutomatic.Reverse)
            {
                curMotorTorque = - MathF.Max(-gearSpeedAutomatic[CarGearPositionAutomatic.Reverse], inputVector.y * curMotorTorque);
            }
            else
            {
                if (inputVector.y > 0)
                {
                    curMotorTorque = MathF.Max(currentMinSpeedAuto, inputVector.y * maxMotorTorque);
                }
                else
                {
                    curMotorTorque = inputVector.y * maxMotorTorque;
                }
            }
            curSteeringAngle = inputVector.x * maxSteeringAngle;
        }
        else
        {
            if (carGearPositionManualCurrent == CarGearPositionManual.Reverse)
            {
                curMotorTorque = - MathF.Max(-gearSpeed[CarGearPositionManual.Reverse], inputVector.y * curMotorTorque);
            }
            else
            {
                if (inputVector.y > 0)
                {
                    curMotorTorque = MathF.Max(currentMinSpeedManual, inputVector.y * maxMotorTorque);
                }
                else
                {
                    curMotorTorque = inputVector.y * maxMotorTorque;
                }
            }
            curSteeringAngle = inputVector.x * maxSteeringAngle;
        }
        
    }

    private void OnSwitchCarType()
    {
        carType = carType == CarType.Automatic ? CarType.Manual : CarType.Automatic;
    }

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
    
    public void FixedUpdate()
    {
        // 等接入轮盘和油门 再更换为 curxxxx
        float motor = curMotorTorque;
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
            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }
    }
}