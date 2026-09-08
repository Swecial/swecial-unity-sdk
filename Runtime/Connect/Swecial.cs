using UnityEngine;
using System.Collections;
using System.Security.Cryptography;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using com.swecial.unity;

public class Swecial : MonoBehaviour {

	internal static readonly int API_VERSION = 3;

	public string apiKey;
	public string apiProductionKey;
	public string apiDevelopmentKey;
	public string apiAppId;
    public int apiAppVersion;
	public string apiProductionUrl;
	public int apiProductionPort;
	public string apiDevelopmentUrl;
	public int apiDevelopmentPort;
	public string[] userIncludes;
    public int stillHereInterval;
    public bool isDevVersion;


    internal string apiUrl;
	internal int apiPort;

	public static Swecial current;
	public static string dinsdale = "larksvomit";
	public static byte[] key, iv;
	public static string installId = null;
	public static string platform = null;
	public static float retryTimeout = 5f;
	public static bool keepTrying = true;

	private static bool isTimeSynced, isTimeSyncing, isInitializing, isInitialized;
	private static long timeDiff;

	public static string systemLanguage;

	public Action<string, string> showMessageAction;

	internal string error;

	public static RSACryptoServiceProvider rsa;
	private static SwecialObject install;
	private static SwecialObject config;

	private static List<Action> actions;

	public static bool isRunning;
	public static Action onSynced;
    public static Func<string, bool> onDeepLink;
	public static Action<string> onDeprecatedApiVersion;

    void Awake () {
		DontDestroyOnLoad(this);
		isRunning = true;
		systemLanguage = Application.systemLanguage.ToString();
		installId = PlayerPrefs.GetString("swecial_install_id", "");
		platform = Application.platform.ToString();
		actions = new List<Action>();
		Swecial.current = this;
		if (isDevVersion) {
			apiUrl = apiDevelopmentUrl;
			apiPort = apiDevelopmentPort;
			if (!string.IsNullOrEmpty(apiDevelopmentKey)) {
				apiKey = apiDevelopmentKey;
			}
		} else {
			apiUrl = apiProductionUrl;
			apiPort = apiProductionPort;
			if (!string.IsNullOrEmpty(apiProductionKey)) {
				apiKey = apiProductionKey;
			}
		}

        if (!apiKey.isEmpty()) {
            rsa = new RSACryptoServiceProvider();
            rsa.PersistKeyInCsp = false;
            string xml = apiKey.base64Decode();
            rsa.FromXmlString(xml);
        }

		init();

        if (stillHereInterval > 0) {
            InvokeRepeating("runStillHere", stillHereInterval, stillHereInterval);
        }
	}
    void Start() {
        checkDeepLink();
    }
    void OnApplicationPause(bool isPaused) {
        if (isPaused) {
            isTimeSynced = false;
        } else {
            if (current == null) {
                current = this;
            }
            if (isInitialized) {
                sync();
            }
            checkDeepLink();
        }
    }
    void OnDestroy() {
        isRunning = false;
        Debug.Log("Swecial is now destroyed. Every thread should stop trying to connect");
    }
    void checkDeepLink() {
        Debug.Log("checkDeepLink");
        string url = PlayerPrefs.GetString("com.swecial.unity.deeplink.url", "");
        if (url != "") {
            if (onDeepLink != null) {
                if (onDeepLink(url)) {
                    PlayerPrefs.SetString("com.swecial.unity.deeplink.url", "");
                    PlayerPrefs.Save();
                } else {
                    Swecial.invoke(checkDeepLink, 0.5f);
                }
            } else {
                PlayerPrefs.SetString("com.swecial.unity.deeplink.url", "");
                PlayerPrefs.Save();
            }
        }
    }
    void runStillHere() {
        if (!apiAppId.isEmpty()) {
            if (isSynced) {
                Swecial.stillHere().execute();
            }
        }
    }
	void Update() {
		if (actions != null && actions.Count > 0) {
			lock (actions) {
				Action action = actions[0];
				actions.Remove(action);
				action();
			}
		}
	}
	public static bool isSynced {
		get {
			return isTimeSynced && getMasterUser() != null && Swecial.time() != DateTimeOffset.MinValue;
		}
	}
	public static double getLocalUTCOffset() {
        return TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalHours;
	}
	public static DateTimeOffset getLocalTime() {
		if (time() > DateTimeOffset.MinValue) {
			return time().AddHours(getLocalUTCOffset());
		} else {
			return DateTimeOffset.MinValue;
		}
	}
	public static void invoke(Action a) {
		lock (actions) {
			actions.Add(a);
		}
	}
	public static void invoke(Action a, float delay) {
		Swecial.current.StartCoroutine(Swecial.current.crInvoke(a, delay));
	}
	IEnumerator crInvoke(Action action, float delay) {
		yield return new WaitForSeconds(delay);
		action();
	}
	public static string appUserKey {
		get {
			return "_appUser_" + current.apiAppId;
		}
	}
	public void showMessage (string heading, string msg) {
		if (showMessageAction != null) {
			showMessageAction(heading, msg);
		}
	}
	public static void setInstall(object so) {
		if (so is SwecialObject) {
			install = (SwecialObject)so;
		}
	}
	public static void setUser(object so) {
		if (so is SwecialObject) {
			install.set("_user", (SwecialObject)so);
		}
	}
	public static void setAppUser(SwecialObject so) {
		var masterUser = getMasterUser();
		if (masterUser != null) {
			so.set("_user", masterUser);
			masterUser.set(appUserKey, so);
		}
	}
	public static SwecialObject getMasterUser() {
		if (install != null) {
			var mu = install.get<SwecialObject>("_user");
			return mu;
		} else {
			return null;
		}
	}
	public static SwecialObject masterUser {
		get {
			return getMasterUser();
		}
	}
	public static SwecialObject appUser {
		get {
			return getAppUser();
		}
		set {
			setAppUser(value);
		}
	}
	public static SwecialObject getAppUser() {
		if (install != null) {
			var masterUser = getMasterUser();
			if (masterUser != null) {
				var appUser = masterUser.get<SwecialObject>(appUserKey);
				return appUser;
			} else {
				return null;
			}
		} else {
			return null;
		}
	}
	public static SwecialObject getConfig() {
		if (config != null) {
			return config;
		} else {
			return null;
		}
	}
	public static void setConfig(SwecialObject so) {
		config = so;
	}
	public static bool isAutoUser() {
		var user = getMasterUser();
		if (user != null) {
			return user.get<bool>("_autoUser");
		} else {
			return false;
		}
	}
	public static SwecialObject getInstall() {
		return install;
	}
	public static void removeInstall() {
		install = null;
	}
	public static SwecialRequest<SwecialObject> logout() {
		var req = new SwecialRequest<SwecialObject>();
		req.command = "logout";
		if (Swecial.current.userIncludes.Length > 0) {
			foreach (string include in Swecial.current.userIncludes) {
				req.include(appUserKey + "." + include);
			}
		}
		return req.execute();
	}
	public static SwecialRequest<SwecialObject> login(string username, string password) {
		SwecialRequest<SwecialObject> req = new SwecialRequest<SwecialObject>();
		var loginObject = new SwecialObject("_user");
		Debug.Log("loginObject: " + loginObject);
		loginObject.set("_username", username);
		loginObject.set("_password", password);
		req.command = "login";
		if (Swecial.current.userIncludes.Length > 0) {
			foreach (string include in Swecial.current.userIncludes) {
				req.include(appUserKey + "." + include);
			}
		}
		req.setObject(loginObject);
		return req.execute();
	}
	public static SwecialRequest<SwecialObject> fbLogin(string facebookId, string email, string name) {
		SwecialRequest<SwecialObject> req = new SwecialRequest<SwecialObject>();
		var loginObject = new SwecialObject("_user");
		Debug.Log("loginObject: " + loginObject);
		loginObject.set("facebookId", facebookId);
		loginObject.set("name", name);
		loginObject.set("email", email);
		req.command = "login";
		if (Swecial.current.userIncludes.Length > 0) {
			foreach (string include in Swecial.current.userIncludes) {
				req.include(appUserKey + "." + include);
			}
		}
		req.setObject(loginObject);
		return req.execute();
	}
	public static SwecialRequest<SwecialObject> signup(string name, string username, string password) {
		return signup(name, username, password, false);
	}
	public static SwecialRequest<SwecialObject> signup(string name, string username, string password, bool newsLetter) {
		SwecialRequest<SwecialObject> req = new SwecialRequest<SwecialObject>();
		SwecialObject loginObject = new SwecialObject("_user");
		loginObject.set("_username", username);
		loginObject.set("_password", password);
		loginObject.set("name", name);
		loginObject.set("newsLetter", newsLetter);
		req.command = "signup";
		if (Swecial.current.userIncludes.Length > 0) {
			foreach (string include in Swecial.current.userIncludes) {
				req.include(appUserKey + "." + include);
			}
		}
		req.setObject(loginObject);
		return req.execute();
	}
	public static SwecialRequest<SwecialObject> resetPassword(string email) {
		SwecialRequest<SwecialObject> req = new SwecialRequest<SwecialObject>();
		req.command = "reset_password";
		SwecialObject so = new SwecialObject("_user");
		so.set("_username", email);
		req.setObject(so);
		return req.execute();
	}
	public static SwecialRequest<SwecialObject> sendVerificationEmail() {
		SwecialRequest<SwecialObject> req = new SwecialRequest<SwecialObject>();
		req.command = "send_verification_email";
		if (getMasterUser() != null) {
			req.setObject(getMasterUser());
			return req.execute();
		} else {
			req.isCompleted = true;
			req.isSucceeded = false;
            req.response = new SwecialResponse<SwecialObject>();
            req.response.status = "error";
			req.response.error = "no master user";
			return req;
		}
	}
	internal void syncTime(long ticks) {
		DateTimeOffset serverTime = new DateTimeOffset(ticks, TimeSpan.Zero);
		timeDiff = (serverTime - DateTimeOffset.UtcNow).Ticks;
		isTimeSynced = true;
	}
	public DateTimeOffset getTime() {
		if (isTimeSynced) {
			var timeToReturn = DateTimeOffset.UtcNow.Add(new TimeSpan(timeDiff));
			return timeToReturn;
		} else {
			return DateTimeOffset.MinValue;
		}
	}
	public static DateTimeOffset localTime() {
		if (Swecial.current != null) {
			return Swecial.current.getTime().ToLocalTime();
		} else {
			return DateTimeOffset.MinValue;
		}
	}
	public static DateTimeOffset time() {
		if (Swecial.current != null) {
			return Swecial.current.getTime();
		} else {
			return DateTimeOffset.MinValue;
		}
	}
	internal void init() {
        if (!apiAppId.isEmpty()) {
            if (!isInitializing) {
                isInitializing = true;
                string prevError = error;
                var req = new SwecialRequest<SwecialObject>();
                req.command = "init";
                SwecialObject io = new SwecialObject();

                key = SwecialCrypto.generateKey(16);
                iv = SwecialCrypto.generateIV();

                io.set("refreshToken", Prefs.getString("swecial_refresh_token", ""));
                io.set("platform", platform);
                string installIdToSend = installId;
                if (prevError == "corrupt install") {
                    installIdToSend = installId + " corrupt";
                }
                io.set("installId", installIdToSend);
                io.set("systemLanguage", systemLanguage);
                io.set("utcOffset", getLocalUTCOffset());
                foreach (string include in Swecial.current.userIncludes) {
                    req.include("install._user." + appUserKey + "." + include);
                }
                req.setObject(io);
                req
                    .execute()
                    .beforeConfirm(t => {
                        if (t.isSucceeded) {
                            install = t.response.result.get<SwecialObject>("install");
                            if (install != null) {
                                install.linkObjects();
                                var installUser = install.get<SwecialObject>("_user");
                                if (installUser != null) {
                                    var config = t.response.result.get<SwecialObject>("config");
                                    setConfig(config);
                                    syncTime(t.response.result.get<long>("ticks"));
                                    installId = install.id;
                                    PlayerPrefs.SetString("swecial_install_id", installId);
                                    PlayerPrefs.Save();
                                    var refreshToken = install.get<string>("_refreshToken");
                                    Prefs.setString("swecial_refresh_token", refreshToken);
                                    Prefs.save();
                                    Debug.Log("Swecial initialization completed");
                                    isInitialized = true;
                                    if (Swecial.onSynced != null) {
                                        Swecial.invoke(Swecial.onSynced);
                                    }
                                } else {
                                    error = "corrupt install";
                                    Debug.Log(error);
                                }
                            } else {
                                error = "no install";
                                Debug.Log(error);
                            }
                        }
                    })
                    .runAfter(t => {
                        isInitializing = false;
                        if (t.isSucceeded) {
                            Debug.Log("init is ok");
                        } else {
                            install = null;
                            Debug.Log(" <- " + t.response.error);
                            Debug.Log("error: " + t.response.error);
                            if (t.response.error == "deprecated_api") {
                                if (onDeprecatedApiVersion != null) {
                                    onDeprecatedApiVersion(t.response.message);
                                }
                            }
                        }
                    });
            }
        }
	}
	public static SwecialRequest<SwecialObject> sync() {
        if (!Swecial.current.apiAppId.isEmpty()) {
            var req = new SwecialRequest<SwecialObject>();
            req.command = "sync";
            var oldUtcOffset = install.get<double>("_utcOffset");
            if (oldUtcOffset != getLocalUTCOffset()) {
                install.set("_utcOffset", getLocalUTCOffset());
            }
            req.setObject(install);
            req.include("config");
            req.include("install._user." + appUserKey);
            foreach (var userInclude in current.userIncludes) {
                req.include("install._user." + appUserKey + "." + userInclude);
            }
            isTimeSynced = false;
            return req.execute();
        } else {
            return null;
        }
	}
    public static SwecialRequest<SwecialObject> stillHere() {
        if (!Swecial.current.apiAppId.isEmpty()) {
            var req = new SwecialRequest<SwecialObject>();
            req.command = "still_here";
            return req;
        } else {
            return null;
        }
    }
	public static SwecialRequest<SwecialObject> code(string method, SwecialObject param) {
		var req = new SwecialRequest<SwecialObject>();
		req.command = "code:" + method;
		req.setObject(param);
		return req.include("list");
	}
	public static SwecialRequest<SwecialObject> code(string method) {
		return code(method, null);
	}
	[Obsolete("Use Swecial.code instead")]
	public static SwecialRequest<SwecialObject> run(string method, SwecialObject param) {
		return code(method, param);
	}
	public static SwecialRequest<List<T>> list<T>(string className) {
		var req = new SwecialRequest<List<T>>();
		req.command = "list";
		req.setObject(new SwecialObject(className));
		return req;
	}
	public static SwecialRequest<SwecialObject> sendPush(SwecialObject user, string title, string message) {
		return sendPush(user, title, message, null);
	}
	public static SwecialRequest<SwecialObject> sendPush(SwecialObject user, string title, string message, string largeIcon) {
		var req = new SwecialRequest<SwecialObject>();
		req.command = "push";
		var pushObject = new SwecialObject("push");
		pushObject.set("user", user);
		pushObject.set("title", title);
		pushObject.set("message", message);
		if (!largeIcon.isEmpty()) {
			pushObject.set("large-icon", largeIcon);
		}
		req.setObject(pushObject);
		req.execute();
		return req;
	}
	public static SwecialRequest<List<SwecialObject>> list(string className) {
		var req = new SwecialRequest<List<SwecialObject>>();
		req.command = "list";
		req.setObject(new SwecialObject(className));
		return req;
	}
	public IEnumerator crRunAfter<T>(SwecialRequest<T> req, Action<SwecialRequest<T>> action) {
		while (!req.isCompleted) {
			yield return null;
		}
		action(req);
	}
	public IEnumerator crBeforeConfirm<T>(SwecialRequest<T> req, Action<SwecialRequest<T>> action) {
		while (!req.hasReadResponse) {
			yield return null;
		}
		action(req);
		req.hasRunBeforeConfirm = true;
	}
	public IEnumerator crProgress<T>(SwecialRequest<T> req, Action<SwecialRequest<T>> action) {
		while (!req.isCompleted) {
			action(req);
			yield return null;
		}
	}
	public static void waitForSync(Action runAfter) {
		Swecial.current.StartCoroutine(Swecial.current.crWaitForSync(runAfter));
	}
	public IEnumerator crWaitForSync(Action runAfter) {
		while (!isTimeSynced || Swecial.current.getTime() == DateTimeOffset.MinValue || Swecial.getAppUser() == null) {
			yield return null;
		}
		runAfter();
	}
}
