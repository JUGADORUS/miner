using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private GameObject _gameUI;
    [SerializeField] private TextMeshProUGUI _rewardPerSessionText;
    [SerializeField] private TextMeshProUGUI _totalMoneyText;
    [SerializeField] private TextMeshProUGUI _blocksDestroyedText;

    private MoneyPickupFeedback _moneyPickupFeedback;

    private void Awake()
    {
        _moneyPickupFeedback = _gameUI.GetComponentInChildren<MoneyPickupFeedback>();
        GameEvents.OnSessionEnded += HandleSessionEnded;
    }

    private void OnDestroy()
    {
        GameEvents.OnSessionEnded -= HandleSessionEnded;
    }

    private void HandleSessionEnded(int reward)
    {
        _rewardPerSessionText.text = $"{reward}";
        _totalMoneyText.text = $"{PlayerPrefs.GetInt("PlayerMoney", 0)}";

        if (GameManager.Instance != null)
        {
            _blocksDestroyedText.text = $"{GameManager.Instance.BlocksDestroyed}";
        }

        if (_moneyPickupFeedback != null && _moneyPickupFeedback.HasActiveFeedback)
        {
            StartCoroutine(ShowPanelAfterCoinFeedback());
            return;
        }

        ShowPanel();
    }

    private IEnumerator ShowPanelAfterCoinFeedback()
    {
        while (_moneyPickupFeedback.HasActiveFeedback) yield return null;
        ShowPanel();
    }

    private void ShowPanel()
    {
        _gameUI.SetActive(false);
        _panel.SetActive(true);
    }

    public void GoToBase() => SceneManager.LoadScene("BaseWorkshop");
    public void PlayAgain() => SceneManager.LoadScene("Mine");
}
