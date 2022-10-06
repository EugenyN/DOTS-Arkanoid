using Unity.Entities;
using UnityEngine;

public class AudioSettingsDataAuthoring : MonoBehaviour
{
    public AudioClipEntry[] Clips;
    
    public class Baker : Baker<AudioSettingsDataAuthoring>
    {
        public override void Bake(AudioSettingsDataAuthoring authoring)
        {
            AddComponentObject(new AudioSettingsData
            {
                Clips = authoring.Clips
            });       
        }
    }
}