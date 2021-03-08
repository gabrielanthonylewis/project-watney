using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ServerListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText = null, ipAddressText = null,  portText = null;
    [SerializeField] private RawImage connectivityImage = null;
    [SerializeField] private Color connectedColour, disconnectedColour;

    private Toggle toggle;
    private ServerList owner = null;

    private void Start()
    {
        this.toggle = this.GetComponent<Toggle>();
        this.toggle.group = this.transform.parent.GetComponent<ToggleGroup>();
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
        this.owner.OnServerSelected(this.toggle.isOn ? this : null);
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
        this.connectivityImage.color = (isConnected) ? 
            this.connectedColour : this.disconnectedColour;
    }
}
