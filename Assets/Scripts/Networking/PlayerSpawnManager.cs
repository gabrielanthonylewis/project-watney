using UnityEngine;
using Mirror;

public class PlayerSpawnManager : NetworkBehaviour
{
    [SerializeField] private GameObject offlinePlayerPrefab = null;
    [SerializeField] private Transform offlinePlayerSpawnPoint = null;
    [SerializeField] private GameObject respawnUI = null;
    [SerializeField] private PlayerInitialisation playerInitialisation = null;

    private GameObject GetLocalPlayer() 
    {
        return (NetworkClient.isConnected) ? NetworkClient.connection.identity.gameObject
            : playerInitialisation.GetOfflinePlayer();
    }

    private void ShowRespawnUI(bool shouldShow)
    {
        this.respawnUI.SetActive(shouldShow);
        Cursor.lockState = (shouldShow) ? CursorLockMode.Confined : CursorLockMode.Locked;
    }

    #region Respawn
    public void RespawnButtonPressed()
    {
        if(NetworkClient.isConnected)
            this.CmdRespawnPlayer(NetworkClient.connection.identity.gameObject);
        else
            this.RespawnPlayer(playerInitialisation.GetOfflinePlayer());
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

        if(this.GetLocalPlayer() == player)
            this.ShowRespawnUI(false);
    }
    #endregion

    #region Player Died
    public void OnLocalPlayerDied()
    {
        GameObject player = this.GetLocalPlayer();
        if(NetworkClient.isConnected)
            this.CmdPlayerDied(player);
        else
            this.PlayerDied(player);
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
        // Return as already dead.
        if(this.respawnUI.activeSelf)
            return;

        player.SetActive(false);

        if(this.GetLocalPlayer() == player)
            this.ShowRespawnUI(true);
    }
    #endregion
}
