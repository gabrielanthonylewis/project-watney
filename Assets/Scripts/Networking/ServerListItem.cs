using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ServerListItem : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI nameText = null, ipAddressText = null,  portText = null;

    [SerializeField]
    private RawImage connectivityImage = null;

    [SerializeField]
    private Color connectedColour, disconnectedColour;


    public bool isConnected { private set; get; }
    private ServerList owner = null;

    private void Start()
    {
        this.GetComponent<Toggle>().group = this.transform.parent.GetComponent<ToggleGroup>();
    }

    public ServerListItem(ServerList owner, ServerDetails serverDetails)
    {
        this.Initialise(owner, serverDetails);
    }

    public void Initialise(ServerList owner, ServerDetails serverDetails)
    {
        this.owner = owner;
        this.UpdateVisuals(serverDetails);
    }

    public void UpdateVisuals(ServerDetails serverDetails)
    {
        this.SetName(serverDetails.name);
        this.SetIPAddress(serverDetails.ipaddress);
        this.SetPort(serverDetails.port);
        this.SetIsConnected(false);
    }

    public void OnButtonPressed()
    {
        this.owner.OnServerSelected(this.GetComponent<Toggle>().isOn ? this : null);
    }

    private void SetName(string name)
    {
        this.nameText.text = name;
    }

    private void SetIPAddress(string ipAddress)
    {
        this.ipAddressText.text = ipAddress;
    }

    private void SetPort(string port)
    {
        this.portText.text = port;
    }

    public void SetIsConnected(bool isConnected)
    {
        this.isConnected = isConnected;

        this.connectivityImage.color = (isConnected) ? 
            this.connectedColour : this.disconnectedColour;
    }
}
