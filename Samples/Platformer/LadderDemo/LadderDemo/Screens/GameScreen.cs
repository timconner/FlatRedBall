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

namespace LadderDemo.Screens;

public partial class GameScreen
{
    private void CustomInitialize()
    {
        Map.Z = -3;
    }

    private void CustomActivity(bool firstTimeCalled)
    {
        DoCollisionActivity();
    }

    private void CustomDestroy()
    {
        
    }

    private static void CustomLoadStaticContent(string contentManagerName)
    {
        
    }

    void DoCollisionActivity()
    {
        // first we reset the collision...
        foreach (var player in PlayerList)
        {
            player.LastCollisionLadderRectange = null;
        }
        // Then we do the collision which sets LastCollisionLadderRectange if a collision happens
        PlayerVsLadderCollision.DoCollisions();
    }
}