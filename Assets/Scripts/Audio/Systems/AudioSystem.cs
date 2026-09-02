using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial struct AudioSystem : ISystem, ISystemStartStop
{
    private Entity _audioSource;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AudioSourceRef>();
        state.RequireForUpdate<AudioClipElement>();
    }

    public void OnStartRunning(ref SystemState state)
    {
        _audioSource = SystemAPI.QueryBuilder().WithAll<AudioSourceRef>().Build().GetSingletonEntity();
        var audioSource = SystemAPI.GetComponent<AudioSourceRef>(_audioSource).Value.Value;
        audioSource.clip = null;
    }
    
    public void OnStopRunning(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        
        foreach (var (_, e) in SystemAPI.Query<RefRO<StopAudioSource>>().WithEntityAccess())
        {
            var audioSource = SystemAPI.GetComponent<AudioSourceRef>(_audioSource).Value.Value;
            audioSource.Stop();
            ecb.DestroyEntity(e);
        }

        foreach (var (start, e) in SystemAPI.Query<RefRO<StartAudioSource>>().WithEntityAccess())
        {
            var audioSource = SystemAPI.GetComponent<AudioSourceRef>(_audioSource).Value.Value;
            var clips = SystemAPI.GetSingletonBuffer<AudioClipElement>(true);
            audioSource.clip = ResolveClip(clips, start.ValueRO.Key);
            audioSource.Play();
            ecb.DestroyEntity(e);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private static AudioClip ResolveClip(in DynamicBuffer<AudioClipElement> clips, AudioClipKeys key)
    {
        foreach (var c in clips)
            if (c.Key == key) return c.Clip.Value;
        Debug.LogWarning($"[AudioSystem] AudioClip for key '{key}' not found.");
        return null;
    }

    public static void PlayAudio(EntityCommandBuffer ecb, AudioClipKeys clipKey)
    {
        var entity = ecb.CreateEntity();
#if UNITY_EDITOR
        ecb.SetName(entity, $"PlayAudio({clipKey})");
#endif
        ecb.AddComponent(entity, new StartAudioSource { Key = clipKey });
    }

    public static void PlayAudio(EntityManager em, AudioClipKeys clipKey)
    {
        var entity = em.CreateEntity();
#if UNITY_EDITOR
        em.SetName(entity, $"PlayAudio({clipKey})");
#endif
        em.AddComponentData(entity, new StartAudioSource { Key = clipKey });
    }

    public static void StopAudio(EntityCommandBuffer ecb)
    {
        var entity = ecb.CreateEntity();
#if UNITY_EDITOR
        ecb.SetName(entity, "StopAudio");
#endif
        ecb.AddComponent(entity, new StopAudioSource());
    }

    public static void StopAudio(EntityManager em)
    {
        var entity = em.CreateEntity();
#if UNITY_EDITOR
        em.SetName(entity, "StopAudio");
#endif
        em.AddComponentData(entity, new StopAudioSource());
    }
}
