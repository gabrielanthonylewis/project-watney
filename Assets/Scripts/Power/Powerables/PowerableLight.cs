using UnityEngine;

[RequireComponent(typeof(Light))]
public class PowerableLight : Powerable
{
    private new Light light;

    private void Start()
    {
        this.light = this.GetComponent<Light>();    
    }

    public override void TurnOn()
    {
        this.light.enabled = true;
    }

    public override void TurnOff()
    {
        this.light.enabled = false;
    }
}
