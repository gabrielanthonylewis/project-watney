using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float speedMultiplier = 1.0f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float maxStanimaDuration = 5.0f;
    [SerializeField] private float stanimaRegenDuration = 1.0f;
    [SerializeField] private float backwardsSpeedMultiplier = 0.5f;
    [SerializeField] private float crouchSpeedMultiplier = 0.75f;
    [SerializeField] private float standingJumpHeight = 1.0f;
    [SerializeField] private float crouchedJumpHeight = 0.5f;
    [SerializeField] private Image stanimaFill = null;
    [SerializeField] private Animator animator = null;

    private readonly string sprintButtonName = "Fire3";
    private readonly string crouchButtonName = "Crouch";
    private readonly string jumpButtonName = "Jump";
    private readonly string vertAxisName = "Vertical";
    private readonly string horizAxisName = "Horizontal";
    private readonly string jumpParamName = "isJumping";
    private readonly string fallParamName = "isFalling";
    private readonly string crouchParamName = "isCrouching";
    private readonly string runParamName = "isRunning";
    private readonly string speedParamName = "Speed";
    private readonly string directionParamName = "Direction";
    private readonly float velocityMargin = 0.1f;

    private new Rigidbody rigidbody = null;
    private float currStanimaDuration;
    private float stanimaLerpT;
    private bool isRunning = false;
    private bool isCrouching = false;
    private float crouchedJumpVelocity;
    private float standingJumpVelocity;

    private void Awake()
    {
        this.rigidbody = this.GetComponent<Rigidbody>();        

        /* Physics Equations:
         * FROM maxHeight = (initialVelocity^2) / (2g)
         * TO velocity = Sqrt(maxHeight * -2g)  
         */
        this.crouchedJumpVelocity = Mathf.Sqrt(this.crouchedJumpHeight * (-2.0f * Physics.gravity.y));
        this.standingJumpVelocity = Mathf.Sqrt(this.standingJumpHeight * (-2.0f * Physics.gravity.y));

        this.ChangeStanima(this.maxStanimaDuration);
    }

    private void Update()
    {
        this.isCrouching = Input.GetButton(this.crouchButtonName);
        this.isRunning = Input.GetButton(this.sprintButtonName);

        if(Input.GetButton(this.jumpButtonName))
            this.TryJump();

        this.HandleAnimator();
    
        if(this.CanRegenStanima())
            this.RegenStanima();
    }

    private void FixedUpdate()
    {
        // Move player.
        Vector3 translationVector = this.CalculateTranslationVector(Time.fixedDeltaTime);
        this.rigidbody.MovePosition(this.rigidbody.position + translationVector);

        if(this.isRunning && translationVector.magnitude > 0.0f)
            this.ChangeStanima(Mathf.Max(this.currStanimaDuration - Time.fixedDeltaTime, 0.0f));
    }

    private void HandleAnimator()
    {
        if(this.animator == null)
            return;

        this.animator.SetFloat(this.speedParamName, Input.GetAxis(this.vertAxisName));
        this.animator.SetFloat(this.directionParamName, Input.GetAxis(this.horizAxisName));

        this.animator.SetBool(this.crouchParamName, this.isCrouching);
        this.animator.SetBool(this.runParamName, this.isRunning);

        float yVelocity = this.rigidbody.velocity.y;
        this.animator.SetBool(this.jumpParamName, (yVelocity > this.velocityMargin));
        this.animator.SetBool(this.fallParamName, (yVelocity < -this.velocityMargin));
    }

    private Vector3 CalculateTranslationVector(float dt)
    {
        float movementSpeed = this.speedMultiplier;
        Vector3 inputVector = new Vector3(Input.GetAxis(this.horizAxisName),
            0.0f, Input.GetAxis(this.vertAxisName));
     
        if(inputVector.z < 0.0f)
            movementSpeed *= this.backwardsSpeedMultiplier;
        if(this.isCrouching)
            movementSpeed *= this.crouchSpeedMultiplier;
        if(this.isRunning)
            movementSpeed *= this.sprintMultiplier;

        Quaternion movementDirection = Quaternion.Euler(0, this.transform.eulerAngles.y, 0);
        // Clamp used to solve issue where diagonal movement is faster.
        return Vector3.ClampMagnitude(movementDirection * inputVector, 1.0f) * movementSpeed * dt;
    }

    private void TryJump()
    {
        // If not on the ground then we can't jump.
        if(!Utils.Approximately(this.rigidbody.velocity.y, 0.0f, this.velocityMargin))
            return;

        Vector3 newVelocity = this.rigidbody.velocity;
        newVelocity.y += (this.isCrouching) ? this.crouchedJumpVelocity : this.standingJumpVelocity;
        this.rigidbody.velocity = newVelocity;
    }

    #region Stanima
    private bool CanRegenStanima()
    {
        return (!Input.GetButton(this.sprintButtonName) && 
            this.currStanimaDuration < this.maxStanimaDuration);
    }

    private void RegenStanima()
    {
        this.stanimaLerpT = Mathf.Clamp(this.stanimaLerpT + (Time.deltaTime / this.stanimaRegenDuration), 0.0f, 1.0f);
        this.ChangeStanima(Mathf.Lerp(0.0f, this.maxStanimaDuration, this.stanimaLerpT));
    }

    private void ChangeStanima(float stanima)
    {
        this.currStanimaDuration = stanima;

        this.stanimaLerpT = this.currStanimaDuration / this.maxStanimaDuration;

        if(this.stanimaFill != null)
            this.stanimaFill.fillAmount = this.stanimaLerpT;
    }
    #endregion   
}
