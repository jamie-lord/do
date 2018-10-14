using Do.Models;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Do.Pages
{
    public class IndexBase : BlazorComponent
    {
        public static List<TaskItem> TodoItems;

        public string CurrentInput { get; set; } = null;

        public TaskItem CurrentItem { get; set; } = null;

        public string InputButtonText { get; private set; } = "Save";

        private const string TODO_KEY = "do";

        protected override async Task OnInitAsync()
        {
            TodoItems = await GetTasksFromLocalStorage();

            await JSRuntime.Current.InvokeAsync<bool>("addEventListeners");
        }

        private static async Task SaveItemsInLocalStorage()
        {
            if (TodoItems != null)
            {
                var s = Json.Serialize(TodoItems);
                await JSRuntime.Current.InvokeAsync<bool>("storeInLocal", new object[] { TODO_KEY, s });
            }
        }

        private async Task<List<TaskItem>> GetTasksFromLocalStorage()
        {
            var r = await JSRuntime.Current.InvokeAsync<object>("getFromLocal", new object[] { TODO_KEY });
            if (r == null)
            {
                return new List<TaskItem>();
            }
            try
            {
                return Json.Deserialize<List<TaskItem>>(r.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new List<TaskItem>();
            }
        }

        public async void AddItem()
        {
            InputButtonText = "Save";
            if (CurrentInput == null || string.IsNullOrWhiteSpace(CurrentInput))
            {
                return;
            }
            try
            {
                // Editing existing task
                if (CurrentItem != null)
                {
                    CurrentItem.FromLine(CurrentInput);
                }
                // New task
                else
                {
                    var t = new TaskItem();
                    t.FromLine(CurrentInput);
                    t.Creation = DateTime.Now;
                    TodoItems.Add(t);
                }
                CurrentInput = null;
                CurrentItem = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            await SaveItemsInLocalStorage();
        }

        public async void Complete(TaskItem item)
        {
            item.Complete();
            await SaveItemsInLocalStorage();
        }

        public async void Edit(TaskItem item)
        {
            CurrentItem = item;
            CurrentInput = CurrentItem.TodoLine();
            InputButtonText = "Update";
            await JSRuntime.Current.InvokeAsync<bool>("focusOnInputField");
        }

        public async void Delete(TaskItem item)
        {
            try
            {
                if (CurrentItem == item)
                {
                    CurrentItem = null;
                    CurrentInput = null;
                }
                TodoItems.Remove(item);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            await SaveItemsInLocalStorage();
        }

        public void ClearInput()
        {
            CurrentInput = null;
            CurrentItem = null;
            InputButtonText = "Save";
        }

        [JSInvokable]
        public static async void Import(string fileData)
        {
            if (string.IsNullOrWhiteSpace(fileData))
            {
                return;
            }

            var items = new List<TaskItem>();
            using (StringReader reader = new StringReader(fileData))
            {
                string line;
                while (!string.IsNullOrWhiteSpace((line = reader.ReadLine())))
                {
                    try
                    {
                        var i = TodoTxt.Parse(line);
                        if (i != null)
                        {
                            items.Add(i);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
            if (items.Count > 0)
            {
                TodoItems = items;
                await SaveItemsInLocalStorage();
                await JSRuntime.Current.InvokeAsync<bool>("reload");
            }
        }

        public async void Export()
        {
            if (TodoItems == null || TodoItems.Count < 1)
            {
                return;
            }

            var output = new List<string>();
            foreach (var item in TodoItems.OrderBy(x => x.Creation))
            {
                var l = item.TodoLine();
                if (!string.IsNullOrWhiteSpace(l))
                {
                    output.Add(l + "\n");
                }
            }
            if (output.Count < 1)
            {
                return;
            }
            await JSRuntime.Current.InvokeAsync<bool>("saveToFile", new object[] { output });
        }
    }
}
