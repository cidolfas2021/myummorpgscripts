
using HeathenEngineering.SteamworksIntegration;
using Steamworks;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    private LeaderboardData board;
   
    void Start()
    {
        LeaderboardData.Get("goonkills", (board, ioError) =>
        {
            if (!ioError)
                this.board = board;
        });
    }

    public void OnScored(int score)
    {
        if (board.Valid)
        {
            board.UploadScore(score, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, (uploaded, ioError) =>
            {
                if (ioError)
                    Debug.Log("IO error");
                else
                    Debug.Log($"Success: {uploaded.Success}, Score Changed: {uploaded.ScoreChanged}, Score: {uploaded.Score}, Rank: {uploaded.GlobalRankNew}");
            });
        }
        else
            Debug.Log("You dont have a board, fix it");
    }

}