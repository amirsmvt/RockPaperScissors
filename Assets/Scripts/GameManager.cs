using FishNet.CodeGenerating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    [field: AllowMutableSyncTypeAttribute]
    public SyncList<Player> _players = new SyncList<Player>();

    [field: AllowMutableSyncTypeAttribute]
    public SyncVar<bool> CanStart = new SyncVar<bool>();

    [field: AllowMutableSyncTypeAttribute]
    public SyncVar<bool> DidStart = new SyncVar<bool>();

    [field: AllowMutableSyncTypeAttribute]
    public SyncVar<bool> Finished = new SyncVar<bool>();

    [field: AllowMutableSyncTypeAttribute]
    public SyncVar<int> roundsPlayed = new SyncVar<int>();

    [field: AllowMutableSyncTypeAttribute]
    public SyncVar<float> currentRoundTime = new SyncVar<float>(15f); 

    private Coroutine _timerCoroutine;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {

        if (!IsServerInitialized) return;
        CanStart.Value = (_players.Count == 2) && !DidStart.Value;
        if (!DidStart.Value && CanStart.Value )
        {
            StartGame();
        }
    }

    [Server]
    public void StartGame()
    {
        DidStart.Value = true;
        RpcShowGame();
        StartNewRound();
    }

    [Server]
    private void StopGameNow(string finalMessage)
    {
        Finished.Value = true;

        if (_timerCoroutine != null)
            StopCoroutine(_timerCoroutine);
        RpcGameOverUI(finalMessage);
    }



    [Server]
    public void StartNewRound()
    {
        foreach (Player p in _players)
            p.currentChoice.Value = RPSChoice.None;

        UnlockChoiceButtons();

        if (_timerCoroutine != null)
            StopCoroutine(_timerCoroutine);
        _timerCoroutine = StartCoroutine(RoundCountdown());
    }

    [Server]
    public void OnPlayerChose()
    {
        if (_players.Count < 2) return;

        var p1Choice = _players[0].currentChoice.Value;
        var p2Choice = _players[1].currentChoice.Value;
        if (p1Choice != RPSChoice.None && p2Choice != RPSChoice.None)
        {
            if (_timerCoroutine != null)
                StopCoroutine(_timerCoroutine);

            CompareChoices(p1Choice, p2Choice);
        }
    }

    private IEnumerator RoundCountdown()
    {
        currentRoundTime.Value = 15f;
        while (currentRoundTime.Value > 0f)
        {
            currentRoundTime.Value -= Time.deltaTime;
            UpdateTimerUI(currentRoundTime.Value);
            yield return null;
        }
        foreach (Player p in _players)
        {
            if (p.currentChoice.Value == RPSChoice.None)
                p.currentChoice.Value = GetRandomChoice();
        }
        CompareChoices(_players[0].currentChoice.Value, _players[1].currentChoice.Value);
    }

    [Server]
    public void CompareChoices(RPSChoice choice1, RPSChoice choice2)
    {
        LockChoiceButtons();

        if (choice1 == choice2)
        {
            TargetUpdateResults(_players[0].Owner, "Tie!");
            TargetUpdateResults(_players[1].Owner, "Tie!");
        }
        else
        {
            bool player1Wins =
                (choice1 == RPSChoice.Rock && choice2 == RPSChoice.Scissors) ||
                (choice1 == RPSChoice.Scissors && choice2 == RPSChoice.Paper) ||
                (choice1 == RPSChoice.Paper && choice2 == RPSChoice.Rock);

            if (player1Wins)
            {
                _players[0].score.Value++;
                TargetUpdateResults(_players[0].Owner, $"You won with {choice1}!");
                TargetUpdateResults(_players[1].Owner, $"You lost with {choice2}!");
            }
            else
            {
                _players[1].score.Value++;
                TargetUpdateResults(_players[1].Owner, $"You won with {choice2}!");
                TargetUpdateResults(_players[0].Owner, $"You lost with {choice1}!");
            }
        }

        roundsPlayed.Value++;
        CheckVictory();
        if (DidStart.Value && !Finished.Value)
            StartNewRound();
    }

    [Server]
    private void CheckVictory()
    {
        foreach (Player player in _players)
        {
            if (player.score.Value >= 5)
            {
                TargetShowVictory(player.Owner, "You Win!");
                var other = _players.Find(p => p != player);
                if (other != null)
                    TargetShowVictory(other.Owner, "You Lose!");


                break;
            }
        }
    }

    #region RPC METHODS

    [ObserversRpc]
    private void RpcShowGame()
    {
        ViewManager.Instance.ShowGame();
    }

    [ObserversRpc]
    private void UpdateTimerUI(float time)
    {
        UIManager.Instance.UpdateTimerDisplay(time);
    }

    [ObserversRpc]
    private void LockChoiceButtons()
    {
        UIManager.Instance.DisableChoiceButtons();
    }
    [ObserversRpc]
    private void RpcGameOverUI(string message)
    {
        UIManager.Instance.DisableChoiceButtons();
        UIManager.Instance.UpdateRoundResult(message);
    }
    [ObserversRpc]
    private void UnlockChoiceButtons()
    {
        UIManager.Instance.EnableChoiceButtons();
    }

    [TargetRpc]
    private void TargetUpdateResults(NetworkConnection conn, string resultText)
    {
        UIManager.Instance.UpdateRoundResult(resultText);
    }

    [TargetRpc]
    private void TargetShowVictory(NetworkConnection conn, string message)
    {
        UIManager.Instance.ShowVictoryScreen(message);
    }

    #endregion

    private RPSChoice GetRandomChoice()
    {
        return (RPSChoice)Random.Range(1, 4);
    }
}

public enum RPSChoice { None, Rock, Paper, Scissors }
