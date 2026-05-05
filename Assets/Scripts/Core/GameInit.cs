using VContainer.Unity;

public class GameInit : IStartable
{
    private readonly PetEquipService _petEquip;
    private readonly Inventory _inventory;

    public GameInit(PetEquipService petEquip, Inventory inventory)
    {
        _petEquip = petEquip;
        _inventory = inventory;
    }

    public void Start()
    {
        // Dev: временно гарантируем стартовые монеты для удобства настройки/тестов.
        const int minCoins = 5000;
        var cur = _inventory.GetCurrencyCount(CurrencyType.Coins);
        if (cur < minCoins)
            _inventory.AddCurrency(CurrencyType.Coins, minCoins - cur);

        _petEquip?.ApplyEquippedToWorld();
    }
}