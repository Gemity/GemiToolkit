using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gemity.GemiTouch;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GemiTouch.EnableFinger = true;
        GemiTouch.OnFingerDown += finger => Debug.Log("Down");
        GemiTouch.OnFingerUp += finger => Debug.Log("Up");
        GemiTouch.OnFingerTap += finger => Debug.Log("Tap");
        GemiTouch.OnFingerDoubleTap += finger => Debug.Log("Double Tap");
    }
}
