using VContainer.Unity;

public class GameInit : IStartable
{
    private readonly PetEquipService _petEquip;

    public GameInit(PetEquipService petEquip)
    {
        _petEquip = petEquip;
    }

    public void Start()
    {
        _petEquip?.ApplyEquippedToWorld();
    }
}