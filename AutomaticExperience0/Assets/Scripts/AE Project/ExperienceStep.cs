using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.FilePathAttribute;

[System.Serializable]
//[CreateAssetMenu]



public class ExperienceStep// : ScriptableObject
{
    #region Fields
    [SerializeField] ExperienceStepType stepType = ExperienceStepType.Null;
    SimulationConstruct stepActor;
    [SerializeField] List<System.Type> referencedType = new List<System.Type>();
    [SerializeField] List<SimulationConstruct> referencedSimulationConstruct = new List<SimulationConstruct>();
    [SerializeField] List<EntityMemory> referencedEntityMemory = new List<EntityMemory>();
    [SerializeField] List<EntityEventMemory> referencedEntityEventMemory = new List<EntityEventMemory>();
    [SerializeField] List<GameObject> referencedGameObject = new List<GameObject>();
    [SerializeField] List<ExperienceStepSequence> referencedExperienceStepSequence = new List<ExperienceStepSequence>();
    [SerializeField] EntityTraits referencedTraits = new EntityTraits();
    [SerializeField] EntityPhysicalAttributes referencedPhysicalAttributes = new EntityPhysicalAttributes();
    [SerializeField] List<Vector3> referencedVector3 = new List<Vector3>();
    [SerializeField] List<string> referencedString = new List<string>();

    ExperienceStepEvent stepCompletedEvent = new ExperienceStepEvent();
    ExperienceStepEvent stepReversedEvent = new ExperienceStepEvent();
    #endregion

    #region Properties
    public ExperienceStepType StepType { get => stepType; set => stepType = value; }
    public SimulationConstruct StepActor { get => stepActor; set => stepActor = value; }
    public List<System.Type> ReferencedType { get => referencedType; set => referencedType = value; }
    public List<SimulationConstruct> ReferencedSimulationConstruct { get => referencedSimulationConstruct; set => referencedSimulationConstruct = value; }
    public List<EntityMemory> ReferencedEntityMemory { get => referencedEntityMemory; set => referencedEntityMemory = value; }
    public List<EntityEventMemory> ReferencedEntityEventMemory { get => referencedEntityEventMemory; set => referencedEntityEventMemory = value; }
    public List<GameObject> ReferencedGameObject { get => referencedGameObject; set => referencedGameObject = value; }
    public List<ExperienceStepSequence> ReferencedExperienceStepSequence { get => referencedExperienceStepSequence; set => referencedExperienceStepSequence = value; }
    public ExperienceStepEvent StepCompletedEvent { get => stepCompletedEvent; set => stepCompletedEvent = value; }
    public ExperienceStepEvent StepReversedEvent { get => stepReversedEvent; set => stepReversedEvent = value; }
    public EntityTraits ReferencedTraits { get => referencedTraits; set => referencedTraits = value; }
    public EntityPhysicalAttributes ReferencedPhysicalAttributes { get => referencedPhysicalAttributes; set => referencedPhysicalAttributes = value; }
    public List<Vector3> ReferencedVector3 { get => referencedVector3; set => referencedVector3 = value; }
    public List<string> ReferencedString { get => referencedString; set => referencedString = value; }
    public enum ExperienceStepType
    {
        // MISC Steps
        Null,
        MoveToRandomLocation,
        // Memory Instance Steps
        ScanSurroundingsForInstances,
        FindInstanceofEntityWithMemory,
        ExecuteFunctionInEntityMemory,
        MoveToInstanceofEntityInMemory,
        ExploreUntilFindSomethingNew,
        AddGameObjectToInventory,
        RemoveGameObjectFromInventory,
        ExploreEntityMemoryFunctions,
        // Memory Retrieval Steps
        GetMemoryWithPhysicalAttributeChange,
        GetMemoryWithTraitChange,
        // Social Steps
        FindSimulationConstructWithPhysicalAttributeChange,
        FindSimulationConstructWithTraitChange,
        // Self Aware Steps
        CompleteExperienceStepSequence
    }
    #endregion
    #region Execution Functions
    public void Execute(ExperienceStepType inputtedStepType = ExperienceStepType.Null, bool nextStepCondition = true)
    {
        switch(inputtedStepType)
        {
            case ExperienceStepType.Null:
                {
                    Execute(stepType, true);
                }
                break;
            case ExperienceStepType.MoveToRandomLocation: // Optional Vector3 input.
                {
                    if (referencedVector3.Count > 0) { stepActor.NavigateAgentToPosition(stepActor.transform.position + referencedVector3[Random.Range(0, referencedVector3.Count - 1)]); }
                    else stepActor.NavigateAgentToPosition(stepActor.transform.position + new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f)));
                    if (!nextStepCondition) break;
                    stepActor.DestinationReachedEvent.RemoveAllListeners();
                    stepActor.DestinationReachedEvent.AddListener(ReachDestinationStepCompleteWithVector3);
                }
                break;
            case ExperienceStepType.ScanSurroundingsForInstances: // Intended to follow move to instance of entity in memory.
                // Optional GameObject input to remove instance from memories if not present in object vision or in inventory.
                // Optional SimulationConstruct input in the event there are no instances within the associated memory.
                // Capable of forcing the parented ExperienceStepSequence to recoil if the above conditons occur.
                {
                    // Get the view.
                    List<GameObject> actorView = stepActor.GameObjectView;
                    bool failedToFindInstance = false;

                    // First default to GameObject Scan
                    if (stepActor.ConstructEntities.Count > 0)
                    {
                        foreach (EntityMemory memory in referencedEntityMemory)
                        {
                            if (memory.InstancesInScene.Count == 0) continue;
                            List<GameObject> objectsToRemoveFromMemory = new List<GameObject>();
                            foreach (GameObject subject in referencedGameObject)
                            {
                                if (subject == null) continue;
                                if (memory.InstancesInScene.ContainsKey(subject) &&
                                    !actorView.Contains(subject) &&
                                    !stepActor.ConstructInventory.Contains(subject))
                                {
                                    objectsToRemoveFromMemory.Add(subject); failedToFindInstance = true;
                                }
                            }
                            foreach (GameObject subject in objectsToRemoveFromMemory)
                            {
                                if (subject == null) continue;
                                referencedVector3.Remove(memory.InstancesInScene[subject]);
                                memory.InstancesInScene.Remove(subject);
                                referencedGameObject.Remove(subject);
                            }
                        }
                    }

                    // If there's a referenced simulation construct, use that.
                    if (referencedSimulationConstruct.Count > 0)
                    {
                        bool guidanceRecieved = false;
                        if (actorView.Contains(referencedSimulationConstruct[0].gameObject))
                        {
                            bool associateIsRelevant = false;
                            foreach(EntityMemory memory in referencedEntityMemory)
                            {
                                if (memory.InstancesInScene.Count > 0) continue;
                                var returnedInstances = referencedSimulationConstruct[0].RequestInstanceKnowledge(stepActor, memory.EntityType);
                                if (returnedInstances == null) continue;
                                if (returnedInstances.Count == 0) continue;
                                memory.InstancesInScene.AddRange(returnedInstances);
                                associateIsRelevant = true;
                            }
                            if (!associateIsRelevant) { referencedSimulationConstruct.Remove(referencedSimulationConstruct[0]); }
                            else guidanceRecieved = true;
                        }
                        if (guidanceRecieved && nextStepCondition) { StepReverse(); break; }
                    }

                    if (!nextStepCondition) break;
                    if (failedToFindInstance)
                    {
                        referencedGameObject.Clear();
                        referencedVector3.Clear();

                        StepReverse();
                    }
                    else StepComplete();
                }
                break;
            case ExperienceStepType.FindInstanceofEntityWithMemory: // Takes memory input, Return closest instance and position.
                // If there are no instances recorded, friend data is searched for friends to consult.
                {
                    // Add sequence compatability for referencing sources of knowlege should the present samples prove non-effective, like other SimulationConstructs

                    // establish holder lists
                    List<EntityMemory> relevantMemories = new List<EntityMemory>();
                    List<GameObject> relevantEntities = new List<GameObject>();
                    List<Vector3> relevantPositions = new List<Vector3>();

                    // add relevant memories
                    relevantMemories.AddRange(referencedEntityMemory);
                    if (relevantMemories.Count <= 0) relevantMemories = stepActor.ConstructEntities;

                    // searches the memories.
                    foreach (EntityMemory memory in relevantMemories)
                    {
                        // If there are no recorded instances, continue.
                        if (memory.InstancesInScene.Count <= 0) continue;

                        // First sets it to the closest interest.
                        KeyValuePair<GameObject, Vector3> kvp = memory.FindClosestInstance();

                        // However, if there is an instance in the inventory, it will default to that.
                        foreach (GameObject subject in stepActor.ConstructInventory)
                        {
                            if(memory.InstancesInScene.ContainsKey(subject))
                            {
                                kvp = new KeyValuePair<GameObject, Vector3>(subject, stepActor.transform.position);
                            }
                        }

                        if (!referencedGameObject.Contains(kvp.Key)) relevantEntities.Add(kvp.Key);
                        if (memory.IsAutonomous) continue;
                        if (!referencedVector3.Contains(kvp.Value)) relevantPositions.Add(kvp.Value);
                    }

                    // If no relevant entities can be found, search friends.
                    if (relevantEntities.Count <= 0 || relevantPositions.Count <= 0)
                    {
                        if(referencedSimulationConstruct.Count <= 0) 
                        foreach (SimulationConstructProfile profile in stepActor.KnownOthers)
                        {
                            if (!profile.IsFriend()) continue;
                            // The below is arguably not needed but should reduce overall processing.
                            if (profile.AssociatedSimulationConstruct.RequestMemoryKnowledge(stepActor, relevantMemories[0].EntityType) != null) continue;

                            if(!referencedSimulationConstruct.Contains(profile.AssociatedSimulationConstruct)) referencedSimulationConstruct.Add(profile.AssociatedSimulationConstruct);
                            // referencedGameObject.Add(profile.AssociatedSimulationConstruct.gameObject);
                        }
                    } else { referencedSimulationConstruct.Clear(); }

                    // Add the appropriate ranges.
                    referencedGameObject.AddRange(relevantEntities);
                    referencedVector3.AddRange(relevantPositions);

                    if (!nextStepCondition) break;
                    StepComplete();
                }
                break;
            case ExperienceStepType.ExecuteFunctionInEntityMemory: // Takes passed gameobject, event, and memory.
                {
                    GameObject functionTarget = null;
                    EntityEventMemory memoryEvent = null;
                    List<EntityMemory> memoryPool = new List<EntityMemory>();

                    // Defaults to passed game object.
                    if (referencedGameObject.Count > 0) functionTarget = referencedGameObject[0];

                    // Defaults to passed event.
                    if (referencedEntityEventMemory.Count > 0) { memoryEvent = referencedEntityEventMemory[0]; }

                    // Defaults to passed memory.
                    if (referencedEntityMemory.Count > 0) { memoryPool.AddRange(referencedEntityMemory); }

                    // Otherwise, resorts to all of the actor's owned memories (not efficient)
                    if (memoryPool.Count == 0) memoryPool.AddRange(stepActor.ConstructEntities);

                    // Assigns a chosen memory
                    EntityMemory chosenMemory = EntityMemory.FindMemoryofFunction(memoryPool, memoryEvent);

                    // If there is no game object, defaults to first game object in inventory.
                    if (stepActor.ConstructInventory.Count > 0 && functionTarget == null)
                    {
                        foreach (GameObject subject in stepActor.ConstructInventory)
                        {
                            foreach (EntityMemory memory in memoryPool)
                            {
                                if (memory.InstancesInScene.ContainsKey(subject))
                                {
                                    functionTarget = subject;
                                }
                            }
                        }
                    }

                    // Execute the event to the best of the scripts ability.
                    chosenMemory.ExecuteEntityEventMemory(functionTarget, memoryEvent);
                    
                    if (!nextStepCondition) break;
                    StepComplete();
                }
                break;
            case ExperienceStepType.MoveToInstanceofEntityInMemory: // Takes Vector3 OR Gameobject OR SimulationConstruct
                // Intended to follow find instance of entitiy in memory.
                // Not necessarily required to follow it, though.
                // 
                {
                    // If no Vector3s or GameObjects are defined, one is pulled manually.
                    if (referencedVector3.Count == 0 && referencedGameObject.Count == 0)
                    {
                        Execute(ExperienceStepType.FindInstanceofEntityWithMemory, false);
                    }

                    // If the object is already in the inventory, the following effort is skipped.
                    if (stepActor.ConstructInventory.Count > 0) if (stepActor.ConstructInventory.Contains(referencedGameObject[0])) { StepComplete(); break; }

                    // Defaults to first imputted Vector3
                    if (referencedVector3.Count > 0)
                    {
                        stepActor.DestinationReachedEvent.RemoveAllListeners();
                        stepActor.NavigateAgentToPosition(referencedVector3[0]);
                        stepActor.DestinationReachedEvent.AddListener(ReachDestinationStepCompleteWithVector3);
                    }
                    else if (referencedSimulationConstruct.Count > 0) // Otherwise, uses friend data.
                    {
                        stepActor.DestinationReachedEvent.RemoveAllListeners();
                        stepActor.NavigateAgentToPosition(referencedSimulationConstruct[0].gameObject);
                        stepActor.DestinationReachedEvent.AddListener(ReachDestinationStepCompleteWithVector3);
                    }
                    else if (referencedGameObject.Count > 0) // Otherwise, uses the GameObject which is should only occur with AUTONOMOUS memories.
                    {
                        stepActor.DestinationReachedEvent.RemoveAllListeners();
                        stepActor.NavigateAgentToPosition(referencedGameObject[0]);
                        stepActor.DestinationReachedEvent.AddListener(ReachDestinationStepCompleteWithVector3);
                    }
                    else Execute(stepType, true); // It shouldn't ever reach this spot since if there are no referenced simulation constructs, it recycles.
                }
                break;
            case ExperienceStepType.ExploreUntilFindSomethingNew: // Forces the SimulationConstruct to wander until it finds something new.
                {
                    stepActor.DestinationReachedEvent.RemoveAllListeners();
                    Execute(ExperienceStepType.MoveToRandomLocation, false);
                    stepActor.DestinationReachedEvent.AddListener(ExecuteWithVector3);
                    if (!nextStepCondition) break;
                    stepActor.DiscoveredNewEntityMemoryEvent.AddListener(StepCompleteWithEntityMemory);
                }
                break;
            case ExperienceStepType.AddGameObjectToInventory: // Takes one gameObject and adds it to the construct's inventory
                // This is perhaps the simplest step to complete.
                {
                    stepActor.AddGameObjectToInventory(referencedGameObject[0]);
                    if (!nextStepCondition) break;
                    StepComplete();
                }
                break;
            case ExperienceStepType.RemoveGameObjectFromInventory: // Takes one gameObject and removes it from the construct's inventory
                // This is perhaps the second simplest step to complete.
                {
                    stepActor.RemoveGameObjectFromInventory(referencedGameObject[0]);
                    if (!nextStepCondition) break;
                    StepComplete();
                }
                break;
            case ExperienceStepType.ExploreEntityMemoryFunctions: // Takes a passed memory and optionally a GameObject.
                // Investigates a memory's potential. If already investigated, executes one of the functions discovered.
                {
                    if (referencedEntityMemory[0].EntityType == stepActor.GetType()) { StepComplete(); break; }
                    referencedEntityMemory[0].InvestigateEntityMethods();
                    if (referencedEntityMemory[0].PerformableInteractions.Count > 0 && referencedGameObject.Count > 0)
                    {
                        referencedEntityMemory[0].ExecuteEntityEventMemory(referencedGameObject[0],
                            referencedEntityMemory[0].PerformableInteractions[Random.Range(0, ReferencedEntityMemory[0].PerformableInteractions.Count - 1)]);
                    }
                    if (!nextStepCondition) break;
                    StepComplete();
                }
                break;
            case ExperienceStepType.GetMemoryWithPhysicalAttributeChange: // WARNING : Heavy latency operation!
                // Takes physical attribute difference. Returns most viable memory event and memory.
                {
                    List<PhysicalAttributeNames> positiveTargets = new List<PhysicalAttributeNames>();
                    List<PhysicalAttributeNames> negativeTargets = new List<PhysicalAttributeNames>();
                    Dictionary<EntityEventMemory, float> candidates = new Dictionary<EntityEventMemory, float>();

                    foreach (PhysicalAttributeDataValue attribute in referencedPhysicalAttributes.MeritData)
                    {
                        if (attribute.Intensity > 0) { positiveTargets.Add(attribute.NameAsEnum); }
                        if (attribute.Intensity < 0) { negativeTargets.Add(attribute.NameAsEnum); }
                    }
                    foreach(EntityMemory memory in stepActor.ConstructEntities)
                    {
                        foreach(EntityEventMemory memoryEvent in memory.PerformableInteractions)
                        {
                            foreach(PhysicalAttributeDataValue attribute in memoryEvent.PhysicalAttributeChange.MeritData)
                            {
                                if (positiveTargets.Contains(attribute.NameAsEnum))
                                {
                                    if (!candidates.ContainsKey(memoryEvent)) candidates.Add(memoryEvent, attribute.Intensity);
                                    else candidates[memoryEvent] += attribute.Intensity;
                                }
                                if (negativeTargets.Contains(attribute.NameAsEnum))
                                {
                                    if (!candidates.ContainsKey(memoryEvent)) candidates.Add(memoryEvent, -attribute.Intensity);
                                    else candidates[memoryEvent] += -attribute.Intensity;
                                }
                            }
                        }
                    }
                    var test = candidates.OrderByDescending(key => key.Value);
                    referencedEntityMemory.Clear();
                    foreach (KeyValuePair<EntityEventMemory, float> subject in test)
                    {
                        referencedEntityMemory.Add(EntityMemory.FindMemoryofFunction(stepActor.ConstructEntities, subject.Key));
                        referencedEntityEventMemory.Add(subject.Key);
                    }
                    if (!nextStepCondition) break;
                    StepComplete();
                }
                break;
            case ExperienceStepType.GetMemoryWithTraitChange: // WARNING : Heavy latency operation!
                // Takes trait difference. Returns most viable memory.
                {
                    List<TraitNames> positiveTargets = new List<TraitNames>();
                    List<TraitNames> negativeTargets = new List<TraitNames>();
                    Dictionary<EntityEventMemory, float> candidates = new Dictionary<EntityEventMemory, float>();

                    foreach (TraitDataValue attribute in referencedTraits.MeritData)
                    {
                        if (attribute.Intensity > 0) { positiveTargets.Add(attribute.NameAsEnum); }
                        if (attribute.Intensity < 0) { negativeTargets.Add(attribute.NameAsEnum); }
                    }
                    foreach (EntityMemory memory in stepActor.ConstructEntities)
                    {
                        foreach (EntityEventMemory memoryEvent in memory.PerformableInteractions)
                        {
                            foreach (TraitDataValue attribute in memoryEvent.TraitChange.MeritData)
                            {
                                if (positiveTargets.Contains(attribute.NameAsEnum)) { candidates.Add(memoryEvent, attribute.Intensity); }
                                if (negativeTargets.Contains(attribute.NameAsEnum)) { candidates.Add(memoryEvent, -attribute.Intensity); }
                            }
                        }
                    }
                    var test = candidates.OrderByDescending(key => key.Value);
                    referencedEntityMemory.Clear();
                    foreach (KeyValuePair<EntityEventMemory, float> subject in test)
                    {
                        referencedEntityMemory.Add(EntityMemory.FindMemoryofFunction(stepActor.ConstructEntities, subject.Key));
                        referencedEntityEventMemory.Add(subject.Key);
                    }
                    if (!nextStepCondition) break;
                    StepComplete();
                }
                break;
            case ExperienceStepType.FindSimulationConstructWithPhysicalAttributeChange: // Optionally takes a passed EntityPhysicalAttributes, otherwise defaults to distance.
                // Returns most viable entity.
                {

                }
                break;
            case ExperienceStepType.FindSimulationConstructWithTraitChange: // Optionally takes a passed EntityTraits, otherwise defaults to distance.
                // Returns most viable entity.
                {

                }
                break;
            case ExperienceStepType.CompleteExperienceStepSequence: // Suspends experience until immbedded sequence concludes.
                // Takes an entire experience step sequence.
                {
                    ExperienceStepSequence newSequence = ExperienceStepSequence.CreateCopy(referencedExperienceStepSequence[0]);
                    newSequence.SequenceCompletedEvent.AddListener(StepCompleteWithExperienceStepSequence);
                    newSequence.StartSequence(stepActor);
                }
                break;
        }
    }
    void ExecuteWithVector3(Vector3 location)
    {
        Execute(stepType, false);
    }
    #endregion
    #region Completion Functions
    void StepCompleteWithEntityMemory(EntityMemory memory)
    {
        if(!referencedEntityMemory.Contains(memory)) referencedEntityMemory.Add(memory);
        stepActor.DestinationReachedEvent.RemoveAllListeners();
        stepActor.DiscoveredNewEntityMemoryEvent.RemoveListener(StepCompleteWithEntityMemory);
        StepComplete();
    }
    void ReachDestinationStepCompleteWithVector3(Vector3 location)
    {
        if (!referencedVector3.Contains(location)) return;
        //Debug.Log("Made it to location: " + location.x + " : " + location.y + " : " + location.z);
        StepComplete();
    }
    void StepCompleteWithExperienceStepSequence(ExperienceStepSequence completedSequence)
    {
        ExperienceStepSequence.DestroyImmediate(completedSequence);
        StepComplete();
    }
    void StepCompleteWithBool(bool successful)
    {
        if (successful) { StepComplete(); }
        else Execute(stepType);
    }
    void StepComplete()
    {
        stepCompletedEvent.Invoke(this);
    }
    void StepReverse()
    {
        stepReversedEvent.Invoke(this);
    }
    #endregion

    #region Utility
    public static ExperienceStep CreateCopy(ExperienceStep origin)
    {
        ExperienceStep newStep = new ExperienceStep();
        newStep.StepType = origin.StepType;
        newStep.ReferencedEntityMemory.AddRange(origin.ReferencedEntityMemory);
        newStep.ReferencedEntityEventMemory.AddRange(origin.ReferencedEntityEventMemory);
        newStep.ReferencedExperienceStepSequence.AddRange(origin.ReferencedExperienceStepSequence);
        newStep.ReferencedGameObject.AddRange(origin.ReferencedGameObject);
        newStep.ReferencedTraits = origin.ReferencedTraits;
        newStep.ReferencedPhysicalAttributes = origin.ReferencedPhysicalAttributes;
        newStep.ReferencedSimulationConstruct.AddRange(origin.ReferencedSimulationConstruct);
        newStep.ReferencedString.AddRange(origin.ReferencedString);
        newStep.ReferencedType.AddRange(origin.ReferencedType);
        newStep.ReferencedVector3.AddRange(origin.ReferencedVector3);
        return newStep;
    }
    #endregion

}
