using System;
using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Specialized;
using FlatRedBall.Audio;
using FlatRedBall.Screens;
using LadderDemo.Entities;
using LadderDemo.Screens;
using FlatRedBall.TileCollisions;
using System.Linq;

namespace LadderDemo.Screens;

public partial class GameScreen
{
    void OnPlayerVsLadderCollisionCollided (Player player, TileShapeCollection ladder) 
    {
        player.LastCollisionLadderRectange = ladder.LastCollisionAxisAlignedRectangles.First();

        // a little inefficient, could use caching to save a little calculation but it won't be too bad:
        var topRectangle = player.LastCollisionLadderRectange;

        var rectangleAbove = ladder.GetRectangleAtPosition(topRectangle.X, topRectangle.Y + ladder.GridSize);

        while (rectangleAbove != null)
        {
            topRectangle = rectangleAbove;
            rectangleAbove = ladder.GetRectangleAtPosition(topRectangle.X, topRectangle.Y + ladder.GridSize);
        }

        player.TopOfLadderY = topRectangle.Bottom;
    }
}