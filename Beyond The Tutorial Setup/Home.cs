using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Home : MonoBehaviour
{
    public GameObject HomeFrog;
    private void OnTriggerEnter2D(Collider2D other) // Some other collider has entered your trigger zone
    {
        if(other.tag == "Player")
        {
            enabled = true; // Set Home then respawn frogger. for enabled = true, certain functions aren't called when behaviors are disabled like but the OnTriggerEnter2D still runs even if the script is disabled
            GameManager.Instance.HomeOccupied();
        }
    }
    private void OnEnable()
    {
        HomeFrog.SetActive(true);
    }
    private void OnDisable()
    {
        HomeFrog.SetActive(false);
    }
}
