using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManag : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        GameObject clientServerInfo = GameObject.Find("ClientServerInfo");

        if (clientServerInfo != null)
        {
            clientServerInfo.GetComponent<ClientServerInfo>().ChangeUserAndIP();
        }
        else
        {
            Debug.Log("ClientServerInfo not found in the scene.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void CloseApp()
    {
        Application.Quit();
    }
}
