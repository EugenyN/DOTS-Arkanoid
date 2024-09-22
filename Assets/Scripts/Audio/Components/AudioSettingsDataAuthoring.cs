using System.Linq;
using Unity.Entities;
using UnityEngine;

public class AudioSettingsDataAuthoring : MonoBehaviour
{
    public AudioClipEntry[] Clips;
    
    public class Baker : Baker<AudioSettingsDataAuthoring>
    {
        public override void Bake(AudioSettingsDataAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            
            AddComponentObject(entity, new AudioSettingsData
            {
                Clips = authoring.Clips.ToDictionary(clip => clip.Key, clip => clip.Clip)
            });       
        }
    }
}