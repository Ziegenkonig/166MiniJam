using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edible : MonoBehaviour
{
    public int energyGain;
    public int upgradeGain;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Trigger when worm head hits roots/food, deleting it from the game scene
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(gameObject);

        WormManager.Instance.eatEdible(energyGain, upgradeGain);
    }

}
