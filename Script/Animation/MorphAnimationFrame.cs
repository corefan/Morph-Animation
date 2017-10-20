using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class MorphAnimationFrame 
{
    public string name;
    public Vector2 EditorPosition;
    public List<Vector3> Positions;
    public List<Quaternion> Rotations;
    public List<Vector3> Scales;
    public float Time;

    public MorphAnimationFrame(string value, Vector2 ep)
    {
        name = value;
        EditorPosition = ep;
        Positions = new List<Vector3>();
        Rotations = new List<Quaternion>();
        Scales = new List<Vector3>();
        Time = 1;
    }
}
