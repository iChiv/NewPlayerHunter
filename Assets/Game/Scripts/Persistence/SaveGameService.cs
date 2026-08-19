using System;
using UnityEngine;

namespace NewPlayerHunter.Persistence
{
    public sealed class SaveGameService
    {
        public const int CurrentSchemaVersion = 1;
        public const string SaveKey = "newplayerhunter.progress";

        private readonly ES3Settings _settings;

        public SaveGameService()
        {
        }

        public SaveGameService(ES3Settings settings)
        {
            _settings = settings;
        }

        public bool HasProgress =>
            _settings == null
                ? ES3.KeyExists(SaveKey)
                : ES3.KeyExists(SaveKey, _settings);

        public void Save(GameProgressSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            snapshot.schemaVersion = CurrentSchemaVersion;
            var json = JsonUtility.ToJson(snapshot);
            if (_settings == null)
            {
                ES3.Save(SaveKey, json);
            }
            else
            {
                ES3.Save(SaveKey, json, _settings);
            }
        }

        public bool TryLoad(out GameProgressSnapshot snapshot)
        {
            snapshot = null;
            if (!HasProgress)
            {
                return false;
            }

            var json = _settings == null
                ? ES3.Load<string>(SaveKey)
                : ES3.Load<string>(SaveKey, _settings);

            GameProgressSnapshot parsed;
            try
            {
                parsed = JsonUtility.FromJson<GameProgressSnapshot>(json);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"[NewPlayerHunter] 存档内容无法解析,已放弃旧存档：{exception.Message}");
                return false;
            }

            if (parsed == null)
            {
                Debug.LogError("[NewPlayerHunter] 存档内容为空,已放弃旧存档。");
                return false;
            }

            if (parsed.schemaVersion != CurrentSchemaVersion)
            {
                Debug.LogWarning(
                    $"[NewPlayerHunter] 存档版本 {parsed.schemaVersion} 与当前版本 {CurrentSchemaVersion} 不匹配,已放弃旧存档。");
                return false;
            }

            snapshot = parsed;
            return true;
        }

        public void DeleteProgress()
        {
            if (_settings == null)
            {
                ES3.DeleteKey(SaveKey);
            }
            else
            {
                ES3.DeleteKey(SaveKey, _settings);
            }
        }
    }
}
