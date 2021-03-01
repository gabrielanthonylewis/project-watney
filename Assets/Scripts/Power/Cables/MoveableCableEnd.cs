public class MoveableCableEnd : Moveable
{
    public delegate void ConnectionMadeDelegate();
    protected ConnectionMadeDelegate connectionMadeCallback = null;

    public delegate void ConnectionRemovedDelegate(PoweredUnit removedConnection, IOSocket removedSocket);
    protected ConnectionRemovedDelegate connectionRemovedCallback = null;

    private PoweredUnit connection = null;
    private IOSocket connectedSocket = null;

    public void SetConnection(PoweredUnit connection, IOSocket connectedSocket)
    {
        if(connection != null && connectedSocket != null)
            this.OnConnectionMade(connection, connectedSocket);
        else
           this.OnConnectionRemoved();
    }

    private void OnConnectionMade(PoweredUnit connection, IOSocket connectedSocket)
    {
        this.connection = connection;
        this.connectedSocket = connectedSocket;

        if(this.connectionMadeCallback != null)
            this.connectionMadeCallback.Invoke();
    }

    private void OnConnectionRemoved()
    {
        /* Temp variables are required as the callback will require
         * the connection to be null and at the same time the original 
         * connection (before removal) must be passed into the callback. */
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

    public IOSocket GetConnectedSocket()
    {
        return this.connectedSocket;
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
