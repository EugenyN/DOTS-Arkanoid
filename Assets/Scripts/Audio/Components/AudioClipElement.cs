using System;
using Unity.Entities;
using UnityEngine;

public enum AudioClipKeys
{
    BallLoss,
    PowerUpReceived,
    BlockDamage,
    BlockDestroy,
    GameOver,
    PaddleHit,
    PaddleCatch,
    RoundCleared,
    RoundStart,
    LaserShot
}

[Serializable]
public struct AudioClipElement : IBufferElementData
{
    public AudioClipKeys Key;
    public UnityObjectRef<AudioClip> Clip;
}