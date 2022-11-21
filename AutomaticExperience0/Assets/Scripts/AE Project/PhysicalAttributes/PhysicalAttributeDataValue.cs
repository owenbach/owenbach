using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PhysicalAttributeDataValue : IComparable <float>
{
    #region Fields
    string nameAsString;
    [SerializeField] PhysicalAttributeNames nameAsEnum;
    [SerializeField] float intensity;
    #endregion
    
    #region Properties
    public string NameAsString { get => nameAsString; set => nameAsString = value; }
    public PhysicalAttributeNames NameAsEnum { get => nameAsEnum; set => nameAsEnum = value; }
    public float Intensity { get => intensity; set => intensity = value; }
    #endregion

    #region Functions
    public int CompareTo(float other)
    {
        return intensity.CompareTo(other);
    }
    #endregion

    #region Constructor
    public PhysicalAttributeDataValue(string inputName, float inputIntensity)
    {
        nameAsString = inputName;
        intensity = inputIntensity;
    }
    public PhysicalAttributeDataValue(PhysicalAttributeNames inputName, float inputIntensity)
    {
        nameAsEnum = inputName;
        intensity = inputIntensity;
        nameAsString = nameAsEnum.ToString();
    }
    #endregion
}
public enum PhysicalAttributeNames
{
    starthealth,
    health,
    starthunger,
    hunger,
    startthirst,
    thirst,
    mass,
    inventorycapacity,
    inventorymasslimit
}
