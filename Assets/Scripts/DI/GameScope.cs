using VContainer;
using VContainer.Unity;

namespace DI
{
    public class GameScope : LifetimeScope
    {
        public MineManagerRefs mineRefs;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(mineRefs);
            builder.RegisterEntryPoint<MineManager>();
        }
    }
}