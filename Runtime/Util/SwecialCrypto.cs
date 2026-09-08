using System;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using UnityEngine;

namespace com.swecial.unity {
	public static class SwecialCrypto {

		private const string initVector = "tu89geij341t89u2jfjjjdjsiuyjfkms";
		
		// This constant is used to determine the keysize of the encryption algorithm.
		private const int blockSize = 256;

		public static bool keyOk() {
			string key = Prefs.getInstallId();
			string oldKey = PlayerPrefs.GetString("install_id", "");
			return key == oldKey;
		}
		public static string encrypt(string plainText) {
			return Encrypt(plainText, Swecial.dinsdale + Prefs.getInstallId());
		}
		public static string decrypt(string encryptedText) {
			return Decrypt(encryptedText, Swecial.dinsdale + Prefs.getInstallId());
		}
		public static string Encrypt(string plainText, string passPhrase) {
			return Encrypt(plainText, passPhrase, initVector);
		}
		public static string Encrypt(string plainText, string passPhrase, string iv) {
			byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
			byte[] keyBytes = getKey(passPhrase);
			byte[] cipherTextBytes = Encrypt(plainText, keyBytes, ivBytes);
			return Convert.ToBase64String(cipherTextBytes);
		}
		public static byte[] Encrypt(string plainText, byte[] keyBytes, byte[] ivBytes) {
			byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
			return Encrypt(plainTextBytes, keyBytes, ivBytes);
		}
		public static byte[] Encrypt(byte[] plainBytes, byte[] keyBytes, byte[] ivBytes) {
			RijndaelManaged symmetricKey = new RijndaelManaged();
			symmetricKey.Mode = CipherMode.CBC;
			symmetricKey.BlockSize = blockSize;
			ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, ivBytes);
			MemoryStream memoryStream = new MemoryStream();
			CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
			cryptoStream.Write(plainBytes, 0, plainBytes.Length);
			cryptoStream.FlushFinalBlock();
			byte[] cipherTextBytes = memoryStream.ToArray();
			memoryStream.Close();
			cryptoStream.Close();
			return cipherTextBytes;
		}
		public static string Decrypt(string cipherText, string passPhrase) {
			return Decrypt(cipherText, passPhrase, initVector);
		}
		public static string Decrypt(string cipherText, string passPhrase, string initVector) {
			byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
			byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
			byte[] keyBytes = getKey(passPhrase);
			return Decrypt(cipherTextBytes, keyBytes, initVectorBytes);
		}
		public static string Decrypt(string cipherText, byte[] keyBytes, byte[] initVectorBytes) {
			byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
			return Decrypt(cipherTextBytes, keyBytes, initVectorBytes);
		}
		public static string Decrypt(byte[] cipherTextBytes, byte[] keyBytes, byte[] ivBytes) {
			var plainTextBytes = DecryptBytes(cipherTextBytes, keyBytes, ivBytes);
			return Encoding.UTF8.GetString(plainTextBytes, 0, plainTextBytes.Length);
		}
		public static byte[] DecryptBytes(byte[] cipherTextBytes, byte[] keyBytes, byte[] ivBytes) {
			RijndaelManaged symmetricKey = new RijndaelManaged();
			symmetricKey.Mode = CipherMode.CBC;
			symmetricKey.BlockSize = blockSize;
			ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, ivBytes);
			MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
			CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
			byte[] plainTextBytes = new byte[cipherTextBytes.Length];
			int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
			memoryStream.Close();
			cryptoStream.Close();
			byte[] truncated = new byte[decryptedByteCount];
			Array.Copy(plainTextBytes, truncated, decryptedByteCount);
			return truncated;
		}
		public static byte[] getKey(string passPhrase) {
			Rfc2898DeriveBytes password = new Rfc2898DeriveBytes(passPhrase, new byte[8]);
			//PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
			byte[] keyBytes = password.GetBytes(blockSize / 8);
			return keyBytes;
		}
		public static byte[] generateIV() {
			return generateKey(blockSize / 8);
		}
		public static byte[] generateKey(int size) {
			System.Random rnd = new System.Random();
			byte[] key = new byte[size];
			for (int i = 0; i < key.Length; i++) {
				key[i] = Convert.ToByte(rnd.Next(0, 256));
			}
			return key;
		}
	}
}