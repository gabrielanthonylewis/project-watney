using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class CustomNetworkManager : NetworkManager
{
    [SerializeField]
    private DayNightCycle _DayNightCycle = null;

    public override void Awake()
    {
        base.Awake();

        SceneManager.sceneLoaded += this.OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex != 1)
            return;

    }

}
