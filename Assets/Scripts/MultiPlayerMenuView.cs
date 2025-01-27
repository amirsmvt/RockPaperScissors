using FishNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiPlayerMenuView : View
{
    [SerializeField] Button _ServerButton;
    [SerializeField] Button _ConnectButton;
    public override void Initialize()
    {
        _ServerButton.onClick.AddListener(ServerConnection);
        _ConnectButton.onClick.AddListener(HostConnection);
        base.Initialize();
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
