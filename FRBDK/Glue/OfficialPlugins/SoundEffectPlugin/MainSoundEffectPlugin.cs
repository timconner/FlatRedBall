using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall.Glue.Plugins;

namespace OfficialPlugins.SoundEffectPlugin
{
    [Export(typeof(PluginBase))]
    public class MainSoundEffectPlugin : PluginBase
    {
        public override void StartUp()
        {

        }
    }
}
