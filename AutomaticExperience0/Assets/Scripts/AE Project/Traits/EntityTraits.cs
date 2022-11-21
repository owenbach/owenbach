using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.VersionControl;
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
        meritToReturn.meritData.Add(new TraitDataValue(TraitNames.adventurous, 5));
        meritToReturn.meritData.Add(new TraitDataValue(TraitNames.curious, 5));
        return meritToReturn;
    }
    #region Utilities
    public static EntityTraits GenerateTraitDifference(EntityTraits original, EntityTraits changes)
    {
        EntityTraits toReturn = new EntityTraits();
        foreach (TraitDataValue subject in original.MeritData)
        {
            TraitDataValue newTrait = new TraitDataValue(subject.NameAsEnum, changes.MeritData[original.MeritData.IndexOf(subject)].Intensity - subject.Intensity);
            toReturn.MeritData.Add(newTrait);
        }
        return toReturn;
    }
    public TraitDataValue GetTrait(TraitNames inputtedNameAsEnum)
    {
        TraitDataValue attribute = null;
        foreach (TraitDataValue value in meritData)
        {
            if (value.NameAsEnum == inputtedNameAsEnum) attribute = value;
        }
        return attribute;
    }
    public static EntityTraits CopyTraits(EntityTraits original)
    {
        EntityTraits toReturn = new EntityTraits();
        foreach (TraitDataValue subject in original.MeritData)
        {
            toReturn.MeritData.Add(new TraitDataValue(subject.NameAsEnum, subject.Intensity));
        }
        return toReturn;
    }
    #endregion
}
