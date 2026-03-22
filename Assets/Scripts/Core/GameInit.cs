using VContainer.Unity;

public class GameInit : IStartable
{
    private readonly MineManager _mineManager;

    public GameInit(MineManager mineManager)
    {
        _mineManager = mineManager;
    }

    public void Start()
    {
        _mineManager.Initialize();
    }
}