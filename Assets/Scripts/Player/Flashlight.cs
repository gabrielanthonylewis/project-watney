using UnityEngine;
using Mirror;

[RequireComponent(typeof(PlayerLook))]
public class Flashlight : NetworkBehaviour
{
    [SerializeField] private GameObject flashLightPrefab = null;
    [SerializeField] private Transform firstPersonCameraHolder = null, thirdPersonCameraHolder = null;
    [SerializeField] private Camera thirdPersonCamera = null;
    [SerializeField] private bool currentVisibility = false;

    private Animator flashlightAnimator = null;
    private Transform flashlightTransform = null;
    private PlayerLook playerLook = null;

    private void Start()
    {
        this.playerLook = this.GetComponent<PlayerLook>();
        this.playerLook.AddChangeViewCallback(this.OnViewChanged);

        GameObject flashlight = GameObject.Instantiate(this.flashLightPrefab);
        this.flashlightAnimator = flashlight.GetComponent<Animator>();
        this.flashlightTransform = flashlight.transform;
        this.UpdateFlashlightParent(this.playerLook.currentCameraView);

        // Send flash state to other players.
        if(NetworkClient.isConnected && this.isLocalPlayer)
            this.SendChangeVisibility(this.currentVisibility);
    }

    private void Update()
    {
        if(NetworkClient.isConnected && !this.isLocalPlayer)
            return;

        if(Input.GetButtonDown("Flashlight"))
            this.SendChangeVisibility(!this.currentVisibility);

        // In thirdperson, only move the flashlight when not free looking.
        if(!this.playerLook.isFreeLooking && !this.playerLook.shouldLerpFromFreeLook)
            this.thirdPersonCameraHolder.rotation = this.thirdPersonCamera.transform.rotation;
    }

    private void OnViewChanged(PlayerLook.View newView)
    {
        this.UpdateFlashlightParent(newView);

        this.flashlightAnimator.SetBool("Visibility", this.currentVisibility);

        if(this.flashlightAnimator.isActiveAndEnabled)
            this.flashlightAnimator.Play(((this.currentVisibility) ? "On" : "Off") + "Idle");

    }

    private void UpdateFlashlightParent(PlayerLook.View cameraView)
    {        
        this.flashlightTransform.SetParent((cameraView == PlayerLook.View.FirstPerson) ?
            this.firstPersonCameraHolder : this.thirdPersonCameraHolder);
        this.flashlightTransform.localRotation = Quaternion.Euler(Vector3.zero);
        this.flashlightTransform.localPosition = Vector3.zero;
    }

    private void SendChangeVisibility(bool visibility)
    {
        if (NetworkClient.isConnected)
            this.CmdSendChangeVisibility(visibility);
        else
            this.ChangeVisibility(visibility);
    }

    private void ChangeVisibility(bool visibility)
    {
        this.flashlightTransform.gameObject.SetActive(visibility);
        this.flashlightAnimator.SetBool("Visibility", visibility);
        this.currentVisibility = visibility;
    }

    [Command]
    private void CmdSendChangeVisibility(bool visibility)
    {
        this.RpcChangeVisibility(visibility);
    }

    [ClientRpc]
    private void RpcChangeVisibility(bool visibility)
    {
        this.ChangeVisibility(visibility);
    }
}
