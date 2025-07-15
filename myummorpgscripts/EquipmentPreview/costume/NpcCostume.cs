using UnityEngine;
namespace uMMORPG
{
    public class NpcCostume : NpcOffer
    {


        public override bool HasOffer(Player player) => player.npcCostume;

        public override string GetOfferName() => "Customize Equips";

        public override void OnSelect(Player player)
        {
            UINpcCostume.singleton.panel.SetActive(true);
            UIInventory.singleton.panel.SetActive(true); // better feedback
            UINpcDialogue.singleton.panel.SetActive(false);
        }
    }
}