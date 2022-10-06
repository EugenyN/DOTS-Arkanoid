using System;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class UISettingsAuthoring : MonoBehaviour
{
    [SerializeField] TitleUI _titleUI;
    [SerializeField] GamePanelUI _inGameUI;

    private void Awake()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entityManager.AddComponentObject(entityManager.CreateEntity(), _titleUI);
        entityManager.AddComponentObject(entityManager.CreateEntity(), _inGameUI);
    }
}