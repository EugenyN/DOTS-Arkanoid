using Unity.Entities;
using UnityEngine;

public struct PaddleMaterialsConfig : IComponentData
{
    public UnityObjectRef<Material> NormalPaddleMaterial;
    public UnityObjectRef<Material> BigPaddleMaterial;
}
