using Gemity.GemiSS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Controller : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;

    public virtual void OnActive(object data) { }
    public virtual void OnShow() { }
    public virtual void OnHidden() { }




    private void Reset()
    {
        _canvas = GetComponentInChildren<Canvas>();
    }
}
