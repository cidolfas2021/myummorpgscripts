using System.Collections.Generic;
using UnityEngine;
using GameZero;
namespace uMMORPG
{
    [CreateAssetMenu(menuName = "uMMORPG Skill/aoedamageskillstart", order = 999)]
    public class aoedamageskillstart : BuffSkill
    {
        public AoeEffect aoe;
        
        HealSkill HealSkill;
        
        
        public bool worksOnSelf = true;
        public bool targetRequired;
        public bool hitBoxAtStartcast;
        public bool hitBoxAtEndCast;
        public bool isMelee;
        Vector3 casterPosition;
        public bool currentlyCasting = false;

        static Collider[] hitsBuffer = new Collider[10000];

        Entity CorrectedTarget(Entity caster)
        {
            if (caster.target == null)
                return worksOnSelf ? caster : null;

            if (caster.target == caster)
                return worksOnSelf ? caster : null;

            return worksOnSelf ? caster.target : null;
        }
        public override bool CheckTarget(Entity caster)
        {
            if (targetRequired != true)
            {
                if (caster.target == null)
                {
                    caster.target = CorrectedTarget(caster);
                }

                return caster.target != null && caster.target.health.current > 0;
            }
            else return caster.target != null && caster.CanAttack(caster.target);
        }

        public override bool CheckDistance(Entity caster, int skillLevel, out Vector3 destination)
        {
            destination = caster.transform.position;
            return true;
        }
        public override void Apply(Entity caster, int skillLevel)
        {
            
        }
        public void onemil(Entity caster, int skillLevel)
        {
            Vector3 newVector = caster.collider.bounds.center + new Vector3(0, +1, 0);
            GameObject go = Instantiate(aoe.gameObject, newVector, Quaternion.identity, caster.transform);
            AoeEffect effectComponent = go.GetComponent<AoeEffect>();

            Debug.Log("1milpunches");

            Destroy(go.gameObject, 1f);
        }
        //player skills
        public void jumplunge(Entity caster, int skillLevel)
        {
            Vector3 newVector = caster.collider.bounds.center + new Vector3(0, +1, 0);
            GameObject go = Instantiate(aoe.gameObject, newVector, Quaternion.identity, caster.transform);
            AoeEffect effectComponent = go.GetComponent<AoeEffect>();

            Debug.Log("jumplunge");

            Destroy(go.gameObject, 1f);
        }
        public void dashforward(Entity caster, int skillLevel)
        {
            Vector3 newVector = caster.collider.bounds.center + new Vector3(0, +1, 0);
            GameObject go = Instantiate(aoe.gameObject, newVector, Quaternion.identity, caster.transform);
            AoeEffect effectComponent = go.GetComponent<AoeEffect>();

            Debug.Log("dashforward");

            Destroy(go.gameObject, 1f);
        }
        public void cyclonecast(Entity caster, int skillLevel)
        {
            Vector3 newVector = caster.collider.bounds.center + new Vector3(0, +1, 0);
            GameObject go = Instantiate(aoe.gameObject, newVector, Quaternion.identity, caster.transform);
            AoeEffect effectComponent = go.GetComponent<AoeEffect>();

            Debug.Log("cyclonecaller");
            
            Destroy(go.gameObject, 1f);
        }
        public void overheadcast(Entity caster, int skillLevel)
        {
            Vector3 newVector = caster.collider.bounds.center + new Vector3(0, +1, 0);
            GameObject go = Instantiate(aoe.gameObject, newVector, Quaternion.identity, caster.transform);
            AoeEffect effectComponent = go.GetComponent<AoeEffect>();

            Debug.Log("overheadcaller");
            
            Destroy(go.gameObject, 1f);
        }
        public void swingcast(Entity caster, int skillLevel)
        {
            Vector3 newVector = caster.collider.bounds.center + new Vector3(0, +1, 0);
            GameObject go = Instantiate(aoe.gameObject, newVector, Quaternion.identity, caster.transform);
            AoeEffect effectComponent = go.GetComponent<AoeEffect>();

            Debug.Log("swingcaller");
            
            Destroy(go.gameObject, 1f);
        }
        public void swingcast2(Entity caster, int skillLevel)
        {
            Vector3 newVector = caster.collider.bounds.center + new Vector3(0, +1, 0);
            GameObject go = Instantiate(aoe.gameObject, newVector, Quaternion.identity, caster.transform);
            AoeEffect effectComponent = go.GetComponent<AoeEffect>();

            Debug.Log("swingcaller2");

            Destroy(go.gameObject, 1f);
        }
        //monsterskills
        public void infernofirebreath(Entity caster, int skillLevel)
        {
            Vector3 newVector = caster.collider.bounds.center + new Vector3(0, +1, 0);
            GameObject go = Instantiate(aoe.gameObject, newVector, Quaternion.identity, caster.transform);
            AoeEffect effectComponent = go.GetComponent<AoeEffect>();

            Debug.Log("infernocaller");

            Destroy(go.gameObject, 1f);
        }
        /*public override void OnCastStartedServer(Entity caster, int skillLevel)
        {
            
        }*/
    }
}