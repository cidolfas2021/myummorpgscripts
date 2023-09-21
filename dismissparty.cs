using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dismissparty : MonoBehaviour
{
    
    void OnButtonClick()
    {
        Player player = GetComponent<Player>();
        PartySystem.DismissParty(player.party.party.partyId, player.name);
        Debug.Log($"dismissed party {player.party.party.partyId}");
    }

}
