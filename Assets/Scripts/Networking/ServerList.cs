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

public class ServerList : MonoBehaviour
{
    [SerializeField] private Transform contentHolder = null;
    [SerializeField] private GameObject serverListItemPrefab = null;
    [SerializeField] private int loadingSceneIndex = 2;
    [SerializeField] private TMP_InputField nameInputField = null;
    [SerializeField] private TMP_InputField ipAddressInputField = null;
    [SerializeField] private TMP_InputField portInputField = null;
    [SerializeField] private Button joinButton, editButton, removeButton;
    [SerializeField] private GameObject joiningServerMessageHolder = null;
    [SerializeField] private TextMeshProUGUI timePassedJoiningServerText = null;
    [SerializeField] private GameObject removeWarningMessageHolder = null;

    private List<Server> serverListItems = new List<Server>();
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
        foreach(Server server in this.serverListItems)
            GameObject.Destroy(server.item.gameObject);

        this.serverListItems = new List<Server>();
    }

    private void PopulateServerList(ServerDetailsCollection serverData)
    {
        foreach(ServerDetails serverDetails in serverData.serverList)
        {
            GameObject newServerListItemObj =
                GameObject.Instantiate(this.serverListItemPrefab, this.contentHolder, false);

            ServerListItem newItem = newServerListItemObj.GetComponent<ServerListItem>();
            newItem.Initialise(this, serverDetails);
            this.AddServer(new Server(serverDetails, newItem));
        }
    }

    private void AddServer(Server newServer)
    {
        this.serverListItems.Add(newServer);
        newServer.item.SetIsConnected(false);
        this.CheckConnection(newServer);
    }

    private void CheckConnection(Server server)
    {
        // Send discovery message to servers to see if there is a response back.
        IPAddress ipAddress = IPAddress.Parse(server.details.ipaddress);
        ushort port = ushort.Parse(server.details.port);
        NetworkDiscovery.SendDiscoveryRequest(new IPEndPoint(ipAddress, port));
    }

    private void JoinServer(string ipaddress)
    {
        NetworkManager.singleton.networkAddress = ipaddress;
        // Note: Nowhere does Mirror ask for a port?
        NetworkManager.singleton.StartClient();

        this.timePassedJoiningServer = 0.0f;
        this.timePassedJoiningServerText.text = this.timePassedJoiningServer.ToString() + "s";
        this.joiningServerMessageHolder.SetActive(true);

        this.StartCoroutine(this.UpdateTimePassedJoiningServer());
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

    private void OnClientConnected()
    {
        this.StopCoroutine(this.UpdateTimePassedJoiningServer());
        this.LoadMainScene();
    }

    private void OnClientDisconnected()
    {
        this.timePassedJoiningServerText.text = "! could not connect !";
    }

    public void LoadMainScene()
    {
        SceneManager.LoadScene(this.loadingSceneIndex);
    }

    public void CheckAllConnections()
    {
        foreach(Server server in this.serverListItems)
        {
            server.item.SetIsConnected(false);

            IPAddress ipAddress = IPAddress.Parse(server.details.ipaddress);
            ushort port = ushort.Parse(server.details.port);
            NetworkDiscovery.SendDiscoveryRequest(new IPEndPoint(ipAddress, port));
        }
    }

    public void OnReceivedServerResponse(NetworkDiscovery.DiscoveryInfo info)
    {
        string endPointIPAddress = info.EndPoint.Address.ToString();
        foreach(Server server in this.serverListItems)
        {
            if(server.details.ipaddress == endPointIPAddress)
                server.item.SetIsConnected(true);
        }
    }

    public void OnServerSelected(ServerListItem serverListItem)
    {
        if(serverListItem == null)
            return;

        foreach(Server server in this.serverListItems)
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
        if(this.selectedServer.item == null)
            return;

        this.JoinServer(this.selectedServer.details.ipaddress);
    }

    public void OnEditButtonPressed()
    {
        if(this.selectedServer.item == null)
            return;

        this.nameInputField.text = this.selectedServer.details.name;
        this.ipAddressInputField.text = this.selectedServer.details.ipaddress;
        this.portInputField.text = this.selectedServer.details.port;
    }

    public void OnSaveButtonPressed()
    {
        if(this.selectedServer.item == null)
            return;
    
        // Save all servers to list.
        List<ServerDetails> toSave = new List<ServerDetails>();
        for(int i = 0; i < this.serverListItems.Count; i++)
        {
            // Update selected server with new details.
            if(this.serverListItems[i].item == this.selectedServer.item)
            {
                Server newServerDetails = new Server(new ServerDetails(this.nameInputField.text,
                    this.ipAddressInputField.text, this.portInputField.text), this.serverListItems[i].item);
                this.serverListItems[i] = newServerDetails;
            }

            toSave.Add(this.serverListItems[i].details);
        }
        this.SaveServerDetailsToJSON(toSave.ToArray());        

        this.isAddingNewServer = false;
    }

    public void OnRemoveButtonPressed()
    {
        if(this.selectedServer.item == null)
            return;

        this.removeWarningMessageHolder.SetActive(true);
    }

    public void OnRemoveServerYesButtonPressed()
    {
        if(this.selectedServer.item == null)
            return;
       
        List<ServerDetails> toSave = new List<ServerDetails>();
        foreach(Server server in this.serverListItems)
        {
            // Skip the server to be removed, so it doesn't get added to the save list.
            if(server.item == this.selectedServer.item)
                continue;

            toSave.Add(server.details);
        }
        this.SaveServerDetailsToJSON(toSave.ToArray());

        this.RefreshList();

        this.removeWarningMessageHolder.SetActive(false);
    }

    private void SaveServerDetailsToJSON(ServerDetails[] serverDetails)
    {
        string json = File.ReadAllText(this.serverListJSONPath);
        ServerDetailsCollection loadedServerData = JsonUtility.FromJson<ServerDetailsCollection>(json);
        loadedServerData.serverList = serverDetails;
        string toSaveString = JsonUtility.ToJson(loadedServerData, true);
        File.WriteAllText(this.serverListJSONPath, toSaveString);
    }

    public void OnRemoveServerNoButtonPressed()
    {
        this.removeWarningMessageHolder.SetActive(false);
    }

    public void OnAddButtonPressed()
    {
        // Default values.
        this.nameInputField.text = "name";
        this.ipAddressInputField.text = "0.0.0.0";
        this.portInputField.text = "0000";

        // Spawn new server button.
        GameObject newServerListItemObj = GameObject.Instantiate(this.serverListItemPrefab,
            this.contentHolder, false);

        // Initialise with default values.
        ServerDetails newServerDetails = new ServerDetails(this.nameInputField.text,
            this.ipAddressInputField.text, this.portInputField.text);
        ServerListItem newItem = newServerListItemObj.GetComponent<ServerListItem>();
        newItem.Initialise(this, newServerDetails);
 
        // Add to the list and select.
        Server newServer = new Server(newServerDetails, newItem);
        this.serverListItems.Add(newServer);
        this.selectedServer = newServer;

        this.isAddingNewServer = true;
    }

    // NOTE: this is also called in edit which is why isAddingNewServer check needs to be done.
    public void OnAddBackButtonPressed()
    {
        if(!this.isAddingNewServer)
            return;

        // Delete newly created server item as the process was cancelled.
        GameObject.Destroy(this.serverListItems[this.serverListItems.Count - 1].item.gameObject);
        this.serverListItems.RemoveAt(this.serverListItems.Count - 1);
        this.selectedServer = new Server(new ServerDetails("", "", ""), null);

        this.editButton.interactable = false;
        this.joinButton.interactable = false;
        this.removeButton.interactable = false;

        this.isAddingNewServer = false;
    }
}
