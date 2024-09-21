using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CameraManager : MonoBehaviour
{
    private CarController carController;

    public Camera firstPersonCamera;
    public Camera thirdPersonCamera;
    
    public Image thirdPersonCameraImage;
    
    public bool shouldShowThirdPersonCamera = true;
    // Start is called before the first frame update
    void Start()
    {
        carController = GetComponent<CarController>();
    }

    public void OnSwitchTPCamera()
    {
        shouldShowThirdPersonCamera = !shouldShowThirdPersonCamera;
        if (shouldShowThirdPersonCamera)
        {
            thirdPersonCamera.enabled = true;
        }
        else
        {
            thirdPersonCamera.enabled = false;
        }
        thirdPersonCameraImage.enabled = shouldShowThirdPersonCamera;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
