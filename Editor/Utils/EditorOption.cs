using UnityEditor;
using UnityEngine;


namespace AIBridge.Editor
{
    public class EditorOption<T>
    {
        private readonly string _key;
        private readonly System.Func<string, T, T> _getter;
        private readonly System.Action<string, T> _setter;
        private T _val;
        private bool _loaded;

        public EditorOption(string key, T val)
            : this(key, val, DefaultGet, DefaultSet)
        {
        }

        public EditorOption(string key, T val, System.Func<string, T, T> getter, System.Action<string, T> setter)
        {
            _key = key;
            _val = val;
            _getter = getter ?? DefaultGet;
            _setter = setter ?? DefaultSet;
        }

        public T Value
        {
            get
            {
                if (!_loaded)
                {
                    Get();
                    _loaded = true;
                }

                return _val;
            }
            set => Set(value);
        }

        private void Get()
        {
            try
            {
                _val = _getter(_key, _val);
            }
            catch (System.ArgumentException ex)
            {
                Debug.LogWarning($"[AIBridge] 读取编辑器设置失败 '{_key}': {ex.Message}");
            }
            catch (System.FormatException ex)
            {
                Debug.LogWarning($"[AIBridge] 编辑器设置格式错误 '{_key}': {ex.Message}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AIBridge] 读取编辑器设置时发生未知错误 '{_key}': {ex.Message}");
            }
        }

        private void Set(T val)
        {
            try
            {
                _val = val;
                _setter(_key, _val);
                _loaded = true;
            }
            catch (System.InvalidCastException ex)
            {
                Debug.LogError($"[AIBridge] 编辑器设置类型转换失败 '{_key}': {ex.Message}");
            }
            catch (System.ArgumentException ex)
            {
                Debug.LogError($"[AIBridge] 编辑器设置序列化失败 '{_key}': {ex.Message}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AIBridge] 写入编辑器设置时发生未知错误 '{_key}': {ex.Message}");
            }
        }

        private static T DefaultGet(string key, T defaultValue)
        {
            if (!EditorPrefs.HasKey(key))
            {
                return defaultValue;
            }

            var type = typeof(T);
            if (type == typeof(bool))
            {
                return (T)(object)EditorPrefs.GetBool(key, defaultValue is bool boolValue && boolValue);
            }

            if (type == typeof(int))
            {
                return (T)(object)EditorPrefs.GetInt(key, defaultValue is int intValue ? intValue : 0);
            }

            if (type == typeof(float))
            {
                return (T)(object)EditorPrefs.GetFloat(key, defaultValue is float floatValue ? floatValue : 0f);
            }

            if (type == typeof(string))
            {
                return (T)(object)EditorPrefs.GetString(key, defaultValue as string ?? string.Empty);
            }

            // 对于复杂类型，仍保留 JSON 序列化能力，兼容旧用法。
            var jsonString = EditorPrefs.GetString(key, string.Empty);
            return string.IsNullOrEmpty(jsonString) ? defaultValue : JsonUtility.FromJson<T>(jsonString);
        }

        private static void DefaultSet(string key, T value)
        {
            var type = typeof(T);
            if (type == typeof(bool))
            {
                EditorPrefs.SetBool(key, value is bool boolValue && boolValue);
                return;
            }

            if (type == typeof(int))
            {
                EditorPrefs.SetInt(key, value is int intValue ? intValue : 0);
                return;
            }

            if (type == typeof(string))
            {
                EditorPrefs.SetString(key, value as string ?? string.Empty);
                return;
            }

            if (type == typeof(float))
            {
                EditorPrefs.SetFloat(key, value is float floatValue ? floatValue : 0f);
                return;
            }

            // 对于复杂类型，仍保留 JSON 序列化能力，兼容旧用法。
            var jsonString = JsonUtility.ToJson(value);
            EditorPrefs.SetString(key, jsonString);
        }
    }
}
