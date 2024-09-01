using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
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

    public GameObject rootPrefab;

    public GameObject bonePrefab;

    public GameObject rockPrefab;

    public GameObject wormDenPrefab;

    public GameObject upgradePointCounter;

    public int poolSize;
    List<GameObject> maskPool;

    public GameObject previousMask;
    public int tunnelLength;
    Queue<GameObject> activeMasks;

    public SpriteMask fullTunnelMask;
    public Texture2D tunnelMaskTexture;

    public GameObject foreground;
    public GameObject wormHead;
    public GameObject wormAss;

    public AudioClip edibleAudio;

    public int foodAmount;

    public int rockAmount;


    public bool isMoving = true;

    public AudioSource wormAudio;
    public AudioSource gameAudio;
    public float globalVolume;
    public bool isMuted = false;

    // Start is called before the first frame update
    void Start()
    {
        Instantiate(wormDenPrefab);

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

        tunnelMaskTexture = new Texture2D(1000, 1000);
        Color fillColor = new Color(0, 0, 0, 0);
        Color[] fillPixels = new Color[tunnelMaskTexture.width * tunnelMaskTexture.height];
        for (int i = 0; i < fillPixels.Length; i++)
        {
            fillPixels[i] = fillColor;
        }
        tunnelMaskTexture.SetPixels(fillPixels);
        tunnelMaskTexture.Apply();


        gameAudio = gameObject.GetComponent<AudioSource>();
        wormAudio = WormManager.Instance.gameObject.GetComponent<AudioSource>();
        gameAudio.volume = globalVolume;
        wormAudio.volume = globalVolume;
        gameAudio.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving)
        {
            moveWormHead();
            carveTunnelMain();
            cleanMasks();
            moveCamera();

            WormManager.Instance.followTheLeader();
        }

        upgradePointCounter.GetComponent<TextMeshProUGUI>().text = WormManager.Instance.upgradePoints.ToString();
    }

    public void carveTunnelMain()
    {
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
            GameObject oldMask = activeMasks.Dequeue();
            tunnelMaster(oldMask.transform.position);
            maskPool.Add(oldMask);
        }
    }

    public void moveCamera()
    {
        Camera.main.transform.position = new Vector3(wormHead.transform.position.x,
                                                     wormHead.transform.position.y,
                                                     -20);
    }

    public void spawnFood()
    {
        Bounds mapBounds = foreground.GetComponent<SpriteRenderer>().bounds;
        float xPosition = UnityEngine.Random.Range(mapBounds.min.x, mapBounds.max.x);
        float yPosition = UnityEngine.Random.Range(mapBounds.min.y, mapBounds.max.y);
        float edibleScale = UnityEngine.Random.Range(0.15f, 0.5f);
        GameObject foodInstance;

        int foodType = Random.Range(0, 2);

        if (foodType == 0)
        {
            foodInstance = Instantiate(rootPrefab);
        }
        else 
        {
            foodInstance = Instantiate(bonePrefab);
        }

        Edible edibleClass = foodInstance.GetComponent<Edible>();
        edibleClass.scaleEdible(edibleScale);
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
        wormHead.transform.position += wormHead.transform.right * Time.deltaTime * 5;
        wormHead.transform.position = new Vector3(wormHead.transform.position.x, 
                                                  wormHead.transform.position.y, 
                                                  -1);


        //rotate to face mouse
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos = new Vector3(mousePos.x, mousePos.y, -1);

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
        wormHead.transform.rotation = Quaternion.RotateTowards(wormHead.transform.rotation, targetRotation, 75 * Time.deltaTime);
    }

    public void tunnelMaster(Vector3 maskPos)
    {
        SpriteRenderer foregroundRender = foreground.GetComponent<SpriteRenderer>();
        float mapDimension = foregroundRender.bounds.max.x - foregroundRender.bounds.min.x;
        Vector3 mapMin = foregroundRender.bounds.min;
        float xPercent = Vector3.Distance(new Vector3(mapMin.x, 0, 0), new Vector3(maskPos.x, 0, 0)) / mapDimension;
        float yPercent = Vector3.Distance(new Vector3(mapMin.y, 0, 0), new Vector3(maskPos.y, 0, 0)) / mapDimension;

        int maskCenterX = Mathf.RoundToInt( xPercent * tunnelMaskTexture.width );
        int maskCenterY = Mathf.RoundToInt( yPercent * tunnelMaskTexture.height );

        tunnelMaskTexture = updateTunnelMask(maskCenterX, maskCenterY);
        applyTextureAndSprite();
    }

    public void applyTextureAndSprite()
    {
        tunnelMaskTexture.Apply();

        Sprite updatedTunnelSprite = Sprite.Create(tunnelMaskTexture,
                                                   new Rect(0, 0, tunnelMaskTexture.width, tunnelMaskTexture.height),
                                                   new Vector2(0.5f, 0.5f));

        fullTunnelMask.sprite = updatedTunnelSprite;
    }

    public Texture2D updateTunnelMask(int x, int y, int radius = 7)
    {
        float rSquared = radius * radius;

        for (int u = x - radius; u < x + radius + 1; u++)
            for (int v = y - radius; v < y + radius + 1; v++)
                if ((x - u) * (x - u) + (y - v) * (y - v) < rSquared)
                    tunnelMaskTexture.SetPixel(u, v, new Color(255, 255, 255, 255));

        return tunnelMaskTexture;
    }

    public void lowerVolume()
    {
        if (globalVolume > 0)
        {
            globalVolume -= .05f;
            wormAudio.volume = globalVolume;
            gameAudio.volume = globalVolume;
        }
    }

    public void raiseVolume()
    {
        if (globalVolume < 1)
        {
            globalVolume += .05f;
            wormAudio.volume = globalVolume;
            gameAudio.volume = globalVolume;
        }
    }

    public void toggleMute()
    {
        if (isMuted)
        {
            isMuted = false;
            wormAudio.mute = false;
            gameAudio.mute = false;
        }
        else
        {
            isMuted = true;
            wormAudio.mute = true;
            gameAudio.mute = true;
        }
    }
}
