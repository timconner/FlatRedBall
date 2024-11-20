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
using MultiplayerPlatformerDemo.GumRuntimes;

namespace MultiplayerPlatformerDemo.Screens;

public partial class GameScreen
{
    public static IndividualJoinComponentRuntime.JoinCategory[] PlayerJoinStates
    {
        get;
        private set;
    } = new IndividualJoinComponentRuntime.JoinCategory[4];

    void CustomInitialize()
    {
        for (int i = 0; i < PlayerJoinStates.Length; i++)
        {
            if (PlayerJoinStates[i] == IndividualJoinComponentRuntime.JoinCategory.Joined)
            {
                var player = Factories.PlayerFactory.CreateNew(160 + 16 * i, -260);
                player.SetIndex(i);
                player.InitializePlatformerInput(InputManager.Xbox360GamePads[i]);
            }
        }

        // Offset the number of layers so players show up in front of the map
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