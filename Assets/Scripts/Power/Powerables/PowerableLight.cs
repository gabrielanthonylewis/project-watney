using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerableLight : Powerable
{
    private Light _light;

    // Start is called before the first frame update
    void Start()
    {
        this._light = this.GetComponent<Light>();    
    }

    public override void TurnOn()
    {
        this._light.enabled = true;
    }

    public override void TurnOff()
    {
        this._light.enabled = false;
    }
}
