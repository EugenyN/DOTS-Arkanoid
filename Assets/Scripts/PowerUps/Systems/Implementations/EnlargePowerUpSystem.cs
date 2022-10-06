using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using BoxCollider = Unity.Physics.BoxCollider;
using Collider = Unity.Physics.Collider;

public partial class EnlargePowerUpSystem : SystemBase
{
    private BlobAssetReference<Collider> _normalColliderBlobAssetRef;
    private BlobAssetReference<Collider> _bigColliderBlobAssetRef;

    private BatchMaterialID _normalPaddleMaterial;
    private BatchMaterialID _bigPaddleMaterial;

    protected override void OnCreate()
    {
        base.OnCreate();
        
        RequireForUpdate<PowerUpReceivedEvent>();
    }
    
    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        
        var gameSettings = GetSingleton<GameSettings>();

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

        var hybridRendererSystem = World.GetExistingSystemManaged<EntitiesGraphicsSystem>();
        
        var materialsConfig = this.GetSingleton<PaddleMaterialsConfig>();
        
        if (_normalPaddleMaterial == BatchMaterialID.Null)
            _normalPaddleMaterial = hybridRendererSystem.RegisterMaterial(materialsConfig.NormalPaddleMaterial);
        if (_bigPaddleMaterial == BatchMaterialID.Null)
            _bigPaddleMaterial = hybridRendererSystem.RegisterMaterial(materialsConfig.BigPaddleMaterial);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        if (_normalColliderBlobAssetRef.IsCreated)
            _normalColliderBlobAssetRef.Dispose();
        if (_bigColliderBlobAssetRef.IsCreated)
            _bigColliderBlobAssetRef.Dispose();
        
        // unregister materials ?
    }
    
    protected override void OnUpdate()
    {
        var gameSettings = GetSingleton<GameSettings>();

        var normalColliderBlobAssetRef = _normalColliderBlobAssetRef;
        var bigColliderBlobAssetRef = _bigColliderBlobAssetRef;

        var normalPaddleMaterial = _normalPaddleMaterial;
        var bigPaddleMaterial = _bigPaddleMaterial;

        Entities
            .ForEach((Entity paddle, ref PaddleData paddleData, ref CompositeScale paddleScale,
                ref PhysicsCollider collider, ref MaterialMeshInfo mmi, in PowerUpReceivedEvent request) =>
            {
                if (paddleData.ExclusivePowerUp == request.Type)
                    return;

                if (paddleData.ExclusivePowerUp == PowerUpType.Enlarge && PowerUpsHelper.IsExclusivePowerUp(request.Type))
                {
                    ResizeCollider(ref paddleData, ref paddleScale, ref collider, gameSettings.PaddleSize,
                        normalColliderBlobAssetRef);
                    mmi.MaterialID = normalPaddleMaterial;
                }
                
                if (request.Type == PowerUpType.Enlarge)
                {
                    ResizeCollider(ref paddleData, ref paddleScale, ref collider, gameSettings.BigPaddleSize,
                        bigColliderBlobAssetRef);
                    mmi.MaterialID = bigPaddleMaterial;
                }
            }).Run();
    }

    private static void ResizeCollider(ref PaddleData paddleData, ref CompositeScale paddleScale, 
                                ref PhysicsCollider collider, float3 size, BlobAssetReference<Collider> blobAssetReference)
    {
        Debug.Assert(!paddleData.Size.Equals(size));
        
        paddleData.Size = size;
        paddleScale.Value = float4x4.Scale(size);
        collider.Value = blobAssetReference;
    }
}