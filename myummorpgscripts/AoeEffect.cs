using System.Collections.Generic;
using UnityEngine;
using Mirror;
using uMMORPG;

namespace GameZero
{
    public class AoeEffect : MonoBehaviour
    {
        public List<Entity> targets = new List<Entity>();
        public Player caster;
        public int skillLevel;
        public LinearInt damage;
        public LinearInt stunChance;
        public LinearInt stunTime;
        private float remainingCooldown = 0.25f;

        private void OnTriggerEnter(Collider co)
        {
            // Find Entity on collider, parent, or child
            Entity entity = co.GetComponent<Entity>() ?? co.GetComponentInParent<Entity>() ?? co.GetComponentInChildren<Entity>();

            if (entity != null && !targets.Contains(entity))
            {
                // Now use the entity's tag, not the collider's
                if (entity.CompareTag("Monster") || entity.CompareTag("Player"))
                {
                    Debug.Log($"Adding {entity.name} to targets.");
                    targets.Add(entity);
                    Apply();
                }
                else
                {
                    Debug.Log($"Entity {entity.name} found but tag is not Player or Monster.");
                }
            }
            else if (entity == null)
            {
                Debug.LogWarning("No Entity component found on collider or relatives.");
            }
        }

        private void OnTriggerExit(Collider co)
        {
            // Find Entity on collider, parent, or child
            Entity entity = co.GetComponent<Entity>() ?? co.GetComponentInParent<Entity>() ?? co.GetComponentInChildren<Entity>();

            if (entity != null && targets.Contains(entity))
            {
                targets.Remove(entity);
                Debug.Log($"Removed {entity.name} from targets.");
            }
        }

        private void Apply()
        {
            caster = GetComponentInParent<Player>();
            Debug.Log("ApplyDamage method called");

            foreach (Entity co in targets)
            {
                if (co != null && caster != null && caster.combat != null)
                {
                    int damageAmount = caster.combat.damage + damage.Get(skillLevel);
                    Debug.Log($"Dealing {damageAmount} damage to {co.name}");
                    caster.combat.DealDamageAt(co,
                        damageAmount,
                        stunChance.Get(skillLevel),
                        stunTime.Get(skillLevel));
                }
                else
                {
                    Debug.LogWarning("Invalid Entity or caster/combat is null.");
                }
            }
        }
    }
}