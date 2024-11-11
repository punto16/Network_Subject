using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RotateLoadingUi : MonoBehaviour
{
    Transform goT;

    [Range(-1.0f, 1.0f)]
    public float degreePerFrame = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        goT = GetComponent<Image>().GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        float rot = goT.eulerAngles.z - degreePerFrame;
        goT.rotation = Quaternion.Euler(goT.eulerAngles.x, goT.eulerAngles.y, rot);
    }
}
