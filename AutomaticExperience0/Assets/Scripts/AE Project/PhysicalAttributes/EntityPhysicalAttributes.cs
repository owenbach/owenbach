using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Two types of merits:
 *  - traits
 *  - physical attributes
 */



[System.Serializable]
public class EntityPhysicalAttributes
{
    [SerializeField] List<PhysicalAttributeDataValue> meritData = new List<PhysicalAttributeDataValue>();

    public List<PhysicalAttributeDataValue> MeritData { get => meritData; set => meritData = value; }
    public static EntityPhysicalAttributes GenerateDefaultPhysicalAttributes()
    {
        EntityPhysicalAttributes meritToReturn = new EntityPhysicalAttributes();
        meritToReturn.meritData.Add(new PhysicalAttributeDataValue(PhysicalAttributeDataValue.PhysicalAttributeNames.starthealth, 100));
        meritToReturn.meritData.Add(new PhysicalAttributeDataValue(PhysicalAttributeDataValue.PhysicalAttributeNames.health, 100));
        meritToReturn.meritData.Add(new PhysicalAttributeDataValue(PhysicalAttributeDataValue.PhysicalAttributeNames.starthunger, 100));
        meritToReturn.meritData.Add(new PhysicalAttributeDataValue(PhysicalAttributeDataValue.PhysicalAttributeNames.hunger, 100));
        meritToReturn.meritData.Add(new PhysicalAttributeDataValue(PhysicalAttributeDataValue.PhysicalAttributeNames.startthirst, 100));
        meritToReturn.meritData.Add(new PhysicalAttributeDataValue(PhysicalAttributeDataValue.PhysicalAttributeNames.thirst, 100));
        meritToReturn.meritData.Add(new PhysicalAttributeDataValue(PhysicalAttributeDataValue.PhysicalAttributeNames.mass, 80));
        return meritToReturn;
    }
    #region Utility
    public static EntityPhysicalAttributes GeneratePhysicalAttributeDifference(EntityPhysicalAttributes original, EntityPhysicalAttributes changes)
    {
        EntityPhysicalAttributes toReturn = new EntityPhysicalAttributes();
        foreach (PhysicalAttributeDataValue trait in original.MeritData)
        {
            toReturn.MeritData.Add(trait);
        }
        EntityPhysicalAttributes toRemove = new EntityPhysicalAttributes();
        foreach (PhysicalAttributeDataValue trait in toReturn.MeritData)
        {
            if (!changes.MeritData.Contains(trait)) { toRemove.MeritData.Remove(trait); }
        }
        foreach (PhysicalAttributeDataValue trait in toRemove.MeritData)
        {
            toReturn.MeritData.Remove(trait);
        }
        toReturn.MeritData.Sort();
        changes.MeritData.Sort();
        foreach (PhysicalAttributeDataValue trait in changes.MeritData)
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
