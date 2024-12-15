using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Task1Info : MonoBehaviour
{
    private GameObject task;
    public GameObject exclamationSign;

    private bool openTask = false;

    // Start is called before the first frame update
    void Start()
    {
        this.task = gameObject;
        exclamationSign.SetActive(openTask);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //setters
    public void EnableTask(bool b)
    {
        this.openTask = b;
        exclamationSign.SetActive(openTask);
    }

    //getters
    public bool GetEnabledTask()
    {
        return this.openTask;
    }
}
