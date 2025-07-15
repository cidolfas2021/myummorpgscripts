using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using GameZero;
namespace uMMORPG
{
    public class GateScript : NetworkBehaviour
    {
        public Text timeText;
        public GameObject gates;
        public float timeRemaining = 10;
        public bool timerIsRunning = false;

        public battlescripts bs;

        private bool isArenaTransitioning = false;

        void Awake()
        {
            Debug.Log($"[GateScript] Awake - InstanceID: {GetInstanceID()}, isServer: {isServer}, isClient: {isClient}");
            bs = Player.localPlayer?.GetComponent<battlescripts>();
        }

        void OnEnable()
        {
            Debug.Log($"[GateScript] OnStartServer - InstanceID: {GetInstanceID()}");

            if (timeText == null || gates == null)
            {
                Debug.LogError("[GateScript] timeText or gates is not assigned!");
                return;
            }

            timeText.enabled = true;
            Debug.Log("time text enabled");
            if (!timerIsRunning)
            {
                Debug.Log("timer is running");
                ResetTimer(false);
                StartTimer();
            }
        }

        void OnDisable()
        {
            Debug.Log($"[GateScript] OnDisable - InstanceID: {GetInstanceID()}");
            if (!isServer) return;
            bs?.CmdLobbyEnd();
            timerIsRunning = false;
        }

        

        public void StartTimer()
        {
            if (!isServer) return;

            if (timerIsRunning)
            {
                Debug.LogWarning("[GateScript] Timer already running, skipping start.");
                return;
            }

            Debug.Log("[GateScript] StartTimer invoked on the server.");
            timerIsRunning = true;
            gates.transform.localPosition = Vector3.zero;
            DisplayTime(timeRemaining);
        }

        void Update()
        {
            if (!isServer) return;

            if (timerIsRunning)
            {
                timeRemaining -= Time.deltaTime;

                if (timeRemaining > 0)
                {
                    DisplayTime(timeRemaining);
                    RpcUpdateTime(timeRemaining);
                }
                else
                {
                    timeRemaining = 0;
                    timerIsRunning = false;
                    DisplayTime(timeRemaining);
                    RpcUpdateTime(timeRemaining);
                    RpcDisableTimeText();
                    Debug.Log("[GateScript] Time has run out!");
                    StartCoroutine(MoveGates(gates.transform.up * 100, 3f));
                }
            }
        }

        private void DisplayTime(float timeToDisplay)
        {
            timeToDisplay = Mathf.Max(timeToDisplay, 0);
            float minutes = Mathf.FloorToInt(timeToDisplay / 60);
            float seconds = Mathf.FloorToInt(timeToDisplay % 60);
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        [Server]
        IEnumerator MoveGates(Vector3 targetOffset, float duration)
        {
            Debug.Log("[GateScript] MoveGates started on server.");
            Vector3 startPosition = gates.transform.localPosition;
            Vector3 targetPosition = startPosition + targetOffset; // Move relative to current position
            float elapsed = 0f;

            while (elapsed < duration)
            {
                gates.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            gates.transform.localPosition = targetPosition;
            RpcSyncGatePosition(targetPosition);
        }

        [ClientRpc]
        void RpcSyncGatePosition(Vector3 position)
        {
            Debug.Log("[GateScript] RpcSyncGatePosition called on client.");
            gates.transform.localPosition = position;
        }

        [ClientRpc]
        public void RpcUpdateTime(float time)
        {
            DisplayTime(time);
        }

        [ClientRpc]
        void RpcDisableTimeText()
        {
            Debug.Log("[GateScript] RpcDisableTimeText called on client.");
            timeText.enabled = false;
        }

        public void ResetTimer(bool disableTimerDisplay = true)
        {
            if (!isServer) return;

            Debug.Log("[GateScript] ResetTimer invoked on the server.");
            timeRemaining = 10;
            timerIsRunning = false;

            if (disableTimerDisplay)
            {
                timeText.enabled = false;
            }

            gates.transform.localPosition = Vector3.zero;
        }


        public void PortPlayerToStartZone()
        {
            ResetTimer();
            bs?.CmdPortPlayerToStartZone();
        }
    }
}
