using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject tunnelMaskPrefab;

    public GameObject previousMask;

    public GameObject wormHead;

    public GameObject rootPrefab;

    public GameObject rockPrefab;

    public int poolSize;
    List<GameObject> maskPool;

    public int tunnelLength;
    Queue<GameObject> activeMasks;

    public GameObject foreground;

    // Start is called before the first frame update
    void Start()
    {
        maskPool = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            maskPool.Add(Instantiate(tunnelMaskPrefab));
        }

        activeMasks = new Queue<GameObject>();

        WormManager.Instance.spawn();
        wormHead = WormManager.Instance.wormHead;
        spawnFood();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = -10;
        wormHead.transform.position = Vector3.MoveTowards(wormHead.transform.position, mousePos, 5 * Time.deltaTime);
        //close but still has some hangups 
        wormHead.transform.right = new Vector3(mousePos.x, mousePos.y, 0);

        carveTunnel();
        cleanMasks();

        WormManager.Instance.followTheLeader();
    }

    public void carveTunnel()
    {
        //Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 wormHeadPos = wormHead.transform.position;
        if (previousMask == null)
        {
            GameObject newMask = maskPool[0];

            newMask.transform.position = new Vector2(wormHeadPos.x, wormHeadPos.y);
            previousMask = newMask;

            maskPool.Remove(newMask);
            activeMasks.Enqueue(previousMask);
        }
        else if (Vector2.Distance(previousMask.transform.position, wormHeadPos) > .35f)
        {
            GameObject newMask = maskPool[0];

            newMask.transform.position = new Vector2(wormHeadPos.x, wormHeadPos.y);
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

    public void spawnFood()
    {
        Bounds mapBounds = foreground.GetComponent<SpriteRenderer>().bounds;
        float xPosition = UnityEngine.Random.Range(mapBounds.min.x, mapBounds.max.x);
        float yPosition = UnityEngine.Random.Range(mapBounds.min.y, mapBounds.max.y);

        GameObject foodInstance = Instantiate(rootPrefab);
        foodInstance.transform.position = new Vector3(xPosition, yPosition, -1);
    }

    public void spawnRocks()
    {
        Bounds mapBounds = foreground.GetComponent<SpriteRenderer>().bounds;
        float xPosition = UnityEngine.Random.Range(mapBounds.min.x, mapBounds.max.x);
        float yPosition = UnityEngine.Random.Range(mapBounds.min.y, mapBounds.max.y);

        GameObject foodInstance = Instantiate(rockPrefab);
        foodInstance.transform.position = new Vector3(xPosition, yPosition, -1);
    }
}
