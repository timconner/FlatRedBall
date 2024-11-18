using System;
using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Specialized;
using FlatRedBall.Audio;
using FlatRedBall.Screens;
using CheckpointAndLevelEndDemo.Entities;
using CheckpointAndLevelEndDemo.Screens;

namespace CheckpointAndLevelEndDemo.Screens;

public partial class GameScreen
{
    void OnPlayerVsPitCollisionCollided (Player player, FlatRedBall.Math.Geometry.ShapeCollection shapeCollection) 
    {
        RestartScreen(reloadScreenContent: false);
    }

    void OnPlayerVsCheckpointCollided (Player player, Checkpoint checkpoint) 
    {
        if (checkpoint.Visible)
        {
            // This is a checkpoint that you can actually touch and "turn on"
            checkpoint.MarkAsChecked();

            LastCheckpointName = checkpoint.Name;
        }
    }

    void OnPlayerVsEndOfLevelCollided (Player player, EndOfLevel endOfLevel) 
    {
        LastCheckpointName = "LevelStart";
        MoveToScreen(endOfLevel.NextLevel);
    }
}