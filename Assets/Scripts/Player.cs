using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Player : NetworkBehaviour
{
    public static Player instance;

    public readonly SyncVar<string> userName =
        new(new SyncTypeSettings(WritePermission.ClientUnsynchronized, ReadPermission.ExcludeOwner));

    public readonly SyncVar<int> score =
        new(new SyncTypeSettings(WritePermission.ClientUnsynchronized, ReadPermission.ExcludeOwner));

    [SerializeField] TMP_Text text;

    [field: AllowMutableSyncTypeAttribute]
    public SyncVar<RPSChoice> currentChoice = new SyncVar<RPSChoice>();

    private void Awake()
    {
        userName.OnChange += UsernameChanged;
    }

    private void UsernameChanged(string prev, string next, bool asServer)
    {
        if (!IsOwner)
            text.text = next;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        if (GameManager.instance != null)
        {
            GameManager.instance._players.Add(this);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner)
        {
            instance = this;
            if (text != null) text.gameObject.SetActive(false);
            SetUserName();

        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        if (GameManager.instance != null)
        {
            GameManager.instance._players.Remove(this);
        }
    }

    //private void Update()
    //{
    //    if (!IsOwner) return;

    //    if (Input.GetKeyDown(KeyCode.A))
    //    {
    //        SetUserName();
    //    }
    //}

    [ServerRpc]
    private void SetUserName()
    {
        string newName = $"Player{Random.Range(0, 100)}";
        userName.Value = newName;
    }

    [ServerRpc]
    public void SubmitChoice(RPSChoice choice)
    {
        currentChoice.Value = choice;
        GameManager.instance.OnPlayerChose();
    }
}
