// WildcardProcessorAPI.cs
using SwarmUI.WebAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SwarmUI.Accounts;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;


namespace Spoomples.Extensions.WildcardImporter
{
    [API.APIClass("API routes related to Wildcard Importer extension")]
    public class WildcardImporterAPI
    {
        private static readonly WildcardProcessor _processor = new WildcardProcessor();

        public static void Register()
        {
            API.RegisterAPICall(ProcessWildcards, true);
            API.RegisterAPICall(GetProcessingStatus);
            API.RegisterAPICall(UndoProcessing);
            API.RegisterAPICall(GetProcessingHistory);
            API.RegisterAPICall(ResolveConflict);
            API.RegisterAPICall(GetDestinationFolder);
            // API.RegisterAPICall(SetDestinationFolder);
        }

        [API.APIDescription("Process wildcard files", "{ success: boolean, message: string, taskId: string }")]
        public static async Task<JObject> ProcessWildcards([API.APIParameter("Files to process")] string filesJson)
        {
            try
            {
                var filesData = JsonConvert.DeserializeObject<List<FileData>>(filesJson);
                // var files = filesData.ToObject<List<FileData>>();
                string taskId = await _processor.ProcessFiles(filesData);
                return new JObject
                {
                    ["success"] = true,
                    ["message"] = "Processing started",
                    ["taskId"] = taskId
                };
            }
            catch (Exception ex)
            {
                return new JObject
                {
                    ["success"] = false,
                    ["message"] = $"Error starting processing: {ex.Message}"
                };
            }
        }

        [API.APIDescription("Get the status of wildcard processing", "{ status: string, progress: number, conflicts: array }")]
        public static async Task<JObject> GetProcessingStatus(Session session, string taskId)
        {
            var status = _processor.GetStatus(taskId);
            return JObject.FromObject(status);
        }

        [API.APIDescription("Undo the last processing operation", "{ success: boolean, message: string }")]
        public static async Task<JObject> UndoProcessing(Session session, string taskId)
        {
            bool success = await _processor.UndoProcessing(taskId);
            return new JObject
            {
                ["success"] = success,
                ["message"] = success ? "Processing undone successfully" : "Failed to undo processing"
            };
        }

        [API.APIDescription("Get the history of processing operations", "{ history: array }")]
        public static async Task<JObject> GetProcessingHistory(Session session)
        {
            var history = _processor.GetHistory();
            return new JObject
            {
                ["history"] = JArray.FromObject(history)
            };
        }

        [API.APIDescription("Resolve a file conflict", "{ success: boolean, message: string }")]
        public static async Task<JObject> ResolveConflict(Session session, string taskId, string filePath, string resolution)
        {
            bool success = await _processor.ResolveConflict(taskId, filePath, resolution);
            return new JObject
            {
                ["success"] = success,
                ["message"] = success ? "Conflict resolved" : "Failed to resolve conflict"
            };
        }

        [API.APIDescription("Get the current destination folder", "{ folderPath: string }")]
        public static async Task<JObject> GetDestinationFolder(Session session)
        {
            string folderPath = _processor.destinationFolder;
            return new JObject
            {
                ["folderPath"] = folderPath ?? ""
            };
        }

        // TODO: Implement this
        // [API.APIDescription("Set the destination folder", "{ success: boolean, message: string }")]
        // public static async Task<JObject> SetDestinationFolder(Session session, string folderPath)
        // {
        //     bool success = _processor.SetDestinationFolder(folderPath);
        //     return new JObject
        //     {
        //         ["success"] = success,
        //         ["message"] = success ? "Destination folder set successfully" : "Failed to set destination folder"
        //     };
        // }
    }
}
