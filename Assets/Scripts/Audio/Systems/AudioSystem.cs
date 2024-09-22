using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial struct AudioSystem : ISystem, ISystemStartStop
{
    private Entity _audioSource;
    
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AudioSource>();
    } 

    public void OnStartRunning(ref SystemState state)
    {
        _audioSource = SystemAPI.QueryBuilder().WithAll<AudioSource>().Build().GetSingletonEntity();
    }
    
    public void OnStopRunning(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        
        foreach (var (stopAudioSource,  e) in SystemAPI.Query<RefRO<StopAudioSource>>().WithEntityAccess())
        {
            var audioSourceM = state.EntityManager.GetComponentObject<AudioSource>(_audioSource);
            audioSourceM.Stop();
            ecb.DestroyEntity(e);
        }
        
        foreach (var (startAudioSource,  e) in SystemAPI.Query<RefRO<StartAudioSource>>().WithEntityAccess())
        {
            var audioSettings = SystemAPI.ManagedAPI.GetSingleton<AudioSettingsData>();
            
            var audioSourceM = state.EntityManager.GetComponentObject<AudioSource>(_audioSource);
            audioSourceM.clip = audioSettings.Clips[startAudioSource.ValueRO.Key];
            audioSourceM.Play();
            ecb.DestroyEntity(e);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
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