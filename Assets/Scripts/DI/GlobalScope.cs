using Core.Audio;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DI
{
    public class GlobalScope : LifetimeScope
    {
        [SerializeField] private AudioService _audioService;
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<JsonPlayerPrefsSaveService>(Lifetime.Singleton).As<ISaveService>();
            builder.RegisterInstance(_audioService).As<IAudioService>();
        }
    }
}