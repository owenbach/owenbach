using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using static UnityEngine.EventSystems.EventTrigger;

[System.Serializable]
public class SimulationConstruct : MonoBehaviour
{
    #region Fields
    // store time
    float timeElapsedSinceInstantiation = 0;

    // store vision
    [SerializeField] List<GameObject> gameObjectView = new List<GameObject>();
    [SerializeField] List<GameObject> shortTermMemory = new List<GameObject>();

    // memories
    [SerializeField] List<EntityMemory> constructEntities = new List<EntityMemory>();

    // merits
    [SerializeField] EntityTraits constructTraits = new EntityTraits();
    [SerializeField] EntityPhysicalAttributes constructPhysicalAttributes = new EntityPhysicalAttributes();

    // events
    public class ReturnBoolEvent : UnityEvent<bool> { }
    public class ReturnGameObjectEvent : UnityEvent<GameObject> { }
    public class ReturnListGameObjectEvent : UnityEvent<List<GameObject>> { }
    ReturnBoolEvent actionCompletedEvent = new ReturnBoolEvent();
    ReturnListGameObjectEvent blinkEvent = new ReturnListGameObjectEvent();

    // childed assets
    [SerializeField] NavMeshAgent navMeshAgent;
    #endregion

    #region Parameters
    public float TimeElapsedSinceInstantiation { get => timeElapsedSinceInstantiation; }

    public List<GameObject> GameObjectView { get => gameObjectView; set => gameObjectView = value; }
    public List<EntityMemory> ConstructEntities { get => constructEntities; }
    //public List<ExperienceMemory> ConstructExperiences { get => constructExperiences; }
    public EntityTraits ConstructTraits { get => constructTraits; }
    public EntityPhysicalAttributes ConstructPhysicalAttributes { get => constructPhysicalAttributes; }

    public ReturnListGameObjectEvent BlinkEvent { get => blinkEvent; set => blinkEvent = value; }
    #endregion

    #region Default Functions
    void Start()
    {
        EntityMemoryManager.Initialize();
        InvokeRepeating("RefreshVision", 0.25f, 0.25f);
        ManuallySearchForObject(GameObject.Find("Cube (4)"));
        actionCompletedEvent.AddListener(PlayHappy);
    }
    private void FixedUpdate()
    {
        timeElapsedSinceInstantiation += Time.deltaTime;
    }
    #endregion

    #region Simulation Construct Actions

    #region Navigation Actions
    public void NavigateAgentToPosition(Vector3 target)
    {
        Vector3 suitableDestination = target;
        navMeshAgent.SetDestination(suitableDestination);
        navMeshAgent.autoTraverseOffMeshLink = true;
        if (navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid) { actionCompletedEvent.Invoke(false); return; }
        StartCoroutine(ProcessNavigation(target));
    }
    public IEnumerator ProcessNavigation(Vector3 target)
    {
        yield return new WaitWhile(() => navMeshAgent.remainingDistance > 0.1f);
        actionCompletedEvent.Invoke(true);
        StopCoroutine(ProcessNavigation(target));
        yield break;
    }
    public bool NavigateAgentToNearestInstance(EntityMemory memory)
    {
        if (memory.InstancesInScene.Count <= 0) return false;
        KeyValuePair<GameObject,Vector3> chosenInstance = memory.InstancesInScene.ToArray()[0];
        foreach(KeyValuePair<GameObject,Vector3> pair in memory.InstancesInScene)
        {
            if(Vector3.Distance(transform.position, pair.Value) <= Vector3.Distance(transform.position, chosenInstance.Value)) { chosenInstance = pair; }
        }
        NavigateAgentToPosition(chosenInstance.Value);
        return true;
    }
    public void ManuallySearchForObject(GameObject target)
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
    public IEnumerator ManuallyProcessSearch(GameObject target)
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

    #region MemorySearchActions
    public Vector3 SearchMemoryForInstance(GameObject instance)
    {
        // Flesh out to include associated entities in the future.
        foreach(EntityMemory subject in constructEntities)
        {
            if(subject.InstancesInScene.ContainsKey(instance)) { return subject.InstancesInScene[instance]; }
        }
        return Vector3.zero;
    }
    #endregion

    #region Utility

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
    bool AddGameObjectToEntityMemory(GameObject entity)
    {
        if (entity == this.gameObject) return false;
        List<MonoBehaviour> behaviours = entity.GetComponents<MonoBehaviour>().ToList<MonoBehaviour>();
        List<System.Type> types = new List<System.Type>();
        foreach (MonoBehaviour subject in behaviours)
        {
            Debug.Log(subject.GetType());
            types.Add(subject.GetType());
        }
        foreach (EntityMemory subject in constructEntities)
        {
            List<System.Type> typesToRemove = new List<System.Type>();
            if (types.Contains(subject.EntityType))
            {
                typesToRemove.Add(subject.EntityType);
                if (!subject.InstancesInScene.ContainsKey(entity)) subject.InstancesInScene.Add(entity, entity.transform.position);
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
            constructEntities.Add(addedEntity);
        }
        return true;
    }
    void PlayHappy(bool completed)
    {
        if (completed) Debug.Log("yay");
        else Debug.Log("boo");
        foreach (EntityMemory entity in constructEntities)
        {
            entity.InvestigateEntityMethods();
        }
    }
    #endregion

    #endregion
}
