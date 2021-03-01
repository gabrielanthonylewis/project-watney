using System.Collections.Generic;
using UnityEngine;

public class PowerSupplyUnit : PoweredUnit
{
    [SerializeField] private List<PoweredUnit> inputs = new List<PoweredUnit>();
    [SerializeField] private PowerGrid[] powerGrids;

    private void Update()
    {
        foreach(PoweredUnit unit in this.inputs)
            this.Charge(unit.TransferPowerUpdate(Time.deltaTime));

        // If unit has power then the power grid can be turned on.
        if(this.currentPower > 0.0f)
        {
            foreach(PowerGrid grid in this.powerGrids)
                grid.TurnOn();

            this.TransferPowerUpdate(Time.deltaTime);
        }
        else
        {
            foreach(PowerGrid grid in this.powerGrids)
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
