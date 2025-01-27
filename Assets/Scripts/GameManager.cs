using FishNet.CodeGenerating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using UnityEngine;

public enum RPSChoice { None, Rock, Paper, Scissors }

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
        if (!DidStart.Value && CanStart.Value)
        {
            StartGame();
        }
    }

    [Server]
    public void StartGame()
    {
        DidStart.Value = true;
        Finished.Value = false;
        RpcShowGame();
        StartNewRound();
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
            StartCoroutine(CompareChoices(p1Choice, p2Choice));
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
        yield return StartCoroutine(CompareChoices(_players[0].currentChoice.Value, _players[1].currentChoice.Value));
    }

    [Server]
    public IEnumerator CompareChoices(RPSChoice choice1, RPSChoice choice2)
    {
        LockChoiceButtons();
        RpcShowChosenHands(choice1, choice2);
        yield return new WaitForSecondsRealtime(1f);
        var p1 = _players[0];
        var p2 = _players[1];

        if (choice1 == choice2)
        {

            TargetUpdateResults(_players[0].Owner, "Tie!", false);
            TargetUpdateResults(_players[1].Owner, "Tie!", false);
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
                TargetUpdateResults(_players[0].Owner, $"You won with {choice1}!", true);
                TargetUpdateResults(_players[1].Owner, $"You lost with {choice2}!", false);
            }
            else
            {
                _players[1].score.Value++;
                TargetUpdateResults(_players[1].Owner, $"You won with {choice2}!", true);
                TargetUpdateResults(_players[0].Owner, $"You lost with {choice1}!", false);
            }
        }
        roundsPlayed.Value++;
        string scoreboard = $"{p1.userName.Value}: {p1.score.Value}  -  {p2.userName.Value}: {p2.score.Value}";

        yield return new WaitForSecondsRealtime(1f);
        RpcHideHands(scoreboard);
        if (!CheckVictory())
        {
            if (DidStart.Value && !Finished.Value)
                StartNewRound();
        }
    }

    [Server]
    private bool CheckVictory()
    {
        var p1 = _players[0];
        var p2 = _players[1];
        if (p1.score.Value >= 5 || p2.score.Value >= 5)
        {
            string scoreboard = $"{p1.userName.Value}: {p1.score.Value}  -  {p2.userName.Value}: {p2.score.Value}";
            if (p1.score.Value > p2.score.Value)
            {
                TargetShowVictory(p1.Owner, $"You Win!\n{scoreboard}");
                TargetShowVictory(p2.Owner, $"You Lose!\n{scoreboard}");
            }
            else
            {
                TargetShowVictory(p2.Owner, $"You Win!\n{scoreboard}");
                TargetShowVictory(p1.Owner, $"You Lose!\n{scoreboard}");
            }
            StopGameNow("Game Over!");
            return true;
        }
        return false;
    }

    [Server]
    private void StopGameNow(string finalMessage)
    {
        Finished.Value = true;
        if (_timerCoroutine != null)
            StopCoroutine(_timerCoroutine);
        RpcGameOverUI(finalMessage);
    }

    [ObserversRpc]
    private void RpcShowGame()
    {
        ViewManager.Instance.ShowGame();
    }

    [ObserversRpc]
    private void RpcHideHands(string res)
    {
        UIManager.Instance.HideHands(res);
    }

    [ObserversRpc]
    private void RpcShowChosenHands(RPSChoice p1Choice, RPSChoice p2Choice)
    {
        UIManager.Instance.ShowHands(p1Choice, p2Choice);
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
    private void UnlockChoiceButtons()
    {
        UIManager.Instance.EnableChoiceButtons();
    }

    [ObserversRpc]
    private void RpcGameOverUI(string message)
    {
        UIManager.Instance.DisableChoiceButtons();
        UIManager.Instance.UpdateRoundResult(message);
    }

    [TargetRpc]
    private void TargetUpdateResults(NetworkConnection conn, string resultText, bool isWinner)
    {
        UIManager.Instance.UpdateRoundResult(resultText, isWinner);

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

    private RPSChoice GetRandomChoice()
    {
        return (RPSChoice)Random.Range(1, 4);
    }
}
