﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilerLibrary.Models
{
    public class ToolbarEntityAndState
    {
        public string EntityName { get; set; }
        public string CategoryName { get; set; }
        public string StateName { get; set; }
    }
    public class CompilerSettingsModel
    {
        public bool GenerateGlueControlManagerCode { get; set; }

        public bool EmbedGameInGameTab { get; set; }

        public bool RestartScreenOnLevelContentChange { get; set; }
        public int PortNumber { get; set; } = 8021;
        public bool ShowScreenBoundsWhenViewingEntities { get; set; }

        public bool ShowGrid { get; set; } = true;
        public decimal GridSize { get; set; } = 32;

        public bool EnableSnapping { get; set; }
        public decimal SnapSize { get; set; }
        public decimal PolygonPointSnapSize { get; set; }

        public bool SetBackgroundColor { get; set; } = false;
        public int BackgroundRed { get; set; }
        public int BackgroundGreen { get; set; }
        public int BackgroundBlue { get; set; }
        public List<ToolbarEntityAndState> ToolbarEntitiesAndStates { get; set; } = new List<ToolbarEntityAndState>();

        public void SetDefaults()
        {
            EmbedGameInGameTab = true;
            ShowScreenBoundsWhenViewingEntities = true;
            RestartScreenOnLevelContentChange = true;

            EnableSnapping = true;
            SnapSize = 8;
            PolygonPointSnapSize = 1;
        }
    }
}