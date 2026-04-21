using System;
using System.IO;
using System.Text.Json;

namespace WT.Renode
{
    /**
     * WT.Renode.ResetValue 类用于加载json格式的默认值配置
     * 调用方法：
     * ResetValue value = ResetValue.Load("path/to/file.json");
     * value.Get("MCResetGenerationModule.DES", DefaultValue); // MCResetGenerationModule为第一层节点，DES为节点下的Key，返回对应的值。值都是UINT32。如果文件不存在，或者对应的Key不存在，就返回默认值
     */
    public class ResetValue : IDisposable
    {
        private readonly JsonDocument _config;

        private ResetValue(JsonDocument config)
        {
            _config = config;
        }

        public static ResetValue Load(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return new ResetValue(null);
            }

            try
            {
                string jsonString = File.ReadAllText(path);
                JsonDocument doc = JsonDocument.Parse(jsonString);
                return new ResetValue(doc);
            }
            catch
            {
                return new ResetValue(null);
            }
        }

        public uint Get(string keyPath, uint defaultValue)
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
                    return numVal;
                }
                else if (currentElement.ValueKind == JsonValueKind.String)
                {
                    string strValue = currentElement.GetString();
                    if (!string.IsNullOrEmpty(strValue))
                    {
                        strValue = strValue.Trim();
                        if (strValue.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                        {
                            return Convert.ToUInt32(strValue.Substring(2), 16);
                        }
                        return Convert.ToUInt32(strValue, 10);
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