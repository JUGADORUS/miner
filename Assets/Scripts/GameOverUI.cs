using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YG;

public class GameOverUI : MonoBehaviour
{
    private const string ReviveOffer = "Restore {0}% durability · Watch ad";
    private const string AdLoading = "Loading video…";
    private const string AdUnavailable = "Video unavailable · Try again";
    private const float AdOpenTimeout = 15f;

    [SerializeField] private GameObject _panel;
    [SerializeField] private GameObject _gameUI;
    [SerializeField] private TextMeshProUGUI _rewardPerSessionText;
    [SerializeField] private TextMeshProUGUI _totalMoneyText;
    [SerializeField] private TextMeshProUGUI _blocksDestroyedText;
    [SerializeField] private Button _reviveButton;
    [SerializeField] private TextMeshProUGUI _reviveLabel;

    private MoneyPickupFeedback _moneyPickupFeedback;
    private string _pendingAdId;
    private int _adAttempt;
    private bool _adOpened;
    private bool _adRewarded;
    private bool _adFailed;

    private void Awake()
    {
        _moneyPickupFeedback = _gameUI.GetComponentInChildren<MoneyPickupFeedback>();
        GameEvents.OnSessionEnded += HandleSessionEnded;
        MinerLocalization.Changed += RefreshReviveOffer;
        YG2.onGetSDKData += RefreshReviveOffer;
        YG2.onOpenRewardedAdv += HandleAdOpened;
        YG2.onRewardAdv += HandleAdRewarded;
        YG2.onCloseRewardedAdv += HandleAdClosed;
        YG2.onErrorRewardedAdv += HandleAdFailed;
    }

    private void OnDestroy()
    {
        GameEvents.OnSessionEnded -= HandleSessionEnded;
        MinerLocalization.Changed -= RefreshReviveOffer;
        YG2.onGetSDKData -= RefreshReviveOffer;
        YG2.onOpenRewardedAdv -= HandleAdOpened;
        YG2.onRewardAdv -= HandleAdRewarded;
        YG2.onCloseRewardedAdv -= HandleAdClosed;
        YG2.onErrorRewardedAdv -= HandleAdFailed;
    }

    private void HandleSessionEnded(int reward)
    {
        _adFailed = false;
        _rewardPerSessionText.text = $"{reward}";
        _totalMoneyText.text = $"{MinerProgress.GetInt("PlayerMoney", 0)}";

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
        RefreshReviveOffer();
        _panel.SetActive(true);
    }

    private void RefreshReviveOffer()
    {
        if (_reviveButton == null || _reviveLabel == null) return;

        bool available = YG2.isSDKEnabled && GameManager.Instance != null && GameManager.Instance.CanRevive;
        _reviveButton.gameObject.SetActive(available);
        _reviveButton.interactable = _pendingAdId == null;
        _reviveLabel.text = _pendingAdId != null ? MinerLocalization.Text(AdLoading)
            : _adFailed ? MinerLocalization.Text(AdUnavailable)
            : MinerLocalization.Format(ReviveOffer, GameManager.ReviveDurabilityPercent);
    }

    public void ReviveWithAd()
    {
        if (_pendingAdId != null || !_panel.activeInHierarchy || !YG2.isSDKEnabled ||
            YG2.nowAdsShow || GameManager.Instance == null || !GameManager.Instance.CanRevive)
            return;

        string adId = $"mine_revive_{GetInstanceID()}_{++_adAttempt}";
        _pendingAdId = adId;
        _adOpened = false;
        _adRewarded = false;
        _adFailed = false;
        RefreshReviveOffer();
        StartCoroutine(WaitForAdToOpen(adId));
        YG2.RewardedAdvShow(adId);
    }

    private IEnumerator WaitForAdToOpen(string adId)
    {
        float deadline = Time.realtimeSinceStartup + AdOpenTimeout;
        while (_pendingAdId == adId && !_adOpened && Time.realtimeSinceStartup < deadline)
            yield return null;

        if (_pendingAdId == adId && !_adOpened)
            HandleAdFailed();
    }

    private void HandleAdOpened()
    {
        if (_pendingAdId != null)
            _adOpened = true;
    }

    private void HandleAdRewarded(string adId)
    {
        if (adId == _pendingAdId)
            _adRewarded = true;
    }

    private void HandleAdClosed()
    {
        if (_pendingAdId == null)
            return;

        _pendingAdId = null;
        if (_adRewarded && GameManager.Instance != null && GameManager.Instance.TryRevive())
        {
            _panel.SetActive(false);
            _gameUI.SetActive(true);
        }

        RefreshReviveOffer();
    }

    private void HandleAdFailed()
    {
        if (_pendingAdId == null)
            return;

        _adFailed = true;
        _pendingAdId = null;
        RefreshReviveOffer();
    }

    public void GoToBase()
    {
        if (_pendingAdId == null && !YG2.nowAdsShow)
            SceneManager.LoadScene("BaseWorkshop");
    }

    public void PlayAgain()
    {
        if (_pendingAdId == null && !YG2.nowAdsShow)
            SceneManager.LoadScene("Mine");
    }
}
