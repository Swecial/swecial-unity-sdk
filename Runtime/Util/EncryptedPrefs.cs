using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Threading;
using com.swecial.unity;
using Newtonsoft.Json;

namespace com.swecial.unity {
	public class EncryptedPrefs {

		private static string fileName;
		private Dictionary<string, string> store;
		public bool isLoaded;
		public bool error;
		private string installId;

		public EncryptedPrefs() {
			fileName = Path.Combine(Application.persistentDataPath, "data.dat");
			isLoaded = false;
		}
        public void delete() {
            if (!usePlayerPrefs()) {
                File.Delete(fileName);
            }
        }
        public void load() {
            if (usePlayerPrefs()) {
                // nothing, using playerprefs
            } else {
                Debug.Log("loading prefs");
                string jsonStringEncrypted = "";
                try {
                    jsonStringEncrypted = PlayerPrefs.GetString("data", "");
                    if (H.isEmpty(jsonStringEncrypted)) {
                        Debug.Log("reading data from file " + fileName);
                        if (File.Exists(fileName)) {
                            StreamReader sr = new StreamReader(fileName, Encoding.UTF8);
                            jsonStringEncrypted = sr.ReadToEnd();
                            sr.Close();
                        } else {
                            Debug.Log("file didn't exist");
                        }
                    } else {
                        Debug.Log("reading data from pp");
                        File.Delete(fileName);
                    }
                } catch {
                    Debug.Log("error");
                    Swecial.current.showMessage("ERROR", "SETTINGS CORRUPTED");
                    File.Delete(fileName);
                }
                try {
                    if (!H.isEmpty(jsonStringEncrypted)) {
                        string jsonString = SwecialCrypto.decrypt(jsonStringEncrypted);
                        store = jsonString.deserialize<Dictionary<string, string>>();
                        if (store == null) {
                            Debug.Log("store was null, creating new");
                            store = new Dictionary<string, string>();
                            PlayerPrefs.SetInt("coins", 0);
                            PlayerPrefs.Save();
                        }
                    } else {
                        if (store == null) {
                            Debug.Log("data was empty and store was null, creating new");
                            store = new Dictionary<string, string>();
                            PlayerPrefs.SetInt("coins", 0);
                            PlayerPrefs.Save();
                        }
                    }
                } catch (System.Exception ex) {
                    Debug.Log(ex.Message + "\n" + ex.StackTrace);
                    Swecial.current.showMessage("ERROR", "SETTINGS CORRUPTED");
                    if (store == null) store = new Dictionary<string, string>();
                }
            }
            isLoaded = true;
        }
        public void resetInstallId() {
			installId = "";
			getInstallId();
		}
		public string getInstallId() {
			if (H.isEmpty(installId)) {
				installId = PlayerPrefs.GetString("install_id", "");
				if (installId.isEmpty()) {
					installId = Guid.NewGuid().ToString();
					PlayerPrefs.SetString("install_id", installId);
				}
			}
			return installId;
		}
		public bool encryptedStoreExists() { 
			return PlayerPrefs.GetString("data", "") != "" || File.Exists(fileName);
		}
		public string GetString(string key, string defaultValue) {
			if (usePlayerPrefs()) {
				return PlayerPrefs.GetString(key, defaultValue);
			} else {
				if (keyInPlayerPrefs(key)) {
					return PlayerPrefs.GetString(key, defaultValue);
				} else {
					if (PlayerPrefs.HasKey(key)) PlayerPrefs.DeleteKey(key);
					string retVal;
					if (store.TryGetValue(key, out retVal)) {
						return retVal;
					} else {
						return defaultValue;
					}
				}
			}
		}
		public void SetString(string key, string value) {
			if (usePlayerPrefs()) {
				PlayerPrefs.SetString(key, value);
			} else {
				if (keyInPlayerPrefs(key)) {
					PlayerPrefs.SetString(key, value);
					PlayerPrefs.Save();
				} else {
					store[key] = value;
					PlayerPrefs.DeleteKey(key);
				}
			}
		}
		public int GetInt (string key, int defaultValue) {
			if (usePlayerPrefs()) {
				return PlayerPrefs.GetInt(key, defaultValue);
			} else {
				int retInt = 0;
				if (int.TryParse(GetString(key, defaultValue.ToString()), out retInt)) {
					if (key == "com.outlinegames.unibill.currencies.coins.balance") {
						int ppCoins = PlayerPrefs.GetInt("coins", 0);
						//Debug.Log("encCoins: " + retInt + " - ppCoins: " + ppCoins);
						if (ppCoins > retInt) {
							PlayerPrefs.SetInt("coins", retInt);
							save();
							//SwecialConnect.instance.logCheat("coins", "old coins: " + retInt, ppCoins);
						}
					}
					return retInt;
				} else {
					return defaultValue;
				}
			}
		}
		public void AddInt(string key, int value) {
			if (usePlayerPrefs()) {
				PlayerPrefs.SetInt(key, PlayerPrefs.GetInt(key, 0) + value);
			} else {
				int oldValue = GetInt(key, 0);
				int newValue = oldValue + value;
				SetInt(key, newValue);
			}
		}
		public void SetInt(string key, int value) {
			if (usePlayerPrefs()) {
				PlayerPrefs.SetInt(key, value);
			} else {
				if (key == "com.outlinegames.unibill.currencies.coins.balance") {
					PlayerPrefs.SetInt("coins", value);
					SetString(key, value.ToString());
					save();
				} else {
					SetString(key, value.ToString());
				}
			}
		}
		public float GetFloat(string key, float defaultValue) {
			float retFloat = 0;
			if (float.TryParse(GetString(key, defaultValue.ToString()), out retFloat)) {
				return retFloat;
			} else {
				return defaultValue;
			}
		}
		public void SetFloat(string key, float value) {
			SetString(key, value.ToString());
		}
		public bool GetBool(string key, bool defaultValue) {
			bool retBool = false;
			if (bool.TryParse(GetString(key, defaultValue.ToString()), out retBool)) {
				return retBool;
			} else {
				return defaultValue;
			}
		}
		public void SetBool(string key, bool value) {
			SetString(key, value.ToString());
		}
		public void save() {
			if (usePlayerPrefs()) {
				PlayerPrefs.Save();
			} else {
				Debug.Log("saving prefs...");
				bool externalSave = false;

				string jsonString = JsonConvert.SerializeObject(store);
				string jsonStringEncrypted = SwecialCrypto.encrypt(jsonString);

				int byteSize = jsonStringEncrypted.Length * sizeof(char);
				if (byteSize > 900000) externalSave = true;

				if (externalSave) {
					File.WriteAllText(fileName, jsonStringEncrypted, Encoding.UTF8);
					PlayerPrefs.DeleteKey("data");
					PlayerPrefs.Save();
				} else {
					PlayerPrefs.SetString("data", jsonStringEncrypted);
					PlayerPrefs.Save();
				}
			}
		}
		public bool keyInPlayerPrefs(string key) {
			return (key == "install_id" || key == "url");
		}
		public bool usePlayerPrefs() {
			if (Swecial.current != null) {
				return Swecial.current.isDevVersion;
			} else {
				return false;
			}
		}
	}
}
