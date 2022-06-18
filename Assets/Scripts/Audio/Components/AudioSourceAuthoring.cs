using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class AudioSourceAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField]
    private AudioSource _audioSource;
    
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentObject(entity, _audioSource);
    }
}