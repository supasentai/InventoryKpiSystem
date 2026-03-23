using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace InventoryKpiSystem.Services.Idempotency
{
    public class ProcessedFileRegistry
    {
        private readonly string _registryFilePath;
        private readonly ConcurrentDictionary<string, bool> _processedFiles;

        // Khóa dùng để đồng bộ hóa việc đọc/ghi file JSON, tránh Race Condition
        private readonly object _fileLock = new object();

        public ProcessedFileRegistry(string storageDirectory = "processed-files")
        {
            Directory.CreateDirectory(storageDirectory);
            _registryFilePath = Path.Combine(storageDirectory, "processed-files.json");
            _processedFiles = new ConcurrentDictionary<string, bool>();
            LoadRegistry();
        }

        private void LoadRegistry()
        {
            if (!File.Exists(_registryFilePath)) return;

            lock (_fileLock)
            {
                try
                {
                    string json = File.ReadAllText(_registryFilePath);
                    var files = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();

                    foreach (var file in files)
                    {
                        _processedFiles.TryAdd(file, true);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Lỗi] Không thể tải lịch sử file: {ex.Message}");
                }
            }
        }

       
        public bool IsFileProcessed(string fileName)
        {
            return _processedFiles.ContainsKey(fileName);
        }
        public bool MarkAsProcessed(string fileName)
        {
            // trả về false nếu file đã tồn tại trong danh sách
            if (_processedFiles.TryAdd(fileName, true))
            {
                SaveRegistry();
                return true;
            }
            return false;
        }

        private void SaveRegistry()
        {
            // Chỉ khóa đúng quá trình ghi file
            lock (_fileLock)
            {
                try
                {
                    var files = new List<string>(_processedFiles.Keys);
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string json = JsonSerializer.Serialize(files, options);

                    File.WriteAllText(_registryFilePath, json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Lỗi] Không thể lưu lịch sử file: {ex.Message}");
                }
            }
        }
    }
}