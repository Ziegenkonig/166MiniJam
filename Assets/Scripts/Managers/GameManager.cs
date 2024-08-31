using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;

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
        spawnRocks();
    }

    // Update is called once per frame
    void Update()
    {
        moveWormHead();
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

    public void moveWormHead()
    {
        // get position of mouse and set z to -10 to prevent worm from disappearing over time
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos = new Vector3(mousePos.x, mousePos.y, -10);

        //move towards mouse
        wormHead.transform.position = Vector3.MoveTowards(wormHead.transform.position, mousePos,
                WormManager.Instance.speed * Time.deltaTime);

        //rotate to face mouse
        Vector3 direction = mousePos - wormHead.transform.position;
        direction = new Vector3(direction.x, direction.y, wormHead.transform.position.z);
        wormHead.transform.LookAt(Vector3.forward, Vector3.Cross(Vector3.forward, direction));
        wormHead.transform.rotation = new Quaternion(0, 0, wormHead.transform.rotation.z, wormHead.transform.rotation.w);
    }
}
