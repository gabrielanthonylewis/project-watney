using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerInitialisation : MonoBehaviour
{
    [SerializeField]
    private GameObject offlinePlayerPrefab = null;

    [SerializeField]
    private Transform offlinePlayerSpawnPoint = null;

    private bool hasSpawned = false;

    public GameObject localPlayer = null;

    private void Update()
    {
        if (this.hasSpawned)
            return;

        if (!NetworkClient.isConnected && !NetworkClient.active)
        {
            this.localPlayer =
                Instantiate(this.offlinePlayerPrefab, this.offlinePlayerSpawnPoint.position + Vector3.up, this.offlinePlayerSpawnPoint.rotation);

            this.hasSpawned = true;
        }
        // client ready
        if (NetworkClient.isConnected)
        {
            if (ClientScene.localPlayer == null)
            {
                ClientScene.AddPlayer();


                NetworkServer.SpawnObjects();
                this.hasSpawned = true;
            }
        }

    }
}
