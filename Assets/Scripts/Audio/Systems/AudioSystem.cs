using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class AudioSystem : SystemBase
{
    private Entity _audioSource;
    
    private readonly Dictionary<AudioClipKeys, AudioClip> _clips = new Dictionary<AudioClipKeys, AudioClip>();

    protected override void OnCreate()
    {
        base.OnCreate();
        
        RequireForUpdate<AudioSource>();
    }

    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        
        _audioSource = GetEntityQuery(typeof(AudioSource)).GetSingletonEntity();

        var audioSettings = SystemAPI.ManagedAPI.GetSingleton<AudioSettingsData>();
        foreach (var clip in audioSettings.Clips)
            _clips.Add(clip.Key, clip.Clip);
    }

    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        Entities
            .WithoutBurst()
            .ForEach((Entity e, ref StopAudioSource stopAudioSource) =>
            {
                var audioSourceM = EntityManager.GetComponentObject<AudioSource>(_audioSource);
                audioSourceM.Stop();
                ecb.DestroyEntity(e);
            }).Run();

        Entities
            .WithoutBurst()
            .ForEach((Entity e, in StartAudioSource startAudioSource) =>
            {
                var audioSourceM = EntityManager.GetComponentObject<AudioSource>(_audioSource);
                audioSourceM.clip = _clips[startAudioSource.Key];
                audioSourceM.Play();
                ecb.DestroyEntity(e);
            }).Run();

        ecb.Playback(EntityManager);
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