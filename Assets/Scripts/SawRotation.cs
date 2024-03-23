using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SawRotation : MonoBehaviour
{
    private void FixedUpdate()
    {
        transform.Rotate(0, 0, 5 * 1);

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            PlayerMovement playerMovement = collision.gameObject.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                Debug.Log("Collision detected with player.");
                playerMovement.health -= 150;
                Debug.Log("Player health reduced by 150. Current health: " + playerMovement.health);
            }
            else
            {
                Debug.LogWarning("PlayerMovement component not found on the player GameObject.");
            }
        }
    }
}
