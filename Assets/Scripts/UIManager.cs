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

    [SerializeField] private GameObject victoryScreen;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void UpdateTimerDisplay(float time)
    {
        timerText.text = $"Time: {Mathf.CeilToInt(time)}";
    }

    public void UpdateRoundResult(string result)
    {
        Debug.Log($"UIManager => UpdateRoundResult => Setting roundResultText to: {result}");
        roundResultText.text = result;
    }


    public void ShowVictoryScreen(string message)
    {
        victoryScreen.SetActive(true);
        victoryText.text = message;
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
    }

    public void OnChoiceSelected(int choice)
    {
        DisableChoiceButtons();

        if (Player.instance != null)
        {
            RPSChoice rpsChoice = (RPSChoice)choice;
            Debug.Log($"UIManager => OnChoiceSelected => {rpsChoice}");
            Player.instance.SubmitChoice(rpsChoice);
        }
        else
        {
            Debug.LogWarning("UIManager => OnChoiceSelected => No local Player.instance found!");
        }
    }
}
