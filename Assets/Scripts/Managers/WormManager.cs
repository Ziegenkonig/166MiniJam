using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public float energyDrainRate;
    public int wormLength;
    public float segmentPadding;
    public int upgradePoints;
    public float currentEnergy;
    public int maxEnergyUpgradeAmount;
    public int speedUpgradeAmount;
    public float energyDecayUpgradeAmount;
    
    public GameObject wormHeadPrefab;
    public GameObject wormBandPrefab;
    public GameObject wormBodyPrefab;
    public GameObject wormAssPrefab;

    public AudioClip explosion;
    public AudioClip crash;

    public GameObject wormHead;
    public GameObject wormAss;

    public Queue<GameObject> segments;
    public bool isDead;
    private DateTime oldTime;
    public GameObject deathCanvas;
    public GameObject denCanvas;
    private DateTime lastDecayTime;

    public Image energyBarFill;

    void Start()
    {
        lastDecayTime = DateTime.Now;
        currentEnergy = maxEnergy;
        segments = new Queue<GameObject>();
        oldTime = DateTime.Now;
        isDead = false;
    }

    private void Update()
    {
        energyDecay();

        if (isDead)
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            TimeSpan interval = DateTime.Now - oldTime;
            if (segments.Count > 0 && interval.TotalSeconds > 0.5 && !deathCanvas.activeSelf)
            {
                audioSource.PlayOneShot(explosion, 0.7f);
                Destroy(segments.Dequeue());
                oldTime = DateTime.Now;
            }
            else if (segments.Count == 0 && !deathCanvas.activeSelf)
            {
                GameManager.Instance.mainCamera.SetActive(false);
                GameManager.Instance.birdsEyeCamera.SetActive(true);
                deathCanvas.SetActive(true);
                isDead = false;
            }
        }

        energyBarFill.fillAmount = Mathf.Lerp(energyBarFill.fillAmount, ((float)currentEnergy / maxEnergy), 2 * Time.deltaTime);
    }

    public void spawn()
    {
        deathCanvas.SetActive(false);
        denCanvas.SetActive(false);

        wormHead = Instantiate(wormHeadPrefab);
        segments.Enqueue(wormHead);
        GameManager.Instance.wormHead = wormHead;
        wormHead.transform.position = Vector3.zero;

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
        GameManager.Instance.wormAss = wormAss;
        
        GameManager.Instance.isMoving = true;
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

        if (currentEnergy > maxEnergy)
        {
            currentEnergy = maxEnergy;
        }
    }

    public void die()
    {
        GameManager.Instance.mainHudCanvas.SetActive(false);
        GameManager.Instance.gameAudio.Stop();
        GetComponent<AudioSource>().PlayOneShot(crash);
        GameManager.Instance.isMoving = false;
        isDead = true;
        currentEnergy = 0;
    }

    public void resetWormStuff()
    {
        isDead = false;
        deathCanvas.SetActive(false);
        denCanvas.SetActive(true);
        currentEnergy = maxEnergy;
    }

    public void energyDecay()
    {
        TimeSpan timeSinceLastDecay = DateTime.Now - lastDecayTime;

        if (timeSinceLastDecay.TotalSeconds >= 1)
        {
            currentEnergy -= energyDrainRate;

            lastDecayTime = DateTime.Now;
        }

        if (currentEnergy <= 0 && !isDead && !deathCanvas.activeSelf)
        {
            die();
        }
    }

    public void maxEnergyUpgrade()
    {
        maxEnergy += maxEnergyUpgradeAmount;
    }

    public void speedUpgrade()
    {
        speed += speedUpgradeAmount;
    }

    public void energyDecayUpgrade()
    {
        energyDrainRate -= energyDecayUpgradeAmount;
    }

}
