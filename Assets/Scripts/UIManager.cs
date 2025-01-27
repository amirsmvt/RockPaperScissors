using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : NetworkBehaviour
{
    public static UIManager Instance;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text roundResultText;
    [SerializeField] private TMP_Text victoryText;

    [SerializeField] private Button rockButton;
    [SerializeField] private Button paperButton;
    [SerializeField] private Button scissorsButton;

    [SerializeField] private GameObject showHands;

    [SerializeField] private GameObject downRock;
    [SerializeField] private GameObject downPaper;
    [SerializeField] private GameObject downScissors;

    [SerializeField] private GameObject upRock;
    [SerializeField] private GameObject upPaper;
    [SerializeField] private GameObject upScissors;

    [SerializeField] private GameObject victoryScreen;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdateTimerDisplay(float time)
    {
        timerText.text = $"{Mathf.CeilToInt(time)}";
    }

    public void UpdateRoundResult(string result, bool isWinner)
    {
        roundResultText.text = result;
        roundResultText.color = isWinner ? Color.red : Color.white;
    }

    public void UpdateRoundResult(string result)
    {
        UpdateRoundResult(result, false);
    }

    public void ShowVictoryScreen(string message)
    {
        victoryScreen.SetActive(true);
    }

    public void DisableChoiceButtons()
    {
        rockButton.interactable = false;
        paperButton.interactable = false;
        scissorsButton.interactable = false;
    }

    public void EnableChoiceButtons()
    {
        rockButton.interactable = true;
        paperButton.interactable = true;
        scissorsButton.interactable = true;
        roundResultText.text = "";
        roundResultText.color = Color.white;
    }

    public void ShowHands(RPSChoice p1Choice, RPSChoice p2Choice)
    {
        showHands.SetActive(true);
        downRock.SetActive(false);
        downPaper.SetActive(false);
        downScissors.SetActive(false);
        upRock.SetActive(false);
        upPaper.SetActive(false);
        upScissors.SetActive(false);

        switch (p1Choice)
        {
            case RPSChoice.Rock: downRock.SetActive(true); break;
            case RPSChoice.Paper: downPaper.SetActive(true); break;
            case RPSChoice.Scissors: downScissors.SetActive(true); break;
        }
        switch (p2Choice)
        {
            case RPSChoice.Rock: upRock.SetActive(true); break;
            case RPSChoice.Paper: upPaper.SetActive(true); break;
            case RPSChoice.Scissors: upScissors.SetActive(true); break;
        }
    }

    public void HideHands(string res)
    {
        victoryText.text = res;

        showHands.SetActive(false);
    }

    public void OnChoiceSelected(int choice)
    {
        DisableChoiceButtons();
        if (Player.instance != null)
        {
            RPSChoice rpsChoice = (RPSChoice)choice;
            Player.instance.SubmitChoice(rpsChoice);
        }
    }
}
