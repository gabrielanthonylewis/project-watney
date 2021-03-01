using UnityEngine;
using Mirror;

public class IOSocket : NetworkBehaviour
{
    public enum SocketType
    {
        Input = 0,
        Output = 1
    };

    [SerializeField] private PoweredUnit controllerUnit = null;
    [SerializeField] private SocketType socketType = SocketType.Input;
    [SerializeField] private MoveableCableEnd insertedCable = null;

    public SocketType GetSocketType()
    {
        return this.socketType;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(this.insertedCable != null)
            return;

        // Atach cable to this socket.
        MoveableCableEnd cableEnd = other.GetComponent<MoveableCableEnd>();
        if(cableEnd != null)
        {
            this.insertedCable = cableEnd;
            this.insertedCable.AddPickedUpCallback(this.OnCablePickedUp);
            this.insertedCable.InvokePickedUpCallback(this.transform);
            this.insertedCable.SetConnection(this.controllerUnit, this);
        }
    }

    private void OnCablePickedUp(Transform by)
    {
        if(by != this.transform)
        {
            // Remove cable from socket.
            if(this.insertedCable != null)
            {
                this.insertedCable.SetConnection(null, null);
                this.insertedCable.RemovePickedUpCallback(this.OnCablePickedUp);
                this.insertedCable = null;
            }
        }
    }
}
