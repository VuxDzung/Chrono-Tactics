using FishNet;
using FishNet.Managing.Scened;
using FishNet.Object;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkSceneLoader : CoreNetworkBehaviour
{
    public static NetworkSceneLoader Singleton;

    [SerializeField] GameObject loadingPanel;
    [SerializeField] Slider loadingBar;
    [SerializeField] TextMeshProUGUI loadingText;
    [SerializeField] private float delayBeforeClosePanel = 2;
    [SerializeField] private Sprite[] backgroundList;

    public Action OnLoadSceneCompleteServer;
    public Action OnLoadSceneCompleteClient;

    public Action<ClientPresenceChangeEventArgs> OnClientPresenceChangeEnd;
    public Action<ClientPresenceChangeEventArgs> OnClientPresenceChangeStart;

    private bool isEventTriggered;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        Singleton = this;
        gameObject.name = "NetworkSceneLoader";

        InstanceFinder.SceneManager.OnClientPresenceChangeEnd += ClientPresenceChangeEnd;
        InstanceFinder.SceneManager.OnClientPresenceChangeStart += ClientPresenceChangeStart;
        InstanceFinder.SceneManager.OnLoadPercentChange += LoadPercentageChange;

        InstanceFinder.SceneManager.OnLoadEnd += OnLoadSceneEnd;
    }

    public void LoadScene(string currentScene, string nextScene)
    {
        if (IsHost) isEventTriggered = false; // Reset the host flag.
            
        SceneLoadData nextSceneData = new SceneLoadData(nextScene);
        SceneUnloadData currentSceneData = new SceneUnloadData(currentScene);

        nextSceneData.ReplaceScenes = ReplaceOption.All;

        loadingPanel.SetActive(true);

        InstanceFinder.SceneManager.UnloadGlobalScenes(currentSceneData);
        InstanceFinder.SceneManager.LoadGlobalScenes(nextSceneData);
    }

    private void OnLoadSceneEnd(SceneLoadEndEventArgs args)
    {
        if (IsHost)
        {
            if (isEventTriggered) return;

            isEventTriggered = true;

            LoadSceneEndedClient();
            LoadSceneEndedServer();
        }
        else
        {
            if (IsServerInitialized) LoadSceneEndedServer();

            if (IsClientInitialized) LoadSceneEndedClient();
        }
    }

    private void ClientPresenceChangeEnd(ClientPresenceChangeEventArgs args)
    {
        OnClientPresenceChangeEnd?.Invoke(args);
    }

    private void ClientPresenceChangeStart(ClientPresenceChangeEventArgs args)
    {
        OnClientPresenceChangeStart?.Invoke(args);
    }

    public void Clear()
    {
        OnLoadSceneCompleteServer = null;
        OnLoadSceneCompleteClient = null;
    }
    
    private void LoadPercentageChange(SceneLoadPercentEventArgs args)
    {
        float percent = args.Percent;
        if (loadingText != null)
            loadingText.text = string.Format("{0:0.0}%", percent * 100f);
        if (loadingBar != null)
            loadingBar.value = percent;

        if (percent >= 1f)
            StartCoroutine(WaitBeforeHidePanel());
    }

    private IEnumerator WaitBeforeHidePanel()
    {
        yield return new WaitForSeconds(delayBeforeClosePanel);
        loadingPanel.SetActive(false);
    }

    [Client]
    private void LoadSceneEndedClient() 
    {
        Debug.Log("LoadSceneEndedClient");
        OnLoadSceneCompleteClient?.Invoke(); 
    }
    [Server]
    private void LoadSceneEndedServer() 
    {
        Debug.Log("LoadSceneEndedServer");
        OnLoadSceneCompleteServer?.Invoke(); 
    }
}