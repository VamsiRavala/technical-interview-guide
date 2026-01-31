# Thread vs Task vs Process

## Process
- A **process** is an independent program in execution with its own memory space, system resources, and state.  
- Each process is isolated from others to ensure stability and security.  
- Processes are managed by the operating system's scheduler.  

**Examples:**
- Running a web browser
- Running a text editor
- Running a database server
Process.Start("notepad.exe");
---

## Thread
- A **thread** is the smallest unit of execution within a process.  
- Threads within the same process share memory and resources but execute independently.  
- Context switching between threads is faster than between processes.  

**Examples:**
- A web browser with one thread handling the UI and another thread handling network requests.  
- A word processor with one thread for typing and another for spell-checking.  

Thread t = new Thread(() =>
{
    Console.WriteLine("Running in new thread");
});
t.Start();
---

## Task
- A **task** is a unit of work scheduled for execution.  
- In many operating systems, "task" is used interchangeably with "process" or "thread," but conceptually it represents a scheduled job.  
- A task can map to either a thread or a process depending on the OS or programming environment.

  await Task.Run(() =>
{
    Console.WriteLine("Running in a task");
});

**Examples:**
- A scheduled background backup job.  
- A database query execution.  
- An asynchronous operation in programming (e.g., async/await in Python, JavaScript).  

---

## Key Differences

| Aspect          | Process                        | Thread                           | Task                          |
|-----------------|--------------------------------|----------------------------------|-------------------------------|
| Memory          | Independent memory space       | Shares memory with process       | Depends on implementation     |
| Scheduling Unit | OS-level scheduling            | Lightweight scheduling           | Logical unit of execution     |
| Overhead        | High (more resources)          | Low (lightweight)                | Varies                        |
| Communication   | Inter-process communication    | Shared memory within process     | Depends (async/IPC/etc.)      |

---

## Summary
- **Processes** are heavy, independent, and isolated units of execution.  
- **Threads** are lightweight, sharing resources but running independently within a process.  
- **Tasks** are logical units of work that may be implemented as processes or threads depending on context.  
