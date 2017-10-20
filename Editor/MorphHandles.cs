using UnityEngine;
using UnityEditor;

public class MorphHandles 
{
    public static void DrawMorphBone(Transform transform, float size)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform tf = transform.GetChild(i);
            if (tf)
            {
                DrawWrieBoneIncludeChildren(tf, size);
            }
        }
    }

    public static void DrawMorphVertex(Vector3 position, float size, HandleType ht)
    {
        if (ht == HandleType.Solid)
        {
            Handles.SphereCap(0, position, Quaternion.identity, size);
        }
        else
        {
            Handles.DotCap(0, position, Quaternion.identity, size);
        }
    }

    private static void DrawWrieBoneIncludeChildren(Transform bone, float size)
    {
        DrawWrieBone(bone, size);
        for (int i = 0; i < bone.childCount; i++)
        {
            Transform tf = bone.GetChild(i);
            if (tf)
            {
                DrawWrieBoneLine(bone, tf, size);
                DrawWrieBoneIncludeChildren(tf, size);
            }
        }
    }
    
    private static void DrawWrieBone(Transform bone, float size)
    {
        Handles.DrawWireCube(bone.position, new Vector3(size, size, size));
    }
    
    private static void DrawWrieBoneLine(Transform bone1, Transform bone2, float size)
    {
        Vector3 bone1v = new Vector3(bone1.position.x, bone1.position.y + size * 0.6f, bone1.position.z);
        Vector3 bone2v = new Vector3(bone2.position.x, bone2.position.y + size * 0.6f, bone2.position.z);
        Handles.DrawLine(bone1v, bone2v);

        bone1v = new Vector3(bone1.position.x, bone1.position.y - size * 0.6f, bone1.position.z);
        bone2v = new Vector3(bone2.position.x, bone2.position.y - size * 0.6f, bone2.position.z);
        Handles.DrawLine(bone1v, bone2v);

        bone1v = new Vector3(bone1.position.x, bone1.position.y, bone1.position.z + size * 0.6f);
        bone2v = new Vector3(bone2.position.x, bone2.position.y, bone2.position.z + size * 0.6f);
        Handles.DrawLine(bone1v, bone2v);

        bone1v = new Vector3(bone1.position.x, bone1.position.y, bone1.position.z - size * 0.6f);
        bone2v = new Vector3(bone2.position.x, bone2.position.y, bone2.position.z - size * 0.6f);
        Handles.DrawLine(bone1v, bone2v);
    }
}
