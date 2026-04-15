using UnityEngine;

/// <summary>
/// Свап визуала питомца под VisualRoot и перепривязка Animator к CompanionPetController.
/// </summary>
public class CompanionPetVisual : MonoBehaviour
{
    [SerializeField] private CompanionPetController companion;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private PetConfig initialPet;

    private GameObject _instance;

    private void Start()
    {
        if (initialPet != null)
            Apply(initialPet);
    }

    public void Apply(PetConfig config)
    {
        if (config == null || config.visualPrefab == null || visualRoot == null || companion == null)
            return;

        ClearVisual();

        _instance = Instantiate(config.visualPrefab, visualRoot);
        _instance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        _instance.transform.localScale = Vector3.one;

        var anim = _instance.GetComponentInChildren<Animator>(true);
        companion.BindAnimator(anim);
    }

    public void ClearVisual()
    {
        if (visualRoot == null)
            return;

        for (var i = visualRoot.childCount - 1; i >= 0; i--)
            Destroy(visualRoot.GetChild(i).gameObject);

        _instance = null;
        if (companion != null)
            companion.BindAnimator(null);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (companion == null)
            companion = GetComponentInParent<CompanionPetController>();
        if (visualRoot == null && transform.childCount > 0)
        {
            var t = transform.Find("VisualRoot");
            if (t != null)
                visualRoot = t;
        }
    }
#endif
}
