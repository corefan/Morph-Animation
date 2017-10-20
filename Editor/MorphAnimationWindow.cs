using UnityEngine;
using UnityEditor;

public class MorphAnimationWindow : EditorWindow 
{
    private MorphAnimation _morphAnimation;
    private MorphAnimationFrame _currentFrame;
    private Transform _currentBone;
    private Transform _currentBoneParent;
    private float _boneDistance;
    private bool _frameEdit;
    private bool _isPreview;
    private int _previewIndex;
    private float _previewLocation;

    private Vector2 _buttonSize;
    private Vector2 _labelSize;
    private bool _isRename;
    private string _newName;
    private Vector2 _scorll1;
    private Vector2 _scorll2;
    private GUIContent _previewButton;
    private GUIContent _addFrameButton;

    public void Init(MorphAnimation ma)
    {
        _morphAnimation = ma;
        _currentFrame = null;
        _currentBone = null;
        _currentBoneParent = null;
        _boneDistance = 0f;
        _frameEdit = false;
        _isPreview = false;
        _previewIndex = 0;
        _previewLocation = 0f;

        _buttonSize = new Vector2(100, 30);
        _labelSize = new Vector2(100, 20);
        _isRename = false;
        _newName = "";
        _previewButton = new GUIContent("", "Preview Animation");
        _addFrameButton = new GUIContent("", "Add Frame In TimeLine");

        EditorApplication.update += Update;
    }

    private void Update()
    {
        AstrictChangeBone();
        if (_isPreview)
        {
            PreviewAnimation();
        }
    }

    private void OnGUI()
    {
        if (!_morphAnimation)
        {
            return;
        }

        TitleGUI();
        TimeLineGUI();
        PropertyGUI();
        KeyFrameGUI();
    }
    private void TitleGUI()
    {
        SetGUIColor(Color.white, Color.white);
        EditorGUILayout.BeginHorizontal("Toolbar");
        if (GUILayout.Button(_morphAnimation.name, "Toolbarbutton"))
        {
            Selection.activeGameObject = _morphAnimation.gameObject;
            _currentFrame = null;
            _currentBone = null;
        }
        SetGUIColor(_isPreview ? Color.red : Color.white, Color.white);
        if (GUILayout.Button(_previewButton, "ProfilerTimelineFoldout"))
        {
            if (_morphAnimation.TimeLine.Count < 2)
            {
                Debug.LogWarning("请确保时间线上存在至少两个关键帧！");
                return;
            }

            _isPreview = !_isPreview;

            if (_isPreview)
            {
                _previewIndex = 0;
                _previewLocation = 0f;
            }
            else
            {
                SetFrameData(_morphAnimation.TimeLine[0]);
            }
        }
        SetGUIColor(Color.white, Color.white);
        if (GUILayout.Button("Save Clip", "Toolbarbutton"))
        {
            string path = EditorUtility.SaveFilePanel("Save Clip", Application.dataPath, "New Clip", "asset");
            if (path.Length != 0)
            {
                string subPath = path.Substring(0, path.IndexOf("Asset"));
                path = path.Replace(subPath, "");

                MorphAnimationClip mac = new MorphAnimationClip();
                mac.AnimationFrames = _morphAnimation.TimeLine;
                mac.IsPosition = _morphAnimation.IsPosition;
                mac.IsRotation = _morphAnimation.IsRotation;
                mac.IsScale = _morphAnimation.IsScale;

                AssetDatabase.CreateAsset(mac, path);
                AssetDatabase.SaveAssets();
            }
        }
        GUILayout.FlexibleSpace();
        SetGUIColor(Color.white, Color.white);
        SetGUIEnabled(!_isPreview);
        if (GUILayout.Button("Add Key Frame", "Toolbarbutton"))
        {
            MorphAnimationFrame maf = new MorphAnimationFrame("New Key Frame", new Vector2(position.width / 2, position.height / 2));
            GetFrameData(maf);
            _morphAnimation.AnimationFrames.Add(maf);
        }

        #region Edit Key Frame
        if (_currentFrame != null)
        {
            SetGUIColor(Color.cyan, Color.white);
            if (_frameEdit)
            {
                if (GUILayout.Button("Rename", "Toolbarbutton"))
                {
                    _isRename = !_isRename;

                    if (_isRename)
                    {
                        _newName = _currentFrame.name;
                    }
                }
                if (GUILayout.Button("Delete", "Toolbarbutton"))
                {
                    _morphAnimation.AnimationFrames.Remove(_currentFrame);
                    _currentFrame = null;
                }
            }
            else
            {
                if (GUILayout.Button("Leave", "Toolbarbutton"))
                {
                    _morphAnimation.TimeLine.Remove(_currentFrame);
                    _morphAnimation.AnimationFrames.Add(_currentFrame);
                    _currentFrame = null;
                }
                if (GUILayout.Button("Insert", "Toolbarbutton"))
                {
                    GenericMenu gm = new GenericMenu();
                    for (int i = 0; i < _morphAnimation.AnimationFrames.Count; i++)
                    {
                        int s = i;
                        gm.AddItem(new GUIContent(_morphAnimation.AnimationFrames[s].name), false, delegate ()
                        {
                            int index = _morphAnimation.TimeLine.IndexOf(_currentFrame);
                            _morphAnimation.TimeLine.Insert(index, _morphAnimation.AnimationFrames[s]);
                            _morphAnimation.AnimationFrames.RemoveAt(s);
                            _currentFrame = null;
                        });
                    }
                    gm.ShowAsContext();
                }
                if (GUILayout.Button("Replace", "Toolbarbutton"))
                {
                    GenericMenu gm = new GenericMenu();
                    for (int i = 0; i < _morphAnimation.AnimationFrames.Count; i++)
                    {
                        int s = i;
                        gm.AddItem(new GUIContent(_morphAnimation.AnimationFrames[s].name), false, delegate ()
                        {
                            int index = _morphAnimation.TimeLine.IndexOf(_currentFrame);
                            _morphAnimation.TimeLine.Insert(index, _morphAnimation.AnimationFrames[s]);
                            _morphAnimation.AnimationFrames.RemoveAt(s);
                            _morphAnimation.TimeLine.Remove(_currentFrame);
                            _morphAnimation.AnimationFrames.Add(_currentFrame);
                            _currentFrame = null;
                        });
                    }
                    gm.ShowAsContext();
                }
            }
        }
        #endregion

        SetGUIColor(Color.white, Color.white);
        GUILayout.Label("position");
        _morphAnimation.IsPosition = EditorGUILayout.Toggle(_morphAnimation.IsPosition, GUILayout.Width(15));
        GUILayout.Label("rotation");
        _morphAnimation.IsRotation = EditorGUILayout.Toggle(_morphAnimation.IsRotation, GUILayout.Width(15));
        GUILayout.Label("scale");
        _morphAnimation.IsScale = EditorGUILayout.Toggle(_morphAnimation.IsScale, GUILayout.Width(15));
        EditorGUILayout.EndHorizontal();
    }
    private void TimeLineGUI()
    {
        _scorll1 = EditorGUILayout.BeginScrollView(_scorll1);
        EditorGUILayout.BeginHorizontal("HelpBox");
        GUILayout.Label("Time Line:", "PreLabel", GUILayout.Width(100));
        for (int i = 0; i < _morphAnimation.TimeLine.Count; i++)
        {
            SetGUIColor(Color.white, _currentFrame == _morphAnimation.TimeLine[i] ? Color.cyan : Color.white);
            string content = (i == 0 ? "GUIEditor.BreadcrumbLeft" : "GUIEditor.BreadcrumbMid");
            if (GUILayout.Button(_morphAnimation.TimeLine[i].name, content, GUILayout.Width(50)))
            {
                _currentFrame = _morphAnimation.TimeLine[i];
                SetFrameData(_currentFrame);
                _frameEdit = false;
            }
        }
        GUILayout.Space(10);
        SetGUIColor(Color.white, Color.white);
        if (GUILayout.Button(_addFrameButton, "OL Plus", GUILayout.Width(20)))
        {
            GenericMenu gm = new GenericMenu();
            for (int i = 0; i < _morphAnimation.AnimationFrames.Count; i++)
            {
                int s = i;
                gm.AddItem(new GUIContent(_morphAnimation.AnimationFrames[s].name), false, delegate ()
                {
                    _morphAnimation.TimeLine.Add(_morphAnimation.AnimationFrames[s]);
                    _morphAnimation.AnimationFrames.RemoveAt(s);
                    _currentFrame = null;
                });
            }
            gm.ShowAsContext();
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
    }
    private void PropertyGUI()
    {
        if (_currentFrame != null)
        {
            float h = 30 + _morphAnimation.Bones.Count * 20;
            if (h < 110) h = 110;

            GUI.BeginGroup(new Rect(0, 60, 210, h), new GUIStyle("HelpBox"));
            GUI.Label(new Rect(0, 0, 100, 20), "Bone List:", "PreLabel");
            for (int i = 0; i < _morphAnimation.Bones.Count; i++)
            {
                Rect rect = new Rect(0, 20 + i * 20, 100, 20);
                SetGUIColor(Color.white, _currentBone == _morphAnimation.Bones[i] ? Color.red : Color.white);
                if (GUI.Button(rect, _morphAnimation.Bones[i].name))
                {
                    _currentBone = _morphAnimation.Bones[i];
                    _currentBoneParent = null;
                    Selection.activeGameObject = _currentBone.gameObject;
                    Tools.current = Tool.Move;

                    if (_currentBone.transform.parent != null && _currentBone.transform.parent != _morphAnimation.transform)
                    {
                        _currentBoneParent = _currentBone.transform.parent;
                        _boneDistance = Vector3.Distance(_currentBone.position, _currentBoneParent.position);
                    }
                }
            }
            SetGUIColor(Color.white, Color.white);
            GUI.Label(new Rect(100, 0, 100, 20), "Frame Name:", "PreLabel");
            GUI.Label(new Rect(100, 20, 100, 20), _currentFrame.name);
            GUI.Label(new Rect(100, 40, 100, 20), "Frame Time:", "PreLabel");
            string oldv = _currentFrame.Time.ToString();
            string newv = GUI.TextField(new Rect(100, 60, 100, 20), oldv);
            if (oldv != newv)
            {
                _currentFrame.Time = float.Parse(newv);
            }
            if (GUI.Button(new Rect(100, 80, 100, 20), "Apply Bone"))
            {
                GetFrameData(_currentFrame);
            }
            GUI.EndGroup();
        }
    }
    private void KeyFrameGUI()
    {
        for (int i = 0; i < _morphAnimation.AnimationFrames.Count; i++)
        {
            SetGUIColor(Color.white, _currentFrame == _morphAnimation.AnimationFrames[i] ? Color.cyan : Color.white);
            Rect rect = new Rect(_morphAnimation.AnimationFrames[i].EditorPosition.x - _buttonSize.x / 2
                , _morphAnimation.AnimationFrames[i].EditorPosition.y - _buttonSize.y / 2, _buttonSize.x, _buttonSize.y);
            if (GUI.RepeatButton(rect, _morphAnimation.AnimationFrames[i].name))
            {
                _currentFrame = _morphAnimation.AnimationFrames[i];
                SetFrameData(_currentFrame);
                _frameEdit = true;
                _morphAnimation.AnimationFrames[i].EditorPosition = Event.current.mousePosition;
                Repaint();
            }

            if (_isRename && _currentFrame == _morphAnimation.AnimationFrames[i])
            {
                SetGUIColor(Color.white, Color.red);
                rect = new Rect(_currentFrame.EditorPosition.x + _buttonSize.x / 2 + 5
                , _currentFrame.EditorPosition.y - 10, _labelSize.x, _labelSize.y);
                _newName = GUI.TextField(rect, _newName);

                rect = new Rect(_currentFrame.EditorPosition.x + _buttonSize.x / 2 + _labelSize.x + 5
                , _currentFrame.EditorPosition.y - 10, _labelSize.x / 2, _labelSize.y);
                if (GUI.Button(rect, "Sure"))
                {
                    _isRename = false;
                    _currentFrame.name = _newName;
                }
            }
        }
    }

    private void OnDestroy()
    {
        EditorApplication.update -= Update;
    }

    #region Assist Method
    private void SetGUIEnabled(bool enabled)
    {
        GUI.enabled = enabled;
    }
    private void SetGUIColor(Color fgColor, Color bgColor)
    {
        GUI.color = fgColor;
        GUI.backgroundColor = bgColor;
    }
    private void SetFrameData(MorphAnimationFrame maf)
    {
        for (int i = 0; i < _morphAnimation.Bones.Count; i++)
        {
            _morphAnimation.Bones[i].position = maf.Positions[i];
            _morphAnimation.Bones[i].rotation = maf.Rotations[i];
            _morphAnimation.Bones[i].localScale = maf.Scales[i];
        }
    }
    private void GetFrameData(MorphAnimationFrame maf)
    {
        maf.Positions.Clear();
        maf.Rotations.Clear();
        maf.Scales.Clear();

        for (int i = 0; i < _morphAnimation.Bones.Count; i++)
        {
            maf.Positions.Add(_morphAnimation.Bones[i].position);
            maf.Rotations.Add(_morphAnimation.Bones[i].rotation);
            maf.Scales.Add(_morphAnimation.Bones[i].localScale);
        }
    }
    private void PreviewAnimation()
    {
        if (_isPreview)
        {
            MorphAnimationFrame maf = _morphAnimation.TimeLine[_previewIndex];
            MorphAnimationFrame lastmaf;
            if (_previewIndex + 1 >= _morphAnimation.TimeLine.Count)
                lastmaf = _morphAnimation.TimeLine[0];
            else
                lastmaf = _morphAnimation.TimeLine[_previewIndex + 1];

            if (_previewLocation <= maf.Time)
            {
                _previewLocation += Time.deltaTime;
            }
            else
            {
                _previewIndex += 1;
                _previewLocation = 0f;

                if (_previewIndex >= _morphAnimation.TimeLine.Count)
                {
                    _previewIndex = 0;
                }
                return;
            }

            float location = _previewLocation / maf.Time;
            for (int i = 0; i < _morphAnimation.Bones.Count; i++)
            {
                if (_morphAnimation.IsPosition)
                    _morphAnimation.Bones[i].position = Vector3.Lerp(maf.Positions[i], lastmaf.Positions[i], location);
                if (_morphAnimation.IsRotation)
                    _morphAnimation.Bones[i].rotation = Quaternion.Lerp(maf.Rotations[i], lastmaf.Rotations[i], location);
                if (_morphAnimation.IsScale)
                    _morphAnimation.Bones[i].localScale = Vector3.Lerp(maf.Scales[i], lastmaf.Scales[i], location);
            }
        }
    }
    private void AstrictChangeBone()
    {
        if (_currentBone != null && _currentBoneParent != null)
        {
            float distance = Vector3.Distance(_currentBone.position, _currentBoneParent.position);
            if (!Mathf.Approximately(_boneDistance, distance))
            {
                Vector3 direction = (_currentBone.position - _currentBoneParent.position).normalized;
                _currentBone.position = _currentBoneParent.position + direction * _boneDistance;
            }
        }
    }
    #endregion
}
