using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Airlock : MonoBehaviour
{
    [SerializeField] private Door innerDoor = null, outerDoor = null;
    [SerializeField] private ButtonInteraction insideButton = null, outsideButton = null;
    [SerializeField] private float secondsToOpen = 3.0f;
    [SerializeField] private string playerTag = "Player";
    
    private List<GameObject> enteredPlayers = new List<GameObject>();
    private List<GameObject> outsidePlayers = new List<GameObject>();
    private bool isInUse = false;
    
    private void Start()
    {
        this.insideButton.AddButtonPressedCallback((GameObject interactor) => this.OnEnterBase());
        this.outsideButton.AddButtonPressedCallback((GameObject interactor) => this.OnExitAirLock());
    }

    public bool IsPlayerInside(GameObject player)
    {
        // So that players heal when the airlock is initiated.
        if(this.enteredPlayers.Contains(player) && this.isInUse
            && !this.innerDoor.IsOpen && !this.outerDoor.IsOpen)
            return true;

        return !this.outsidePlayers.Contains(player);
    }

    private void OnEnterBase()
    {
        foreach (GameObject player in this.enteredPlayers)
            this.outsidePlayers.Remove(player);

        this.StartCoroutine(this.InitiateAirlockCoroutine(this.innerDoor));
    }

    private void OnExitAirLock()
    {
        foreach (GameObject player in this.enteredPlayers)
            this.outsidePlayers.Add(player);

        this.StartCoroutine(this.InitiateAirlockCoroutine(this.outerDoor));
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == this.playerTag)
            this.enteredPlayers.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == this.playerTag)
            this.enteredPlayers.Remove(other.gameObject);
    }

    private IEnumerator InitiateAirlockCoroutine(Door doorToOpen)
    {
        if(this.isInUse)
            yield break;

        this.isInUse = true;

        // Close and lock both doors.
        this.innerDoor.SetOpenState(false);
        this.outerDoor.SetOpenState(false);
        this.innerDoor.SetLockState(true);
        this.outerDoor.SetLockState(true);
        this.innerDoor.SetIsBusy(true);
        this.outerDoor.SetIsBusy(true);

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
        this.innerDoor.SetIsBusy(false);
        this.outerDoor.SetIsBusy(false);

        this.isInUse = false;
    }
}
