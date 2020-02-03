using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoading : MonoBehaviour
{
    [SerializeField]
    private int sceneIndexToLoad = 1;

    [SerializeField]
    private Image progressBarImage = null;

    // Start is called before the first frame update
    void Start()
    {
        // start async operation
        StartCoroutine(this.LoadAsyncOperation());
    }

    IEnumerator LoadAsyncOperation()
    {
        // NOTE: REMOVE WHEN GAME TAKES LONGER TO LOAD, USED TO SEE SCREEN
        yield return new WaitForSeconds(1.0f);

        // create an async operation
        AsyncOperation gameLevel = SceneManager.LoadSceneAsync(this.sceneIndexToLoad);

        while(!gameLevel.isDone)
        {
            this.progressBarImage.fillAmount = gameLevel.progress;
            yield return new WaitForEndOfFrame(); // so it doesnt crash, allows program to breathe
        }

        yield return new WaitForEndOfFrame();
    }
}
