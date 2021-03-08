using UnityEngine;
using Mirror;
using TMPro;

public class Door : NetworkBehaviour
{
    [SerializeField]
    private bool isUnlocked = true;
    public bool IsUnlocked { get { return this.isUnlocked; } private set { this.isUnlocked = value; } }

    [SerializeField]
    private bool isOpen = false;
    public bool IsOpen { get { return this.isOpen; } private set { this.isOpen = value; } }

    [SerializeField]
    private Animator _Animator = null;

    [SerializeField]
    private Material lockedMaterial, closedMaterial, openMaterial;

    [SerializeField]
    private MeshRenderer displayRenderer = null;

    [SerializeField] private TextMeshProUGUI displayText = null;


    // TODO: Why isn't updatedisplay working??
    private void Start()
    {
        this.UpdateDisplay();
    }

    public void Interact()
    {
        if (isUnlocked == false)
            return;

        this.SetOpenState(!this.IsOpen);
    }

    public void SetLockState(bool shouldLock)
    {
        if (NetworkClient.isConnected)
            this.RpcChangeLockState(!shouldLock);
        else
            this.ChangeLockState(!shouldLock);
    }

    private void ChangeLockState(bool state)
    {
        this.isUnlocked = state;
        this.UpdateDisplay();
    }

    [ClientRpc]
    private void RpcChangeLockState(bool state)
    {
        this.ChangeLockState(state);
    }

    public void SetOpenState(bool state)
    {
        if (NetworkClient.isConnected)
            this.RpcChangeOpenState(state);
        else
            this.ChangeOpenState(state);
    }

    private void ChangeOpenState(bool state)
    {
        this.isOpen = state;
        this._Animator.SetBool("isOpen", state);
        this.UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        Material[] displayMaterials = this.displayRenderer.materials;

        displayMaterials[1] = (this.isUnlocked) ? this.openMaterial : this.lockedMaterial;
        /*if(this.isUnlocked == false)
            displayMaterials[1] = this.lockedMaterial;
        else
            displayMaterials[1] = (this.isOpen) ? this.openMaterial : this.closedMaterial;
*/

        this.displayRenderer.materials = displayMaterials;
    }

    public void SetDisplayText(string text)
    {
        this.displayText.text = text;
    }

    [ClientRpc]
    private void RpcChangeOpenState(bool state)
    {
        this.ChangeOpenState(state);
    }

    //[ClientRpc]
    public void RpcInitialiseOpenState(bool isOpen)
    {
        this.isOpen = isOpen;
        this._Animator.SetBool("isOpen", this.isOpen);
    }
}
