using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System.Data;
using System.Text;

public class TestClass
{
    public int i;
    public string s;
}

public class readFile : MonoBehaviour
{

    private void Start()
    {
        deserialize();
    }

    void deserialize()
    {

        for (int i = 0; i < 30; i++)
        {
            MemoryStream ms = new MemoryStream();
            using (FileStream fs = File.OpenRead(Application.dataPath + "/Files/clues/clue" + i + ".txt"))
            {
                fs.CopyTo(ms);

                var t = new TestClass();

                BinaryReader reader = new BinaryReader(ms);
                ms.Seek(0, SeekOrigin.Begin);

                t.i = reader.ReadInt32();
                t.s = reader.ReadString();

                Debug.Log("\n------ CLUE " + i + " -----");
                if (t == null)
                {
                    Debug.Log("ERROR\n");
                    continue;
                }
                Debug.Log("INTEGER IS : " + t.i);
                Debug.Log("STRING IS : " + t.s);
                Debug.Log("-----------\n");
            }
        }
    }
}
