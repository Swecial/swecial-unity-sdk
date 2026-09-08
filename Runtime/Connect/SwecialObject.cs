using UnityEngine;
using System.Collections;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace com.swecial.unity {
	public class SwecialObject : IDictionary<string, object> {

		[JsonIgnore]
		private Dictionary<string, object> properties;
		[JsonIgnore]
		public Dictionary<string, int> changes;

		public SwecialObject() { 
			properties = new Dictionary<string, object>();
		}
		public SwecialObject(string className) {
			properties = new Dictionary<string, object>();
			this.className = className;
		}
		public SwecialObject(string className, string id) {
			properties = new Dictionary<string, object>();
			this.className = className;
			this.id = id;
		}
		public string id {
			get {
				return this.get<string>("_id");
			}
			set {
				set("_id", value);
			}
		}
		public string className {
			get {
				return this.get<string>("_className");
			}
			set {
				set("_className", value);
			}
		}
		public string appName {
			get {
				return this.get<string>("@appName");
			}
			set {
				set("@appName", value);
			}
		}
		public bool isLoaded {
			get {
				return !this.get<bool>("@isLinkObject");
			}
			set {
				set("@isLinkObject", !value);
			}
		}
		public void set(string key, object value) {
			if (changes == null) {
				changes = new Dictionary<string, int>();
			}
			changes[key] = 1;
			properties[key] = value;
		}
		public void increment(string key) {
			increment(key, 1);
		}
		public void increment(string key, int value) {
			if (changes == null) {
				changes = new Dictionary<string, int>();
			}
			if (changes.ContainsKey(key)) {
				// already changed, no need to increment on server
				set(key, get<int>(key) + value);
			} else {
				set(key, get<int>(key) + value);
				changes.Remove(key);
				string incrementKey = "+" + key;
				if (changes.ContainsKey(incrementKey)) {
					changes[incrementKey] += value;
				} else {
					changes[incrementKey] = value;
				}
			}
		}
		public void remove(string key) {
			Remove(key);
		}
		public T get<T>(string key) {
			if (key.Contains(".")) {
				string fieldName = key.first('.');
				string rest = key.rest('.');
				var so = get<SwecialObject>(fieldName);
				if (so != null) {
					return so.get<T>(rest);
				} else {
					return default(T);
				}
			} else {
                object o;
                if (TryGetValue(key, out o)) {
                    try {
                        if (o != null) {
                            if (typeof(T).IsAssignableFrom(o.GetType())) {
                                return (T)o;
                            } else if (typeof(T) == typeof(int)) {
                                o = Convert.ToInt32(o);
                                return (T)o;
                            } else if (typeof(T) == typeof(bool)) {
                                o = Convert.ToBoolean(o);
                                return (T)o;
                            } else if (typeof(T) == typeof(long)) {
                                o = Convert.ToInt64(o);
                                return (T)o;
                            } else if (typeof(T) == typeof(DateTimeOffset)) {
                                o = DateTimeOffset.Parse(o.ToString());
                                return (T)o;
                            } else if (typeof(T) == typeof(string)) {
                                if (!(o is string)) {
                                    o = Convert.ToString(o);
                                }
                                return (T)o;
                            } else if (typeof(T) == typeof(float)) {
                                o = Convert.ToSingle(o);
                                return (T)o;
                            } else if (typeof(T) == typeof(List<SwecialObject>)) {
                                if (o is List<object>) {
                                    var tmp = (object)((List<object>)o).Cast<SwecialObject>().ToList();
                                    return (T)tmp;
                                }
                            } else if (typeof(T) == typeof(List<SwecialFile>)) {
                                if (o is List<object>) {
                                    var tmp = (object)((List<object>)o).Cast<SwecialFile>().ToList();
                                    return (T)tmp;
                                }
                            } else if (typeof(T) == typeof(List<SwecialUser>)) {
                                if (o is List<object>) {
                                    var tmp = (object)((List<object>)o).Cast<SwecialUser>().ToList();
                                    return (T)tmp;
                                }
                            } else if (typeof(T) == typeof(List<long>)) {
                                if (o is List<object>) {
                                    var tmp = (object)((List<object>)o).Cast<long>().ToList();
                                    return (T)tmp;
                                }
                            } else if (typeof(T) == typeof(List<int>)) {
                                try {
                                    if (o is List<object>) {
                                        var tmp = (object)((List<object>)o).Cast<int>().ToList();
                                        return (T)tmp;
                                    }
                                } catch {
                                    List<int> list = ((List<object>)o).ConvertAll(i => Convert.ToInt32(i));
                                    return (T)(object)list;
                                }
                            } else if (typeof(T) == typeof(List<double>)) {
                                if (o is List<object>) {
                                    var tmp = (object)((List<object>)o).Cast<double>().ToList();
                                    return (T)tmp;
                                }
                            } else if (typeof(T) == typeof(List<string>)) {
                                if (o is List<object>) {
                                    var tmp = (object)((List<object>)o).Cast<string>().ToList();
                                    return (T)tmp;
                                }
                            }
                            try {
                                return (T)o;
                            } catch {
                                return default(T);
                            }
                        } else {
                            return default(T);
                        }
                    } catch (Exception ex) {
                        Debug.Log(ex);
                        return default(T);
                    }
                } else {
					if (typeof(T) == typeof(SwecialObject) || typeof(T) == typeof(SwecialFile)) {
						string s = get<string>("__" + key);
						if (s != null) {
							string[] split = s.Split('|');
							string linkAppName = null, linkClassName = null, linkObjectId = null;
                            if (split.Length > 2) {
                                linkAppName = split[0];
                                linkClassName = split[1];
                                linkObjectId = split[2];
                            } else if (split.Length == 2) {
                                linkClassName = split[0];
                                linkObjectId = split[1];
                            } else {
                                linkObjectId = s;
                            }
							if (linkObjectId != null) {
								SwecialObject linkObject;
								if (typeof(T) == typeof(SwecialFile)) {
									var fileObject = new SwecialFile();
									linkObject = (SwecialObject)fileObject;
                                    linkClassName = "_file";
								} else {
									linkObject = new SwecialObject();
								}
								linkObject.id = linkObjectId;
								linkObject.className = linkClassName;
								linkObject.appName = linkAppName;
								linkObject.isLoaded = false;
								return (T)(object)linkObject;
							}
						}
					}
				}
				return default(T);
			}
		} 
		public bool hasKey(string key) {
			if (key.Contains(".")) {
				string fieldName = key.first('.');
				string rest = key.rest('.');
				var so = get<SwecialObject>(fieldName);
				if (so != null) {
					return so.hasKey(rest);
				} else {
					return false;
				}
			} else {
				if (ContainsKey(key)) {
					return true;
				} else {
					// check for links
					return ContainsKey("__" + key);
				}
			}
		}
		public void linkObjects() {
			string[] keys = new string[Keys.Count];
			Keys.CopyTo(keys, 0);
            foreach (string key in keys) {
                object o = this[key];
                if (o is JArray) {
                    var jarr = (JArray)o;
                    List<SwecialObject> list = new List<SwecialObject>();
                    foreach (var jt in jarr) {
                        var listItem = jt.ToString().deserialize<SwecialObject>();
                        list.Add(listItem);
                    }
                    this[key] = list;
                } else if (o is JObject || o is JContainer) {
                    var ss = new JsonSerializerSettings() {
                        DateParseHandling = DateParseHandling.DateTimeOffset
                    };
                    SwecialObject so = JsonConvert.DeserializeObject<SwecialObject>(o.ToString(), ss);
                    if (so.className == "_file") {
                        var sf = o.ToString().deserialize<SwecialFile>();
                        so = (SwecialObject)sf;
                    }
                    so.linkObjects();
                    so.setAsUnchanged();
                    this[key] = so;
                } else if (key.StartsWith("__")) {
                    if (o != null && o.ToString().Contains(".")) {
                        string newKey = key.Substring(2);
                        SwecialFile sf = new SwecialFile();
                        sf.id = o.ToString();
                        set(newKey, sf);
                        remove(key);
                        break;
                    }
                }
            }
		}
		public virtual void copyFrom(object o) {
			if (o is SwecialObject) {
				var so = (SwecialObject)o;
				Clear();
				foreach (string key in so.Keys) {
					this[key] = so[key];
				}
			}
		}
		public SwecialRequest<SwecialObject> prepareLoad() {
			var sr = new SwecialRequest<SwecialObject>();
			sr.command = "load";
			sr.setObject(this);
			return sr;
		}
		public virtual SwecialRequest<SwecialObject> load() {
			var sr = new SwecialRequest<SwecialObject>();
			sr.command = "load";
			sr.setObject(this);
			return sr.execute();
		}
		public SwecialRequest<SwecialObject> prepareSave() {
			var sr = new SwecialRequest<SwecialObject>();
			sr.command = "save";
			sr.setObject(this);
			return sr;
		}
		public SwecialRequest<SwecialObject> createOrLoad() {
			var sr = new SwecialRequest<SwecialObject>();
			sr.command = "create_or_load";
			sr.setObject(this);
			return sr;
		}
		public virtual SwecialRequest<SwecialObject> save() {
			var sr = new SwecialRequest<SwecialObject>();
			sr.command = "save";
			sr.setObject(this);
			return sr.execute();
		}
		public SwecialRequest<SwecialObject> delete() {
			var sr = new SwecialRequest<SwecialObject>();
			sr.command = "delete";
			sr.setObject(this);
			return sr.execute();
		}
		public bool isClass(string className) {
			return this.className == className;
		}
		public SwecialObject getRequestObject() {
			SwecialObject so = new SwecialObject();
			foreach (var key in properties.Keys) {
				if (changes != null && changes.ContainsKey(key)) {
					if (changes[key] == 1) {
						so.properties[key] = properties[key];
					}
				}
			}
			if (so.id == null && id != null) {
				so.id = id;
			}
			if (so.className == null && className != null) {
				so.className = className;
			}
			if (so.appName == null && appName != null) {
				so.appName = appName;
			}
			if (changes != null) {
				foreach (var key in changes.Keys) {
					if (key.StartsWith("+")) {
						so.properties[key] = changes[key];
					} else if (changes[key] == -1) {
						so.properties["-" + key] = "deleted";
					}
				}
			}
			return so;
		}


		// IDictionary implementation
		public bool ContainsKey (string key) {
			if (properties.ContainsKey(key)) {
				return true;
			} else {
				return properties.ContainsKey("__" + key);
			}
		}
		public void Add (string key, object value) {
			set(key, value);
		}
		public bool TryGetValue (string key, out object value) {
			return properties.TryGetValue(key, out value);
		}
		public object this [string index] {
			get {
				return get<object>(index);
			}
			set {
				set(index, value);
			}
		}
		public ICollection<string> Keys {
			get {
				return properties.Keys;
			}
		}
		public ICollection<object> Values {
			get {
				return properties.Values;
			}
		}
		public void Add (KeyValuePair<string, object> item) {
			set(item.Key, item.Value);
		}
		public void Clear() {
			properties.Clear();
		}
		public bool Remove(string key) {
			if (changes == null) {
				changes = new Dictionary<string, int>();
			}
			changes[key] = -1;
			if (ContainsKey(key)) {
				return properties.Remove(key);
			} else if (ContainsKey("__" + key)) {
				return properties.Remove("__" + key);
			}
			return false;
		}
		public int Count {
			get {
				return properties.Count;
			}
		}
		public IEnumerator<KeyValuePair<string, object>> GetEnumerator ()
		{
			throw new NotImplementedException ();
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return properties.GetEnumerator();
		}
		public bool IsReadOnly {
			get {
				return false;
			}
		}
		public bool Contains (KeyValuePair<string, object> item) {
			return properties.ContainsKey(item.Key) && properties[item.Key] == item.Value;
		}
		public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) {
			throw new NotImplementedException();
		}
		public bool Remove (KeyValuePair<string, object> item) {
			throw new NotImplementedException();
		}
	}
}