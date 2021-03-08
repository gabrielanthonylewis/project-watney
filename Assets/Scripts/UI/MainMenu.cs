using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class MainMenu : MonoBehaviour
{
    private enum Screen
    {
        Main = 0,
        Multiplayer = 1,
        EditServerDetails = 2
    }

    [SerializeField] private GameObject mainScreen = null;
    [SerializeField] private GameObject multiplayerScreen = null;
    [SerializeField] private GameObject editServerDetailsScreen = null;
    [SerializeField] private int loadingScreenIndex = 2;
    [SerializeField] private NetworkManager networkManager;

    private Screen currentScreen = Screen.Main;

    private void SetScreenActive(Screen screen, bool state)
    {
        switch(screen)
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

    public void ChangeScreen(int screenIdx)
    {
        Screen newScreen = (Screen)screenIdx;

        this.SetScreenActive(this.currentScreen, false);
        this.SetScreenActive(newScreen, true);

        this.currentScreen = newScreen;
    }

    public void LoadMainScene()
    {
        SceneManager.LoadScene(this.loadingScreenIndex);
    }

    public void HostGame()
    {
        if (Application.platform != RuntimePlatform.WebGLPlayer)
            this.networkManager.StartHost();

        this.LoadMainScene();
    }    

    public void OnExitButtonPressed()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
