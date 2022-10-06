using Unity.Entities;
 using Unity.Mathematics;
 using Unity.Rendering;
 
 [MaterialProperty("_BaseMap_ST")]
 public struct MaterialTextureSTData : IComponentData
 {
     public float4 Value;
 }