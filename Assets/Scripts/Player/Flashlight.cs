using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(PlayerLook))]
public class Flashlight : NetworkBehaviour
{
    [SerializeField]
    private GameObject flashLightPrefab = null;

    [SerializeField]
    private Transform firstPersonCameraHolder = null, thirdPersonCameraHolder = null;

    [SerializeField]
    private Camera thirdPersonCamera = null;

    [SerializeField]
    private bool currentVisibility = false;

    private Animator flashlight = null;
    private PlayerLook _PlayerLook = null;

    void Start()
    {
        this._PlayerLook = this.GetComponent<PlayerLook>();
        this._PlayerLook.AddChangeViewCallback(this.OnViewChanged);

        this.flashlight = GameObject.Instantiate(this.flashLightPrefab)
            .GetComponent<Animator>();

        Transform newParent = (this._PlayerLook.currentCameraView == PlayerLook.View.FirstPerson)
            ? this.firstPersonCameraHolder : this.thirdPersonCameraHolder;
        this.flashlight.gameObject.transform.SetParent(newParent);
        this.flashlight.gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
        this.flashlight.gameObject.transform.localPosition = Vector3.zero;

        if (NetworkClient.isConnected && this.isLocalPlayer)
            this.SendChangeVisibility(this.currentVisibility);
    }

    void Update()
    {
        if (NetworkClient.isConnected && !this.isLocalPlayer)
            return;

        bool flashLightInput = Input.GetButtonDown("Flashlight");
        if (flashLightInput)
            this.SendChangeVisibility(!this.currentVisibility);

        if(!this._PlayerLook.isFreeLooking && !this._PlayerLook.shouldLerpFromFreeLook)
            this.thirdPersonCameraHolder.rotation = this.thirdPersonCamera.transform.rotation;
    }

    private void OnViewChanged(PlayerLook.View newView)
    {
        Transform newParent = (newView == PlayerLook.View.FirstPerson)
            ? this.firstPersonCameraHolder : this.thirdPersonCameraHolder;
        this.flashlight.gameObject.transform.SetParent(newParent);
        this.flashlight.gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
        this.flashlight.gameObject.transform.localPosition = Vector3.zero;

        this.flashlight.SetBool("Visibility", this.currentVisibility);

        if(this.flashlight.isActiveAndEnabled)
            this.flashlight.Play(((this.currentVisibility) ? "On" : "Off") + "Idle");

    }

    private void SendChangeVisibility(bool visibility)
    {
        if (NetworkClient.isConnected)
            this.CmdSendChangeVisibility(visibility);
        else
            this.ChangeVisibility(visibility);
    }

    [Command]
    private void CmdSendChangeVisibility(bool visibility)
    {
        this.RpcChangeVisibility(visibility);
    }

    private void ChangeVisibility(bool visibility)
    {
        this.flashlight.gameObject.SetActive(visibility);
        this.flashlight.SetBool("Visibility", visibility);

        this.currentVisibility = visibility;
    }

    [ClientRpc]
    private void RpcChangeVisibility(bool visibility)
    {
        this.ChangeVisibility(visibility);
    }
}
