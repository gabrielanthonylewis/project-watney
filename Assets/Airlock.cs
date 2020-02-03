using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Airlock : MonoBehaviour
{
    [SerializeField]
    private Door innerDoor = null, outerDoor = null;

    [SerializeField]
    private ButtonInteraction insideButton = null, outsidebutton = null;

    [SerializeField]
    private bool isInUse = false;
    
    void Start()
    {
        this.insideButton.AddButtonPressedCallback(this.OpenOutside);
        this.outsidebutton.AddButtonPressedCallback(this.OpenInside);

        // Close both doors if both are open.
        if(!innerDoor.IsOpen && !outerDoor.IsOpen)
        {
            outerDoor.SetLockState(true);
        }
        if (innerDoor.IsOpen && outerDoor.IsOpen)
        {
            innerDoor.SetOpenState(false);
            outerDoor.SetOpenState(false);
            innerDoor.SetLockState(false);
            outerDoor.SetLockState(true);
        }
        // Close opposite door and lock
        else
        {
            if (innerDoor.IsOpen)
            {
                outerDoor.SetOpenState(false);
                outerDoor.SetLockState(true);
            }

            if (outerDoor.IsOpen)
            {
                innerDoor.SetOpenState(false);
                innerDoor.SetLockState(true);
            }
        }
    }

    public bool IsPlayerInside()
    {
        return this.innerDoor.IsUnlocked;
    }

    private void OpenOutside()
    {
        StartCoroutine(this.InitiateAirlock(this.outerDoor));
    }

    private void OpenInside()
    {
        StartCoroutine(this.InitiateAirlock(this.innerDoor));
    }

    public IEnumerator InitiateAirlock(Door doorToOpen)
    {
        if (this.isInUse)
            yield break;

        this.isInUse = true;

        this.innerDoor.SetOpenState(false);
        this.outerDoor.SetOpenState(false);
        this.innerDoor.SetLockState(true);
        this.outerDoor.SetLockState(true);

        yield return new WaitForSeconds(3.0f);

        doorToOpen.SetLockState(false);
        doorToOpen.SetOpenState(true);

        this.isInUse = false;

    }

}
