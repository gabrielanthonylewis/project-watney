using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public enum View
    {
        FirstPerson = 0,
        ThirdPerson = 1
    }

    public View currentView = View.FirstPerson;
    public Camera currentCamera;
    public bool isFreeLooking = false;
    public bool shouldLerpFromFreeLook = false;

    public delegate void ChangeViewDelegate(View newView);
    protected ChangeViewDelegate changeViewCallback;
    
    [SerializeField] private float horizontalSpeedMultiplier = 1.0f;
    [SerializeField] private float verticalSpeedMultiplier = 1.0f;
    [SerializeField] private Vector2 firstPersonLimitsPitch = new Vector2(-50.0f, 50.0f);
    [SerializeField] private Vector2 firstPersonLimitsYaw = new Vector2(-60.0f, 60.0f);
    [SerializeField] private Vector2 thirdPersonLimitsPitch = new Vector2(-26.0f, 60.0f);
    [SerializeField] private float timeToResetFromFreeLook = 0.5f;
    [SerializeField] private float timeToResetFromFreeLookThirdPerson = 0.5f;
    [SerializeField] private Camera firstPersonCamera = null;
    [SerializeField] private Camera thirdPersonCamera = null;
    [SerializeField] private float maxScrollDistanceFromPoint = 20;
    [SerializeField] private float minScrollDistanceFromPoint = 1;
    [SerializeField] private float scrollSpeedMultiplier = 5.0f;
    [SerializeField] private float thirdPersonLerpMultiplier = 10.0f;
    [SerializeField] private CameraCollisionDetection collision = new CameraCollisionDetection();
    [SerializeField] private bool drawDesiredCollisionLines = true;
    [SerializeField] private bool drawAdjustedCollisionLines = true;
    [SerializeField] private float safteyDistance = 1.0f;

    private readonly string freeLookButtonName = "FreeLook";
    private readonly string changeViewButtonName = "ChangeView";
    private readonly string horizAxisName = "Mouse X";
    private readonly string vertAxisName = "Mouse Y";

    private float freeLookInterpolationValue;
    private Vector2 initialFreeLookLerpValues;
    private float initialDistanceFromPoint;
    private float currentDistanceFromPoint;
    private Vector3 targetThirdPersonCameraAngles;
    private float adjustmentDistance = -8.0f; // Camera distance will change if there is a collision.
    private Vector3 desiredCameraPosition = Vector3.zero; // Where the normal camera is meant to be (destination)
    private Vector3 adjustedCameraPosition = Vector3.zero; // If colliding use this Camera position, otherwise use normal destination (adjustedDestination)
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
        this.currentCamera = (this.currentView == View.FirstPerson)
            ? this.firstPersonCamera : this.thirdPersonCamera;
  
        Vector3 initialVector = (this.transform.position + Vector3.up) - this.thirdPersonCamera.transform.position;
        this.initialDistanceFromPoint = Vector3.Magnitude(initialVector);
        this.currentDistanceFromPoint = this.initialDistanceFromPoint;

        this.collision.Initialise(this.thirdPersonCamera);
        this.collision.UpdateCameraClipPoints(this.thirdPersonCamera.transform.position,
            this.thirdPersonCamera.transform.rotation, ref this.collision.adjustedCameraClipPoints);
        this.collision.UpdateCameraClipPoints(this.desiredCameraPosition,
            this.thirdPersonCamera.transform.rotation, ref this.collision.desiredCameraClipPoints);
    }

    private void Update()
    {
        if(Input.GetButtonDown(this.changeViewButtonName))
            this.SwitchView();

        this.HandleFreeLook(this.currentCamera);

        if(!this.isFreeLooking && !this.shouldLerpFromFreeLook)
            this.HandlePlayerRotation();

        this.HandleCameraRotation();
    }

    private void FixedUpdate()
    {
        if(this.currentView != View.ThirdPerson)
            return;

        this.UpdateCameraCollisionData();

        if(this.drawDesiredCollisionLines || this.drawAdjustedCollisionLines)
            this.DrawDebugCollisionLines();
    }

    private void UpdateCameraCollisionData()
    {
        this.collision.UpdateCameraClipPoints(this.thirdPersonCamera.transform.position,
            this.thirdPersonCamera.transform.rotation, ref this.collision.adjustedCameraClipPoints);
        this.collision.UpdateCameraClipPoints(this.transform.TransformPoint(this.desiredCameraPosition),
            this.thirdPersonCamera.transform.rotation, ref this.collision.desiredCameraClipPoints);
        this.collision.CheckColliding(this.transform.position);
        this.adjustmentDistance = this.collision.GetAdjustedDistanceWithRayFrom(this.transform.position);   
    }

    private void DrawDebugCollisionLines()
    {
        for(int i = 0; i < this.collision.desiredCameraClipPoints.Length; i++)
        {
            // Where our camera wants to be.
            if(this.drawDesiredCollisionLines)
                Debug.DrawLine(this.transform.position, this.collision.desiredCameraClipPoints[i], Color.white); 

            // Where our camera be because of collisions.
            if(this.drawAdjustedCollisionLines)
                Debug.DrawLine(this.transform.position, this.collision.adjustedCameraClipPoints[i], Color.green);
        }
    }

    private void SwitchView()
    {
        // Reset
        this.currentCamera.transform.localRotation = Quaternion.Euler(Vector3.zero);
        this.currentDistanceFromPoint = this.initialDistanceFromPoint;
        this.targetThirdPersonCameraAngles = Vector3.zero;
        this.thirdPersonCamera.transform.localPosition = 
            this.CalculateThirdPersonPos(Quaternion.Euler(this.targetThirdPersonCameraAngles));

        // Switch
        this.currentView = (this.currentView == View.FirstPerson) ? View.ThirdPerson : View.FirstPerson;
        this.currentCamera = (this.currentView == View.FirstPerson) ? this.firstPersonCamera : this.thirdPersonCamera;
        this.firstPersonCamera.gameObject.SetActive(this.currentView == View.FirstPerson);
        this.thirdPersonCamera.gameObject.SetActive(this.currentView == View.ThirdPerson);
        this.changeViewCallback.Invoke(this.currentView);
    }

    // The camera affects the horizontal rotation of the player itself, not the camera.
    private void HandlePlayerRotation()
    {
        float newHorizAngle = Input.GetAxisRaw(this.horizAxisName)
            * this.horizontalSpeedMultiplier * Time.deltaTime;

        this.transform.Rotate(this.transform.up, newHorizAngle);
    }

    private void HandleCameraRotation()
    {
        float vertAngle = -Input.GetAxisRaw(this.vertAxisName)
            * this.verticalSpeedMultiplier * Time.deltaTime;
              
        if(this.currentView == View.FirstPerson)
            this.HandleFirstPersonCamera(vertAngle);
        else if(this.currentView == View.ThirdPerson && !this.shouldLerpFromFreeLook)
            this.HandleThirdPersonCamera(vertAngle);
    }

    // In First person there is no Lerping.
    private void HandleFirstPersonCamera(float newAngle)
    {
        Vector3 newRotationAngles = this.firstPersonCamera.transform.localRotation.eulerAngles;
        newRotationAngles.x = Utils.ClampAngle(newRotationAngles.x + newAngle,
            this.firstPersonLimitsPitch.x, this.firstPersonLimitsPitch.y);
        newRotationAngles.z = 0.0f; // Lock the roll.

        this.firstPersonCamera.transform.localRotation = Quaternion.Euler(newRotationAngles);
    }

    // In Third person we want to Lerp position and rotation.
    private void HandleThirdPersonCamera(float newAngle)
    {
        this.targetThirdPersonCameraAngles.x = Utils.ClampAngle(
            this.targetThirdPersonCameraAngles.x + newAngle,
            this.thirdPersonLimitsPitch.x, this.thirdPersonLimitsPitch.y);
        this.targetThirdPersonCameraAngles.z = 0.0f; // Lock the roll.

        Quaternion newRotation = Quaternion.Euler(this.targetThirdPersonCameraAngles);

        this.thirdPersonCamera.transform.localRotation = Quaternion.Slerp(this.thirdPersonCamera.transform.localRotation,
            newRotation, this.thirdPersonLerpMultiplier * Time.deltaTime);

        this.thirdPersonCamera.transform.localPosition = Vector3.Lerp(this.thirdPersonCamera.transform.localPosition,
            this.CalculateThirdPersonPos(newRotation), this.thirdPersonLerpMultiplier * Time.deltaTime);
    }

    private Vector3 CalculateThirdPersonPos(Quaternion rotation)
    {
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -this.currentDistanceFromPoint);
        this.desiredCameraPosition = rotation * negDistance + Vector3.up;

        Vector3 negDistance2 = new Vector3(0, 0, -this.adjustmentDistance + this.safteyDistance);
        this.adjustedCameraPosition = rotation * negDistance2 + Vector3.up;

        return (collision.isColliding) ? this.adjustedCameraPosition : this.desiredCameraPosition;
    }

    private void HandleFreeLook(Camera camera)
    {
        // Free look stopped.
        if (this.isFreeLooking && Input.GetButtonUp(this.freeLookButtonName))
        {
            this.shouldLerpFromFreeLook = true;
            this.initialFreeLookLerpValues.x = Utils.ConvertTo180Degrees(camera.transform.localRotation.eulerAngles.x);
            this.initialFreeLookLerpValues.y = Utils.ConvertTo180Degrees(camera.transform.localRotation.eulerAngles.y);
            this.initialFreeLookPos = camera.transform.localPosition;
            this.initialFreeLookRot = camera.transform.localRotation;
            this.freeLookInterpolationValue = 0;
        }

        // Lerp back to center of the screen.
//!!!!TODO: Move this stuff into a HandleFirstPerson and HandleThirdperson free look functions
        if (this.shouldLerpFromFreeLook)
        {
            if (this.currentView == View.FirstPerson)
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
                    cameraYCorrect = true; 
 
                bool cameraXCorrect = false;
                if (Utils.Approximately(camera.transform.localRotation.eulerAngles.x, this.initialFreeLookAngles.x, 0.01f)) // 0.00015f for accruacy but locks camera as too precise 
                    cameraXCorrect = true;

                if (cameraXCorrect && cameraYCorrect)
                {
                    newRotationAngles = this.initialFreeLookAngles;
                    // newRotationAngles.y = 0.0f;
                    this.shouldLerpFromFreeLook = false;
                }

                this.targetThirdPersonCameraAngles = newRotationAngles;
                this.targetThirdPersonCameraAngles.x = Utils.ClampAngle(this.targetThirdPersonCameraAngles.x, 
                    this.thirdPersonLimitsPitch.x, this.thirdPersonLimitsPitch.y);
                this.targetThirdPersonCameraAngles.z = 0.0f;

                Quaternion newRotation = Quaternion.Euler(this.targetThirdPersonCameraAngles);
                camera.transform.localRotation = Quaternion.Slerp(this.initialFreeLookRot,
                    newRotation, this.freeLookInterpolationValue);

                camera.transform.localPosition = Vector3.Lerp(this.initialFreeLookPos,
                    this.CalculateThirdPersonPos(newRotation), this.freeLookInterpolationValue);
            }
        }

        bool freeLookInput = Input.GetButton(this.freeLookButtonName);
        if(!this.isFreeLooking && freeLookInput)
            this.initialFreeLookAngles = camera.transform.localRotation.eulerAngles; 

        this.isFreeLooking = freeLookInput;
        if(this.isFreeLooking)
        {
            float horizInput = Input.GetAxisRaw(this.horizAxisName);
            float horizAngle = horizInput * this.horizontalSpeedMultiplier * Time.deltaTime;

            Vector3 newRotationAngles = camera.transform.localRotation.eulerAngles;
            newRotationAngles.z = 0.0f; // Lock the roll.
            newRotationAngles.y = Utils.ConvertTo180Degrees(newRotationAngles.y + horizAngle);

            // Clamp within the specified range.
            if(this.currentView == View.FirstPerson)
                newRotationAngles.y = Mathf.Clamp(newRotationAngles.y, this.firstPersonLimitsYaw.x, this.firstPersonLimitsYaw.y);

            if(this.currentView == View.FirstPerson)
                camera.transform.localRotation = Quaternion.Euler(newRotationAngles);
            else
                this.targetThirdPersonCameraAngles.y += horizAngle;

            if(this.currentView == View.ThirdPerson)
            {
                float scrollInput = Input.GetAxisRaw("Mouse ScrollWheel");
                this.currentDistanceFromPoint += -scrollInput * this.scrollSpeedMultiplier  * Time.deltaTime;
                this.currentDistanceFromPoint = Mathf.Clamp(this.currentDistanceFromPoint, this.minScrollDistanceFromPoint,
                    this.maxScrollDistanceFromPoint);
            }
        }
    }
}
