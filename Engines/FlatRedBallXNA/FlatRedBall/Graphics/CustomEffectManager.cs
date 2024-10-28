using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlatRedBall.Graphics
{
    public class CustomEffectManager
    {
        Effect _effect;

        // Cached effect members to avoid list lookups while rendering
        public EffectParameter ParameterCurrentTexture;
        public EffectParameter ParameterViewProj;
        public EffectParameter ParameterColorModifier;

        bool _effectHasNewformat;

        EffectTechnique _techniqueTexture;
        EffectTechnique _techniqueAdd;
        EffectTechnique _techniqueSubtract;
        EffectTechnique _techniqueModulate;
        EffectTechnique _techniqueModulate2X;
        EffectTechnique _techniqueModulate4X;
        EffectTechnique _techniqueInverseTexture;
        EffectTechnique _techniqueColor;
        EffectTechnique _techniqueColorTextureAlpha;
        EffectTechnique _techniqueInterpolateColor;

        EffectTechnique _techniqueTexture_CM;
        EffectTechnique _techniqueAdd_CM;
        EffectTechnique _techniqueSubtract_CM;
        EffectTechnique _techniqueModulate_CM;
        EffectTechnique _techniqueModulate2X_CM;
        EffectTechnique _techniqueModulate4X_CM;
        EffectTechnique _techniqueInverseTexture_CM;
        EffectTechnique _techniqueColor_CM;
        EffectTechnique _techniqueColorTextureAlpha_CM;
        EffectTechnique _techniqueInterpolateColor_CM;

        EffectTechnique _techniqueTexture_LN;
        EffectTechnique _techniqueAdd_LN;
        EffectTechnique _techniqueSubtract_LN;
        EffectTechnique _techniqueModulate_LN;
        EffectTechnique _techniqueModulate2X_LN;
        EffectTechnique _techniqueModulate4X_LN;
        EffectTechnique _techniqueInverseTexture_LN;
        EffectTechnique _techniqueColor_LN;
        EffectTechnique _techniqueColorTextureAlpha_LN;
        EffectTechnique _techniqueInterpolateColor_LN;

        EffectTechnique _techniqueTexture_LN_CM;
        EffectTechnique _techniqueAdd_LN_CM;
        EffectTechnique _techniqueSubtract_LN_CM;
        EffectTechnique _techniqueModulate_LN_CM;
        EffectTechnique _techniqueModulate2X_LN_CM;
        EffectTechnique _techniqueModulate4X_LN_CM;
        EffectTechnique _techniqueInverseTexture_LN_CM;
        EffectTechnique _techniqueColor_LN_CM;
        EffectTechnique _techniqueColorTextureAlpha_LN_CM;
        EffectTechnique _techniqueInterpolateColor_LN_CM;

        EffectTechnique _techniqueTexture_Linear;
        EffectTechnique _techniqueAdd_Linear;
        EffectTechnique _techniqueSubtract_Linear;
        EffectTechnique _techniqueModulate_Linear;
        EffectTechnique _techniqueModulate2X_Linear;
        EffectTechnique _techniqueModulate4X_Linear;
        EffectTechnique _techniqueInverseTexture_Linear;
        EffectTechnique _techniqueColor_Linear;
        EffectTechnique _techniqueColorTextureAlpha_Linear;
        EffectTechnique _techniqueInterpolateColor_Linear;

        EffectTechnique _techniqueTexture_Linear_CM;
        EffectTechnique _techniqueAdd_Linear_CM;
        EffectTechnique _techniqueSubtract_Linear_CM;
        EffectTechnique _techniqueModulate_Linear_CM;
        EffectTechnique _techniqueModulate2X_Linear_CM;
        EffectTechnique _techniqueModulate4X_Linear_CM;
        EffectTechnique _techniqueInverseTexture_Linear_CM;
        EffectTechnique _techniqueColor_Linear_CM;
        EffectTechnique _techniqueColorTextureAlpha_Linear_CM;
        EffectTechnique _techniqueInterpolateColor_Linear_CM;

        EffectTechnique _techniqueTexture_Linear_LN;
        EffectTechnique _techniqueAdd_Linear_LN;
        EffectTechnique _techniqueSubtract_Linear_LN;
        EffectTechnique _techniqueModulate_Linear_LN;
        EffectTechnique _techniqueModulate2X_Linear_LN;
        EffectTechnique _techniqueModulate4X_Linear_LN;
        EffectTechnique _techniqueInverseTexture_Linear_LN;
        EffectTechnique _techniqueColor_Linear_LN;
        EffectTechnique _techniqueColorTextureAlpha_Linear_LN;
        EffectTechnique _techniqueInterpolateColor_Linear_LN;

        EffectTechnique _techniqueTexture_Linear_LN_CM;
        EffectTechnique _techniqueAdd_Linear_LN_CM;
        EffectTechnique _techniqueSubtract_Linear_LN_CM;
        EffectTechnique _techniqueModulate_Linear_LN_CM;
        EffectTechnique _techniqueModulate2X_Linear_LN_CM;
        EffectTechnique _techniqueModulate4X_Linear_LN_CM;
        EffectTechnique _techniqueInverseTexture_Linear_LN_CM;
        EffectTechnique _techniqueColor_Linear_LN_CM;
        EffectTechnique _techniqueColorTextureAlpha_Linear_LN_CM;
        EffectTechnique _techniqueInterpolateColor_Linear_LN_CM;

        public Effect Effect
        {
            get { return _effect; }
            set
            {
                _effect = value;

                ParameterViewProj = _effect.Parameters["ViewProj"];
                ParameterCurrentTexture = _effect.Parameters["CurrentTexture"];
                try { ParameterColorModifier = _effect.Parameters["ColorModifier"]; } catch { }

                // Let's check if the shader has the new format (which includes
                // separate versions of techniques for Point and Linear filtering).
                // We try to cache the first technique in order to do so.
                try { _techniqueTexture = _effect.Techniques["Texture_Point"]; } catch { }

                if (_techniqueTexture != null)
                {
                    _effectHasNewformat = true;

                    //try { mTechniqueTexture = mEffect.Techniques["Texture_Point"]; } catch { } // This has been already cached
                    try { _techniqueAdd = _effect.Techniques["Add_Point"]; } catch { }
                    try { _techniqueSubtract = _effect.Techniques["Subtract_Point"]; } catch { }
                    try { _techniqueModulate = _effect.Techniques["Modulate_Point"]; } catch { }
                    try { _techniqueModulate2X = _effect.Techniques["Modulate2X_Point"]; } catch { }
                    try { _techniqueModulate4X = _effect.Techniques["Modulate4X_Point"]; } catch { }
                    try { _techniqueInverseTexture = _effect.Techniques["InverseTexture_Point"]; } catch { }
                    try { _techniqueColor = _effect.Techniques["Color_Point"]; } catch { }
                    try { _techniqueColorTextureAlpha = _effect.Techniques["ColorTextureAlpha_Point"]; } catch { }
                    try { _techniqueInterpolateColor = _effect.Techniques["InterpolateColor_Point"]; } catch { }

                    try { _techniqueTexture_CM = _effect.Techniques["Texture_Point_CM"]; } catch { }
                    try { _techniqueAdd_CM = _effect.Techniques["Add_Point_CM"]; } catch { }
                    try { _techniqueSubtract_CM = _effect.Techniques["Subtract_Point_CM"]; } catch { }
                    try { _techniqueModulate_CM = _effect.Techniques["Modulate_Point_CM"]; } catch { }
                    try { _techniqueModulate2X_CM = _effect.Techniques["Modulate2X_Point_CM"]; } catch { }
                    try { _techniqueModulate4X_CM = _effect.Techniques["Modulate4X_Point_CM"]; } catch { }
                    try { _techniqueInverseTexture_CM = _effect.Techniques["InverseTexture_Point_CM"]; } catch { }
                    try { _techniqueColor_CM = _effect.Techniques["Color_Point_CM"]; } catch { }
                    try { _techniqueColorTextureAlpha_CM = _effect.Techniques["ColorTextureAlpha_Point_CM"]; } catch { }
                    try { _techniqueInterpolateColor_CM = _effect.Techniques["InterpolateColor_Point_CM"]; } catch { }

                    try { _techniqueTexture_LN = _effect.Techniques["Texture_Point_LN"]; } catch { }
                    try { _techniqueAdd_LN = _effect.Techniques["Add_Point_LN"]; } catch { }
                    try { _techniqueSubtract_LN = _effect.Techniques["Subtract_Point_LN"]; } catch { }
                    try { _techniqueModulate_LN = _effect.Techniques["Modulate_Point_LN"]; } catch { }
                    try { _techniqueModulate2X_LN = _effect.Techniques["Modulate2X_Point_LN"]; } catch { }
                    try { _techniqueModulate4X_LN = _effect.Techniques["Modulate4X_Point_LN"]; } catch { }
                    try { _techniqueInverseTexture_LN = _effect.Techniques["InverseTexture_Point_LN"]; } catch { }
                    try { _techniqueColor_LN = _effect.Techniques["Color_Point_LN"]; } catch { }
                    try { _techniqueColorTextureAlpha_LN = _effect.Techniques["ColorTextureAlpha_Point_LN"]; } catch { }
                    try { _techniqueInterpolateColor_LN = _effect.Techniques["InterpolateColor_Point_LN"]; } catch { }

                    try { _techniqueTexture_LN_CM = _effect.Techniques["Texture_Point_LN_CM"]; } catch { }
                    try { _techniqueAdd_LN_CM = _effect.Techniques["Add_Point_LN_CM"]; } catch { }
                    try { _techniqueSubtract_LN_CM = _effect.Techniques["Subtract_Point_LN_CM"]; } catch { }
                    try { _techniqueModulate_LN_CM = _effect.Techniques["Modulate_Point_LN_CM"]; } catch { }
                    try { _techniqueModulate2X_LN_CM = _effect.Techniques["Modulate2X_Point_LN_CM"]; } catch { }
                    try { _techniqueModulate4X_LN_CM = _effect.Techniques["Modulate4X_Point_LN_CM"]; } catch { }
                    try { _techniqueInverseTexture_LN_CM = _effect.Techniques["InverseTexture_Point_LN_CM"]; } catch { }
                    try { _techniqueColor_LN_CM = _effect.Techniques["Color_Point_LN_CM"]; } catch { }
                    try { _techniqueColorTextureAlpha_LN_CM = _effect.Techniques["ColorTextureAlpha_Point_LN_CM"]; } catch { }
                    try { _techniqueInterpolateColor_LN_CM = _effect.Techniques["InterpolateColor_Point_LN_CM"]; } catch { }

                    try { _techniqueTexture_Linear = _effect.Techniques["Texture_Linear"]; } catch { }
                    try { _techniqueAdd_Linear = _effect.Techniques["Add_Linear"]; } catch { }
                    try { _techniqueSubtract_Linear = _effect.Techniques["Subtract_Linear"]; } catch { }
                    try { _techniqueModulate_Linear = _effect.Techniques["Modulate_Linear"]; } catch { }
                    try { _techniqueModulate2X_Linear = _effect.Techniques["Modulate2X_Linear"]; } catch { }
                    try { _techniqueModulate4X_Linear = _effect.Techniques["Modulate4X_Linear"]; } catch { }
                    try { _techniqueInverseTexture_Linear = _effect.Techniques["InverseTexture_Linear"]; } catch { }
                    try { _techniqueColor_Linear = _effect.Techniques["Color_Linear"]; } catch { }
                    try { _techniqueColorTextureAlpha_Linear = _effect.Techniques["ColorTextureAlpha_Linear"]; } catch { }
                    try { _techniqueInterpolateColor_Linear = _effect.Techniques["InterpolateColor_Linear"]; } catch { }

                    try { _techniqueTexture_Linear_CM = _effect.Techniques["Texture_Linear_CM"]; } catch { }
                    try { _techniqueAdd_Linear_CM = _effect.Techniques["Add_Linear_CM"]; } catch { }
                    try { _techniqueSubtract_Linear_CM = _effect.Techniques["Subtract_Linear_CM"]; } catch { }
                    try { _techniqueModulate_Linear_CM = _effect.Techniques["Modulate_Linear_CM"]; } catch { }
                    try { _techniqueModulate2X_Linear_CM = _effect.Techniques["Modulate2X_Linear_CM"]; } catch { }
                    try { _techniqueModulate4X_Linear_CM = _effect.Techniques["Modulate4X_Linear_CM"]; } catch { }
                    try { _techniqueInverseTexture_Linear_CM = _effect.Techniques["InverseTexture_Linear_CM"]; } catch { }
                    try { _techniqueColor_Linear_CM = _effect.Techniques["Color_Linear_CM"]; } catch { }
                    try { _techniqueColorTextureAlpha_Linear_CM = _effect.Techniques["ColorTextureAlpha_Linear_CM"]; } catch { }
                    try { _techniqueInterpolateColor_Linear_CM = _effect.Techniques["InterpolateColor_Linear_CM"]; } catch { }

                    try { _techniqueTexture_Linear_LN = _effect.Techniques["Texture_Linear_LN"]; } catch { }
                    try { _techniqueAdd_Linear_LN = _effect.Techniques["Add_Linear_LN"]; } catch { }
                    try { _techniqueSubtract_Linear_LN = _effect.Techniques["Subtract_Linear_LN"]; } catch { }
                    try { _techniqueModulate_Linear_LN = _effect.Techniques["Modulate_Linear_LN"]; } catch { }
                    try { _techniqueModulate2X_Linear_LN = _effect.Techniques["Modulate2X_Linear_LN"]; } catch { }
                    try { _techniqueModulate4X_Linear_LN = _effect.Techniques["Modulate4X_Linear_LN"]; } catch { }
                    try { _techniqueInverseTexture_Linear_LN = _effect.Techniques["InverseTexture_Linear_LN"]; } catch { }
                    try { _techniqueColor_Linear_LN = _effect.Techniques["Color_Linear_LN"]; } catch { }
                    try { _techniqueColorTextureAlpha_Linear_LN = _effect.Techniques["ColorTextureAlpha_Linear_LN"]; } catch { }
                    try { _techniqueInterpolateColor_Linear_LN = _effect.Techniques["InterpolateColor_Linear_LN"]; } catch { }

                    try { _techniqueTexture_Linear_LN_CM = _effect.Techniques["Texture_Linear_LN_CM"]; } catch { }
                    try { _techniqueAdd_Linear_LN_CM = _effect.Techniques["Add_Linear_LN_CM"]; } catch { }
                    try { _techniqueSubtract_Linear_LN_CM = _effect.Techniques["Subtract_Linear_LN_CM"]; } catch { }
                    try { _techniqueModulate_Linear_LN_CM = _effect.Techniques["Modulate_Linear_LN_CM"]; } catch { }
                    try { _techniqueModulate2X_Linear_LN_CM = _effect.Techniques["Modulate2X_Linear_LN_CM"]; } catch { }
                    try { _techniqueModulate4X_Linear_LN_CM = _effect.Techniques["Modulate4X_Linear_LN_CM"]; } catch { }
                    try { _techniqueInverseTexture_Linear_LN_CM = _effect.Techniques["InverseTexture_Linear_LN_CM"]; } catch { }
                    try { _techniqueColor_Linear_LN_CM = _effect.Techniques["Color_Linear_LN_CM"]; } catch { }
                    try { _techniqueColorTextureAlpha_Linear_LN_CM = _effect.Techniques["ColorTextureAlpha_Linear_LN_CM"]; } catch { }
                    try { _techniqueInterpolateColor_Linear_LN_CM = _effect.Techniques["InterpolateColor_Linear_LN_CM"]; } catch { }
                }
                else
                {
                    _effectHasNewformat = false;

                    try { _techniqueTexture = _effect.Techniques["Texture"]; } catch { }
                    try { _techniqueAdd = _effect.Techniques["Add"]; } catch { }
                    try { _techniqueSubtract = _effect.Techniques["Subtract"]; } catch { }
                    try { _techniqueModulate = _effect.Techniques["Modulate"]; } catch { }
                    try { _techniqueModulate2X = _effect.Techniques["Modulate2X"]; } catch { }
                    try { _techniqueModulate4X = _effect.Techniques["Modulate4X"]; } catch { }
                    try { _techniqueInverseTexture = _effect.Techniques["InverseTexture"]; } catch { }
                    try { _techniqueColor = _effect.Techniques["Color"]; } catch { }
                    try { _techniqueColorTextureAlpha = _effect.Techniques["ColorTextureAlpha"]; } catch { }
                    try { _techniqueInterpolateColor = _effect.Techniques["InterpolateColor"]; } catch { }
                }
            }
        }

        static EffectTechnique GetTechniqueVariant(bool useDefaultOrPointFilter, EffectTechnique point, EffectTechnique pointLinearized, EffectTechnique linear, EffectTechnique linearLinearized)
        {
            return useDefaultOrPointFilter ?
                (Renderer.LinearizeTextures ? pointLinearized : point) :
                (Renderer.LinearizeTextures ? linearLinearized : linear);
        }

        public EffectTechnique GetVertexColorTechniqueFromColorOperation(ColorOperation value, bool? useDefaultOrPointFilter = null)
        {
            if (_effect == null)
                throw new Exception("The effect hasn't been set.");

            EffectTechnique technique = null;

            bool useDefaultOrPointFilterInternal;

            if (_effectHasNewformat)
            {
                // If the shader has the new format both point and linear are available
                if (!useDefaultOrPointFilter.HasValue)
                {
                    // Filter not specified, so we get the filter from options
                    useDefaultOrPointFilterInternal = FlatRedBallServices.GraphicsOptions.TextureFilter == TextureFilter.Point;
                }
                else
                {
                    // Filter specified
                    useDefaultOrPointFilterInternal = useDefaultOrPointFilter.Value;
                }
            }
            else
            {
                // If the shader doesn't have the new format only one version of
                // the techniques are available, probably using point filtering.
                useDefaultOrPointFilterInternal = true;
            }

            switch (value)
            {
                case ColorOperation.Texture:
                    technique = GetTechniqueVariant(
                    useDefaultOrPointFilterInternal, _techniqueTexture, _techniqueTexture_LN, _techniqueTexture_Linear, _techniqueTexture_Linear_LN); break;

                case ColorOperation.Add:
                    technique = GetTechniqueVariant(
                    useDefaultOrPointFilterInternal, _techniqueAdd, _techniqueAdd_LN, _techniqueAdd_Linear, _techniqueAdd_Linear_LN); break;

                case ColorOperation.Subtract:
                    technique = GetTechniqueVariant(
                    useDefaultOrPointFilterInternal, _techniqueSubtract, _techniqueSubtract_LN, _techniqueSubtract_Linear, _techniqueSubtract_Linear_LN); break;

                case ColorOperation.Modulate:
                    technique = GetTechniqueVariant(
                    useDefaultOrPointFilterInternal, _techniqueModulate, _techniqueModulate_LN, _techniqueModulate_Linear, _techniqueModulate_Linear_LN); break;

                case ColorOperation.Modulate2X:
                    technique = GetTechniqueVariant(
                    useDefaultOrPointFilterInternal, _techniqueModulate2X, _techniqueModulate2X_LN, _techniqueModulate2X_Linear, _techniqueModulate2X_Linear_LN); break;

                case ColorOperation.Modulate4X:
                    technique = GetTechniqueVariant(
                    useDefaultOrPointFilterInternal, _techniqueModulate4X, _techniqueModulate4X_LN, _techniqueModulate4X_Linear, _techniqueModulate4X_Linear_LN); break;

                case ColorOperation.InverseTexture:
                    technique = GetTechniqueVariant(
                    useDefaultOrPointFilterInternal, _techniqueInverseTexture, _techniqueInverseTexture_LN, _techniqueInverseTexture_Linear, _techniqueInverseTexture_Linear_LN); break;

                case ColorOperation.Color:
                    technique = GetTechniqueVariant(
                    useDefaultOrPointFilterInternal, _techniqueColor, _techniqueColor_LN, _techniqueColor_Linear, _techniqueColor_Linear_LN); break;

                case ColorOperation.ColorTextureAlpha:
                    technique = GetTechniqueVariant(
                    useDefaultOrPointFilterInternal, _techniqueColorTextureAlpha, _techniqueColorTextureAlpha_LN, _techniqueColorTextureAlpha_Linear, _techniqueColorTextureAlpha_Linear_LN); break;

                case ColorOperation.InterpolateColor:
                    technique = GetTechniqueVariant(
                    useDefaultOrPointFilterInternal, _techniqueInterpolateColor, _techniqueInterpolateColor_LN, _techniqueInterpolateColor_Linear, _techniqueInterpolateColor_Linear_LN); break;

                default: throw new InvalidOperationException();
            }

            return technique;
        }

        public EffectTechnique GetColorModifierTechniqueFromColorOperation(ColorOperation value, bool? useDefaultOrPointFilter = null)
        {
            if (_effect == null)
                throw new Exception("The effect hasn't been set.");

            EffectTechnique technique = null;

            bool useDefaultOrPointFilterInternal;

            if (_effectHasNewformat)
            {
                // If the shader has the new format both point and linear are available
                if (!useDefaultOrPointFilter.HasValue)
                {
                    // Filter not specified, so we get the filter from options
                    useDefaultOrPointFilterInternal = FlatRedBallServices.GraphicsOptions.TextureFilter == TextureFilter.Point;
                }
                else
                {
                    // Filter specified
                    useDefaultOrPointFilterInternal = useDefaultOrPointFilter.Value;
                }
            }
            else
            {
                // If the shader doesn't have the new format only one version of
                // the techniques are available, probably using point filtering.
                useDefaultOrPointFilterInternal = true;
            }

            switch (value)
            {
                case ColorOperation.Texture:
                    technique = GetTechniqueVariant(
                    useDefaultOrPointFilterInternal, _techniqueTexture_CM, _techniqueTexture_LN_CM, _techniqueTexture_Linear_CM, _techniqueTexture_Linear_LN_CM); break;

                case ColorOperation.Add:
                    technique = GetTechniqueVariant(
                    useDefaultOrPointFilterInternal, _techniqueAdd_CM, _techniqueAdd_LN_CM, _techniqueAdd_Linear_CM, _techniqueAdd_Linear_LN_CM); break;

                case ColorOperation.Subtract:
                    technique = GetTechniqueVariant(
                    useDefaultOrPointFilterInternal, _techniqueSubtract_CM, _techniqueSubtract_LN_CM, _techniqueSubtract_Linear_CM, _techniqueSubtract_Linear_LN_CM); break;

                case ColorOperation.Modulate:
                    technique = GetTechniqueVariant(
                    useDefaultOrPointFilterInternal, _techniqueModulate_CM, _techniqueModulate_LN_CM, _techniqueModulate_Linear_CM, _techniqueModulate_Linear_LN_CM); break;

                case ColorOperation.Modulate2X:
                    technique = GetTechniqueVariant(
                    useDefaultOrPointFilterInternal, _techniqueModulate2X_CM, _techniqueModulate2X_LN_CM, _techniqueModulate2X_Linear_CM, _techniqueModulate2X_Linear_LN_CM); break;

                case ColorOperation.Modulate4X:
                    technique = GetTechniqueVariant(
                    useDefaultOrPointFilterInternal, _techniqueModulate4X_CM, _techniqueModulate4X_LN_CM, _techniqueModulate4X_Linear_CM, _techniqueModulate4X_Linear_LN_CM); break;

                case ColorOperation.InverseTexture:
                    technique = GetTechniqueVariant(
                    useDefaultOrPointFilterInternal, _techniqueInverseTexture_CM, _techniqueInverseTexture_LN_CM, _techniqueInverseTexture_Linear_CM, _techniqueInverseTexture_Linear_LN_CM); break;

                case ColorOperation.Color:
                    technique = GetTechniqueVariant(
                    useDefaultOrPointFilterInternal, _techniqueColor_CM, _techniqueColor_LN_CM, _techniqueColor_Linear_CM, _techniqueColor_Linear_LN_CM); break;

                case ColorOperation.ColorTextureAlpha:
                    technique = GetTechniqueVariant(
                    useDefaultOrPointFilterInternal, _techniqueColorTextureAlpha_CM, _techniqueColorTextureAlpha_LN_CM, _techniqueColorTextureAlpha_Linear_CM, _techniqueColorTextureAlpha_Linear_LN_CM); break;

                case ColorOperation.InterpolateColor:
                    technique = GetTechniqueVariant(
                    useDefaultOrPointFilterInternal, _techniqueInterpolateColor_CM, _techniqueInterpolateColor_LN_CM, _techniqueInterpolateColor_Linear_CM, _techniqueInterpolateColor_Linear_LN_CM); break;

                default: throw new InvalidOperationException();
            }

            return technique;
        }

        public static Vector4 ProcessColorForColorOperation(ColorOperation colorOperation, Vector4 input)
        {
            if (colorOperation == ColorOperation.Color)
            {
                return new Vector4(input.X * input.W, input.Y * input.W, input.Z * input.W, input.W);
            }
            else if (colorOperation == ColorOperation.Texture)
            {
                return new Vector4(input.W, input.W, input.W, input.W);
            }
            else
            {
                return new Vector4(input.X, input.Y, input.Z, input.W);
            }
        }
    }
}
