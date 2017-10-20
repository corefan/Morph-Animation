using System;

[Serializable]
public class MorphTriangle : MorphBase
{
    public MorphVertex Vertex1;
    public MorphVertex Vertex2;
    public MorphVertex Vertex3;

    public MorphTriangle(int id, ref MorphVertex vertex1, ref MorphVertex vertex2, ref MorphVertex vertex3)
    {
        Vertex1 = vertex1;
        Vertex2 = vertex2;
        Vertex3 = vertex3;
    }
}
