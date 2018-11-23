using System;
using System.Collections.Generic;

namespace Do.Models
{
    public class TaskItem
    {
        public string Body { get; set; }
        public bool Completed { get; set; }
        public DateTime? Completion { get; set; }
        public List<string> Context { get; set; } = new List<string>();
        public DateTime? Creation { get; set; }
        public Dictionary<string, string> Meta { get; set; } = new Dictionary<string, string>();
        public string Priority { get; set; }
        public List<string> Project { get; set; } = new List<string>();

        public string TodoLine()
        {
            return TodoTxt.GenerateTodoLine(this);
        }

        public void FromLine(string line)
        {
            var t = TodoTxt.Parse(line);
            Body = t.Body;
            Completed = t.Completed;
            Completion = t.Completion;
            Context = t.Context;
            Creation = t.Creation;
            Meta = t.Meta;
            Priority = t.Priority;
            Project = t.Project;
        }

        public void Complete()
        {
            Completed = true;
            Completion = DateTime.Now;
        }

        public void Incomplete()
        {
            Completed = false;
            Completion = null;
        }
    }
}
