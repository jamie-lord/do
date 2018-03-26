using System;

namespace Do.Models
{
    public class TaskItem : TodoItem
    {
        public string TodoLine
        {
            get
            {
                return TodoTxt.GenerateTodoLine(this);
            }
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
            Console.WriteLine(Creation);
        }

        public void Update(string line)
        {
            var t = TodoTxt.Parse(line);
            if (!string.IsNullOrWhiteSpace(t.Body))
            {
                Body = t.Body;
            }
            Completed = t.Completed;
            if (t.Completion != null)
            {
                Completion = t.Completion;
            }
            Context = t.Context;
            if (t.Creation != null)
            {
                Creation = t.Creation;
            }
            Console.WriteLine(Creation);
            Meta = t.Meta;
            Priority = t.Priority;
            Project = t.Project;
        }

        public void Complete()
        {
            Completed = true;
            Completion = DateTime.Now;
        }
    }
}
