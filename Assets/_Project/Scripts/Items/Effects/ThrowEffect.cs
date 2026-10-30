using UnityEngine;

namespace Animas.Items
{
    // Agua bendita and molotov: thrown toward where the Padre looks (GDD §14). The flask breaks on
    // whatever it hits and leaves its effect there.
    [CreateAssetMenu(fileName = "EFF_Lanzar", menuName = "Ánimas/Effects/Throw")]
    public class ThrowEffect : ItemEffect
    {
        [SerializeField] Rigidbody flaskPrefab;
        [SerializeField, Min(1f)] float throwSpeed = 12f;
        [Tooltip("A little upward push, so it flies in an arc.")]
        [SerializeField, Min(0f)] float upward = 2.5f;

        public override bool Use(ItemUser user, ItemDefinition item)
        {
            Transform aim = user.Aim;
            Rigidbody flask = Instantiate(flaskPrefab, aim.position + aim.forward * 0.6f, aim.rotation);
            // It starts inside the Padre's reach: it must not hit his own body.
            foreach (Collider own in user.GetComponentsInChildren<Collider>())
                foreach (Collider mine in flask.GetComponentsInChildren<Collider>())
                    Physics.IgnoreCollision(own, mine);
            flask.linearVelocity = aim.forward * throwSpeed + Vector3.up * upward;
            user.NotifyThrew();
            return true;
        }
    }
}
