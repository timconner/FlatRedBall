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
        public override ErrorViewModel[]? GetAllErrors()
        {
            List<ErrorViewModel> errors = new List<ErrorViewModel>();
            foreach (var element in ObjectFinder.Self.GetAllElements())
            {
                foreach (var namedObject in element.AllNamedObjects)
                {
                    if (SoundEffectInstantiationErrorViewModel.GetIfHasError(element, namedObject))
                    {
                        errors = errors ?? new List<ErrorViewModel>();
                        errors.Add(new SoundEffectInstantiationErrorViewModel(element, namedObject));
                    }
                }
            }

            return errors?.ToArray();
        }
    }
}
