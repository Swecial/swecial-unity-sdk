using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using com.swecial.util;
using System.Threading;

namespace com.swecial.unity {

	public static class H {
        public static Dictionary<string, Texture2D> spriteTextures;
		public static readonly int indentation = 3;
		public static readonly string selfBBColor = "[ff33ff]";
		public static DateTimeOffset StartOfWeek(this DateTimeOffset dt, DayOfWeek startOfWeek) {
			int diff = dt.DayOfWeek - startOfWeek;
			if (diff < 0) {
				diff += 7;
			}
			return dt.AddDays(-1 * diff).Date;
		}
		public static string clean(this string dirty) {
			return dirty.Replace(" ", "").ToLower();
		}
		public static T deserialize<T>(this string json) {
			return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings {
				DateParseHandling = DateParseHandling.DateTimeOffset,
				Error = (sender, errorArgs) => {
					errorArgs.ErrorContext.Handled = true;
				}
			});
		}
		/// <summary>
		/// Gets the 00:00:00 instance of a DateTime
		/// </summary>
		public static DateTimeOffset StartOfDay(this DateTimeOffset dateTime) {
			return new DateTimeOffset(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, dateTime.Offset);
		}
		/// <summary>
		/// Gets the 23:59:59 instance of a DateTime
		/// </summary>
		public static DateTimeOffset EndOfDay(this DateTimeOffset dateTime) {
			var sod = dateTime.StartOfDay();
			var eod = sod.AddDays(1).AddTicks(-1);
			return eod;
		}
		public static DateTimeOffset toDate(this long seconds) {
			return new DateTimeOffset(seconds * 10000000, TimeSpan.Zero);
		}
		public static long TotalSeconds(this DateTimeOffset dateTime) {
			return (long)(dateTime.Ticks / 10000000);
		}
		/// 
		/// Output a hex string from a color
		/// 
		///
		///Set to true to include a # character at the start
		/// 
		public static string ToHex(this Color color) {
			return color.ToHex(false);
		}
		public static string ToHex(this Color color, bool includeHash) {
			string red = Mathf.FloorToInt(color.r*255).ToString("X2");
			string green = Mathf.FloorToInt(color.g * 255).ToString("X2");
			string blue = Mathf.FloorToInt(color.b * 255).ToString("X2");
			return (includeHash ? "#" : "") + red + green + blue;
		}
		public static DateTimeOffset toDate(this string sDate) {
			DateTime dt = DateTime.MinValue;
			if (DateTime.TryParseExact(sDate, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out dt)) {
				return new DateTimeOffset(dt, TimeSpan.Zero);
			} else {
				return DateTimeOffset.MinValue;
			}
		}
		/// 
		/// Create a Color object from a Hex string (It's not important if you have a # character at
		/// the start or not)
		/// 
		///The hex string to convert
		/// A Color object
		public static Color toColor(this string color) {
			// remove the # character if there is one.
			color = color.TrimStart('#');
			float red = (HexToInt(color[1]) + HexToInt(color[0]) * 16f) / 255f;
			float green = (HexToInt(color[3]) + HexToInt(color[2]) * 16f) / 255f;
			float blue = (HexToInt(color[5]) + HexToInt(color[4]) * 16f) / 255f;
			Color finalColor = new Color { r = red, g = green, b = blue, a = 1 };
			return finalColor;
		}
		
		/// 
		/// Create a color object from integer R G B (A) components
		/// 
		///The red component
		///The green component
		///The blue component
		///The alpha component (Defaults to 255, or fully opaque)
		/// A Color object
		public static Color FromInt(int r, int g, int b) {
				return FromInt(r, g, b, 255);
		}
		public static Color FromInt(int r, int g, int b, int a) {
			return new Color(r/255f, g/255f, b/255f, a/255f);
		}
		
		private static int HexToInt(char hexValue) {
			return int.Parse(hexValue.ToString(), System.Globalization.NumberStyles.HexNumber);
		}
		public static Color color(int pColor) {
			Color color = new Color();
			color.r = ((pColor & 0xFF0000) >> 16) / 255f;
			color.g = ((pColor & 0x00FF00) >> 8) / 255f;
			color.b = (pColor & 0x0000FF) / 255f;
			color.a = 1.0f;
			return color;
		}
		public static int getHash(string s) {
			int hash = 0;
			for (int i = 0; i < s.Length; i++) {
				int code = s.Substring(i, 1).ToCharArray()[0];
				hash += code;
			}
			return hash * 1231;
		}
		public static int getHashOLD(string s) {
			//int hashLength = 100000000;
			long hash = 0;
			var bytes = System.Text.Encoding.ASCII.GetBytes(s);
			foreach (byte b in bytes) {
				hash += b;
				hash += (hash << 10);
				hash ^= (hash >> 6);
			}
			// final avalanche
	//		hash += (hash << 3);
	//		hash ^= (hash >> 11);
	//		hash += (hash << 15);
			
			//return (int)(hash % hashLength);
			return (int)hash;
		}
		public static bool isEmpty(this string s) {
			return (s == null || s.Trim() == "");
		}
		public static bool isEmpty<T>(this List<T> list) {
			return (list == null || list.Count == 0);
		}
		public static bool isEmpty(this object o) {
			return o == null;
		}
        public static T removeRandom<T>(this List<T> list) {
            T obj = list.getRandom();
            try {
                list.Remove(obj);
                return obj;
            } catch {
                return default(T);
            }
        }
        public static T removeAt<T>(this List<T> list, int index) {
            if (list.Count > index) {
                T item = list[index];
                list.RemoveAt(index);
                return item;
            } else {
                return default(T);
            }
        }
        public static T getRandom<T>(this List<T> list) {
            if (list.Count > 0) {
                int index = UnityEngine.Random.Range(0, list.Count);
                return list[index];
            } else {
                return default(T);
            }
        }
        public static T removeRandom<T>(this List<T> list, Func<T, bool> condition) {
            List<T> listThatMetCondition = new List<T>();
            foreach (T o in list) {
                if (condition(o)) {
                    listThatMetCondition.Add(o);
                }
            }
            if (listThatMetCondition.Count > 0) {
                try {
                    var objectToRemove = listThatMetCondition.removeRandom();
                    list.Remove(objectToRemove);
                    return objectToRemove;
                } catch {
                    return default(T);
                }
            } else {
                return default(T);
            }
        }
        public static T getRandom<T>(this List<WeightedObject<T>> list) {
            float totalWeight = 0;
            foreach (WeightedObject<T> wo in list) {
                totalWeight += wo.weight;
            }
            int randomIndex = -1;
            float random = UnityEngine.Random.value * totalWeight;
            for (int i = 0; i < list.Count; ++i) {
                WeightedObject<T> wo = list[i];
                random -= wo.weight;
                if (random <= 0.0f) {
                    randomIndex = i;
                    break;
                }
            }
            return list[randomIndex].obj;
        }
        public static T getRandom<T>(params WeightedObject<T>[] list) {
			float totalWeight = 0;
			foreach (WeightedObject<T> wo in list) {
				totalWeight += wo.weight;
			}
			int randomIndex = -1;
			float random = UnityEngine.Random.value * totalWeight;
			for (int i = 0; i < list.Length; ++i) {
				WeightedObject<T> wo = list[i];
				random -= wo.weight;
				if (random <= 0.0f) {
					randomIndex = i;
					break;
				}
			}
			return list[randomIndex].obj;
		}
        public static WeightedObject<T> wo<T>(this T obj, float weight) {
            return new WeightedObject<T>(obj, weight);
        }
        public static string getTime(this TimeSpan ts) {
            return getTime(ts.TotalSeconds);
        }
		public static string getTime(this double seconds) {
			return ((long)seconds).getTime(true);
		}
		public static string getTime(this double seconds, bool includeSeconds) {
			return ((long)seconds).getTime(includeSeconds);
		}
		public static string getTime(this long seconds) {
			return seconds.getTime(true);
		}
		public static string getTime(this long seconds, bool includeSeconds) {
			int minutes = (int)(seconds / 60);
			int hours = (int)(minutes / 60);
			minutes = (int)(minutes % 60);
			int secs = (int)(seconds % 60);
			if (hours > 0) {
				if (includeSeconds) {
					return hours.ToString("D2") + ":" + minutes.ToString("D2") + ":" + secs.ToString("D2");
				} else {
					return hours.ToString("D2") + ":" + minutes.ToString("D2");
				}
			} else {
				if (includeSeconds) {
					return minutes.ToString("D2") + ":" + secs.ToString("D2");
				} else {
					return minutes.ToString("D2");
				}
			}
		}
        public static bool isSame(this SwecialObject so1, SwecialObject so2) {
			if (so1 != null && so2 != null && !so1.id.isEmpty() && !so2.id.isEmpty()) {
				return so1.id.Equals(so2.id) && so1.className.Equals(so2.className);
			} else {
				return false;
			}
		}
		public static string format(this int number) {
			return ((long)number).format();
		}
		public static string first(this string orig, char delimiter) {
			return orig.Substring(0, orig.IndexOf(delimiter));
		}
		public static string rest(this string orig, char delimiter) {
			return orig.Substring(orig.IndexOf(delimiter) + 1);
		}
		public static string format(this long number) {
			string retVal = number.ToString("### ### ### ### ###").Trim();
			if (retVal == "") retVal = "0";
			return retVal;
		}
		public static string formatLength(this long mm) {
			return ((double)mm).formatLength();
		}
		public static string formatLength(this double mm) {
			double cm = mm / 10d;
			double m = cm / 100d;
			double km = m / 1000d;
			double ml = km / 10d;
			if (ml > 1) {
				return Math.Round(ml, 2).ToString("#,##0.00") + "ml";
			} else if (km > 1) {
				return Math.Round(km, 2).ToString("#,##0.00") + "km";
			} else if (m > 1) {
				return Math.Round(m, 2).ToString("#,##0.00") + "m";
			} else if (cm > 1) {
				return Math.Round(cm, 2).ToString("#,##0.00") + "cm";
			} else {
				return Math.Round(mm, 2).ToString("#,##0.00") + "mm";
			}
		}
		public static string base64Encode(this string plainText) {
			return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(plainText));
		}
		public static string base64Encode(this byte[] bytes) {
			return Convert.ToBase64String(bytes);
		}
		public static string base64Decode(this string encodedText) {
			return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedText));
		}


		public static string toJson(this object o) {
			StringBuilder sb = new StringBuilder();
			o.toJson(sb, null, null, 0);
			return sb.ToString();
		}
		public static void toJson(this object o, StringBuilder sb, List<string> includes, string trail, int indent) {
			if (o is SwecialObject) {
				var so = (SwecialObject)o;
				sb.Append("{\n");
				int counter = 0;
				foreach (string key in so.Keys) {
					if (!key.StartsWith("@")) { // @ prefixes values that are just to be kept in-memory, like keyBytes and ivBytes
						counter++;
						string fieldTrail = "";
						if (trail == null) {
							fieldTrail = key;
						} else {
							fieldTrail = trail + "." + key;
						}
						string label = key;
						object value = so[key];
						if (value is SwecialObject) {
							SwecialObject soValue = (SwecialObject)value;
							if (includes.isIncluded(fieldTrail)) {
								sb.indent(indent + 1).Append("\"").Append(label).Append("\": ");
								value.toJson(sb, includes, fieldTrail, indent + 1);
							} else {
								sb.indent(indent + 1).Append("\"").Append("__" + label).Append("\": ");
								if (soValue.className == "_user") {
									sb.Append("\"_users|" + soValue.className + "|" + soValue.id + "\"");
								} else {
									string appName = Swecial.current.apiAppId;
									if (!soValue.get<string>("_appName").isEmpty()) {
										appName = soValue.get<string>("_appName");
									}
									sb.Append("\"" + appName + "|" + soValue.className + "|" + soValue.id + "\"");
								}
							}
						} else {
							sb.indent(indent + 1).Append("\"").Append(label).Append("\": ");
							value.toJson(sb, includes, fieldTrail, indent + 1);
						}
						if (counter < so.Keys.Count) sb.Append(",");
						sb.Append("\n");
					}
				}
				sb.indent(indent).Append("}");
			} else if (o is List<SwecialObject>) {
				sb.Append("[");
				List<SwecialObject> list = (List<SwecialObject>)o;
				for (int i = 0; i < list.Count; i++) {
					sb.Append("\n").indent(indent + 1);
					list[i].toJson(sb, includes, trail, indent + 1);
					if (i < (list.Count - 1)) {
						sb.Append(",");
					} else {
						sb.Append("\n").indent(indent);
					}
				}
				sb.Append("]");
			} else {
				sb.Append(JsonConvert.SerializeObject(o, Formatting.Indented));
			}
		}
		public static bool isIncluded(this List<string> includes, string trail) {
			if (includes != null) {
				foreach (string include in includes) {
					if (include.ToLower().StartsWith(trail.ToLower())) {
						return true;
					}
				}
			}
			return false;
		}
		public static StringBuilder indent(this StringBuilder sb, int indent) {
			return sb.Append(' ', indent * indentation);
		}
		public static bool getBool(this Dictionary<string, bool> dict, string key) {
			if (dict != null) {
				bool retVal = false;
				if (dict.TryGetValue(key, out retVal)) {
					return retVal;
				} else {
					return false;
				}
			} else {
				return false;
			}
		}
		public static bool isNumeric(this JProperty property) { 
			string json = property.ToString();
			int colon = json.IndexOf(':');
			if (colon > 0) {
				string value = json.Substring(colon + 1, json.Length - colon - 1).Trim();
				if (value.Length > 0) {
					if (value.Contains("\"")) {
						return false;
					} else {
						double d;
						return double.TryParse(value, out d);
					}
				}
			}
			return false;
		}
		public static bool isBool(this JProperty property) {
			string json = property.ToString();
			int colon = json.IndexOf(':');
			if (colon > 0) {
				string value = json.Substring(colon + 1, json.Length - colon - 1).Trim();
				if (value.Length > 0) {
					if (value.Contains("\"")) {
						return false;
					} else {
						bool d;
						return bool.TryParse(value, out d);
					}
				}
			}
			return false;
		}
		public static void setAsUnchanged(this object o) {
			if (o != null) {
				if (o.GetType().IsAssignableFrom(typeof(SwecialObject))) {
					((SwecialObject)o).changes = null;
				} else if (o is List<SwecialObject>) {
					foreach (SwecialObject so in (List<SwecialObject>)o) {
						so.changes = null;
					}
				} else if (o is SwecialResponse<SwecialObject>) {
					((SwecialResponse<SwecialObject>)o).result.setAsUnchanged();
				} else if (o is SwecialResponse<List<SwecialObject>>) {
					((SwecialResponse<List<SwecialObject>>)o).result.setAsUnchanged();
				}
			}
		}
		public static void linkObjects(this object o) {
			if (o != null) {
				if (o is SwecialObject) {
					((SwecialObject)o).linkObjects();
				} else if (o is List<SwecialObject>) {
					foreach (SwecialObject so in (List<SwecialObject>)o) {
						so.linkObjects();
					}
				} else if (o is SwecialResponse<SwecialObject>) {
					((SwecialResponse<SwecialObject>)o).result.linkObjects();
				} else if (o is SwecialResponse<List<SwecialObject>>) {
					((SwecialResponse<List<SwecialObject>>)o).result.linkObjects();
				}
			}
		}
		public static string ReadLine(this BinaryReader reader) {
			StringBuilder line = new StringBuilder();
			char c = reader.ReadChar();
			while (c != '\n') {
				if (c != '\r') {
					line.Append(c);
				}
				c = reader.ReadChar();
			}
			return line.ToString();
		}
		public static void WriteLine(this BinaryWriter writer, string line) {
			foreach (char c in line.ToCharArray()) {
				writer.Write(c);
			}
			writer.Write('\n');
		}
		public static bool isTrue(this bool? nullable) {
			return (nullable != null && (bool)nullable);
		}
        public static string readFile(this string filePath) {
            var fileLock = Locker.getLock(filePath);
            string fileContent = "";
            lock (fileLock) {
                bool tryRead = true;
                while (tryRead) {
                    try {
                        fileContent = File.ReadAllText(filePath);
                        tryRead = false;
                    } catch (Exception ex) {
                        if (ex is IOException) {
                            Thread.Sleep(1);
                        } else {
                            throw;
                        }
                    }
                }
            }
            Locker.deleteLock(fileLock);
            return fileContent;
        }
        public static void writeFile(this string filePath, string fileContent) {
            //SwecialFileSystemWatcher.ignore(filePath);
            var fileLock = Locker.getLock(filePath);
            lock (fileLock) {
                bool tryWrite = true;
                while (tryWrite) {
                    try {
                        File.WriteAllText(filePath, fileContent);
                        tryWrite = false;
                    } catch (Exception ex) {
                        if (ex is IOException) {
                            Thread.Sleep(1);
                        } else {
                            throw;
                        }
                    }
                }
            }
            Locker.deleteLock(fileLock);
        }
        public static void savePng(Texture2D tx, string fileName) {
			string path = Application.persistentDataPath + "/" + fileName;
			var bytes = tx.EncodeToPNG();
			var file = File.Open(path, FileMode.Create);
			var bw = new BinaryWriter(file);
			bw.Write(bytes);
			file.Close();
		}
		public static bool loadPng(Texture2D tx, string fileName) {
			string path = Application.persistentDataPath + "/" + fileName;
			if (File.Exists(path)) {
				byte[] bytes = File.ReadAllBytes(path);
				tx.LoadImage(bytes);
				return true;
			} else {
				return false;
			}
		}
		public static void deletePng(string fileName) {
			string path = Application.persistentDataPath + "/" + fileName;
			File.Delete(path);
		}
		public static long seconds(this TimeSpan ts) {
			return ts.Ticks / TimeSpan.TicksPerSecond;
		}
		public static long millis(this TimeSpan ts) {
			return ts.Ticks / TimeSpan.TicksPerMillisecond;
		}
        public static Texture2D getTexture(this Sprite sprite) {
            if (spriteTextures == null) {
                spriteTextures = new Dictionary<string, Texture2D>();
            }
            if (spriteTextures.ContainsKey(sprite.name)) {
                return spriteTextures[sprite.name];
            } else {
                var croppedTexture = new Texture2D((int)sprite.textureRect.width, (int)sprite.textureRect.height);
                var pixels = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                        (int)sprite.textureRect.y,
                                                        (int)sprite.textureRect.width,
                                                        (int)sprite.textureRect.height);
                croppedTexture.SetPixels(pixels);
                croppedTexture.Apply();
                spriteTextures[sprite.name] = croppedTexture;
                return croppedTexture;
            }
        }
	}

}