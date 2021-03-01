using UnityEngine;
using Mirror;

public class SceneInitialisation : NetworkBehaviour
{
    private DayNightCycle _DayNightCycle = null;

    public void InitialisSceneState()
    {
        if (this._DayNightCycle == null)
            this._DayNightCycle = GameObject.FindObjectOfType<DayNightCycle>();

        if (NetworkClient.isConnected)
            this.CmdInitialiseSceneState();
    }

    public void CmdInitialiseSceneState()
    {
        this.CmdInitialiseDayNight();
        this.CmdInitialisePoweredUnits();
        this.CmdInitialiseDoors();
    }

    [Command]
    public void CmdInitialiseDoors()
    {
        Door[] doors = GameObject.FindObjectsOfType<Door>();
        foreach (Door door in doors)
            door.RpcInitialiseOpenState(door.IsOpen);
    }

    [Command]
    public void CmdInitialisePoweredUnits()
    {
        PoweredUnit[] poweredUnits = GameObject.FindObjectsOfType<PoweredUnit>();
        foreach (PoweredUnit poweredUnit in poweredUnits)
            poweredUnit.RpcInitialiseCurrentPower(poweredUnit.CurrentPower);
    }

    [Command]
    private void CmdInitialiseDayNight()
    {
        // todo: why do I need to pass the time here? can't it just use it in the function
        if (this._DayNightCycle != null)
            this._DayNightCycle.RpcSetInitialTime(this._DayNightCycle.GetInitialTime());
    }
}
