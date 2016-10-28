using UnityEngine;
using System.Collections;
using System;

public class VertexAnimator : MonoBehaviour
{
    public int? CurrentAnimationClip
    {
        get
        {
            return _targetClip;
        }
    }

    public MaterialPropertyBlock MaterialPropertyBlock { get; set; }

    [SerializeField]
    bool AutoPlay;
    [SerializeField]
    int fps = 30;
    [SerializeField]
    Vector4[] Clips;

    Renderer _renderer;
    float _cur_t = 1;
    float _next_t = 1;
    float _t = 1;
    float _transition_t = 1;
    float _speed = 1;
    int? _targetClip;
    float _tt;
    float _transition_from_t;
    float _transition_to_t;
    bool _isPlay;
    bool _stopRepeat;
    bool _repeating;

    Color _color;

    public event EventHandler AnimationEnded;
    private void OnAnimationEnded()
    {
        if (AnimationEnded == null)
            return;

        AnimationEnded(this, null);
    }

    // Use this for initialization
    void Start()
    {
        _renderer = this.GetComponent<Renderer>();

        _isPlay = AutoPlay;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isPlay == true)
        {

            //var animationEnd = block.GetVector("_AnimTex_AnimEnd");
            var animationEnd = new Vector3(70, 0, 0);// _renderer.sharedMaterial.GetVector("_AnimTex_AnimEnd");

            //Debug.Log("animationEnd.x :" + animationEnd.x);
            if (_t >= animationEnd.x)
            {
                OnAnimationEnded();
                _isPlay = false;
            }

            if (_transition_t < 1)
            {
                _tt += Time.deltaTime * 1;
                _transition_t = Mathf.Clamp01(_tt / 0.4f);

                _t = Mathf.Lerp(_transition_from_t, _transition_to_t, _transition_t);
            }
            else
            {
                _transition_t = 1.1f;

                MaterialPropertyBlock.SetFloat("_AnimTex_T", _t);
                _cur_t = _t;
                _t += Time.deltaTime * _speed;
                _next_t = _t;
            }

            MaterialPropertyBlock.SetFloat("_AnimTex_Transition_T", _transition_t);
            //_renderer.material.SetFloat("_AnimTex_Transition_T", _transition_t);

            for (int i = 0; i < Clips.Length; i++)
            {
                var from = Clips[i].x / fps;
                var to = Clips[i].y / fps;
                var s = Clips[i].z;
                var isRepeat = Clips[i].w;

                if (_cur_t >= from && _cur_t <= to)
                {
                    _targetClip = i;
                    _speed = s + s * UnityEngine.Random.Range(-0.5f, 0.5f);

                    _repeating = isRepeat >= 1 ? true : false;

                    if (isRepeat == 1)
                    {
                        if (_stopRepeat == true && _transition_t >= 1)
                        {
                            _transition_from_t = _cur_t;
                            _transition_to_t = Clips[i + 1].x / fps;

                            MaterialPropertyBlock.SetFloat("_AnimTex_Transition_From_T", _transition_from_t);
                            MaterialPropertyBlock.SetFloat("_AnimTex_Transition_To_T", _transition_to_t);

                            //_renderer.material.SetFloat("_AnimTex_Transition_From_T", _transition_from_t);
                            //_renderer.material.SetFloat("_AnimTex_Transition_To_T", _transition_to_t);
                            _tt = 0;
                            _transition_t = 0;
                            _stopRepeat = false;
                        }

                        if (_next_t > to && _transition_t >= 1)
                            _t = from - (Time.deltaTime * _speed);
                    }
                }
            }
        }

        //block.SetColor("_Color", _color);

        _renderer.SetPropertyBlock(MaterialPropertyBlock);
    }

    public void SetColor(Color color)
    {
        _color = color;
    }

    public void StopRepeat()
    {
        if(_repeating == true)
            _stopRepeat = true;
    }

    public void Play()
    {
        if (_isPlay == true)
            return;

        _t = 0;
        _isPlay = true;
    }
}
