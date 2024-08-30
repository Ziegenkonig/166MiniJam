using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using UnityEngine;

public class Fore : MonoBehaviour
{
    public GameObject tunnelMaskPrefab;

    public GameObject previousMask;

    public int poolSize;
    List<GameObject> maskPool;

    public int tunnelLength;
    Queue<GameObject> activeMasks;

    // Start is called before the first frame update
    void Start()
    {
        maskPool = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            maskPool.Add(Instantiate(tunnelMaskPrefab));
        }

        activeMasks = new Queue<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        colorPixel();
        cleanMasks();
    }

    public void colorPixel()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (previousMask == null)
        {
            GameObject newMask = maskPool[0];

            newMask.transform.position = new Vector2(mousePos.x, mousePos.y);
            previousMask = newMask;

            maskPool.Remove(newMask);
            activeMasks.Enqueue(previousMask);
        }
        else if (Vector2.Distance(previousMask.transform.position, mousePos) > .35f)
        {
            GameObject newMask = maskPool[0];

            newMask.transform.position = new Vector2(mousePos.x, mousePos.y);
            previousMask = newMask;

            maskPool.Remove(newMask);
            activeMasks.Enqueue(previousMask);
        }
        
    }

    public void cleanMasks()
    {
        if (activeMasks.Count >= tunnelLength)
        {
            maskPool.Add(activeMasks.Dequeue());
        }
    }
}
