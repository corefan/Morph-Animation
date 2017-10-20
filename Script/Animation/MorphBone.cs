using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MorphBone : MonoBehaviour
{
    //外封套
    public float ExternalRange;
    //内封套
    public float InternalRange;
    
    public List<MorphVertex> ExternalRangeVertexs;
    public List<MorphVertex> InternalRangeVertexs;
    public List<MorphVertex> ErrorVertexs;
    public bool IsHideInRootBone;

    public MorphBone()
    {
        ExternalRange = 1;
        InternalRange = 0.5f;
        ExternalRangeVertexs = new List<MorphVertex>();
        InternalRangeVertexs = new List<MorphVertex>();
        ErrorVertexs = new List<MorphVertex>();
        IsHideInRootBone = false;
    }
}
