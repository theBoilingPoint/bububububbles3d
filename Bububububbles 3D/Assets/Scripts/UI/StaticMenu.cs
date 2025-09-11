using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticMenu : MonoBehaviour
{
    public static StaticMenu Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
}
