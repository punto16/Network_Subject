using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExclamationBounce : MonoBehaviour
{
    public float bounceAmplitude = 0.2f;
    public float bounceSpeed = 3f;

    private Vector3 initialPosition;

    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        float bounce = Mathf.Sin(Time.time * bounceSpeed) * bounceAmplitude;
        transform.localPosition = new Vector3(initialPosition.x, initialPosition.y + bounce, initialPosition.z);
    }
}
