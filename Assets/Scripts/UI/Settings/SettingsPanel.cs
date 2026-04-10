using Core.Audio;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class SettingsPanel : PopUpWindow
{
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    private IAudioService _audioService;
    private bool _syncingSliders;

    [Inject]
    public void Initialize(IAudioService audioService)
    {
        _audioService = audioService;
    }

    private void Start()
    {
        if (_audioService == null || musicVolumeSlider == null || sfxVolumeSlider == null)
            return;

        musicVolumeSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        ApplyVolumesToSliders();
    }

    public override void OnWindowShow()
    {
        base.OnWindowShow();
        ApplyVolumesToSliders();
    }

    private void ApplyVolumesToSliders()
    {
        if (_audioService == null || musicVolumeSlider == null || sfxVolumeSlider == null)
            return;

        _syncingSliders = true;
        musicVolumeSlider.SetValueWithoutNotify(_audioService.GetMusicVolume());
        sfxVolumeSlider.SetValueWithoutNotify(_audioService.GetSfxVolume());
        _syncingSliders = false;
    }

    private void OnMusicSliderChanged(float value)
    {
        if (_syncingSliders)
            return;
        _audioService?.SetMusicVolume(value);
    }

    private void OnSfxSliderChanged(float value)
    {
        if (_syncingSliders)
            return;
        _audioService?.SetSfxVolume(value);
    }
}
