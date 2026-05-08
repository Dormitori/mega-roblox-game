#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public class PetDebugWindow : EditorWindow
{
    private List<PetConfig> _pets = new();
    private Vector2 _scroll;
    private string _equippedId;
    private Dictionary<string, int> _ownedCounts = new();

    [MenuItem("Tools/Питомцы/Дебаг-окно питомцев")]
    public static void Open() => GetWindow<PetDebugWindow>("Pet Debug");

    private void OnEnable() => Reload();

    private void Reload()
    {
        _pets = AssetDatabase.FindAssets("t:PetConfig")
            .Select(guid => AssetDatabase.LoadAssetAtPath<PetConfig>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(c => c != null)
            .OrderBy(c => c.rarity)
            .ThenBy(c => c.id)
            .ToList();

        var data = LoadSaveData();
        _equippedId = data?.equippedPetId ?? string.Empty;
        _ownedCounts = data?.ownedPetCounts ?? new Dictionary<string, int>();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(4);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Обновить", GUILayout.Width(90)))
                Reload();

            if (GUILayout.Button("Добавить всех (×1)", GUILayout.Width(140)))
            {
                foreach (var pet in _pets)
                    ModifyOwned(pet.id, 1);
                Reload();
            }

            if (GUILayout.Button("Добавить всех (×10)", GUILayout.Width(150)))
            {
                foreach (var pet in _pets)
                    ModifyOwned(pet.id, 10);
                Reload();
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Сбросить питомцев", GUILayout.Width(130)))
            {
                if (EditorUtility.DisplayDialog("Сброс питомцев",
                        "Удалить все данные питомцев (pet_progress)?", "Да", "Отмена"))
                {
                    PlayerPrefs.DeleteKey("pet_progress");
                    PlayerPrefs.Save();
                    Reload();
                }
            }
        }

        EditorGUILayout.Space(6);

        if (_pets.Count == 0)
        {
            EditorGUILayout.HelpBox("PetConfig-ассеты не найдены в проекте.", MessageType.Warning);
            return;
        }

        DrawHeader();

        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        PetRarity? lastRarity = null;
        foreach (var pet in _pets)
        {
            if (pet.rarity != lastRarity)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField(pet.rarity.ToString(), EditorStyles.boldLabel);
                lastRarity = pet.rarity;
            }
            DrawPetRow(pet);
        }
        EditorGUILayout.EndScrollView();
    }

    private static void DrawHeader()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            GUILayout.Label("ID", GUILayout.Width(140));
            GUILayout.Label("Статат + бонус", GUILayout.Width(200));
            GUILayout.Label("Есть", GUILayout.Width(40));
            GUILayout.Label("Надет", GUILayout.Width(45));
            GUILayout.Label("", GUILayout.ExpandWidth(true));
        }
    }

    private void DrawPetRow(PetConfig pet)
    {
        var isEquipped = pet.id == _equippedId;
        var owned = _ownedCounts.TryGetValue(pet.id, out var c) ? c : 0;

        using var _ = new EditorGUILayout.HorizontalScope();

        // Icon
        if (pet.icon != null)
        {
            var tex = AssetPreview.GetAssetPreview(pet.icon);
            if (tex == null) tex = AssetPreview.GetMiniThumbnail(pet.icon);
            GUILayout.Label(tex != null ? new GUIContent(tex) : GUIContent.none, GUILayout.Width(20), GUILayout.Height(20));
        }
        else
        {
            GUILayout.Space(24);
        }

        GUILayout.Label(pet.id, GUILayout.Width(120));
        GUILayout.Label($"{pet.statType} +{pet.bonusPercent}%", GUILayout.Width(200));
        GUILayout.Label(owned.ToString(), GUILayout.Width(40));

        var style = isEquipped ? EditorStyles.boldLabel : EditorStyles.label;
        GUILayout.Label(isEquipped ? "✓" : "", style, GUILayout.Width(45));

        if (GUILayout.Button("+1", GUILayout.Width(32)))
        {
            ModifyOwned(pet.id, 1);
            Reload();
        }

        if (GUILayout.Button("+10", GUILayout.Width(36)))
        {
            ModifyOwned(pet.id, 10);
            Reload();
        }

        using (new EditorGUI.DisabledScope(owned == 0))
        {
            if (GUILayout.Button("Надеть", GUILayout.Width(60)))
            {
                EquipPet(pet.id);
                Reload();
            }
        }

        if (GUILayout.Button("Пинг", GUILayout.Width(42)))
            EditorGUIUtility.PingObject(pet);
    }

    // ── PlayerPrefs helpers ──────────────────────────────────────────────

    private static PetProgressSaveData LoadSaveData()
    {
        if (!PlayerPrefs.HasKey("pet_progress"))
            return new PetProgressSaveData { ownedPetCounts = new Dictionary<string, int>() };

        try
        {
            return JsonConvert.DeserializeObject<PetProgressSaveData>(
                PlayerPrefs.GetString("pet_progress"))
                ?? new PetProgressSaveData { ownedPetCounts = new Dictionary<string, int>() };
        }
        catch
        {
            return new PetProgressSaveData { ownedPetCounts = new Dictionary<string, int>() };
        }
    }

    private static void WriteSaveData(PetProgressSaveData data)
    {
        PlayerPrefs.SetString("pet_progress", JsonConvert.SerializeObject(data));
        PlayerPrefs.Save();
    }

    private static void ModifyOwned(string petId, int delta)
    {
        var data = LoadSaveData();
        data.ownedPetCounts ??= new Dictionary<string, int>();
        data.ownedPetCounts.TryGetValue(petId, out var cur);
        data.ownedPetCounts[petId] = Mathf.Max(0, cur + delta);
        WriteSaveData(data);
    }

    private static void EquipPet(string petId)
    {
        var data = LoadSaveData();
        data.equippedPetId = petId;
        WriteSaveData(data);
    }
}
#endif
