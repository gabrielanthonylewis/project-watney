using System.Collections.Generic;
using UnityEngine;

public class Battery : PoweredUnit
{
    [SerializeField]
    private List<PoweredUnit> inputs = new List<PoweredUnit>();

    private void Update()
    {
        // Transfer power from all inputs every update.
        foreach(PoweredUnit unit in this.inputs)
            this.Charge(unit.TransferPowerUpdate(Time.deltaTime));    
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
