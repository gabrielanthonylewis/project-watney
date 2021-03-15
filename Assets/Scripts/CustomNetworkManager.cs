using UnityEngine;
using Mirror;

public class CustomNetworkManager : NetworkManager
{
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        SceneInitialisation sceneInitialisation = GameObject.FindObjectOfType<SceneInitialisation>();
        if(sceneInitialisation != null)
            sceneInitialisation.InitialiseSceneState(conn);
    }
}
