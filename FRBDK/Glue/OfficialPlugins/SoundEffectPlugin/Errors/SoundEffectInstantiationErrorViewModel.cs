using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall.Glue.Errors;
using FlatRedBall.Glue.Plugins.ExportedImplementations;
using FlatRedBall.Glue.SaveClasses;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace OfficialPlugins.SoundEffectPlugin.Errors
{

    internal class SoundEffectInstantiationErrorViewModel : ErrorViewModel
    {
        public override string UniqueId => Details;

        NamedObjectSave _namedObjectSave;
        GlueElement _glueElement;

        public SoundEffectInstantiationErrorViewModel(GlueElement glueElement, NamedObjectSave namedObjectSave)
        {
            _namedObjectSave = namedObjectSave;
            _glueElement = glueElement;
            Details = $"{namedObjectSave} cannot be directly instantiated - it must either be set from file, or must have its SetByDerived set to true";
        }

        public override bool GetIfIsFixed() => GetIfHasError(_glueElement, _namedObjectSave) == false;

        public override void HandleDoubleClick() => GlueState.Self.CurrentNamedObjectSave = _namedObjectSave;

        public static bool GetIfHasError(GlueElement glueElement, NamedObjectSave namedObject)
        {
            if (namedObject.SourceType == FlatRedBall.Glue.SaveClasses.SourceType.FlatRedBallType && namedObject.Instantiate && namedObject.IsDisabled == false)
            {
                var ati = namedObject.GetAssetTypeInfo();
                if (ati.QualifiedRuntimeTypeName.QualifiedType == "Microsoft.Xna.Framework.Audio.SoundEffectInstance")
                {
                    return true;
                }
            }
            return false;
        }
    }
}
