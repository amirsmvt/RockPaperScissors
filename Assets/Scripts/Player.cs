using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class Player : NetworkBehaviour
{
    public readonly SyncVar<string>  userName=new(new SyncTypeSettings(WritePermission.ClientUnsynchronized, ReadPermission.ExcludeOwner));


    public readonly SyncVar<int> score = new(new SyncTypeSettings(WritePermission.ClientUnsynchronized, ReadPermission.ExcludeOwner));



    [SerializeField] TMP_Text text;

    private void Awake()
    {
        userName.OnChange += UsernameChanged;
    }

    private void UsernameChanged(string prev, string next, bool asServer)
    {
        //throw new NotImplementedException();
        if (!IsOwner)
        {
            text.text = next;
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        SetUserName();
        if (IsOwner)
            text.gameObject.SetActive(false);
    }


    private void Update()
    {
        if (!IsOwner) { return; }
        if(Input.GetKeyDown(KeyCode.A))
        {
            SetUserName();
        }
    }
    [ServerRpc]
    private void SetUserName()
    {
        userName.Value = $"Player{Random.Range(0, 10)}";
    }
}
