using UnityEngine;

[System.Serializable]
public class PowerGrid 
{
    [SerializeField]
    public Powerable[] powerables;

    public void TurnOn()
    {
        foreach (Powerable powerable in this.powerables)
            powerable.TurnOn();
    }

    public void TurnOff()
    {
        foreach (Powerable powerable in this.powerables)
            powerable.TurnOff();
    }
}
