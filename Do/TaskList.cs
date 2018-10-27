using Do.Models;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Do
{
    public class TaskList
    {
        private List<TaskItem> _todoItems;

        public static string NoProject { get; } = "No project";

        public static string CompletedTasks { get; } = "Completed tasks";

        public void SetTasks(List<TaskItem> tasks)
        {
            _todoItems = tasks;
        }

        public void SetTasks(object json)
        {
            if (json != null)
            {
                try
                {
                    _todoItems = Json.Deserialize<List<TaskItem>>(json.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    _todoItems = new List<TaskItem>();
                }
            }
        }

        public void Add(TaskItem task)
        {
            if (_todoItems == null)
            {
                _todoItems = new List<TaskItem>();
            }
            _todoItems.Add(task);
        }

        public void Remove(TaskItem task)
        {
            _todoItems?.Remove(task);
        }

        public bool HasAnyTasks
        {
            get
            {
                if (_todoItems == null)
                {
                    return false;
                }
                return _todoItems.Count > 0;
            }
        }

        public bool HasIncompleteTasks
        {
            get
            {
                if (_todoItems == null)
                {
                    return false;
                }
                return _todoItems.Any(x => !x.Completed);
            }
        }

        public bool HasCompleteTasks
        {
            get
            {
                if (_todoItems == null)
                {
                    return false;
                }
                return _todoItems.Any(x => x.Completed);
            }
        }

        public IEnumerable<TaskItem> IncompleteTasks
        {
            get
            {
                return _todoItems?.Where(x => !x.Completed);
            }
        }

        public IEnumerable<TaskItem> CompleteTasks
        {
            get
            {
                return _todoItems?.Where(x => x.Completed).OrderByDescending(x => x.Completion);
            }
        }

        public IEnumerable<string> Projects
        {
            get
            {
                List<string> projects = new List<string>();
                if (_todoItems != null)
                {
                    foreach (TaskItem item in _todoItems)
                    {
                        if (item.Completed)
                        {
                            continue;
                        }

                        foreach (string p in item.Project)
                        {
                            if (!string.IsNullOrWhiteSpace(p) && !projects.Contains(p))
                            {
                                projects.Add(p);
                            }
                        }
                    }

                    projects.Sort();

                    if (TasksWithNoProject?.Count() > 0)
                    {
                        projects.Insert(0, NoProject);
                    }

                    if (CompleteTasks?.Count() > 0)
                    {
                        projects.Add(CompletedTasks);
                    }
                }
                return projects;
            }
        }

        public IEnumerable<TaskItem> TasksForProject(string project)
        {
            if (project == NoProject)
            {
                return TasksWithNoProject;
            }

            if (project == CompletedTasks)
            {
                return CompleteTasks;
            }

            return _todoItems?.Where(x => !x.Completed && x.Project.Contains(project));
        }

        public IEnumerable<TaskItem> TasksWithNoProject
        {
            get
            {
                return _todoItems?.Where(x => !x.Completed && x.Project.Count == 0);
            }
        }

        public string JsonString
        {
            get
            {
                string json = string.Empty;
                if (_todoItems != null)
                {
                    json = Json.Serialize(_todoItems);
                }
                return json;
            }
        }

        public List<string> Export()
        {
            if (_todoItems == null || _todoItems.Count < 1)
            {
                return null;
            }

            List<string> output = new List<string>();
            foreach (var item in _todoItems.OrderBy(x => x.Creation))
            {
                var l = item.TodoLine();
                if (!string.IsNullOrWhiteSpace(l))
                {
                    output.Add(l + "\n");
                }
            }
            if (output.Count < 1)
            {
                return null;
            }
            return output;
        }
    }
}
