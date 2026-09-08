using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json.Utilities;

namespace com.swecial.unity {
	public class SwecialRequest<T> {

		public class Operator {
			public string key, op, value;
			public Operator(string key, string op, string value) {
				this.key = key;
				this.op = op;
				this.value = value;
			}
		}
		[JsonIgnore]
		private bool isExecuting = false;
		[JsonIgnore]
		public float retryTimeout = 5f;
		[JsonIgnore]
		public bool isCompleted;
		[JsonIgnore]
		public bool isSucceeded;
		[JsonIgnore]
		public bool hasReadResponse;
		[JsonIgnore]
		public bool hasRunBeforeConfirm;
		[JsonIgnore]
		public bool isRunningBeforeConfirm;
		[JsonIgnore]
		public bool keepTrying = true;
		[JsonIgnore]
		public SwecialResponse<T> response;
		[JsonIgnore]
		public long loadTime;
		[JsonIgnore]
		public long requestSize;
		[JsonIgnore]
		public string key;
        [JsonIgnore]
        private List<byte[]> files;

        [JsonProperty]
        public List<int> fileLengths;
		[JsonProperty]
		internal string command;
		[JsonProperty]
		internal string installId;
		[JsonProperty]
		private List<Operator> filters;
		[JsonProperty]
		private SwecialObject o;
		[JsonIgnore]
		private SwecialObject original;

		public SwecialRequest() {
			this.installId = Swecial.installId;
			this.retryTimeout = Swecial.retryTimeout;
			this.keepTrying = Swecial.keepTrying;
		}
        [JsonIgnore]
        public string error {
            get {
                if (response != null) {
                    return response.error;
                } else {
                    return null;
                }
            }
        }
        [JsonIgnore]
        public string message {
            get {
                if (response != null) {
                    return response.message;
                } else {
                    return null;
                }
            }
        }
        public T result {
			get {
                if (response != null) {
                    return response.result;
                } else {
                    return default(T);
                }
			}
		}
		public string toJson() {
			StringBuilder sb = new StringBuilder();
			sb.Append("{\n");
			sb.indent(1).Append("\"command\": \"").Append(command).Append("\",\n");
			sb.indent(1).Append("\"installId\": \"").Append(installId).Append("\",\n");
			sb.indent(1).Append("\"o\": ");
			if (o != null) {
				o.toJson(sb, null, null, 1);
			} else {
				sb.Append("null");
			}
			sb.Append(",\n");
			if (fileLengths != null && fileLengths.Count > 0) {
				sb.indent(1).Append("\"fileLengths\": ").Append(fileLengths.toJson()).Append(",\n");
			}
			sb.indent(1).Append("\"filters\": ").Append(filters.toJson()).Append("\n");
			sb.Append("}\n");
			return sb.ToString();
		}
		public SwecialRequest<T> setRetryTimeout(float timeout) {
			retryTimeout = timeout;
			return this;
		}
		public SwecialRequest<T> setNoRetries() {
			keepTrying = false;
			return this;
		}
        public SwecialRequest<T> addFile(byte[] rawBytes) {
            if (files == null) {
                files = new List<byte[]>();
                fileLengths = new List<int>();
            }
            var encryptedBytes = SwecialCrypto.Encrypt(rawBytes, Swecial.key, Swecial.iv);
            files.Add(encryptedBytes);
            fileLengths.Add(encryptedBytes.Length);
            return this;
        }
		public SwecialRequest<T> runAfter(Action<SwecialRequest<T>> action) {
			var sr = this;
			Swecial.current.StartCoroutine(Swecial.current.crRunAfter<T>(this, action));
			if (!isExecuting) {
				sr = sr.execute();
			}
			return this;
		}
		public SwecialRequest<T> beforeConfirm(Action<SwecialRequest<T>> action) {
			isRunningBeforeConfirm = true;
			Swecial.current.StartCoroutine(Swecial.current.crBeforeConfirm<T>(this, action));
			return this;
		}
		public SwecialRequest<T> progress(Action<SwecialRequest<T>> action) {
			Swecial.current.StartCoroutine(Swecial.current.crProgress<T>(this, action));
			return this;
		}
		public SwecialRequest<T> setObject(SwecialObject o) {
			this.o = o;
			return this;
		}
		public SwecialRequest<T> setIfGreaterThan(string key) {
			object val = null;
			if (o.TryGetValue(key, out val)) {
				if (val is int || val is long || val is double) {
					addFilter(key, "cond_updatekey_gt_num", "");
				} else if (val is DateTimeOffset) {
					addFilter(key, "cond_updatekey_gt_date", "");
				} else {
					addFilter(key, "cond_updatekey_gt_string", "");
				}
			}
			return this;
		}
		public SwecialRequest<T> saveIfGreaterThan(string key) {
			object val = null;
			if (o.TryGetValue(key, out val)) {
				if (val is int || val is long || val is double) {
					addFilter(key, "cond_save_gt_num", "");
				} else if (val is DateTimeOffset) {
					addFilter(key, "cond_save_gt_date", "");
				} else {
					addFilter(key, "cond_save_gt_string", "");
				}
			}
			return this;
		}
		public SwecialRequest<T> saveIfLessThan(string key) {
			object val = null;
			if (o.TryGetValue(key, out val)) {
				if (val is int || val is long || val is double) {
					addFilter(key, "cond_save_lt_num", "");
				} else if (val is DateTimeOffset) {
					addFilter(key, "cond_save_lt_date", "");
				} else {
					addFilter(key, "cond_save_lt_string", "");
				}
			}
			return this;
		}
		public SwecialRequest<T> saveIfLessThan(string key, DateTimeOffset val) {
			addFilter(key, "cond_save_lt_date", val.ToString("o"));
			return this;
		}
		public SwecialRequest<T> saveIfGreaterThan(string key, DateTimeOffset val) {
			addFilter(key, "cond_save_gt_date", val.ToString("o"));
			return this;
		}
		public SwecialRequest<T> or() {
			var lastFilter = getLastFilter();
			if (lastFilter.op.StartsWith("cond_save")) {
				addFilter("", "cond_save_or", "");
			} else if (lastFilter.op.StartsWith("cond_updatekey")) {
				addFilter("", "cond_updatekey_or", "");
			} else {
				addFilter("", "or", "");
			}
			return this;
		}
		public Operator getLastFilter() {
			if (filters != null && filters.Count > 0) {
				return filters[filters.Count - 1];
			} else {
				return null;
			}
		}
		public SwecialRequest<T> startSub() {
			addFilter("", "start_sub", "");
			return this;
		}
		public SwecialRequest<T> endSub() {
			addFilter("", "end_sub", "");
			return this;
		}
		public SwecialRequest<T> equalsCurrentMasterUser(string key) {
			addFilter(key, "eq_current_master_user", "");
			return this;
		}
		public SwecialRequest<T> equalsCurrentAppUser(string key) {
			addFilter(key, "eq_current_app_user", "");
			return this;
		}
		public SwecialRequest<T> equals(string key, SwecialObject value) {
			addFilter(key, "eq_obj", value.id);
			return this;
		}
		public SwecialRequest<T> hasValue(string key) {
			addFilter(key, "has_value", "");
			return this;
		}
		public SwecialRequest<T> noValue(string key) {
			addFilter(key, "no_value", "");
			return this;
		}
		public SwecialRequest<T> notEquals(string key, SwecialObject value) {
			addFilter(key, "not_eq_obj", value.id);
			return this;
		}
		public SwecialRequest<T> equals(string key, string value) {
			addFilter(key, "eq_str", value);
			return this;
		}
		public SwecialRequest<T> notEquals(string key, string value) {
			addFilter(key, "not_eq_str", value);
			return this;
		}
		public SwecialRequest<T> equals(string key, int value) {
			addFilter(key, "eq_num", value.ToString());
			return this;
		}
		public SwecialRequest<T> equals(string key, bool value) {
			addFilter(key, "eq_bool", value.ToString());
			return this;
		}
		public SwecialRequest<T> greaterThan(string key, DateTimeOffset value) {
			addFilter(key, "gt_date", value.ToString("o"));
			return this;
		}
		public SwecialRequest<T> greaterThan(string key, int value) {
			addFilter(key, "gt_num", value.ToString());
			return this;
		}
		public SwecialRequest<T> lessThan(string key, DateTimeOffset value) {
			addFilter(key, "lt_date", value.ToString("o"));
			return this;
		}
		public SwecialRequest<T> lessThan(string key, int value) {
			addFilter(key, "lt_num", value.ToString());
			return this;
		}
		public SwecialRequest<T> lessThanOrEquals(string key, string value) {
			addFilter(key, "lt_or_eq_string", value);
			return this;
		}
		public SwecialRequest<T> containedIn(string key, List<string> list){
			addFilter(key, "in_list_str", JsonConvert.SerializeObject(list));
			return this;
		}
		public SwecialRequest<T> orderByDesc(string key) {
			addFilter(key, "desc", "");
			return this;
		}
		public SwecialRequest<T> orderBy(string key) {
			addFilter(key, "asc", "");
			return this;
		}
        public SwecialRequest<T> limit(int l) {
			addFilter("", "limit", l.ToString());
			return this;
		}
		public SwecialRequest<T> include(string incl) {
			addFilter("", "include", incl);
			return this;
		}
		private void addFilter(string key, string op, string value) {
			if (filters == null) filters = new List<Operator>();
			filters.Add(new Operator(key, op, value));
		}
		public SwecialRequest<T> remap(string oldKey, string newKey) {
			if (filters == null) filters = new List<Operator>();
			filters.Add(new Operator(oldKey, "remap", newKey));
			return this;
		}
		public SwecialRequest<T> execute() {
            if (!isExecuting) {
                isExecuting = true;
                Thread thread = new Thread(executeRequest);
                thread.Start();
            }
			return this;
		}
		private void executeRequest() {
			if (Swecial.current.error == null) {
				while (Swecial.getAppUser() == null && command != "init") {
					Thread.Sleep(100);
				}
				try {
					var stopWatch = System.Diagnostics.Stopwatch.StartNew();
					if (filters != null) {
						// check for special cases, current users may not be instantiated when request was built
						foreach (var filter in filters) {
							if (filter.op == "eq_current_master_user") {
								filter.op = "eq_obj";
								filter.value = Swecial.getMasterUser().id;
							} else if (filter.op == "eq_current_app_user") {
								filter.op = "eq_obj";
								filter.value = Swecial.getAppUser().id;
							}
						}
					}
					if (original == null && o != null) {
						// original is not null if this is a retry. in that case don't transform o to a new requestObject, because it would look unchanged
						// e.g. a retry for INIT would send an empty init object, since no values were changed since last try
						original = o;
						o = original.getRequestObject(); // this is specially formatted to only hold changed values, removed properties and incrementals
					}
					string json = this.toJson();
					using (TcpClient client = new TcpClient()) {
						client.Connect(Swecial.current.apiUrl, Swecial.current.apiPort);
						NetworkStream stream = client.GetStream();
						BinaryReader reader = new BinaryReader(stream, Encoding.UTF8);
						BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8);
						byte[] jsonEncrypted = SwecialCrypto.Encrypt(json, Swecial.key, Swecial.iv);
						if (command == "init") {
							installId = "init";
						}
						string upstreamHeader = "SWECIAL " + Swecial.current.apiAppId + " " + installId + " " + jsonEncrypted.Length.ToString() + " " + Swecial.API_VERSION + " " + Swecial.current.apiAppVersion;
						writer.WriteLine(upstreamHeader);
						if (command == "init") {
							writer.WriteLine(Swecial.rsa.Encrypt(Swecial.key, true).base64Encode());
							writer.WriteLine(Swecial.rsa.Encrypt(Swecial.iv, true).base64Encode());
						}
						writer.Write(jsonEncrypted);
						writer.Flush();

						if (fileLengths != null && fileLengths.Count > 0) {
                            foreach (var byteArray in files) {
                                writer.Write(byteArray);
                                writer.Flush();
                            }
						}
						string header = reader.ReadLine();
						string[] words = header.Split(' ');
						int contentLength = Convert.ToInt32(words[1]);
						byte[] responseBytes = reader.ReadBytes(contentLength);
						requestSize = contentLength;
						stopWatch.Stop();
						loadTime = stopWatch.ElapsedMilliseconds;

						bool isEncrypted = (words[0] == "encrypted");
						string responseString = "";
						if (isEncrypted) {
							responseString = SwecialCrypto.Decrypt(responseBytes, Swecial.key, Swecial.iv);
						} else {
							responseString = Encoding.UTF8.GetString(responseBytes);
						}
                        if (command == "load_file") {
                            Debug.Log("#h");
                            var ty = typeof(T);
                            Debug.Log(ty);
                        }
                        response = SwecialJson.deserialize<SwecialResponse<T>>(responseString);

                        if (command == "load_file") {
                            Debug.Log("load file");
                            var fl1 = response.fileLengths;
                            var fl2 = response.get<List<long>>("fileLengths");
                            Debug.Log("   fl1: " + fl1);
                            Debug.Log("   fl2: " + fl2);
                        }

                        bool succeeded = (response.status == "ok");
						isSucceeded = succeeded;

						if (response.fileLengths != null && response.fileLengths.Count > 0) {
                            foreach (var fileLength in response.fileLengths) {
                                Debug.Log("   loading file: " + fileLength);
                                byte[] rawFileBytes = reader.ReadBytes(fileLength);
                                if (response.files == null) {
                                    response.files = new List<byte[]>();
                                }
                                if (isEncrypted) {
                                    response.files.Add(SwecialCrypto.DecryptBytes(rawFileBytes, Swecial.key, Swecial.iv));
                                } else {
                                    response.files.Add(rawFileBytes);
                                }
                            }
                        }


						hasReadResponse = true;
						if (response.confirm) {
							if (isRunningBeforeConfirm) {
								while (!hasRunBeforeConfirm) {
									Thread.Sleep(1);
								}
							}
							writer.WriteLine("ok");
							writer.Flush();
						}

						reader.Close();
						writer.Close();
						stream.Close();
						client.Close();

                        if (command == "load_file") {
                            Debug.Log("#h");
                        }

						if (succeeded) {
							response.result.linkObjects();
                            if (response.result is SwecialFile) {
                                Debug.Log("   result is SF");
                                Debug.Log("   setting file bytes to SF object");
                                object holder = response.result;
                                ((SwecialFile)holder).bytes = response.files[0];
                            }
							if (command == "save" || command == "load" || command == "create_or_load"
								|| command == "load_file" || command == "save_file") {
								original.copyFrom(response.result);
								original.setAsUnchanged();
							}
							response.result.setAsUnchanged();
						} else {
							if (response.error == "unknown install") {
								if (Swecial.getInstall() != null) {
									if (Swecial.getInstall().id.Equals(installId)) {
										Swecial.removeInstall();
										Swecial.invoke(() => {
											Swecial.current.init();
										});
									}
									command = "";
								}
							} else if (response.error == "unknown encryption key") {
								Swecial.invoke(() => {
									Swecial.current.init();
								});
								command = "";
							}
						}
                        if (succeeded) {
                            if ((command == "login" || command == "signup" || command == "logout")) {
                                Swecial.setUser(response.result);
                                Swecial.invoke(() => {
                                    if (Swecial.onSynced != null) {
                                        Swecial.onSynced();
                                    }
                                });
                            } else if (command == "sync") {
                                object conversionObject = (object)response.result;
                                var syncObject = (SwecialObject)conversionObject;
                                var install = syncObject.get<SwecialObject>("install");
                                var config = syncObject.get<SwecialObject>("config");
                                long ticks = syncObject.get<long>("ticks");
                                if (install != null) {
                                    Swecial.setInstall(install);
                                    Swecial.setConfig(config);
                                    Swecial.current.syncTime(ticks);
                                    if (Swecial.onSynced != null) {
                                        Swecial.invoke(Swecial.onSynced);
                                    }
                                } else {
                                    Swecial.invoke(() => {
                                        Swecial.current.init();
                                    });
                                }
                            } else {
                                if (response.result is SwecialObject) {
                                    object conversionObject = (object)response.result;
                                    var result = (SwecialObject)conversionObject;
                                    if (result.className == "_user") {
                                        if (result.id == Swecial.getMasterUser().id) {
                                            Swecial.setUser(result);
                                        }
                                    } else if (result.className == "_appuser") {
                                        if (result.id == Swecial.getAppUser().id) {
                                            Swecial.setAppUser(result);
                                        }
                                    }
                                }
                            }
                        }
						isCompleted = true;

					}
				} catch (SocketException sex) {
					Debug.Log("Socket Exception");
					Debug.Log(sex);
					if (keepTrying) {
						if (Swecial.isRunning) {
							Debug.Log(command.ToUpper() + " connection failure. retrying in " + retryTimeout + "s");
							Thread.Sleep((int)(retryTimeout * 1000));
							executeRequest();
						} else {
							Debug.Log("dropping thread " + command.ToUpper());
							isCompleted = true;
							isSucceeded = false;
                            if (response == null) {
                                response = new SwecialResponse<T>();
                                response.status = "error";
                            }
							response.error = sex.Message;
						}
					} else {
						isCompleted = true;
						isSucceeded = false;
                        if (response == null) {
                            response = new SwecialResponse<T>();
                            response.status = "error";
                        }
						response.error = sex.Message;
					}
				} catch (EndOfStreamException eosex) {
					Debug.Log("End Of Stream Exception");
					Debug.Log(eosex);
					Swecial.invoke(() => { Swecial.current.init(); });
				} catch (Exception ex) {
					Debug.Log("SwecialRequest.execute() - EXCEPTION");
					Debug.Log(ex);
				}
			} else {
				isCompleted = true;
				isSucceeded = false;
                if (response == null) {
                    response = new SwecialResponse<T>();
                    response.status = "error";
                }
				response.error = "init error";
			}
			if (command == "logout" && !isSucceeded) {
				Swecial.getInstall().set("_user", null);
				Prefs.setString("swecial_refresh_token", "");
			}
		}
	}
}
