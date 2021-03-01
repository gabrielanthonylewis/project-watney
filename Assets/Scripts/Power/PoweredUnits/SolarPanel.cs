using UnityEngine;

public class SolarPanel : PoweredUnit
{
    [SerializeField] private DayNightCycle dayNightCycle = null;

    private void Start()
    {
        if(this.dayNightCycle == null)
            this.dayNightCycle = GameObject.FindObjectOfType<DayNightCycle>();
    }

    private void Update()
    {
        float currentExposure = this.dayNightCycle.GetExposure();
        if(currentExposure > 0)
            this.ChargeUpdate(Time.deltaTime * currentExposure);
    }
}
