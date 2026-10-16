using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class AudioSettingsAuthoring : MonoBehaviour
{
    [SerializeField]
    public Dictionary<AudioClipKeys, AudioClip> Clips = new();

    public class Baker : Baker<AudioSettingsAuthoring>
    {
        public override void Bake(AudioSettingsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            var buffer = AddBuffer<AudioClipElement>(entity);

            foreach (var entry in authoring.Clips)
                buffer.Add(new AudioClipElement { Key = entry.Key, Clip = entry.Value });
        }
    }
}
