using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class Pause : MonoBehaviour
{
    private bool isPaused = false;

    [SerializeField]
    private GameObject uiHolder = null;

    [SerializeField]
    private int mainMenuSceneIndex = 0;

    private void Start()
    {
        this.isPaused = false;
        this.UpdatePauseState();
    }

    private void Update()
    {
        if(Input.GetButtonDown("Pause"))
            this.TogglePause();
    }

    public void MainMenuButton()
    {
        if (NetworkClient.isConnected)
            NetworkClient.Disconnect();

        NetworkManager nm = GameObject.FindObjectOfType<NetworkManager>();
        //nm.StopHost();
     
        if (nm != null)
        {
            //nm.StopServer();
            //nm.StopClient();
            Destroy(nm.gameObject);
        }

        this.isPaused = false;
        Time.timeScale = 1.0f;

        SceneManager.LoadScene(this.mainMenuSceneIndex);
    }

    private void TogglePause()
    {
        this.isPaused = !this.isPaused;
        this.UpdatePauseState();
    }

    private void UpdatePauseState()
    {
        if (!NetworkClient.isConnected)
            Time.timeScale = (this.isPaused) ? 0.0f : 1.0f;

        Cursor.lockState = (this.isPaused) ? CursorLockMode.Confined : CursorLockMode.Locked;

        if (this.uiHolder != null)
            this.uiHolder.SetActive(this.isPaused);
    }
}
