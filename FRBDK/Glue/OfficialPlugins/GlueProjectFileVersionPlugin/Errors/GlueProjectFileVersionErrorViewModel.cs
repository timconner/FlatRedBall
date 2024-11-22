using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall.Glue.Errors;
using FlatRedBall.Glue.Plugins.ExportedInterfaces;
using FlatRedBall.Glue.SaveClasses;
using FlatRedBall.Glue.VSHelpers.Projects;

namespace OfficialPlugins.GlueProjectFileVersionPlugin.Errors;

class GlueProjectFileVersionErrorViewModel : ErrorViewModel
{
    private readonly GlueProjectSave _glueProjectSave;
    private readonly IGlueState _glueState;

    public override string UniqueId => Details;

    public GlueProjectFileVersionErrorViewModel(GlueProjectSave glueProjectSave, IGlueState glueState)
    {
        _glueProjectSave = glueProjectSave;
        _glueState = glueState;

        var fileVersion = glueProjectSave.FileVersion;
        this.Details = $"The FlatRedBall (.gluj) project has a FileVersion of {fileVersion} but the engine syntax only supports version {glueState.EngineDllSyntaxVersion}\n" +
            $"To solve this problem, upgrade the engine, link it to source, or downgrade the FileVersion in the .gluj file.";
    }

    public override bool GetIfIsFixed()
    {
        return GetIfHasError(_glueProjectSave, _glueState) == false;
    }

    public static bool GetIfHasError(GlueProjectSave glueProjectSave, IGlueState glueState)
    {
        if(glueProjectSave == null)
        {
            return false;
        }
        else if(glueProjectSave != glueState.CurrentGlueProject)
        {
            return false;
        }

        var glujVersion = glueProjectSave.FileVersion;
        var dllSyntaxVersion = glueState.EngineDllSyntaxVersion;

        if(dllSyntaxVersion == null)
        {
            return false;
        }
        else
        {
            return dllSyntaxVersion < glujVersion;
        }
    }
}
