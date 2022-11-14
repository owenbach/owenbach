using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ExperienceMemory : MonoBehaviour
{
    #region Fields
    float timeOfCreation;
    SimulationConstruct activePerformer;
    List<EntityEventMemory> experienceSequence = new List<EntityEventMemory>();
    public class ReturnBoolEvent : UnityEvent<bool> { }
    ReturnBoolEvent actionCompletedEvent;



    EntityTraits performerTraitChange;
    EntityPhysicalAttributes performerPhysicalAttributeChange;

    #endregion
    #region Properties
    public List<EntityEventMemory> ExperienceSequence { get => experienceSequence; set => experienceSequence = value; }
    public EntityTraits PerformerTraitChange { get => performerTraitChange; set => performerTraitChange = value; }
    public EntityPhysicalAttributes PerformerPhysicalAttributeChange { get => performerPhysicalAttributeChange; set => performerPhysicalAttributeChange = value; }
    #endregion
    #region Functions
    public void BeginExperience(SimulationConstruct performer)
    {
        actionCompletedEvent.RemoveAllListeners();
        activePerformer = performer;
        ExecuteEntityEventMemory(experienceSequence[0]);

    }
    public void ExecuteEntityEventMemory(EntityEventMemory memory)
    {
        //if(memory == )
        //memory.PerformerMonoBehaviour
    }
    #endregion
    #region Utility
    #endregion
}
