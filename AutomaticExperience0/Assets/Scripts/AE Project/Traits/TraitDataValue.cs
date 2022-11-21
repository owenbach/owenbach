using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TraitDataValue : IComparable<float>
{
    #region Fields
    string nameAsString;
    [SerializeField] TraitNames nameAsEnum;
    [SerializeField] float intensity;
    #endregion

    #region Properties
    public string NameAsString { get => nameAsString; set => nameAsString = value; }
    public TraitNames NameAsEnum { get => nameAsEnum; set => nameAsEnum = value; }
    public float Intensity { get => intensity; set => intensity = value; }
    #endregion

    #region Functions

    public int CompareTo(float other)
    {
        return intensity.CompareTo(other);
    }

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
    #endregion
}
public enum TraitNames
{
    friend, // Is friend? How much?
    enemy, // Is Enemy? How Much?
    adventurous, // Likelyhood of going on a search for something new.
    curious, // Likelyhood of reviewing discovered entities to learn their functions.
    storer, // Likelyhood of collecting discovered entities in their inventory.
    associative, // Likelyhood of making and consulting friends.
}
