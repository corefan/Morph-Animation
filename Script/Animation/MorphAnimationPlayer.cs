using UnityEngine;

public class MorphAnimationPlayer : MonoBehaviour 
{
    public MorphAnimationClip Clip;
    public float Speed = 1;
    public bool Loop = true;
    public bool PlayOnAwake = true;
    public bool IsPlaying
    {
        get
        {
            return _isPlaying;
        }
    }

    private Transform[] _bones;
    private bool _isPlaying;
    private int _playIndex;
    private float _playLocation;

    private void Awake()
	{
        SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
        if (smr)
            _bones = smr.bones;

        _playIndex = 0;
        _playLocation = 0f;

        if (PlayOnAwake)
        {
            Play();
        }
	}
	
	private void Update () 
	{
        if (_isPlaying)
        {
            MorphAnimationFrame currentMaf = Clip.AnimationFrames[_playIndex];
            MorphAnimationFrame lastMaf;
            if (_playIndex + 1 >= Clip.AnimationFrames.Count)
                lastMaf = Clip.AnimationFrames[0];
            else
                lastMaf = Clip.AnimationFrames[_playIndex + 1];

            if (_playLocation <= currentMaf.Time)
            {
                _playLocation += Speed * Time.deltaTime;
            }
            else
            {
                _playIndex += 1;
                _playLocation = 0f;
                     
                if (_playIndex >= Clip.AnimationFrames.Count)
                {
                    _playIndex = 0;
                }
                if (!Loop && _playIndex == 0)
                {
                    Stop();
                }
                return;
            }

            float location = _playLocation / currentMaf.Time;
            for (int i = 0; i < _bones.Length; i++)
            {
                if (Clip.IsPosition)
                    _bones[i].position = Vector3.Lerp(currentMaf.Positions[i], lastMaf.Positions[i], location);
                if (Clip.IsRotation)
                    _bones[i].rotation = Quaternion.Lerp(currentMaf.Rotations[i], lastMaf.Rotations[i], location);
                if (Clip.IsScale)
                    _bones[i].localScale = Vector3.Lerp(currentMaf.Scales[i], lastMaf.Scales[i], location);
            }
        }
    }

    /// <summary>
    /// 设置剪辑显示为指定的动画帧
    /// </summary>
    public void SetFrame(int index)
    {
        if (index >= 0 && index < Clip.AnimationFrames.Count)
        {
            for (int i = 0; i < _bones.Length; i++)
            {
                if (Clip.IsPosition)
                    _bones[i].position = Clip.AnimationFrames[index].Positions[i];
                if (Clip.IsRotation)
                    _bones[i].rotation = Clip.AnimationFrames[index].Rotations[i];
                if (Clip.IsScale)
                    _bones[i].localScale = Clip.AnimationFrames[index].Scales[i];
            }
            _playIndex = index;
            _playLocation = 0f;
        }
    }

    /// <summary>
    /// 播放动画
    /// </summary>
    public void Play()
    {
        if (Clip != null && _bones != null)
        {
            _isPlaying = true;
        }
        else
        {
            Debug.Log("动画剪辑或模型骨骼为空，无法播放动画！");
        }
    }

    /// <summary>
    /// 重新播放动画
    /// </summary>
    public void RePlay()
    {
        if (Clip != null && _bones != null)
        {
            _isPlaying = true;
            SetFrame(0);
        }
        else
        {
            Debug.Log("动画剪辑或模型骨骼为空，无法播放动画！");
        }
    }

    /// <summary>
    /// 暂停动画
    /// </summary>
    public void Pause()
    {
        _isPlaying = false;
    }

    /// <summary>
    /// 停止动画
    /// </summary>
    public void Stop()
    {
        _isPlaying = false;

        if (Clip != null && _bones != null)
        {
            SetFrame(0);
        }
    }
}
