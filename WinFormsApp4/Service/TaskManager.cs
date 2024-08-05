using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timer = System.Threading.Timer;

public class TaskManager
{
    private volatile List<Func<Task<IDeviceEntity>>> originalTaskFunctions = new List<Func<Task<IDeviceEntity>>>();
    private volatile List<Task<IDeviceEntity>> tasks = new List<Task<IDeviceEntity>>();
    private IDeviceService deviceService;
    private Timer timer;

    public TaskManager(IDeviceService deviceService)
    {
        this.deviceService = deviceService;
    }

    public void Start()
    {
        if (timer != null)
        {
            timer.Dispose();
        }
        timer = new Timer(TimerCallback, null, 0, 1000); // 每 1000 ms 執行一次
    }

    public void Stop()
    {
        if(timer != null)
        {
            timer.Dispose();
        }
    }

    private async void TimerCallback(object? state)
    {
        // 等待當前所有任務完成再繼續添加新任務
        await Task.WhenAll(tasks);

        // 檢查未完成的任務
        if (tasks.Exists(t => !t.IsCompleted))
        {
            //UpdateStatus("Some tasks are still running. Waiting for completion.");
            return;
        }

        // 清除已完成的任務並處理結果
        List<IDeviceEntity> results = new List<IDeviceEntity>();
        foreach (var task in tasks.ToList())
        {
            if (task.IsCompletedSuccessfully)
            {
                results.Add(await task);
            }
        }

        // 清除已完成的任務
        tasks.Clear();

        // 重新添加所有原始任務
        foreach (var taskFunction in originalTaskFunctions)
        {
            Task<IDeviceEntity> newTask = taskFunction();
            tasks.Add(newTask);
        }

        deviceService.ProcessResults(results);
    }

    public void AddTask(Func<Task<IDeviceEntity>> taskFunction)
    {
        originalTaskFunctions.Add(taskFunction);
    }
}