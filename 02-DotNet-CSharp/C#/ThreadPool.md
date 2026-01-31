A thread pool is a collection of reusable threads managed by a scheduler.

Instead of creating a new thread for each task, tasks are submitted to the pool and assigned to available worker threads.

This reduces overhead (since creating/destroying threads is expensive).

Pools also support queuing: if all threads are busy, tasks wait in a queue until a thread is free.

This model is widely used in servers and async frameworks for handling large numbers of short-lived tasks.

```c#
using System;
using System.Threading;

class Program
{
    static void Worker(object? taskId)
    {
        Console.WriteLine($"Task {taskId} executed by Thread {Thread.CurrentThread.ManagedThreadId}");
    }

    static void Main()
    {
        for (int i = 1; i <= 5; i++)
        {
            ThreadPool.QueueUserWorkItem(Worker, i);
        }

        Console.ReadLine(); // Keep app alive until tasks finish
    }
}

  ```


## What is Task.Run?

A helper method introduced in .NET 4.5.

Runs code asynchronously on the ThreadPool.

Simplest way to say: “I want this work to run in the background.”

Always uses safe defaults, async-friendly.

await Task.Run(() => HeavyCalculation());


➡ Runs HeavyCalculation on a background thread, freeing the caller thread.

## What is Task.Factory.StartNew?

The original way (from .NET 4.0) to start tasks.

More flexible, but also more complex.

Lets you control:

Scheduler (where the task runs, e.g., UI context, custom thread pool)

Task creation options (e.g., LongRunning, DenyChildAttach)

Cancellation tokens

Task.Factory.StartNew(() => HeavyCalculation(),
    CancellationToken.None,
    TaskCreationOptions.LongRunning,
    TaskScheduler.Default);
