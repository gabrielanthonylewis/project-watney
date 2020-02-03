using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [SerializeField]
    private float maxHealth = 100;

    [SerializeField]
    private float currentHealth;

    [SerializeField]
    private float currentTemperature;

    [SerializeField]
    private float bodyTemperature = 37;

    [SerializeField]
    private TextMeshProUGUI temperatureText = null;

    [SerializeField]
    private TextMeshProUGUI oxygenText = null;

    [SerializeField]
    private Image healthBarFill = null;

    [SerializeField]
    private float maxOxygen = 100;

    [SerializeField]
    private float currentOxygen = 100;

    [SerializeField]
    private float oxygenGainMultiplier = 2.0f;

    [SerializeField]
    private float oxygenLossMultiplier = 0.01f;

    private float oxygenRateMultiplier;

    private bool isInside = false;
    private float targetOxygen = 100;

    private GlobalTemperature _GlobalTemperature = null;
    private Airlock baseAirlock;

    private bool isDead = false;

    private void Start()
    {
        this.oxygenRateMultiplier = this.oxygenGainMultiplier;
        this.currentOxygen = this.maxOxygen;
        this.ChangeHealth(this.maxHealth);
        this._GlobalTemperature = GameObject.FindObjectOfType<GlobalTemperature>();
        this.baseAirlock = GameObject.FindObjectOfType<Airlock>();
        this.currentTemperature = this.bodyTemperature;
    }

    private void Update()
    {
        if (this.isDead)
            return;
        
        float targetTemperature = this.currentTemperature;
        if(this.baseAirlock.IsPlayerInside())
        {
            // increase oxygen
            this.targetOxygen = this.maxOxygen;
            this.oxygenRateMultiplier = this.oxygenGainMultiplier;

            targetTemperature = this.bodyTemperature;
        }
        else
        {
            // decrease oxygen
            this.targetOxygen = 0.0f;
            this.oxygenRateMultiplier = this.oxygenLossMultiplier;

            targetTemperature = this._GlobalTemperature.GetTemperature();
        }

        if (this.currentOxygen <= 0.0f)
            this.ChangeHealth(this.currentHealth - Time.deltaTime);
        if(this.currentTemperature <= -20.0f)
            this.ChangeHealth(this.currentHealth - Time.deltaTime);

        this.currentOxygen = Mathf.Lerp(this.currentOxygen, targetOxygen, Time.deltaTime * this.oxygenRateMultiplier);
        this.currentTemperature = Mathf.Lerp(this.currentTemperature, targetTemperature, Time.deltaTime);

        if (this.oxygenText != null)
            this.oxygenText.SetText("{0} O2", this.currentOxygen); // \u2082 wont work with this font

        if (this.temperatureText != null)
            this.temperatureText.SetText("{0}\u00B0C", this.currentTemperature);
    }

    public void Respawn()
    {
        this.oxygenRateMultiplier = this.oxygenGainMultiplier;
        this.currentOxygen = this.maxOxygen;
        this.ChangeHealth(this.maxHealth);
        this.currentTemperature = this.bodyTemperature;

        this.targetOxygen = this.maxOxygen;

        this.isDead = false;
    }

    private void ChangeHealth(float newAmount)
    {
        this.currentHealth = newAmount;

        if (this.currentHealth <= 0.0f)
        {
            this.isDead = true;
            GameObject.FindObjectOfType<PlayerSpawnManager>().LocalPlayerDied();

            this.currentHealth = 0.0f;
        }
        if (this.currentHealth > this.maxHealth)
            this.currentHealth = this.maxHealth;

        this.healthBarFill.fillAmount = this.currentHealth / this.maxHealth;
    }
}
