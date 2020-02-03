using UnityEngine;
using Mirror;

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
    }

    [ClientRpc]
    private void RpcChangeOpenState(bool state)
    {
        this.ChangeOpenState(state);
    }

    [ClientRpc]
    public void RpcInitialiseOpenState(bool isOpen)
    {
        this.isOpen = isOpen;
        this._Animator.SetBool("isOpen", this.isOpen);
    }
}
