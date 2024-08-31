using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    //Singleton
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public GameObject tunnelMaskPrefab;

    public GameObject previousMask;

    public GameObject wormHead;
    public GameObject wormAss;

    public GameObject rootPrefab;

    public GameObject rockPrefab;

    public int poolSize;
    List<GameObject> maskPool;

    public int tunnelLength;
    Queue<GameObject> activeMasks;

    public GameObject foreground;

    public int foodAmount;

    public int rockAmount;

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
        wormAss = WormManager.Instance.wormAss;

        for (int i = 0; i < foodAmount; i++)
        {
            spawnFood();
        }

        for (int i = 0; i < rockAmount; i++)
        {
            spawnRocks();
        }
    }

    // Update is called once per frame
    void Update()
    {
        moveWormHead();
        carveTunnel();
        cleanMasks();
        moveCamera();

        WormManager.Instance.followTheLeader();
    }

    public void carveTunnel()
    {
        //Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 maskTargetPos = wormAss.transform.position;
        if (previousMask == null)
        {
            GameObject newMask = maskPool[0];

            newMask.transform.position = new Vector2(maskTargetPos.x, maskTargetPos.y);
            previousMask = newMask;

            maskPool.Remove(newMask);
            activeMasks.Enqueue(previousMask);
        }
        else if (Vector2.Distance(previousMask.transform.position, maskTargetPos) > .35f)
        {
            GameObject newMask = maskPool[0];

            newMask.transform.position = new Vector2(maskTargetPos.x, maskTargetPos.y);
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

    public void moveCamera()
    {
        Camera.main.transform.position = new Vector3(wormHead.transform.position.x,
                                                     wormHead.transform.position.y,
                                                     Camera.main.transform.position.z);
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
        Vector3 myLocation = wormHead.transform.position;
        Vector3 targetLocation = mousePos;
        targetLocation.z = myLocation.z; // ensure there is no 3D rotation by aligning Z position

        // vector from this object towards the target location
        Vector3 vectorToTarget = targetLocation - myLocation;
        // rotate that vector by 90 degrees around the Z axis
        Vector3 rotatedVectorToTarget = Quaternion.Euler(0, 0, 90) * vectorToTarget;

        // get the rotation that points the Z axis forward, and the Y axis 90 degrees away from the target
        // (resulting in the X axis facing the target)
        Quaternion targetRotation = Quaternion.LookRotation(forward: Vector3.forward, upwards: rotatedVectorToTarget);

        // changed this from a lerp to a RotateTowards because you were supplying a "speed" not an interpolation value
        wormHead.transform.rotation = Quaternion.RotateTowards(wormHead.transform.rotation, targetRotation, 1000 * Time.deltaTime);
    }
}
