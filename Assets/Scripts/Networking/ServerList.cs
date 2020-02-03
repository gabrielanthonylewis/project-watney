using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using Mirror;
using System.Net;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public struct ServerDetailsCollection
{
    public ServerDetails[] serverList;
}

[System.Serializable]
public struct ServerDetails
{
    public string name;
    public string ipaddress;
    public string port;

    public ServerDetails(string name, string ipaddress, string port)
    {
        this.name = name;
        this.ipaddress = ipaddress;
        this.port = port;
    }
}

public struct Server
{
    public ServerDetails details;
    public ServerListItem item;

    public Server(ServerDetails details, ServerListItem item)
    {
        this.details = details;
        this.item = item;

        if(item != null)
            this.item.UpdateVisuals(details);
    }
}

public class DiscoveryMessage : MessageBase
{
    public string ipAddress;

    public DiscoveryMessage(string ipAddress)
    {
        this.ipAddress = ipAddress;
    }

    public override void Deserialize(NetworkReader reader)
    {
        this.ipAddress = reader.ReadString();
    }

    public override void Serialize(NetworkWriter writer)
    {
        writer.WriteString(this.ipAddress);
    }
}

public class ServerList : MonoBehaviour
{
    [SerializeField]
    private Transform contentHolder = null;

    [SerializeField]
    private GameObject serverListItemPrefab = null;

    [SerializeField]
    private CustomNetworkManager _NetworkManager;

    [SerializeField]
    private int mainSceneIndex = 1;

    [SerializeField]
    private int loadingSceneIndex = 2;

    private List<Server> serverListItems = new List<Server>();

    [SerializeField]
    private NetworkDiscovery _NetworkDiscovery = null;

    [SerializeField]
    private TMP_InputField nameInputField = null;

    [SerializeField]
    private TMP_InputField ipAddressInputField = null;

    [SerializeField]
    private TMP_InputField portInputField = null;

    [SerializeField]
    private Button joinButton, refreshButton, editButton, addButton, removeButton;

    [SerializeField]
    private GameObject joiningServerMessageHolder = null;

    [SerializeField]
    private TextMeshProUGUI timePassedJoiningServerText = null;

    [SerializeField]
    private GameObject removeWarningMessageHolder = null;

    private Server selectedServer = new Server(new ServerDetails("","",""), null);

    private float timePassedJoiningServer = 0.0f;
    private bool isAddingNewServer = false;
    private string serverListJSONPath;

    private void Start()
    {
        this.serverListJSONPath = Application.dataPath + "/StreamingAssets/serverlist.json";

        this.RegisterEventListeners();
        this.RefreshList();
    }

    private void RegisterEventListeners()
    {
        NetworkDiscovery.onReceivedServerResponse += this.OnReceivedServerResponse;
        Transport.activeTransport.OnClientConnected.AddListener(this.OnClientConnected);
        Transport.activeTransport.OnClientDisconnected.AddListener(this.OnClientDisconnected);
    }

    public void RefreshList()
    {
        this.EmptyList();

        // Get JSON data.
        string json = File.ReadAllText(this.serverListJSONPath);
        ServerDetailsCollection loadedServerData = JsonUtility.FromJson<ServerDetailsCollection>(json);

        // Populate server list.
        this.PopulateServerList(loadedServerData);

        this.selectedServer = new Server(new ServerDetails("", "", ""), null);

        this.editButton.interactable = false;
        this.joinButton.interactable = false;
        this.removeButton.interactable = false;
    }

    private void EmptyList()
    {
        foreach (Server server in this.serverListItems)
            Destroy(server.item.gameObject);

        this.serverListItems = new List<Server>();
    }

    private void PopulateServerList(ServerDetailsCollection serverData)
    {
        foreach (ServerDetails serverDetails in serverData.serverList)
        {
            GameObject newServerListItemObj =
                GameObject.Instantiate(this.serverListItemPrefab, this.contentHolder, false);

            ServerListItem newItem = newServerListItemObj.GetComponent<ServerListItem>();
            newItem.Initialise(this, serverDetails);
            this.OnServerLoaded(new Server(serverDetails, newItem));
        }
    }

    private void OnServerLoaded(Server newServer)
    {
        this.serverListItems.Add(newServer);
        newServer.item.SetIsConnected(false);

        this.CheckConnection(newServer);
    }

    private void CheckConnection(Server server)
    {
        // Send message to servers to see if there is a response back.s);
        IPAddress ipAddress = IPAddress.Parse(server.details.ipaddress);
        ushort port = ushort.Parse(server.details.port);

        //todo: after calling this 3 times (once for each server i get the port error log, ask discord?)
        NetworkDiscovery.SendDiscoveryRequest(new IPEndPoint(ipAddress, port));
    }

    private void JoinServer(string ipaddress)
    {
        NetworkManager.singleton.networkAddress = ipaddress;
        // Todo: set port somehow?
        NetworkManager.singleton.StartClient();

        this.timePassedJoiningServer = 0.0f;
        this.joiningServerMessageHolder.SetActive(true);
        this.timePassedJoiningServerText.text = this.timePassedJoiningServer.ToString() + "s";

        StartCoroutine(this.UpdateTimePassedJoiningServer());
    }

    private IEnumerator UpdateTimePassedJoiningServer()
    {
        while(NetworkClient.active)
        {
            this.timePassedJoiningServer += Time.deltaTime;
            this.timePassedJoiningServerText.text = this.timePassedJoiningServer.ToString("F2") + "s";
            yield return new WaitForEndOfFrame();
        }

        yield return null;
    }

    public void StopTryingToConnect()
    {
        this.joiningServerMessageHolder.SetActive(false);

        NetworkManager.singleton.StopClient();
    }

    void OnClientConnected()
    {
        // todo: do I need this? I think I do as .active is true if connected.
        StopCoroutine(this.UpdateTimePassedJoiningServer());
        this.LoadMainScene();
    }

    void OnClientDisconnected()
    {
        this.timePassedJoiningServerText.text = "! could not connect !";
    }

    public void LoadMainScene()
    {
        SceneManager.LoadScene(this.loadingSceneIndex);
    }

    public void CheckAllConnections()
    {
        for(int i = 0; i < this.serverListItems.Count; i++)
        {
            this.serverListItems[i].item.SetIsConnected(false);

            IPAddress ipAddress = IPAddress.Parse(this.serverListItems[i].details.ipaddress);
            ushort port = ushort.Parse(this.serverListItems[i].details.port);
            NetworkDiscovery.SendDiscoveryRequest(new IPEndPoint(ipAddress, port));
        }
    }

    public void OnReceivedServerResponse(NetworkDiscovery.DiscoveryInfo info)
    {
        foreach (Server server in this.serverListItems)
        {
            if (server.details.ipaddress == info.EndPoint.Address.ToString())
                server.item.SetIsConnected(true);
        }
    }

    public void OnServerSelected(ServerListItem serverListItem)
    {
        if (serverListItem == null)
            return;

        foreach (Server server in this.serverListItems)
        {
            if(server.item == serverListItem)
                this.selectedServer = server;
        }

        this.editButton.interactable = true;
        this.joinButton.interactable = true;
        this.removeButton.interactable = true;
    }

    public void OnJoinButtonPressed()
    {
        if (this.selectedServer.item == null)
            return;

        this.JoinServer(this.selectedServer.details.ipaddress);
    }

    public void OnEditButtonPressed()
    {
        if (this.selectedServer.item == null)
            return;

        this.nameInputField.text = this.selectedServer.details.name;
        this.ipAddressInputField.text = this.selectedServer.details.ipaddress;
        this.portInputField.text = this.selectedServer.details.port;
    }

    public void OnSaveButtonPressed()
    {
        if (this.selectedServer.item == null)
            return;
    
        string json = File.ReadAllText(this.serverListJSONPath);
        ServerDetailsCollection loadedServerData = JsonUtility.FromJson<ServerDetailsCollection>(json);

        // Update selected server with new details.        


        // Save all servers to list.
        List<ServerDetails> toSave = new List<ServerDetails>();
        for(int i = 0; i < this.serverListItems.Count; i++)
        {
            if(this.serverListItems[i].item == this.selectedServer.item)
            {
                Server newServerDetails = new Server(new ServerDetails(this.nameInputField.text,
                    this.ipAddressInputField.text, this.portInputField.text), this.serverListItems[i].item);
                this.serverListItems[i] = newServerDetails;
            }

            toSave.Add(this.serverListItems[i].details);
        }

        loadedServerData.serverList = toSave.ToArray();
        string toSaveString = JsonUtility.ToJson(loadedServerData, true);

        File.WriteAllText(this.serverListJSONPath, toSaveString);

        this.isAddingNewServer = false;
    }

    public void OnRemoveButtonPressed()
    {
        if (this.selectedServer.item == null)
            return;

        this.removeWarningMessageHolder.SetActive(true);
    }

    public void OnRemoveServerYesButtonPressed()
    {
        if (this.selectedServer.item == null)
            return;

        string json = File.ReadAllText(this.serverListJSONPath);
        ServerDetailsCollection loadedServerData = JsonUtility.FromJson<ServerDetailsCollection>(json);

        List<ServerDetails> toSave = new List<ServerDetails>();
        foreach (Server server in this.serverListItems)
        {
            if (server.item == this.selectedServer.item)
                continue;

            toSave.Add(server.details);
        }

        loadedServerData.serverList = toSave.ToArray();
        string toSaveString = JsonUtility.ToJson(loadedServerData, true);

        File.WriteAllText(this.serverListJSONPath, toSaveString);

        this.RefreshList();

        this.removeWarningMessageHolder.SetActive(false);
    }

    public void OnRemoveServerNoButtonPressed()
    {
        this.removeWarningMessageHolder.SetActive(false);
    }

    public void OnAddButtonPressed()
    {
        this.nameInputField.text = "name";
        this.ipAddressInputField.text = "0.0.0.0";
        this.portInputField.text = "0000";

        GameObject newServerListItemObj =
            GameObject.Instantiate(this.serverListItemPrefab, this.contentHolder, false);

        ServerDetails newServerDetails = new ServerDetails("name", "0.0.0.0", "0000");
        ServerListItem newItem = newServerListItemObj.GetComponent<ServerListItem>();
        newItem.Initialise(this, newServerDetails);
 
        Server newServer = new Server(newServerDetails, newItem);
        this.serverListItems.Add(newServer);
        this.selectedServer = newServer;

        this.isAddingNewServer = true;
    }

    // NOTE that this is also called in edit which is why check needs to be done..
    public void OnAddBackButtonPressed()
    {
        if (!this.isAddingNewServer)
            return;

        // delete last server item
        GameObject.Destroy(this.serverListItems[this.serverListItems.Count - 1].item.gameObject);
        this.serverListItems.RemoveAt(this.serverListItems.Count - 1);
        this.selectedServer = new Server(new ServerDetails("", "", ""), null);

        this.editButton.interactable = false;
        this.joinButton.interactable = false;
        this.removeButton.interactable = false;

        this.isAddingNewServer = false;
    }
}
