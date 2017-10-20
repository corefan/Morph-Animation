using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(MeshFilter))]
public class MeshFilterEditor : Editor 
{
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private SkinnedMeshRenderer _skinnedMeshRenderer;

    private void OnEnable()
    {
        _meshFilter = target as MeshFilter;
        _meshRenderer = _meshFilter.transform.GetComponent<MeshRenderer>();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GenerateMorphAnimation();
    }
    /// <summary>
    /// 创建变形动画
    /// </summary>
    private void GenerateMorphAnimation()
    {
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Generate Morph Animation"))
        {
            Mesh mesh = Instantiate(_meshFilter.sharedMesh);
            mesh.name = _meshFilter.sharedMesh.name + "(Morph)";
            string path = "Assets/" + mesh.name + ".asset";
            AssetDatabase.CreateAsset(mesh, path);
            AssetDatabase.SaveAssets();
            Mesh meshAsset = AssetDatabase.LoadAssetAtPath(path, typeof(Mesh)) as Mesh;

            //生成蒙皮网格组件
            _skinnedMeshRenderer = _meshFilter.transform.GetComponent<SkinnedMeshRenderer>();
            if (_skinnedMeshRenderer)
            {
                DestroyImmediate(_skinnedMeshRenderer);
            }
            _skinnedMeshRenderer = _meshFilter.transform.gameObject.AddComponent<SkinnedMeshRenderer>();
            _skinnedMeshRenderer.hideFlags = HideFlags.HideInInspector;
            _skinnedMeshRenderer.sharedMesh = meshAsset;
            _skinnedMeshRenderer.rootBone = _meshFilter.transform;
            _skinnedMeshRenderer.sharedMaterial = _meshRenderer ? _meshRenderer.sharedMaterial : null;
            _skinnedMeshRenderer.enabled = true;

            //生成网格碰撞器
            MeshCollider mc = _meshFilter.transform.GetComponent<MeshCollider>();
            if (mc)
            {
                DestroyImmediate(mc);
            }
            mc = _meshFilter.transform.gameObject.AddComponent<MeshCollider>();
            mc.sharedMesh = meshAsset;
            mc.enabled = true;

            //生成变形动画组件
            MorphAnimation ma = _meshFilter.transform.GetComponent<MorphAnimation>();
            if (ma)
            {
                DestroyImmediate(ma);
            }
            ma = _meshFilter.transform.gameObject.AddComponent<MorphAnimation>();
            ma.IsApprove = true;
            //处理顶点
            ma.Vertexs = new List<MorphVertex>();
            List<int> repetitionVertices = new List<int>();
            for (int i = 0; i < meshAsset.vertices.Length; i++)
            {
                EditorUtility.DisplayProgressBar("请稍等", "处理顶点（" + i + "/" + meshAsset.vertices.Length + "）......", 1.0f / meshAsset.vertices.Length * i);

                if (repetitionVertices.Contains(i))
                    continue;

                List<int> verticesGroup = new List<int>();
                verticesGroup.Add(i);

                for (int j = i + 1; j < meshAsset.vertices.Length; j++)
                {
                    if (meshAsset.vertices[i] == meshAsset.vertices[j])
                    {
                        verticesGroup.Add(j);
                        repetitionVertices.Add(j);
                    }
                }
                ma.Vertexs.Add(new MorphVertex(_meshFilter.transform.localToWorldMatrix.MultiplyPoint3x4(meshAsset.vertices[i]), verticesGroup));
            }
            //处理三角面
            List<int> allTriangles = new List<int>(meshAsset.triangles);
            ma.Triangles = new List<MorphTriangle>();
            for (int i = 0; (i + 2) < allTriangles.Count; i += 3)
            {
                EditorUtility.DisplayProgressBar("请稍等", "处理三角面（" + i + "/" + allTriangles.Count + "）......", 1.0f / allTriangles.Count * i);

                MorphVertex mv1 = GetVertexByIndex(ma.Vertexs, allTriangles[i]);
                MorphVertex mv2 = GetVertexByIndex(ma.Vertexs, allTriangles[i + 1]);
                MorphVertex mv3 = GetVertexByIndex(ma.Vertexs, allTriangles[i + 2]);
                MorphTriangle mt = new MorphTriangle(ma.Triangles.Count, ref mv1, ref mv2, ref mv3);
                ma.Triangles.Add(mt);
            }
            //默认生成一条骨骼（根骨骼）
            Transform[] bones = new Transform[1];
            Matrix4x4[] bindPoses = new Matrix4x4[1];
            GameObject go = new GameObject("BoneRoot");
            go.AddComponent<MorphBone>();
            go.GetComponent<MorphBone>().hideFlags = HideFlags.HideInInspector;
            go.hideFlags = HideFlags.HideInHierarchy;
            bones[0] = go.transform;
            bones[0].SetParent(_skinnedMeshRenderer.transform);
            bones[0].localPosition = Vector3.zero;
            bones[0].localRotation = Quaternion.identity;
            bindPoses[0] = bones[0].worldToLocalMatrix * _skinnedMeshRenderer.transform.localToWorldMatrix;
            _skinnedMeshRenderer.bones = bones;
            _skinnedMeshRenderer.sharedMesh.bindposes = bindPoses;
            EditorUtility.ClearProgressBar();


            if (_meshFilter.transform.GetComponent<Collider>() != null)
            {
                _meshFilter.transform.GetComponent<Collider>().enabled = false;
            }
            if (_meshRenderer)
            {
                _meshRenderer.enabled = false;
                _meshRenderer.hideFlags = HideFlags.HideInInspector;
            }
            DestroyImmediate(_meshFilter);
        }
    }

    #region Assist Method
    private MorphVertex GetVertexByIndex(List<MorphVertex> vertexs, int index)
    {
        for (int i = 0; i < vertexs.Count; i++)
        {
            if (vertexs[i].VertexIndexs.Contains(index))
            {
                return vertexs[i];
            }
        }
        return null;
    }
    #endregion
}
