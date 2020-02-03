using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableCableEnd : Moveable
{
    public delegate void ConnectionMadeDelegate();
    protected ConnectionMadeDelegate connectionMadeCallback = null;

    public delegate void ConnectionRemovedDelegate(PoweredUnit removedConnection, IOSocket removedSocket);
    protected ConnectionRemovedDelegate connectionRemovedCallback = null;

    private PoweredUnit connection = null;
    private IOSocket connectedSocket = null;

    public IOSocket GetConnectedSocket()
    {
        return this.connectedSocket;
    }

    public void SetConnection(PoweredUnit newConnection, IOSocket connectedSocket)
    {
        if(newConnection != null)
        {
            this.connection = newConnection;
            this.connectedSocket = connectedSocket;


            if (this.connectionMadeCallback != null)
                this.connectionMadeCallback.Invoke();
        }
        else
        {
            PoweredUnit tempConnection = this.connection;
            IOSocket tempSocket = this.connectedSocket;

            this.connection = null;
            this.connectedSocket = null;

            if (tempConnection != null && tempSocket != null)
            {
                if (this.connectionRemovedCallback != null)
                    this.connectionRemovedCallback.Invoke(tempConnection, tempSocket);
            }
        }
    }

    public bool HasConnection()
    {
        return (this.connection != null);
    }

    public PoweredUnit GetConnection()
    {
        return this.connection;
    }

    public void AddConnectionMadeCallback(ConnectionMadeDelegate myCallback)
    {
        this.connectionMadeCallback += myCallback;
    }

    public void AddConnectionRemovedCallback(ConnectionRemovedDelegate myCallback)
    {
        this.connectionRemovedCallback += myCallback;
    }
}
