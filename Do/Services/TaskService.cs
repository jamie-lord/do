using Do.Models;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Do.Services
{
    public class TaskService
    {
        private List<TaskItem> _todoItems;

        private const string DoKey = "do";

        public static string NoProject { get; } = "No project";

        public static string CompletedTasks { get; } = "Completed tasks";

        public async Task Init()
        {
            await GetTasksFromLocalStorage();
        }

        public async Task SetTasks(List<TaskItem> tasks)
        {
            _todoItems = tasks;
            await SaveItemsInLocalStorage();
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

        public async Task Add(TaskItem task)
        {
            if (_todoItems == null)
            {
                _todoItems = new List<TaskItem>();
            }
            _todoItems.Add(task);

            await SaveItemsInLocalStorage();
        }

        public async Task Update(TaskItem oldTask, TaskItem newTask)
        {
            int index = _todoItems.IndexOf(oldTask);

            if (index == -1)
            {
                Console.WriteLine("Task couldn't be updated as it does not exist");
                return;
            }

            _todoItems[index] = newTask;
            await SaveItemsInLocalStorage();
        }

        public async Task Remove(TaskItem task)
        {
            _todoItems?.Remove(task);
            await SaveItemsInLocalStorage();
        }

        public async Task CompleteTask(TaskItem task)
        {
            task.Complete();
            await SaveItemsInLocalStorage();
        }

        public async Task IncompleteTask(TaskItem task)
        {
            task.Incomplete();
            await SaveItemsInLocalStorage();
        }

        public async Task SetTaskPriority(TaskItem task, string priority)
        {
            task.Priority = priority;
            await SaveItemsInLocalStorage();
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

        public int CompletedTaskCountForProject(string project)
        {
            int count = 0;
            foreach (TaskItem completedTask in CompleteTasks.Where(x => x.Project.Contains(project)))
            {
                count++;
            }
            return count;
        }

        private string JsonString
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

        private async Task SaveItemsInLocalStorage()
        {
            if (!string.IsNullOrWhiteSpace(JsonString))
            {
                Console.WriteLine("Saving tasks to local storage");
                await JSRuntime.Current.InvokeAsync<bool>("storeInLocal", new object[] { DoKey, JsonString });
            }
        }


        private async Task GetTasksFromLocalStorage()
        {
            Console.WriteLine("Getting tasks from local storage");
            object json = await JSRuntime.Current.InvokeAsync<object>("getFromLocal", new object[] { DoKey });
            SetTasks(json);
        }
    }
}
