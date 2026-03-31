using VContainer;
using VContainer.Unity;

namespace DI
{
    public class GameScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<Inventory>(Lifetime.Singleton).As<IInventory>();
            builder.RegisterComponentInHierarchy<MineManager>();
            builder.RegisterComponentInHierarchy<MoneyView>();
            builder.RegisterComponentInHierarchy<SellShop>();
            builder.RegisterComponentInHierarchy<PickaxeShop>();
            builder.RegisterEntryPoint<GameInit>();
        }
    }
}