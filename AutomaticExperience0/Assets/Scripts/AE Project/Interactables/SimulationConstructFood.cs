using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationConstructFood : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Consume(SimulationConstruct construct, EntityMemory associatedMemory)
    {
        associatedMemory.InstancesInScene.Remove(gameObject);
        construct.ConstructPhysicalAttributes.GetPhysicalAttribute(PhysicalAttributeNames.hunger).Intensity += 25;
        construct.RemoveGameObjectFromInventory(gameObject);
        Destroy(gameObject);
    }
}
