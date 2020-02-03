using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [System.Serializable]
    public enum Screen
    {
        Main = 0,
        Multiplayer = 1,
        EditServerDetails = 2
    }

    [SerializeField]
    private GameObject mainScreen = null;

    [SerializeField]
    private GameObject multiplayerScreen = null;

    [SerializeField]
    private GameObject editServerDetailsScreen = null;

    private Screen currentScreen = Screen.Main;

    [SerializeField]
    private int mainSceneIndex = 1;

    [SerializeField]
    private int loadingScreenIndex = 2;

    [SerializeField]
    private CustomNetworkManager _NetworkManager;

    bool hasJoined = false;

    public void LoadMainScene()
    {
        SceneManager.LoadScene(this.loadingScreenIndex);
    }

    public void HostGame()
    {
        if (Application.platform != RuntimePlatform.WebGLPlayer)
            this._NetworkManager.StartHost();

        this.LoadMainScene();
    }

    public void ChangeScreen(int newScreen)
    {
        this.SetScreenActive(this.currentScreen, false);
        this.SetScreenActive((Screen)newScreen, true);

        this.currentScreen = (Screen)newScreen;
    }

    private void SetScreenActive(Screen screen, bool state)
    {
        switch (screen)
        {
            case Screen.Main:
                this.mainScreen.SetActive(state);
                break;

            case Screen.Multiplayer:
                this.multiplayerScreen.SetActive(state);
                break;

            case Screen.EditServerDetails:
                this.editServerDetailsScreen.SetActive(state);
                break;
        }
    }

    public void ExitButtonPressed()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
