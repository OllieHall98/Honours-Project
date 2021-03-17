using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using NaughtyAttributes;

namespace VHS
{    
    public class InputHandler : MonoBehaviour
    {
        #region Data
            [Space,Header("Input Data")]
            [SerializeField] private CameraInputData cameraInputData = null;
            [SerializeField] private MovementInputData movementInputData = null;
        #endregion

        #region BuiltIn Methods
            void Start()
            {
                cameraInputData.ResetInput();
                movementInputData.ResetInput();
            }

            void Update()
            {
                GetCameraInput();
                GetMovementInputData();
            }
        #endregion

        #region Custom Methods

        void GetCameraInput()
            {
                if (Input.GetJoystickNames().Length > 0)
                {
                    cameraInputData.InputVectorX = Input.GetAxis("Joystick X");
                    cameraInputData.InputVectorY = Input.GetAxis("Joystick Y");

                    cameraInputData.ZoomClicked = Input.GetButtonDown("Zoom");
                    cameraInputData.ZoomReleased = Input.GetButtonUp("Zoom");
                    
                    return;
                }
                
                
                cameraInputData.InputVectorX = Input.GetAxis("Mouse X");
                cameraInputData.InputVectorY = Input.GetAxis("Mouse Y");

                cameraInputData.ZoomClicked = Input.GetMouseButtonDown(1);
                cameraInputData.ZoomReleased = Input.GetMouseButtonUp(1);


            }

            void GetMovementInputData()
            {
                movementInputData.InputVectorX = Input.GetAxisRaw("Horizontal");
                movementInputData.InputVectorY = Input.GetAxisRaw("Vertical");

                if (Input.GetJoystickNames().Length > 0)
                {
                    movementInputData.RunClicked = Input.GetButtonDown("Sprint");
                    movementInputData.RunReleased = Input.GetButtonUp("Sprint");
                    
                    movementInputData.JumpClicked = Input.GetButtonDown("Jump");
                    movementInputData.CrouchClicked = Input.GetButtonUp("Crouch");
                }
                else
                {
                    movementInputData.RunClicked = Input.GetKeyDown(KeyCode.LeftShift);
                    movementInputData.RunReleased = Input.GetKeyUp(KeyCode.LeftShift);
                    
                    movementInputData.JumpClicked = Input.GetKeyDown(KeyCode.Space);
                    movementInputData.CrouchClicked = Input.GetKeyDown(KeyCode.C);
                }
                

                if(movementInputData.RunClicked)
                    movementInputData.IsRunning = true;

                if(movementInputData.RunReleased)
                    movementInputData.IsRunning = false;

                
            }
        #endregion
    }
}