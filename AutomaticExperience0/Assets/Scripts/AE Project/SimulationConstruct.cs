using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[System.Serializable]
public class SimulationConstruct : MonoBehaviour
{
    #region Fields
    // store time
    float timeElapsedSinceInstantiation = 0;

    // store vision
    [SerializeField] List<GameObject> gameObjectView = new List<GameObject>();
    List<GameObject> shortTermMemory = new List<GameObject>();
    [SerializeField] List<GameObject> constructInventory = new List<GameObject>();

    // memories
    [SerializeField] List<EntityMemory> constructEntities = new List<EntityMemory>();
    [SerializeField] List<SimulationConstructProfile> knownOthers = new List<SimulationConstructProfile>();

    // tasks
    [SerializeField] List<ExperienceStepSequence> learnedConstructSequences = new List<ExperienceStepSequence>();
    [SerializeField] List<ExperienceStepSequence> queuedConstructSequences = new List<ExperienceStepSequence>();
    [SerializeField] ExperienceStepSequence currentConstructSequence;

    // merits
    [SerializeField] EntityTraits constructTraits = new EntityTraits();
    [SerializeField] EntityPhysicalAttributes constructPhysicalAttributes = new EntityPhysicalAttributes();

    // events
    public class ReturnBoolEvent : UnityEvent<bool> { }
    public class ReturnVector3Event : UnityEvent<Vector3> { }
    public class ReturnGameObjectEvent : UnityEvent<GameObject> { }
    public class ReturnListGameObjectEvent : UnityEvent<List<GameObject>> { }
    ReturnBoolEvent actionCompletedEvent = new ReturnBoolEvent();
    ReturnVector3Event destinationReachedEvent = new ReturnVector3Event();
    ReturnListGameObjectEvent blinkEvent = new ReturnListGameObjectEvent();
    EntityMemoryEvent discoveredNewEntityMemoryEvent = new EntityMemoryEvent();

    // childed assets
    [SerializeField] NavMeshAgent navMeshAgent;
    #endregion

    #region Parameters
    public float TimeElapsedSinceInstantiation { get => timeElapsedSinceInstantiation; }

    public List<GameObject> GameObjectView { get => gameObjectView; set => gameObjectView = value; }
    public List<GameObject> ConstructInventory { get => constructInventory; set => constructInventory = value; }
    public List<EntityMemory> ConstructEntities { get => constructEntities; set => constructEntities = value; }
    public List<SimulationConstructProfile> KnownOthers { get => knownOthers; set => knownOthers = value; }
    //public List<ExperienceStepSequence> ConstructSequences { get => constructSequences; }
    public EntityTraits ConstructTraits { get => constructTraits; }
    public EntityPhysicalAttributes ConstructPhysicalAttributes { get => constructPhysicalAttributes; }

    public ReturnBoolEvent ActionCompletedEvent { get => actionCompletedEvent; set => actionCompletedEvent = value; }
    public ReturnVector3Event DestinationReachedEvent { get => destinationReachedEvent; set => destinationReachedEvent = value; }
    public ReturnListGameObjectEvent BlinkEvent { get => blinkEvent; set => blinkEvent = value; }
    public EntityMemoryEvent DiscoveredNewEntityMemoryEvent { get => discoveredNewEntityMemoryEvent; set => discoveredNewEntityMemoryEvent = value; }
    #endregion

    #region Default Functions
    void Start()
    {
        EntityMemoryManager.Initialize();

        constructTraits = EntityTraits.GenerateDefaultTraits();
        constructPhysicalAttributes = EntityPhysicalAttributes.GenerateDefaultPhysicalAttributes();

        InvokeRepeating("RefreshVision", 0.25f, 0.25f);
        Invoke("RandomTickUpdate", Random.Range(5f, 10f));
    }
    private void FixedUpdate()
    {
        timeElapsedSinceInstantiation += Time.deltaTime;
        constructPhysicalAttributes.GetPhysicalAttribute(PhysicalAttributeNames.hunger).Intensity += -0.01f;
        if(constructPhysicalAttributes.GetPhysicalAttribute(PhysicalAttributeNames.hunger).Intensity <= 50)
        {
            bool alreadyAdded = false;
            foreach(ExperienceStepSequence sequence in queuedConstructSequences)
            {
                if (sequence.name == "SearchForAndConsumeFood (copy)")
                {
                    alreadyAdded = true;
                }
            }
            if (!alreadyAdded) AddExperienceStepSequence(GetMostEligablePhyiscalAttributeSequence(new PhysicalAttributeDataValue(PhysicalAttributeNames.hunger, 1f)));
        }
    }
    void RandomTickUpdate()
    {
        if(queuedConstructSequences.Count <= 0)
        {
            DecideFreedom();
        }
        Invoke("RandomTickUpdate", Random.Range(5f, 10f));
    }
    #endregion

    #region Simulation Construct Functions

    #region Navigation Actions
    public void NavigateAgentToPosition(Vector3 target)
    {
        StartCoroutine(ProcessNavigationToPosition(target));
    }
    public void NavigateAgentToPosition(GameObject target)
    {
        StartCoroutine(ProcessNavigationToPosition(target));
    }
    public IEnumerator ProcessNavigationToPosition(Vector3 target)
    {
        navMeshAgent.SetDestination(target);
        navMeshAgent.autoTraverseOffMeshLink = true;

        Debug.DrawLine(transform.position, target, Color.green, 2.5f);

        yield return new WaitForFixedUpdate();

        yield return new WaitUntil(() => navMeshAgent.remainingDistance <= 0.1f); //  Vector3.Distance(transform.position, target) <= 0.5f);

        destinationReachedEvent.Invoke(target);
        actionCompletedEvent.Invoke(true);

        StopCoroutine(ProcessNavigationToPosition(target));

        yield break;
    }
    public IEnumerator ProcessNavigationToPosition(GameObject target)
    {
        navMeshAgent.SetDestination(target.transform.position);
        navMeshAgent.autoTraverseOffMeshLink = true;

        Debug.DrawLine(transform.position, target.transform.position, Color.green, 2.5f);

        yield return new WaitForFixedUpdate();

        yield return new WaitUntil(() => navMeshAgent.remainingDistance <= 0.1f); //  Vector3.Distance(transform.position, target) <= 0.5f);

        destinationReachedEvent.Invoke(target.transform.position);
        actionCompletedEvent.Invoke(true);

        StopCoroutine(ProcessNavigationToPosition(target));

        yield break;
    }
    public bool NavigateAgentToNearestInstance(EntityMemory memory)
    {
        NavigateAgentToPosition(memory.FindClosestInstance().Value);
        return true;
    }
    public void ManuallySearchForObject(GameObject target) // DO NOT USE UNLESS ABSOLUTELY NECESSARY
    {
        do
        {
            float randomAngle = UnityEngine.Random.Range(0f, Mathf.PI * 2);
            float searchDistance = 10;
            Vector3 suitableDestination = transform.position + new Vector3(Mathf.Cos(randomAngle), 0, Mathf.Sin(randomAngle)) * searchDistance;
            navMeshAgent.SetDestination(suitableDestination);
            navMeshAgent.autoTraverseOffMeshLink = true;
        } while (navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid);
        StopCoroutine(ManuallyProcessSearch(target));
        StartCoroutine(ManuallyProcessSearch(target));
    } 
    public IEnumerator ManuallyProcessSearch(GameObject target) // DO NOT USE UNLESS ABSOLUTELY NECESSARY
    {
        yield return new WaitUntil(() => gameObjectView.Contains(target) || navMeshAgent.remainingDistance <= 0.1f);
       
        if (gameObjectView.Contains(target))
        {
            Vector3 suitableDestination = target.transform.position;
            if (target.GetComponent<Collider>())
            {
                suitableDestination = target.GetComponent<Collider>().bounds.ClosestPoint(transform.position);
            }
            NavigateAgentToPosition(suitableDestination);
        }
        else ManuallySearchForObject(target);
        yield break;
    }
    #endregion

    #region Utility

    #region Vision
    void RefreshVision()
    {
        gameObjectView.Clear();
        foreach (RaycastHit hit in PerformRaycastScan())
        {
            gameObjectView.Add(hit.transform.gameObject);
            if(!shortTermMemory.Contains(hit.transform.gameObject))
            {
                shortTermMemory.Add(hit.transform.gameObject);
                AddGameObjectToEntityMemory(hit.transform.gameObject);
            }
        }
    }
    public List<RaycastHit> PerformRaycastScan()
    {
        List<RaycastHit> results = new List<RaycastHit>();
        Debug.DrawLine(transform.position, transform.position + transform.forward * 4, Color.green, 0.2f);
        Debug.DrawLine(transform.position + transform.forward * 4, transform.position + transform.forward * 8, Color.blue, 0.2f);
        results.AddRange(Physics.CapsuleCastAll(transform.position, transform.position + transform.forward * 4, 2f, transform.forward, 4));
        return results;
    }
    #endregion

    #region Memory
    bool AddGameObjectToEntityMemory(GameObject entity)
    {
        if (entity == this.gameObject) return false;

        // If they are a construct, automatically check them.
        if (entity.GetComponent<SimulationConstruct>()) GetProfileReference(entity.GetComponent<SimulationConstruct>());

        // Otherwise, standard EntityMemory proceedure.
        List<MonoBehaviour> behaviours = entity.GetComponents<MonoBehaviour>().ToList<MonoBehaviour>();
        List<System.Type> types = new List<System.Type>();
        foreach (MonoBehaviour subject in behaviours)
        {
            types.Add(subject.GetType());
        }
        foreach (EntityMemory subject in constructEntities)
        {
            List<System.Type> typesToRemove = new List<System.Type>();
            if (types.Contains(subject.EntityType))
            {
                typesToRemove.Add(subject.EntityType);
                if (!subject.InstancesInScene.ContainsKey(entity)) { subject.InstancesInScene.Add(entity, entity.transform.position); }
                if (subject.InstancesInScene[entity] != entity.transform.position) { subject.InstancesInScene[entity] = entity.transform.position; }
            }
            foreach (System.Type type in typesToRemove)
            {
                types.Remove(type);
            }
        }
        foreach (System.Type type in types)
        {
            EntityMemory addedEntity = new EntityMemory();
            addedEntity.EntityType = type;
            addedEntity.MemoryHolder = this;
            addedEntity.CreationTime = timeElapsedSinceInstantiation;
            addedEntity.InstancesInScene.Add(entity, entity.transform.position);
            discoveredNewEntityMemoryEvent.Invoke(addedEntity);
            constructEntities.Add(addedEntity);
        }
        return true;
    }
    public ExperienceStepSequence GetMostEligableTraitSequence(EntityTraits filter)
    {
        List<TraitNames> positiveTargets = new List<TraitNames>();
        List<TraitNames> neutralTargets = new List<TraitNames>();
        List<TraitNames> negativeTargets = new List<TraitNames>();
        Dictionary<ExperienceStepSequence, float> candidates = new Dictionary<ExperienceStepSequence, float>();

        foreach (TraitDataValue attribute in filter.MeritData)
        {
            if (attribute.Intensity > 0) { positiveTargets.Add(attribute.NameAsEnum); }
            if (attribute.Intensity < 0) { negativeTargets.Add(attribute.NameAsEnum); }
            if (attribute.Intensity == 0) { neutralTargets.Add(attribute.NameAsEnum); }
        }
        foreach (ExperienceStepSequence sequence in learnedConstructSequences)
        {
            foreach (TraitDataValue attribute in sequence.TraitChange.MeritData)
            {
                if (positiveTargets.Contains(attribute.NameAsEnum))
                {
                    if (!candidates.ContainsKey(sequence)) candidates.Add(sequence, attribute.Intensity);
                    else candidates[sequence] += attribute.Intensity;
                }
                if (negativeTargets.Contains(attribute.NameAsEnum))
                {
                    if (!candidates.ContainsKey(sequence)) candidates.Add(sequence, -attribute.Intensity);
                    else candidates[sequence] += -attribute.Intensity;
                }
                if (neutralTargets.Contains(attribute.NameAsEnum))
                {
                    if (!candidates.ContainsKey(sequence)) candidates.Add(sequence, attribute.Intensity);
                    else candidates[sequence] += 1;
                }
            }
        }
        var test = candidates.OrderByDescending(key => key.Value);
        return test.First().Key;
    }
    public ExperienceStepSequence GetMostEligableTraitSequence(TraitDataValue filter)
    {
        List<TraitNames> positiveTargets = new List<TraitNames>();
        List<TraitNames> neutralTargets = new List<TraitNames>();
        List<TraitNames> negativeTargets = new List<TraitNames>();
        Dictionary<ExperienceStepSequence, float> candidates = new Dictionary<ExperienceStepSequence, float>();

        if (filter.Intensity > 0) { positiveTargets.Add(filter.NameAsEnum); }
        if (filter.Intensity < 0) { negativeTargets.Add(filter.NameAsEnum); }
        if (filter.Intensity == 0) { neutralTargets.Add(filter.NameAsEnum); }

        foreach (ExperienceStepSequence sequence in learnedConstructSequences)
        {
            foreach (TraitDataValue attribute in sequence.TraitChange.MeritData)
            {
                if (positiveTargets.Contains(attribute.NameAsEnum))
                {
                    if (!candidates.ContainsKey(sequence)) candidates.Add(sequence, attribute.Intensity);
                    else candidates[sequence] += attribute.Intensity;
                }
                if (negativeTargets.Contains(attribute.NameAsEnum))
                {
                    if (!candidates.ContainsKey(sequence)) candidates.Add(sequence, -attribute.Intensity);
                    else candidates[sequence] += -attribute.Intensity;
                }
                if (neutralTargets.Contains(attribute.NameAsEnum))
                {
                    if (!candidates.ContainsKey(sequence)) candidates.Add(sequence, attribute.Intensity);
                    else candidates[sequence] += 1;
                }
            }
        }
        var test = candidates.OrderByDescending(key => key.Value);
        return test.First().Key;
    }
    public ExperienceStepSequence GetMostEligablePhyiscalAttributeSequence(EntityPhysicalAttributes filter)
    {
        List<PhysicalAttributeNames> positiveTargets = new List<PhysicalAttributeNames>();
        List<PhysicalAttributeNames> negativeTargets = new List<PhysicalAttributeNames>();
        List<PhysicalAttributeNames> neutralTargets = new List<PhysicalAttributeNames>();
        Dictionary<ExperienceStepSequence, float> candidates = new Dictionary<ExperienceStepSequence, float>();

        foreach (PhysicalAttributeDataValue attribute in filter.MeritData)
        {
            if (attribute.Intensity > 0) { positiveTargets.Add(attribute.NameAsEnum); }
            if (attribute.Intensity < 0) { negativeTargets.Add(attribute.NameAsEnum); }
            if (attribute.Intensity == 0) { neutralTargets.Add(attribute.NameAsEnum); }
        }
        foreach (ExperienceStepSequence sequence in learnedConstructSequences)
        {
            foreach (PhysicalAttributeDataValue attribute in sequence.PhysicalAttributeChange.MeritData)
            {
                if (positiveTargets.Contains(attribute.NameAsEnum))
                {
                    if (!candidates.ContainsKey(sequence)) candidates.Add(sequence, attribute.Intensity);
                    else candidates[sequence] += attribute.Intensity;
                }
                if (negativeTargets.Contains(attribute.NameAsEnum))
                {
                    if (!candidates.ContainsKey(sequence)) candidates.Add(sequence, -attribute.Intensity);
                    else candidates[sequence] += -attribute.Intensity;
                }
                if (neutralTargets.Contains(attribute.NameAsEnum))
                {
                    if (!candidates.ContainsKey(sequence)) candidates.Add(sequence, attribute.Intensity);
                    else candidates[sequence] += 1;
                }
            }
        }
        var test = candidates.OrderByDescending(key => key.Value);
        return test.First().Key;
    }
    public ExperienceStepSequence GetMostEligablePhyiscalAttributeSequence(PhysicalAttributeDataValue filter)
    {
        List<PhysicalAttributeNames> positiveTargets = new List<PhysicalAttributeNames>();
        List<PhysicalAttributeNames> negativeTargets = new List<PhysicalAttributeNames>();
        List<PhysicalAttributeNames> neutralTargets = new List<PhysicalAttributeNames>();
        Dictionary<ExperienceStepSequence, float> candidates = new Dictionary<ExperienceStepSequence, float>();

        if (filter.Intensity > 0) { positiveTargets.Add(filter.NameAsEnum); }
        if (filter.Intensity < 0) { negativeTargets.Add(filter.NameAsEnum); }
        if (filter.Intensity == 0) { neutralTargets.Add(filter.NameAsEnum); }

        foreach (ExperienceStepSequence sequence in learnedConstructSequences)
        {
            foreach (PhysicalAttributeDataValue attribute in sequence.PhysicalAttributeChange.MeritData)
            {
                if (positiveTargets.Contains(attribute.NameAsEnum))
                {
                    if (!candidates.ContainsKey(sequence)) candidates.Add(sequence, attribute.Intensity);
                    else candidates[sequence] += attribute.Intensity;
                }
                if (negativeTargets.Contains(attribute.NameAsEnum))
                {
                    if (!candidates.ContainsKey(sequence)) candidates.Add(sequence, -attribute.Intensity);
                    else candidates[sequence] += -attribute.Intensity;
                }
                if (neutralTargets.Contains(attribute.NameAsEnum))
                {
                    if (!candidates.ContainsKey(sequence)) candidates.Add(sequence, attribute.Intensity);
                    else candidates[sequence] += 1;
                }
            }
        }
        var test = candidates.OrderByDescending(key => key.Value);
        return test.First().Key;
    }
    public Vector3 SearchMemoryForInstance(GameObject instance)
    {
        // Flesh out to include associated entities in the future.
        foreach (GameObject subject in constructInventory)
        {
            if (subject == instance) { return Vector3.one; }
        }
        foreach (EntityMemory subject in constructEntities)
        {
            if (subject.InstancesInScene.ContainsKey(instance)) { return subject.InstancesInScene[instance]; }
        }
        return Vector3.zero;
    }
    #endregion

    #region Inventory
    public void AddGameObjectToInventory(GameObject entity)
    {
        if (entity == null) return;
        if (constructInventory.Contains(entity)) return;
        if (entity.GetComponent<SimulationConstruct>()) return;
        if (entity.GetComponent<Rigidbody>())
        {
            if (entity.GetComponent<Rigidbody>().mass > constructPhysicalAttributes.GetPhysicalAttribute(PhysicalAttributeNames.inventorymasslimit).Intensity)
            {
                return;
            }
        }
        if (constructInventory.Count >= constructPhysicalAttributes.GetPhysicalAttribute(PhysicalAttributeNames.inventorycapacity).Intensity)
        {
            constructInventory.RemoveAt(0);
        }

        constructInventory.Add(entity);
        ProcessObjectPackaging(entity.transform, true);
    }
    public void RemoveGameObjectFromInventory(GameObject entity)
    {
        if (!constructInventory.Contains(entity)) return;
        constructInventory.Remove(entity);
        Vector3 respawnPosition = transform.position + transform.forward * (0.1f + entity.transform.GetComponent<Collider>().bounds.extents.magnitude);
        entity.transform.position = respawnPosition;
        ProcessObjectPackaging(entity.transform, false);
    }
    public void ProcessObjectPackaging(Transform transform, bool isPuttingAway = true)
    {
        if (transform.GetComponent<Rigidbody>()) transform.GetComponent<Rigidbody>().isKinematic = !isPuttingAway;
        if (transform.GetComponent<Collider>()) transform.GetComponent<Collider>().enabled = !isPuttingAway;
        if (transform.GetComponent<Renderer>()) transform.GetComponent<Renderer>().enabled = !isPuttingAway;

        foreach (Transform child in transform.GetComponentsInChildren<Transform>())
        {
            if (child == transform) continue;
            ProcessObjectPackaging(child, isPuttingAway);
        }
    }

    #endregion

    #region Experiences
    public void AddExperienceStepSequence(ExperienceStepSequence sequence)
    {
        ExperienceStepSequence sequenceCopy = ExperienceStepSequence.CreateCopy(sequence);

        queuedConstructSequences.Add(sequenceCopy);

        if (currentConstructSequence == null) { currentConstructSequence = sequenceCopy; currentConstructSequence.StartSequence(this); }

        DecideOnWhatSequenceToPursue(currentConstructSequence);
    }
    public void RemoveSequence(ExperienceStepSequence sequence)
    {
        if(queuedConstructSequences.Contains(sequence)) queuedConstructSequences.Remove(sequence);

        if(queuedConstructSequences.Count > 0) DecideOnWhatSequenceToPursue(queuedConstructSequences[0]);

        ExperienceStepSequence.DestroyImmediate(sequence);
    }
    public void DecideOnWhatSequenceToPursue(ExperienceStepSequence crossReference)
    {
        ExperienceStepSequence goingToBeExecuted = crossReference;
        foreach (ExperienceStepSequence toPerform in queuedConstructSequences)
        {
            if (goingToBeExecuted.SequencePriority < toPerform.SequencePriority) { destinationReachedEvent.RemoveAllListeners(); goingToBeExecuted = toPerform; }
        }
        if (goingToBeExecuted == null) return;

        if (goingToBeExecuted != currentConstructSequence)
        {
            currentConstructSequence = goingToBeExecuted;
            currentConstructSequence.StartSequence(this);
        }
    }
    public void DecideFreedom()
    {

    }
    #endregion

    #region Simulation Construct Interactions
    /// <summary>
    /// Get the profile of the select construct from memory. If a profile does not exist, one is created automatically.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public SimulationConstructProfile GetProfileReference(SimulationConstruct other)
    {
        foreach (SimulationConstructProfile subject in knownOthers)
        {
            if (subject.AssociatedSimulationConstruct == other)
            {
                return subject;
            }
        }
        SimulationConstructProfile newProfile = new SimulationConstructProfile(other);
        knownOthers.Add(newProfile);
        return newProfile;
    }
    /// <summary>
    /// Function called by a different simulation construct to identify this construct as a friend.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Greet(SimulationConstruct other)
    {
        SimulationConstructProfile profile = GetProfileReference(other);
        if (profile.AssumedTraits.GetTrait(TraitNames.friend).Intensity == 0 && profile.AssumedTraits.GetTrait(TraitNames.enemy).Intensity < 0)
        {
            profile.AssumedTraits.GetTrait(TraitNames.friend).Intensity += 1;
            return true;
        }
        return false;
    }
    /// <summary>
    /// Returns the memory of an associated type, if it exists within this construct's memory.
    /// </summary>
    /// <param name="other"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public EntityMemory RequestMemoryKnowledge(SimulationConstruct other, System.Type type)
    {
        SimulationConstructProfile profile = GetProfileReference(other);
        if (!profile.IsFriend()) return null;
        foreach (EntityMemory memory in constructEntities)
        {
            if (memory.EntityType == type)
            {
                return memory;
            }
        }
        return null;
    }
    /// <summary>
    /// Returns the instances of the associated type memory, if it exists within this construct's memory.
    /// </summary>
    /// <param name="other"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public Dictionary<GameObject, Vector3> RequestInstanceKnowledge(SimulationConstruct other, System.Type type)
    {
        Dictionary<GameObject, Vector3> toReturn = new Dictionary<GameObject, Vector3>();
        SimulationConstructProfile profile = GetProfileReference(other);
        if (!profile.IsFriend()) return null;
        foreach(EntityMemory memory in constructEntities)
        {
            if(memory.EntityType == type)
            {
                foreach(KeyValuePair<GameObject, Vector3> pair in memory.InstancesInScene)
                {
                    toReturn.Add(pair.Key, pair.Value);
                }
            }
        }
        return toReturn;
    }
    /// <summary>
    /// Returns the trait of this construct.
    /// </summary>
    /// <param name="other"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public TraitDataValue RequestTraitDataValue(SimulationConstruct other, TraitNames name)
    {
        TraitDataValue value = new TraitDataValue(name, 0);
        SimulationConstructProfile profile = GetProfileReference(other);
        if (!profile.IsFriend()) value = constructTraits.GetTrait(name);
        return value;
    }
    /// <summary>
    /// Returns the physical attribute of this construct.
    /// </summary>
    /// <param name="other"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public PhysicalAttributeDataValue RequestPhysicalAttributeDataValue(SimulationConstruct other, PhysicalAttributeNames name)
    {
        PhysicalAttributeDataValue value = new PhysicalAttributeDataValue(name, 0);
        SimulationConstructProfile profile = GetProfileReference(other);
        if (!profile.IsFriend()) value = constructPhysicalAttributes.GetPhysicalAttribute(name);
        return value;
    }



    #endregion

    #endregion

    #endregion
}
