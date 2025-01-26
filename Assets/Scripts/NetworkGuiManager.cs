using FishNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkGuiManager : MonoBehaviour
{
    [SerializeField] Button _ServerButton;
    [SerializeField] Button _ConnectButton;
    [SerializeField] GameObject _MainMenu;
    [SerializeField] GameObject _GameMenu;
    // Start is called before the first frame update
    void Start()
    {
        _ServerButton.onClick.AddListener(ServerConnection);
        _ConnectButton.onClick.AddListener(HostConnection);
    }

    private void ServerConnection()
    {
        InstanceFinder.ServerManager.StartConnection();
        InstanceFinder.ClientManager.StartConnection();
    }

    private void HostConnection()
    {
        InstanceFinder.ClientManager.StartConnection();
    }
    
}
