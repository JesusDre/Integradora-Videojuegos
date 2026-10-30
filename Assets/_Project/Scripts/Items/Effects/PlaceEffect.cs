using UnityEngine;

namespace Animas.Items
{
    // Línea de sal, veladora and copal: placed on the ground in front of the Padre (GDD §14),
    // facing the way he looks.
    [CreateAssetMenu(fileName = "EFF_Colocar", menuName = "Ánimas/Effects/Place")]
    public class PlaceEffect : ItemEffect
    {
        [SerializeField] GameObject prefab;
        [SerializeField, Min(0f)] float distance = 1.5f;

        public override bool Use(ItemUser user, ItemDefinition item)
        {
            Vector3 forward = user.Aim.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f) forward = user.transform.forward;
            forward.Normalize();

            Vector3 spot = user.transform.position + forward * distance;
            // Down onto the ground; if there is no ground there (a cliff, the canal), it can't be placed.
            if (!Physics.Raycast(spot + Vector3.up * 1.5f, Vector3.down, out RaycastHit ground, 4f, ~0, QueryTriggerInteraction.Ignore))
            {
                user.Inventory.Notify($"No hay dónde poner: {item.displayName}");
                return false;
            }
            Instantiate(prefab, ground.point, Quaternion.LookRotation(forward));
            user.NotifyPlaced();
            return true;
        }
    }
}
