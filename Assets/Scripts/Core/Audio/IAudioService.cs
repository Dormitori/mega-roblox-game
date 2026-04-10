using System.Collections;
using UnityEngine;

namespace Core.Audio
{
    public interface IAudioService
    {
        void SetFollowTarget(Transform target);
        void SetMasterVolume(float linear);
        float GetMasterVolume();
        public void MuteAll(bool muted);
        void StopMusic(float fadeDuration = 0.3f);
        void SetMusicVolume(float linear);

        void PlayMusic(SoundId id, bool loop = true, float fadeDuration = 0.3f);

        public void PlayMusic(AudioClip clip, bool loop = true, float fadeDuration = 0.3f);
        float GetMusicVolume();
        void PlaySfx(SoundId id, float volume = 1f, float pitchJitterHalfRange = 0f);
        void PlaySfx(SoundId id, float volume, float pitchMin, float pitchMax);
        void PlaySfx(AudioClip clip, float volume = 1f, float pitchJitterHalfRange = 0f);
        void PlaySfx(AudioClip clip, float volume, float pitchMin, float pitchMax);
        void PlaySfxAtPosition(SoundId id, Vector3 pos, float volume = 1f);
        void SetSfxVolume(float linear);
        float GetSfxVolume();
        float GetVolume(string param, float fallback);
        IEnumerator FadeMixerParam(string param, float target, float duration, bool stopMusic = false);
    }
}