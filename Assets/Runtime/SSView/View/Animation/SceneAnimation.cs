// This code is part of the SS-Scene library, released by Anh Pham (anhpt.csit@gmail.com).

using UnityEngine;
using System.Collections;
using Cysharp.Threading.Tasks;

namespace SS.View
{
    [ExecuteInEditMode]
    public abstract class SceneAnimation : Tweener
    {
        protected enum StartState  
        {
            IDLE,
            SHOW,
            HIDE
        }

        protected StartState _startState;

        public virtual void Init() { }

        /// <summary>
        /// After a scene is loaded, its view will be put at center of screen. So you have to put it somewhere temporary before playing the show-animation.
        /// </summary>
        public virtual void HideBeforeShowing()
        {
        }

        /// <summary>
        /// Play the show-animation. Don't forget to call OnShown right after the animation finishes.
        /// </summary>
        public abstract UniTask Show();

        /// <summary>
        /// Play the hide-animation. Don't forget to call OnHidden right after the animation finishes.
        /// </summary>
        public abstract UniTask Hide();
    }
}

