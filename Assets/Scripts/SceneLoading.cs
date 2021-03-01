using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoading : MonoBehaviour
{
    [SerializeField] private int sceneIndexToLoad = 1;
    [SerializeField] private Image progressBarImage = null;

    private void Start()
    {
        this.StartCoroutine(this.LoadAsyncOperation());
    }

    private IEnumerator LoadAsyncOperation()
    {
        // As the game loads very quickly, this is needed so that the player can see the screen.
        yield return new WaitForSeconds(1.0f);

        AsyncOperation loadingOperation = SceneManager.LoadSceneAsync(this.sceneIndexToLoad);

        while(!loadingOperation.isDone)
        {
            this.progressBarImage.fillAmount = loadingOperation.progress;
            yield return new WaitForEndOfFrame(); // So it doesn't crash, allows program to breathe.
        }

        yield return new WaitForEndOfFrame();
    }
}
