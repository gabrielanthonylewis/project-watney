using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Mirror;
using UnityEngine.Rendering.HighDefinition;

public class DayNightCycle : NetworkBehaviour
{
    private Material initialSkyBoxMat;
    private Material newSkyBoxMat;

    private readonly float dayLengthMinutes = 1477;

    private float secondFraction;
    private float exposureNormalised;

    [SerializeField]
    private float secondMultiplier = 1.0f;

    [SerializeField]
    [Range(0, 1)]
    private float timeOfDayNormalised = 0.5f;

    [SerializeField]
    [Range(0, 1)]
    private float normalisedSunrise = 0.2f;

    [SerializeField]
    [Range(0,1)]
    private float normaliseSunset = 0.8f;

    [SerializeField]
    private Light directionalLight = null;

    [SerializeField]
    private float debugExposure;

    [SerializeField]
    private float debugRotX;

    [SerializeField]
    private Volume _skyboxVolume;

    private void Start()
    {
        this.initialSkyBoxMat = RenderSettings.skybox;
        //this.newSkyBoxMat = new Material(this.initialSkyBoxMat);
       // RenderSettings.skybox = this.newSkyBoxMat;

        float secondsInDay = this.dayLengthMinutes * 60.0f;
        this.secondFraction = (1.0f / secondsInDay) * this.secondMultiplier;
    }

    [ClientRpc]
    public void RpcUpdateCurrentTime(float time)
    {
        this.timeOfDayNormalised = time;
    }

    // Update is called once per frame
    void Update()
    {
        this.timeOfDayNormalised = Mathf.Clamp(this.timeOfDayNormalised + this.secondFraction * Time.deltaTime, 0, 1);

        // change exposure
        if (this.timeOfDayNormalised == 1.0f)
            this.timeOfDayNormalised = 0.0f;


        this.UpdateExposure();
        this.UpdateLightRotation(this.exposureNormalised);
        this.UpdateLightIntensity(this.exposureNormalised);
    }

    public float GetNormalisedTime()
    {
        return this.timeOfDayNormalised;
    }

    public float GetExposure()
    {
        return this.exposureNormalised;
    }

    private void UpdateExposure()
    {
        float exposureNormalised = 0.0f;

        if (this.timeOfDayNormalised >= this.normalisedSunrise && this.timeOfDayNormalised <= 0.5f)
            exposureNormalised = (this.timeOfDayNormalised - this.normalisedSunrise) / (0.5f - this.normalisedSunrise);

        if (this.timeOfDayNormalised > 0.5f && this.timeOfDayNormalised <= this.normaliseSunset)
            exposureNormalised = (1.0f - (this.timeOfDayNormalised - 0.5f) / (this.normaliseSunset - 0.5f));

        debugExposure = exposureNormalised;
        this.exposureNormalised = exposureNormalised;

        // if(this.newSkyBoxMat != null)
        //    this.newSkyBoxMat.SetFloat("_Exposure", this.exposureNormalised);
        HDRISky tmp;
        if(this._skyboxVolume.profile.TryGet<HDRISky>(out tmp))
        {
            FloatParameter newval = new FloatParameter(exposureNormalised);
            tmp.desiredLuxValue.SetValue(newval);
        }
    }

    private void UpdateLightRotation(float exposureNormalised)
    {
        float newX = 0.0f;
        if (this.timeOfDayNormalised >= this.normalisedSunrise && this.timeOfDayNormalised <= 0.5f)
            newX = -12.5f + exposureNormalised * (90.0f + 12.5f);

        if (this.timeOfDayNormalised > 0.5f && this.timeOfDayNormalised <= this.normaliseSunset)
            newX = 90.0f + (1.0f - exposureNormalised) * (173.0f - 90.0f);

        debugRotX = newX;
        this.directionalLight.transform.localRotation = Quaternion.Euler(newX, 0.0f, 0.0f);
    }

    private void UpdateLightIntensity(float exposureNormalised)
    {
        float maxIntensity = 10.0f;

        this.directionalLight.intensity = exposureNormalised * maxIntensity;
    }

    void OnApplicationQuit()
    {
        RenderSettings.skybox = this.initialSkyBoxMat;
    }
}
