using UnityEngine;

public class CursorLock : MonoBehaviour
{
    [SerializeField] CursorLockMode cursorLockMode = CursorLockMode.Locked;

    private void Start()
    {
        Cursor.lockState = this.cursorLockMode;
    }
}
