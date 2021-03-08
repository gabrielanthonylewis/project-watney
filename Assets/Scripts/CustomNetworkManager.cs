using UnityEngine;
using Mirror;

public class CustomNetworkManager : NetworkManager
{
    [SerializeField] private SceneInitialisation sceneInitialisation;

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        // Scene Initialisation
        if(this.sceneInitialisation != null)
            this.sceneInitialisation.InitialisSceneState();
    }
}
