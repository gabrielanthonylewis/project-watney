using UnityEngine;
using UnityEngine.Events;

public class ButtonInteraction : MonoBehaviour
{
    private UnityEvent<GameObject> buttonPressedCallback = new UnityEvent<GameObject>();

    public void AddButtonPressedCallback(UnityAction<GameObject> callback)
    {
        this.buttonPressedCallback.AddListener(callback);
    }

    public void RemoveButtonPressedCallback(UnityAction<GameObject> callback)
    {
        this.buttonPressedCallback.RemoveListener(callback);
    }

    public void Interact(GameObject interactor)
    {
        if(this.buttonPressedCallback != null)
            this.buttonPressedCallback.Invoke(interactor);
    }
}
