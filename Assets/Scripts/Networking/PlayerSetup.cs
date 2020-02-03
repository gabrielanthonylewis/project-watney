using UnityEngine;
using Mirror;

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField]
    private Behaviour[] componentsToDisable;

    [SerializeField]
    private GameObject[] objectsToDisable;

    private void Start()
    {
        if (!NetworkClient.isConnected)
        {
            // Player is in singleplayer so spawn in all objects as server wont... 
            //todo: move this to its own script
            if(!NetworkClient.active)
            {
                NetworkIdentity[] identities = Resources.FindObjectsOfTypeAll<NetworkIdentity>();
                foreach (NetworkIdentity identity in identities)
                    identity.gameObject.SetActive(true);
            }
            
            return;
        }

        if (!this.isLocalPlayer)
        {
            foreach (Behaviour behaviour in this.componentsToDisable)
                behaviour.enabled = false;

            foreach (GameObject obj in this.objectsToDisable)
                obj.SetActive(false);
        }

        if (!this.isServer)
        {
            this.CmdSendName("Player" + this.netId.ToString());
        }

        this.InitialiseGameState();
    }

    private void InitialiseGameState()
    {
        // We are the server and client joined so send initial sync.
        if (GameObject.FindObjectOfType<SceneInitialisation>() != null)
            GameObject.FindObjectOfType<SceneInitialisation>().InitialisSceneState();
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
