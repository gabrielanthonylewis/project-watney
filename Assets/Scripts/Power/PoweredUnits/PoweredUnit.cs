using UnityEngine;
using TMPro;
using Mirror;

public class PoweredUnit: NetworkBehaviour
{
    [SerializeField] protected float currentPower; // Watts that are currently stored.
    public float CurrentPower { get { return this.currentPower; } private set { this.currentPower = value; } }

    [SerializeField] private float maximumCapacity; // Max Watts that can be stored.
    public float MaximumCapacity { get { return this.maximumCapacity; } private set { this.maximumCapacity = value; } }

    [SerializeField] private float chargeRate; // Rate at which Watts are gained per second.
    public float ChargeRate { get { return this.chargeRate; } private set { this.chargeRate = value; } }

    [SerializeField] private float transferRate; // Rate at which Watts are transfered from the power source to a battery.
    public float TransferRate { get { return this.transferRate; } private set { this.transferRate = value; } }

    [SerializeField] private TextMeshProUGUI percentageText = null;
    [SerializeField] private bool canCharge = true;

    public virtual void AddInput(PoweredUnit input) { }
    public virtual void RemoveInput(PoweredUnit input) { }
    public virtual void AddOutput(PoweredUnit input) { }
    public virtual void RemoveOutput(PoweredUnit input) { }

   // [ClientRpc]
    public void RpcInitialiseCurrentPower(float power)
    {
        this.currentPower = power;
        this.UpdateText();
    }

    /**
     * Increases the current power using the charge rate.
     * Returns the amount charged.
     * */
    protected float ChargeUpdate(float dt)
    {
        float additionalPower = this.chargeRate * dt;
        return this.Charge(additionalPower);
    }

    protected float Charge(float additionalPower)
    {
        if (!this.canCharge)
            return 0;

        // Already charged.
        if (this.currentPower == this.maximumCapacity)
            return 0;

        // Add power.
        this.currentPower += additionalPower;

        // Dont overcharge.
        if (this.currentPower > this.maximumCapacity)
        {
            additionalPower -= (this.currentPower - this.maximumCapacity);
            this.currentPower = this.maximumCapacity;
        }

        this.UpdateText();

        return additionalPower;
    }

    public float TransferPowerUpdate(float dt)
    {
        // Have no power left.
        if (this.currentPower == 0)
            return 0;

        // Minus power.
        float subtractedPower = this.transferRate * dt;
        this.currentPower -= subtractedPower;

        // Dont undercharge.
        if(this.currentPower < 0.0f)
        {
            subtractedPower += this.currentPower;
            this.currentPower = 0.0f;
        }

        this.UpdateText();

        return subtractedPower;
    }

    private void UpdateText()
    {
        if(this.percentageText != null)
            this.percentageText.SetText((this.currentPower / this.MaximumCapacity * 100.0f).ToString("F2") + "%");
    }
}
