using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class MorphAnimation : MonoBehaviour 
{
    [HideInInspector] public bool IsApprove = false;
    [HideInInspector] public bool IsDone = false;
    public List<MorphTriangle> Triangles;
    public List<MorphVertex> Vertexs;

    public bool IsPosition = true;
    public bool IsRotation = true;
    public bool IsScale = false;
    public List<Transform> Bones;
    public List<MorphAnimationFrame> AnimationFrames;
    public List<MorphAnimationFrame> TimeLine;
}
