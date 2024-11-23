using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall.Glue.Plugins;
using FlatRedBall.Glue.Plugins.ExportedImplementations;
using OfficialPlugins.GlueProjectFileVersionPlugin.Errors;

namespace OfficialPlugins.GlueProjectFileVersionPlugin;

[Export(typeof(PluginBase))]
public class MainGlueProjectFileVersionPlugin : PluginBase
{
    public override void StartUp()
    {
        this.AddErrorReporter(new GlueProjectFileVersionErrorReporter(GlueState.Self));
    }
}
