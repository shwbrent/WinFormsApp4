using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace WindowsFormsApp5
{
    public class TaskManager<TResult>
    {
        private volatile List<Func<Task>> originalTaskFunctions = new List<Func<Task>>();
        private volatile List<Task> tasks = new List<Task>();
        private System.Timers.Timer timer;

        public TaskManager()
        {
            timer = new System.Timers.Timer(500); // 每2秒檢查一次
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
        }

        public void Start()
        {
            timer.Start();
        }

        public void AddTask(Func<Task> taskFunction)
        {
            originalTaskFunctions.Add(taskFunction);
        }

        private async void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            // 等待當前所有任務完成再繼續添加新任務
            await Task.WhenAll(tasks.ToArray());

            // 檢查未完成的任務
            if (tasks.Exists(t => !t.IsCompleted))
            {
                //UpdateStatus("Some tasks are still running. Waiting for completion.");
                return;
            }

            // 清除已完成的任務
            tasks.Clear();

            // 重新添加所有原始任務
            foreach (var taskFunction in tasks)
            {
                //Task newTask = taskFunction();
                //tasks.Add(newTask);
                taskFunction.Start();
            }

            //UpdateStatus("All tasks restarted.");
            // 列印當前執行緒數量
            int threadCount = Process.GetCurrentProcess().Threads.Count;
            //UpdateStatus($"Current thread count: {threadCount}");
        }
    }
}
