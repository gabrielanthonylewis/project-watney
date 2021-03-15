using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100.0f;
    [SerializeField] private float maxOxygen = 100.0f;
    [SerializeField] private float oxygenLossDuration = 120.0f;
    [SerializeField] private float oxygenGainDuration = 10.0f;
    [SerializeField] private float oxygenDPS = 5.0f;
    [SerializeField] private float temperatureDPS = 5.0f;
    [SerializeField] private float temperatureLossMultiplier = 0.5f;
    [SerializeField] private float temperatureGainMultiplier = 10.0f;
    [SerializeField] private float temperatureLowerBound = -20.0f;
    [SerializeField] private Image healthBarFill = null;
    [SerializeField] private TextMeshProUGUI temperatureText = null;
    [SerializeField] private TextMeshProUGUI oxygenText = null;

    private const float BODY_TEMPERATURE = 37.0f;

    private float currentHealth;
    private float currentTemperature;
    private float currentOxygen;
    private float fromOxygen;
    private float toOxygen;
    private float oxygenLerpT;
    private float oxygenLerpDuration;
    private bool isInside;
    private bool isDead;
    private GlobalTemperature globalTemperature = null;
    private Airlock baseAirlock;

    private void Start()
    {
        this.globalTemperature = GameObject.FindObjectOfType<GlobalTemperature>();
        this.baseAirlock = GameObject.FindObjectOfType<Airlock>();

        this.SetIsInside(true);
        this.SetMaxStats();
    }

    private void SetMaxStats()
    {
        this.currentOxygen = this.maxOxygen;
        this.fromOxygen = this.currentOxygen;
        this.toOxygen = this.currentOxygen;
        this.oxygenLerpT = 1.0f;
        this.oxygenLerpDuration = 1.0f;

        this.SetHealth(this.maxHealth);
        this.currentTemperature = PlayerStats.BODY_TEMPERATURE;
    }

    public void Respawn()
    {
        this.SetMaxStats();
    }

    private void Update()
    {
        if(this.isDead)
            return;

        bool isInside = this.baseAirlock.IsPlayerInside();
        if(isInside != this.isInside)
            this.SetIsInside(isInside);

        // Oxygen
        this.oxygenLerpT += Time.deltaTime / this.oxygenLerpDuration;
        this.SetOxygen(Mathf.Lerp(this.fromOxygen, this.toOxygen, this.oxygenLerpT));

        if(this.currentOxygen <= 0.0f)
            this.SetHealth(this.currentHealth - (Time.deltaTime * this.oxygenDPS));

    
        // Temperature
        float targetTemperature = (this.isInside) ? PlayerStats.BODY_TEMPERATURE : this.globalTemperature.GetTemperature();
        if(targetTemperature > this.currentTemperature)
            this.SetTemperature(Mathf.Min(this.currentTemperature + (Time.deltaTime * this.temperatureGainMultiplier), targetTemperature));
        else
            this.SetTemperature(Mathf.Max(this.currentTemperature - (Time.deltaTime * this.temperatureLossMultiplier), targetTemperature));

        if(this.currentTemperature <= this.temperatureLowerBound)
            this.SetHealth(this.currentHealth - (Time.deltaTime * this.temperatureDPS));
    }

    private void SetIsInside(bool isInside)
    {
        this.isInside = isInside;

        this.fromOxygen = this.currentOxygen;
        this.toOxygen = (this.isInside) ? maxHealth : 0.0f;
        this.oxygenLerpT = 0.0f;
        this.oxygenLerpDuration = (this.isInside) ? this.oxygenGainDuration : this.oxygenLossDuration;
    }

    private void SetOxygen(float oxygen)
    {
        this.currentOxygen = Mathf.Clamp(oxygen, 0.0f, this.maxOxygen);

        if(this.oxygenText != null)
            this.oxygenText.SetText(this.currentOxygen.ToString("F0") + " O2");
    }

    private void SetHealth(float health)
    {
        this.currentHealth = Mathf.Clamp(health, 0.0f, this.maxHealth);

        this.isDead = (this.currentHealth <= 0.0f);
        if(this.isDead)
            GameObject.FindObjectOfType<PlayerSpawnManager>().OnLocalPlayerDied();

        if(this.healthBarFill != null)
            this.healthBarFill.fillAmount = (this.currentHealth / this.maxHealth);
    }

    private void SetTemperature(float temperature)
    {
        this.currentTemperature = temperature;

        if(this.temperatureText != null)
            this.temperatureText.SetText(this.currentTemperature.ToString("F0") + "C");
    }
}
