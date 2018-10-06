using Do.Models;
using System;
using System.Text.RegularExpressions;

namespace Do
{
    public static class TodoTxt
    {
        /// <summary>
        ///     Parse a string line to the TodoItem
        /// </summary>
        /// <param name="line">String line</param>
        /// <returns>TodoItem</returns>
        public static TaskItem Parse(string line)
        {
            var entry = new TaskItem();
            // Completed
            if (Regex.IsMatch(line, "^x (.*)$"))
            {
                entry.Completed = true;
                // Remove the completion mark
                line = Regex.Replace(line, "^x (.*)$", "$1");
            }
            else
            {
                entry.Completed = false;
            }

            // Priority
            if (Regex.IsMatch(line, "^\\(.\\) (.*)$"))
            {
                var priority = Regex.Match(line, "^\\((.)\\)");
                entry.Priority = priority.Groups[1].ToString();
                line = Regex.Replace(line, "^\\(.\\) (.*)$", "$1");
            }

            // Completion and Creation
            if (Regex.IsMatch(line, "^(.{4}-.{2}-.{2}) (.{4}-.{2}-.{2}) (.*)$"))
            {
                // Completion and Creation
                var result = Regex.Matches(line, ".{4}-.{2}-.{2}");

                entry.Completion = DateTime.Parse(result[0].ToString());
                entry.Creation = DateTime.Parse(result[1].ToString());
                line = Regex.Replace(line, "^.{4}-.{2}-.{2} .{4}-.{2}-.{2} (.*)$", "$1");
            }
            else if (Regex.IsMatch(line, "^(.{4}-.{2}-.{2}) (.*)$"))
            {
                // Creation only
                var result = Regex.Matches(line, "^.{4}-.{2}-.{2}");
                entry.Creation = DateTime.Parse(result[0].ToString());
                line = Regex.Replace(line, "^.{4}-.{2}-.{2} (.*)$", "$1");
            }

            //// Context
            foreach (var item in Regex.Matches(line, "@([^\\s]*)"))
                entry.Context.Add(item.ToString().Replace("@", "").Trim());

            line = Regex.Replace(line, "@([^\\s]*)", "").Trim();

            //// Project
            foreach (var item in Regex.Matches(line, "\\+([^\\s]*)"))
                entry.Project.Add(item.ToString().Replace("+", "").Trim());

            line = Regex.Replace(line, "\\+([^\\s]*)", "").Trim();

            //// Meta
            foreach (var item in Regex.Matches(line, "([^\\s]*?)\\:([^\\s]*)"))
            {
                var result = Regex.Match(item.ToString(), "([^\\s]*?)\\:([^\\s]*)");
                entry.Meta.Add(result.Groups[1].ToString(), result.Groups[2].ToString());
            }

            line = Regex.Replace(line, "([^\\s]*?)\\:([^\\s]*)", "").Trim();

            entry.Body = line;

            return entry;
        }

        /// <summary>
        ///     Generate output string from TodoItem
        /// </summary>
        /// <param name="item">TodoItem to process</param>
        /// <returns>String representation of the TodoItem</returns>
        public static string GenerateTodoLine(TaskItem item)
        {
            var output = "";

            // Completion marker
            if (item.Completed) output += "x ";

            if (!string.IsNullOrWhiteSpace(item.Priority))
            {
                output += "(" + item.Priority + ") ";
            }

            if (item.Completion.HasValue)
            {
                output += item.Completion.Value.Date.ToString("yyyy-MM-dd") + " ";
            }

            if (item.Creation.HasValue)
            {
                output += item.Creation.Value.Date.ToString("yyyy-MM-dd") + " ";
            }

            output += item.Body + " ";

            //// Context
            foreach (var context in item.Context) output += "@" + context + " ";

            //// Project
            foreach (var project in item.Project) output += "+" + project + " ";

            //// Meta
            foreach (var meta in item.Meta) output += meta.Key + ":" + meta.Value + " ";

            return output.Trim();
        }
    }
}
