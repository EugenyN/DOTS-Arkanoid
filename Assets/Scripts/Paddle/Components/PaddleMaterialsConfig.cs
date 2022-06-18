using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public class PaddleMaterialsConfig : IComponentData
{
    public Material NormalPaddleMaterial;
    public Material BigPaddleMaterial;
}