using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battery : PoweredUnit
{
    [SerializeField]
    private bool acceptPower = false;

    [SerializeField]
    private List<PoweredUnit> outputs = new List<PoweredUnit>();

    [SerializeField]
    private List<PoweredUnit> inputs = new List<PoweredUnit>();

    [SerializeField]
    private IOSocket inputSocket = null;

    [SerializeField]
    private IOSocket outputSocket = null;

    // Update is called once per frame
    void Update()
    {
        if(this.acceptPower)
        {
            foreach(PoweredUnit unit in this.inputs)
            {
              //  unit.ShouldDischarge();
                this.Charge(unit.TransferPowerUpdate(Time.deltaTime));
     
            }
        }
        else
        {
            foreach (PoweredUnit unit in this.inputs)
            {
              //  unit.CanCharge();
            }
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

    //todo: do i need output??
    public override void AddOutput(PoweredUnit output)
    {
        base.AddOutput(output);

        this.outputs.Add(output);
    }

    public override void RemoveOutput(PoweredUnit output)
    {
        base.RemoveOutput(output);

        this.outputs.Remove(output);
    }
}
