using System.Collections;
using UnityEngine;

public class Airlock : MonoBehaviour
{
    [SerializeField] private Door innerDoor = null, outerDoor = null;
    [SerializeField] private ButtonInteraction insideButton = null, outsideButton = null;
    [SerializeField] private float secondsToOpen = 3.0f;
    
    private bool isInUse = false;
    
    private void Start()
    {
        this.insideButton.AddButtonPressedCallback(() =>
            this.StartCoroutine(this.InitiateAirlockCoroutine(this.innerDoor)));
        this.outsideButton.AddButtonPressedCallback(() =>
            this.StartCoroutine(this.InitiateAirlockCoroutine(this.outerDoor)));
    }

    public bool IsPlayerInside()
    {
        if(this.isInUse && !this.innerDoor.IsOpen && !this.outerDoor.IsOpen)
            return true;

        return this.innerDoor.IsUnlocked;
    }

    public IEnumerator InitiateAirlockCoroutine(Door doorToOpen)
    {
        if(this.isInUse)
            yield break;

        this.isInUse = true;

        // Close and lock both doors.
        this.innerDoor.SetOpenState(false);
        this.outerDoor.SetOpenState(false);
        this.innerDoor.SetLockState(true);
        this.outerDoor.SetLockState(true);

        // Updates button text every second. e.g. 3, 2, 1
        float currSecondsToOpen = this.secondsToOpen;
        doorToOpen.SetDisplayText(currSecondsToOpen.ToString());
        while(currSecondsToOpen > 0)
        {
            yield return new WaitForSeconds(1.0f);
            currSecondsToOpen--;
            doorToOpen.SetDisplayText((currSecondsToOpen > 0) ? currSecondsToOpen.ToString() : "");
        }
        
        doorToOpen.SetLockState(false);
        doorToOpen.SetOpenState(true);

        this.isInUse = false;
    }
}
