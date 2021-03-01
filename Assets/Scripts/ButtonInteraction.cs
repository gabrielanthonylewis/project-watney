using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ButtonInteraction : NetworkBehaviour
{
    public delegate void ButtonPressedDelegate();
    private ButtonPressedDelegate buttonPressedCallback;

    public void AddButtonPressedCallback(ButtonPressedDelegate myCallback)
    {
        this.buttonPressedCallback += myCallback;
    }

    public void RemoveButtonPressedCallback(ButtonPressedDelegate myCallback)
    {
        this.buttonPressedCallback -= myCallback;
    }

    
    public void Interact()
    {
        if (NetworkClient.isConnected)
            this.RpcInteract();
        else
            this.InvokeCallback();
    }

    [ClientRpc]
    private void RpcInteract()
    {
        this.InvokeCallback();
    }

    private void InvokeCallback()
    {
        this.buttonPressedCallback?.Invoke();
    }
}
