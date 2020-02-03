using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerSpawnManager : NetworkBehaviour
{
    [SerializeField]
    private GameObject offlinePlayerPrefab = null;

    [SerializeField]
    private Transform offlinePlayerSpawnPoint = null;

    [SerializeField]
    private GameObject respawnUI = null;

    [SerializeField]
    private PlayerInitialisation _PlayerInitialisation = null;

    public void RespawnButtonPressed()
    {
        this.RespawnLocalPlayer();
    }

    private void RespawnLocalPlayer()
    {
        if (NetworkClient.isConnected)
            this.CmdRespawnPlayer(NetworkClient.connection.identity.gameObject);
        else
            this.RespawnPlayer(_PlayerInitialisation.localPlayer);
    }

    [Command]
    private void CmdRespawnPlayer(GameObject player)
    {
        this.RpcRespawnPlayer(player);
    }

    [ClientRpc]
    private void RpcRespawnPlayer(GameObject player)
    {
        this.RespawnPlayer(player);
    }

    private void RespawnPlayer(GameObject player)
    {
        player.transform.position = this.offlinePlayerSpawnPoint.position + Vector3.up;
        player.transform.rotation = this.offlinePlayerSpawnPoint.rotation;

        player.GetComponent<PlayerStats>().Respawn();

        player.SetActive(true);

        GameObject localPlayer = _PlayerInitialisation.localPlayer;
        if (NetworkClient.isConnected)
            localPlayer = NetworkClient.connection.identity.gameObject;
        if (localPlayer == player)
        {
            this.respawnUI.SetActive(false);

            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    [Command]
    private void CmdPlayerDied(GameObject player)
    {
        this.RpcPlayerDied(player);
    }

    [ClientRpc]
    private void RpcPlayerDied(GameObject player)
    {
        this.PlayerDied(player);
    }

    private void PlayerDied(GameObject player)
    {
        // return as already dead
        if (this.respawnUI.activeSelf)
            return;

        player.SetActive(false);

        GameObject localPlayer = _PlayerInitialisation.localPlayer;
        if (NetworkClient.isConnected)
            localPlayer = NetworkClient.connection.identity.gameObject;
        if (localPlayer == player)
        {
            Cursor.lockState = CursorLockMode.Confined;
            this.respawnUI.SetActive(true);
        }
    }

    public void LocalPlayerDied()
    {
        GameObject player = _PlayerInitialisation.localPlayer;
        if (NetworkClient.isConnected)
            player = NetworkClient.connection.identity.gameObject;

        if(NetworkClient.isConnected)
            this.CmdPlayerDied(player);
        else
            this.PlayerDied(player);
    }
}
