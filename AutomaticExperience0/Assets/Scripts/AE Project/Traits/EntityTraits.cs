using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Two types of merits:
 *  - traits
 *  - physical attributes
 */



[System.Serializable]
public class EntityTraits
{
    [SerializeField] List<TraitDataValue> meritData = new List<TraitDataValue>();

    public List<TraitDataValue> MeritData { get => meritData; set => meritData = value; }
    public static EntityTraits GenerateDefaultTraits()
    {
        EntityTraits meritToReturn = new EntityTraits();
        meritToReturn.meritData.Add(new TraitDataValue(TraitDataValue.TraitNames.creativity, 5));
        meritToReturn.meritData.Add(new TraitDataValue(TraitDataValue.TraitNames.curiosity, 5));
        return meritToReturn;
    }
    #region Utilities
    public static EntityTraits GenerateTraitDifference(EntityTraits original, EntityTraits changes)
    {
        EntityTraits toReturn = new EntityTraits();
        foreach (TraitDataValue trait in original.MeritData)
        {
            toReturn.MeritData.Add(trait);
        }
        EntityTraits toRemove = new EntityTraits();
        foreach (TraitDataValue trait in toReturn.MeritData)
        {
            if (!changes.MeritData.Contains(trait)) { toRemove.MeritData.Remove(trait); }
        }
        foreach (TraitDataValue trait in toRemove.MeritData)
        {
            toReturn.MeritData.Remove(trait);
        }
        toReturn.MeritData.Sort();
        changes.MeritData.Sort();
        foreach (TraitDataValue trait in changes.MeritData)
        {
            if (!toReturn.MeritData.Contains(trait)) { toReturn.MeritData.Add(trait); toReturn.MeritData.Sort(); }
            else
            {
                changes.MeritData.IndexOf(trait);
                toReturn.MeritData[changes.MeritData.IndexOf(trait)].Intensity = trait.Intensity - toReturn.MeritData[changes.MeritData.IndexOf(trait)].Intensity;
            }
        }
        return toReturn;
    }
    #endregion
}
