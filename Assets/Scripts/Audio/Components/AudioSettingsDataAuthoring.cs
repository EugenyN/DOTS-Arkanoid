using Unity.Entities;
using UnityEngine;

public class AudioSettingsDataAuthoring : MonoBehaviour
{
    public AudioClipEntry[] Clips;
    
    public class Baker : Baker<AudioSettingsDataAuthoring>
    {
        public override void Bake(AudioSettingsDataAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, new AudioSettingsData
            {
                Clips = authoring.Clips
            });       
        }
    }
}