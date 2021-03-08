using UnityEngine;
using UnityEngine.Rendering;
using Mirror;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(Light))]
public class DayNightCycle : NetworkBehaviour
{
    [SerializeField][Range(0, 1)] private float durationMultiplier = 0.1f;
    [SerializeField][Range(0, 1)] private float timeOfDay = 0.5f;
    [SerializeField][Range(0, 1)] private float sunrise = 0.2f;
    [SerializeField][Range(0, 1)] private float sunset = 0.8f;
    [SerializeField] private float directionalLightIntensity = 20.0f;
    [SerializeField] private float debugExposure;
    [SerializeField] private Volume skyboxVolume;

    // A martian day is approx 24 hours, 39 minutes and 35 seconds
    private readonly float MARS_DURATION = 88775;

    private float secondsDelta;
    private float exposure;
    private Material initialSkyBoxMat;
    private Light directionalLight = null;
    private float initialT;
    private float dayLength;
    private float initialTimeElapsed = 0.0f;
    private float timeElapsed = 0.0f;

    private void Awake()
    {
        this.directionalLight = this.GetComponent<Light>();

        /* Skybox settings are saved in the editor for some reason,
         * so we need to save and then restore them when exiting. */
        this.initialSkyBoxMat = RenderSettings.skybox;

        this.dayLength = (this.sunset - this.sunrise); 

        this.secondsDelta = this.MARS_DURATION * this.durationMultiplier;
        this.initialT = this.timeOfDay * this.secondsDelta;
    }

    private void Update()
    {
        // Repeat the day night cycle [0, 1]
        this.timeElapsed += Time.deltaTime;
        this.timeOfDay = Mathf.Repeat((initialT + (this.timeElapsed)) / secondsDelta, 1.0f);

        this.UpdateExposure();
        this.UpdateLightRotation();
        this.UpdateLightIntensity();
    }

    private void UpdateExposure()
    {
        bool isDay = (this.timeOfDay >= this.sunrise && this.timeOfDay <= this.sunset);
        const float MAX_EXPOSURE = 1.0f;
        const float MIN_EXPOSURE = 0.0f;

        if(isDay)
        {
            // Where we are in this day.
            float dayNormalised = (this.timeOfDay - this.sunrise) / this.dayLength; 

            /* Like a triangle, the heighest point is in the middle of the day.
             * This isn't realistic but works well enough for the prototype. */
            if(dayNormalised <= 0.5f)
                this.exposure = Mathf.Lerp(MIN_EXPOSURE, MAX_EXPOSURE, dayNormalised * 2.0f);
            else
                this.exposure = Mathf.Lerp(MAX_EXPOSURE, MIN_EXPOSURE, (dayNormalised - 0.5f) * 2.0f);
        }
        else
        {
            this.exposure = MIN_EXPOSURE;
        }

        debugExposure = this.exposure;

        // Update skybox exposure.
        HDRISky hdriSky;
        if(this.skyboxVolume.profile.TryGet<HDRISky>(out hdriSky))
        {
            hdriSky.desiredLuxValue.SetValue(new FloatParameter(this.exposure));
            DynamicGI.UpdateEnvironment();
        }
    }

    private void UpdateLightRotation()
    {
        bool isDay = (this.timeOfDay >= this.sunrise && this.timeOfDay <= this.sunset);

        Vector3 angles = new Vector3(this.directionalLight.transform.rotation.x,
            this.directionalLight.transform.rotation.y, this.directionalLight.transform.rotation.z);

        if(isDay)
        {
            float dayNormalised = (this.timeOfDay - this.sunrise) / this.dayLength;
            angles.x = Mathf.Lerp(0.0f, 180.0f, dayNormalised);
        }   
        else
        {
            // Night may be [0.8, 1.0] + [0.0, 0.2] so will have to do some ugly maths.
            float nightNormalised = 0.0f;
            if(this.timeOfDay >= this.sunset)
                nightNormalised = this.timeOfDay - this.sunset;
            else if(this.timeOfDay <= this.sunrise)
                nightNormalised = (1.0f - this.sunset) + this.timeOfDay;
            nightNormalised /= ((1.0f - this.sunset) + this.sunrise);

            angles.x = Mathf.Lerp(180.0f, 360.0f, nightNormalised);
        }

        this.directionalLight.transform.rotation = Quaternion.Euler(angles);
    }

    private void UpdateLightIntensity()
    {
        this.directionalLight.intensity = this.exposure * this.directionalLightIntensity;
    }

    public void SetInitialTimeElapsed(float time)
    {
        this.initialTimeElapsed = time;
        this.timeElapsed = this.initialTimeElapsed;
    }

    public void SetInitialTime(float initialTime)
    {
        this.initialT = initialTime;
    }

    public float GenerateInitialTime()
    {
        return this.timeOfDay * this.secondsDelta;
    }

    public float GetExposure()
    {
        return this.exposure;
    }

    private void OnDisable()
    {
        RenderSettings.skybox = this.initialSkyBoxMat;
        DynamicGI.UpdateEnvironment();
    }
}
