using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall.Glue.Elements;
using FlatRedBall.Glue.Errors;
using FlatRedBall.Glue.SaveClasses;

namespace OfficialPlugins.SoundEffectPlugin.Errors
{
    class SoundEffectErrorReporter : ErrorReporterBase
    {
        public override ErrorViewModel[] GetAllErrors()
        {
            foreach(var namedObject in ObjectFinder.Self.GetAllNamedObjects())
            {
                if (namedObject.SourceType == FlatRedBall.Glue.SaveClasses.SourceType.FlatRedBallType && namedObject.Instantiate && namedObject.IsDisabled == false)
                {
                    var ati = namedObject.GetAssetTypeInfo();
                    if(ati == )
                    var extension = FileManager.GetExtension(namedObject.SourceFile);
                    if (extension == "wav" || extension == "mp3")
                    {
                        return new ErrorViewModel[]
                        {
                            new ErrorViewModel
                            {
                                Message = "SoundEffect files should not be used directly. Instead, use the SoundEffect plugin to create SoundEffect objects.",
                                ErrorType = ErrorType.Error,
                                RelatedObject = namedObject
                            }
                        };
                    }
                }
            }
        }
}
