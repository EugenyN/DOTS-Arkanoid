using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class SingleFrameComponentAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddBuffer<SingleFrameComponent>(entity);
    }
}