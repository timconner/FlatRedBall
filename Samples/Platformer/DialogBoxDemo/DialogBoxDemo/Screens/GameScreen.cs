using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using FlatRedBall.AI.Pathfinding;
using FlatRedBall.Graphics.Animation;
using FlatRedBall.Gui;
using FlatRedBall.Math;
using FlatRedBall.Math.Geometry;
using FlatRedBall.Localization;
using Microsoft.Xna.Framework;




namespace DialogBoxDemo.Screens
{
    public partial class GameScreen
    {
        private void CustomInitialize()
        {
            LocalizationManager.CurrentLanguage = 1;
            Map.Z = -3;
        }

        private void CustomActivity(bool firstTimeCalled)
        {
            
        }

        private void CustomDestroy()
        {
            
        }

        private static void CustomLoadStaticContent(string contentManagerName)
        {
            
        }
    }
}
