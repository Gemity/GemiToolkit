using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GemiExtensions
{
    public static AnimationClip GetClipByIndex(this Animation animation, int index)
    {
        int i = 0;
        foreach (AnimationState animationState in animation)
        {
            if (i == index)
                return animationState.clip;
            i++;
        }
        return null;
    }

    public static void TryMove2Target(this Transform transform, Vector3 target, float vel, Action onEndMove)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, vel * Time.deltaTime);
        if (Vector3.SqrMagnitude(transform.position - target) < 0.01f)
        {
            transform.position = target;
            onEndMove?.Invoke();
        }
    }

    public static void TryRotate2Target(this Transform transform, Quaternion target, float vel)
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, vel * Time.deltaTime);
    }

    public static T GetRandomElement<T>(this ICollection<T> collection)
    {
        int index = UnityEngine.Random.Range(0, collection.Count);
        return collection.ToList().ElementAt(index);
    }

    public static Coroutine InvokeRoutine(this MonoBehaviour mb, Action action, float delay)
    {
        return mb.StartCoroutine(InvokeRoutine(action, delay));
    }

    private static IEnumerator InvokeRoutine(System.Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action();
    }

    /// <summary>
    /// value in range (min, max)
    /// </summary>
    public static bool InRange(this float value, float min, float max)
    {
        return min < value && value < max;
    }

    /// <summary>
    /// value in range [min, max]
    /// </summary>
    public static bool InRange(this int value, int min, int max)
    {
        return min <= value && value <= max;
    }
}
