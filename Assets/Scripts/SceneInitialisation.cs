using UnityEngine;
using Mirror;

public class SceneInitialisation : NetworkBehaviour
{
    private DayNightCycle dayNightCycle = null;

    public void InitialisSceneState()
    {
        if(!this.isServer)
            return;

        if(this.dayNightCycle == null)
            this.dayNightCycle = GameObject.FindObjectOfType<DayNightCycle>();

        this.RpcInitialiseDayNight();
        this.RpcInitialisePoweredUnits();
        this.RpcInitialiseDoors();
    }

    [ClientRpc]
    private void RpcInitialiseDoors()
    {
        Door[] doors = GameObject.FindObjectsOfType<Door>();
        foreach(Door door in doors)
            door.RpcInitialiseOpenState(door.IsOpen);
    }

    [ClientRpc]
    private void RpcInitialisePoweredUnits()
    {
        PoweredUnit[] poweredUnits = GameObject.FindObjectsOfType<PoweredUnit>();
        foreach(PoweredUnit poweredUnit in poweredUnits)
            poweredUnit.RpcInitialiseCurrentPower(poweredUnit.CurrentPower);
    }

    [ClientRpc]
    private void RpcInitialiseDayNight()
    {
        // Due to latency the Day Night cycle has a slight variance
        // either method has this isse, I like the initialTime one rather than elapsed..
        if(this.dayNightCycle != null)
        {
            //this.dayNightCycle.SetInitialTime(this.dayNightCycle.GenerateInitialTime());
            this.dayNightCycle.SetInitialTimeElapsed(Time.time);
        }
    }
}
