using UnityEngine;
using Mirror;

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField] private Behaviour[] componentsToDisable;
    [SerializeField] private GameObject[] objectsToDisable;

    private void Start()
    {
        if(!NetworkClient.isConnected && !NetworkClient.active)
            return;

        // Disables things such as the camera and movement on the non-local players.
        if(!this.isLocalPlayer)
        {
            foreach(Behaviour behaviour in this.componentsToDisable)
                behaviour.enabled = false;

            foreach(GameObject obj in this.objectsToDisable)
                obj.SetActive(false);
        }

        // Setup transform name on all clients for debugging purposes.
        this.CmdSendName("Player" + this.netId.ToString());
    }

    [Command]
    private void CmdSendName(string name)
    {
        this.RpcUpdateName(name);
    }

    [ClientRpc]
    private void RpcUpdateName(string name)
    {
        this.transform.name = name;
    }
}
