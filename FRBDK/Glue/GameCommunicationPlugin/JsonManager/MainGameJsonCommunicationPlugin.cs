using FlatRedBall.Glue.Plugins;
using FlatRedBall.Glue.Plugins.ExportedImplementations;
using FlatRedBall.Glue.Plugins.Interfaces;
using GameCommunicationPlugin.CodeGeneration;
using GameCommunicationPlugin.Common;
using GameCommunicationPlugin.JsonManager;
using GameJsonCommunicationPlugin.Common;
using JsonDiffPatchDotNet.Formatters.JsonPatch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace GameJsonCommunicationPlugin.JsonManager
{
    [Export(typeof(PluginBase))]
    public class MainGameJsonCommunicationPlugin : PluginBase
    {
        private const string PacketType_JsonUpdate = "JsonUpdate";
        private GlueJsonContainer _glueJsonContainer;
        private bool _running = true;
        private Task _periodicTask;

        public override string FriendlyName => "Game JSON Communication Plugin";

        public override Version Version => new Version(1, 0);

        public override bool ShutDown(PluginShutDownReason shutDownReason)
        {
            //Turning this plugin off
            //return true;

            ReactToLoadedGlux -= HandleGluxLoaded;
            ReactToGlueJsonLoad -= HandleReactToGlueJsonLoad;
            ReactToScreenJsonLoad -= HandleReactToScreenJsonLoad;
            ReactToEntityJsonLoad -= HandleReactToEntityJsonLoad;

            ReactToGlueJsonSave -= HandleReactToGlueJsonSave;
            ReactToScreenJsonSave -= HandleReactToScreenJsonSave;
            ReactToEntityJsonSave -= HandleReactToEntityJsonSave;

            _running = false;

            if (_periodicTask != null)
                _periodicTask.Wait();

            _glueJsonContainer = null;

            return true;
        }

        public override void StartUp()
        {
            //Turning this plugin off
            //return;

            _glueJsonContainer = new GlueJsonContainer();

            ReactToGlueJsonLoad += HandleReactToGlueJsonLoad;
            ReactToScreenJsonLoad += HandleReactToScreenJsonLoad;
            ReactToEntityJsonLoad += HandleReactToEntityJsonLoad;

            ReactToGlueJsonSave += HandleReactToGlueJsonSave;
            ReactToScreenJsonSave += HandleReactToScreenJsonSave;
            ReactToEntityJsonSave += HandleReactToEntityJsonSave;

            ReactToLoadedGlux += HandleGluxLoaded;

            _running = true;
            _periodicTask = Task.Run(async () =>
            {
                while (_running)
                {
                    if (_glueJsonContainer.IsDirty && (DateTime.Now - _glueJsonContainer.LastUpdated).TotalMilliseconds > 100)
                    {
                        ReactToPluginEvent("GameCommunication_SendPacket", new GameConnectionManager.Packet
                        {
                            PacketType = PacketType_JsonUpdate,
                            Payload = _glueJsonContainer.GetAsJson()
                        });
                        _glueJsonContainer.MarkClean();
                    }

                    await Task.Delay(10);
                }
            });
        }

        private void HandleGluxLoaded()
        {
            if (GameCommunicationHelper.IsFrbUsesJson())
            {
                EmbeddedCodeManager.Embed(new System.Collections.Generic.List<string>
                {
                    "Json.GlueJsonManager.cs",
                    "Json.JsonContainer.cs",
                    "Json.JsonManager.cs",
                    "Json.ScreenJsonContainer.cs",

                    "Json.Operations.JsonOperations.cs"
                });

                Task.Run(async () =>
                {
                    var jsonVersion = await GlueCommands.Self.ProjectCommands.AddNugetIfNotAddedWithReturn("Newtonsoft.Json", "12.0.3");
                    GlueCommands.Self.ProjectCommands.AddNugetIfNotAdded("JsonDiffPatch.Net", "2.3.0");

                    var majorVersion = jsonVersion.Substring(0, jsonVersion.IndexOf('.'));

                    //GlueCommands.Self.ProjectCommands.AddAssemblyBinding("Newtonsoft.Json", "30ad4fe6b2a6aeed", $"0.0.0.0-{majorVersion}.0.0.0", $"{majorVersion}.0.0.0");
                });
            }
        }

        private void HandleLoad(string type, string name, string json)
        {
            //if (GameCommunicationHelper.IsFrbUsesJson())
            //{
            //    if (_glueJsonContainer.Get(type, name) == null)
            //    {
            //        _glueJsonContainer.Add(type, name);
            //    }

            //    var jsonManager = _glueJsonContainer.Get(type, name);

            //    var container = jsonManager.Reset(json);
            //    ReactToPluginEvent("GameCommunication_SendPacket", new GameConnectionManager.Packet
            //    {
            //        PacketType = PacketType_JsonUpdate,
            //        Payload = JsonConvert.SerializeObject(new JsonPayload
            //        {
            //            Type = type,
            //            Name = name,
            //            Patch = JsonConvert.SerializeObject(container)
            //        })
            //    });
            //}
        }

        private void HandleSave(string type, string name, string json)
        {
            if (GameCommunicationHelper.IsFrbUsesJson())
            {
                {
                    _glueJsonContainer.Set(type, name, json);
                    
                    //if (container != null)
                    //{
                    //    Debug.Print($"Changes for {type} {name}");
                    //    Debug.Print(container.Data.ToString());

                    //    ReactToPluginEvent("GameCommunication_SendPacket", new GameConnectionManager.Packet
                    //    {
                    //        PacketType = PacketType_JsonUpdate,
                    //        Payload = JsonConvert.SerializeObject(new JsonPayload
                    //        {
                    //            Type = type,
                    //            Name = name,
                    //            Patch = JsonConvert.SerializeObject(container)
                    //        })
                    //    });
                    //}
                }
            }
        }

        private void HandleReactToEntityJsonLoad(string entityName, string json)
        {
            //HandleLoad(GlueJsonManager.TYPE_ENTITY, entityName, json);
        }

        private void HandleReactToScreenJsonLoad(string screenName, string json)
        {
            //HandleLoad(GlueJsonManager.TYPE_SCREEN, screenName, json);
        }

        private void HandleReactToGlueJsonLoad(string json)
        {
            //HandleLoad(GlueJsonManager.TYPE_GLUE, "", json);
        }

        private void HandleReactToEntityJsonSave(string entityName, string json)
        {
            HandleSave(GlueJsonContainer.TYPE_ENTITY, entityName, json);
        }

        private void HandleReactToScreenJsonSave(string screenName, string json)
        {
            HandleSave(GlueJsonContainer.TYPE_SCREEN, screenName, json);
        }

        private void HandleReactToGlueJsonSave(string json)
        {
            HandleSave(GlueJsonContainer.TYPE_GLUE, "", json);
        }

        public override void HandleEvent(string eventName, string payload)
        {
            //Turning plugin off
            //return;
            base.HandleEvent(eventName, payload);

            switch(eventName)
            {
                case "GameCommunication_Connected":
                    //foreach(var item in _glueJsonContainer.GetAll())
                    //{
                    //    var mgr = _glueJsonContainer.Get(item.Type, item.Name);

                    //    HandleLoad(item.Type, item.Name, mgr.CurrentJson.ToString());
                    //}

                    break;
            }
        }
    }

    public class JsonPayload
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Patch { get; set; }
    }
}
