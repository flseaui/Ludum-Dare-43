using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CrewMovement : MonoBehaviour
{

    public float MovementTime = 5;
    private float _lerpValue = 0;
    private bool _aiming;

    private Vector3 _fromPos, _toPos;

    private Action _onFinish;
    
    public void GoToPosition(Vector3 fromPos, Vector3 toPos, Action whenFinished = null)
    {
        _aiming = true;
        _fromPos = fromPos;
        _toPos = toPos;
        _onFinish = whenFinished;
    }
    
    private void Update()
    {
        if (_aiming)
        {
            if (_lerpValue < 1)
            {
                _lerpValue += Time.deltaTime / MovementTime;
                transform.position = Vector3.Lerp(_fromPos, _toPos, _lerpValue);
            }
            else
            {
                _lerpValue = 0;
                _aiming = false;
                _onFinish?.Invoke();
            }
        }
    }
}
