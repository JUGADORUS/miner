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

    private void Awake()
    {
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

        _gameUI.SetActive(false);
        _panel.SetActive(true);
    }

    public void GoToBase() => SceneManager.LoadScene("Base");
    public void PlayAgain() => SceneManager.LoadScene("Mine");
}