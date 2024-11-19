﻿using Gum.Wireframe;
using System;
using System.Collections.Generic;
using System.Text;
using FlatRedBall.Gui;
using RenderingLibrary;
using FlatRedBall.Forms.GumExtensions;
using FlatRedBall.Forms.Controls.Primitives;

namespace FlatRedBall.Forms.Controls;

public class ScrollBar : RangeBase
{
    #region Fields/Properties

    Button upButton;
    Button downButton;

    public float MinimumThumbSize { get; set; } = 16;
    
    double viewportSize = .1;
    public double ViewportSize
    {
        get { return viewportSize; }
        set
        {
#if DEBUG
            if(double.IsNaN(value))
            {
                throw new Exception("ScrollBar ViewportSize cannot be float.NaN");
            }
#endif
            viewportSize = value;

            UpdateThumbSize();
            UpdateThumbPositionAccordingToValue();

        }
    }


    float MinThumbPosition => 0;
    float MaxThumbPosition => Track.GetAbsoluteHeight() - thumb.ActualHeight;


    #endregion

    #region Initialize

    public ScrollBar() : base() { }

    public ScrollBar(GraphicalUiElement visual) : base(visual) { }

    protected override void ReactToVisualChanged()
    {
        var upButtonVisual = this.Visual.GetGraphicalUiElementByName("UpButtonInstance");
#if DEBUG
        if (upButtonVisual == null)
        {
            throw new Exception("The ScrollBar Gum object must have a button called UpButtonInstance");
        }
#endif
        if(upButtonVisual.FormsControlAsObject == null)
        {
            upButton = new Button(upButtonVisual);
        }
        else
        {
            upButton = upButtonVisual.FormsControlAsObject as Button;
        }

        var downButtonVisual = this.Visual.GetGraphicalUiElementByName("DownButtonInstance");
#if DEBUG
        if(downButtonVisual == null)
        {
            throw new Exception("The ScrollBar Gum object must have a button called DownButtonInstance");
        }
#endif
        if(downButtonVisual.FormsControlAsObject == null)
        {
            downButton = new Button(downButtonVisual);
        }
        else
        {
            downButton = downButtonVisual.FormsControlAsObject as Button;
        }


        base.ReactToVisualChanged();

        
        var thumbHeight = thumb.ActualHeight;

        upButton.Push += (not, used) => this.Value -= this.SmallChange;
        downButton.Push += (not, used) => this.Value += this.SmallChange;
        Track.Push += HandleTrackPush;
        Visual.SizeChanged += HandleVisualSizeChange;



        var visibleTrackSpace = Track.Height - upButton.ActualHeight - downButton.ActualHeight;

        if(visibleTrackSpace != 0)
        {
            var thumbRatio = thumbHeight / visibleTrackSpace;

            ViewportSize = (Maximum - Minimum) * thumbRatio;
            LargeChange = ViewportSize;

            Value = Minimum;
        }
        else
        {
            ViewportSize = 10;
            LargeChange = 10;
            SmallChange = 2;
        }
    }

    #endregion

    #region Event Handlers
    protected override void HandleThumbPush(object sender, EventArgs e)
    {
        var topOfThumb = this.thumb.ActualY;
        var cursorScreen = GuiManager.Cursor.GumY();

        cursorGrabOffsetRelativeToThumb = cursorScreen - topOfThumb;
    }

    private void HandleTrackPush(IWindow window)
    {
        if (GuiManager.Cursor.GumY() < thumb.ActualY)
        {
            Value -= LargeChange;
        }
        else if (GuiManager.Cursor.GumY() > thumb.ActualY + thumb.ActualHeight)
        {
            Value += LargeChange;
        }
    }

    private void HandleVisualSizeChange(object sender, EventArgs e)
    {
        UpdateThumbPositionAccordingToValue();
        UpdateThumbSize();
    }

    protected override void OnMinimumChanged(double oldMinimum, double newMinimum)
    {
        base.OnMinimumChanged(oldMinimum, newMinimum);

        UpdateThumbSize();
        UpdateThumbPositionAccordingToValue();
    }

    protected override void OnMaximumChanged(double oldMaximum, double newMaximum)
    {
        base.OnMaximumChanged(oldMaximum, newMaximum);

        UpdateThumbSize();
        UpdateThumbPositionAccordingToValue();
    }

    protected override void OnValueChanged(double oldValue, double newValue)
    {
        base.OnValueChanged(oldValue, newValue);

        UpdateThumbPositionAccordingToValue();
    }

    #endregion

    #region UpdateTo Methods

    private void UpdateThumbPositionAccordingToValue()
    {
        var ratioDown = (Value - Minimum) / (Maximum - Minimum);
        ratioDown = System.Math.Max(0, ratioDown);
        ratioDown = System.Math.Min(1, ratioDown);
        if (Maximum <= Minimum)
        {
            ratioDown = 0;
        }

        thumb.Visual.YUnits = global::Gum.Converters.GeneralUnitType.PixelsFromSmall;
        thumb.Visual.HeightUnits = global::Gum.DataTypes.DimensionUnitType.Absolute;

        thumb.Visual.YOrigin = RenderingLibrary.Graphics.VerticalAlignment.Top;
        thumb.Y = Microsoft.Xna.Framework.MathHelper.Lerp(MinThumbPosition, MaxThumbPosition, 
            (float)ratioDown);

    }

    protected override void UpdateThumbPositionToCursorDrag(Cursor cursor)
    {
        var cursorScreenY = cursor.GumY();
        var cursorYRelativeToTrack = cursorScreenY - Track.GetTop();


        thumb.Visual.YUnits = global::Gum.Converters.GeneralUnitType.PixelsFromSmall;
        thumb.Visual.HeightUnits = global::Gum.DataTypes.DimensionUnitType.Absolute;

        thumb.Y = cursorYRelativeToTrack - cursorGrabOffsetRelativeToThumb;

        float range = MaxThumbPosition - MinThumbPosition;

        var valueBefore = Value;
        if(range != 0)
        {
            var ratio = (thumb.Y) / range;
            var ratioBefore = ratio;
            ratio = System.Math.Max(0, ratio);
            ratio = System.Math.Min(1, ratio);


            Value = Minimum + (Maximum - Minimum) * ratio;

            if(valueBefore != Value)
            {
                RaiseValueChangedByUi();
            }

            if(ratioBefore != ratio)
            {
                // we clamped it, so force the thumb:
                UpdateThumbPositionAccordingToValue();
            }
        }
        else
        {
            // In this case the user may have dragged the thumb outside of its bounds. We are resetting
            // the value back to the minimum, but the value may already be 0, so the if check will bypass
            // the updating of the value...
            var shouldForceUpdateThumb = Value == Minimum;
            
            Value = Minimum;

            if (valueBefore != Value)
            {
                RaiseValueChangedByUi();
            }

            if (shouldForceUpdateThumb)
            {
                UpdateThumbPositionAccordingToValue();
            }
        }
    }
    
    private void UpdateThumbSize()
    {
        var desiredHeight = MinimumThumbSize;
        if(ViewportSize != 0)
        {
            float trackHeight = Track.GetAbsoluteHeight();

            var valueRange = (Maximum - Minimum) + ViewportSize;
            if(valueRange > 0)
            {
                var thumbRatio = ViewportSize / valueRange;

                thumb.Visual.YUnits = global::Gum.Converters.GeneralUnitType.PixelsFromSmall;
                thumb.Visual.HeightUnits = global::Gum.DataTypes.DimensionUnitType.Absolute;

                thumb.Height = System.Math.Max(MinimumThumbSize, (float)(trackHeight * thumbRatio));
            }
        }
    }

    #endregion
}
