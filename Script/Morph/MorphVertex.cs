using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class MorphVertex : MorphBase
{
    public Vector3 Vertex;
    public List<int> VertexIndexs;
    public MorphBoneWeight VertexBoneWeight;

    public MorphVertex(Vector3 vertex, List<int> vertexIndexs)
    {
        Vertex = vertex;
        VertexIndexs = vertexIndexs;
        VertexBoneWeight = new MorphBoneWeight();
    }
}
