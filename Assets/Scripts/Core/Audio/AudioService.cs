using System.Collections;
using Core.Common;
using UnityEngine;
using UnityEngine.Audio;

namespace Core.Audio
{
    public class AudioService : MonoBehaviour, IAudioService
    {
        [SerializeField] private AudioMixer audioMixer;

        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [SerializeField] private bool playMenuMusicOnStart = true;
        [SerializeField] private float startupMenuMusicFadeDuration = 1f;

        [SerializeField, Range(0f, 1f)] private float defaultMaster = 1f;

        [SerializeField, Range(0f, 1f)] private float defaultMusic = 1f;
        [SerializeField, Range(0f, 1f)] private float defaultSfx = 1f;

        [SerializeField] private SoundBank soundBank;

        private Coroutine _musicFadeCoroutine;

        private const string MasterParam = StringData.MasterParam;
        private const string MusicParam = StringData.MusicParam;
        private const string SfxParam = StringData.SfxParam;

        private float _musicLocal;
        private float _sfxLocal;

        private bool _suppressVolumeSave;

        private Transform _followTransform;

        private void Update()
        {
            if (!_followTransform)
                return;
            transform.position = _followTransform.position;
        }

        private void Awake()
        {
            _musicLocal = defaultMusic;
            _sfxLocal = defaultSfx;
        }

        private void Start()
        {
            _suppressVolumeSave = true;
            if (PlayerPrefs.HasKey(StringData.MusicKey))
                _musicLocal = Mathf.Clamp01(PlayerPrefs.GetFloat(StringData.MusicKey));
            if (PlayerPrefs.HasKey(StringData.SfxKey))
                _sfxLocal = Mathf.Clamp01(PlayerPrefs.GetFloat(StringData.SfxKey));

            SetMasterVolume(defaultMaster);
            SetMusicVolume(_musicLocal);
            SetSfxVolume(_sfxLocal);
            _suppressVolumeSave = false;

            if (playMenuMusicOnStart)
            {
                StopMusic(0f);
                PlayMusic(SoundId.MenuMusic, true, startupMenuMusicFadeDuration);
            }
        }

        public void SetFollowTarget(Transform target)
        {
            _followTransform = target;
        }

        #region Master

        public void SetMasterVolume(float linear)
        {
            linear = Mathf.Clamp01(linear);
            audioMixer?.SetFloat(MasterParam, LinearToDecibel(linear));
        }

        public float GetMasterVolume() => GetVolume(MasterParam, defaultMaster);

        #endregion

        #region Music

        public void PlayMusic(SoundId id, bool loop = true, float fadeDuration = 0.3f)
        {
            var clip = soundBank?.GetFirstClip(id);
            if (clip != null) PlayMusic(clip, loop, fadeDuration);
        }

        public void PlayMusic(AudioClip clip, bool loop = true, float fadeDuration = 0.3f)
        {
            if (musicSource == null || clip == null) return;

            if (musicSource.isPlaying)
                musicSource.Stop();

            if (_musicFadeCoroutine != null)
                StopCoroutine(_musicFadeCoroutine);

            musicSource.loop = loop;
            musicSource.clip = clip;
            musicSource.Play();

            _musicFadeCoroutine = StartCoroutine(
                FadeMixerParam(MusicParam, _musicLocal, fadeDuration));
        }

        public void StopMusic(float fadeDuration = 0.3f)
        {
            if (musicSource == null) return;

            if (_musicFadeCoroutine != null)
                StopCoroutine(_musicFadeCoroutine);

            _musicFadeCoroutine = StartCoroutine(
                FadeMixerParam(MusicParam, 0f, fadeDuration, stopMusic: true));
        }

        public void SetMusicVolume(float linear)
        {
            _musicLocal = Mathf.Clamp01(linear);
            audioMixer?.SetFloat(MusicParam, LinearToDecibel(_musicLocal));
            if (!_suppressVolumeSave)
            {
                PlayerPrefs.SetFloat(StringData.MusicKey, _musicLocal);
                PlayerPrefs.Save();
            }
        }

        public float GetMusicVolume() => _musicLocal;

        #endregion

        #region SFX

        public void PlaySfx(SoundId id, float volume = 1f, float pitchJitterHalfRange = 0f)
        {
            var clip = soundBank?.GetRandomClip(id);
            if (clip != null) PlaySfx(clip, volume, pitchJitterHalfRange);
        }

        public void PlaySfx(SoundId id, float volume, float pitchMin, float pitchMax)
        {
            var clip = soundBank?.GetRandomClip(id);
            if (clip != null) PlaySfx(clip, volume, pitchMin, pitchMax);
        }

        public void PlaySfx(AudioClip clip, float volume = 1f, float pitchJitterHalfRange = 0f)
        {
            if (sfxSource == null || clip == null) return;
            var prevPitch = sfxSource.pitch;
            if (pitchJitterHalfRange > 0f)
                sfxSource.pitch = Random.Range(1f - pitchJitterHalfRange, 1f + pitchJitterHalfRange);
            else
                sfxSource.pitch = 1f;
            sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
            sfxSource.pitch = prevPitch;
        }

        public void PlaySfx(AudioClip clip, float volume, float pitchMin, float pitchMax)
        {
            if (sfxSource == null || clip == null) return;
            var prevPitch = sfxSource.pitch;
            sfxSource.pitch = pitchMax > pitchMin ? Random.Range(pitchMin, pitchMax) : 1f;
            sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
            sfxSource.pitch = prevPitch;
        }

        public void PlaySfxAtPosition(SoundId id, Vector3 pos, float volume = 1f)
        {
            var clip = soundBank?.GetRandomClip(id);
            if (clip != null)
                AudioSource.PlayClipAtPoint(clip, pos, Mathf.Clamp01(volume));
        }

        public void SetSfxVolume(float linear)
        {
            _sfxLocal = Mathf.Clamp01(linear);
            audioMixer?.SetFloat(SfxParam, LinearToDecibel(_sfxLocal));
            if (!_suppressVolumeSave)
            {
                PlayerPrefs.SetFloat(StringData.SfxKey, _sfxLocal);
                PlayerPrefs.Save();
            }
        }

        public float GetSfxVolume() => _sfxLocal;

        #endregion

        #region Helpers

        private static float LinearToDecibel(float linear)
            => linear <= 0.0001f ? -80f : Mathf.Log10(linear) * 20f;

        private static float DecibelToLinear(float dB)
            => Mathf.Pow(10f, dB / 20f);

        public float GetVolume(string param, float fallback)
        {
            if (audioMixer != null && audioMixer.GetFloat(param, out var dB))
                return DecibelToLinear(dB);
            return fallback;
        }

        public IEnumerator FadeMixerParam(string param, float target, float duration, bool stopMusic = false)
        {
            if (!audioMixer.GetFloat(param, out var startDb))
                yield break;

            float startLinear = DecibelToLinear(startDb);
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float value = Mathf.Lerp(startLinear, target, t / duration);
                audioMixer.SetFloat(param, LinearToDecibel(value));
                yield return null;
            }

            audioMixer.SetFloat(param, LinearToDecibel(target));

            if (stopMusic && musicSource.isPlaying)
                musicSource.Stop();
        }

        #endregion

        #region Mute / Unmute

        private bool _isMuted;
        private float _prevMaster;
        private float _prevMusic;
        private float _prevSfx;

        public void MuteAll(bool muted)
        {
            if (_isMuted == muted)
                return;

            _isMuted = muted;

            if (muted)
            {
                _prevMaster = GetMasterVolume();
                _prevMusic = GetMusicVolume();
                _prevSfx = GetSfxVolume();

                _suppressVolumeSave = true;
                try
                {
                    SetMasterVolume(0f);
                    SetMusicVolume(0f);
                    SetSfxVolume(0f);
                }
                finally
                {
                    _suppressVolumeSave = false;
                }
            }
            else
            {
                _suppressVolumeSave = true;
                try
                {
                    SetMasterVolume(_prevMaster);
                    SetMusicVolume(_prevMusic);
                    SetSfxVolume(_prevSfx);
                }
                finally
                {
                    _suppressVolumeSave = false;
                }
            }
        }

        public bool IsMuted => _isMuted;

        #endregion
    }
}
