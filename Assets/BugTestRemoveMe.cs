using UnityEngine;
using Mirror;

public class BugTestRemoveMe : MonoBehaviour
{
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.G))
            this.JoinServer("25.20.18.205");

        if (Input.GetKeyDown(KeyCode.H))
            this.StopTryingToConnect();

        Debug.Log(Time.frameCount);
    }

    private void JoinServer(string ipaddress)
    {
        NetworkManager.singleton.networkAddress = ipaddress;
        NetworkManager.singleton.StartClient();
    }

    public void StopTryingToConnect()
    {
        NetworkManager.singleton.StopClient();
    }
}
