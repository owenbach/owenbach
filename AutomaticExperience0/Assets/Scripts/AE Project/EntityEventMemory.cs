using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class EntityEventMemory
{
    #region Fields
    string name = "";
    System.Reflection.MethodInfo functionMethodInfo;
    EntityTraits traitChange;
    EntityPhysicalAttributes physicalAttributeChange;
    #endregion
    #region Properties
    public string Name { get => name; set => name = value; }
    public System.Reflection.MethodInfo FunctionMethodInfo { get => functionMethodInfo; set => functionMethodInfo = value; }
    public EntityTraits TraitChange { get => traitChange; set => traitChange = value; }
    public EntityPhysicalAttributes PhysicalAttributeChange { get => physicalAttributeChange; set => physicalAttributeChange = value; }
    #endregion
}
