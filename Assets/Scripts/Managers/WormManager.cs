using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormManager : MonoBehaviour
{
    public static WormManager Instance;

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

    public int speed;
    public int maxEnergy;
    public int energyDrainRate;
    public int wormLength;
    public int segmentPadding;
    public int upgradePoints;
    
    public GameObject wormHeadPrefab;
    public GameObject wormBandPrefab;
    public GameObject wormBodyPrefab;
    public GameObject wormAssPrefab;

    public GameObject wormHead;

    private Queue<GameObject> segments;
    void Start()
    {
        segments = new Queue<GameObject>();
    }

    public void spawn()
    {
        wormHead = Instantiate(wormHeadPrefab);
        segments.Enqueue(wormHead);
        for (int i = 0; i < wormLength - 2; i++)
        {
            if (i == Mathf.Round(wormLength * .2f))
            {
                GameObject wormBand = Instantiate(wormBandPrefab);
                segments.Enqueue(wormBand);
            } else
            {
                GameObject wormBody = Instantiate(wormBodyPrefab);
                segments.Enqueue(wormBody);
            }
        }
        GameObject wormAss = Instantiate(wormAssPrefab);
        segments.Enqueue(wormAss);
        wormHead.transform.position = Vector3.zero;
        followTheLeader();
    }

    public void followTheLeader()
    {
        GameObject lastWormSegment = segments.Dequeue();
        segments.Enqueue(lastWormSegment);
        for (int i = 0; i < wormLength - 2; i++)
        {
            GameObject currentWormSegment = segments.Dequeue();
            float distance = Vector3.Distance(currentWormSegment.transform.position, lastWormSegment.transform.position);
            if (distance > segmentPadding) 
            {
                currentWormSegment.transform.position = Vector3.MoveTowards(currentWormSegment.transform.position, lastWormSegment.transform.position, speed * Time.deltaTime);
            }
            lastWormSegment = currentWormSegment;
            segments.Enqueue(currentWormSegment);
        }
    }

    void eatEdible()
    {
        // increase worm energy and upgrade points. maybe run animation
    }

    void die()
    {
        // destroys all segments of the worm but does not erase this class because we need to keep the values. so just blow up the segments and spawn()
    }
}
