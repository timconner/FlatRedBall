﻿using FlatRedBall.Glue.Plugins;
using FlatRedBall.Glue.Plugins.Interfaces;
using GameJsonCommunicationPlugin.Common;
using Newtonsoft.Json;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace GameJsonCommunicationPlugin.JsonManager
{
    [Export(typeof(PluginBase))]
    public class MainGameJsonCommunicationPlugin : PluginBase
    {
        private const string PacketType_JsonUpdate = "JsonUpdate";
        private GlueJsonManager _glueJsonManager;

        public override string FriendlyName => "Game JSON Communication Plugin";

        public override Version Version => new Version(1, 0);

        public override bool ShutDown(PluginShutDownReason shutDownReason)
        {
            ReactToGlueJsonLoad -= HandleReactToGlueJsonLoad;
            ReactToScreenJsonLoad -= HandleReactToScreenJsonLoad;
            ReactToEntityJsonLoad -= HandleReactToEntityJsonLoad;

            ReactToGlueJsonSave -= HandleReactToGlueJsonSave;
            ReactToScreenJsonSave -= HandleReactToScreenJsonSave;
            ReactToEntityJsonSave -= HandleReactToEntityJsonSave;

            _glueJsonManager = null;

            return true;
        }

        public override void StartUp()
        {
            _glueJsonManager = new GlueJsonManager();

            ReactToGlueJsonLoad += HandleReactToGlueJsonLoad;
            ReactToScreenJsonLoad += HandleReactToScreenJsonLoad;
            ReactToEntityJsonLoad += HandleReactToEntityJsonLoad;

            ReactToGlueJsonSave += HandleReactToGlueJsonSave;
            ReactToScreenJsonSave += HandleReactToScreenJsonSave;
            ReactToEntityJsonSave += HandleReactToEntityJsonSave;
        }

        private void HandleReactToEntityJsonLoad(string entityName, string json)
        {
            if (_glueJsonManager.ContainsEntity(entityName))
                HandleReactToEntityJsonSave(entityName, json);
            else
                _glueJsonManager.AddEntity(entityName, json);
        }

        private void HandleReactToScreenJsonLoad(string screenName, string json)
        {
            if (_glueJsonManager.ContainsScreen(screenName))
                HandleReactToScreenJsonSave(screenName, json);
            else
                _glueJsonManager.AddScreen(screenName, json);
        }

        private void HandleReactToGlueJsonLoad(string json)
        {
            if (_glueJsonManager.GetGlueProjectSave() == null)
                _glueJsonManager.SetGlueProjectSave(json);
            else
                HandleReactToGlueJsonSave(json);
        }

        private void HandleReactToEntityJsonSave(string entityName, string json)
        {
            if (!_glueJsonManager.ContainsEntity(entityName))
                _glueJsonManager.AddEntity(entityName, "{}");
            var patch = _glueJsonManager.GetEntity(entityName).ApplyUIUpdate(json);

            if (patch != null)
            {
                Debug.Print($"Changes for Entity {entityName}");
                Debug.Print(patch.ToString());

                GameConnectionManager.Instance.SendItem(new GameConnectionManager.Packet
                {
                    PacketType = PacketType_JsonUpdate,
                    Payload = JsonConvert.SerializeObject(new JsonPayload
                    {
                        Type = "Entity",
                        Name = entityName,
                        Patch = patch.ToString()
                    })
                });
            }
        }

        private void HandleReactToScreenJsonSave(string screenName, string json)
        {
            if (!_glueJsonManager.ContainsScreen(screenName))
                _glueJsonManager.AddScreen(screenName, "{}");
            var patch = _glueJsonManager.GetScreen(screenName).ApplyUIUpdate(json);

            if (patch != null)
            {
                Debug.Print($"Changes for Screen {screenName}");
                Debug.Print(patch.ToString());

                GameConnectionManager.Instance.SendItem(new GameConnectionManager.Packet
                {
                    PacketType = PacketType_JsonUpdate,
                    Payload = JsonConvert.SerializeObject(new JsonPayload
                    {
                        Type = "Screen",
                        Name = screenName,
                        Patch = patch.ToString()
                    })
                });
            }
        }

        private void HandleReactToGlueJsonSave(string json)
        {
            var patch = _glueJsonManager.GetGlueProjectSave().ApplyUIUpdate(json);

            if (patch != null)
            {
                Debug.Print($"Changes for Glue Project Save");
                Debug.Print(patch.ToString());

                GameConnectionManager.Instance.SendItem(new GameConnectionManager.Packet
                {
                    PacketType = PacketType_JsonUpdate,
                    Payload = JsonConvert.SerializeObject(new JsonPayload
                    {
                        Type = "GlueProjectSave",
                        Name = "",
                        Patch = patch.ToString()
                    })
                });
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