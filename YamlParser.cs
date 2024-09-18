using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SwarmUI.Utils;

namespace Spoomples.Extensions.WildcardImporter
{
    public class YamlParser
    {
        public Dictionary<string, object> Parse(string yamlContent)
        {
            var result = new Dictionary<string, object>();
            var lines = yamlContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // Stack to keep track of the context: (indent level, container)
            var stack = new Stack<(int indent, object container)>();
            stack.Push((-1, (object)result));

            for (int i = 0; i < lines.Length; i++)
            {
                var rawLine = lines[i];
                var line = rawLine.TrimEnd();

                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                    continue; // Skip empty lines and comments

                int indent = rawLine.Length - rawLine.TrimStart().Length;

                // Pop the stack until we find the correct parent
                while (stack.Peek().indent >= indent)
                {
                    stack.Pop();
                }

                var currentContainer = stack.Peek().container;

                if (line.TrimStart().StartsWith("-"))
                {
                    // List item
                    string item = line.TrimStart().Substring(1).Trim();

                    if (currentContainer is List<object> list)
                    {
                        list.Add(item);
                    }
                    else if (currentContainer is Dictionary<string, object> map)
                    {
                        if (map.Count == 0)
                        {
                            continue; // Or handle as needed
                        }

                        var lastKey = GetLastKey(map);
                        if (lastKey == null)
                        {
                            continue; // Or handle as needed
                        }

                        if (map[lastKey] is List<object> childList)
                        {
                            childList.Add(item);
                        }
                        else
                        {
                            // Convert existing entry to a list
                            childList = new List<object> { item };
                            map[lastKey] = childList;
                        }
                    }
                    else
                    {
                        Logs.Error("List item found but current container is neither a list nor a map.");
                    }
                }
                else if (line.Contains(":"))
                {
                    // Key-value pair
                    var parts = line.Split(new[] { ':' }, 2);
                    string key = parts[0].Trim();
                    string valuePart = parts[1].Trim();

                    if (currentContainer is Dictionary<string, object> map)
                    {
                        if (string.IsNullOrEmpty(valuePart))
                        {
                            // Determine if the next significant line is a list or a map
                            bool isNextLineList = false;
                            for (int j = i + 1; j < lines.Length; j++)
                            {
                                var nextRawLine = lines[j];
                                var nextLine = nextRawLine.TrimStart();

                                if (string.IsNullOrWhiteSpace(nextLine) || nextLine.StartsWith("#"))
                                    continue; // Skip empty lines and comments

                                if (nextLine.StartsWith("-"))
                                {
                                    isNextLineList = true;
                                }
                                break;
                            }

                            if (isNextLineList)
                            {
                                // Assign a new list to this key
                                var newList = new List<object>();
                                map[key] = newList;
                                stack.Push((indent, newList));
                            }
                            else
                            {
                                // Assign a new dictionary to this key
                                var newMap = new Dictionary<string, object>();
                                map[key] = newMap;
                                stack.Push((indent, newMap));
                            }
                        }
                        else
                        {
                            // Inline list or single value
                            if (valuePart.StartsWith("[") && valuePart.EndsWith("]"))
                            {
                                // Inline list
                                var list = new List<object>();
                                var items = valuePart.Trim('[', ']').Split(',');

                                foreach (var item in items)
                                {
                                    list.Add(item.Trim());
                                }

                                map[key] = list;
                            }
                            else
                            {
                                // Single value
                                map[key] = valuePart;
                            }
                        }
                    }
                    else
                    {
                        Logs.Error("Found a key-value pair but current container is not a map.");
                    }
                }
                else
                {
                    // Unhandled line format
                    Logs.Warning($"Unhandled line format: {line}");
                }
            }

            // Log the final result as JSON for debugging
            Logs.Info($"Final result: {SerializeObject(result)}");

            return result;
        }

        public static string SerializeObject(object obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj, Formatting.Indented);
            }
            catch
            {
                return obj.ToString();
            }
        }

        private string GetLastKey(Dictionary<string, object> map)
        {
            if (map == null || map.Count == 0)
                return null;

            foreach (var key in map.Keys)
            {
                // Iterate to the last key
            }

            // Alternative using LINQ
            return map.Keys.LastOrDefault();
        }
    }
}
