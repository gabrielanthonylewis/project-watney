using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolarPanel : PoweredUnit
{
    [SerializeField]
    private DayNightCycle _DayNightCycle = null;

    private void Start()
    {
        if(_DayNightCycle == null)
            this._DayNightCycle = GameObject.FindObjectOfType<DayNightCycle>();
    }

    // Update is called once per frame
    void Update()
    {
        float currentExposure = this._DayNightCycle.GetExposure();

        if (currentExposure <= 0)
            return;

        this.ChargeUpdate(Time.deltaTime * currentExposure);
    }
}
