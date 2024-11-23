using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall.Glue.Errors;
using FlatRedBall.Glue.Plugins.ExportedImplementations;
using FlatRedBall.Glue.Plugins.ExportedInterfaces;

namespace OfficialPlugins.GlueProjectFileVersionPlugin.Errors;

internal class GlueProjectFileVersionErrorReporter : ErrorReporterBase
{
    private readonly IGlueState _glueState;

    public GlueProjectFileVersionErrorReporter(IGlueState glueState)
    {
        _glueState = glueState;
    }

    public override ErrorViewModel[]? GetAllErrors()
    {
        if(GlueProjectFileVersionErrorViewModel.GetIfHasError(_glueState.CurrentGlueProject, GlueState.Self))
        {
            return new ErrorViewModel [] { new GlueProjectFileVersionErrorViewModel(GlueState.Self.CurrentGlueProject, GlueState.Self) };
        }
        return null;
    }
}
