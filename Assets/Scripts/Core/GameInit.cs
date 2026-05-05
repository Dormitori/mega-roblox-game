using VContainer.Unity;

public sealed class DevStartupSettings
{
    public bool GiveMinStartCoins { get; }
    public int MinStartCoins { get; }

    public DevStartupSettings(bool giveMinStartCoins, int minStartCoins)
    {
        GiveMinStartCoins = giveMinStartCoins;
        MinStartCoins = minStartCoins;
    }
}

public class GameInit : IStartable
{
    private readonly PetEquipService _petEquip;
    private readonly Inventory _inventory;
    private readonly DevStartupSettings _dev;

    public GameInit(PetEquipService petEquip, Inventory inventory, DevStartupSettings dev)
    {
        _petEquip = petEquip;
        _inventory = inventory;
        _dev = dev;
    }

    public void Start()
    {
        if (_dev is { GiveMinStartCoins: true } && _dev.MinStartCoins > 0)
        {
            var cur = _inventory.GetCurrencyCount(CurrencyType.Coins);
            if (cur < _dev.MinStartCoins)
                _inventory.AddCurrency(CurrencyType.Coins, _dev.MinStartCoins - cur);
        }

        _petEquip?.ApplyEquippedToWorld();
    }
}