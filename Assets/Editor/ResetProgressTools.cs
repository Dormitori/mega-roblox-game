#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Инструменты разработчика: сброс сейва в PlayerPrefs.
/// </summary>
public static class ResetProgressTools
{
    private const string MenuRoot = "Tools/Прогресс/";

    [MenuItem(MenuRoot + "Сброс инвентаря и глубины шахты", false, 1)]
    public static void ClearGameSaveOnly()
    {
        if (!EditorUtility.DisplayDialog(
                "Сброс прогресса",
                "Удалить ключи: inventory, mine_deep_level?\nНастройки громкости не трогаются.\n\nПерезапусти Play или сцену, чтобы подтянуть чистое состояние.",
                "Да",
                "Отмена"))
            return;

        PlayerPrefs.DeleteKey(SaveKeys.Inventory);
        PlayerPrefs.DeleteKey(SaveKeys.MineDeepLevel);
        PlayerPrefs.Save();
        Debug.Log("[ResetProgress] Удалены SaveKeys.Inventory и SaveKeys.MineDeepLevel.");
    }

    [MenuItem(MenuRoot + "Очистить ВСЕ PlayerPrefs", false, 11)]
    public static void ClearAllPlayerPrefs()
    {
        if (!EditorUtility.DisplayDialog(
                "Очистка PlayerPrefs",
                "Будут удалены ВСЕ данные PlayerPrefs (включая громкость, инвентарь, шахту и ключи других систем/плагинов).\n\nПродолжить?",
                "Удалить всё",
                "Отмена"))
            return;

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[ResetProgress] PlayerPrefs.DeleteAll() выполнен.");
    }
}
#endif
