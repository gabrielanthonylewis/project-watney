using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GlobalTemperature : MonoBehaviour
{
    private DayNightCycle _DayNightCycle = null;

    [SerializeField]
    private TextMeshProUGUI temperatureText = null;

    private float currentTemperature;

    private void Start()
    {
        this._DayNightCycle = GameObject.FindObjectOfType<DayNightCycle>();
    }

    public float GetTemperature()
    {
        return this.currentTemperature;
    }

    // Update is called once per frame
    void Update()
    {
        if(this._DayNightCycle == null)
            this._DayNightCycle = GameObject.FindObjectOfType<DayNightCycle>();
        
        if (this._DayNightCycle == null)
            return;

        float currentExposure = this._DayNightCycle.GetExposure();

        float minTemp = -73;
        float maxTemp = 20;

        float totalDiff = Mathf.Abs(minTemp) + Mathf.Abs(maxTemp); // 93
        this.currentTemperature = ((totalDiff) / (1.0f / (currentExposure))) - Mathf.Abs(minTemp);

        if(temperatureText != null)
            this.temperatureText.SetText("{0}\u00B0C", this.currentTemperature);
    }
}
