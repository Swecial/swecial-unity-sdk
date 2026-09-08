using UnityEngine;
using System.Collections;

namespace com.swecial.unity {
	public class Prefs {

		public static string removeAdsBonus = "remove_all_ads_bonus";
		public static string removeAdsTemp = "remove_all_ads_temp";
		public static string adIntervalGeneral = "ad_interval_general";
		public static string adIntervalVideo = "ad_interval_video";
		public static string adFirstDelayGeneral = "ad_first_delay_general";
		public static string adFirstDelayVideo = "ad_first_delay_video";
		public static string playDelay = "play_delay";
		public static string extendExitCoins = "extend_exit_coins";
		public static string extendExitMoves = "extend_exit_moves";
		public static string maxPlays = "max_plays";
		public static string playCoins = "play_coins";
		public static string boostExitOdds = "boost_exit_odds";
		public static string boostExitCoins = "boost_exit_coins";
		public static string boostLastMoveCoins = "last_move_coins";
		public static string playTimerStarted = "play_timer_started";
		public static string unityAdsDefaultZone = "rewardedVideo";
		public static string adInterstitialTimer = "ad_interstitial_timer";
		public static string dailyWinCoins = "daily_win_coins";
		private static EncryptedPrefs store;

		public static long getLong(string key, long defaultValue) {
			init();
			string s = store.GetString(key, defaultValue.ToString());
			return long.Parse(s);
		}
		public static void setLong(string key, long value) {
			init();
            string s = value.ToString();
			store.SetString(key, s);
		}
		public static ulong getULong(string key, ulong defaultValue) {
			init();
			string s = store.GetString(key, defaultValue.ToString());
			return ulong.Parse(s);
		}
		public static void setULong(string key, ulong value) {
			init();
			store.SetString(key, value.ToString());
		}
		public static string getString(string key, string defaultValue) {
			init();
			return store.GetString(key, defaultValue);
		}
		public static void setString(string key, string value) {
			init();
			store.SetString(key, value);
		}
		public static int getInt(string key, int defaultValue) {
			init();
			return store.GetInt(key, defaultValue);
		}
		public static void setInt(string key, int value) {
			init();
			store.SetInt(key, value);
		}
		public static void addInt(string key, int value) {
			init();
			store.AddInt(key, value);
		}
		public static float getFloat(string key, float defaultValue) {
			init();
			return store.GetFloat(key, defaultValue);
		}
		public static void setFloat(string key, float value) {
			init();
			store.SetFloat(key, value);
		}
		public static bool getBool(string key, bool defaultValue) {
			init();
			return store.GetBool(key, defaultValue);
		}
		public static bool getBool(string key) {
			return getBool(key, false);
		}
		public static void setBool(string key, bool value) {
			init();
			store.SetBool(key, value);
		}
		public static void save() {
			init();
			store.save();
		}
		public static EncryptedPrefs getStore() {
			init();
			return store;
		}
		public static string getInstallId() {
			if (store == null) init();
			return store.getInstallId();
		}
        public static void delete() {
            if (store != null) {
                store.delete();
            }
        }
		public static void init() {
			if (store == null) {
				store = new EncryptedPrefs();
				store.load();
			}
		}
	}
}