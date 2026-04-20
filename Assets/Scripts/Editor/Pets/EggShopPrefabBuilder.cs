using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Собирает черновой префаб магазина яиц/петов: раскладка колонками, скролл с сеткой,3 слота инкубатора.
/// Меню: Tools → Pets → Build EggShop UI Prefab
/// </summary>
public static class EggShopPrefabBuilder
{
    private const string PrefabDir = "Assets/Prefabs/UI";
    private const string EggRowPrefabPath = PrefabDir + "/EggShopEggRow.prefab";
    private const string PetCellPrefabPath = PrefabDir + "/EggShopPetCell.prefab";
    private const string EggShopPrefabPath = PrefabDir + "/EggShop.prefab";

    [MenuItem("Tools/Pets/Build EggShop UI Prefab")]
    public static void BuildAll()
    {
        EnsureFolder("Assets/Prefabs");
        EnsureFolder(PrefabDir);

        var eggRow = BuildEggRowPrefab();
        var petCell = BuildPetCellPrefab();
        BuildEggShopPrefab(eggRow, petCell);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"EggShop: префабы сохранены — {EggShopPrefabPath} (подпрефабы: EggShopEggRow, EggShopPetCell). Перетащи EggShop под твой Canvas в сцене.");
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;
        var parent = System.IO.Path.GetDirectoryName(path)?.Replace("\\", "/");
        var name = System.IO.Path.GetFileName(path);
        if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(name))
            return;
        if (!AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }

    private static GameObject BuildEggRowPrefab()
    {
        var root = new GameObject("EggShopEggRow", typeof(RectTransform));
        var rt = root.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(260f, 220f);

        var vlg = root.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(8, 8, 8, 8);
        vlg.spacing = 8f;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;

        var leRoot = root.AddComponent<LayoutElement>();
        leRoot.minWidth = 240f;
        leRoot.preferredWidth = 260f;

        var icon = CreateImage("Icon", root.transform, new Vector2(72f, 72f));
        var title = CreateTmp("Title", root.transform, "Egg", 20, TextAlignmentOptions.Center);

        var buyRow = new GameObject("BuyRow", typeof(RectTransform));
        buyRow.transform.SetParent(root.transform, false);
        var buyRt = buyRow.GetComponent<RectTransform>();
        var hlg = buyRow.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8f;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = false;
        var buyLe = buyRow.AddComponent<LayoutElement>();
        buyLe.preferredHeight = 48f;
        buyLe.flexibleWidth = 1f;

        var (btnCoins, tmpCoins) = CreateCompactBuyButton(buyRow.transform, "BuyCoins");
        var (btnCrystals, tmpCrystals) = CreateCompactBuyButton(buyRow.transform, "BuyCrystals");

        var view = root.AddComponent<EggShopEggView>();
        view.iconImage = icon;
        view.titleText = title;
        view.buyCoinsButton = btnCoins;
        view.buyCoinsPriceText = tmpCoins;
        view.buyCrystalsButton = btnCrystals;
        view.buyCrystalsPriceText = tmpCrystals;

        return SavePrefab(root, EggRowPrefabPath);
    }

    private static GameObject BuildPetCellPrefab()
    {
        var root = new GameObject("EggShopPetCell", typeof(RectTransform));
        var rt = root.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(120f, 120f);

        var bg = CreateImage("Raycast", root.transform, stretch: true);
        bg.color = new Color(1f, 1f, 1f, 0.001f);
        bg.raycastTarget = true;
        StretchFull(bg.rectTransform);

        var icon = CreateImage("Icon", root.transform, stretch: true);
        icon.raycastTarget = false;

        var dark = CreateImage("Dark", root.transform, stretch: true);
        dark.color = new Color(0f, 0f, 0f, 0.55f);
        dark.raycastTarget = false;
        StretchFull(dark.rectTransform);

        var outline = CreateImage("SelectedOutline", root.transform, stretch: true);
        outline.color = new Color(1f, 0.85f, 0.2f, 0f);
        outline.raycastTarget = false;
        StretchWithPadding(outline.rectTransform, -3f);

        var equipped = CreateImage("Equipped", root.transform, new Vector2(22f, 22f));
        equipped.color = Color.green;
        equipped.rectTransform.anchorMin = new Vector2(0f, 1f);
        equipped.rectTransform.anchorMax = new Vector2(0f, 1f);
        equipped.rectTransform.pivot = new Vector2(0f, 1f);
        equipped.rectTransform.anchoredPosition = new Vector2(6f, -6f);

        var count = CreateTmp("Count", root.transform, "0", 18, TextAlignmentOptions.BottomRight);
        count.rectTransform.anchorMin = new Vector2(1f, 0f);
        count.rectTransform.anchorMax = new Vector2(1f, 0f);
        count.rectTransform.pivot = new Vector2(1f, 0f);
        count.rectTransform.anchoredPosition = new Vector2(-4f, 4f);

        var le = root.AddComponent<LayoutElement>();
        le.minWidth = 110f;
        le.minHeight = 110f;
        le.preferredWidth = 120f;
        le.preferredHeight = 120f;

        var gridItem = root.AddComponent<PetShopGridItemView>();
        gridItem.iconImage = icon;
        gridItem.darkImage = dark;
        gridItem.equippedIcon = equipped;
        gridItem.selectedOutline = outline;
        gridItem.countText = count;

        return SavePrefab(root, PetCellPrefabPath);
    }

    private static void BuildEggShopPrefab(GameObject eggRowPrefab, GameObject petCellPrefab)
    {
        var root = new GameObject("EggShop", typeof(RectTransform), typeof(CanvasGroup));
        var rt = root.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(1400f, 920f);

        var dim = CreateImage("Dim", root.transform, stretch: true);
        dim.color = new Color(0f, 0f, 0f, 0.45f);
        dim.raycastTarget = true;
        StretchFull(dim.rectTransform);

        var panel = CreateImage("Panel", root.transform, stretch: true);
        panel.color = new Color(0.18f, 0.2f, 0.24f, 0.98f);
        StretchWithPadding(panel.rectTransform, 24f);

        var topBar = new GameObject("TopBar", typeof(RectTransform));
        topBar.transform.SetParent(panel.transform, false);
        var topRt = topBar.GetComponent<RectTransform>();
        StretchTop(topRt, 56f);
        var topH = topBar.AddComponent<HorizontalLayoutGroup>();
        topH.padding = new RectOffset(12, 12, 8, 8);
        topH.childAlignment = TextAnchor.MiddleLeft;
        topH.childForceExpandHeight = true;
        topH.childForceExpandWidth = true;

        var title = CreateTmp("ShopTitle", topBar.transform, "Egg shop", 28, TextAlignmentOptions.Left);
        var titleLe = title.gameObject.AddComponent<LayoutElement>();
        titleLe.flexibleWidth = 1f;

        var closeGo = new GameObject("Button_Close", typeof(RectTransform), typeof(Image), typeof(Button));
        closeGo.transform.SetParent(topBar.transform, false);
        var closeRt = closeGo.GetComponent<RectTransform>();
        closeRt.sizeDelta = new Vector2(96f, 44f);
        var closeImg = closeGo.GetComponent<Image>();
        closeImg.color = new Color(0.35f, 0.35f, 0.4f, 1f);
        var closeBtn = closeGo.GetComponent<Button>();
        var closeLabel = CreateTmp("Label", closeGo.transform, "X", 22, TextAlignmentOptions.Center);
        StretchFull(closeLabel.rectTransform);

        var body = new GameObject("Body", typeof(RectTransform));
        body.transform.SetParent(panel.transform, false);
        var bodyRt = body.GetComponent<RectTransform>();
        StretchMiddleBelowTop(bodyRt, topOffset: 56f, bottomOffset: 16f);
        var bodyH = body.AddComponent<HorizontalLayoutGroup>();
        bodyH.padding = new RectOffset(12, 12, 8, 8);
        bodyH.spacing = 12f;
        bodyH.childAlignment = TextAnchor.UpperCenter;
        bodyH.childForceExpandHeight = true;
        bodyH.childForceExpandWidth = false;

        // Left: eggs column
        var leftCol = new GameObject("EggsColumn", typeof(RectTransform));
        leftCol.transform.SetParent(body.transform, false);
        var leftLe = leftCol.AddComponent<LayoutElement>();
        leftLe.preferredWidth = 280f;
        leftLe.flexibleWidth = 0f;
        leftLe.flexibleHeight = 1f;
        var leftV = leftCol.AddComponent<VerticalLayoutGroup>();
        leftV.spacing = 10f;
        leftV.childAlignment = TextAnchor.UpperCenter;
        leftV.childForceExpandWidth = true;
        var leftHeader = CreateTmp("EggsHeader", leftCol.transform, "Eggs", 18, TextAlignmentOptions.Center);
        var leftHeaderLe = leftHeader.gameObject.AddComponent<LayoutElement>();
        leftHeaderLe.preferredHeight = 28f;
        var eggButtonsRoot = new GameObject("EggButtonsRoot", typeof(RectTransform));
        eggButtonsRoot.transform.SetParent(leftCol.transform, false);
        var eggRootRt = eggButtonsRoot.GetComponent<RectTransform>();
        var eggRootV = eggButtonsRoot.AddComponent<VerticalLayoutGroup>();
        eggRootV.spacing = 10f;
        eggRootV.childAlignment = TextAnchor.UpperCenter;
        eggRootV.childForceExpandWidth = true;
        var eggRootLe = eggButtonsRoot.AddComponent<LayoutElement>();
        eggRootLe.flexibleHeight = 1f;

        // Center: scroll + grid
        var centerCol = new GameObject("PetsColumn", typeof(RectTransform));
        centerCol.transform.SetParent(body.transform, false);
        var centerLe = centerCol.AddComponent<LayoutElement>();
        centerLe.flexibleWidth = 1f;
        centerLe.flexibleHeight = 1f;
        var centerV = centerCol.AddComponent<VerticalLayoutGroup>();
        centerV.spacing = 8f;
        centerV.childAlignment = TextAnchor.UpperCenter;
        centerV.childForceExpandWidth = true;
        centerV.childForceExpandHeight = true;

        var petsHeader = CreateTmp("PetsHeader", centerCol.transform, "Pets", 18, TextAlignmentOptions.Left);
        var petsHeaderLe = petsHeader.gameObject.AddComponent<LayoutElement>();
        petsHeaderLe.preferredHeight = 28f;

        var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollGo.transform.SetParent(centerCol.transform, false);
        var scrollRt = scrollGo.GetComponent<RectTransform>();
        var scrollLe = scrollGo.AddComponent<LayoutElement>();
        scrollLe.flexibleHeight = 1f;
        scrollLe.flexibleWidth = 1f;
        var scrollImg = scrollGo.GetComponent<Image>();
        scrollImg.color = new Color(0.1f, 0.11f, 0.13f, 1f);
        var scroll = scrollGo.GetComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 40f;

        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(scrollGo.transform, false);
        var vpRt = viewport.GetComponent<RectTransform>();
        StretchFull(vpRt);
        var vpImg = viewport.GetComponent<Image>();
        vpImg.color = new Color(1f, 1f, 1f, 0.02f);
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        var content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(viewport.transform, false);
        var contentRt = content.GetComponent<RectTransform>();
        StretchTopAnchor(contentRt);
        var grid = content.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(120f, 120f);
        grid.spacing = new Vector2(10f, 10f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperLeft;
        var fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = vpRt;
        scroll.content = contentRt;

        // Right: selected pet
        var rightCol = new GameObject("SelectedColumn", typeof(RectTransform));
        rightCol.transform.SetParent(body.transform, false);
        var rightLe = rightCol.AddComponent<LayoutElement>();
        rightLe.preferredWidth = 300f;
        rightLe.flexibleWidth = 0f;
        rightLe.flexibleHeight = 1f;
        var rightV = rightCol.AddComponent<VerticalLayoutGroup>();
        rightV.spacing = 8f;
        rightV.childAlignment = TextAnchor.UpperLeft;
        rightV.childForceExpandWidth = true;
        rightV.childForceExpandHeight = true;

        var rightHeader = CreateTmp("DetailHeader", rightCol.transform, "Pet", 18, TextAlignmentOptions.Left);
        rightHeader.gameObject.AddComponent<LayoutElement>().preferredHeight = 28f;

        var detailBg = CreateImage("DetailBg", rightCol.transform, stretch: false);
        detailBg.color = new Color(0.12f, 0.13f, 0.16f, 1f);
        var detailBgRt = detailBg.rectTransform;
        var detailBgLe = detailBg.gameObject.AddComponent<LayoutElement>();
        detailBgLe.flexibleHeight = 1f;
        detailBgLe.minHeight = 200f;
        StretchFull(detailBgRt);

        var detailInner = new GameObject("DetailInner", typeof(RectTransform));
        detailInner.transform.SetParent(detailBg.transform, false);
        var detailInnerRt = detailInner.GetComponent<RectTransform>();
        StretchFull(detailInnerRt);
        var dV = detailInner.AddComponent<VerticalLayoutGroup>();
        dV.padding = new RectOffset(12, 12, 12, 12);
        dV.spacing = 8f;
        dV.childAlignment = TextAnchor.UpperLeft;
        dV.childForceExpandWidth = true;

        var petIcon = CreateImage("PetIcon", detailInner.transform, new Vector2(120f, 120f));
        petIcon.preserveAspect = true;

        var petName = CreateTmp("PetName", detailInner.transform, "-", 24, TextAlignmentOptions.Left);
        var rarity = CreateTmp("Rarity", detailInner.transform, "", 16, TextAlignmentOptions.Left);
        var bonus = CreateTmp("Bonus", detailInner.transform, "", 16, TextAlignmentOptions.Top);
        var bonusLe = bonus.gameObject.AddComponent<LayoutElement>();
        bonusLe.preferredHeight = 80f;
        var owned = CreateTmp("Owned", detailInner.transform, "0", 16, TextAlignmentOptions.Left);

        var equippedBadge = new GameObject("EquippedBadge", typeof(RectTransform), typeof(Image));
        equippedBadge.transform.SetParent(detailInner.transform, false);
        var badgeRt = equippedBadge.GetComponent<RectTransform>();
        badgeRt.sizeDelta = new Vector2(120f, 28f);
        var badgeImg = equippedBadge.GetComponent<Image>();
        badgeImg.color = new Color(0.2f, 0.55f, 0.3f, 1f);
        var badgeTxt = CreateTmp("Txt", equippedBadge.transform, "Equipped", 14, TextAlignmentOptions.Center);
        StretchFull(badgeTxt.rectTransform);
        equippedBadge.SetActive(false);

        var takeGo = new GameObject("Button_Take", typeof(RectTransform), typeof(Image), typeof(Button));
        takeGo.transform.SetParent(detailInner.transform, false);
        var takeRt = takeGo.GetComponent<RectTransform>();
        takeRt.sizeDelta = new Vector2(200f, 44f);
        var takeImg = takeGo.GetComponent<Image>();
        takeImg.color = new Color(0.25f, 0.45f, 0.85f, 1f);
        var takeBtn = takeGo.GetComponent<Button>();
        var takeTmp = CreateTmp("Label", takeGo.transform, "Take", 18, TextAlignmentOptions.Center);
        StretchFull(takeTmp.rectTransform);

        var selectedView = detailBg.gameObject.AddComponent<SelectedPetView>();
        selectedView.petIconImage = petIcon;
        selectedView.petNameText = petName;
        selectedView.rarityText = rarity;
        selectedView.bonusText = bonus;
        selectedView.ownedCountText = owned;
        selectedView.takeButton = takeBtn;
        selectedView.equippedBadge = equippedBadge;

        var shop = root.AddComponent<EggShop>();
        shop.closeButton = closeBtn;
        shop.animationYOffset = 50f;
        shop.animationDuration = 0.2f;
        shop.characterControls = null;
        shop.eggButtonsRoot = eggButtonsRoot.transform;
        shop.eggViewPrefab = eggRowPrefab.GetComponent<EggShopEggView>();
        shop.petGridRoot = content.transform;
        shop.petItemViewPrefab = petCellPrefab.GetComponent<PetShopGridItemView>();
        shop.selectedPetView = selectedView;

        SavePrefab(root, EggShopPrefabPath);
    }

    private static (Button btn, TextMeshProUGUI priceTmp) CreateCompactBuyButton(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100f, 44f);
        var img = go.GetComponent<Image>();
        img.color = new Color(0.28f, 0.32f, 0.38f, 1f);
        var btn = go.GetComponent<Button>();
        var le = go.AddComponent<LayoutElement>();
        le.flexibleWidth = 1f;
        le.preferredHeight = 44f;

        var row = new GameObject("Row", typeof(RectTransform));
        row.transform.SetParent(go.transform, false);
        StretchFull(row.GetComponent<RectTransform>());
        var h = row.AddComponent<HorizontalLayoutGroup>();
        h.padding = new RectOffset(6, 6, 0, 0);
        h.spacing = 4f;
        h.childAlignment = TextAnchor.MiddleCenter;
        h.childForceExpandWidth = true;

        var icon = CreateImage("CurrencyIcon", row.transform, new Vector2(22f, 22f));
        icon.color = name.Contains("Crystal") ? new Color(0.5f, 0.35f, 0.9f) : new Color(0.95f, 0.8f, 0.2f);

        var tmp = CreateTmp("Price", row.transform, "0", 16, TextAlignmentOptions.Center);
        var tmpLe = tmp.gameObject.AddComponent<LayoutElement>();
        tmpLe.flexibleWidth = 1f;
        return (btn, tmp);
    }

    private static Button CreateSmallButton(Transform parent, string label)
    {
        var go = new GameObject($"Btn_{label}", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = new Color(0.3f, 0.34f, 0.4f, 1f);
        var btn = go.GetComponent<Button>();
        var le = go.AddComponent<LayoutElement>();
        le.flexibleWidth = 1f;
        le.preferredHeight = 36f;
        var tmp = CreateTmp("Txt", go.transform, label, 13, TextAlignmentOptions.Center);
        StretchFull(tmp.rectTransform);
        return btn;
    }

    private static Image CreateImage(string name, Transform parent, Vector2? size = null, bool stretch = false)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        if (stretch)
            StretchFull(rt);
        else if (size.HasValue)
        {
            rt.sizeDelta = size.Value;
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = size.Value.x;
            le.preferredHeight = size.Value.y;
        }
        return go.GetComponent<Image>();
    }

    private static TextMeshProUGUI CreateTmp(
        string name,
        Transform parent,
        string text,
        float size,
        TextAlignmentOptions align)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100f, 30f);
        return tmp;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void StretchWithPadding(RectTransform rt, float pad)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(pad, pad);
        rt.offsetMax = new Vector2(-pad, -pad);
    }

    private static void StretchTop(RectTransform rt, float height)
    {
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.offsetMin = new Vector2(0f, -height);
        rt.offsetMax = new Vector2(0f, 0f);
    }

    private static void StretchMiddleBelowTop(RectTransform rt, float topOffset, float bottomOffset)
    {
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = new Vector2(0f, bottomOffset);
        rt.offsetMax = new Vector2(0f, -topOffset);
    }

    private static void StretchBottom(RectTransform rt, float height)
    {
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.offsetMin = new Vector2(0f, 0f);
        rt.offsetMax = new Vector2(0f, height);
    }

    private static void StretchTopAnchor(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.offsetMin = new Vector2(0f, 0f);
        rt.offsetMax = new Vector2(0f, 0f);
    }

    private static GameObject SavePrefab(GameObject instance, string path)
    {
        PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }
}
