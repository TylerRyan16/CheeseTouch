using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private GameObject player;
    private PlayerManager playerController;

    public Vector3 offset; 
    public float smoothSpeed = 0.125f;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerController = player.GetComponent<PlayerManager>();
    }

    void Update()
    {
        FollowPlayer();  
    }


    void FollowPlayer()
    {
        Vector3 targetPosition = playerController.GetPlayerLocation() + offset;
        targetPosition.z = -10f;

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
