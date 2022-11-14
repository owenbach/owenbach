using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TraitDataValue
{
    #region Fields
    string nameAsString;
    TraitNames nameAsEnum;
    float intensity;
    #endregion
    #region Properties
    public string NameAsString { get => nameAsString; set => nameAsString = value; }
    public TraitNames NameAsEnum { get => nameAsEnum; set => nameAsEnum = value; }
    public float Intensity { get => intensity; set => intensity = value; }
    #endregion
    #region Constructor
    public TraitDataValue(string inputName, float inputIntensity)
    {
        nameAsString = inputName;
        intensity = inputIntensity;
    }
    public TraitDataValue(TraitNames inputName, float inputIntensity)
    {
        nameAsEnum = inputName;
        intensity = inputIntensity;
        nameAsString = nameAsEnum.ToString();
    }
    public enum TraitNames
    {
        friend,
        enemy,
        curiosity,
        creativity
    }
    #endregion
}
