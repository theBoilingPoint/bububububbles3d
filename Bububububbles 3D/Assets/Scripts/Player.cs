using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;
        BubblesManager mgr = FindObjectOfType<BubblesManager>();
        if (mgr == null)
        {
            throw new System.Exception("BubblesManager is not found!");
        }
        
        if (other.CompareTag("NormalBubble"))
        {
            ProgressBarFill.Instance.UpdateCurrentScore(10);
            mgr.RemoveBubble(other.gameObject);
            Destroy(other.gameObject);
        }

        if (other.CompareTag("AddTimeBubble"))
        {
            Timer.Instance.AddTime(5);
            mgr.RemoveBubble(other.gameObject);
            Destroy(other.gameObject);
        }
        
        if (other.CompareTag("DangerBubble"))
        {
            ProgressBarFill.Instance.UpdateCurrentScore(-5);
            mgr.RemoveBubble(other.gameObject);
            Destroy(other.gameObject);
        }
    }
}
