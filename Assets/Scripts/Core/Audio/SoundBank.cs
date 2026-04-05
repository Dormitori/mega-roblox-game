using System.Collections.Generic;
using UnityEngine;

namespace Core.Audio
{
    [CreateAssetMenu(fileName = "SoundBank", menuName = "Audio/SoundBank")]
    public class SoundBank : ScriptableObject
    {
        [System.Serializable]
        public class Entry
        {
            public SoundId id;
            public List<AudioClip> clips = new ();
        }

        [SerializeField] private List<Entry> sounds = new ();
        private Dictionary<SoundId, List<AudioClip>> _map;

        public void Init()
        {
            _map = new Dictionary<SoundId, List<AudioClip>>();
            foreach (var entry in sounds)
            {
                if (!_map.ContainsKey(entry.id))
                    _map[entry.id] = entry.clips;
            }
        }
        
        public AudioClip GetRandomClip(SoundId id)
        {
            if (_map == null) Init();

            if (_map.TryGetValue(id, out var list) && list.Count > 0)
            {
                int index = Random.Range(0, list.Count);
                return list[index];
            }

            return null;
        }
        
        public AudioClip GetFirstClip(SoundId id)
        {
            if (_map == null) Init();

            if (_map.TryGetValue(id, out var list) && list.Count > 0)
                return list[0];

            return null;
        }
    }
    
}