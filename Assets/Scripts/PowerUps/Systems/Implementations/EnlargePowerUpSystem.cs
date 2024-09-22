using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using BoxCollider = Unity.Physics.BoxCollider;
using Collider = Unity.Physics.Collider;

[UpdateInGroup(typeof(PowerUpsSystemGroup))]
public partial struct EnlargePowerUpSystem : ISystem, ISystemStartStop
{
    private BlobAssetReference<Collider> _normalColliderBlobAssetRef;
    private BlobAssetReference<Collider> _bigColliderBlobAssetRef;

    private BatchMaterialID _normalPaddleMaterial;
    private BatchMaterialID _bigPaddleMaterial;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameSettings>();
    }
    
    public void OnStartRunning(ref SystemState state)
    {
        var gameSettings = SystemAPI.GetSingleton<GameSettings>();

        if (!_normalColliderBlobAssetRef.IsCreated)
        {
            _normalColliderBlobAssetRef = BoxCollider.Create(new BoxGeometry
            {
                Center = float3.zero, Orientation = quaternion.identity, Size = gameSettings.PaddleSize,
                BevelRadius = 0.0f
            });
        }

        if (!_bigColliderBlobAssetRef.IsCreated)
        {
            _bigColliderBlobAssetRef = BoxCollider.Create(new BoxGeometry
            {
                Center = float3.zero, Orientation = quaternion.identity, Size = gameSettings.BigPaddleSize,
                BevelRadius = 0.0f
            });
        }
        
        var hybridRendererSystem = state.World.GetExistingSystemManaged<EntitiesGraphicsSystem>();
        
        var materialsConfig = SystemAPI.ManagedAPI.GetSingleton<PaddleMaterialsConfig>();
        
        if (_normalPaddleMaterial == BatchMaterialID.Null)
            _normalPaddleMaterial = hybridRendererSystem.RegisterMaterial(materialsConfig.NormalPaddleMaterial);
        if (_bigPaddleMaterial == BatchMaterialID.Null)
            _bigPaddleMaterial = hybridRendererSystem.RegisterMaterial(materialsConfig.BigPaddleMaterial);
    }
    
    public void OnStopRunning(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        if (_normalColliderBlobAssetRef.IsCreated)
            _normalColliderBlobAssetRef.Dispose();
        if (_bigColliderBlobAssetRef.IsCreated)
            _bigColliderBlobAssetRef.Dispose();
        
        // unregister materials ?
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var gameSettings = SystemAPI.GetSingleton<GameSettings>();
        
        new EnlargePowerUpJob
        {
            PaddleSize = gameSettings.PaddleSize,
            BigPaddleSize = gameSettings.BigPaddleSize,
            NormalColliderBlobAssetRef = _normalColliderBlobAssetRef,
            BigColliderBlobAssetRef = _bigColliderBlobAssetRef,
            NormalPaddleMaterial =_normalPaddleMaterial,
            BigPaddleMaterial = _bigPaddleMaterial
        }.Schedule();
    }
    
    [BurstCompile]
    public partial struct EnlargePowerUpJob : IJobEntity
    {
        public float3 PaddleSize;
        public float3 BigPaddleSize;
        
        public BlobAssetReference<Collider> NormalColliderBlobAssetRef;
        public BlobAssetReference<Collider> BigColliderBlobAssetRef;

        public BatchMaterialID NormalPaddleMaterial;
        public BatchMaterialID BigPaddleMaterial;
        
        private void Execute(ref PaddleData paddleData, ref PostTransformMatrix paddleScale,
            ref PhysicsCollider collider, ref MaterialMeshInfo mmi, in PowerUpReceivedEvent request)
        {
            if (paddleData.ExclusivePowerUp == request.Type)
                return;

            if (paddleData.ExclusivePowerUp == PowerUpType.Enlarge && PowerUpsHelper.IsExclusivePowerUp(request.Type))
            {
                ResizeCollider(ref paddleData, ref paddleScale, ref collider, PaddleSize, NormalColliderBlobAssetRef);
                mmi.MaterialID = NormalPaddleMaterial;
            }
                
            if (request.Type == PowerUpType.Enlarge)
            {
                ResizeCollider(ref paddleData, ref paddleScale, ref collider, BigPaddleSize, BigColliderBlobAssetRef);
                mmi.MaterialID = BigPaddleMaterial;
            }
        }
        
        private static void ResizeCollider(ref PaddleData paddleData, ref PostTransformMatrix paddleScale, 
            ref PhysicsCollider collider, float3 size, BlobAssetReference<Collider> blobAssetReference)
        {
            Debug.Assert(!paddleData.Size.Equals(size));
        
            paddleData.Size = size;
            paddleScale.Value = float4x4.Scale(size);
            collider.Value = blobAssetReference;
        }
    }
}