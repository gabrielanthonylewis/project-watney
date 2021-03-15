using UnityEngine;
using Mirror;
using TMPro;

public class Door : NetworkBehaviour
{
    [SerializeField] private bool isUnlocked = true;
    public bool IsUnlocked { get { return this.isUnlocked; } private set { this.isUnlocked = value; } }

    [SerializeField] private bool isOpen = false;
    public bool IsOpen { get { return this.isOpen; } private set { this.isOpen = value; } }

    [SerializeField] private Animator animator = null;
    [SerializeField] private Material canOpenMaterial, cantOpenMaterial, isBusyMaterial;
    [SerializeField] private MeshRenderer displayRenderer = null;
    [SerializeField] private TextMeshProUGUI displayText = null;

    private bool isBusy = false;
    public bool IsBusy { get { return this.isBusy; } private set { this.isBusy = value; } }

    private void Start()
    {
        this.UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        Material[] displayMaterials = this.displayRenderer.materials;
        if(this.isBusy)
            displayMaterials[1] = this.isBusyMaterial;
        else
            displayMaterials[1] = (this.isUnlocked) ? this.cantOpenMaterial : this.canOpenMaterial;
        this.displayRenderer.materials = displayMaterials;
    }

    public void SetDisplayText(string text)
    {
        this.displayText.text = text;
    }

    public void Interact()
    {
        if(!this.isUnlocked)
            return;

        this.SetOpenState(!this.IsOpen);
    }

    #region isOpen
    public void SetOpenState(bool state)
    {
        if(NetworkClient.isConnected)
            this.CmdChangeOpenState(state);
        else
            this.ChangeOpenState(state);
    }

    [Command]
    private void CmdChangeOpenState(bool state)
    {
        this.RpcChangeOpenState(state);
    }

    [ClientRpc]
    private void RpcChangeOpenState(bool state)
    {
        this.ChangeOpenState(state);
    }

    private void ChangeOpenState(bool state)
    {
        this.isOpen = state;
        this.animator.SetBool("isOpen", state);
        this.UpdateDisplay();
    }
    #endregion

    #region IsUnlocked
    public void SetLockState(bool shouldLock)
    {
        if (NetworkClient.isConnected)
            this.CmdChangeLockState(!shouldLock);
        else
            this.ChangeLockState(!shouldLock);
    }

    [Command]
    private void CmdChangeLockState(bool state)
    {
        this.RpcChangeLockState(state);
    }

    [ClientRpc]
    private void RpcChangeLockState(bool state)
    {
        this.ChangeLockState(state);
    }

    private void ChangeLockState(bool state)
    {
        this.isUnlocked = state;
        this.UpdateDisplay();
    }
    #endregion

    #region IsBusy
    public void SetIsBusy(bool isBusy)
    {
        if (NetworkClient.isConnected)
            this.CmdChangeIsBusy(isBusy);
        else
            this.ChangeIsBusy(isBusy);
    }

    [Command]
    private void CmdChangeIsBusy(bool isBusy)
    {
        this.RpcChangeIsBusy(isBusy);
    }

    [ClientRpc]
    private void RpcChangeIsBusy(bool isBusy)
    {
        this.ChangeIsBusy(isBusy);
    }

    private void ChangeIsBusy(bool isBusy)
    {
        this.isBusy = isBusy;
        this.UpdateDisplay();
    }
    #endregion

    [TargetRpc]
    public void TargetInitialiseDoorState(NetworkConnection target, bool isOpen, bool isUnlocked, bool isBusy)
    {
        this.ChangeOpenState(isOpen);
        this.ChangeLockState(isUnlocked);
        this.ChangeIsBusy(isBusy);
    }
}
