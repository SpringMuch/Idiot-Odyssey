using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class SaveSystemExtensions
{
    private const string BackupSuffix = ".bak";
    private const string VersionKey = "v1";

    private static string BackupPath(string path) => path + BackupSuffix;

    public static void SaveSecure(PlayerProgress data, bool encrypt = false, string password = null)
    {
        string json = JsonUtility.ToJson(data, true);
        string payload = VersionKey + "\n" + json;

        if (encrypt)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password required for encryption");
            payload = EncryptString(payload, password);
        }

        string path = SaveSystem.GetSavePath();
        string backup = BackupPath(path);

        try
        {
            if (File.Exists(path))
            {
                File.Copy(path, backup, true);
            }

            string temp = path + ".tmp";
            File.WriteAllText(temp, payload);
            if (File.Exists(path)) File.Delete(path);
            File.Move(temp, path);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveSystemExtensions] SaveSecure failed: {ex}");
            try
            {
                if (File.Exists(backup))
                {
                    File.Copy(backup, path, true);
                    Debug.Log("[SaveSystemExtensions] Restored from backup.");
                }
            }
            catch (Exception e2)
            {
                Debug.LogError($"[SaveSystemExtensions] Backup restore failed: {e2}");
            }
        }
    }

    public static PlayerProgress LoadSecure(bool encrypted = false, string password = null)
    {
        string path = SaveSystem.GetSavePath();
        if (!File.Exists(path)) return null;

        try
        {
            string raw = File.ReadAllText(path);
            string content = raw;
            if (encrypted)
            {
                if (string.IsNullOrEmpty(password)) throw new ArgumentException("Password required for decryption");
                content = DecryptString(raw, password);
            }

            var idx = content.IndexOf('\n');
            if (idx <= 0)
            {
                Debug.LogWarning("[SaveSystemExtensions] Save file missing version header.");
                return JsonUtility.FromJson<PlayerProgress>(content);
            }

            var version = content.Substring(0, idx);
            var json = content.Substring(idx + 1);

            if (version != VersionKey)
            {
                Debug.LogWarning($"[SaveSystemExtensions] Save version mismatch: {version}");
            }

            var data = JsonUtility.FromJson<PlayerProgress>(json);
            return data;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveSystemExtensions] LoadSecure failed: {ex}");
            return null;
        }
    }

    public static void DeleteBackup()
    {
        string path = SaveSystem.GetSavePath();
        string backup = BackupPath(path);
        try
        {
            if (File.Exists(backup)) File.Delete(backup);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveSystemExtensions] DeleteBackup failed: {ex}");
        }
    }

    #region AES helpers
    private static string EncryptString(string plainText, string password)
    {
        using (var aes = Aes.Create())
        {
            var key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password));
            aes.Key = key;
            aes.GenerateIV();
            using (var ms = new MemoryStream())
            {
                ms.Write(aes.IV, 0, aes.IV.Length);
                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    private static string DecryptString(string cipherTextBase64, string password)
    {
        var cipher = Convert.FromBase64String(cipherTextBase64);
        using (var aes = Aes.Create())
        {
            var key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password));
            aes.Key = key;
            using (var ms = new MemoryStream(cipher))
            {
                byte[] iv = new byte[16];
                ms.Read(iv, 0, iv.Length);
                aes.IV = iv;
                using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
    #endregion
}