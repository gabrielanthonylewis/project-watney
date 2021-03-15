using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float speedMultiplier = 1.0f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float sprintTimeToAccelerate = 1.0f;
    [SerializeField] private float sprintTimeToDecelerate = 1.0f;
    [SerializeField] private float maxSprintDuration = 1.0f;
    [SerializeField] private float timeToRegenStanima = 1.0f;
    [SerializeField] private float backwardsSpeedMultiplier = 0.5f;
    [SerializeField] private float crouchSpeedMultiplier = 0.75f;
    [SerializeField] private float proneSpeedMultiplier = 0.5f;
    [SerializeField] private float jumpHeight = 2.0f;
    [SerializeField] private float crouchJumpHeight = 0.5f;
    [SerializeField] private Image stanimaFill = null;
    [SerializeField] private Animator animator = null;

    private readonly string sprintButtonName = "Fire3";
    private readonly string crouchButtonName = "Crouch";
    private readonly string proneButtonName = "Prone";
    private readonly string jumpButtonName = "Jump";

    private float sprintSpeedTarget;
    private float initialLerpSprintMultiplier;
    private float sprintInterpolationValue;
    private bool shouldLerpSprint;

    private float stanimaDurationLeft;
    private float stanimaInterpolationValue;
    private float initialLerpStanimaDuration;
    private bool hasStanima;
    private bool canUseStanima;

    private bool isCrouching = false;
    private bool isProne = false;
    private bool isJumping = false;
    private bool isWalking = false;

    private new Rigidbody rigidbody = null;

    private void Start()
    {
        this.rigidbody = this.GetComponent<Rigidbody>();
    
        this.sprintSpeedTarget = 1.0f;
        this.sprintInterpolationValue = 1.0f;
        this.shouldLerpSprint = false;
        this.ChangeStanima(this.maxSprintDuration);
        this.stanimaInterpolationValue = 1.0f;
        this.hasStanima = (this.stanimaDurationLeft > 0.0f);
        this.canUseStanima = true;
    }

    private void Update()
    {
        // Handle the sprint state changing.
        if(Input.GetButtonDown(this.sprintButtonName))
            this.OnSprintInputChanged(true);
        else if(Input.GetButtonUp(this.sprintButtonName))
            this.OnSprintInputChanged(false);

        this.RegenStanima();
        this.HandleStances();
        this.HandleJump();

        // Animations
        float vertInput = Input.GetAxis("Vertical");
        this.animator.SetFloat("Speed", vertInput);
        this.animator.SetFloat("Direction", Input.GetAxis("Horizontal"));

        bool isRunning = (this.GetCurrentSprintMultiplier(Time.deltaTime) > 1.0f);
        this.animator.SetBool("isRunning", isRunning);

        float velocityY = this.rigidbody.velocity.y;
        if (this.isJumping)
        {
            this.animator.SetBool("isJumping", (velocityY >= 0.0f));
            this.animator.SetBool("isFalling", (velocityY < 0.0f));
        }  
    }

    private void FixedUpdate()
    {
        // Move player.
        Vector3 translationVector = this.CalculateTranslationVector(Time.fixedDeltaTime);
        this.rigidbody.MovePosition(this.rigidbody.position + translationVector);

        if (translationVector != Vector3.zero && !this.isWalking)
            this.isWalking = true;
        else if (translationVector == Vector3.zero && this.isWalking)
            this.isWalking = false;
    }

    private void ChangeStanima(float newValue)
    {
        this.stanimaDurationLeft = newValue;
        this.stanimaFill.fillAmount = this.stanimaDurationLeft / this.maxSprintDuration;
    }

    private void HandleJump()
    {
        if (this.isJumping)
        {
            bool onGround = Utils.Approximately(this.rigidbody.velocity.y, 0.0f, 0.001f);
            if (onGround)
            {
                this.animator.SetBool("isFalling", false);
                this.isJumping = false;
            }
        }

        if (this.isProne)
            return;

        if (!this.isJumping && Input.GetButton(this.jumpButtonName))
        {
            float jumpHeight = (this.isCrouching) ? this.crouchJumpHeight : this.jumpHeight;
        
            // maxHeight = (initialVelocity^2) / (2g)
            // velocity = Sqrt(maxHeight * -2g)
            Vector3 newVelocity = this.rigidbody.velocity;
            newVelocity.y += Mathf.Sqrt(jumpHeight * -2 * Physics.gravity.y);
            this.rigidbody.velocity = newVelocity;

            this.isJumping = true;
        }
    }

    private void HandleStances()
    {
        // Handle Crouching, cant go from prone to crouch.
        if (!this.isProne)
        {
            this.isCrouching = Input.GetButton(this.crouchButtonName);
            this.animator.SetBool("isCrouching", this.isCrouching);
        }

        // Handle Prone, can go from anything to prone.
        if (!this.isJumping)
        {
            this.isProne = Input.GetButton(this.proneButtonName);
        }

        // If both in both stances must be in prone and not crouch.
        if (this.isProne && this.isCrouching)
            this.isCrouching = false;
    }

    /**
     * Calculates the translation vector using the
     * user input (which also includes sprinting).
     * */
    private Vector3 CalculateTranslationVector(float dt)
    {
        Vector3 inputVector = Vector3.zero;
        inputVector.x = Input.GetAxis("Horizontal");
        inputVector.z = Input.GetAxis("Vertical");

        float currentSprintSpeedMultiplier = this.GetCurrentSprintMultiplier(dt);
        float movementSpeed = this.speedMultiplier * currentSprintSpeedMultiplier;

        if (inputVector.z < 0.0f)
            movementSpeed *= this.backwardsSpeedMultiplier;

        if (this.isCrouching)
            movementSpeed *= this.crouchSpeedMultiplier;
        if (this.isProne)
            movementSpeed *= this.proneSpeedMultiplier;

        Quaternion movementDirection = Quaternion.Euler(0, this.transform.eulerAngles.y, 0);

        // Clamp used to solve issue where diagonal movement is faster.
        return Vector3.ClampMagnitude(movementDirection * inputVector, 1.0f) * movementSpeed * dt;
    }

    /**
     * When the sprint input is pressed or let go of, reset the values so
     * that the lerp can start from the beginning.
     * */
    private void OnSprintInputChanged(bool isDown)
    {
        this.sprintInterpolationValue = 0.0f;
        this.initialLerpSprintMultiplier = (isDown || !this.hasStanima) ? 1.0f : this.sprintMultiplier;
        this.shouldLerpSprint = true;

        if(isDown)
        {
            this.canUseStanima = true;
        }

        if (!isDown)
        {
            this.stanimaInterpolationValue = this.stanimaDurationLeft / this.maxSprintDuration;
            this.initialLerpStanimaDuration = this.stanimaDurationLeft;

            if (this.stanimaInterpolationValue >= 1.0f)
                this.canUseStanima = true;
        }
    }

    /**
     * Calculate the current sprint speed along an interpolator.
     * */
    private float GetCurrentSprintMultiplier(float dt)
    {
        if (!this.shouldLerpSprint)
            return this.sprintSpeedTarget;

        bool isSprintInput = Input.GetButton(this.sprintButtonName);

        // Reduce stanima.
        Vector3 inputVector = Vector3.zero;
        inputVector.x = Input.GetAxis("Horizontal");
        inputVector.z = Input.GetAxis("Vertical");

        if (isSprintInput && this.canUseStanima && inputVector.magnitude > 0.0f)
        {
            if (this.stanimaDurationLeft > 0.0f)
                this.ChangeStanima(Mathf.Max(this.stanimaDurationLeft - dt, 0.0f));

            this.hasStanima = (this.stanimaDurationLeft > 0.0f);
            if (!this.hasStanima)
            {
                this.stanimaInterpolationValue = this.stanimaDurationLeft / this.maxSprintDuration;
                this.initialLerpStanimaDuration = this.stanimaDurationLeft;
                this.canUseStanima = false;
            }
        }

        // Adjust variables that are specific to acceleration/deceleration.
        float sprintTimeToLerp = (isSprintInput && this.hasStanima) ? this.sprintTimeToAccelerate 
            : this.sprintTimeToDecelerate;
        this.sprintSpeedTarget = (isSprintInput && this.hasStanima) ? this.sprintMultiplier : 1.0f;

        // Increase the sprint speed interpolation value.
        this.sprintInterpolationValue += dt / sprintTimeToLerp;

        if (this.sprintInterpolationValue > 1.0f)
        {
            this.sprintInterpolationValue = 1.0f;
            this.shouldLerpSprint = true;
        }

        // Return the current sprint speed along the interpolator.
        return Mathf.Lerp(this.initialLerpSprintMultiplier, this.sprintSpeedTarget,
            this.sprintInterpolationValue);
    }

    private void RegenStanima()
    {
        if(Input.GetButton(this.sprintButtonName))
            return;

        if((this.stanimaDurationLeft < this.maxSprintDuration) || this.canUseStanima == false)
        {
            this.stanimaInterpolationValue = Mathf.Min(this.stanimaInterpolationValue + (Time.deltaTime
                / this.timeToRegenStanima), 1.0f);

            this.ChangeStanima(Mathf.Lerp(this.initialLerpStanimaDuration, this.maxSprintDuration,
                this.stanimaInterpolationValue));
        }
    }
}
