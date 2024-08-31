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
    public float segmentPadding;
    public int upgradePoints;
    public int currentEnergy;
    
    public GameObject wormHeadPrefab;
    public GameObject wormBandPrefab;
    public GameObject wormBodyPrefab;
    public GameObject wormAssPrefab;

    public GameObject wormHead;
    public GameObject wormAss;

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
        wormAss = Instantiate(wormAssPrefab);
        segments.Enqueue(wormAss);
        wormHead.transform.position = Vector3.zero;
        followTheLeader();
    }

    public void followTheLeader()
    {
        GameObject lastWormSegment = segments.Dequeue();
        segments.Enqueue(lastWormSegment);
        for (int i = 0; i < wormLength - 1; i++)
        {
            GameObject currentWormSegment = segments.Dequeue();
            float distance = Vector3.Distance(currentWormSegment.transform.position, lastWormSegment.transform.position);
            if (distance > segmentPadding) 
            {
                currentWormSegment.transform.position = Vector3.MoveTowards(currentWormSegment.transform.position, lastWormSegment.transform.position, speed * Time.deltaTime);
                
                //Vector3 direction = lastWormSegment.transform.position - currentWormSegment.transform.position;
                //currentWormSegment.transform.LookAt(Vector3.forward, Vector3.Cross(Vector3.forward,direction));
                //currentWormSegment.transform.rotation = new Quaternion(0, 0, currentWormSegment.transform.rotation.z, currentWormSegment.transform.rotation.w);

                //rotate to face the previous segment
                Vector3 myLocation = currentWormSegment.transform.position;
                Vector3 targetLocation = lastWormSegment.transform.position;
                targetLocation.z = myLocation.z; // ensure there is no 3D rotation by aligning Z position

                // vector from this object towards the target location
                Vector3 vectorToTarget = targetLocation - myLocation;
                // rotate that vector by 90 degrees around the Z axis
                Vector3 rotatedVectorToTarget = Quaternion.Euler(0, 0, 90) * vectorToTarget;

                // get the rotation that points the Z axis forward, and the Y axis 90 degrees away from the target
                // (resulting in the X axis facing the target)
                Quaternion targetRotation = Quaternion.LookRotation(forward: Vector3.forward, upwards: rotatedVectorToTarget);

                // changed this from a lerp to a RotateTowards because you were supplying a "speed" not an interpolation value
                currentWormSegment.transform.rotation = Quaternion.RotateTowards(currentWormSegment.transform.rotation, targetRotation, 1000 * Time.deltaTime);
            }
            
            lastWormSegment = currentWormSegment;
            segments.Enqueue(currentWormSegment);
        }
    }

    public void eatEdible(int energyGain, int upgradeGain)
    {
        upgradePoints += upgradeGain;
        currentEnergy += energyGain;

        Debug.Log("Eaten");
        // increase worm energy and upgrade points. maybe run animation
    }

    void die()
    {
        // destroys all segments of the worm but does not erase this class because we need to keep the values. so just blow up the segments and spawn()
    }

}
