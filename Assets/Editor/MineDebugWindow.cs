#if UNITY_EDITOR
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public class MineDebugWindow : EditorWindow
{
    private int _targetLevel;

    [MenuItem("Tools/Шахта/Дебаг уровня шахты")]
    public static void Open() => GetWindow<MineDebugWindow>("Mine Debug");

    private void OnGUI()
    {
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Уровень шахты (PlayerPrefs)", EditorStyles.boldLabel);

        var saved = LoadLevel();
        EditorGUILayout.LabelField($"Сохранённый уровень: {saved}");
        EditorGUILayout.Space(6);

        using (new EditorGUILayout.HorizontalScope())
        {
            _targetLevel = EditorGUILayout.IntField("Установить уровень", _targetLevel);

            if (GUILayout.Button("Применить", GUILayout.Width(90)))
            {
                SaveLevel(Mathf.Max(0, _targetLevel));
                Repaint();
            }

            if (GUILayout.Button("Сброс → 0", GUILayout.Width(80)))
            {
                SaveLevel(0);
                _targetLevel = 0;
                Repaint();
            }
        }

        EditorGUILayout.Space(4);
        EditorGUILayout.HelpBox("Изменения применяются до входа в Play Mode.", MessageType.Info);
    }

    private static int LoadLevel()
    {
        if (!PlayerPrefs.HasKey(SaveKeys.MineDeepLevel)) return 0;
        try { return JsonConvert.DeserializeObject<int>(PlayerPrefs.GetString(SaveKeys.MineDeepLevel)); }
        catch { return 0; }
    }

    private static void SaveLevel(int level)
    {
        PlayerPrefs.SetString(SaveKeys.MineDeepLevel, JsonConvert.SerializeObject(level));
        PlayerPrefs.Save();
    }
}
#endif
