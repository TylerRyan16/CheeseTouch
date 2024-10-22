using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private GameObject player;
    public float moveSpeed = 5f;


    private void Start()
    {
        player = GameObject.FindWithTag("Player");    
    }

    // Update is called once per frame
    void Update()
    {
        // get player position
        Vector2 playerPosition = GetPlayerPosition();

        // move enemy to player position
        MoveTowardsPlayer(playerPosition);
    }

    public Vector2 GetPlayerPosition()
    {
        return player.transform.position;
    }

    public void MoveTowardsPlayer(Vector2 playerPosition)
    {
        transform.position = Vector2.MoveTowards(transform.position, playerPosition, moveSpeed * Time.deltaTime);
    }
}
