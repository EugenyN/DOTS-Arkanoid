using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;

public partial class EnlargePowerUpSystem : SystemBase
{
    private BlobAssetReference<Collider> _normalColliderBlobAssetRef;
    private BlobAssetReference<Collider> _bigColliderBlobAssetRef;
    
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    
    protected override void OnCreate()
    {
        base.OnCreate();
        
        _endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        
        RequireSingletonForUpdate<PowerUpReceivedEvent>();
    }
    
    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        
        var gameSettings = GetSingleton<GameSettings>();
        
        _normalColliderBlobAssetRef = BoxCollider.Create(new BoxGeometry
        {
            Center = float3.zero, Orientation = quaternion.identity, Size = gameSettings.PaddleSize,
            BevelRadius = 0.0f
        });
        
        _bigColliderBlobAssetRef = BoxCollider.Create(new BoxGeometry
        {
            Center = float3.zero, Orientation = quaternion.identity, Size = gameSettings.BigPaddleSize,
            BevelRadius = 0.0f
        });
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        if (_normalColliderBlobAssetRef.IsCreated)
            _normalColliderBlobAssetRef.Dispose();
        if (_bigColliderBlobAssetRef.IsCreated)
            _bigColliderBlobAssetRef.Dispose();
    }
    
    protected override void OnUpdate()
    {
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();
        
        var gameSettings = GetSingleton<GameSettings>();

        var normalColliderBlobAssetRef = _normalColliderBlobAssetRef;
        var bigColliderBlobAssetRef = _bigColliderBlobAssetRef;

        Entities
            .WithStructuralChanges()
            .ForEach((Entity paddle, ref PaddleData paddleData, ref CompositeScale paddleScale,
                ref PhysicsCollider collider, in PowerUpReceivedEvent request,
                in RenderMesh renderMesh, in PaddleMaterialsConfig materialsConfig) =>
            {
                if (paddleData.ExclusivePowerUp == request.Type)
                    return;

                if (paddleData.ExclusivePowerUp == PowerUpType.Enlarge && PowerUpsHelper.IsExclusivePowerUp(request.Type))
                {
                    ResizeCollider(ref paddleData, ref paddleScale, ref collider, gameSettings.PaddleSize,
                        normalColliderBlobAssetRef);
                    ChangeMaterial(ecb, paddle, renderMesh, materialsConfig.NormalPaddleMaterial);
                }

                if (request.Type == PowerUpType.Enlarge)
                {
                    ResizeCollider(ref paddleData, ref paddleScale, ref collider, gameSettings.BigPaddleSize,
                        bigColliderBlobAssetRef);
                    ChangeMaterial(ecb, paddle, renderMesh, materialsConfig.BigPaddleMaterial);
                }
            }).Run();
        
        _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }

    private static void ResizeCollider(ref PaddleData paddleData, ref CompositeScale paddleScale, 
                                ref PhysicsCollider collider, float3 size, BlobAssetReference<Collider> blobAssetReference)
    {
        UnityEngine.Debug.Assert(!paddleData.Size.Equals(size));
        
        paddleData.Size = size;
        paddleScale.Value = float4x4.Scale(size);
        collider.Value = blobAssetReference;
    }

    private static void ChangeMaterial(EntityCommandBuffer ecb, Entity paddle, in RenderMesh renderMesh, 
        UnityEngine.Material material)
    {
        var modifiedMesh = renderMesh;
        modifiedMesh.material = material;
        ecb.SetSharedComponent(paddle, modifiedMesh);
    }
}