using I2.Loc;

namespace Core.Localization
{
    public static class PickaxeTypeLocalization
    {
        private const string TermPrefix = "Pickaxes/";

        public static string GetLocalizedName(PickaxeType type)
        {
            return LocalizationManager.GetTranslation(TermPrefix + type);
        }
    }
}
