using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        meritToReturn.meritData.Add(new PhysicalAttributeDataValue(PhysicalAttributeNames.starthealth, 100));
        meritToReturn.meritData.Add(new PhysicalAttributeDataValue(PhysicalAttributeNames.health, 100));
        meritToReturn.meritData.Add(new PhysicalAttributeDataValue(PhysicalAttributeNames.starthunger, 100));
        meritToReturn.meritData.Add(new PhysicalAttributeDataValue(PhysicalAttributeNames.hunger, 100));
        meritToReturn.meritData.Add(new PhysicalAttributeDataValue(PhysicalAttributeNames.startthirst, 100));
        meritToReturn.meritData.Add(new PhysicalAttributeDataValue(PhysicalAttributeNames.thirst, 100));
        meritToReturn.meritData.Add(new PhysicalAttributeDataValue(PhysicalAttributeNames.mass, 80));
        meritToReturn.meritData.Add(new PhysicalAttributeDataValue(PhysicalAttributeNames.inventorycapacity, 3));
        meritToReturn.MeritData.Add(new PhysicalAttributeDataValue(PhysicalAttributeNames.inventorymasslimit, 3));
        return meritToReturn;
    }
    #region Utility
    public static EntityPhysicalAttributes GeneratePhysicalAttributeDifference(EntityPhysicalAttributes original, EntityPhysicalAttributes changes)
    {
        EntityPhysicalAttributes toReturn = new EntityPhysicalAttributes();
        foreach(PhysicalAttributeDataValue subject in original.MeritData)
        {
            PhysicalAttributeDataValue newTrait = new PhysicalAttributeDataValue(subject.NameAsEnum, changes.MeritData[original.MeritData.IndexOf(subject)].Intensity - subject.Intensity);
            toReturn.MeritData.Add(newTrait);
        }
        return toReturn;
    }
    public PhysicalAttributeDataValue GetPhysicalAttribute(PhysicalAttributeNames inputtedNameAsEnum)
    {
        PhysicalAttributeDataValue attribute = null;
        foreach(PhysicalAttributeDataValue value in meritData)
        {
            if (value.NameAsEnum == inputtedNameAsEnum) attribute = value;
        }
        return attribute;
    }
    public static EntityPhysicalAttributes CopyPhysicalAttributes(EntityPhysicalAttributes original)
    {
        EntityPhysicalAttributes toReturn = new EntityPhysicalAttributes();
        foreach (PhysicalAttributeDataValue subject in original.MeritData)
        {
            toReturn.MeritData.Add(new PhysicalAttributeDataValue(subject.NameAsEnum, subject.Intensity));
        }
        return toReturn;
    }
    #endregion
}
