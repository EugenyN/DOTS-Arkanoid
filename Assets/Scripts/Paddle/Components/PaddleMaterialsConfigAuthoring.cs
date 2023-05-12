using Unity.Entities;
using UnityEngine;

public class PaddleMaterialsConfigAuthoring : MonoBehaviour
{
    public Material NormalPaddleMaterial;
    public Material BigPaddleMaterial;
    
    public class Baker : Baker<PaddleMaterialsConfigAuthoring>
    {
        public override void Bake(PaddleMaterialsConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, new PaddleMaterialsConfig
            {
                NormalPaddleMaterial = authoring.NormalPaddleMaterial,
                BigPaddleMaterial = authoring.BigPaddleMaterial
            });
        }
    }
}