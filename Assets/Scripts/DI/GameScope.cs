using VContainer;
using VContainer.Unity;

namespace DI
{
    public class GameScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<Inventory>(Lifetime.Singleton);
            
            builder.Register<ConfigManager<BlockConfig>>(Lifetime.Singleton);
            builder.Register<ConfigManager<PickaxeConfig>>(Lifetime.Singleton);
            
            builder.RegisterComponentInHierarchy<MineManager>();
            builder.RegisterComponentInHierarchy<PlayerBlockInventory>();
            builder.RegisterComponentInHierarchy<MoneyView>();
            builder.RegisterComponentInHierarchy<SellShop>();
            builder.RegisterComponentInHierarchy<PickaxeShop>();
            builder.RegisterEntryPoint<GameInit>();
        }
    }
}