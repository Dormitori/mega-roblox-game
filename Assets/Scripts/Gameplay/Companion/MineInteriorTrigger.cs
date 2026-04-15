using UnityEngine;

/// <summary>
/// Повесь на BoxCollider (isTrigger) вокруг объёма шахты под «землёй».
/// Пока игрок внутри — питомец ждёт на <see cref="CompanionPetController"/> wait anchor.
/// </summary>
[RequireComponent(typeof(Collider))]
public class MineInteriorTrigger : MonoBehaviour
{
    private void Reset()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;
        CompanionMineState.Enter();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;
        CompanionMineState.Exit();
    }

    private static bool IsPlayer(Collider other) =>
        other.GetComponentInParent<CharacterMovement>() != null;
}
