using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDashController : MonoBehaviour
{
    public Player player;
    public float stopDistance;
    public Vector3 goal;
    public string buffName;
    public Buff buff;
    public Animator anim;
    public void FixedUpdate()
    {
        
            if (player.charging == true)
        {
            DashMovement(buffName, stopDistance);
        
        }
    }
    public void DashMovement(string buffName, float stopDistance)
    {
        if (player.target != null)
        {
            Vector3 goal = player.target.transform.position;
            int index = player.skills.GetBuffIndexByName(buffName);

            player.charging = true;
            anim.SetBool("chargeattack", true);
            player.transform.position = Vector3.MoveTowards(player.transform.position, goal, player.speed * Time.fixedDeltaTime);
            player.transform.LookAt(goal);

            if (Vector3.Distance(player.transform.position, goal) <= stopDistance)
            {
                if (index != -1)
                {
                player.skills.buffs.RemoveAt(index);
                }
                player.charging = false;
                anim.SetBool("chargeattack", false);

            }
        }
    }
}
