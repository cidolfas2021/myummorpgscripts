using HeathenEngineering.SteamworksIntegration;
using Steamworks;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
namespace GameZero
{ 
public class LobbyBrowser_LobbyRecord : MonoBehaviour
{
    public TMPro.TextMeshProUGUI nameOrID;
    public TMPro.TextMeshProUGUI buttonLabel;

    private LobbyData target;
    private MyLobbyManager lobbyManager;

    private void OnDestroy()
    {
        if (lobbyManager != null)
        {
            lobbyManager.evtEnterSuccess.RemoveListener(HandleLobbyEnter);
            lobbyManager.evtLeave.RemoveListener(HandleLobbyLeave);
        }
    }

    public void SetLobby(LobbyData lobby, MyLobbyManager parent)
    {
        target = lobby;
        lobbyManager = parent;

        if (lobbyManager.Lobby == target)
            buttonLabel.text = "Leave";
        else
            buttonLabel.text = "Join";

        nameOrID.text = lobby.Name;
        if (string.IsNullOrEmpty(nameOrID.text))
            nameOrID.text = lobby.ToString();

        parent.evtEnterSuccess.AddListener(HandleLobbyEnter);
        parent.evtLeave.AddListener(HandleLobbyLeave);
    }

    private void HandleLobbyLeave()
    {
        if (lobbyManager.Lobby == target)
            buttonLabel.text = "Leave";
        else
            buttonLabel.text = "Join";
    }

    private void HandleLobbyEnter(LobbyData arg0)
    {
        if (target.IsAMember(UserData.Me))
            buttonLabel.text = "Leave";
        else
            buttonLabel.text = "Join";
    }

    public void JoinOrExit()
    {
        if (lobbyManager.Lobby == target)
            lobbyManager.Leave();
        else
        {
            
            lobbyManager.Join(target);
        }
    }
}
}
