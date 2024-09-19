// WildcardProcessor.cs
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SwarmUI.Core;
using SwarmUI.Utils;

using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;

namespace Spoomples.Extensions.WildcardImporter
{
    public class WildcardProcessor
    {
        // TODO: Allow setting the destination folder via API. Users might want to set it to their own custom folder.
        public readonly string destinationFolder = Utilities.CombinePathWithAbsolute(Program.ServerSettings.Paths.DataPath, Program.ServerSettings.Paths.WildcardsFolder);
        private readonly ConcurrentDictionary<string, ProcessingTask> _tasks = new();
        private readonly ConcurrentBag<ProcessingHistoryItem> _history = new();

        public WildcardProcessor()
        {
            // Directory.CreateDirectory(destinationFolder);
            Logs.Info($"WildcardProcessor initialized with destination folder: {destinationFolder}");
        }

        public async Task<string> ProcessFiles(List<FileData> files)
        {
            string taskId = Guid.NewGuid().ToString();
            var task = new ProcessingTask { Id = taskId, TotalFiles = files.Count };
            _tasks[taskId] = task;

            _ = Task.Run(async () =>
            {
                foreach (var file in files)
                {
                    if (file.Base64Content == null)
                    {
                        if (!File.Exists(file.FilePath))
                        {
                            _tasks[taskId].Errors.Add($"File not found: {file.FilePath}");
                            return;
                        }

                        string fileName = Path.GetFileName(file.FilePath);
                        try
                        {
                            await ProcessFile(taskId, fileName, file.FilePath);
                        }
                        catch (Exception ex)
                        {
                            _tasks[taskId].Errors.Add($"Error processing {file.FilePath}: {ex.Message}");
                        }
                    }
                    else
                    {
                        string tempPath = Path.GetTempFileName();
                        try
                        {
                            byte[] fileBytes = Convert.FromBase64String(file.Base64Content);
                            await File.WriteAllBytesAsync(tempPath, fileBytes);
                            string fileName = Path.GetFileName(file.FilePath);

                            await ProcessFile(taskId, fileName, tempPath);
                        }
                        catch (FormatException fe)
                        {
                            _tasks[taskId].Errors.Add($"Invalid base64 content for file {file.FilePath}: {fe.Message}");
                        }
                        catch (Exception ex)
                        {
                            _tasks[taskId].Errors.Add($"Error processing {file.FilePath}: {ex.Message}");
                        }
                        finally
                        {
                            if (File.Exists(tempPath))
                            {
                                File.Delete(tempPath);
                            }
                        }
                    }
                    task.ProcessedFiles++;
                }
                task.Status = ProcessingStatusEnum.Completed;
                _history.Add(new ProcessingHistoryItem { TaskId = taskId, Timestamp = DateTime.UtcNow, Description = $"Processed {task.ProcessedFiles} files" });
            });

            return taskId;
        }

        private async Task ProcessFile(string taskId, string fileName, string filePath)
        {
            var task = _tasks[taskId];
            try
            {
                if (fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    await ProcessZipFile(taskId, filePath);
                }
                else if (fileName.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".yml", StringComparison.OrdinalIgnoreCase))
                {
                    string content = await File.ReadAllTextAsync(filePath);
                    await ProcessYamlFile(taskId, content, fileName);
                }
                else if (fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    string content = await File.ReadAllTextAsync(filePath);
                    await ProcessTextContent(taskId, content, fileName);
                }
            }
            catch (Exception ex)
            {
                task.Errors.Add($"Error processing {fileName}: {ex.Message}");
            }
        }

        private async Task ProcessZipFile(string taskId, string zipPath)
        {
            Logs.Info($"Processing ZIP file: {zipPath}");
            using (var archive = ZipFile.OpenRead(zipPath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) || entry.FullName.EndsWith(".yml", StringComparison.OrdinalIgnoreCase))
                    {
                        using (var stream = entry.Open())
                        using (var reader = new StreamReader(stream))
                        {
                            string content = await reader.ReadToEndAsync();
                            await ProcessYamlFile(taskId, content, entry.FullName);
                        }
                    }
                    else if (entry.FullName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                    {
                        using (var stream = entry.Open())
                        using (var reader = new StreamReader(stream))
                        {
                            string content = await reader.ReadToEndAsync();
                            await ProcessTextContent(taskId, content, entry.FullName);
                        }
                    }
                }
            }
            Logs.Info($"ZIP file processed: {zipPath}");
        }

        private async Task ProcessYamlFile(string taskId, string yamlContent, string yamlPath)
        {
            Logs.Info($"Processing YAML file: {yamlPath}");
            var parser = new YamlParser();
            var parsedYaml = parser.Parse(yamlContent);

            foreach (var topLevelKvp in parsedYaml)
            {
                string topLevelKey = topLevelKvp.Key;
                var topLevelValue = topLevelKvp.Value;
                Logs.Info($"Top-level key: {topLevelKey}");
                Logs.Info($"Top-level value: {YamlParser.SerializeObject(topLevelValue)}");

                await TraverseYaml(taskId, topLevelKey, topLevelKey, topLevelValue);
            }
            Logs.Info($"YAML file processed: {yamlPath}");
        }

        private async Task TraverseYaml(string taskId, string topLevelKey, string currentKey, object currentValue)
        {
            if (currentValue is Dictionary<string, object> currentMap)
            {
                Logs.Info($"Processing map for key: {currentKey}");
                foreach (var kvp in currentMap)
                {
                    string key = kvp.Key;
                    var value = kvp.Value;
                    Logs.Info($"Key: {key}");
                    Logs.Info($"Value: {YamlParser.SerializeObject(value)}");

                    await TraverseYaml(taskId, topLevelKey, key, value);
                }
            }
            else if (currentValue is List<object> currentList)
            {
                Logs.Info($"Processing list for key: {currentKey}");
                if (currentList.Count == 1 && currentList[0] is string singleItem)
                {
                    Logs.Info($"Single-item list for key: {currentKey}");
                    string processedLine = ProcessWildcardLine(singleItem);
                    await WriteToMainCategory(taskId, topLevelKey, currentKey, processedLine);
                }
                else
                {
                    Logs.Info($"Multi-item list for key: {currentKey}");
                    List<string> processedLines = new List<string>();
                    foreach (var item in currentList)
                    {
                        if (item is string line)
                        {
                            processedLines.Add(ProcessWildcardLine(line));
                        }
                        else if (item is Dictionary<string, object> subMap)
                        {
                            foreach (var subKvp in subMap)
                            {
                                await TraverseYaml(taskId, topLevelKey, subKvp.Key, subKvp.Value);
                            }
                        }
                    }

                    if (processedLines.Any())
                    {
                        await WriteToMainCategory(taskId, topLevelKey, currentKey, processedLines);
                    }
                }
            }
            else if (currentValue is string stringValue)
            {
                Logs.Info($"Processing string value for key: {currentKey}");
                string processedLine = ProcessWildcardLine(stringValue);
                await WriteToMainCategory(taskId, topLevelKey, currentKey, processedLine);
            }
        }

        private async Task WriteToMainCategory(string taskId, string topLevelKey, string categoryKey, string line)
        {
            string categoryFolderPath = Path.Combine(destinationFolder, topLevelKey);
            Directory.CreateDirectory(categoryFolderPath);

            string mainCategoryFilePath = Path.Combine(categoryFolderPath, $"{categoryKey}.txt");
            Logs.Info($"Adding line to main category file: {mainCategoryFilePath}");

            await File.AppendAllLinesAsync(mainCategoryFilePath, new List<string> { line });
            _tasks[taskId].ProcessedFiles++;
        }

        private async Task WriteToMainCategory(string taskId, string topLevelKey, string categoryKey, List<string> lines)
        {
            string categoryFolderPath = Path.Combine(destinationFolder, topLevelKey);
            Directory.CreateDirectory(categoryFolderPath);

            string mainCategoryFilePath = Path.Combine(categoryFolderPath, $"{categoryKey}.txt");
            Logs.Info($"Writing lines to main category file: {mainCategoryFilePath}");

            await File.WriteAllLinesAsync(mainCategoryFilePath, lines);
            _tasks[taskId].ProcessedFiles++;
        }

        private async Task ProcessTextContent(string taskId, string content, string fileName)
        {
            Logs.Info($"Processing text file: {fileName}");
            var lines = content.Split('\n');
            var processedLines = lines.Select(ProcessWildcardLine);

            string outputPath = Path.Combine(destinationFolder, fileName);
            await File.WriteAllLinesAsync(outputPath, processedLines);
            _tasks[taskId].ProcessedFiles++;
            Logs.Info($"Text content processed: {outputPath}");
        }

        private string ProcessWildcardLine(string line)
        {
            // Replace __ wildcards
            line = System.Text.RegularExpressions.Regex.Replace(line, @"__(\w+)__", match => $"<wildcard:{match.Groups[1].Value}>");

            // Replace {} random selections
            line = System.Text.RegularExpressions.Regex.Replace(line, @"\{([^}]+)\}", match => $"<random:{match.Groups[1].Value.Replace(" ", "")}>");

            return line;
        }

        public ProgressStatus GetStatus(string taskId)
        {
            if (_tasks.TryGetValue(taskId, out var task))
            {
                return new ProgressStatus
                {
                    Status = task.Status.ToString(),
                    Progress = (float)task.ProcessedFiles / task.TotalFiles,
                    Conflicts = task.Conflicts.ToList()
                };
            }
            return new ProgressStatus { Status = "Not Found" };
        }

        public async Task<bool> UndoProcessing(string taskId)
        {
            Logs.Info($"Undoing processing for task: {taskId}");
            if (_tasks.TryGetValue(taskId, out var task))
            {
                foreach (var backup in task.Backups)
                {
                    File.Move(backup.Value, backup.Key, true);
                }
                _tasks.TryRemove(taskId, out _);
                Logs.Info($"Processing undone for task: {taskId}");
                return true;
            }
            Logs.Warning($"Processing undo failed for task: {taskId}");
            return false;
        }

        public List<ProcessingHistoryItem> GetHistory()
        {
            return _history.OrderByDescending(h => h.Timestamp).ToList();
        }

        public async Task<bool> ResolveConflict(string taskId, string filePath, string resolution)
        {
            if (_tasks.TryGetValue(taskId, out var task))
            {
                Logs.Info($"Resolving conflict for task: {taskId}, filePath: {filePath}, resolution: {resolution}");
                var conflict = task.Conflicts.FirstOrDefault(c => c.FilePath == filePath);
                if (conflict != null)
                {
                    switch (resolution)
                    {
                        case "overwrite":
                            Logs.Info($"Overwriting file: {conflict.FilePath}");
                            File.Move(conflict.TempPath, conflict.FilePath, true);
                            break;
                        case "rename":
                            string newPath = GetUniqueFilePath(conflict.FilePath);
                            Logs.Info($"Renaming file: {conflict.FilePath} to {newPath}");
                            File.Move(conflict.TempPath, newPath);
                            break;
                        case "skip":
                            Logs.Info($"Skipping file: {conflict.FilePath}");
                            File.Delete(conflict.TempPath);
                            break;
                    }
                    task.Conflicts.TryTake(out _);
                    Logs.Info($"Conflict resolved for task: {taskId}, filePath: {filePath}");
                    return true;
                }
            }
            Logs.Warning($"Failed to resolve conflict for task: {taskId}, filePath: {filePath}");
            return false;
        }

        private string GetUniqueFilePath(string originalPath)
        {
            string directory = Path.GetDirectoryName(originalPath);
            string fileName = Path.GetFileNameWithoutExtension(originalPath);
            string extension = Path.GetExtension(originalPath);
            int counter = 1;

            string newPath;
            do
            {
                newPath = Path.Combine(directory, $"{fileName}_{counter}{extension}");
                counter++;
            } while (File.Exists(newPath));

            return newPath;
        }

        public async Task<string> ProcessFilesAsync(List<FileData> filesData, Func<ProgressStatus, Task> statusCallback)
        {
            string taskId = Guid.NewGuid().ToString();
            var processingTask = new ProcessingTask
            {
                Id = taskId,
                Files = filesData,
                StatusCallback = statusCallback
            };

            _tasks[taskId] = processingTask;

            _ = Task.Run(async () =>
            {
                try
                {
                    // Initialize processing
                    processingTask.ProgressStatus = new ProgressStatus
                    {
                        Status = "Started",
                        Progress = 0,
                        Conflicts = new()
                    };
                    await processingTask.StatusCallback(processingTask.ProgressStatus);

                    // Example processing loop
                    for (int i = 1; i <= 100; i++)
                    {
                        // Simulate processing work
                        await Task.Delay(100);

                        // Update progress
                        processingTask.ProgressStatus.Progress = i;
                        await processingTask.StatusCallback(processingTask.ProgressStatus);
                    }

                    processingTask.ProgressStatus.Status = "Completed";
                    await processingTask.StatusCallback(processingTask.ProgressStatus);
                }
                catch (Exception ex)
                {
                    processingTask.ProgressStatus.Status = "Error";
                    processingTask.ProgressStatus.Message = ex.Message;
                    await processingTask.StatusCallback(processingTask.ProgressStatus);
                }
            });

            return taskId;
        }

        public async Task WaitForProcessingToCompleteAsync(string taskId)
        {
            if (_tasks.ContainsKey(taskId))
            {
                var processingTask = _tasks[taskId];
                await processingTask.CompletionSource.Task;
            }
        }
    }

    public class ProcessingTask
    {
        public string Id;
        public List<FileData> Files;
        public int TotalFiles;
        public int ProcessedFiles;
        public ProcessingStatusEnum Status;
        public ProgressStatus ProgressStatus;
        public ConcurrentBag<string> Errors = new();
        public ConcurrentDictionary<string, string> Backups = new();
        public ConcurrentBag<ConflictInfo> Conflicts = new();
        public TaskCompletionSource<bool> CompletionSource = new();
        public Func<ProgressStatus, Task> StatusCallback;
    }

    public struct ProgressStatus
    {
        public string Status;
        public float Progress;
        public List<ConflictInfo> Conflicts;
        public string Message;
    }

    public enum ProcessingStatusEnum
    {
        InProgress,
        Completed,
        Failed,
    }

    public class ConflictInfo
    {
        public string FilePath;
        public string TempPath;
    }

    public class ProcessingHistoryItem
    {
        public string TaskId;
        public DateTime Timestamp;
        public string Description;
    }

    /// <summary>Represents file data sent from the frontend. Can be a file path or a base64 encoded file. This is represented as a file path if the Base64Content is null, and as a base64 encoded file otherwise.</summary>
    public class FileData
    {
        public string FilePath;
        public string? Base64Content;
    }
}
