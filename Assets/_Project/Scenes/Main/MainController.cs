using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SS.View;

public class MainController : Controller
{
    public const string MAIN_SCENE_NAME = "Main";

    public override string SceneName()
    {
        return MAIN_SCENE_NAME;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Manager.Load(GameplayController.GAMEPLAY_SCENE_NAME);
        }
    }
}