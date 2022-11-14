using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

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
    #endregion

    #region Properties
    public System.Type EntityType { get => entityType; set => entityType = value; }
    public float CreationTime { get => creationTime; set => creationTime = value; }
    public SimulationConstruct MemoryHolder { get => memoryHolder; set => memoryHolder = value; }
    public Dictionary<GameObject, Vector3> InstancesInScene { get => instancesInScene; set => instancesInScene = value; }
    // public List<ExperienceMemory> AssociatedMemories { get => associatedMemories; set => associatedMemories = value; }
    public EntityTraits AssumedTraits { get => assumedTraits; set => assumedTraits = value; }
    public EntityPhysicalAttributes AssumedPhysicalAttributes { get => assumedPhysicalAttributes; set => assumedPhysicalAttributes = value;  }
    public List<EntityEventMemory> PerformableInteractions { get => performableInteractions; set => performableInteractions = value; }
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
        if (!performableInteractions.Contains(memory)) return;
        if (!instancesInScene.ContainsKey(instance)) return;
        EntityTraits originalTraits = memoryHolder.ConstructTraits;
        EntityPhysicalAttributes originalPhysicalAttributes = memoryHolder.ConstructPhysicalAttributes;

        List<object> inputParameters = new List<object>();
        List<System.Reflection.ParameterInfo> generatedParameterInfo = memory.FunctionMethodInfo.GetParameters().ToList<System.Reflection.ParameterInfo>();
        foreach (System.Reflection.ParameterInfo parameter in generatedParameterInfo)
        {
            if (parameter.ParameterType == typeof(SimulationConstruct)) inputParameters.Add(memoryHolder);
            if (parameter.ParameterType == typeof(EntityMemory)) inputParameters.Add(this);
        }
        memory.FunctionMethodInfo.Invoke(instance, inputParameters.ToArray());
        memory.TraitChange = EntityTraits.GenerateTraitDifference(originalTraits, memoryHolder.ConstructTraits);
        memory.PhysicalAttributeChange = EntityPhysicalAttributes.GeneratePhysicalAttributeDifference(originalPhysicalAttributes, memoryHolder.ConstructPhysicalAttributes);

    }
    public void VerifyPresenceOfInstanceInView(GameObject instance)
    {
        if (!memoryHolder.GameObjectView.Contains(instance) && instancesInScene.ContainsKey(instance)) instancesInScene.Remove(instance);
    }
    #endregion
}
