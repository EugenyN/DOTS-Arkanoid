using System;
using Unity.Entities;

[Serializable]
public struct InputNames
{
    public string MouseMove;
    public string MouseAction;
    public string Move;
    public string Pause;
    public string Action;
}

public class InputSettings : IComponentData
{
    public InputNames[] InputNames;
    public float MouseSensitivity;
}