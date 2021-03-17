using UnityEngine;

public class IOSocket : MonoBehaviour
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
            if(cableEnd == this.insertedCable)
                return;
        
            this.insertedCable = cableEnd;
            this.insertedCable.Pickup(this.transform, Vector3.zero, false, false, this.OnCablePickedUpByOther);
            this.insertedCable.SetConnection(this.controllerUnit, this);
        }
    }

    private void OnCablePickedUpByOther()
    {
        // Remove cable from socket.
        if(this.insertedCable != null)
        {
            this.insertedCable.SetConnection(null, null);
            this.insertedCable = null;
        }
    }
}
