using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class TeamColor : MonoBehaviour
{
    public TMP_Text buttonText; // Reference to the button's TMP Text component
    public Image buttonBackground; // Reference to the button's background Image component

    /*public void OnButtonClick()
    {
        Player player = Player.localPlayer;
        // Toggle between team states
        if (!player.IsRedTeam && !player.IsBlueTeam && !player.IsBlackTeam && !player.IsWhiteTeam)
        {
            player.IsRedTeam = true;
        }
        else if (player.IsRedTeam)
        {
            Debug.Log("player is red team");
            player.IsBlueTeam = true;
            player.IsRedTeam = false;
        }
        else if (player.IsBlueTeam)
        {
            Debug.Log("player is blue team");
            player.IsBlackTeam = true;
            player.IsBlueTeam = false;
        }
        else if (player.IsBlackTeam)
        {
            Debug.Log("player is black team");
            player.IsWhiteTeam = true;
            player.IsBlackTeam = false;
        }
        else if (player.IsWhiteTeam)
        {
            Debug.Log("player is white team");
            player.IsRedTeam = true;
            player.IsWhiteTeam = false;
        }

        // Update the button text and color based on the selected team
        UpdateButtonUI();
    }*/

    /*private void UpdateButtonUI()
    {
        Player player = Player.localPlayer;
        if (player.IsRedTeam)
        {
            buttonText.text = "Red Team";
            buttonBackground.color = player.nameOverlayRedTeamColor;
        }
        else if (player.IsBlueTeam)
        {
            buttonText.text = "Blue Team";
            buttonBackground.color = player.nameOverlayBlueTeamColor;
        }
        else if (player.IsBlackTeam)
        {
            buttonText.text = "Black Team";
            buttonBackground.color = player.nameOverlayBlackTeamColor;
        }
        else if (player.IsWhiteTeam)
        {
            buttonText.text = "White Team";
            buttonBackground.color = player.nameOverlayWhiteTeamColor;
        }
        else
        {
            buttonText.text = "No Team";
            buttonBackground.color = Color.gray;
        }
    }*/
}