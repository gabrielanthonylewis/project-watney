using UnityEngine;
using Mirror;

public class PlayerLook : NetworkBehaviour
{
    public enum View
    {
        FirstPerson = 0,
        ThirdPerson = 1
    }

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
    [SerializeField] private float maxZoomDistance = 20;
    [SerializeField] private float minZoomDistance = 1;
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
    private readonly string zoomAxisName = "Mouse ScrollWheel";

    private float freeLookReturnLerpT;
    private Quaternion initialFreeLookRotation;
    private Quaternion finalFreeLookRotation;
    private bool isFreeLooking = false;
    private bool shouldReturnFromFreeLook = false;

    private float initialDistanceFromPoint;
    private float currZoomDistance;
    
    public View currentView = View.FirstPerson;
    private Camera currentCamera;
    private Vector3 targetThirdPersonCameraAngles;
    
    private float adjustmentDistance = -8.0f; // Camera distance will change if there is a collision.
    private Vector3 desiredCameraPosition = Vector3.zero; // Where the normal camera is meant to be (destination)
    private Vector3 adjustedCameraPosition = Vector3.zero; // If colliding use this Camera position, otherwise use normal destination (adjustedDestination)

    private void Start()
    {
        this.freeLookReturnLerpT = 1.0f;
        this.currentCamera = (this.currentView == View.FirstPerson) ?
            this.firstPersonCamera : this.thirdPersonCamera;
  
        Vector3 initialVector = (this.transform.position + Vector3.up) - this.thirdPersonCamera.transform.position;
        this.initialDistanceFromPoint = Vector3.Magnitude(initialVector);
        this.currZoomDistance = this.initialDistanceFromPoint;

        this.collision.Initialise(this.thirdPersonCamera);
        this.collision.UpdateCameraClipPoints(this.thirdPersonCamera.transform.position,
            this.thirdPersonCamera.transform.rotation, ref this.collision.adjustedCameraClipPoints);
        this.collision.UpdateCameraClipPoints(this.desiredCameraPosition,
            this.thirdPersonCamera.transform.rotation, ref this.collision.desiredCameraClipPoints);
    }

    private void Update()
    {
        if(NetworkClient.isConnected && !this.isLocalPlayer)
            return;

        if(Input.GetButtonDown(this.changeViewButtonName))
            this.SwitchView();

        this.HandleFreeLook(this.currentCamera);

        if(!this.isFreeLooking && !this.shouldReturnFromFreeLook)
            this.HandlePlayerRotation();

        this.HandleCameraRotation();
    }

    private void FixedUpdate()
    {
        if(NetworkClient.isConnected && !this.isLocalPlayer)
            return;

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
        // Reset.
        this.currentCamera.transform.localRotation = Quaternion.Euler(Vector3.zero);
        this.currZoomDistance = this.initialDistanceFromPoint;
        this.targetThirdPersonCameraAngles = Vector3.zero;
        this.thirdPersonCamera.transform.localPosition = 
            this.CalculateThirdPersonPos(Quaternion.Euler(this.targetThirdPersonCameraAngles));

        // Switch views.
        this.currentView = (this.currentView == View.FirstPerson) ? View.ThirdPerson : View.FirstPerson;

        this.currentCamera = (this.currentView == View.FirstPerson) ?
            this.firstPersonCamera : this.thirdPersonCamera;
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

        /* If returning then we want to update the rotation but now
         * allow the player to change it. */
        if(this.shouldReturnFromFreeLook)
            vertAngle = 0.0f;
              
        if(this.currentView == View.FirstPerson)
            this.HandleFirstPersonCamera(vertAngle);
        else
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

        this.thirdPersonCamera.transform.localRotation = Quaternion.Lerp(this.thirdPersonCamera.transform.localRotation,
            newRotation, this.thirdPersonLerpMultiplier * Time.deltaTime);

        this.thirdPersonCamera.transform.localPosition = Vector3.Lerp(this.thirdPersonCamera.transform.localPosition,
            this.CalculateThirdPersonPos(newRotation), this.thirdPersonLerpMultiplier * Time.deltaTime);
    }

    private Vector3 CalculateThirdPersonPos(Quaternion rotation)
    {
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -this.currZoomDistance);
        this.desiredCameraPosition = rotation * negDistance + Vector3.up;

        Vector3 negDistance2 = new Vector3(0.0f, 0.0f, -this.adjustmentDistance + this.safteyDistance);
        this.adjustedCameraPosition = rotation * negDistance2 + Vector3.up;

        return (collision.isColliding) ? this.adjustedCameraPosition : this.desiredCameraPosition;
    }

    private void HandleFreeLook(Camera camera)
    {
        // Start FreeLooking.
        if(Input.GetButtonDown(this.freeLookButtonName))
            this.initialFreeLookRotation = camera.transform.localRotation;

        // Actively FreeLooking.
        this.isFreeLooking = Input.GetButton(this.freeLookButtonName);
        if(this.isFreeLooking)
        {
            this.HandleHorizontalRotation(camera);

            if(this.currentView == View.ThirdPerson)
                this.HandleZoomDistance();
        }

        // Stop FreeLooking.
        if(Input.GetButtonUp(this.freeLookButtonName))
        {
            this.shouldReturnFromFreeLook = true;
            this.finalFreeLookRotation = camera.transform.localRotation;
            this.freeLookReturnLerpT = 0.0f;
        }

        // If stopped then lerp back.
        if(this.shouldReturnFromFreeLook)
            this.HandleFreeLookReturn(camera);
    }

    private void HandleHorizontalRotation(Camera camera)
    {
        float horizAngle = Input.GetAxisRaw(this.horizAxisName)
            * this.horizontalSpeedMultiplier * Time.deltaTime;

        if(this.currentView == View.FirstPerson)
        { 
            Vector3 newRotationAngles = camera.transform.localRotation.eulerAngles;
            newRotationAngles.y = Utils.ClampAngle(newRotationAngles.y + horizAngle,
                this.firstPersonLimitsYaw.x, this.firstPersonLimitsYaw.y);
            newRotationAngles.z = 0.0f; // Lock the roll.

            camera.transform.localRotation = Quaternion.Euler(newRotationAngles);
        }    
        else
            this.targetThirdPersonCameraAngles.y += horizAngle;
    }

    private void HandleZoomDistance()
    {
        float newDistance = this.currZoomDistance - Input.GetAxisRaw(this.zoomAxisName)
            * this.scrollSpeedMultiplier * Time.deltaTime;
        this.currZoomDistance = Mathf.Clamp(newDistance, this.minZoomDistance, this.maxZoomDistance);
    }

    private void HandleFreeLookReturn(Camera camera)
    {
        float duration = (this.currentView == View.FirstPerson) ?
            this.timeToResetFromFreeLook : this.timeToResetFromFreeLookThirdPerson;
        
        this.freeLookReturnLerpT = Mathf.Clamp(this.freeLookReturnLerpT +
            (Time.deltaTime / duration), 0.0f, 1.0f);

        Quaternion newRotation = Quaternion.Lerp(this.finalFreeLookRotation,
            initialFreeLookRotation, this.freeLookReturnLerpT);

        if (this.currentView == View.FirstPerson)
            camera.transform.localRotation = newRotation;
        else
            this.targetThirdPersonCameraAngles = newRotation.eulerAngles;

        if(this.freeLookReturnLerpT == 1.0f)
            this.shouldReturnFromFreeLook = false;
    }

    public void AddChangeViewCallback(ChangeViewDelegate myCallback)
    {
        this.changeViewCallback += myCallback;
    }

    public bool IsFreeLookActive()
    {
        return (this.isFreeLooking || this.shouldReturnFromFreeLook);
    }

    public Camera GetCurrentCamera()
    {
        return this.currentCamera;
    }
}
