using UnityEngine;
using Mirror;

public class ServerController : MonoBehaviour
{
    private NetworkManager networkManager;

    private void Awake()
    {
        networkManager = GetComponent<NetworkManager>();
    }

    private void Start()
    {
        // Start the server when the game starts
        networkManager.StartServer();
    }
}