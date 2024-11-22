using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;

public static class GemiUtilities
{
    public static Sprite CreateSprite(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100);
        return sprite;
    }

    public static string FormatString(float f)
    {
        return f.ToString("F2");
    }

    public static float InverseLerpColor(Color a, Color b, Color value)
    {
        float t = Mathf.InverseLerp(0f, Vector4.Distance(a, b), Vector4.Distance(value, a));
        return t;
    }

    public static Texture2D CaptureCamera(Camera cam)
    {
        RenderTexture screenTexture = new(Screen.width, Screen.height, 16);
        cam.targetTexture = screenTexture;
        RenderTexture.active = screenTexture;
        cam.Render();
        Texture2D renderedTexture = new(Screen.width, Screen.height);
        renderedTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        RenderTexture.active = null;
        renderedTexture.Apply();
        return renderedTexture;
    }

    public static RenderTexture CaptureCamera(Camera camera, LayerMask layer)
    {
        var preMask = camera.cullingMask;
        var preRtActive = camera.targetTexture;

        camera.cullingMask = layer;
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 16);
        rt.format = RenderTextureFormat.ARGB32;
        camera.targetTexture = rt;
        camera.Render();
        camera.targetTexture = preRtActive;
        camera.cullingMask = preMask;

        return rt;
    }

    public static void DebugDrawBounds(Bounds b, Color color, float delay = 0)
    {
        // bottom
        var p1 = new Vector3(b.min.x, b.min.y, b.min.z);
        var p2 = new Vector3(b.max.x, b.min.y, b.min.z);
        var p3 = new Vector3(b.max.x, b.min.y, b.max.z);
        var p4 = new Vector3(b.min.x, b.min.y, b.max.z);

        Debug.DrawLine(p1, p2, color, delay);
        Debug.DrawLine(p2, p3, color, delay);
        Debug.DrawLine(p3, p4, color, delay);
        Debug.DrawLine(p4, p1, color, delay);

        // top
        var p5 = new Vector3(b.min.x, b.max.y, b.min.z);
        var p6 = new Vector3(b.max.x, b.max.y, b.min.z);
        var p7 = new Vector3(b.max.x, b.max.y, b.max.z);
        var p8 = new Vector3(b.min.x, b.max.y, b.max.z);

        Debug.DrawLine(p5, p6, color, delay);
        Debug.DrawLine(p6, p7, color, delay);
        Debug.DrawLine(p7, p8, color, delay);
        Debug.DrawLine(p8, p5, color, delay);

        // sides
        Debug.DrawLine(p1, p5, color, delay);
        Debug.DrawLine(p2, p6, color, delay);
        Debug.DrawLine(p3, p7, color, delay);
        Debug.DrawLine(p4, p8, color, delay);
    }

    public static void GizmosDrawBounds(Bounds b, Color color)
    {
        Gizmos.color = color;
        // bottom
        var p1 = new Vector3(b.min.x, b.min.y, b.min.z);
        var p2 = new Vector3(b.max.x, b.min.y, b.min.z);
        var p3 = new Vector3(b.max.x, b.min.y, b.max.z);
        var p4 = new Vector3(b.min.x, b.min.y, b.max.z);

        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p4);
        Gizmos.DrawLine(p4, p1);

        // top
        var p5 = new Vector3(b.min.x, b.max.y, b.min.z);
        var p6 = new Vector3(b.max.x, b.max.y, b.min.z);
        var p7 = new Vector3(b.max.x, b.max.y, b.max.z);
        var p8 = new Vector3(b.min.x, b.max.y, b.max.z);

        Gizmos.DrawLine(p5, p6);
        Gizmos.DrawLine(p6, p7);
        Gizmos.DrawLine(p7, p8);
        Gizmos.DrawLine(p8, p5);

        // sides
        Gizmos.DrawLine(p1, p5);
        Gizmos.DrawLine(p2, p6);
        Gizmos.DrawLine(p3, p7);
        Gizmos.DrawLine(p4, p8);
    }

    public static float ConvertDegToRad(float degrees)
    {
        return ((float)Math.PI / (float)180) * degrees;
    }

    public static Matrix4x4 GetTranslationMatrix(Vector3 position)
    {
        return new Matrix4x4(new Vector4(1, 0, 0, 0),
                             new Vector4(0, 1, 0, 0),
                             new Vector4(0, 0, 1, 0),
                             new Vector4(position.x, position.y, position.z, 1));
    }

    public static Matrix4x4 GetRotationMatrix(Vector3 anglesDeg)
    {
        anglesDeg = new Vector3(ConvertDegToRad(anglesDeg[0]), ConvertDegToRad(anglesDeg[1]), ConvertDegToRad(anglesDeg[2]));

        Matrix4x4 rotationX = new Matrix4x4(new Vector4(1, 0, 0, 0),
                                            new Vector4(0, Mathf.Cos(anglesDeg[0]), Mathf.Sin(anglesDeg[0]), 0),
                                            new Vector4(0, -Mathf.Sin(anglesDeg[0]), Mathf.Cos(anglesDeg[0]), 0),
                                            new Vector4(0, 0, 0, 1));

        Matrix4x4 rotationY = new Matrix4x4(new Vector4(Mathf.Cos(anglesDeg[1]), 0, -Mathf.Sin(anglesDeg[1]), 0),
                                            new Vector4(0, 1, 0, 0),
                                            new Vector4(Mathf.Sin(anglesDeg[1]), 0, Mathf.Cos(anglesDeg[1]), 0),
                                            new Vector4(0, 0, 0, 1));

        Matrix4x4 rotationZ = new Matrix4x4(new Vector4(Mathf.Cos(anglesDeg[2]), Mathf.Sin(anglesDeg[2]), 0, 0),
                                            new Vector4(-Mathf.Sin(anglesDeg[2]), Mathf.Cos(anglesDeg[2]), 0, 0),
                                            new Vector4(0, 0, 1, 0),
                                            new Vector4(0, 0, 0, 1));

        return rotationX * rotationY * rotationZ;
    }

    public static Matrix4x4 GetScaleMatrix(Vector3 scale)
    {
        return new Matrix4x4(new Vector4(scale.x, 0, 0, 0),
                             new Vector4(0, scale.y, 0, 0),
                             new Vector4(0, 0, scale.z, 0),
                             new Vector4(0, 0, 0, 1));
    }

    public static Matrix4x4 Get_TRS_Matrix(Vector3 position, Vector3 rotationAngles, Vector3 scale)
    {
        return GetTranslationMatrix(position) * GetRotationMatrix(rotationAngles) * GetScaleMatrix(scale);
    }

    public static Mesh CombineMesh(MeshFilter[] meshFilters, out Vector3 pivot)
    {
        pivot = Vector3.zero;
        for (int i = 0; i < meshFilters.Length; i++)
            pivot += meshFilters[i].transform.position;
        pivot /= meshFilters.Length;

        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            Transform tf = meshFilters[i].transform;
            combine[i].transform = Matrix4x4.TRS(tf.position - pivot, tf.rotation, tf.lossyScale);
        }

        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine);
        return mesh;
    }
}