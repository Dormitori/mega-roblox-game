using Core.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class BackpackShop : PopUpWindow
{
    public BackpackShopConfig backpackShopConfig;
    public Button upgradeButton;
    public TextMeshProUGUI upgradePriceText;
    public TextMeshProUGUI currentCapacityText;
    public TextMeshProUGUI nextCapacityText;

    private Inventory _inventory;
    private IAudioService _audioService;

    private int _currentLevel;

    private int NextLevel => _currentLevel + 1;

    [Inject]
    private void Initialize(Inventory inventory, IAudioService audioService)
    {
        _inventory = inventory;
        _audioService = audioService;
        var upgradeStep = GetStep(_currentLevel);
        while (upgradeStep.capacity < _inventory.GetBackpackCapacity())
        {
            _currentLevel++;
            upgradeStep = GetStep(_currentLevel);
        }
    }

    private void Start()
    {
        upgradeButton.onClick.AddListener(OnUpgrade);
    }

    public override void OnWindowShow()
    {
        base.OnWindowShow();
        Refresh();
    }

    private void OnUpgrade()
    {
        var upgradeStep = GetStep(NextLevel);
        if (!_inventory.TryRemoveCurrency(CurrencyType.Coins, upgradeStep.price))
            return;
        _currentLevel++;
        _inventory.SetBackpackCapacity(upgradeStep.capacity);
        _audioService?.PlaySfx(SoundId.BuySell);
        Refresh();
    }

    private BackpackUpgradeStep GetStep(int level)
    {
        if (level < backpackShopConfig.upgradeSteps.Count)
            return backpackShopConfig.upgradeSteps[level];

        var lastStep = backpackShopConfig.upgradeSteps[^1];

        var stepsOver = level - (backpackShopConfig.upgradeSteps.Count - 1);

        var generatedCapacity = lastStep.capacity + 10 * stepsOver;
        var generatedPrice = (int)(lastStep.price * Mathf.Pow(2, stepsOver));

        return new BackpackUpgradeStep
        {
            capacity = generatedCapacity,
            price = generatedPrice
        };
    }

    private void Refresh()
    {
        var currentStep = GetStep(_currentLevel);
        var nextStep = GetStep(NextLevel);

        upgradeButton.interactable = _inventory.GetCurrencyCount(CurrencyType.Coins) >= nextStep.price;

        upgradePriceText.text = nextStep.price.ToString();
        currentCapacityText.text = currentStep.capacity.ToString();
        nextCapacityText.text = nextStep.capacity.ToString();
    }
}