using Unity.Entities;
using UnityEngine;

public class AudioSourceRefAuthoring : MonoBehaviour
{
    public class Baker : Baker<AudioSourceRefAuthoring>
    {
        public override void Bake(AudioSourceRefAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            var src = GetComponent<AudioSource>();
            AddComponent(entity, new AudioSourceRef { Value = src });
        }
    }
}
