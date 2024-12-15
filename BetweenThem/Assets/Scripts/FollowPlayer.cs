using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject followGO;
    public float closeSpeed = 2.0f;
    public float farSpeed = 7.0f;
    public float speed = 5.0f;
    public float closeDistance = 1.0f;
    public float farDistance = 4.0f;
    public float stopDistance = 0.1f;

    private Rigidbody2D phy;
    private Rigidbody2D followPhy;

    void Start()
    {
        phy = gameObject?.GetComponent<Rigidbody2D>();
        followPhy = followGO?.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (phy != null && followPhy != null)
        {
            float distance = Vector2.Distance(phy.position, followPhy.position);
            float currentSpeed;

            if (distance <= stopDistance)
            {
                currentSpeed = 0.0f;
            }
            else if (distance < closeDistance)
            {
                currentSpeed = closeSpeed;
            }
            else if (distance > farDistance)
            {
                currentSpeed = farSpeed;
            }
            else
            {
                currentSpeed = speed;
            }

            Vector2 direction = (followPhy.position - phy.position).normalized;
            phy.velocity = direction * currentSpeed;
        }
    }
}

