using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class Pause : MonoBehaviour
{
    [SerializeField] private GameObject uiHolder = null;
    [SerializeField] private int mainMenuSceneIndex = 0;

    private bool isPaused = false;

    private void Start()
    {
        this.SetPausedState(false);
    }

    private void Update()
    {
        if(Input.GetButtonDown("Pause"))
            this.SetPausedState(!this.isPaused);
    }

    private void SetPausedState(bool isPaused)
    {
        this.isPaused = isPaused;

        if(this.uiHolder != null)
            this.uiHolder.SetActive(this.isPaused);

        Cursor.lockState = (this.isPaused) ? CursorLockMode.Confined : CursorLockMode.Locked;

        if(!NetworkClient.isConnected)
            Time.timeScale = Convert.ToInt32(!this.isPaused);
    }

    public void MainMenuButton()
    {
        if(NetworkClient.isConnected)
            NetworkClient.Disconnect();

        NetworkManager networkManager = GameObject.FindObjectOfType<NetworkManager>();    
        if(networkManager != null)
            Destroy(networkManager.gameObject);

        this.isPaused = false;
        Time.timeScale = 1.0f;

        SceneManager.LoadScene(this.mainMenuSceneIndex);
    }
}
