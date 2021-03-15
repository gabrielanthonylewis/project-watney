using UnityEngine;
using Mirror;

public class PlayerInitialisation : MonoBehaviour
{
    [SerializeField] private GameObject offlinePlayerPrefab = null;
    [SerializeField] private Transform offlinePlayerSpawnPoint = null;

    private GameObject offlinePlayer = null;
    private bool hasSpawned = false;

    private void Start()
    {
        // If no connection and not connecting then it's a local game so spawn the player.
        if(!NetworkClient.isConnected && !NetworkClient.active)
        {
            Vector3 spawnPos = this.offlinePlayerSpawnPoint.position + Vector3.up;
            this.offlinePlayer = GameObject.Instantiate(this.offlinePlayerPrefab, spawnPos,
                 this.offlinePlayerSpawnPoint.rotation);

            /* As it's a local game we need to manually set all network identies on.
             * This must only be done once the player is spawned. */
            NetworkIdentity[] identities = Resources.FindObjectsOfTypeAll<NetworkIdentity>();
            foreach (NetworkIdentity identity in identities)
                identity.gameObject.SetActive(true);

            this.hasSpawned = true;
        }
    }

    private void Update()
    {
        if(this.hasSpawned)
            return;

        // If the client has connected to the server then add it to the game.
        if(NetworkClient.isConnected)
        {
            if(ClientScene.localPlayer == null)
            {
                ClientScene.AddPlayer();
                NetworkServer.SpawnObjects();

                this.hasSpawned = true;
            }            
        }
    }

    public GameObject GetOfflinePlayer()
    {
        return this.offlinePlayer;
    }
}
