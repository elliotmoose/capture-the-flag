using MLAPI;
using MLAPI.Transports.UNET;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MLAPINetworkHUD : MonoBehaviour
{
    public InputField connectAddress;
    public InputField connectPort;
    public Button hostButton;
    public Button clientButton;
    public Button disconnectButton;
    public GameObject ipAndPortDisplayPanel;
    public InputField ipDisplayField;
    public InputField portDisplayField;

    UNetTransport transport;

    private void Start()
    {
        transport = GetComponent<UNetTransport>();
    }

    private void Update()
    {
        hostButton.gameObject.SetActive(!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient);
        clientButton.gameObject.SetActive(!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient);
        connectAddress.gameObject.SetActive(!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient);
        connectPort.gameObject.SetActive(!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient);
        ipAndPortDisplayPanel.SetActive(NetworkManager.Singleton.IsServer);
        disconnectButton.gameObject.SetActive(NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient);
    }

    public void Host()
    {
        NetworkManager.Singleton.StartHost();
        ipDisplayField.text = transport.ConnectAddress;
        portDisplayField.text = transport.ServerListenPort.ToString();
    }

    public void Client()
    {
        transport.ConnectAddress = connectAddress.text;
        transport.ConnectPort = int.Parse(connectPort.text);
        NetworkManager.Singleton.StartClient();
    }
}
