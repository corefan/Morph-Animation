using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MorphBoneWeight : MorphBase
{
    public Transform bone0;
    public Transform bone1;
    public Transform bone2;
    public Transform bone3;
    public float weight0;
    public float weight1;
    public float weight2;
    public float weight3;

    public MorphBoneWeight()
    {
        bone0 = null;
        bone1 = null;
        bone2 = null;
        bone3 = null;
        weight0 = 1;
        weight1 = 0;
        weight2 = 0;
        weight3 = 0;
    }

    private float GetWeights()
    {
        return weight0 + weight1 + weight2 + weight3;
    }

    public BoneWeight ToBoneWeight(List<Transform> bones)
    {
        BoneWeight bw = new BoneWeight();
        bw.boneIndex0 = bone0 ? bones.IndexOf(bone0) : 0;
        bw.boneIndex1 = bone1 ? bones.IndexOf(bone1) : 0;
        bw.boneIndex2 = bone2 ? bones.IndexOf(bone2) : 0;
        bw.boneIndex3 = bone3 ? bones.IndexOf(bone3) : 0;
        bw.weight0 = weight0;
        bw.weight1 = weight1;
        bw.weight2 = weight2;
        bw.weight3 = weight3;

        return bw;
    }

    /// <summary>
    /// 限制所有骨骼权重值为1
    /// </summary>
    public void AstrictWeights()
    {
        float oldValue = GetWeights();
        if (oldValue <= 0f) oldValue = 1f;

        weight0 = weight0 / oldValue;
        weight1 = weight1 / oldValue;
        weight2 = weight2 / oldValue;
        weight3 = weight3 / oldValue;
    }

    /// <summary>
    /// （排除索引0骨骼）限制所有骨骼权重值为1
    /// </summary>
    public void AstrictWeightsExclude0()
    {
        float oldValue = weight1 + weight2 + weight3;
        if (oldValue <= 0f) oldValue = 1f;

        float percent = 1.0f - weight0;
        weight1 = weight1 / oldValue * percent;
        weight2 = weight2 / oldValue * percent;
        weight3 = weight3 / oldValue * percent;
    }

    /// <summary>
    /// （排除索引1骨骼）限制所有骨骼权重值为1
    /// </summary>
    public void AstrictWeightsExclude1()
    {
        float oldValue = weight0 + weight2 + weight3;
        if (oldValue <= 0f) oldValue = 1f;

        float percent = 1.0f - weight1;
        weight0 = weight0 / oldValue * percent;
        weight2 = weight2 / oldValue * percent;
        weight3 = weight3 / oldValue * percent;
    }

    /// <summary>
    /// （排除索引2骨骼）限制所有骨骼权重值为1
    /// </summary>
    public void AstrictWeightsExclude2()
    {
        float oldValue = weight0 + weight1 + weight3;
        if (oldValue <= 0f) oldValue = 1f;

        float percent = 1.0f - weight2;
        weight0 = weight0 / oldValue * percent;
        weight1 = weight1 / oldValue * percent;
        weight3 = weight3 / oldValue * percent;
    }

    /// <summary>
    /// （排除索引3骨骼）限制所有骨骼权重值为1
    /// </summary>
    public void AstrictWeightsExclude3()
    {
        float oldValue = weight0 + weight1 + weight2;
        if (oldValue <= 0f) oldValue = 1f;

        float percent = 1.0f - weight3;
        weight0 = weight0 / oldValue * percent;
        weight1 = weight1 / oldValue * percent;
        weight2 = weight2 / oldValue * percent;
    }
}
