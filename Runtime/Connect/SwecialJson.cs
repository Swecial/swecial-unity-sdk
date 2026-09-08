using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace com.swecial.unity {
    public class SwecialJson {
        public static T deserialize<T>(string json) where T : SwecialObject {
            //var jo = JObject.Parse(json);
            var settings = new JsonSerializerSettings() {
                DateParseHandling = DateParseHandling.DateTimeOffset,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };
            var jo = JsonConvert.DeserializeObject<JObject>(json, settings);
            return deserialize<T>(jo);
        }
        public static T deserialize<T>(JObject jo) where T : SwecialObject {
            T so = Activator.CreateInstance(typeof(T)) as T;
            foreach (var jp in jo.Properties()) {
                var key = jp.Name;
                var value = jp.Value;
                if (typeof(T) == typeof(SwecialResponse<SwecialFile>) && key == "result") {
                    Debug.Log("sf result");
                }
                so.set(key, toObject(value));
            }
            return so;
        }
        public static object toObject(JToken value) {
            var type = value.Type;
            if (type == JTokenType.String) {
                return (string)value;
            } else if (type == JTokenType.Boolean) {
                return (bool)value;
            } else if (type == JTokenType.Float) {
                return (double)value;
            } else if (type == JTokenType.Integer) {
                return (long)value;
            } else if (type == JTokenType.Date) {
                return (DateTimeOffset)value;
            } else if (type == JTokenType.Null) {
                return null;
            } else if (type == JTokenType.Object) {
                var jo = (JObject)value;
                try {
                    var className = (string)jo["_className"];
                    if (className == "_file") {
                        return deserialize<SwecialFile>((JObject)value);
                    } else if (className == "_user") {
                        return deserialize<SwecialUser>((JObject)value);
                    } else {
                        return deserialize<SwecialObject>((JObject)value);
                    }
                } catch {
                    return deserialize<SwecialObject>((JObject)value);
                }
            } else if (type == JTokenType.Array) {
                var arr = value.ToArray();
                if (arr.Length > 0) {
                    var firstValue = arr[0];
                    List<object> list = new List<object>();
                    foreach (var arrValue in arr) {
                        list.Add(toObject(arrValue));
                    }
                    return list;
                } else {
                    return new List<SwecialObject>();
                }
            } else {
                return (string)value;
            }
        }
    }
}