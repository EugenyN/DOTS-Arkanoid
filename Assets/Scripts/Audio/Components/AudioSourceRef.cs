using Unity.Entities;
using UnityEngine;

public struct AudioSourceRef : IComponentData
{
    public UnityObjectRef<AudioSource> Value;
}
