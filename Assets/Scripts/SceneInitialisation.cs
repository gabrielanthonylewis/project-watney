using UnityEngine;
using Mirror;

// This class is for initialising the scene state on a newly connected client.
public class SceneInitialisation : NetworkBehaviour
{
    private DayNightCycle dayNightCycle = null;

    public void InitialiseSceneState(NetworkConnection target)
    {
        if(this.isServer)
        {
            this.InitialiseDayNight();
            this.InitialisePoweredUnits(target);
            this.InitialiseDoors(target);
        }
    }

    private void InitialiseDoors(NetworkConnection target)
    {
        Door[] doors = GameObject.FindObjectsOfType<Door>();
        foreach(Door door in doors)
            door.TargetInitialiseDoorState(target, door.IsOpen, door.IsUnlocked, door.IsBusy);
    }

    private void InitialisePoweredUnits(NetworkConnection target)
    {
        PoweredUnit[] poweredUnits = GameObject.FindObjectsOfType<PoweredUnit>();
        foreach(PoweredUnit poweredUnit in poweredUnits)
            poweredUnit.TargetInitialiseCurrentPower(target, poweredUnit.CurrentPower);
    }

    private void InitialiseDayNight()
    {
        if(this.dayNightCycle == null)
            this.dayNightCycle = GameObject.FindObjectOfType<DayNightCycle>();

        /* Initialise on every client so that they are in sync.
         * This has the negative effect of potential time skipping due to ping
         * but in practice this is very minor and elimites the need for ping
         * calculations. As this is a prototype, it's completely acceptable. */
        if(this.dayNightCycle != null)
            this.dayNightCycle.RpcSetTimeElapsed(Time.time);
    }
}
