// Area heal that heals all entities of same type in cast range
// => player heals players in cast range
// => monster heals monsters in cast range
//
// Based on BuffSkill so it can be added to Buffs list.
using System.Collections.Generic;
using UnityEngine;
using uMMORPG;
namespace GameZero
{
    [CreateAssetMenu(menuName = "uMMORPG Skill/aoedamageskillend", order = 999)]
public class aoedamageskillend : BuffSkill
{

    public bool worksOnSelf = true;
    public bool targetRequired;
    public bool hitBoxAtStartcast;
    public bool hitBoxAtEndCast;
    public bool isMelee;
        public AoeEffect aoe;

        // OverlapSphereNonAlloc array to avoid allocations.
        // -> static so we don't create one per skill
        // -> this is worth it because skills are casted a lot!
        // -> should be big enough to work in just about all cases
        static Collider[] hitsBuffer = new Collider[10000];

    Entity CorrectedTarget(Entity caster)
    {
        // targeting nothing? then try to cast on self
        if (caster.target == null)
            return worksOnSelf ? caster : null;

        // targeting self?
        if (caster.target == caster)
            return worksOnSelf ? caster : null;

        // no valid target? try to cast on self or don't cast at all
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
            // correct the target
            // can only buff the target if it's not dead
            // correct the target
            // can only buff the target if it's not dead
            return caster.target != null && caster.target.health.current > 0;
        }
        else return caster.target != null && caster.CanAttack(caster.target);

    }

    public override bool CheckDistance(Entity caster, int skillLevel, out Vector3 destination)
    {
        // can cast anywhere
        destination = caster.transform.position;
        return true;
    }

    public override void Apply(Entity caster, int skillLevel)
    {
            Vector3 newVector = caster.collider.bounds.center + new Vector3(0, +1, 0);
            GameObject go = Instantiate(aoe.gameObject, newVector, Quaternion.identity, caster.transform);
            AoeEffect effectComponent = go.GetComponent<AoeEffect>();

            Debug.Log("jumplunge");

            Destroy(go.gameObject, 1f);
        }

    public void OnCastStartedServer(Entity caster,int skillLevel)
    {

    }
}
}