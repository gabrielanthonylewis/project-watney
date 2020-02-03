using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cable : MonoBehaviour
{
    [SerializeField]
    private MoveableCableEnd connectionA = null;

    [SerializeField]
    private MoveableCableEnd connectionB = null;

    private void Start()
    {
        if (this.connectionA != null)
        {
            this.connectionA.AddConnectionMadeCallback(this.ConnectionMade);
            this.connectionA.AddConnectionRemovedCallback(this.ConnectionRemoved);
        }

        if (this.connectionB != null)
        {
            this.connectionB.AddConnectionMadeCallback(this.ConnectionMade);
            this.connectionB.AddConnectionRemovedCallback(this.ConnectionRemoved);
        }
    }

    private void ConnectionMade()
    {
        if(this.connectionA.HasConnection() &&
            this.connectionB.HasConnection())
        {
            // Find end that is connected to input, if other end connected to output then add input to connection
            if(this.connectionA.GetConnectedSocket() != null)
            {
                if (this.connectionA.GetConnectedSocket().GetSocketType() == IOSocket.SocketType.Input)
                {
                    if (this.connectionB.GetConnectedSocket().GetSocketType() == IOSocket.SocketType.Output)
                    {
                        this.connectionA.GetConnection().AddInput(this.connectionB.GetConnection());
                    }
                }

                if (this.connectionB.GetConnectedSocket().GetSocketType() == IOSocket.SocketType.Input)
                {
                    if (this.connectionA.GetConnectedSocket().GetSocketType() == IOSocket.SocketType.Output)
                    {
                        this.connectionB.GetConnection().AddInput(this.connectionA.GetConnection());
                    }
                }
            }
        }
    }

    private void ConnectionRemoved(PoweredUnit removedConnection, IOSocket removedSocket)
    {
        if(removedSocket.GetSocketType() == IOSocket.SocketType.Input)
        {
            // find which one is removed
            if(!this.connectionA.HasConnection())
            {
                if(this.connectionB.HasConnection())
                    removedConnection.RemoveInput(this.connectionB.GetConnection());
            }
            else if (!this.connectionB.HasConnection())
            {
                if (this.connectionA.HasConnection())
                    removedConnection.RemoveInput(this.connectionA.GetConnection());
            }
        }
        else if (removedSocket.GetSocketType() == IOSocket.SocketType.Output)
        {
            // find which one is removed
            if (!this.connectionA.HasConnection())
            {
                if (this.connectionB.HasConnection())
                    this.connectionB.GetConnection().RemoveInput(removedConnection);
            }
            else if (!this.connectionB.HasConnection())
            {
                if (this.connectionA.HasConnection())
                    this.connectionA.GetConnection().RemoveInput(removedConnection);
            }
        }
    }

}
