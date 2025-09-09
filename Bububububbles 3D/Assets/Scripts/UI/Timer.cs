using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    public static Timer Instance { get; private set; }

    [SerializeField] private float defaultTime = 60f;
    [SerializeField] private TextMeshProUGUI text;
    
    private float time; 

    private void Awake()
    {
        // enforce singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        time = defaultTime;
        
        DontDestroyOnLoad(gameObject); 
    }
    
    void Update()
    {
        if (time > 0f)
        {
            time -= Time.deltaTime;
            if (time < 0f) time = 0f;
        }

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        text.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public float GetTime() => time;
    public void AddTime(float amount) => time += amount;
    public void ResetTime() => time = defaultTime;
}
