using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameZero
{
    public class GateScript : MonoBehaviour
    {
        public Text timeText;
        public GameObject gates;
        public float timeRemaining = 10;
        public bool timerIsRunning = false;
        public static GateScript gs;
        
        void Awake()
        {
           
            // initialize singleton
            if (gs == null) gs = this;
        }
       
        void OnEnable()
        {
            timeText.enabled = true;
            StartTimer();
        }
        public void StartTimer()
        {
                timerIsRunning = true;
                gates.transform.localPosition = new Vector3(0, 0, 0);
         
        }

        private void Update()
        {
            if (timerIsRunning)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);

                if (timeRemaining <= 0)
                {
                    Debug.Log("Time has run out!");
                    StartCoroutine(MoveGates(new Vector3(0, 100, 0), 3f));
                    timeText.enabled = false;
                    timerIsRunning = false;
                    
                }
                else
                {
                    timeText.enabled = true;
                    gates.transform.localPosition = new Vector3(0, 0, 0);
                }
            }
            else
            {
                timeRemaining = 10;
            }
        }

        private void DisplayTime(float timeToDisplay)
        {
            timeToDisplay += 1;
            float minutes = Mathf.FloorToInt(timeToDisplay / 60);
            float seconds = Mathf.FloorToInt(timeToDisplay % 60);
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        IEnumerator MoveGates(Vector3 targetPosition, float duration)
        {
            Vector3 startPosition = gates.transform.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                gates.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            gates.transform.localPosition = targetPosition;
        }
        public void ResetTimer()
        {
            timeRemaining = 10;
            DisplayTime(timeRemaining);
            timerIsRunning = true;
        }
        public void SetActive(bool active)
        {
            gates.SetActive(active);
        }
    }
}
