using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(MorphAnimation))]
public class MorphAnimationEditor : Editor
{
    #region Field
    private MorphAnimation _morphAnimation;
    private Transform _transform;
    private SkinnedMeshRenderer _skinnedMeshRenderer;
    private Mesh _mesh;
    private MeshCollider _meshCollider;
    private Transform _currentCheckedBone;
    private MorphBone _currentCheckedMorphBone;
    private MorphVertex _currentCheckedVertex;
    private MAEditType _MAEditType;
    private MATool _MATool;
    private HandleType _vertexHandleType;
    private float _vertexIconSize;
    private float _boneSize;
    private bool _reNameBone;
    private string _newNameBone;

    private bool _showMorphSetting;
    private bool _showRenderSetting;
    private bool _showMaterials;
    #endregion

    private void OnEnable()
    {
        _morphAnimation = target as MorphAnimation;
        ReSet();

        if (!_morphAnimation.IsApprove)
            return;

        if (_morphAnimation.IsDone)
            return;

        _transform = _morphAnimation.transform;
        _skinnedMeshRenderer = _transform.GetComponent<SkinnedMeshRenderer>();
        _mesh = _skinnedMeshRenderer.sharedMesh;
        _meshCollider = _transform.GetComponent<MeshCollider>();
        _currentCheckedBone = null;
        _currentCheckedMorphBone = null;
        _currentCheckedVertex = null;
        _MAEditType = MAEditType.Bone;
        _MATool = MATool.Move;
        _vertexHandleType = HandleType.Wire;
        _vertexIconSize = 0.01f;
        _boneSize = 0.1f;
        _reNameBone = false;
        _newNameBone = "";

        _showMorphSetting = true;
        _showRenderSetting = true;
        _showMaterials = false;

        SaveMorphMesh();
    }

    public override void OnInspectorGUI()
    {
        if (Application.isPlaying)
            return;

        if (!_morphAnimation.IsApprove)
            return;

        if (_morphAnimation.IsDone)
        {
            DoneGUI();
            return;
        }

        PreviewTitleGUI();
        PreviewBoneGUI();
        PreviewVertexGUI();
        SceneView.RepaintAll();
    }
    /// <summary>
    /// 标题栏
    /// </summary>
    private void PreviewTitleGUI()
    {
        SetGUIEnabled(true);
        SetGUIColor(Color.white, Color.white);

        #region renderSetting
        EditorGUILayout.BeginVertical("HelpBox");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("RenderSetting", "PreLabel"))
        {
            _showRenderSetting = !_showRenderSetting;
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        if (_showRenderSetting)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Mesh");
            GUILayout.FlexibleSpace();
            _skinnedMeshRenderer.sharedMesh = EditorGUILayout.ObjectField(_skinnedMeshRenderer.sharedMesh, typeof(Mesh), false) as Mesh;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            _showMaterials = EditorGUILayout.Foldout(_showMaterials, "Materials");
            EditorGUILayout.EndHorizontal();

            if (_showMaterials)
            {
                for (int i = 0; i < _skinnedMeshRenderer.sharedMaterials.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    GUILayout.Label("Element " + i);
                    _skinnedMeshRenderer.sharedMaterials[i] = EditorGUILayout.ObjectField(_skinnedMeshRenderer.sharedMaterials[i], typeof(Material), false) as Material;
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Cast Shadows");
            GUILayout.FlexibleSpace();
            _skinnedMeshRenderer.shadowCastingMode = (ShadowCastingMode)EditorGUILayout.EnumPopup(_skinnedMeshRenderer.shadowCastingMode);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Receive Shadows");
            GUILayout.FlexibleSpace();
            _skinnedMeshRenderer.receiveShadows = EditorGUILayout.Toggle(_skinnedMeshRenderer.receiveShadows);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
        #endregion

        #region morphSetting
        EditorGUILayout.BeginVertical("HelpBox");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("MorphSetting", "PreLabel"))
        {
            _showMorphSetting = !_showMorphSetting;
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        if (_showMorphSetting)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("EditType");
            _MAEditType = (MAEditType)EditorGUILayout.EnumPopup(_MAEditType, GUILayout.Width(55));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Apply", "Minibutton"))
            {
                SaveMorphMesh();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Vertex Size");
            _vertexIconSize = EditorGUILayout.Slider(_vertexIconSize, 0.01f, 0.1f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Bone Size");
            _boneSize = EditorGUILayout.Slider(_boneSize, 0.1f, 1f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Done & Open In Editor Window"))
            {
                if (EditorUtility.DisplayDialog("警告", "确认之后将无法再次更改骨骼及蒙皮信息！", "确定", "取消"))
                {
                    Done();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
        #endregion
    }
    /// <summary>
    /// 预览骨骼
    /// </summary>
    private void PreviewBoneGUI()
    {
        if (_MAEditType == MAEditType.Bone)
        {
            SetGUIColor(Color.white, Color.white);
            EditorGUILayout.BeginHorizontal("HelpBox");
            GUILayout.Label("Bone List ( Number " + _skinnedMeshRenderer.bones.Length + " )", "PreLabel");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add", "Minibutton", GUILayout.Width(35)))
            {
                AddBone();
            }
            if (_currentCheckedBone)
            {
                if (GUILayout.Button("AddSub", "Minibutton", GUILayout.Width(60)))
                {
                    AddSubBone();
                }
                if (GUILayout.Button("ReName", "Minibutton", GUILayout.Width(60)))
                {
                    _reNameBone = !_reNameBone;
                }
                if (GUILayout.Button("Delete", "Minibutton", GUILayout.Width(50)))
                {
                    DeleteBone();
                }
            }
            EditorGUILayout.EndHorizontal();

            if (_currentCheckedBone && _reNameBone)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                _newNameBone = GUILayout.TextField(_newNameBone, GUILayout.Width(100));
                if (GUILayout.Button("Sure", "minibutton"))
                {
                    _reNameBone = !_reNameBone;
                    if (_newNameBone != "")
                    {
                        _currentCheckedBone.name = _newNameBone;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            if (_skinnedMeshRenderer.bones != null)
            {
                SetGUIColor(Color.white, Color.white);
                for (int i = 0; i < _transform.childCount; i++)
                {
                    Transform tf = _transform.GetChild(i);
                    if (tf)
                    {
                        PreviewBoneChild(tf, 0);
                    }
                }
            }
        }
    }
    /// <summary>
    /// 预览顶点
    /// </summary>
    private void PreviewVertexGUI()
    {
        if (_MAEditType == MAEditType.Vertex)
        {
            SetGUIColor(Color.white, Color.white);
            EditorGUILayout.BeginHorizontal("HelpBox");
            GUILayout.Label("Vertex Number  " + _mesh.vertexCount, "PreLabel");
            GUILayout.FlexibleSpace();
            _vertexHandleType = (HandleType)EditorGUILayout.EnumPopup(_vertexHandleType, GUILayout.Width(55));
            EditorGUILayout.EndHorizontal();
        }
    }
    /// <summary>
    /// 骨骼与蒙皮操作完成
    /// </summary>
    private void DoneGUI()
    {
        SetGUIColor(Color.white, Color.white);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Open In Editor Window"))
        {
            MorphAnimationWindow maw = EditorWindow.GetWindow<MorphAnimationWindow>();
            maw.titleContent = new GUIContent(EditorGUIUtility.IconContent("Animation Icon"));
            maw.titleContent.text = "Morph Editor";
            maw.autoRepaintOnSceneChange = true;
            maw.Init(_morphAnimation);
            maw.Show();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void OnSceneGUI()
    {
        if (Application.isPlaying)
            return;

        if (!_morphAnimation.IsApprove)
            return;

        if (_morphAnimation.IsDone)
            return;

        ChangeHandleTool();
        EditBone();
        EditVertex();
    }
    /// <summary>
    /// 编辑骨骼模式
    /// </summary>
    private void EditBone()
    {
        if (_MAEditType == MAEditType.Bone)
        {
            MorphHandles.DrawMorphBone(_transform, _boneSize);

            if (_currentCheckedBone != null)
            {
                SetHandlesColor(Color.white);
                switch (_MATool)
                {
                    case MATool.Move:
                        Vector3 oldVec = _currentCheckedBone.position;
                        Vector3 newVec = Handles.PositionHandle(oldVec, Quaternion.identity);
                        if (oldVec != newVec)
                        {
                            Undo.RegisterFullObjectHierarchyUndo(_transform.gameObject, "Morph Move");
                            _currentCheckedBone.position = newVec;
                        }
                        break;
                    case MATool.Rotate:
                        Quaternion oldRot = _currentCheckedBone.rotation;
                        Quaternion newRot = Handles.RotationHandle(oldRot, _currentCheckedBone.position);
                        if (oldRot != newRot)
                        {
                            Undo.RegisterFullObjectHierarchyUndo(_transform.gameObject, "Morph Rotate");
                            _currentCheckedBone.rotation = newRot;
                        }
                        break;
                    case MATool.Scale:
                        Vector3 oldSca = _currentCheckedBone.localScale;
                        Vector3 newSca = Handles.ScaleHandle(oldSca, _currentCheckedBone.position, Quaternion.identity, 0.5f);
                        if (oldSca != newSca)
                        {
                            Undo.RegisterFullObjectHierarchyUndo(_transform.gameObject, "Morph Scale");
                            _currentCheckedBone.localScale = newSca;
                        }
                        break;
                }

                SetHandlesColor(Color.cyan);
                _currentCheckedMorphBone.InternalRange = Handles.RadiusHandle(Quaternion.identity, _currentCheckedBone.position, _currentCheckedMorphBone.InternalRange);

                for (int i = 0; i < _currentCheckedMorphBone.InternalRangeVertexs.Count; i++)
                {
                    MorphHandles.DrawMorphVertex(_currentCheckedMorphBone.InternalRangeVertexs[i].Vertex, _vertexIconSize, _vertexHandleType);
                }

                SetHandlesColor(Color.yellow);
                _currentCheckedMorphBone.ExternalRange = Handles.RadiusHandle(Quaternion.identity, _currentCheckedBone.position, _currentCheckedMorphBone.ExternalRange);

                for (int i = 0; i < _currentCheckedMorphBone.ExternalRangeVertexs.Count; i++)
                {
                    MorphHandles.DrawMorphVertex(_currentCheckedMorphBone.ExternalRangeVertexs[i].Vertex, _vertexIconSize, _vertexHandleType);
                }

                SetHandlesColor(Color.red);
                for (int i = 0; i < _currentCheckedMorphBone.ErrorVertexs.Count; i++)
                {
                    MorphHandles.DrawMorphVertex(_currentCheckedMorphBone.ErrorVertexs[i].Vertex, _vertexIconSize, _vertexHandleType);
                }
            }
        }
    }
    /// <summary>
    /// 编辑顶点模式
    /// </summary>
    private void EditVertex()
    {
        if (_MAEditType == MAEditType.Vertex)
        {
            if (Event.current.button == 0 && Event.current.isMouse && Event.current.type == EventType.MouseDown)
            {
                RaycastHit hit;
                if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out hit))
                {
                    if (hit.triangleIndex >= 0 && hit.triangleIndex < _morphAnimation.Triangles.Count)
                    {
                        _currentCheckedVertex = GetVertexByClick(_morphAnimation.Triangles[hit.triangleIndex], hit.point);
                    }
                }
                Selection.activeObject = _transform.gameObject;
            }
            if (_currentCheckedVertex != null)
            {
                SetHandlesColor(Color.cyan);
                MorphHandles.DrawMorphVertex(_currentCheckedVertex.Vertex, _vertexIconSize, _vertexHandleType);
                EditBoneWeight();
            }
        }
    }
    /// <summary>
    /// 编辑顶点权重
    /// </summary>
    private void EditBoneWeight()
    {
        try
        {
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(100, 100, 200, 500));

            if (_skinnedMeshRenderer.bones.Length > 0)
            {
                if (GUILayout.Button("bone1：" + (_currentCheckedVertex.VertexBoneWeight.bone0 ? _currentCheckedVertex.VertexBoneWeight.bone0.name : "Null"), GUILayout.Width(150)))
                {
                    GenericMenu g = GetBonesMenu(0);
                    g.ShowAsContext();
                }
                float w0 = _currentCheckedVertex.VertexBoneWeight.weight0;
                _currentCheckedVertex.VertexBoneWeight.weight0 = EditorGUILayout.FloatField("weight：", _currentCheckedVertex.VertexBoneWeight.weight0);
                
                if (GUILayout.Button("bone2：" + (_currentCheckedVertex.VertexBoneWeight.bone1 ? _currentCheckedVertex.VertexBoneWeight.bone1.name : "Null"), GUILayout.Width(150)))
                {
                    GenericMenu g = GetBonesMenu(1);
                    g.ShowAsContext();
                }
                float w1 = _currentCheckedVertex.VertexBoneWeight.weight1;
                _currentCheckedVertex.VertexBoneWeight.weight1 = EditorGUILayout.FloatField("weight：", _currentCheckedVertex.VertexBoneWeight.weight1);

                if (GUILayout.Button("bone3：" + (_currentCheckedVertex.VertexBoneWeight.bone2 ? _currentCheckedVertex.VertexBoneWeight.bone2.name : "Null"), GUILayout.Width(150)))
                {
                    GenericMenu g = GetBonesMenu(2);
                    g.ShowAsContext();
                }
                float w2 = _currentCheckedVertex.VertexBoneWeight.weight2;
                _currentCheckedVertex.VertexBoneWeight.weight2 = EditorGUILayout.FloatField("weight：", _currentCheckedVertex.VertexBoneWeight.weight2);

                if (GUILayout.Button("bone4：" + (_currentCheckedVertex.VertexBoneWeight.bone3 ? _currentCheckedVertex.VertexBoneWeight.bone3.name : "Null"), GUILayout.Width(150)))
                {
                    GenericMenu g = GetBonesMenu(3);
                    g.ShowAsContext();
                }
                float w3 = _currentCheckedVertex.VertexBoneWeight.weight3;
                _currentCheckedVertex.VertexBoneWeight.weight3 = EditorGUILayout.FloatField("weight：", _currentCheckedVertex.VertexBoneWeight.weight3);

                if (w0 != _currentCheckedVertex.VertexBoneWeight.weight0)
                {
                    _currentCheckedVertex.VertexBoneWeight.AstrictWeightsExclude0();
                }
                else if (w1 != _currentCheckedVertex.VertexBoneWeight.weight1)
                {
                    _currentCheckedVertex.VertexBoneWeight.AstrictWeightsExclude1();
                }
                else if (w2 != _currentCheckedVertex.VertexBoneWeight.weight2)
                {
                    _currentCheckedVertex.VertexBoneWeight.AstrictWeightsExclude2();
                }
                else if (w3 != _currentCheckedVertex.VertexBoneWeight.weight3)
                {
                    _currentCheckedVertex.VertexBoneWeight.AstrictWeightsExclude3();
                }
            }
            else
            {
                GUILayout.Label("请添加至少一条骨骼！");
            }
            GUILayout.EndArea();
            Handles.EndGUI();
        }
        catch
        { }
    }

    #region Assist Method
    private void ReSet()
    {
        _skinnedMeshRenderer = _morphAnimation.transform.GetComponent<SkinnedMeshRenderer>();
        if (_skinnedMeshRenderer)
        {
            _skinnedMeshRenderer.hideFlags = HideFlags.HideInInspector;
            if (_skinnedMeshRenderer.bones != null)
            {
                foreach (Transform tf in _skinnedMeshRenderer.bones)
                {
                    tf.gameObject.hideFlags = HideFlags.HideInHierarchy;
                }
            }
        }
    }
    private void PreviewBoneChild(Transform bone, int retract)
    {
        EditorGUILayout.BeginHorizontal();
        bool checkd = _currentCheckedBone == bone;
        SetGUIColor(Color.white, checkd ? Color.yellow : Color.white);
        GUILayout.Space(retract);
        if (GUILayout.Button(bone.name, "Minibutton", GUILayout.Width(100)))
        {
            _currentCheckedBone = bone;
            _currentCheckedMorphBone = bone.GetComponent<MorphBone>();
        }
        if (retract == 0)
        {
            MorphBone mb = bone.GetComponent<MorphBone>();
            bool hide = (mb != null ? mb.IsHideInRootBone : false);
            if (GUILayout.Button("", hide ? "OL Plus" : "OL Minus", GUILayout.Width(20)))
            {
                mb = bone.GetComponent<MorphBone>();
                if (mb != null) mb.IsHideInRootBone = !hide;
            }
        }
        EditorGUILayout.EndHorizontal();

        if (checkd)
        {
            SetGUIColor(Color.white, Color.yellow);
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("External Range");
            _currentCheckedMorphBone.ExternalRange = EditorGUILayout.FloatField(_currentCheckedMorphBone.ExternalRange);
            GUILayout.Label("Internal Range");
            _currentCheckedMorphBone.InternalRange = EditorGUILayout.FloatField(_currentCheckedMorphBone.InternalRange);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Apply Range", "Minibutton"))
            {
                ApplyRange();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        MorphBone mb1 = bone.GetComponent<MorphBone>();
        if (mb1 != null && !mb1.IsHideInRootBone)
        {
            for (int i = 0; i < bone.childCount; i++)
            {
                Transform tf = bone.GetChild(i);
                if (tf)
                {
                    PreviewBoneChild(tf, retract + 50);
                }
            }
        }
    }
    private void SetGUIEnabled(bool enabled)
    {
        GUI.enabled = enabled;
    }
    private void SetGUIColor(Color fgColor, Color bgColor)
    {
        GUI.color = fgColor;
        GUI.backgroundColor = bgColor;
    }
    private void SetHandlesColor(Color color)
    {
        Handles.color = color;
    }
    private void ChangeHandleTool()
    {
        if (Tools.current == Tool.Move)
        {
            _MATool = MATool.Move;
        }
        else if (Tools.current == Tool.Rotate)
        {
            _MATool = MATool.Rotate;
        }
        else if (Tools.current == Tool.Scale)
        {
            _MATool = MATool.Scale;
        }
        if (Tools.current != Tool.None)
        {
            Tools.current = Tool.None;
        }
    }
    private MorphVertex GetVertexByClick(MorphTriangle triangle, Vector3 clickPoint)
    {
        float distance1 = Vector3.Distance(triangle.Vertex1.Vertex, clickPoint);
        float distance2 = Vector3.Distance(triangle.Vertex2.Vertex, clickPoint);
        float distance3 = Vector3.Distance(triangle.Vertex3.Vertex, clickPoint);

        if (distance1 < distance2 && distance1 < distance3)
            return triangle.Vertex1;
        if (distance2 < distance1 && distance2 < distance3)
            return triangle.Vertex2;
        if (distance3 < distance1 && distance3 < distance2)
            return triangle.Vertex3;
        return triangle.Vertex1;
    }
    private GenericMenu GetBonesMenu(int index)
    {
        GenericMenu gm = new GenericMenu();
        for (int i = 0; i < _skinnedMeshRenderer.bones.Length; i++)
        {
            int s = i;
            gm.AddItem(new GUIContent(_skinnedMeshRenderer.bones[s].name), false, delegate ()
            {
                switch (index)
                {
                    case 0:
                        _currentCheckedVertex.VertexBoneWeight.bone0 = _skinnedMeshRenderer.bones[s];
                        break;
                    case 1:
                        _currentCheckedVertex.VertexBoneWeight.bone1 = _skinnedMeshRenderer.bones[s];
                        break;
                    case 2:
                        _currentCheckedVertex.VertexBoneWeight.bone2 = _skinnedMeshRenderer.bones[s];
                        break;
                    case 3:
                        _currentCheckedVertex.VertexBoneWeight.bone3 = _skinnedMeshRenderer.bones[s];
                        break;
                }
            });
        }
        return gm;
    }
    private void ApplyRange()
    {
        _currentCheckedMorphBone.InternalRangeVertexs.Clear();
        _currentCheckedMorphBone.ExternalRangeVertexs.Clear();
        _currentCheckedMorphBone.ErrorVertexs.Clear();

        for (int i = 0; i < _morphAnimation.Vertexs.Count; i++)
        {
            float distance = Vector3.Distance(_morphAnimation.Vertexs[i].Vertex, _currentCheckedBone.position);
            if (distance < _currentCheckedMorphBone.InternalRange)
            {
                MorphBoneWeight mbw = _morphAnimation.Vertexs[i].VertexBoneWeight;
                mbw.bone0 = _currentCheckedBone;
                mbw.bone1 = _currentCheckedBone;
                mbw.bone2 = _currentCheckedBone;
                mbw.bone3 = _currentCheckedBone;
                mbw.weight0 = 1;
                mbw.weight1 = 0;
                mbw.weight2 = 0;
                mbw.weight3 = 0;
                _currentCheckedMorphBone.InternalRangeVertexs.Add(_morphAnimation.Vertexs[i]);
            }
            else if (distance >= _currentCheckedMorphBone.InternalRange && distance < _currentCheckedMorphBone.ExternalRange)
            {
                MorphBoneWeight mbw = _morphAnimation.Vertexs[i].VertexBoneWeight;
                if (mbw.bone0 == _currentCheckedBone)
                {
                    mbw.bone0 = _currentCheckedBone;
                    mbw.weight0 = 1 - distance / _currentCheckedMorphBone.ExternalRange;
                    mbw.AstrictWeights();
                    _currentCheckedMorphBone.ExternalRangeVertexs.Add(_morphAnimation.Vertexs[i]);
                }
                else if (mbw.bone1 == _currentCheckedBone)
                {
                    mbw.bone1 = _currentCheckedBone;
                    mbw.weight1 = 1 - distance / _currentCheckedMorphBone.ExternalRange;
                    mbw.AstrictWeights();
                    _currentCheckedMorphBone.ExternalRangeVertexs.Add(_morphAnimation.Vertexs[i]);
                }
                else if (mbw.bone2 == _currentCheckedBone)
                {
                    mbw.bone2 = _currentCheckedBone;
                    mbw.weight2 = 1 - distance / _currentCheckedMorphBone.ExternalRange;
                    mbw.AstrictWeights();
                    _currentCheckedMorphBone.ExternalRangeVertexs.Add(_morphAnimation.Vertexs[i]);
                }
                else if (mbw.bone3 == _currentCheckedBone)
                {
                    mbw.bone3 = _currentCheckedBone;
                    mbw.weight3 = 1 - distance / _currentCheckedMorphBone.ExternalRange;
                    mbw.AstrictWeights();
                    _currentCheckedMorphBone.ExternalRangeVertexs.Add(_morphAnimation.Vertexs[i]);
                }
                else if (!mbw.bone0)
                {
                    mbw.bone0 = _currentCheckedBone;
                    mbw.weight0 = 1 - distance / _currentCheckedMorphBone.ExternalRange;
                    mbw.AstrictWeights();
                    _currentCheckedMorphBone.ExternalRangeVertexs.Add(_morphAnimation.Vertexs[i]);
                }
                else if (!mbw.bone1)
                {
                    mbw.bone1 = _currentCheckedBone;
                    mbw.weight1 = 1 - distance / _currentCheckedMorphBone.ExternalRange;
                    mbw.AstrictWeights();
                    _currentCheckedMorphBone.ExternalRangeVertexs.Add(_morphAnimation.Vertexs[i]);
                }
                else if (!mbw.bone2)
                {
                    mbw.bone2 = _currentCheckedBone;
                    mbw.weight2 = 1 - distance / _currentCheckedMorphBone.ExternalRange;
                    mbw.AstrictWeights();
                    _currentCheckedMorphBone.ExternalRangeVertexs.Add(_morphAnimation.Vertexs[i]);
                }
                else if (!mbw.bone3)
                {
                    mbw.bone3 = _currentCheckedBone;
                    mbw.weight3 = 1 - distance / _currentCheckedMorphBone.ExternalRange;
                    mbw.AstrictWeights();
                    _currentCheckedMorphBone.ExternalRangeVertexs.Add(_morphAnimation.Vertexs[i]);
                }
                else
                {
                    _currentCheckedMorphBone.ErrorVertexs.Add(_morphAnimation.Vertexs[i]);
                }
            }
        }
    }
    private void AddBone()
    {
        int number = 0;
        if (_skinnedMeshRenderer.bones != null && _skinnedMeshRenderer.bones.Length > 0)
        {
            number = _skinnedMeshRenderer.bones.Length;
        }

        GameObject go = new GameObject("NewBone" + number);
        go.AddComponent<MorphBone>();
        go.GetComponent<MorphBone>().hideFlags = HideFlags.HideInInspector;
        go.hideFlags = HideFlags.HideInHierarchy;
        Transform bone = go.transform;
        bone.SetParent(_transform);
        bone.localPosition = Vector3.zero;
        bone.localRotation = Quaternion.identity;

        if (_skinnedMeshRenderer.bones != null)
        {
            List<Transform> bonesList = _skinnedMeshRenderer.bones.ToList();
            bonesList.Add(bone);
            _skinnedMeshRenderer.bones = bonesList.ToArray();
        }
        else
        {
            Transform[] bones = new Transform[1];
            bones[0] = bone;
            _skinnedMeshRenderer.bones = bones;
        }
    }
    private void AddSubBone()
    {
        GameObject go = new GameObject("NewBone" + _skinnedMeshRenderer.bones.Length);
        go.AddComponent<MorphBone>();
        go.GetComponent<MorphBone>().hideFlags = HideFlags.HideInInspector;
        go.hideFlags = HideFlags.HideInHierarchy;
        Transform bone = go.transform;
        bone.SetParent(_currentCheckedBone);
        bone.localPosition = Vector3.zero;
        bone.localRotation = Quaternion.identity;

        List<Transform> bonesList = _skinnedMeshRenderer.bones.ToList();
        bonesList.Add(bone);
        _skinnedMeshRenderer.bones = bonesList.ToArray();
    }
    private void DeleteBone()
    {
        if (_currentCheckedBone)
        {
            if (_currentCheckedBone.transform.childCount > 0)
            {
                Debug.LogWarning("请先删除该骨骼的子骨骼！");
                return;
            }

            List<Transform> bonesList = _skinnedMeshRenderer.bones.ToList();
            bonesList.Remove(_currentCheckedBone);
            DestroyImmediate(_currentCheckedBone.gameObject);
            _currentCheckedBone = null;
            _skinnedMeshRenderer.bones = bonesList.ToArray();

            SaveMorphMesh();
        }
    }
    private void SaveMorphMesh()
    {
        BoneWeight[] bws = new BoneWeight[_mesh.vertexCount];
        List<Transform> bs = _skinnedMeshRenderer.bones.ToList();
        for (int i = 0; i < _morphAnimation.Vertexs.Count; i++)
        {
            BoneWeight bw = _morphAnimation.Vertexs[i].VertexBoneWeight.ToBoneWeight(bs);
            for (int j = 0; j < _morphAnimation.Vertexs[i].VertexIndexs.Count; j++)
            {
                int index = _morphAnimation.Vertexs[i].VertexIndexs[j];
                bws[index] = bw;
            }
        }

        Matrix4x4[] bindPoses = new Matrix4x4[_skinnedMeshRenderer.bones.Length];
        for (int i = 0; i < bindPoses.Length; i++)
        {
            bindPoses[i] = _skinnedMeshRenderer.bones[i].worldToLocalMatrix * _transform.localToWorldMatrix;
        }
        
        _mesh.boneWeights = bws;
        _mesh.bindposes = bindPoses;
        _mesh.RecalculateNormals();
        _skinnedMeshRenderer.sharedMesh = _mesh;
        _meshCollider.sharedMesh = _mesh;
    }
    private void Done()
    {
        SaveMorphMesh();

        _morphAnimation.IsDone = true;
        _morphAnimation.Bones = _skinnedMeshRenderer.bones.ToList();
        _morphAnimation.AnimationFrames = new List<MorphAnimationFrame>();
        _morphAnimation.TimeLine = new List<MorphAnimationFrame>();

        _morphAnimation.Triangles = null;
        _morphAnimation.Vertexs = null;
        for (int i = 0; i < _morphAnimation.Bones.Count; i++)
        {
            DestroyImmediate(_morphAnimation.Bones[i].GetComponent<MorphBone>());
        }

        MorphAnimationWindow maw = EditorWindow.GetWindow<MorphAnimationWindow>();
        maw.titleContent = new GUIContent(EditorGUIUtility.IconContent("Animation Icon"));
        maw.titleContent.text = "Morph Editor";
        maw.autoRepaintOnSceneChange = true;
        maw.Init(_morphAnimation);
        maw.Show();
    }
    #endregion
}
#region Enum
public enum MAEditType
{
    Vertex = 0,
    Bone = 1
}
public enum MATool
{
    Move = 1,
    Rotate = 2,
    Scale = 3,
}
public enum HandleType
{
    Wire = 0,
    Solid = 1
}
#endregion