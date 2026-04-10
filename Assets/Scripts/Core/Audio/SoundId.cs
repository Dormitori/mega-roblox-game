namespace Core.Audio
{
    public enum SoundId
    {
        ButtonClick,
        /// <summary>Удар киркой (каждый свинг; клип в SoundBank).</summary>
        BlockHit,
        /// <summary>Шаги: в SoundBank несколько клипов — каждый раз случайный.</summary>
        Footstep,
        /// <summary>Разрушение блока (клип в SoundBank).</summary>
        BlockDestroy,
        MenuMusic,
        GameMusic,
    }
}
