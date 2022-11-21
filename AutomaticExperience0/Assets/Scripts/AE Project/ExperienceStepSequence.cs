using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;



[System.Serializable]
[CreateAssetMenu]



public class ExperienceStepSequence : ScriptableObject
{
    #region Fields
    [SerializeField] int sequencePriority = 0;
    [SerializeField] EntityTraits traitChange = new EntityTraits();
    [SerializeField] EntityPhysicalAttributes physicalAttributeChange = new EntityPhysicalAttributes();
    [SerializeField] ExperienceStep currentStep = null;
    [SerializeField] List<ExperienceStep> sequenceSteps = new List<ExperienceStep>();

    ExperienceStepSequence scriptableObjectOrigin;
    SimulationConstruct stepActor;
    ExperienceStep lastStep;
    EntityTraits startingTraits;
    EntityPhysicalAttributes startingPhysicalAttributes;

    ExperienceStepEvent stepCompletedEvent = new ExperienceStepEvent();
    ExperienceStepEvent stepReversedEvent = new ExperienceStepEvent();
    ExperienceStepSequenceEvent sequenceCompletedEvent = new ExperienceStepSequenceEvent();

    #endregion

    #region Properties
    public int SequencePriority { get => sequencePriority; set => sequencePriority = value; }
    public EntityTraits TraitChange { get => traitChange; set => traitChange = value; }
    public EntityPhysicalAttributes PhysicalAttributeChange { get => physicalAttributeChange; set => physicalAttributeChange = value; }
    public List<ExperienceStep> SequenceSteps { get => sequenceSteps; set => sequenceSteps = value; }
    public ExperienceStep CurrentStep { get => currentStep; set => currentStep = value; }
    public ExperienceStepSequence ScriptableObjectOrigin { get => scriptableObjectOrigin; set => scriptableObjectOrigin = value; }
    public SimulationConstruct StepActor { get => stepActor; set => stepActor = value; }
    public ExperienceStepEvent StepCompletedEvent { get => stepCompletedEvent; set => stepCompletedEvent = value; }
    public ExperienceStepEvent StepReversedEvent { get => stepReversedEvent; set => stepReversedEvent = value; }
    public ExperienceStepSequenceEvent SequenceCompletedEvent { get => sequenceCompletedEvent; set => sequenceCompletedEvent = value; }
    #endregion

    #region Functions
    public void StartSequence(SimulationConstruct actor)
    {
        stepActor = actor;
        startingTraits = EntityTraits.CopyTraits(stepActor.ConstructTraits);
        startingPhysicalAttributes = EntityPhysicalAttributes.CopyPhysicalAttributes(stepActor.ConstructPhysicalAttributes);

        foreach(ExperienceStep step in sequenceSteps)
        {
            step.StepActor = stepActor;
        }

        sequenceCompletedEvent.AddListener(stepActor.RemoveSequence);

        currentStep = sequenceSteps[0];
        currentStep.StepCompletedEvent.RemoveAllListeners();
        currentStep.StepReversedEvent.RemoveAllListeners();
        currentStep.StepCompletedEvent.AddListener(ProceedToNextStep);
        currentStep.StepReversedEvent.AddListener(GoBackOneStep);
        currentStep.Execute(currentStep.StepType);
    }
    public void ProceedToNextStep(ExperienceStep previousStep)
    {
        if (previousStep == null) return;
        lastStep = previousStep;
        stepCompletedEvent.Invoke(lastStep);
        lastStep.StepCompletedEvent.RemoveAllListeners();
        lastStep.StepReversedEvent.RemoveAllListeners();
        if (sequenceSteps.IndexOf(lastStep) == sequenceSteps.Count - 1)
        {
            EndSequence();
        }
        else
        {
            currentStep = sequenceSteps[sequenceSteps.IndexOf(lastStep) + 1];

            if (currentStep.ReferencedEntityMemory.Count == 0) { currentStep.ReferencedEntityMemory.AddRange(lastStep.ReferencedEntityMemory); }
            if (currentStep.ReferencedEntityEventMemory.Count == 0) { currentStep.ReferencedEntityEventMemory.AddRange(lastStep.ReferencedEntityEventMemory); }
            if (currentStep.ReferencedExperienceStepSequence.Count == 0) { currentStep.ReferencedExperienceStepSequence.AddRange(lastStep.ReferencedExperienceStepSequence); }
            if (currentStep.ReferencedGameObject.Count == 0) { currentStep.ReferencedGameObject.AddRange(lastStep.ReferencedGameObject); }
            if (currentStep.ReferencedTraits.MeritData.Count == 0) { currentStep.ReferencedTraits = lastStep.ReferencedTraits; }
            if (currentStep.ReferencedPhysicalAttributes.MeritData.Count == 0) { currentStep.ReferencedPhysicalAttributes = lastStep.ReferencedPhysicalAttributes; }
            if (currentStep.ReferencedSimulationConstruct.Count == 0) { currentStep.ReferencedSimulationConstruct.AddRange(lastStep.ReferencedSimulationConstruct); }
            if (currentStep.ReferencedString.Count == 0) { currentStep.ReferencedString.AddRange(lastStep.ReferencedString); }
            if (currentStep.ReferencedType.Count == 0) { currentStep.ReferencedType.AddRange(lastStep.ReferencedType); }
            if (currentStep.ReferencedVector3.Count == 0) { currentStep.ReferencedVector3.AddRange(lastStep.ReferencedVector3); }

            currentStep.StepCompletedEvent.AddListener(ProceedToNextStep);
            currentStep.StepReversedEvent.AddListener(GoBackOneStep);
            currentStep.Execute(currentStep.StepType, true);
        }
    }
    public void GoBackOneStep(ExperienceStep previousStep)
    {
        if (previousStep == null) return;
        lastStep = previousStep;
        stepReversedEvent.Invoke(lastStep);
        lastStep.StepCompletedEvent.RemoveAllListeners();
        lastStep.StepReversedEvent.RemoveAllListeners();
        if (sequenceSteps.IndexOf(lastStep) == sequenceSteps.Count)
        {
            EndSequence();
        }
        else
        {
            currentStep = sequenceSteps[sequenceSteps.IndexOf(lastStep) - 1];

            currentStep.ReferencedEntityMemory.Clear(); { currentStep.ReferencedEntityMemory.AddRange(lastStep.ReferencedEntityMemory); }
            currentStep.ReferencedEntityEventMemory.Clear(); { currentStep.ReferencedEntityEventMemory.AddRange(lastStep.ReferencedEntityEventMemory); }
            currentStep.ReferencedExperienceStepSequence.Clear(); { currentStep.ReferencedExperienceStepSequence.AddRange(lastStep.ReferencedExperienceStepSequence); }
            currentStep.ReferencedGameObject.Clear(); { currentStep.ReferencedGameObject.AddRange(lastStep.ReferencedGameObject); }
            { currentStep.ReferencedTraits = lastStep.ReferencedTraits; }
            { currentStep.ReferencedPhysicalAttributes = lastStep.ReferencedPhysicalAttributes; }
            currentStep.ReferencedSimulationConstruct.Clear(); { currentStep.ReferencedSimulationConstruct.AddRange(lastStep.ReferencedSimulationConstruct); }
            currentStep.ReferencedString.Clear(); { currentStep.ReferencedString.AddRange(lastStep.ReferencedString); }
            currentStep.ReferencedType.Clear(); { currentStep.ReferencedType.AddRange(lastStep.ReferencedType); }
            currentStep.ReferencedVector3.Clear(); { currentStep.ReferencedVector3.AddRange(lastStep.ReferencedVector3); }

            currentStep.StepCompletedEvent.AddListener(ProceedToNextStep);
            currentStep.StepReversedEvent.AddListener(GoBackOneStep);
            currentStep.Execute(currentStep.StepType, true);
        }
    }
    public void EndSequence()
    {
        scriptableObjectOrigin.TraitChange = EntityTraits.GenerateTraitDifference(startingTraits, stepActor.ConstructTraits);
        scriptableObjectOrigin.PhysicalAttributeChange = EntityPhysicalAttributes.GeneratePhysicalAttributeDifference(startingPhysicalAttributes, stepActor.ConstructPhysicalAttributes);

        foreach (ExperienceStep step in sequenceSteps)
        {
            step.StepCompletedEvent.RemoveAllListeners();
        }
        sequenceCompletedEvent.Invoke(this);
    }
    #endregion

    #region Utility
    public static ExperienceStepSequence CreateCopy(ExperienceStepSequence template)
    {
        ExperienceStepSequence sequenceCopy = ExperienceStepSequence.CreateInstance<ExperienceStepSequence>();
        sequenceCopy.name = template.name + " (copy)";
        sequenceCopy.SequencePriority = template.SequencePriority;
        sequenceCopy.ScriptableObjectOrigin = template;
        foreach (ExperienceStep step in template.SequenceSteps)
        {
            sequenceCopy.SequenceSteps.Add(ExperienceStep.CreateCopy(step));
        }
        return sequenceCopy;
    }
    #endregion
}
public class ExperienceStepEvent : UnityEvent<ExperienceStep> { }
public class ExperienceStepSequenceEvent : UnityEvent<ExperienceStepSequence> { }
