using UnityEditor;

namespace AIBridge.Editor
{
    /// <summary>
    /// Persistent settings for GIF recording.
    /// </summary>
    public static class GifRecorderSettings
    {
        private const string KeyPrefix = "AIBridge_GifRecorder_";
        private static EditorOption<int> _defaultFrameCountOption;
        private static EditorOption<int> _defaultFpsOption;
        private static EditorOption<float> _defaultScaleOption;
        private static EditorOption<int> _defaultColorCountOption;
        private static EditorOption<float> _defaultStartDelayOption;

        public static int DefaultFrameCount
        {
            get
            {
                if (_defaultFrameCountOption == null)
                {
                    _defaultFrameCountOption = new EditorOption<int>(KeyPrefix + "FrameCount", AIBridgeProjectSettings.DefaultGifFrameCount, ReadFrameCount, WriteFrameCount);
                }
                return _defaultFrameCountOption.Value;
            }
            set
            {
                if (_defaultFrameCountOption == null)
                {
                    _defaultFrameCountOption = new EditorOption<int>(KeyPrefix + "FrameCount", AIBridgeProjectSettings.DefaultGifFrameCount, ReadFrameCount, WriteFrameCount);
                }
                _defaultFrameCountOption.Value = value;
            }
        }

        public static int DefaultFps
        {
            get
            {
                if (_defaultFpsOption == null)
                {
                    _defaultFpsOption = new EditorOption<int>(KeyPrefix + "Fps", AIBridgeProjectSettings.DefaultGifFps, ReadFps, WriteFps);
                }
                return _defaultFpsOption.Value;
            }
            set
            {
                if (_defaultFpsOption == null)
                {
                    _defaultFpsOption = new EditorOption<int>(KeyPrefix + "Fps", AIBridgeProjectSettings.DefaultGifFps, ReadFps, WriteFps);
                }
                _defaultFpsOption.Value = value;
            }
        }

        public static float DefaultScale
        {
            get
            {
                if (_defaultScaleOption == null)
                {
                    _defaultScaleOption = new EditorOption<float>(KeyPrefix + "Scale", AIBridgeProjectSettings.DefaultGifScale, ReadScale, WriteScale);
                }
                return _defaultScaleOption.Value;
            }
            set
            {
                if (_defaultScaleOption == null)
                {
                    _defaultScaleOption = new EditorOption<float>(KeyPrefix + "Scale", AIBridgeProjectSettings.DefaultGifScale, ReadScale, WriteScale);
                }
                _defaultScaleOption.Value = value;
            }
        }

        public static int DefaultColorCount
        {
            get
            {
                if (_defaultColorCountOption == null)
                {
                    _defaultColorCountOption = new EditorOption<int>(KeyPrefix + "ColorCount", AIBridgeProjectSettings.DefaultGifColorCount, ReadColorCount, WriteColorCount);
                }
                return _defaultColorCountOption.Value;
            }
            set
            {
                if (_defaultColorCountOption == null)
                {
                    _defaultColorCountOption = new EditorOption<int>(KeyPrefix + "ColorCount", AIBridgeProjectSettings.DefaultGifColorCount, ReadColorCount, WriteColorCount);
                }
                _defaultColorCountOption.Value = value;
            }
        }

        public static float DefaultStartDelay
        {
            get
            {
                if (_defaultStartDelayOption == null)
                {
                    _defaultStartDelayOption = new EditorOption<float>(KeyPrefix + "StartDelay", AIBridgeProjectSettings.DefaultGifStartDelay, ReadStartDelay, WriteStartDelay);
                }
                return _defaultStartDelayOption.Value;
            }
            set
            {
                if (_defaultStartDelayOption == null)
                {
                    _defaultStartDelayOption = new EditorOption<float>(KeyPrefix + "StartDelay", AIBridgeProjectSettings.DefaultGifStartDelay, ReadStartDelay, WriteStartDelay);
                }
                _defaultStartDelayOption.Value = value;
            }
        }

        public static void ResetToDefaults()
        {
            DefaultFrameCount = AIBridgeProjectSettings.DefaultGifFrameCount;
            DefaultFps = AIBridgeProjectSettings.DefaultGifFps;
            DefaultScale = AIBridgeProjectSettings.DefaultGifScale;
            DefaultColorCount = AIBridgeProjectSettings.DefaultGifColorCount;
            DefaultStartDelay = AIBridgeProjectSettings.DefaultGifStartDelay;
        }

        private static int ReadFrameCount(string key, int defaultValue)
        {
            EnsureLegacyMigrated();
            return AIBridgeProjectSettings.Instance.GifRecorder.FrameCount;
        }

        private static void WriteFrameCount(string key, int value)
        {
            var settings = AIBridgeProjectSettings.Instance;
            if (settings.GifRecorder.FrameCount == value)
            {
                return;
            }

            settings.GifRecorder.FrameCount = value;
            settings.SaveSettings();
        }

        private static int ReadFps(string key, int defaultValue)
        {
            EnsureLegacyMigrated();
            return AIBridgeProjectSettings.Instance.GifRecorder.Fps;
        }

        private static void WriteFps(string key, int value)
        {
            var settings = AIBridgeProjectSettings.Instance;
            if (settings.GifRecorder.Fps == value)
            {
                return;
            }

            settings.GifRecorder.Fps = value;
            settings.SaveSettings();
        }

        private static float ReadScale(string key, float defaultValue)
        {
            EnsureLegacyMigrated();
            return AIBridgeProjectSettings.Instance.GifRecorder.Scale;
        }

        private static void WriteScale(string key, float value)
        {
            var settings = AIBridgeProjectSettings.Instance;
            if (settings.GifRecorder.Scale.Equals(value))
            {
                return;
            }

            settings.GifRecorder.Scale = value;
            settings.SaveSettings();
        }

        private static int ReadColorCount(string key, int defaultValue)
        {
            EnsureLegacyMigrated();
            return AIBridgeProjectSettings.Instance.GifRecorder.ColorCount;
        }

        private static void WriteColorCount(string key, int value)
        {
            var settings = AIBridgeProjectSettings.Instance;
            if (settings.GifRecorder.ColorCount == value)
            {
                return;
            }

            settings.GifRecorder.ColorCount = value;
            settings.SaveSettings();
        }

        private static float ReadStartDelay(string key, float defaultValue)
        {
            EnsureLegacyMigrated();
            return AIBridgeProjectSettings.Instance.GifRecorder.StartDelay;
        }

        private static void WriteStartDelay(string key, float value)
        {
            var settings = AIBridgeProjectSettings.Instance;
            if (settings.GifRecorder.StartDelay.Equals(value))
            {
                return;
            }

            settings.GifRecorder.StartDelay = value;
            settings.SaveSettings();
        }

        private static void EnsureLegacyMigrated()
        {
            var settings = AIBridgeProjectSettings.Instance;
            if (settings.LegacyGifMigrated)
            {
                return;
            }

            var gifSettings = settings.GifRecorder;

            // 仅首次读取时迁移旧 EditorPrefs，避免丢失老用户已有配置。
            var frameCountKey = KeyPrefix + "FrameCount";
            if (EditorPrefs.HasKey(frameCountKey))
            {
                gifSettings.FrameCount = EditorPrefs.GetInt(frameCountKey, AIBridgeProjectSettings.DefaultGifFrameCount);
                EditorPrefs.DeleteKey(frameCountKey);
            }

            var fpsKey = KeyPrefix + "Fps";
            if (EditorPrefs.HasKey(fpsKey))
            {
                gifSettings.Fps = EditorPrefs.GetInt(fpsKey, AIBridgeProjectSettings.DefaultGifFps);
                EditorPrefs.DeleteKey(fpsKey);
            }

            var scaleKey = KeyPrefix + "Scale";
            if (EditorPrefs.HasKey(scaleKey))
            {
                gifSettings.Scale = EditorPrefs.GetFloat(scaleKey, AIBridgeProjectSettings.DefaultGifScale);
                EditorPrefs.DeleteKey(scaleKey);
            }

            var colorCountKey = KeyPrefix + "ColorCount";
            if (EditorPrefs.HasKey(colorCountKey))
            {
                gifSettings.ColorCount = EditorPrefs.GetInt(colorCountKey, AIBridgeProjectSettings.DefaultGifColorCount);
                EditorPrefs.DeleteKey(colorCountKey);
            }

            var startDelayKey = KeyPrefix + "StartDelay";
            if (EditorPrefs.HasKey(startDelayKey))
            {
                gifSettings.StartDelay = EditorPrefs.GetFloat(startDelayKey, AIBridgeProjectSettings.DefaultGifStartDelay);
                EditorPrefs.DeleteKey(startDelayKey);
            }

            settings.LegacyGifMigrated = true;
            settings.SaveSettings();
        }
    }
}
