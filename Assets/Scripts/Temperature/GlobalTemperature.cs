using UnityEngine;
using TMPro;

public class GlobalTemperature : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI temperatureText = null;
    [SerializeField] private float minTemperature = -73.0f;
    [SerializeField] private float maxTemperature = 20.0f;

    private DayNightCycle dayNightCycle = null;
    private float currentTemperature;

    private void Awake()
    {
        this.dayNightCycle = GameObject.FindObjectOfType<DayNightCycle>();
    }

    public float GetTemperature()
    {
        return this.currentTemperature;
    }

    private void Update()
    {
        // TODO: This looks weird, make it better
        float totalDiff = Mathf.Abs(minTemperature) + Mathf.Abs(maxTemperature);
        this.currentTemperature = ((totalDiff) / (1.0f / (this.dayNightCycle.GetExposure()))) - Mathf.Abs(minTemperature);

        if(this.temperatureText != null)
            this.temperatureText.SetText("{0}\u00B0C", this.currentTemperature);
    }
}
