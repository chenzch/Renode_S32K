using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using Antmicro.Renode.Logging;

namespace WT.Renode
{
    /**
     * WT.Renode.ResetValue 类用于加载json格式的默认值配置
     * 调用方法：
     * ResetValue value = ResetValue.Load("path/to/file.json");
     * value.Get("MC_RGM.DES", DefaultValue); // MC_RGM为第一层节点，DES为节点下的Key，返回对应的值。值都是UINT32。如果文件不存在，或者对应的Key不存在，就返回默认值
     */
    public class ResetValue : IDisposable
    {
        private readonly JsonDocument _config;
        private readonly string _filePath;
        private static readonly ConcurrentDictionary<string, ResetValue> _cache = new ConcurrentDictionary<string, ResetValue>();

        private ResetValue(JsonDocument config, string filePath = null)
        {
            _config = config;
            _filePath = filePath;
        }

        public static ResetValue Load(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return new ResetValue(null, path);
            }

            string cacheKey = Path.GetFullPath(path);
            if (_cache.TryGetValue(cacheKey, out var cachedValue))
            {
                return cachedValue;
            }

            try
            {
                string jsonString = File.ReadAllText(path);
                JsonDocument doc = JsonDocument.Parse(jsonString);
                var value = new ResetValue(doc, cacheKey);
                _cache[cacheKey] = value;
                return value;
            }
            catch
            {
                return new ResetValue(null, path);
            }
        }

        public uint Get(string keyPath, uint defaultValue, Action<LogLevel, string> log = null)
        {
            if (_config == null || string.IsNullOrEmpty(keyPath))
            {
                return defaultValue;
            }

            try
            {
                string[] keys = keyPath.Split('.');
                JsonElement currentElement = _config.RootElement;

                foreach (string key in keys)
                {
                    if (currentElement.ValueKind != JsonValueKind.Object || 
                        !currentElement.TryGetProperty(key, out JsonElement nextElement))
                    {
                        return defaultValue;
                    }
                    currentElement = nextElement;
                }

                if (currentElement.ValueKind == JsonValueKind.Number && currentElement.TryGetUInt32(out uint numVal))
                {
                    string msg = $"[ResetValue] Load from {_filePath}, Key: {keyPath}, Value: 0x{numVal:X} ({numVal})";
                    if (log != null)
                    {
                        log(LogLevel.Info, msg);
                    }
                    else
                    {
                        Console.WriteLine(msg);
                    }
                    return numVal;
                }
                else if (currentElement.ValueKind == JsonValueKind.String)
                {
                    string strValue = currentElement.GetString();
                    if (!string.IsNullOrEmpty(strValue))
                    {
                        strValue = strValue.Trim();
                        uint parsedVal;
                        if (strValue.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                        {
                            parsedVal = Convert.ToUInt32(strValue.Substring(2), 16);
                        }
                        else
                        {
                            parsedVal = Convert.ToUInt32(strValue, 10);
                        }
                        string msg = $"[ResetValue] Load from {_filePath}, Key: {keyPath}, Value: 0x{parsedVal:X} ({parsedVal})";
                        if (log != null)
                        {
                            log(LogLevel.Info, msg);
                        }
                        else
                        {
                            Console.WriteLine(msg);
                        }
                        return parsedVal;
                    }
                }

                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public void Dispose()
        {
            _config?.Dispose();
        }
    }
}