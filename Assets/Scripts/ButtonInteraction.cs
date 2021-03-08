using Mirror;
using UnityEngine.Events;

public class ButtonInteraction : NetworkBehaviour
{
    private UnityEvent buttonPressedCallback = new UnityEvent();

    public void AddButtonPressedCallback(UnityAction callback)
    {
        this.buttonPressedCallback.AddListener(callback);
    }

    public void RemoveButtonPressedCallback(UnityAction callback)
    {
        this.buttonPressedCallback.RemoveListener(callback);
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
        if(this.buttonPressedCallback != null)
            this.buttonPressedCallback.Invoke();
    }
}
