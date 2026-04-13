using UnityEngine;

public class ScreenBloodController : MonoBehaviour
{
    [Header("심장 박동 세팅 (빈사 상태)")]
    public float heartbeatInterval = 2.0f;

    private ParticleSystem _particleSystem;
    private bool _isDanger = false;
    private float _heartbeatTimer = 0f;

    private float _hitTimer = 0f;

    private void Awake()
    {
        _particleSystem = GetComponentInChildren<ParticleSystem>(true);
    }

    private void Update()
    {
        if (_isDanger)
        {
            _heartbeatTimer -= Time.deltaTime;
            if (_heartbeatTimer <= 0f)
            {
                PlayParticle();
                _heartbeatTimer = heartbeatInterval;
            }
        }
        else
        {
            if (_hitTimer > 0)
            {
                _hitTimer -= Time.deltaTime;

                if (_hitTimer <= 0f) gameObject.SetActive(false);
            }
        }
    }

    public void PlayHitEffect(float duration = 1.0f)
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        PlayParticle();

        if (_isDanger) _heartbeatTimer = heartbeatInterval;
        else _hitTimer = duration;
    }

    public void SetDangerMode(bool isDanger)
    {
        if (_isDanger == isDanger) return;

        _isDanger = isDanger;

        if (_isDanger)
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            PlayParticle();
            _heartbeatTimer = heartbeatInterval;
        }
        else
        {
            if (_particleSystem != null) 
                _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            _hitTimer = 1.0f;
        }
    }

    private void PlayParticle()
    {
        if (_particleSystem == null)
            _particleSystem = GetComponentInChildren<ParticleSystem>(true);

        if (_particleSystem != null)
        {
            _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _particleSystem.Play(true);
        }
    }
}