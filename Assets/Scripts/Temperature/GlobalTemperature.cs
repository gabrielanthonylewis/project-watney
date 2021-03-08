using UnityEngine;
using TMPro;

public class GlobalTemperature : MonoBehaviour
{
    [SerializeField] private DayNightCycle dayNightCycle = null;
    [SerializeField] private TextMeshProUGUI temperatureText = null;
    [SerializeField] private float minTemperature = -73.0f;
    [SerializeField] private float maxTemperature = 20.0f;

    private float currentTemperature;

    public float GetTemperature()
    {
        return this.currentTemperature;
    }

    public float GetMinTemperature()
    {
        return this.minTemperature;
    }

    private void Update()
    {
        this.currentTemperature = Mathf.Lerp(minTemperature, maxTemperature, this.dayNightCycle.GetExposure());

        // Make only 1 character, e.g. 1 degrees celsius
        if(this.temperatureText != null)
            this.temperatureText.SetText("Outside: " + this.currentTemperature.ToString("F0") + "C");
    }
}
