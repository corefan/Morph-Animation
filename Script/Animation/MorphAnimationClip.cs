using UnityEngine;
using System.Collections.Generic;

public class MorphAnimationClip : ScriptableObject 
{
    public List<MorphAnimationFrame> AnimationFrames;
    public bool IsPosition;
    public bool IsRotation;
    public bool IsScale;
}
