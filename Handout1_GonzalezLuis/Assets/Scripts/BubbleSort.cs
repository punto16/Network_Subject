using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Timers;

public class BubbleSort : MonoBehaviour
{
    float[] array;
    float[] array2;
    List<GameObject> mainObjects;
    List<GameObject> mainObjects2;
    public GameObject prefab;
    public GameObject prefab2;

    bool sorted = false;
    bool sorted2 = false;
    bool changes = true;
    bool changes2 = true;

    void Start()
    {
        mainObjects = new List<GameObject>();
        array = new float[30000];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = (float)Random.Range(0, 1000) / 100;
        }

        mainObjects2 = new List<GameObject>();
        array2 = new float[30000];
        for (int i = 0; i < array2.Length; i++)
        {
            array2[i] = (float)Random.Range(0, 1000) / 100;
        }

        //TO DO 4
        //Call the three previous functions in order to set up the exercise
        spawnObjs(true);

        logArray(true);

        updateHeights(true);


        spawnObjs(false);

        logArray(false);

        updateHeights(false);
        //TO DO 5
        //Create a new thread using the function "bubbleSort" and start it.


        Thread thread = new Thread(bubbleSort);
        thread.Start();

        Thread bogoThread = new Thread(bogoSort);
        bogoThread.Start();

    }

    void Update()
    {
        //TO DO 6
        //Call ChangeHeights() in order to update our object list.
        //Since we'll be calling UnityEngine functions to retrieve and change some data,
        //we can't call this function inside a Thread
        updateHeights(true);
        updateHeights(false);
    }

    void bogoSort()
    {
        System.Timers.Timer timer = new System.Timers.Timer();
        timer.Start();
        Debug.Log("Bogo sort Start");

        while (!sorted2)
        {
            shuffleArray();
            if (sortedArray()) sorted2 = true;
        }


        //sorted2 = true;
        timer.Stop();
        Debug.Log("Bogo sort finished at " + timer.ToString());
        //You may debug log your Array here in case you want to. It will only be called one the bubble algorithm has finished sorting the array
        logArray(false);
    }

    void shuffleArray()
    {
        System.Random rand = new System.Random();

        for (int i = array2.Length - 1; i > 0; i--)
        {
            int randomIndex = rand.Next(0, i + 1);

            float temp = array2[i];
            array2[i] = array2[randomIndex];
            array2[randomIndex] = temp;
        }
    }

    bool sortedArray()
    {
        for (int i = 0; i < array2.Length - 1; i++)
        {
            if (array2[i] > array2[i + 1])
            {
                return false;
            }
        }
        return true;
    }

    //TO DO 5
    //Create a new thread using the function "bubbleSort" and start it.
    void bubbleSort()
    {
        System.Timers.Timer timer = new System.Timers.Timer();
        timer.Start();
        Debug.Log("Bubble sort Start");
        int i, j;
        int n = array.Length;
        bool swapped;
        for (i = 0; i < n - 1; i++)
        {
            swapped = false;
            for (j = 0; j < n - i - 1; j++)
            {
                if (array[j] > array[j + 1])
                {
                    (array[j], array[j + 1]) = (array[j + 1], array[j]);
                    swapped = true;
                    changes = true;
                }
            }
            if (swapped == false)
                break;
        }
        sorted = true;
        timer.Stop();
        Debug.Log("Bubble sort finished at " + timer);
        //You may debug log your Array here in case you want to. It will only be called one the bubble algorithm has finished sorting the array
        logArray(true);
    }

    void logArray(bool bubblesort)
    {
        string text = "";

        //TO DO 1
        //Simply show in the console what's inside our array.

        for (int i = 0; i < (bubblesort ? array.Length : array2.Length); i++)
        {
            text += bubblesort ? array[i].ToString() : array2[i].ToString();
        }

        Debug.Log(text);
    }

    void spawnObjs(bool bubblesort)
    {
        //TO DO 2
        //We should be storing our objects in a list so we can access them later on.

        for (int i = 0; i < (bubblesort ? array.Length : array2.Length); i++)
        {
            //We have to separate the objs accordingly to their width, in which case we divide their position by 1000.
            //If you decide to make your objs wider, don't forget to up this value
            if (bubblesort)
                mainObjects.Add(Instantiate(prefab, new Vector3((float)i / 1000,
                15, 0), Quaternion.identity));
            else
                mainObjects2.Add(Instantiate(prefab2, new Vector3((float)i / 1000,
                5, 0), Quaternion.identity));
        }
    }

    //TO DO 3
    //We'll just change the height of every obj in our list to match the values of the array.
    //To avoid calling this function once everything is sorted, keep track of new changes to the list.
    //If there weren't, you might as well stop calling this function

    bool updateHeights(bool bubblesort)
    {
        if (bubblesort)
        {
            if (sorted) return false;
            if (!changes) return false;
        }
        else
        {
            if (sorted2) return false;
            if (!changes2) return false;
        }

        Debug.Log("Start Updating Height // " + (bubblesort ? "Bubble Sort" : "Bogo Sort"));

        bool changed = false;
        for (int i = 0; i < (bubblesort ? array.Length : array2.Length); i++)
        {
            if (bubblesort)
                if (mainObjects[i].transform.localScale.y != array[i])
                {
                    mainObjects[i].transform.localScale = new Vector3(mainObjects[i].transform.localScale.x, array[i], mainObjects[i].transform.localScale.z);
                    changed = true;
                }
            if (!bubblesort)
                if (mainObjects2[i].transform.localScale.y != array2[i])
                {
                    mainObjects2[i].transform.localScale = new Vector3(mainObjects2[i].transform.localScale.x, array2[i], mainObjects2[i].transform.localScale.z);
                    changed = true;
                }
        }
        Debug.Log("Ending Updating Height");
        return changed;
    }
}