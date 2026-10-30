using System;
using Animas.Items;
using Animas.Player;
using Animas.Save;
using UnityEngine;

namespace Animas.Levels
{
    // The entity's anchor (GDD §5 and §13): the 3 key items are placed here to start the final
    // ritual. F places every one the Padre carries; when all are in, OfferingsComplete fires and
    // the ritual (#34) begins. If the Padre dies, the anchor empties (the ritual is repeated, GDD §6).
    public class Anchor : MonoBehaviour, IInteractable
    {
        [SerializeField] ItemDefinition offering;
        [SerializeField, Min(1)] int required = 3;
        [Tooltip("One model per slot, shown as offerings are placed.")]
        [SerializeField] GameObject[] placedModels;

        public int Placed { get; private set; }
        public bool Complete => Placed >= required;
        public string Prompt => Complete ? "" : $"Colocar {offering.displayName} ({Placed}/{required})";

        public event Action<int> OfferingPlaced;
        public event Action OfferingsComplete;

        PlayerRespawn respawn;

        void Start()
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) respawn = player.GetComponent<PlayerRespawn>();
            if (respawn != null) respawn.Respawned += ResetOfferings;
            ShowModels();
        }

        void OnDestroy()
        {
            if (respawn != null) respawn.Respawned -= ResetOfferings;
        }

        public void Interact(Interactor interactor)
        {
            if (Complete) return;
            var bag = interactor.GetComponentInParent<KeyItemBag>();
            var inventory = interactor.GetComponentInParent<Inventory>();
            int carried = bag != null ? bag.Count(offering) : 0;
            int toPlace = Mathf.Min(carried, required - Placed);
            if (toPlace == 0)
            {
                if (inventory != null) inventory.Notify($"Te faltan {required - Placed} {offering.displayName}");
                return;
            }
            bag.Remove(offering, toPlace);
            Placed += toPlace;
            ShowModels();
            OfferingPlaced?.Invoke(Placed);
            if (inventory != null)
                inventory.Notify(Complete ? "Las ofrendas están completas" : $"Faltan {required - Placed} {offering.displayName}");
            if (Complete) OfferingsComplete?.Invoke();
        }

        void ResetOfferings()
        {
            Placed = 0;
            ShowModels();
        }

        void ShowModels()
        {
            for (int i = 0; i < placedModels.Length; i++)
                if (placedModels[i] != null) placedModels[i].SetActive(i < Placed);
        }
    }
}
