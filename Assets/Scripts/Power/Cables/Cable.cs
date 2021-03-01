using UnityEngine;

public class Cable : MonoBehaviour
{
    [SerializeField] private MoveableCableEnd cableEndA = null;
    [SerializeField] private MoveableCableEnd cableEndB = null;

    private void Start()
    {
        this.InitialiseCableEnd(this.cableEndA);
        this.InitialiseCableEnd(this.cableEndB);
    }

    private void InitialiseCableEnd(MoveableCableEnd cableEnd)
    {
        cableEnd.AddConnectionMadeCallback(this.OnConnectionMade);
        cableEnd.AddConnectionRemovedCallback(this.OnConnectionRemoved);
    }

    /* On a full connection energy the inputs are hooked up so that
     * the energy can be transfered from one device to another. */
    private void OnConnectionMade()
    {
        bool hasFullConnection = this.cableEndA.HasConnection() && this.cableEndB.HasConnection(); 
        if(!hasFullConnection)
            return;
  
        IOSocket.SocketType cableEndASocket = this.cableEndA.GetConnectedSocket().GetSocketType();
        IOSocket.SocketType cableEndBSocket = this.cableEndB.GetConnectedSocket().GetSocketType();

        if(cableEndASocket == IOSocket.SocketType.Input &&
            cableEndBSocket == IOSocket.SocketType.Output)
        {
            // In this case cableEndB is outputting its energy to to cableEndA's input socket.
            this.cableEndA.GetConnection().AddInput(this.cableEndB.GetConnection());
        }

        if(cableEndBSocket == IOSocket.SocketType.Input &&
            cableEndASocket == IOSocket.SocketType.Output)
        {
            // In this case cableEndA is outputting its energy to to cableEndB's input socket.
            this.cableEndB.GetConnection().AddInput(this.cableEndA.GetConnection());
        }
    }

    // Disconnect the still connected device from the removed device.
    private void OnConnectionRemoved(PoweredUnit removedConnection, IOSocket removedSocket)
    {
        switch(removedSocket.GetSocketType())
        {
            case IOSocket.SocketType.Input:

                if(!this.cableEndA.HasConnection() && this.cableEndB.HasConnection())
                    removedConnection.RemoveInput(this.cableEndB.GetConnection());
                else if(!this.cableEndB.HasConnection() && this.cableEndA.HasConnection())
                    removedConnection.RemoveInput(this.cableEndA.GetConnection());

                break;

            case IOSocket.SocketType.Output:

                if(!this.cableEndA.HasConnection() && this.cableEndB.HasConnection())
                    this.cableEndB.GetConnection().RemoveInput(removedConnection);
                else if(!this.cableEndB.HasConnection() && this.cableEndA.HasConnection())
                    this.cableEndA.GetConnection().RemoveInput(removedConnection);

                break;
        }
    }
}
