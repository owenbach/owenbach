using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationConstructProfile
{
    #region Fields
    [SerializeField] SimulationConstruct associatedSimulationConstruct;
    [SerializeField] EntityTraits assumedTraits;
    [SerializeField] EntityPhysicalAttributes assumedPhysicalAttributes;
    #endregion
    #region Properties
    public SimulationConstruct AssociatedSimulationConstruct { get => associatedSimulationConstruct; set => associatedSimulationConstruct = value; }
    public EntityTraits AssumedTraits { get => assumedTraits; set => assumedTraits = value; }
    public EntityPhysicalAttributes AssumedPhysicalAttributes { get => assumedPhysicalAttributes; set => assumedPhysicalAttributes = value; }
    #endregion
    #region Constructor
    public SimulationConstructProfile(SimulationConstruct profile)
    {
        associatedSimulationConstruct = profile;
    }
    #endregion
    #region Utility
    public bool IsFriend()
    {
        if (assumedTraits.GetTrait(TraitNames.friend).Intensity > 0) return true;
        return false;
    }
    #endregion
}
