using FlatRedBall.Glue.SaveClasses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCommunicationPlugin.JsonManager
{
    public class GlueJsonContainer
    {
        public const string TYPE_ENTITY = "Entity";
        public const string TYPE_SCREEN = "Screen";
        public const string TYPE_GLUE = "Glue";


        private object LockObject = new object();

        public DateTime LastUpdated { get; private set; }
        public bool IsDirty { get; private set; }

        private JsonContainer<GlueProjectSave> _glue;
        public JsonContainer<GlueProjectSave> Glue
        {
            get
            {
                return _glue;
            }
            set
            {
                lock (LockObject)
                {
                    _glue = value;
                    MarkDirty();
                }
            }
        }

        public void MarkDirty()
        {
            lock (LockObject)
            {
                LastUpdated = DateTime.Now;
                IsDirty = true;
            }
        }

        public void MarkClean()
        {
            lock (LockObject)
            {
                LastUpdated = DateTime.Now;
                IsDirty = false;
            }
        }

        private Dictionary<string, JsonContainer<EntitySave>> Entities { get; set; } = new Dictionary<string, JsonContainer<EntitySave>>();
        private Dictionary<string, JsonContainer<ScreenSave>> Screens { get; set; } = new Dictionary<string, JsonContainer<ScreenSave>>();


        public GlueJsonContainer()
        {
        }

        public JsonContainer<EntitySave> GetEntity(string name)
        {
            return Entities.ContainsKey(name) ? Entities[name] : null;
        }

        public void SetEntity(string name, JsonContainer<EntitySave> value)
        {
            lock (LockObject)
            {
                if (Entities.ContainsKey(name))
                    Entities[name] = value;
                else
                    Entities.Add(name, value);

                MarkDirty();
            }
        }

        public void RemoveEntity(string name)
        {
            lock (LockObject)
            {
                if (Entities.ContainsKey(name))
                {
                    Entities.Remove(name);
                    MarkDirty();
                }
            }
        }

        public JsonContainer<ScreenSave> GetScreen(string name)
        {
            return Screens.ContainsKey(name) ? Screens[name] : null;
        }

        public void SetScreen(string name, JsonContainer<ScreenSave> value)
        {
            lock (LockObject)
            {
                if (Screens.ContainsKey(name))
                    Screens[name] = value;
                else
                    Screens.Add(name, value);

                MarkDirty();
            }
        }

        public void RemoveScreen(string name)
        {
            lock (LockObject)
            {
                if (Screens.ContainsKey(name))
                {
                    Screens.Remove(name);
                    MarkDirty();
                }
            }
        }

        public class JsonContainer<T>
        {
            public JsonContainer(string json)
            {
                Json = JToken.Parse(json);
                Value = (T)JsonConvert.DeserializeObject(json, typeof(T));
            }

            public JToken Json { get; private set; }
            public T Value { get; private set; }
        }

        internal void Set(string type, string name, string json)
        {
            if (type == "Entity")
            {
                SetEntity(name, new JsonContainer<EntitySave>(json));
            }
            else if (type == "Screen")
            {
                SetScreen(name, new JsonContainer<ScreenSave>(json));
            }
            else if (type == "Glue")
            {
                Glue = new JsonContainer<GlueProjectSave>(json);
            }
            else
            {
                throw new NotImplementedException($"Type {type} unexpected.");
            }
        }

        public string GetAsJson()
        {
            lock (LockObject)
            {
                var jEntities = JToken.Parse("{}");

                foreach (var item in Entities)
                {
                    jEntities[item.Key] = item.Value.Json;
                }

                var jScreens = JToken.Parse("{}");

                foreach (var item in Screens)
                {
                    jScreens[item.Key] = item.Value.Json;
                }

                return JsonConvert.SerializeObject(new
                {
                    Glue = Glue.Json,
                    Entities = jEntities,
                    Screens = jScreens
                });
            }
        }
    }
}
