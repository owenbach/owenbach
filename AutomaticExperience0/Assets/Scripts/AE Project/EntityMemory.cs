using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class EntityMemory
{
    #region Fields
    [SerializeField] System.Type entityType;
    [SerializeField] float creationTime;
    [SerializeField] SimulationConstruct memoryHolder;
    [SerializeField] Dictionary<GameObject, Vector3> instancesInScene = new Dictionary<GameObject, Vector3>();
    [SerializeField] EntityTraits assumedTraits;
    [SerializeField] EntityPhysicalAttributes assumedPhysicalAttributes;
    [SerializeField] List<EntityEventMemory> performableInteractions = new List<EntityEventMemory>();
    EntityEventMemoryEvent finishedExecutingEvent = new EntityEventMemoryEvent();
    #endregion

    #region Properties
    public System.Type EntityType { get => entityType; set => entityType = value; }
    public float CreationTime { get => creationTime; set => creationTime = value; }
    public SimulationConstruct MemoryHolder { get => memoryHolder; set => memoryHolder = value; }
    public Dictionary<GameObject, Vector3> InstancesInScene { get => instancesInScene; set => instancesInScene = value; }
    public EntityTraits AssumedTraits { get => assumedTraits; set => assumedTraits = value; }
    public EntityPhysicalAttributes AssumedPhysicalAttributes { get => assumedPhysicalAttributes; set => assumedPhysicalAttributes = value;  }
    public List<EntityEventMemory> PerformableInteractions { get => performableInteractions; set => performableInteractions = value; }
    public EntityEventMemoryEvent FinishedExecutingEvent { get => finishedExecutingEvent; set => finishedExecutingEvent = value; }
    #endregion

    #region Utility
    public void InvestigateEntityMethods()
    {
        foreach(var method in entityType.GetMethods())
        {
            if (EntityMemoryManager.UnneccessaryTypes.Contains(method.Name)) continue;
            bool alreadyRecorded = false;
            foreach (EntityEventMemory subject in performableInteractions)
            {
                if (subject.FunctionMethodInfo == method) alreadyRecorded = true;
            }
            if (alreadyRecorded) continue;
            EntityEventMemory newMemory = new EntityEventMemory();
            newMemory.Name = method.Name;
            Debug.Log(newMemory.Name);
            newMemory.FunctionMethodInfo = method;
            performableInteractions.Add(newMemory);
        }
    }
    public void ExecuteEntityEventMemory(GameObject instance, EntityEventMemory memory)
    {
        memoryHolder.StartCoroutine(ReviewEntityEventMemory(instance, memory));
    }
    public IEnumerator ReviewEntityEventMemory(GameObject instance, EntityEventMemory memory)
    {
        if (!performableInteractions.Contains(memory)) yield break;
        if (!instancesInScene.ContainsKey(instance)) yield break;
        EntityTraits originalTraits = EntityTraits.CopyTraits(memoryHolder.ConstructTraits);
        EntityPhysicalAttributes originalPhysicalAttributes = EntityPhysicalAttributes.CopyPhysicalAttributes(memoryHolder.ConstructPhysicalAttributes);

        
        System.Reflection.ParameterInfo[] generatedParameterInfo = memory.FunctionMethodInfo.GetParameters();
        object[] inputParameters = new object[generatedParameterInfo.Count()];
        int count = 0;
        foreach (System.Reflection.ParameterInfo parameter in generatedParameterInfo)
        {
            if (parameter.ParameterType == typeof(SimulationConstruct)) inputParameters[count] = (memoryHolder);
            if (parameter.ParameterType == typeof(EntityMemory)) inputParameters[count] = (this);
            if (parameter.ParameterType == typeof(EntityTraits)) inputParameters[count] = (memoryHolder.ConstructTraits);
            if (parameter.ParameterType == typeof(EntityPhysicalAttributes)) inputParameters[count] = (memoryHolder.ConstructPhysicalAttributes);
            count++;
        }
        memory.FunctionMethodInfo.Invoke(instance.GetComponent(entityType), inputParameters);
        // memory.FunctionMethodInfo.InvokeOptimized(instance, inputParameters);

        yield return new WaitForSeconds(1f);

        memory.TraitChange = EntityTraits.GenerateTraitDifference(originalTraits, memoryHolder.ConstructTraits);
        memory.PhysicalAttributeChange = EntityPhysicalAttributes.GeneratePhysicalAttributeDifference(originalPhysicalAttributes, memoryHolder.ConstructPhysicalAttributes);
        finishedExecutingEvent.Invoke(memory);
        yield break;
    }
    public KeyValuePair<GameObject, Vector3> FindClosestInstance()
    {
        if (instancesInScene.Count <= 0) return new KeyValuePair<GameObject, Vector3>(null, Vector3.zero);
        KeyValuePair<GameObject, Vector3> chosenInstance = instancesInScene.ToArray()[0];
        foreach (KeyValuePair<GameObject, Vector3> pair in instancesInScene)
        {
            if (Vector3.Distance(memoryHolder.transform.position, pair.Value) <= Vector3.Distance(memoryHolder.transform.position, chosenInstance.Value)) { chosenInstance = pair; }
        }
        return chosenInstance;
    }
    public static EntityMemory FindMemoryofFunction(List<EntityMemory> memories, EntityEventMemory action)
    {
        EntityMemory memory = null;
        foreach(EntityMemory mem in memories)
        {
            if (mem.PerformableInteractions.Contains(action)) memory = mem;
        }
        return memory;
    }
    #endregion
}
public class EntityMemoryEvent : UnityEvent<EntityMemory> { }
