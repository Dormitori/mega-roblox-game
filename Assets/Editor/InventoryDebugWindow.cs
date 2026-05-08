#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public class InventoryDebugWindow : EditorWindow
{
    private static readonly string[] TabNames = { "Валюта", "Кирки", "Блоки" };
    private int _tab;
    private Vector2 _scroll;

    // Currency
    private int _coinsAdd = 10000;
    private int _crystalsAdd = 100;

    // Pickaxes
    private List<PickaxeConfig> _pickaxeConfigs = new();

    // Blocks
    private List<BlockConfig> _blockConfigs = new();
    private readonly Dictionary<BlockType, int> _blockAddAmounts = new();
    private int _bulkAmount = 100;
    private static readonly HashSet<BlockType> BuyableBlocks = new()
    {
        BlockType.Sand, BlockType.Ground, BlockType.Stone, BlockType.Rock, BlockType.Lava, BlockType.Obsidian,
        BlockType.OreCoal, BlockType.OreCopper, BlockType.OreIron, BlockType.OreBronze, BlockType.OreQuartz,
        BlockType.OreSilver, BlockType.OreGold, BlockType.OreLapis, BlockType.OreDiamond, BlockType.OreAmethyst,
        BlockType.OreGreenDiamond, BlockType.OreRuby,
    };

    [MenuItem("Tools/Инвентарь/Дебаг инвентаря")]
    public static void Open() => GetWindow<InventoryDebugWindow>("Inventory Debug");

    private void OnEnable() => Reload();

    private void Reload()
    {
        _pickaxeConfigs = AssetDatabase.FindAssets("t:PickaxeConfig")
            .Select(g => AssetDatabase.LoadAssetAtPath<PickaxeConfig>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(c => c != null)
            .OrderBy(c => (int)c.type)
            .ToList();

        _blockConfigs = AssetDatabase.FindAssets("t:BlockConfig")
            .Select(g => AssetDatabase.LoadAssetAtPath<BlockConfig>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(c => c != null)
            .OrderBy(c => (int)c.type)
            .ToList();

        foreach (BlockType t in Enum.GetValues(typeof(BlockType)))
            if (!_blockAddAmounts.ContainsKey(t))
                _blockAddAmounts[t] = 100;
    }

    private void OnGUI()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            _tab = GUILayout.Toolbar(_tab, TabNames, EditorStyles.toolbarButton);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Обновить", EditorStyles.toolbarButton, GUILayout.Width(80)))
                Reload();
        }

        EditorGUILayout.Space(4);
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        switch (_tab)
        {
            case 0: DrawCurrencyTab(); break;
            case 1: DrawPickaxeTab(); break;
            case 2: DrawBlocksTab(); break;
        }

        EditorGUILayout.EndScrollView();
    }

    // ── Currency ─────────────────────────────────────────────────────────

    private void DrawCurrencyTab()
    {
        var data = LoadInventory();
        var coins = GetCurrency(data, CurrencyType.Coins);
        var crystals = GetCurrency(data, CurrencyType.Crystals);

        EditorGUILayout.LabelField("Текущий баланс", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"  Монеты:   {coins:N0}");
        EditorGUILayout.LabelField($"  Кристаллы: {crystals:N0}");
        EditorGUILayout.Space(8);

        EditorGUILayout.LabelField("Добавить", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            _coinsAdd = EditorGUILayout.IntField("Монеты", _coinsAdd);
            if (GUILayout.Button("+", GUILayout.Width(32)))
            {
                ModifyCurrency(CurrencyType.Coins, _coinsAdd);
                Repaint();
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            _crystalsAdd = EditorGUILayout.IntField("Кристаллы", _crystalsAdd);
            if (GUILayout.Button("+", GUILayout.Width(32)))
            {
                ModifyCurrency(CurrencyType.Crystals, _crystalsAdd);
                Repaint();
            }
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Установить", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Монеты → 0"))    SetCurrency(CurrencyType.Coins, 0);
            if (GUILayout.Button("Монеты → 999 999")) SetCurrency(CurrencyType.Coins, 999999);
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Кристаллы → 0"))    SetCurrency(CurrencyType.Crystals, 0);
            if (GUILayout.Button("Кристаллы → 9 999")) SetCurrency(CurrencyType.Crystals, 9999);
        }
    }

    // ── Pickaxes ──────────────────────────────────────────────────────────

    private void DrawPickaxeTab()
    {
        var data = LoadInventory();

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Разблокировать все кирки"))
            {
                var inv = LoadInventory();
                foreach (PickaxeType t in Enum.GetValues(typeof(PickaxeType)))
                    inv.Pickaxes.Add(t);
                WriteInventory(inv);
            }
        }

        EditorGUILayout.Space(6);
        DrawPickaxeHeader();

        if (_pickaxeConfigs.Count == 0)
        {
            EditorGUILayout.HelpBox("PickaxeConfig-ассеты не найдены.", MessageType.Warning);

            // Fallback: iterate enum directly
            foreach (PickaxeType t in Enum.GetValues(typeof(PickaxeType)))
                DrawPickaxeRowEnum(t, data);
            return;
        }

        foreach (var cfg in _pickaxeConfigs)
            DrawPickaxeRow(cfg, data);
    }

    private static void DrawPickaxeHeader()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            GUILayout.Label("Кирка", GUILayout.Width(160));
            GUILayout.Label("Урон", GUILayout.Width(50));
            GUILayout.Label("Скорость", GUILayout.Width(70));
            GUILayout.Label("Разблок.", GUILayout.Width(70));
            GUILayout.Label("Экипир.", GUILayout.Width(65));
            GUILayout.Label("", GUILayout.ExpandWidth(true));
        }
    }

    private static void DrawPickaxeRow(PickaxeConfig cfg, InventorySaveData data)
    {
        var isUnlocked = data.Pickaxes.Contains(cfg.type);
        var isEquipped = data.CurrentPickaxe == cfg.type;

        using (new EditorGUILayout.HorizontalScope())
        {
            if (cfg.pickaxeIcon != null)
            {
                var tex = AssetPreview.GetAssetPreview(cfg.pickaxeIcon)
                          ?? AssetPreview.GetMiniThumbnail(cfg.pickaxeIcon);
                GUILayout.Label(tex != null ? new GUIContent(tex) : GUIContent.none,
                    GUILayout.Width(20), GUILayout.Height(20));
            }
            else GUILayout.Space(24);

            GUILayout.Label(cfg.type.ToString(), GUILayout.Width(140));
            GUILayout.Label(cfg.baseDamage.ToString(), GUILayout.Width(50));
            GUILayout.Label(cfg.baseSpeedMultiplier.ToString("F2"), GUILayout.Width(70));
            GUILayout.Label(isUnlocked ? "✓" : "—", GUILayout.Width(70));
            GUILayout.Label(isEquipped ? "✓" : "—", GUILayout.Width(65));

            if (!isUnlocked && GUILayout.Button("Купить", GUILayout.Width(58)))
            {
                var inv = LoadInventory();
                inv.Pickaxes.Add(cfg.type);
                WriteInventory(inv);
            }

            using (new EditorGUI.DisabledScope(!isUnlocked))
            {
                if (!isEquipped && GUILayout.Button("Надеть", GUILayout.Width(58)))
                {
                    var inv = LoadInventory();
                    inv.CurrentPickaxe = cfg.type;
                    WriteInventory(inv);
                }
            }

            if (GUILayout.Button("Пинг", GUILayout.Width(42)))
                EditorGUIUtility.PingObject(cfg);
        }
    }

    private static void DrawPickaxeRowEnum(PickaxeType t, InventorySaveData data)
    {
        var isUnlocked = data.Pickaxes.Contains(t);
        var isEquipped = data.CurrentPickaxe == t;

        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label(t.ToString(), GUILayout.Width(200));
            GUILayout.Label(isUnlocked ? "✓" : "—", GUILayout.Width(70));
            GUILayout.Label(isEquipped ? "✓" : "—", GUILayout.Width(65));

            if (!isUnlocked && GUILayout.Button("Купить", GUILayout.Width(58)))
            {
                var inv = LoadInventory();
                inv.Pickaxes.Add(t);
                WriteInventory(inv);
            }

            using (new EditorGUI.DisabledScope(!isUnlocked))
            {
                if (!isEquipped && GUILayout.Button("Надеть", GUILayout.Width(58)))
                {
                    var inv = LoadInventory();
                    inv.CurrentPickaxe = t;
                    WriteInventory(inv);
                }
            }
        }
    }

    // ── Blocks ────────────────────────────────────────────────────────────

    private void DrawBlocksTab()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            _bulkAmount = EditorGUILayout.IntField("Кол-во (bulk)", _bulkAmount, GUILayout.Width(220));

            if (GUILayout.Button($"Добавить все ×{_bulkAmount}"))
            {
                var inv = LoadInventory();
                foreach (var cfg in _blockConfigs)
                    AddBlockToData(inv, cfg.type, _bulkAmount, cfg.baseSellPrice);
                WriteInventory(inv);
            }

            if (GUILayout.Button("Очистить все блоки"))
            {
                if (EditorUtility.DisplayDialog("Очистить блоки",
                        "Обнулить все блоки в инвентаре?", "Да", "Отмена"))
                {
                    var inv = LoadInventory();
                    foreach (BlockType t in Enum.GetValues(typeof(BlockType)))
                    {
                        inv.Blocks[t] = 0;
                        inv.BlockSellValueTotals[t] = 0;
                    }
                    WriteInventory(inv);
                }
            }
        }

        EditorGUILayout.Space(6);
        DrawBlockHeader();

        var data = LoadInventory();

        if (_blockConfigs.Count == 0)
        {
            EditorGUILayout.HelpBox("BlockConfig-ассеты не найдены.", MessageType.Warning);
            foreach (BlockType t in Enum.GetValues(typeof(BlockType)))
                DrawBlockRowEnum(t, data);
            return;
        }

        foreach (var cfg in _blockConfigs)
            DrawBlockRow(cfg, data);
    }

    private static void DrawBlockHeader()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            GUILayout.Label("Блок", GUILayout.Width(150));
            GUILayout.Label("Цена/шт", GUILayout.Width(65));
            GUILayout.Label("В инв.", GUILayout.Width(60));
            GUILayout.Label("Добавить", GUILayout.Width(80));
            GUILayout.Label("", GUILayout.ExpandWidth(true));
        }
    }

    private void DrawBlockRow(BlockConfig cfg, InventorySaveData data)
    {
        var currentCount = data.Blocks.TryGetValue(cfg.type, out var c) ? c : 0;
        _blockAddAmounts.TryGetValue(cfg.type, out var addAmt);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (cfg.icon != null)
            {
                var tex = AssetPreview.GetAssetPreview(cfg.icon)
                          ?? AssetPreview.GetMiniThumbnail(cfg.icon);
                GUILayout.Label(tex != null ? new GUIContent(tex) : GUIContent.none,
                    GUILayout.Width(20), GUILayout.Height(20));
            }
            else GUILayout.Space(24);

            GUILayout.Label(cfg.type.ToString(), GUILayout.Width(128));
            GUILayout.Label(cfg.baseSellPrice.ToString(), GUILayout.Width(65));
            GUILayout.Label(currentCount.ToString(), GUILayout.Width(60));

            _blockAddAmounts[cfg.type] = EditorGUILayout.IntField(addAmt, GUILayout.Width(60));

            if (GUILayout.Button("+", GUILayout.Width(26)))
            {
                var inv = LoadInventory();
                AddBlockToData(inv, cfg.type, _blockAddAmounts[cfg.type], cfg.baseSellPrice);
                WriteInventory(inv);
            }

            if (GUILayout.Button("Пинг", GUILayout.Width(42)))
                EditorGUIUtility.PingObject(cfg);
        }
    }

    private void DrawBlockRowEnum(BlockType t, InventorySaveData data)
    {
        var currentCount = data.Blocks.TryGetValue(t, out var c) ? c : 0;
        _blockAddAmounts.TryGetValue(t, out var addAmt);

        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label(t.ToString(), GUILayout.Width(180));
            GUILayout.Label(currentCount.ToString(), GUILayout.Width(60));
            _blockAddAmounts[t] = EditorGUILayout.IntField(addAmt, GUILayout.Width(60));

            if (GUILayout.Button("+", GUILayout.Width(26)))
            {
                var inv = LoadInventory();
                AddBlockToData(inv, t, _blockAddAmounts[t], 0);
                WriteInventory(inv);
            }
        }
    }

    // ── Inventory PlayerPrefs helpers ─────────────────────────────────────

    private static InventorySaveData LoadInventory()
    {
        if (!PlayerPrefs.HasKey(SaveKeys.Inventory))
            return DefaultInventory();

        try
        {
            var data = JsonConvert.DeserializeObject<InventorySaveData>(
                PlayerPrefs.GetString(SaveKeys.Inventory));

            if (data == null) return DefaultInventory();

            data.Currencies ??= new Dictionary<CurrencyType, int>();
            data.Blocks ??= new Dictionary<BlockType, int>();
            data.BlockSellValueTotals ??= new Dictionary<BlockType, long>();
            data.Pickaxes ??= new HashSet<PickaxeType>();
            data.UnlockedBuyableBlocks ??= new HashSet<BlockType>();

            foreach (CurrencyType cur in Enum.GetValues(typeof(CurrencyType)))
                if (!data.Currencies.ContainsKey(cur)) data.Currencies[cur] = 0;

            foreach (BlockType t in Enum.GetValues(typeof(BlockType)))
            {
                if (!data.Blocks.ContainsKey(t)) data.Blocks[t] = 0;
                if (!data.BlockSellValueTotals.ContainsKey(t)) data.BlockSellValueTotals[t] = 0;
            }

            return data;
        }
        catch
        {
            return DefaultInventory();
        }
    }

    private static InventorySaveData DefaultInventory()
    {
        var d = new InventorySaveData(
            new Dictionary<CurrencyType, int>(),
            new Dictionary<BlockType, int>(),
            new Dictionary<BlockType, long>(),
            new HashSet<PickaxeType> { PickaxeType.PickaxeWood01 },
            new HashSet<BlockType>(),
            PickaxeType.PickaxeWood01,
            40);

        foreach (CurrencyType c in Enum.GetValues(typeof(CurrencyType)))
            d.Currencies[c] = 0;
        foreach (BlockType t in Enum.GetValues(typeof(BlockType)))
        {
            d.Blocks[t] = 0;
            d.BlockSellValueTotals[t] = 0;
        }

        return d;
    }

    private static void WriteInventory(InventorySaveData data)
    {
        PlayerPrefs.SetString(SaveKeys.Inventory, JsonConvert.SerializeObject(data));
        PlayerPrefs.Save();
    }

    private static int GetCurrency(InventorySaveData data, CurrencyType type)
        => data.Currencies.TryGetValue(type, out var v) ? v : 0;

    private static void ModifyCurrency(CurrencyType type, int delta)
    {
        var inv = LoadInventory();
        inv.Currencies[type] = Math.Max(0, GetCurrency(inv, type) + delta);
        WriteInventory(inv);
    }

    private static void SetCurrency(CurrencyType type, int value)
    {
        var inv = LoadInventory();
        inv.Currencies[type] = Math.Max(0, value);
        WriteInventory(inv);
    }

    private static void AddBlockToData(InventorySaveData inv, BlockType type, int amount, int unitSellPrice)
    {
        inv.Blocks[type] = inv.Blocks.TryGetValue(type, out var cur) ? cur + amount : amount;
        inv.BlockSellValueTotals[type] =
            (inv.BlockSellValueTotals.TryGetValue(type, out var val) ? val : 0)
            + (long)unitSellPrice * amount;

        if (BuyableBlocks.Contains(type))
            inv.UnlockedBuyableBlocks.Add(type);
    }
}
#endif
