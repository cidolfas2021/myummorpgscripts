using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Mirror;
namespace GameZero
{ 
public class cycloneeffect : MonoBehaviour
{
    HealSkill spawn;
    float remainingCooldown;
    //float remainingCooldown2;
    public List<Entity> targets = new List<Entity>();
    aoedamageskillstart aoe;
    public Entity caster;
    public Entity target;
    public int skillLevel;
    public LinearInt damage;
    
    private void OnTriggerEnter(Collider co)
    {
        if (co.tag == "Monster" || co.tag == "Player")
        {
            targets.Add(co.gameObject.GetComponent<Entity>());

        }
    }
        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "Monster" || other.tag == "Player")
            {
                Entity entity = other.GetComponent<Entity>();
                if (entity != null && targets.Contains(entity))
                {
                    targets.Remove(entity);
                }
            }
        }

        void FixedUpdate()
    {

        remainingCooldown -= Time.deltaTime;
        if (remainingCooldown <= 0)
        {
            foreach (Entity co in targets)
            {
                // find the skill that we casted this effect with
                caster.combat.DealDamageAt(co, caster.combat.damage + damage.Get(skillLevel));
            }
            remainingCooldown = .25f;
            Debug.Log("cyclone attack");
           
        }

        
        
           
        
        //else if (isServer) NetworkServer.Destroy(gameObject);
    }


 
    public void cyclonecaller(Entity caster, int skillLevel)
    {
        
       
    }
    }
}