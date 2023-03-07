using UnityEngine;

public abstract class BaseUiAnimtaion : MonoBehaviour
{
    public float PlayTime => _playTime;
    private float _playTime;
    private float _startPlayTime;

    public virtual void Play(float playTime) {
        _startPlayTime = Time.time;
        _playTime = playTime;
    }

    public virtual void Stop() {

    }

    private void Update() {
        
    }
}
