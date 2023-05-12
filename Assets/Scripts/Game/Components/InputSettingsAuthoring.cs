using System;
using Unity.Entities;
using UnityEngine;

public class InputSettingsAuthoring : MonoBehaviour
{
    public InputNames[] InputNames;
    
    public class Baker : Baker<InputSettingsAuthoring>
    {
        public override void Bake(InputSettingsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, new InputSettings
            {
                InputNames = authoring.InputNames
            });       
        }
    }
}