using System;
using System.Collections.Generic;
using FlatRedBall.Math;
using FlatRedBall.Math.Geometry;
using FlatRedBall.Performance.Measurement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlatRedBall.Graphics
{
    #region Enums

    /// <summary>
    /// Rendering modes available in FlatRedBall.
    /// </summary>
    public enum RenderMode
    {
        /// <summary>
        /// Default rendering mode. Uses embedded effects in models.
        /// </summary>
        Default,
        /// <summary>
        /// Color rendering mode. Renders color values for a model (does not
        /// include lighting information). Effect technique: RenderColor.
        /// </summary>
        Color,
        /// <summary>
        /// Normals rendering mode. Renders normals. Effect technique: RenderNormals.
        /// </summary>
        Normals,
        /// <summary>
        /// Depth rendering mode. Renders depth. Effect technique: RenderDepth.
        /// </summary>
        Depth,
        /// <summary>
        /// Position rendering mode. Renders position. Effect technique: RenderPosition.
        /// </summary>
        Position
    }

    #endregion

    static partial class Renderer
    {
        static IComparer<Sprite> mSpriteComparer;
        static IComparer<Text> mTextComparer;
        static IComparer<IDrawableBatch> mDrawableBatchComparer;

        /// <summary>
        /// Gets the default camera (SpriteManager.Camera).
        /// </summary>
        static public Camera Camera { get { return SpriteManager.Camera; } }

        /// <summary>
        /// Gets the list of cameras (SpriteManager.Cameras).
        /// </summary>
        static public PositionedObjectList<Camera> Cameras { get { return SpriteManager.Cameras; } }

        public static IComparer<Sprite> SpriteComparer { get { return mSpriteComparer; } set { mSpriteComparer = value; } }

        public static IComparer<Text> TextComparer { get { return mTextComparer; } set { mTextComparer = value; } }

        public static IComparer<IDrawableBatch> DrawableBatchComparer { get { return mDrawableBatchComparer; } set { mDrawableBatchComparer = value; } }

        /// <summary>
        /// Controls whether the renderer will refresh the sorting of its internal lists in the next draw call.
        /// </summary>
        public static bool UpdateSorting { get { return mUpdateSorting; } set { mUpdateSorting = value; } }
        static bool mUpdateSorting = true;

        /// <summary>
        /// Controls whether the renderer will update the drawable batches in the next draw call.
        /// </summary>
        public static bool UpdateDrawableBatches { get { return mUpdateDrawableBatches; } set { mUpdateDrawableBatches = value; } }
        static bool mUpdateDrawableBatches = true;

        static void DrawIndividualLayer(Camera camera, RenderMode renderMode, Layer layer, Section section,
            ref RenderTarget2D lastRenderTarget, ref Viewport lastViewport)
        {
            bool hasLayerModifiedCamera = false;

            if (layer.Visible)
            {
                CurrentLayer = layer;

                if (section != null)
                {
                    string layerName = "No Layer";

                    if (layer != null)
                    {
                        layerName = layer.Name;
                    }

                    Section.GetAndStartContextAndTime("Layer: " + layerName);
                }

                bool didSetRenderTarget = layer.RenderTarget != lastRenderTarget;

                if (didSetRenderTarget)
                {
                    lastRenderTarget = layer.RenderTarget;

                    if (layer.RenderTarget != null)
                    {
                        lastViewport = GraphicsDevice.Viewport;
                    }

                    GraphicsDevice.SetRenderTarget(layer.RenderTarget);

                    if (layer.RenderTarget == null)
                    {
                        GraphicsDevice.Viewport = lastViewport;
                    }

                    if (layer.RenderTarget != null)
                    {
                        mGraphics.GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 1, 0);
                    }
                }

                // No need to clear depth buffer if it's a render target
                if (!didSetRenderTarget)
                {
                    ClearBackgroundForLayer(camera);
                }

                #region Set view and projection

                // Store the camera's FieldOfView in the oldFieldOfView and set the
                // camera's FieldOfView to the layer's OverridingFieldOfView if necessary.
                mOldCameraLayerSettings.SetFromCamera(camera);

                var oldPosition = camera.Position;
                var oldUpVector = camera.UpVector;

                if (layer.LayerCameraSettings != null)
                {
                    layer.LayerCameraSettings.ApplyValuesToCamera(camera, SetCameraOptions.PerformZRotation, null, layer.RenderTarget);
                    hasLayerModifiedCamera = true;
                }

                camera.SetDeviceViewAndProjection(mCurrentEffect, layer.RelativeToCamera);

                #endregion

                if (renderMode == RenderMode.Default)
                {
                    if (layer.mZBufferedSprites.Count > 0)
                    {
                        DrawZBufferedSprites(camera, layer.mZBufferedSprites);
                    }

                    // Draw the camera's layer
                    DrawMixed(layer.mSprites, layer.mSortType,
                        layer.mTexts, layer.mBatches, layer.RelativeToCamera, camera, section);

                    // Draw shapes
                    DrawShapes(camera,
                        layer.mSpheres,
                        layer.mCubes,
                        layer.mRectangles,
                        layer.mCircles,
                        layer.mPolygons,
                        layer.mLines,
                        layer.mCapsule2Ds,
                        layer);
                }

                // Set the camera's field of view back.
                // Vic asks: What if the user wants to have a wacky field of view?
                // Does that mean that this will regulate it on layers? This is something
                // that may need to be fixed in the future, but it seems rare and will bloat
                // the visible property list considerably. Let's leave it like this for now
                // to establish a pattern then if the time comes to change this we'll be comfortable
                // with the overriding field of view pattern so a better decision can be made.
                if (hasLayerModifiedCamera)
                {
                    // Use the render target here, because it may not have been unset yet.
                    mOldCameraLayerSettings.ApplyValuesToCamera(camera, SetCameraOptions.ApplyMatrix, layer.LayerCameraSettings, layer.RenderTarget);
                    camera.Position = oldPosition;
                    camera.UpVector = oldUpVector;
                }

                if (section != null)
                {
                    Section.EndContextAndTime();
                }
            }
        }

        static List<Sprite> mVisibleSprites = new List<Sprite>();
        static List<Text> mVisibleTexts = new List<Text>();
        static List<IDrawableBatch> mVisibleBatches = new List<IDrawableBatch>();

        struct SortValues
        {
            public float PrimarySortValue;
            public float SecondarySortValue;

            public override string ToString()
            {
                return $"{PrimarySortValue} ({SecondarySortValue})";
            }

            // Overloading the '<' operator
            public static bool operator <(SortValues left, SortValues right)
            {
                return left.PrimarySortValue < right.PrimarySortValue ||
                    (left.PrimarySortValue == right.PrimarySortValue && left.SecondarySortValue < right.SecondarySortValue);
            }

            public static bool operator >(SortValues left, SortValues right)
            {
                return left.PrimarySortValue > right.PrimarySortValue ||
                    (left.PrimarySortValue == right.PrimarySortValue && left.SecondarySortValue > right.SecondarySortValue);
            }

            public static bool operator <=(SortValues left, SortValues right)
            {
                return (left.PrimarySortValue == right.PrimarySortValue && left.SecondarySortValue == right.SecondarySortValue) || left < right;
            }

            public static bool operator >=(SortValues left, SortValues right)
            {
                return (left.PrimarySortValue == right.PrimarySortValue && left.SecondarySortValue == right.SecondarySortValue) || left > right;
            }
        }

        static void DrawMixed(SpriteList spriteListUnfiltered, SortType sortType,
            PositionedObjectList<Text> textListUnfiltered, List<IDrawableBatch> batches,
            bool relativeToCamera, Camera camera, Section section)
        {
            if (section != null)
            {
                Section.GetAndStartContextAndTime("Start of Draw Mixed");
            }

            DrawMixedStart(camera);

            int spriteIndex = 0;
            int textIndex = 0;
            int batchIndex = 0;

            // The sort values can represent different
            // things depending on the sortType argument.
            // They can either represent pure Z values or they
            // can represent distance from the camera (squared).
            // The problem is that a larger Z means closer to the
            // camera, but a larger distance means further from the
            // camera. Therefore, to fix this problem if these values
            // represent distance from camera, they will be multiplied by
            // negative 1.
            var nextSpriteSortValue = new SortValues { PrimarySortValue = float.PositiveInfinity };
            var nextTextSortValue = new SortValues { PrimarySortValue = float.PositiveInfinity };
            var nextBatchSortValue = new SortValues { PrimarySortValue = float.PositiveInfinity };

            if (section != null)
            {
                Section.EndContextAndTime();
                Section.GetAndStartContextAndTime("Sort Lists");
            }

            // Vic asks - why do we sort before identifying visible objects?
            // The reason is because we want to sort the actual lists that are
            // passed in so that the next time sorting is called, the lists are 
            // already sorted (or nearly so), making each subsequent sort faster.
            if (mUpdateSorting)
            {
                SortAllLists(spriteListUnfiltered, sortType, textListUnfiltered, batches, relativeToCamera, camera);
            }

            mVisibleSprites.Clear();
            mVisibleTexts.Clear();
            mVisibleBatches.Clear();

            if (batches != null)
            {
                for (int i = 0; i < batches.Count; i++)
                {
                    var shouldAdd = true;
                    var batch = batches[i];

                    if (batch is IVisible asIVisible)
                    {
                        shouldAdd = asIVisible.AbsoluteVisible;
                    }
                    if (shouldAdd)
                    {
                        mVisibleBatches.Add(batch);
                    }
                }
            }

            for (int i = 0; i < spriteListUnfiltered.Count; i++)
            {
                var sprite = spriteListUnfiltered[i];

                bool isVisible = sprite.AbsoluteVisible &&
                    (sprite.ColorOperation == ColorOperation.InterpolateColor || sprite.Alpha > .0001) &&
                    camera.IsSpriteInView(sprite, relativeToCamera);

                if (isVisible)
                {
                    mVisibleSprites.Add(sprite);
                }
            }

            for (int i = 0; i < textListUnfiltered.Count; i++)
            {
                var text = textListUnfiltered[i];

                if (text.AbsoluteVisible && text.Alpha > .0001 && camera.IsTextInView(text, relativeToCamera))
                {
                    mVisibleTexts.Add(text);
                }
            }

            int indexOfNextSpriteToReposition = 0;

            GetNextZValuesByCategory(mVisibleSprites, sortType, mVisibleTexts, mVisibleBatches, camera, ref spriteIndex, ref textIndex,
                ref nextSpriteSortValue, ref nextTextSortValue, ref nextBatchSortValue);

            int numberToDraw = 0;

            // This is used as a temporary variable for Z or distance from camera
            var sortingValue = new SortValues();

            Section performDrawingSection = null;
            if (section != null)
            {
                Section.EndContextAndTime();
                performDrawingSection = Section.GetAndStartContextAndTime("Perform Drawing");
            }

            while (spriteIndex < mVisibleSprites.Count || textIndex < mVisibleTexts.Count ||
                (batchIndex < mVisibleBatches.Count))
            {
                #region Only 1 array remains to be drawn so finish it off completely

                #region Draw texts

                if (spriteIndex >= mVisibleSprites.Count && (batchIndex >= mVisibleBatches.Count) &&
                    textIndex < mVisibleTexts.Count)
                {
                    if (section != null)
                    {
                        if (Section.Context != performDrawingSection)
                        {
                            Section.EndContextAndTime();
                        }

                        Section.GetAndStartMergedContextAndTime("Draw Texts");
                    }

                    if (sortType == SortType.DistanceAlongForwardVector)
                    {
                        int temporaryCount = mVisibleTexts.Count;

                        for (int i = textIndex; i < temporaryCount; i++)
                        {
                            mVisibleTexts[i].Position = mVisibleTexts[i].mOldPosition;
                        }
                    }

                    // Texts: draw all texts from textIndex to numberOfVisibleTexts - textIndex
                    DrawTexts(mVisibleTexts, textIndex, mVisibleTexts.Count - textIndex, camera, section);
                    break;
                }

                #endregion

                #region Draw Sprites

                else if (textIndex >= mVisibleTexts.Count && (batchIndex >= mVisibleBatches.Count) &&
                    spriteIndex < mVisibleSprites.Count)
                {
                    if (section != null)
                    {
                        if (Section.Context != performDrawingSection)
                        {
                            Section.EndContextAndTime();
                        }

                        Section.GetAndStartMergedContextAndTime("Draw Sprites");
                    }

                    numberToDraw = mVisibleSprites.Count - spriteIndex;

                    if (sortType == SortType.DistanceAlongForwardVector)
                    {
                        int temporaryCount = mVisibleSprites.Count;

                        for (int i = indexOfNextSpriteToReposition; i < temporaryCount; i++)
                        {
                            mVisibleSprites[i].Position = mVisibleSprites[i].mOldPosition;
                            indexOfNextSpriteToReposition++;
                        }
                    }

                    PrepareSprites(
                        mSpriteVertices, mSpriteRenderBreaks,
                        mVisibleSprites, spriteIndex, numberToDraw
                        );

                    DrawSprites(
                        mSpriteVertices, mSpriteRenderBreaks,
                        mVisibleSprites, spriteIndex,
                        numberToDraw, camera);

                    break;
                }

                #endregion

                #region Draw drawable batches

                else if (spriteIndex >= mVisibleSprites.Count && textIndex >= mVisibleTexts.Count &&
                    batchIndex < mVisibleBatches.Count)
                {
                    if (section != null)
                    {
                        if (Section.Context != performDrawingSection)
                        {
                            Section.EndContextAndTime();
                        }

                        Section.GetAndStartMergedContextAndTime("Draw IDrawableBatches");
                    }

                    // Only drawable batches remain so draw them all
                    while (batchIndex < mVisibleBatches.Count)
                    {
                        var batchAtIndex = mVisibleBatches[batchIndex];

                        if (RecordRenderBreaks)
                        {
                            // Even though we aren't using a RenderBreak here, we should record a render break
                            // for this batch as it does cause rendering to be interrupted:
                            var renderBreak = new RenderBreak();
#if DEBUG
                            renderBreak.ObjectCausingBreak = batchAtIndex;
#endif
                            renderBreak.LayerName = CurrentLayerName;
                            LastFrameRenderBreakList.Add(renderBreak);
                        }

                        batchAtIndex.Draw(camera);
                        batchIndex++;
                    }

                    FixRenderStatesAfterBatchDraw();
                    break;
                }

                #endregion

                #endregion

                #region More than 1 list remains so find which group of objects to render

                #region Sprites

                else if (nextSpriteSortValue <= nextTextSortValue && nextSpriteSortValue <= nextBatchSortValue && spriteIndex < mVisibleSprites.Count)
                {
                    if (section != null)
                    {
                        if (Section.Context != performDrawingSection)
                        {
                            Section.EndContextAndTime();
                        }

                        Section.GetAndStartMergedContextAndTime("Draw Sprites");
                    }

                    // The next furthest object is a sprite. Find how many to draw.

                    #region Count how many sprites to draw and store it in numberToDraw

                    numberToDraw = 0;

                    sortingValue.SecondarySortValue = 0;

                    if (sortType == SortType.Z || sortType == SortType.DistanceAlongForwardVector || sortType == SortType.ZSecondaryParentY || sortType == SortType.CustomComparer)
                    {
                        sortingValue.PrimarySortValue = mVisibleSprites[spriteIndex + numberToDraw].Position.Z;

                        if (sortType == SortType.ZSecondaryParentY)
                        {
                            sortingValue.SecondarySortValue = -mVisibleSprites[spriteIndex + numberToDraw].TopParent.Y;
                        }
                    }
                    else
                    {
                        sortingValue.PrimarySortValue = -(camera.Position - mVisibleSprites[spriteIndex + numberToDraw].Position).LengthSquared();
                    }

                    while (sortingValue <= nextTextSortValue &&
                           sortingValue <= nextBatchSortValue)
                    {
                        numberToDraw++;

                        if (spriteIndex + numberToDraw == mVisibleSprites.Count)
                        {
                            break;
                        }

                        sortingValue.SecondarySortValue = 0;

                        if (sortType == SortType.Z || sortType == SortType.DistanceAlongForwardVector || sortType == SortType.ZSecondaryParentY || sortType == SortType.CustomComparer)
                        {
                            sortingValue.PrimarySortValue = mVisibleSprites[spriteIndex + numberToDraw].Position.Z;

                            if (sortType == SortType.ZSecondaryParentY)
                            {
                                sortingValue.SecondarySortValue = -mVisibleSprites[spriteIndex + numberToDraw].TopParent.Y;
                            }
                        }
                        else
                        {
                            sortingValue.PrimarySortValue = -(camera.Position - mVisibleSprites[spriteIndex + numberToDraw].Position).LengthSquared();
                        }
                    }

                    #endregion

                    if (sortType == SortType.DistanceAlongForwardVector)
                    {
                        for (int i = indexOfNextSpriteToReposition; i < numberToDraw + spriteIndex; i++)
                        {
                            mVisibleSprites[i].Position = mVisibleSprites[i].mOldPosition;
                            indexOfNextSpriteToReposition++;
                        }
                    }

                    PrepareSprites(
                        mSpriteVertices, mSpriteRenderBreaks,
                        mVisibleSprites, spriteIndex,
                        numberToDraw);

                    DrawSprites(
                        mSpriteVertices, mSpriteRenderBreaks,
                        mVisibleSprites, spriteIndex,
                        numberToDraw, camera);

                    // numberToDraw represents a range so increase spriteIndex by that amount
                    spriteIndex += numberToDraw;

                    if (spriteIndex >= mVisibleSprites.Count)
                    {
                        nextSpriteSortValue.PrimarySortValue = float.PositiveInfinity;
                    }
                    else
                    {
                        nextSpriteSortValue.SecondarySortValue = 0;

                        if (sortType == SortType.Z || sortType == SortType.DistanceAlongForwardVector || sortType == SortType.ZSecondaryParentY || sortType == SortType.CustomComparer)
                        {
                            nextSpriteSortValue.PrimarySortValue = mVisibleSprites[spriteIndex].Position.Z;

                            if (sortType == SortType.ZSecondaryParentY)
                            {
                                nextSpriteSortValue.SecondarySortValue = -mVisibleSprites[spriteIndex].TopParent.Y;
                            }
                        }
                        else
                        {
                            nextSpriteSortValue.PrimarySortValue = -(camera.Position - mVisibleSprites[spriteIndex].Position).LengthSquared();
                        }
                    }
                }

                #endregion

                #region Texts

                else if (nextTextSortValue <= nextSpriteSortValue && nextTextSortValue <= nextBatchSortValue) // Draw texts
                {
                    if (section != null)
                    {
                        if (Section.Context != performDrawingSection)
                        {
                            Section.EndContextAndTime();
                        }

                        Section.GetAndStartMergedContextAndTime("Draw Texts");
                    }

                    numberToDraw = 0;

                    if (sortType == SortType.Z || sortType == SortType.DistanceAlongForwardVector)
                        sortingValue.PrimarySortValue = mVisibleTexts[textIndex + numberToDraw].Position.Z;
                    else
                        sortingValue.PrimarySortValue = -(camera.Position - mVisibleTexts[textIndex + numberToDraw].Position).LengthSquared();

                    while (sortingValue <= nextSpriteSortValue &&
                           sortingValue <= nextBatchSortValue)
                    {
                        numberToDraw++;

                        if (textIndex + numberToDraw == mVisibleTexts.Count)
                        {
                            break;
                        }

                        if (sortType == SortType.Z || sortType == SortType.DistanceAlongForwardVector)
                            sortingValue.PrimarySortValue = mVisibleTexts[textIndex + numberToDraw].Position.Z;
                        else
                            sortingValue.PrimarySortValue = -(camera.Position - mVisibleTexts[textIndex + numberToDraw].Position).LengthSquared();

                    }

                    if (sortType == SortType.DistanceAlongForwardVector)
                    {
                        for (int i = textIndex; i < textIndex + numberToDraw; i++)
                        {
                            mVisibleTexts[i].Position = mVisibleTexts[i].mOldPosition;
                        }
                    }

                    DrawTexts(mVisibleTexts, textIndex, numberToDraw, camera, section);

                    textIndex += numberToDraw;

                    if (textIndex == mVisibleTexts.Count)
                    {
                        nextTextSortValue.PrimarySortValue = float.PositiveInfinity;
                    }
                    else
                    {
                        if (sortType == SortType.Z || sortType == SortType.DistanceAlongForwardVector || sortType == SortType.ZSecondaryParentY || sortType == SortType.CustomComparer)
                            nextTextSortValue.PrimarySortValue = mVisibleTexts[textIndex].Position.Z;
                        else
                            nextTextSortValue.PrimarySortValue = -(camera.Position - mVisibleTexts[textIndex].Position).LengthSquared();
                    }
                }

                #endregion

                #region Batches

                else if (nextBatchSortValue <= nextSpriteSortValue && nextBatchSortValue <= nextTextSortValue)
                {
                    if (section != null)
                    {
                        if (Section.Context != performDrawingSection)
                        {
                            Section.EndContextAndTime();
                        }

                        Section.GetAndStartMergedContextAndTime("Draw IDrawableBatches");
                    }

                    while (nextBatchSortValue <= nextSpriteSortValue && nextBatchSortValue <= nextTextSortValue && batchIndex < mVisibleBatches.Count)
                    {
                        var batchAtIndex = mVisibleBatches[batchIndex];

                        if (RecordRenderBreaks)
                        {
                            // Even though we aren't using a RenderBreak here, we should record a render break
                            // for this batch as it does cause rendering to be interrupted:
                            var renderBreak = new RenderBreak();
#if DEBUG
                            renderBreak.ObjectCausingBreak = batchAtIndex;
#endif
                            renderBreak.LayerName = CurrentLayerName;
                            LastFrameRenderBreakList.Add(renderBreak);
                        }

                        batchAtIndex.Draw(camera);
                        batchIndex++;

                        nextBatchSortValue.SecondarySortValue = 0;

                        if (batchIndex == mVisibleBatches.Count)
                        {
                            nextBatchSortValue.PrimarySortValue = float.PositiveInfinity;
                        }
                        else
                        {
                            batchAtIndex = mVisibleBatches[batchIndex];

                            if (sortType == SortType.Z || sortType == SortType.ZSecondaryParentY || sortType == SortType.CustomComparer)
                            {
                                nextBatchSortValue.PrimarySortValue = batchAtIndex.Z;

                                if (sortType == SortType.ZSecondaryParentY)
                                {
                                    nextBatchSortValue.SecondarySortValue = -batchAtIndex.Y;
                                }
                            }
                            else if (sortType == SortType.DistanceAlongForwardVector)
                            {
                                var vectorDifference = new Vector3(
                                batchAtIndex.X - camera.X,
                                batchAtIndex.Y - camera.Y,
                                batchAtIndex.Z - camera.Z);

                                float firstDistance;
                                var forwardVector = camera.RotationMatrix.Forward;

                                Vector3.Dot(ref vectorDifference, ref forwardVector, out firstDistance);

                                nextBatchSortValue.PrimarySortValue = -firstDistance;
                            }
                            else
                            {
                                nextBatchSortValue.PrimarySortValue = -(batchAtIndex.Z * batchAtIndex.Z);
                            }
                        }
                    }

                    FixRenderStatesAfterBatchDraw();
                }

                #endregion

                #endregion
            }

            if (section != null)
            {
                // Hop up a level
                if (Section.Context != performDrawingSection)
                {
                    Section.EndContextAndTime();
                }

                Section.EndContextAndTime();
                Section.GetAndStartContextAndTime("End of Draw Mixed");
            }

            // Return the position of any objects not drawn
            if (sortType == SortType.DistanceAlongForwardVector)
            {
                for (int i = indexOfNextSpriteToReposition; i < mVisibleSprites.Count; i++)
                {
                    mVisibleSprites[i].Position = mVisibleSprites[i].mOldPosition;
                }
            }

            Texture = null;
            TextureOnDevice = null;

            if (section != null)
            {
                Section.EndContextAndTime();
            }
        }

        static void FixRenderStatesAfterBatchDraw()
        {
            FlatRedBallServices.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            FlatRedBallServices.GraphicsOptions.TextureFilter = FlatRedBallServices.GraphicsOptions.TextureFilter;
            ForceSetBlendOperation();
            mCurrentEffect = mEffect;
        }

        static void GetNextZValuesByCategory(List<Sprite> spriteList, SortType sortType, List<Text> textList, List<IDrawableBatch> batches,
            Camera camera, ref int spriteIndex, ref int textIndex, ref SortValues nextSpriteSortValue, ref SortValues nextTextSortValue, ref SortValues nextBatchSortValue)
        {
            #region Find out the initial Z values of the 3 categories of objects to know which to render first

            if (sortType == SortType.Z || sortType == SortType.DistanceAlongForwardVector || sortType == SortType.ZSecondaryParentY ||
                // Custom comparers define how objects sort within the category, but outside we just have to rely on Z
                sortType == SortType.CustomComparer)
            {
                lock (spriteList)
                {
                    int spriteNumber = 0;

                    while (spriteNumber < spriteList.Count)
                    {
                        nextSpriteSortValue.PrimarySortValue = spriteList[spriteNumber].Z;
                        nextSpriteSortValue.SecondarySortValue = sortType == SortType.ZSecondaryParentY ? -spriteList[spriteNumber].TopParent.Y : 0;
                        spriteIndex = spriteNumber;
                        break;
                    }
                }

                if (textList != null && textList.Count != 0)
                    nextTextSortValue.PrimarySortValue = textList[0].Position.Z;
                else if (textList != null)
                    textIndex = textList.Count;

                if (batches != null && batches.Count != 0)
                {
                    if (sortType == SortType.Z || sortType == SortType.ZSecondaryParentY || sortType == SortType.CustomComparer)
                    {
                        // The Z value of the current batch is used. Batches are always visible to this code.
                        nextBatchSortValue.PrimarySortValue = batches[0].Z;

                        // Y value is the sorting value, so use that.
                        nextBatchSortValue.SecondarySortValue = sortType == SortType.ZSecondaryParentY ? -batches[0].Y : 0;
                    }
                    else
                    {
                        var vectorDifference = new Vector3(
                            batches[0].X - camera.X,
                            batches[0].Y - camera.Y,
                            batches[0].Z - camera.Z);

                        float firstDistance;
                        var forwardVector = camera.RotationMatrix.Forward;
                        Vector3.Dot(ref vectorDifference, ref forwardVector, out firstDistance);
                        nextBatchSortValue.PrimarySortValue = -firstDistance;
                    }
                }
            }

            else if (sortType == SortType.Texture)
            {
                throw new Exception("Sorting based on texture is not supported on non z-buffered Sprites");
            }

            else if (sortType == SortType.DistanceFromCamera)
            {
                // Code duplication to prevent tight-loop if statements
                lock (spriteList)
                {
                    int spriteNumber = 0;
                    while (spriteNumber < spriteList.Count)
                    {
                        nextSpriteSortValue.PrimarySortValue =
                            -(camera.Position - spriteList[spriteNumber].Position).LengthSquared();
                        spriteIndex = spriteNumber;
                        break;
                    }
                }

                if (textList != null && textList.Count != 0)
                {
                    nextTextSortValue.PrimarySortValue = -(camera.Position - textList[0].Position).LengthSquared();
                }
                else if (textList != null)
                {
                    textIndex = textList.Count;
                }

                // The Z value of the current batch is used. Batches are always visible to this code.
                if (batches != null && batches.Count != 0)
                {
                    // Working with squared length, so use that here.
                    nextBatchSortValue.PrimarySortValue = -(batches[0].Z * batches[0].Z);
                }
            }

            #endregion
        }

        static void SortAllLists(SpriteList spriteList, SortType sortType, PositionedObjectList<Text> textList, List<IDrawableBatch> batches, bool relativeToCamera, Camera camera)
        {
            StoreOldPositionsForDistanceAlongForwardVectorSort(spriteList, sortType, textList, batches, camera);

            #region Sort the sprite list and get the number of visible sprites in numberOfVisibleSprites

            if (spriteList != null && spriteList.Count != 0)
            {
                lock (spriteList)
                {
                    switch (sortType)
                    {
                        case SortType.None:
                            break;

                        case SortType.Z:
                        case SortType.DistanceAlongForwardVector:
                            // Sorting ascending means everything will be drawn back to front. This
                            // is slower but necessary for translucent objects.
                            // Sorting descending means everything will be drawn back to front. This
                            // is faster but will cause problems for translucency.
                            spriteList.SortZInsertionAscending();
                            break;

                        case SortType.DistanceFromCamera:
                            spriteList.SortCameraDistanceInsersionDescending(camera);
                            break;

                        case SortType.ZSecondaryParentY:
                            spriteList.SortZInsertionAscending();
                            spriteList.SortParentYInsertionDescendingOnZBreaks();
                            break;

                        case SortType.CustomComparer:
                            if (mSpriteComparer != null)
                            {
                                spriteList.Sort(mSpriteComparer);
                            }
                            else
                            {
                                spriteList.SortZInsertionAscending();
                            }
                            break;

                        case SortType.Texture:
                            spriteList.SortTextureInsertion();
                            break;

                        default:
                            break;
                    }
                }
            }

            #endregion

            #region Sort the text list

            if (textList != null && textList.Count != 0)
            {
                switch (sortType)
                {
                    case SortType.None:
                        break;

                    case SortType.Z:
                    case SortType.DistanceAlongForwardVector:
                        textList.SortZInsertionAscending();
                        break;

                    case SortType.DistanceFromCamera:
                        textList.SortCameraDistanceInsersionDescending(camera);
                        break;

                    case SortType.CustomComparer:
                        if (mTextComparer != null)
                        {
                            textList.Sort(mTextComparer);
                        }
                        else
                        {
                            textList.SortZInsertionAscending();
                        }
                        break;

                    default:
                        break;
                }
            }

            #endregion

            #region Sort the batches

            if (batches != null && batches.Count != 0)
            {
                switch (sortType)
                {
                    case SortType.None:
                        break;

                    case SortType.Z:
                        // Z serves as the radius if using SortType.DistanceFromCamera.
                        // If Z represents actual Z or radius, the larger the value the
                        // further away from the camera the object will be.
                        SortBatchesZInsertionAscending(batches);
                        break;

                    case SortType.DistanceAlongForwardVector:
                        batches.Sort(new BatchForwardVectorSorter(camera));
                        break;

                    case SortType.ZSecondaryParentY:
                        SortBatchesZInsertionAscending(batches);
                        // Even though the sort type is by parent, IDB doesn't have a Parent object,
                        // so we'll just rely on Y. May need to revisit this if it causes problems.
                        SortBatchesYInsertionDescendingOnZBreaks(batches);
                        break;

                    case SortType.CustomComparer:
                        if (mDrawableBatchComparer != null)
                        {
                            batches.Sort(mDrawableBatchComparer);
                        }
                        else
                        {
                            SortBatchesZInsertionAscending(batches);
                        }
                        break;
                }
            }

            #endregion
        }

        static List<int> batchZBreaks = new List<int>(10);

        static void SortBatchesYInsertionDescendingOnZBreaks(List<IDrawableBatch> batches)
        {
            GetBatchZBreaks(batches, batchZBreaks);

            batchZBreaks.Insert(0, 0);
            batchZBreaks.Add(batches.Count);

            for (int i = 0; i < batchZBreaks.Count - 1; i++)
            {
                SortBatchInsertionDescending(batches, batchZBreaks[i], batchZBreaks[i + 1]);
            }
        }

        static void SortBatchInsertionDescending(List<IDrawableBatch> batches, int firstObject, int lastObjectExclusive)
        {
            int whereObjectBelongs;

            float yAtI;
            float yAtIMinusOne;

            for (int i = firstObject + 1; i < lastObjectExclusive; i++)
            {
                yAtI = batches[i].Y;
                yAtIMinusOne = batches[i - 1].Y;

                if (yAtI > yAtIMinusOne)
                {
                    if (i == firstObject + 1)
                    {
                        batches.Insert(firstObject, batches[i]);
                        batches.RemoveAt(i + 1);
                        continue;
                    }

                    for (whereObjectBelongs = i - 2; whereObjectBelongs > firstObject - 1; whereObjectBelongs--)
                    {
                        if (yAtI <= (batches[whereObjectBelongs]).Y)
                        {
                            batches.Insert(whereObjectBelongs + 1, batches[i]);
                            batches.RemoveAt(i + 1);
                            break;
                        }
                        else if (whereObjectBelongs == firstObject && yAtI > (batches[firstObject]).Y)
                        {
                            batches.Insert(firstObject, batches[i]);
                            batches.RemoveAt(i + 1);
                            break;
                        }
                    }
                }
            }
        }

        static void GetBatchZBreaks(List<IDrawableBatch> batches, List<int> zBreaks)
        {
            zBreaks.Clear();

            if (batches.Count == 0 || batches.Count == 1)
                return;

            for (int i = 1; i < batches.Count; i++)
            {
                if (batches[i].Z != batches[i - 1].Z)
                    zBreaks.Add(i);
            }
        }

        static LayerCameraSettings mOldCameraLayerSettings = new LayerCameraSettings();
        static void DrawLayers(Camera camera, RenderMode renderMode, Section section)
        {
            RenderTarget2D lastRenderTarget = null;
            var lastViewport = GraphicsDevice.Viewport;

            #region Draw world layers

            if (camera.DrawsWorld)
            {
                // These layers are still considered in the "world" because all cameras can see them
                for (int i = 0; i < SpriteManager.LayersWriteable.Count; i++)
                {
                    var layer = SpriteManager.LayersWriteable[i];
                    DrawIndividualLayer(camera, renderMode, layer, section, ref lastRenderTarget, ref lastViewport);
                }
            }

            #endregion

            #region Draw camera layers

            if (camera.DrawsCameraLayer)
            {
                int layerCount = camera.Layers.Count;

                for (int i = 0; i < layerCount; i++)
                {
                    var layer = camera.Layers[i];
                    DrawIndividualLayer(camera, renderMode, layer, section, ref lastRenderTarget, ref lastViewport);
                }
            }

            #endregion

            #region Last, draw the top layer

            if (camera.DrawsWorld && !SpriteManager.TopLayer.IsEmpty)
            {
                var layer = SpriteManager.TopLayer;
                DrawIndividualLayer(camera, renderMode, layer, section, ref lastRenderTarget, ref lastViewport);
            }

            #endregion

            if (lastRenderTarget != null)
            {
                mGraphics.GraphicsDevice.SetRenderTarget(null);
            }
        }

        static void DrawUnlayeredObjects(Camera camera, RenderMode renderMode, Section section)
        {
            CurrentLayer = null;

            if (section != null)
            {
                Section.GetAndStartContextAndTime("Draw above shapes");
            }

            #region Draw shapes if UnderEverything

            if (camera.DrawsWorld && renderMode == RenderMode.Default && camera.DrawsShapes &&
                ShapeManager.ShapeDrawingOrder == ShapeDrawingOrder.UnderEverything)
            {
                DrawShapes(
                    camera,
                    ShapeManager.mSpheres,
                    ShapeManager.mCubes,
                    ShapeManager.mRectangles,
                    ShapeManager.mCircles,
                    ShapeManager.mPolygons,
                    ShapeManager.mLines,
                    ShapeManager.mCapsule2Ds,
                    null
                    );
            }

            #endregion

            #region Draw Z-buffered sprites and mixed

            // Only draw the rest if in default rendering mode
            if (renderMode == RenderMode.Default)
            {
                if (camera.DrawsWorld)
                {
                    if (section != null)
                    {
                        Section.EndContextAndTime();
                        Section.GetAndStartContextAndTime("Draw Z Buffered Sprites");
                    }

                    if (SpriteManager.ZBufferedSpritesWriteable.Count != 0)
                    {
                        // Draw the Z-buffered sprites
                        DrawZBufferedSprites(camera, SpriteManager.ZBufferedSpritesWriteable);
                    }

                    foreach (var drawableBatch in SpriteManager.mZBufferedDrawableBatches)
                    {
                        drawableBatch.Draw(camera);
                    }

                    if (section != null)
                    {
                        Section.EndContextAndTime();
                        Section.GetAndStartContextAndTime("Draw Ordered objects");
                    }

                    // Draw the OrderedByDistanceFromCamera objects (Sprites, Texts, DrawableBatches)
                    DrawOrderedByDistanceFromCamera(camera, section);
                }
            }

            #endregion

            if (section != null)
            {
                Section.EndContextAndTime();
                Section.GetAndStartContextAndTime("Draw below shapes");
            }

            #region Draw shapes if OverEverything

            if (camera.DrawsWorld &&
                renderMode == RenderMode.Default &&
                camera.DrawsShapes &&
                ShapeManager.ShapeDrawingOrder == ShapeDrawingOrder.OverEverything)
            {
                DrawShapes(
                    camera,
                    ShapeManager.mSpheres,
                    ShapeManager.mCubes,
                    ShapeManager.mRectangles,
                    ShapeManager.mCircles,
                    ShapeManager.mPolygons,
                    ShapeManager.mLines,
                    ShapeManager.mCapsule2Ds,
                    null
                    );
            }

            #endregion

            if (section != null)
            {
                Section.EndContextAndTime();
            }
        }

        public static void DrawCamera(Camera camera, Section section)
        {
            DrawCamera(camera, RenderMode.Default, section);
        }

        static void DrawCamera(Camera camera, RenderMode renderMode, Section section)
        {
            if (section != null)
            {
                Section.GetAndStartContextAndTime("Start of camera draw");
            }

            PrepareForDrawScene(camera, renderMode);

            if (section != null)
            {
                Section.EndContextAndTime();
                Section.GetAndStartContextAndTime("Draw UnderAllLayer");
            }

            if (camera.DrawsWorld && !SpriteManager.UnderAllDrawnLayer.IsEmpty)
            {
                var layer = SpriteManager.UnderAllDrawnLayer;
                RenderTarget2D lastRenderTarget = null;
                var lastViewport = GraphicsDevice.Viewport;
                DrawIndividualLayer(camera, RenderMode.Default, layer, section, ref lastRenderTarget, ref lastViewport);

                if (lastRenderTarget != null)
                {
                    GraphicsDevice.SetRenderTarget(null);
                }
            }

            if (section != null)
            {
                Section.EndContextAndTime();
                Section.GetAndStartContextAndTime("Draw Unlayered");
            }

            DrawUnlayeredObjects(camera, renderMode, section);

            if (section != null)
            {
                Section.EndContextAndTime();
                Section.GetAndStartContextAndTime("Draw Regular Layers");
            }

            // Draw layers. This method will check internally for
            // the camera's DrawsWorld and DrawsCameraLayers properties.
            DrawLayers(camera, renderMode, section);

            if (section != null)
            {
                Section.EndContextAndTime();
            }
        }

        static void StoreOldPositionsForDistanceAlongForwardVectorSort(PositionedObjectList<Sprite> spriteList, SortType sortType, PositionedObjectList<Text> textList, List<IDrawableBatch> batches, Camera camera)
        {
            #region If DistanceAlongForwardVector store old values

            // If the objects are using SortType.DistanceAlongForwardVector
            // then store the old positions, then rotate the objects by the matrix that
            // moves the forward vector to the Z = -1 vector (the camera's inverse rotation
            // matrix)
            if (sortType == SortType.DistanceAlongForwardVector)
            {
                var inverseRotationMatrix = camera.RotationMatrix;
                Matrix.Invert(ref inverseRotationMatrix, out inverseRotationMatrix);

                int temporaryCount = spriteList.Count;

                for (int i = 0; i < temporaryCount; i++)
                {
                    spriteList[i].mOldPosition = spriteList[i].Position;

                    spriteList[i].Position -= camera.Position;
                    Vector3.Transform(ref spriteList[i].Position,
                        ref inverseRotationMatrix, out spriteList[i].Position);
                }

                temporaryCount = textList.Count;

                for (int i = 0; i < temporaryCount; i++)
                {
                    textList[i].mOldPosition = textList[i].Position;

                    textList[i].Position -= camera.Position;
                    Vector3.Transform(ref textList[i].Position,
                        ref inverseRotationMatrix, out textList[i].Position);
                }

                temporaryCount = batches.Count;
            }

            #endregion
        }

        static void DrawOrderedByDistanceFromCamera(Camera camera, Section section)
        {
            if (SpriteManager.OrderedByDistanceFromCameraSprites.Count != 0 ||
                SpriteManager.WritableDrawableBatchesList.Count != 0 ||
                TextManager.mDrawnTexts.Count != 0)
            {
                CurrentLayer = null;

                DrawMixed(
                    SpriteManager.OrderedByDistanceFromCameraSprites,
                    SpriteManager.OrderedSortType, TextManager.mDrawnTexts,
                    SpriteManager.WritableDrawableBatchesList,
                    false, camera, section);
            }
        }

        /// <summary>
        /// Creates a SwapChain instance matching the game's resolution which automatically adjusts when the game window resizes.
        /// </summary>
        public static void CreateDefaultSwapChain()
        {
            SwapChain = new PostProcessing.SwapChain(
                FlatRedBallServices.Game.Window.ClientBounds.Width,
                FlatRedBallServices.Game.Window.ClientBounds.Height);

            FlatRedBallServices.GraphicsOptions.SizeOrOrientationChanged += (not, used) =>
            {
                SwapChain.UpdateRenderTargetSize(
                    FlatRedBallServices.Game.Window.ClientBounds.Width,
                    FlatRedBallServices.Game.Window.ClientBounds.Height);
            };
        }
    }
}
