using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SS.View;

public class GameplayController : Controller
{
    public const string GAMEPLAY_SCENE_NAME = "Gameplay";

    public override string SceneName()
    {
        return GAMEPLAY_SCENE_NAME;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            Manager.Add(PopupController.POPUP_SCENE_NAME, 123);

        if (Input.GetMouseButtonDown(1))
            Manager.Close();
    }
}