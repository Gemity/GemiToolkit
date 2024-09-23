using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPSCounter
{
    public float startTime, startFrameCount;
    private int fps;

    public FPSCounter()
    {
        this.startTime = Time.realtimeSinceStartup;
        this.startFrameCount = Time.frameCount;
    }

    public async void FPSConourine()
    {
        while (true)
        {
            var counter = new FPSCounter();
            await System.Threading.Tasks.Task.Delay(1000);
            fps = counter.GetFPS();
        }
    }

    public int GetFPS()
    {
        fps = Mathf.FloorToInt((Time.frameCount - startFrameCount) / (Time.realtimeSinceStartup - startTime));
        return fps;
    }

    public float GetTime()
    {
        return Time.realtimeSinceStartup - startTime;
    }
}


public class FPSDevide
{
    int fps;
    int highest, lowest;

    public FPSDevide()
    {
        fps = 0;
        highest = 0;
        lowest = 1000;
    }

    public int LowestFps => lowest;

    public int HighestFps => highest;

    public int Fps => fps;

    public IEnumerator GetFPS()
    {
        while (true)
        {
            var u = new FPSCounter();
            yield return new WaitForSecondsRealtime(1);
            fps = u.GetFPS();
            highest = Mathf.Max(fps, highest);
            lowest = Mathf.Min(fps, lowest);
        }
    }
}

public class ShowFPS : MonoBehaviour
{
    private static ShowFPS instance;
    internal int currentFPS;
    FPSDevide fpsGameplay;
    FPSDevide fpsInGame;
    FPSCounter counterInGame;

    public static float PlayTimeSec => instance.counterInGame.GetTime();
    public static int LowestFps => instance.fpsGameplay.LowestFps;
    public static int HighestFps => instance.fpsGameplay.HighestFps;
    public static int CurrentFPS => instance.currentFPS;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void DisplayFPS()
    {
        var gameObject = new GameObject("Show FPS");
        gameObject.SetActive(true);
        var u = gameObject.AddComponent<ShowFPS>();
        instance = u;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        fpsInGame = new FPSDevide();
        StartCoroutine(fpsInGame.GetFPS());
    }

    private void OnGUI()
    {
        if (!Debug.isDebugBuild)
            return;
        currentFPS = fpsInGame.Fps;
        GUI.skin.label.fontSize = 24;
        GUI.skin.label.fontStyle = FontStyle.Bold;
        GUI.contentColor = Color.red;
        GUI.Label(new Rect(40, 40, 200, 40), currentFPS.ToString());

    }

    public static void OnBeginDraw()
    {
        instance.counterInGame = new FPSCounter();
        instance.fpsGameplay = new FPSDevide();
        instance.StartCoroutine(instance.fpsGameplay.GetFPS());
    }
}
