using UnityEngine;
using UnityEngine.Events;

public class ButtonInteraction : MonoBehaviour
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
        if(this.buttonPressedCallback != null)
            this.buttonPressedCallback.Invoke();
    }
}
