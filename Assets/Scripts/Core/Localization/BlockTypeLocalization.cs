using I2.Loc;

namespace Core.Localization
{
    public static class BlockTypeLocalization
    {
        private const string TermPrefix = "Blocks/";

        public static string GetLocalizedName(BlockType type)
        {
            return LocalizationManager.GetTranslation(TermPrefix + type);
        }
    }
}
