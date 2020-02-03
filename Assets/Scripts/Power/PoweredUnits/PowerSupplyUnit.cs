using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PowerSupplyUnit : PoweredUnit
{
    [SerializeField]
    private bool acceptPower = true;

    [SerializeField]
    private List<PoweredUnit> inputs = new List<PoweredUnit>();

    [SerializeField]
    private PowerGrid[] powerGrids;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
            this.acceptPower = !this.acceptPower;

        if(this.acceptPower)
        {
            foreach (PoweredUnit unit in this.inputs)
                this.Charge(unit.TransferPowerUpdate(Time.deltaTime));
        }

        if(this.CurrentPower > 0.0f)
        {
            foreach(PowerGrid grid in this.powerGrids)
                grid.TurnOn();

            this.TransferPowerUpdate(Time.deltaTime);
        }
        else
        {
            foreach (PowerGrid grid in this.powerGrids)
                grid.TurnOff();
        }
    }

    public override void AddInput(PoweredUnit input)
    {
        base.AddInput(input);

        this.inputs.Add(input);
    }

    public override void RemoveInput(PoweredUnit input)
    {
        base.RemoveInput(input);

        this.inputs.Remove(input);
    }
}
