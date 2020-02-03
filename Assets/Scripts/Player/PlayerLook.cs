using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerLook : NetworkBehaviour
{
    public enum View
    {
        FirstPerson = 0,
        ThirdPerson = 1
    }
    
    [SerializeField]
    private float horizontalSpeedMultiplier = 1.0f;

    [SerializeField]
    private float verticalSpeedMultiplier = 1.0f;

    [SerializeField]
    private Vector2 firstPersonLimitsPitch = new Vector2(-50.0f, 50.0f);

    [SerializeField]
    private Vector2 firstPersonLimitsYaw = new Vector2(-60.0f, 60.0f);

    [SerializeField]
    private Vector2 thirdPersonLimitsPitch = new Vector2(-26.0f, 60.0f);

    [SerializeField]
    private float timeToResetFromFreeLook = 0.5f;

    [SerializeField]
    private float timeToResetFromFreeLookThirdPerson = 0.5f;

    [SerializeField]
    private Camera firstPersonCamera = null;

    [SerializeField]
    private Camera thirdPersonCamera = null;

    [SerializeField]
    private float maxScrollDistanceFromPoint = 20;

    [SerializeField]
    private float minScrollDistanceFromPoint = 1;

    [SerializeField]
    private float scrollSpeedMultiplier = 5.0f;

    [SerializeField]
    private float thirdPersonLerpMultiplier = 10.0f;

    public View currentCameraView = View.FirstPerson;
    public Camera currentCamera;

    private readonly string freeLookButtonName = "FreeLook";
    private readonly string changeViewButtonName = "ChangeView";

    public bool isFreeLooking = false;
    public bool shouldLerpFromFreeLook = false;
    private float freeLookInterpolationValue;
    private Vector2 initialFreeLookLerpValues;

    private float initialDistanceFromPoint;
    private float currentDistanceFromPoint;

    private Vector3 targetThirdPersonCameraAngles;
    private Vector3 targetThirdPersonPlayerAngles;

    [SerializeField]
    private CollisionHandler collision = new CollisionHandler();

    private float adjustmentDistance = -8.0f; // Camera distance will change if there is a collision.

    private Vector3 desiredCameraPosition = Vector3.zero; // Where the normal camera is meant to be (destination)
    private Vector3 adjustedCameraPosition = Vector3.zero; // If colliding use this Camera position, otherwise use normal destination (adjustedDestination)

    [SerializeField]
    private bool drawDesiredCollisionLines = true;
    [SerializeField]
    private bool drawAdjustedCollisionLines = true;

    public delegate void ChangeViewDelegate(View newView);
    protected ChangeViewDelegate changeViewCallback;

    private Vector3 initialFreeLookAngles;
    private Quaternion initialFreeLookRot;
    private Vector3 initialFreeLookPos;

    public void AddChangeViewCallback(ChangeViewDelegate myCallback)
    {
        this.changeViewCallback += myCallback;
    }

    private void Start()
    {
        this.freeLookInterpolationValue = 1.0f;
        this.currentCamera = this.GetCurrentCamera();
  
        Vector3 initialVector = (this.transform.position + Vector3.up) - this.thirdPersonCamera.transform.position;
        this.initialDistanceFromPoint = Vector3.Magnitude(initialVector);
        this.currentDistanceFromPoint = this.initialDistanceFromPoint;
        this.targetThirdPersonPlayerAngles = this.transform.rotation.eulerAngles;

        collision.Initialise(this.thirdPersonCamera);
        collision.UpdateCameraClipPoints(this.thirdPersonCamera.transform.position, this.thirdPersonCamera.transform.rotation, ref collision.adjustedCameraClipPoints);
        collision.UpdateCameraClipPoints(this.desiredCameraPosition, this.thirdPersonCamera.transform.rotation, ref collision.desiredCameraClipPoints);
    }

    void Update()
    {
        if (NetworkClient.isConnected && !this.isLocalPlayer)
            return;

        this.HandleViewChange();

        this.HandleFreeLook(this.currentCamera);

        if (!this.isFreeLooking)
            this.HandlePlayerRotation();

        this.HandleCameraRotation();
    }

    private void FixedUpdate()
    {
        if (NetworkClient.isConnected && !this.isLocalPlayer)
            return;

        if (this.currentCameraView == View.ThirdPerson)
        {
            Vector3 playerPos = this.transform.position;
            Vector3 cameraPos = this.thirdPersonCamera.transform.position;
            Quaternion cameraRot = this.thirdPersonCamera.transform.rotation;

            collision.UpdateCameraClipPoints(cameraPos, cameraRot, ref collision.adjustedCameraClipPoints);
            collision.UpdateCameraClipPoints(this.transform.TransformPoint(this.desiredCameraPosition), cameraRot, ref collision.desiredCameraClipPoints);

            for (int i = 0; i < collision.desiredCameraClipPoints.Length; i++)
            {
                if (this.drawDesiredCollisionLines)
                    Debug.DrawLine(playerPos, collision.desiredCameraClipPoints[i], Color.white); // where our camera wants to be

                if (this.drawAdjustedCollisionLines)
                    Debug.DrawLine(playerPos, collision.adjustedCameraClipPoints[i], Color.green); // where our camera be because of collisions
            }

            collision.CheckColliding(playerPos);
            this.adjustmentDistance = collision.GetAdjustedDistanceWithRayFrom(playerPos);
        }
    }

    private void HandleViewChange()
    {
        bool viewButtonInput = Input.GetButtonDown(this.changeViewButtonName);

        if(viewButtonInput)
        {
            View newView = (this.currentCameraView == View.FirstPerson) ? View.ThirdPerson : View.FirstPerson;
            if (newView == View.FirstPerson)
            {
                this.thirdPersonCamera.gameObject.SetActive(false);
                this.firstPersonCamera.gameObject.SetActive(true);
            }
            else if(newView == View.ThirdPerson)
            {
                this.firstPersonCamera.gameObject.SetActive(false);
                this.thirdPersonCamera.gameObject.SetActive(true);
            }

            this.currentCameraView = newView;
            this.currentCamera = this.GetCurrentCamera();
            this.currentCamera.transform.localRotation = Quaternion.Euler(Vector3.zero);
            this.currentDistanceFromPoint = this.initialDistanceFromPoint;
            this.targetThirdPersonCameraAngles = Vector3.zero;
            this.targetThirdPersonPlayerAngles = this.transform.localRotation.eulerAngles;

            Quaternion newRotation = Quaternion.Euler(this.targetThirdPersonCameraAngles);
            Vector3 negDistance = new Vector3(0, 0, -this.currentDistanceFromPoint);
            Vector3 newPosition = newRotation * negDistance + Vector3.up;
            this.thirdPersonCamera.transform.localPosition = newPosition;

            this.changeViewCallback.Invoke(newView);
        }
    }

    private Camera GetCurrentCamera()
    {
        return (this.currentCameraView == View.FirstPerson)
            ? this.firstPersonCamera : this.thirdPersonCamera;
    }

    private void HandlePlayerRotation()
    {
        if (this.shouldLerpFromFreeLook)
            return;

        float horizInput = Input.GetAxisRaw("Mouse X");
        float horizAngle = horizInput * this.horizontalSpeedMultiplier * Time.deltaTime;

        if (this.currentCameraView == View.FirstPerson)
            this.transform.Rotate(this.transform.up, horizAngle);
        else
        {
            this.targetThirdPersonPlayerAngles += this.transform.up * horizAngle;

            Quaternion newRotation = Quaternion.Euler(this.targetThirdPersonPlayerAngles);
            this.transform.localRotation = Quaternion.Slerp(this.transform.localRotation, newRotation, this.thirdPersonLerpMultiplier * Time.deltaTime);
        }
    }

    private void HandleFreeLook(Camera camera)
    {
        bool freeLookInput = Input.GetButton(this.freeLookButtonName);

        // Free look stopped.
        if (this.isFreeLooking && !freeLookInput
                && Input.GetButtonUp(this.freeLookButtonName))
        {
            this.shouldLerpFromFreeLook = true;
            this.initialFreeLookLerpValues.x = Utils.ConvertTo180Degrees(camera.transform.localRotation.eulerAngles.x);
            this.initialFreeLookLerpValues.y = Utils.ConvertTo180Degrees(camera.transform.localRotation.eulerAngles.y);
            this.initialFreeLookPos = this.thirdPersonCamera.transform.localPosition;
            this.initialFreeLookRot = this.thirdPersonCamera.transform.localRotation;
            this.freeLookInterpolationValue = 0;
        }

        // Lerp back to center of the screen.
        if (this.shouldLerpFromFreeLook)
        {

            if (this.currentCameraView == View.FirstPerson)
            {
                this.freeLookInterpolationValue += Time.deltaTime / this.timeToResetFromFreeLook;
                this.freeLookInterpolationValue = Mathf.Clamp(this.freeLookInterpolationValue, 0.0f, 1.0f);

                Vector3 newRotationAngles = camera.transform.localRotation.eulerAngles;
                newRotationAngles.y = Mathf.Lerp(this.initialFreeLookLerpValues.y, 0.0f, this.freeLookInterpolationValue);


                newRotationAngles.x = Mathf.Lerp(this.initialFreeLookLerpValues.x, 0.0f, this.freeLookInterpolationValue);
                if (Utils.Approximately(newRotationAngles.y, 0.0f, 0.01f))
                {
                    newRotationAngles.y = 0.0f;
                    this.shouldLerpFromFreeLook = false;
                }

                camera.transform.localRotation = Quaternion.Euler(newRotationAngles);
            }
            else
            {
                this.freeLookInterpolationValue += Time.deltaTime / this.timeToResetFromFreeLookThirdPerson;
                this.freeLookInterpolationValue = Mathf.Clamp(this.freeLookInterpolationValue, 0.0f, 1.0f);

                Vector3 newRotationAngles = camera.transform.localRotation.eulerAngles;
                newRotationAngles.y = Mathf.Lerp(this.initialFreeLookLerpValues.y, 0.0f, this.freeLookInterpolationValue);


                newRotationAngles.x = Mathf.Lerp(this.initialFreeLookLerpValues.x, this.initialFreeLookAngles.x, this.freeLookInterpolationValue);

                bool cameraYCorrect = false;
                if (Utils.Approximately(camera.transform.localRotation.eulerAngles.y, 0.0f, 0.01f)) // 0.00015f for accruacy but locks camera as too precise 
                {
                    cameraYCorrect = true; 
                }
                bool cameraXCorrect = false;
                if (Utils.Approximately(camera.transform.localRotation.eulerAngles.x, this.initialFreeLookAngles.x, 0.01f)) // 0.00015f for accruacy but locks camera as too precise 
                {
                    cameraXCorrect = true;
                }

                if (cameraXCorrect && cameraYCorrect)
                {
                    newRotationAngles = this.initialFreeLookAngles;
                    // newRotationAngles.y = 0.0f;
                    this.shouldLerpFromFreeLook = false;
                }

                this.targetThirdPersonCameraAngles = newRotationAngles;


                this.targetThirdPersonCameraAngles.x = Utils.ClampAngle(this.targetThirdPersonCameraAngles.x, this.thirdPersonLimitsPitch.x, this.thirdPersonLimitsPitch.y);
                this.targetThirdPersonCameraAngles.z = 0.0f;

                // Rotation
                Quaternion newRotation = Quaternion.Euler(this.targetThirdPersonCameraAngles);
                camera.transform.localRotation = Quaternion.Slerp(this.initialFreeLookRot, newRotation, this.freeLookInterpolationValue);

                // Position
                Vector3 negDistance = new Vector3(0, 0, -this.currentDistanceFromPoint);
                this.desiredCameraPosition = newRotation * negDistance + Vector3.up;

                const float safteyDistance = 1.0f; // So will go little closer to player
                Vector3 negDistance2 = new Vector3(0, 0, -this.adjustmentDistance + safteyDistance);
                this.adjustedCameraPosition = newRotation * negDistance2 + Vector3.up;

                Vector3 newCameraPosition = (collision.isColliding) ? this.adjustedCameraPosition : this.desiredCameraPosition;
                camera.transform.localPosition = Vector3.Lerp(this.initialFreeLookPos, newCameraPosition, this.freeLookInterpolationValue);
            }
        }

        if(!this.isFreeLooking && freeLookInput)
        {
            this.initialFreeLookAngles = this.thirdPersonCamera.transform.localRotation.eulerAngles; 
        }

        this.isFreeLooking = freeLookInput;

        if (this.isFreeLooking)
        {
            float horizInput = Input.GetAxisRaw("Mouse X");
            float horizAngle = horizInput * this.horizontalSpeedMultiplier * Time.deltaTime;

            Vector3 newRotationAngles = camera.transform.localRotation.eulerAngles;
            newRotationAngles.z = 0.0f; // Lock the roll.
            newRotationAngles.y = Utils.ConvertTo180Degrees(newRotationAngles.y + horizAngle);

            // Clamp within the specified range.
            if(this.currentCameraView == View.FirstPerson)
                newRotationAngles.y = Mathf.Clamp(newRotationAngles.y, this.firstPersonLimitsYaw.x, this.firstPersonLimitsYaw.y);

            if(this.currentCameraView == View.FirstPerson)
                camera.transform.localRotation = Quaternion.Euler(newRotationAngles);
            else
            {
                // Lerp happens in HandleThirdPersonCamera (todo: should move?).
                this.targetThirdPersonCameraAngles.y += horizAngle;
            }
            if(this.currentCameraView == View.ThirdPerson)
            {
                float scrollInput = Input.GetAxisRaw("Mouse ScrollWheel");
                this.currentDistanceFromPoint += -scrollInput * this.scrollSpeedMultiplier  * Time.deltaTime;
                this.currentDistanceFromPoint = Mathf.Clamp(this.currentDistanceFromPoint, this.minScrollDistanceFromPoint,
                    this.maxScrollDistanceFromPoint);
            }
        }
    }

    private void HandleCameraRotation()
    {
        float vertInput = Input.GetAxisRaw("Mouse Y");
        float vertAngle = -vertInput * this.verticalSpeedMultiplier * Time.deltaTime;
              
        if (this.currentCameraView == View.FirstPerson)
            this.HandleFirstPersonCamera(vertAngle);
        else
            this.HandleThirdPersonCamera(vertAngle);
    }

    private void HandleFirstPersonCamera(float newAngle)
    {
        Camera camera = this.firstPersonCamera;

        Vector3 newRotationAngles = camera.transform.localRotation.eulerAngles;
        newRotationAngles.z = 0.0f; // Lock the roll.
        newRotationAngles.x = Utils.ConvertTo180Degrees(newRotationAngles.x + newAngle);

        // Clamp within the specified range.
        newRotationAngles.x = Mathf.Clamp(newRotationAngles.x, this.firstPersonLimitsPitch.x, this.firstPersonLimitsPitch.y);

        camera.transform.localRotation = Quaternion.Euler(newRotationAngles);
    }

    private void HandleThirdPersonCamera(float newAngle)
    {
        if (this.shouldLerpFromFreeLook)
            return;

        Camera camera = this.thirdPersonCamera;

        this.targetThirdPersonCameraAngles.x += newAngle;
        this.targetThirdPersonCameraAngles.x = Utils.ClampAngle(this.targetThirdPersonCameraAngles.x, this.thirdPersonLimitsPitch.x, this.thirdPersonLimitsPitch.y);
        this.targetThirdPersonCameraAngles.z = 0.0f;

        // Rotation
        Quaternion newRotation = Quaternion.Euler(this.targetThirdPersonCameraAngles);
        camera.transform.localRotation = Quaternion.Slerp(camera.transform.localRotation, newRotation, this.thirdPersonLerpMultiplier * Time.deltaTime);

        // Position
        Vector3 negDistance = new Vector3(0, 0, -this.currentDistanceFromPoint);
        this.desiredCameraPosition = newRotation * negDistance + Vector3.up;

        const float safteyDistance = 1.0f; // So will go little closer to player
        Vector3 negDistance2 = new Vector3(0, 0, -this.adjustmentDistance + safteyDistance);
        this.adjustedCameraPosition = newRotation * negDistance2 + Vector3.up;

        Vector3 newCameraPosition = (collision.isColliding) ? this.adjustedCameraPosition : this.desiredCameraPosition;
        camera.transform.localPosition = Vector3.Lerp(camera.transform.localPosition, newCameraPosition, this.thirdPersonLerpMultiplier * Time.deltaTime);
    }
}
