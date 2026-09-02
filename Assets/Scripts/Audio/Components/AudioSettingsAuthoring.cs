using Unity.Entities;
using UnityEngine;

public class AudioSettingsAuthoring : MonoBehaviour
{
    public AudioClipElement[] Clips;

    public class Baker : Baker<AudioSettingsAuthoring>
    {
        public override void Bake(AudioSettingsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            var buffer = AddBuffer<AudioClipElement>(entity);

            if (authoring.Clips == null)
                return;

            foreach (var entry in authoring.Clips)
                buffer.Add(entry);
        }
    }
}
