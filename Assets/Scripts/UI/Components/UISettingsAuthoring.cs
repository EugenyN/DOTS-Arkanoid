using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class UISettingsAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] TitleUI _titleUI;
    [SerializeField] GamePanelUI _inGameUI;
    
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentObject(entity, _titleUI);
        dstManager.AddComponentObject(entity, _inGameUI);
    }
}