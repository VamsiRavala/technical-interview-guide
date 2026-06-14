> Company-specific prep — STERIS Staff Software Developer / Tech Lead interview (Volume 2 — Advanced).

# STERIS — Staff Software Developer: Volume 2 — Advanced Technical Deep Dive

*Coding Interviews | Design Patterns | Microservices on Azure | Concurrency*

**Companion to Volume 1 (Tech Lead Guide)**

Candidate: Vamsi Raavala | Interviewer: Beller, Keith

Interview: Tuesday May 12, 2026

## How to Use Volume 2

Volume 1 covered: role framing, your strengths, basic .NET/SQL/React/Azure Q&A, behavioral preparation. This volume adds the deep technical content a Staff Developer / Tech Lead is actually tested on.

### What's New in Volume 2

| Part | Content |
| ----------------------------------- | ------------------------------------------------------------------------------------------------------- |
| 9. Live Coding Interview Questions | How to approach coding rounds. 30+ algorithm and code-review questions with full solutions. |
| 10. Design Patterns Deep Dive | Creational, Structural, Behavioral, Enterprise. Full C# implementations, when to use each, trade-offs. |
| 11. Microservices on Azure | Service Bus deep config, Azure Functions deep, AKS production setup, APIM policies, full code. |
| 12. Concurrency & Threading | async/await pitfalls, locks, channels, IAsyncEnumerable, parallel patterns. |
| 13. STERIS-Specific Scenarios | Sterilization telemetry, multi-tenant SaaS, FDA 21 CFR Part 11, FHIR integration. |

### How to Read This Volume

- Skim first to know what's there. Then deep read the parts most likely to come up.
- For coding (Part 9): work through 5-10 questions actively. Type the code yourself. Don't just read.
- For patterns (Part 10): memorize the trigger sentence + when-to-use for each. Code is for reference.
- For Azure microservices (Part 11): this is your biggest gap closer. Read it twice.
- Concurrency (Part 12) and STERIS scenarios (Part 13): read once. They show up if Keith goes deep.

### The Honest Frame to Use

Even with this depth, you should not pretend to know what you haven't shipped. The point is: you'll RECOGNIZE what Keith asks. You can have an intelligent conversation. When a question goes beyond your shipped experience, fall back to:

> ***Say it like this:** "I haven't owned that in production. From what I know, the approach would be [X]. The trade-off I'd watch for is [Y]. Before committing in a real system, I'd want to spike it."*

## PART 9 — LIVE CODING INTERVIEW QUESTIONS

Staff developer interviews include hands-on coding. Keith may ask you to write code on a whiteboard, in a shared editor (CoderPad, HackerRank), or just talk through the solution. These are real questions tech leads get asked.

### Section 9.1 — How to Approach Coding Questions

**Q121. What's the right way to approach a live coding question?**

**A.** Don't start typing. Use this 5-step framework every time:

**1. Clarify (1-2 min):** What's the input? Output? Edge cases? Empty input? Null? Negative numbers? Duplicates? Size constraints?

**2. Examples (1 min):** Walk through 1-2 examples manually. Confirms you understand the problem.

**3. Approach (2-3 min):** Describe your solution in plain English BEFORE coding. Discuss time/space complexity. Get interviewer agreement.

**4. Code (10-15 min):** Write clean, readable code. Talk through what you're doing. Use meaningful names.

**5. Test (3-5 min):** Walk through your code with the original example. Check edge cases. Fix bugs.

> ***Say it like this:** "Before I write code, can I confirm the input format and edge cases? — That sentence alone signals senior. Most candidates start typing immediately."*

**Q122. What if I don't know the optimal solution?**

**A.** Start with brute force. Then optimize.

> ***Say it like this:** "My first instinct is a brute-force O(n²) solution using nested loops. Let me write that first to make sure I understand the problem, then I'll see if there's a better approach. — That's senior. Pretending to know the optimal answer when you don't is junior."*

**Q123. What if I get stuck mid-solution?**

**A.** Talk through what you know. Don't go silent.

- 'I know the result for the first iteration. Let me see what changes for the second...'
- 'I'm thinking through whether to use a hash map or sort first...'
- 'Let me trace through with example values...'

Interviewers can help if they know where you're stuck. Silence helps no one.

### Section 9.2 — Strings & Arrays

**Q124. Reverse a string in C#.**

**A.** Multiple approaches. Show you know them all and discuss trade-offs:

```csharp
// Approach 1: Built-in (best for production)
string Reverse(string s) => new string(s.Reverse().ToArray());

// Approach 2: Char array swap (O(n) time, O(1) extra space)
string ReverseManual(string s) {
    var chars = s.ToCharArray();
    int left = 0, right = chars.Length - 1;
    while (left < right) {
        (chars[left], chars[right]) = (chars[right], chars[left]);
        left++; right--;
    }
    return new string(chars);
}

// Approach 3: Span-based (zero extra allocation, modern .NET)
string ReverseSpan(string s) {
    Span<char> chars = stackalloc char[s.Length];
    s.AsSpan().CopyTo(chars);
    chars.Reverse();
    return new string(chars);
}
```

> **Tip:** Watch out: Unicode surrogate pairs! 'a😀b'.Reverse() may produce broken characters. Use StringInfo for proper grapheme-aware reverse.

**Q125. Check if a string is a palindrome.**

**A.** Two-pointer approach. Walk from both ends:

```csharp
bool IsPalindrome(string s) {
    int left = 0, right = s.Length - 1;
    while (left < right) {
        if (s[left] != s[right]) return false;
        left++; right--;
    }
    return true;
}

// Real-world variant: ignore case and non-alphanumerics
bool IsPalindromeRobust(string s) {
    int left = 0, right = s.Length - 1;
    while (left < right) {
        while (left < right && !char.IsLetterOrDigit(s[left])) left++;
        while (left < right && !char.IsLetterOrDigit(s[right])) right--;
        if (char.ToLower(s[left]) != char.ToLower(s[right])) return false;
        left++; right--;
    }
    return true;
}
```

Time: O(n). Space: O(1). The robust version is what real-world inputs need.

**Q126. Find the first non-repeating character.**

**A.** Hash map for counts, second pass to find first with count=1:

```csharp
char? FirstNonRepeating(string s) {
    var counts = new Dictionary<char, int>();
    foreach (var ch in s) {
        counts[ch] = counts.GetValueOrDefault(ch, 0) + 1;
    }
    foreach (var ch in s) {
        if (counts[ch] == 1) return ch;
    }
    return null;
}
// Time: O(n), Space: O(k) where k = unique chars
```

Why two passes? Need to find FIRST in original order. Single pass would give A non-repeating char but not necessarily the first.

**Q127. Two Sum: find indices that sum to target.**

**A.** Classic. Hash map for O(n) instead of O(n²) brute force:

```csharp
int[] TwoSum(int[] nums, int target) {
    var seen = new Dictionary<int, int>(); // value -> index
    for (int i = 0; i < nums.Length; i++) {
        int complement = target - nums[i];
        if (seen.TryGetValue(complement, out int j)) {
            return new[] { j, i };
        }
        seen[nums[i]] = i;
    }
    return Array.Empty<int>();
}
```

> ***Say it like this:** "I could do nested loops O(n²), but with a hash map I can do single pass in O(n). For each number, I check if its complement is already seen."*

**Q128. Find all duplicates in an array.**

```csharp
// HashSet approach
List<int> FindDuplicates(int[] nums) {
    var seen = new HashSet<int>();
    var dupes = new List<int>();
    foreach (var n in nums) {
        if (!seen.Add(n)) dupes.Add(n);
    }
    return dupes;
}

// LINQ for production
var dupes = nums.GroupBy(x => x)
    .Where(g => g.Count() > 1)
    .Select(g => g.Key)
    .ToList();
```

HashSet.Add returns true if new, false if exists. Time: O(n), Space: O(n).

**Q129. Reverse a linked list.**

```csharp
public class ListNode {
    public int Val;
    public ListNode? Next;
    public ListNode(int val, ListNode? next = null) {
        Val = val; Next = next;
    }
}

ListNode? Reverse(ListNode? head) {
    ListNode? prev = null;
    ListNode? curr = head;
    while (curr != null) {
        var next = curr.Next; // save next
        curr.Next = prev;     // reverse pointer
        prev = curr;          // advance prev
        curr = next;          // advance curr
    }
    return prev; // new head
}
```

Time: O(n), Space: O(1). The three-pointer dance (prev/curr/next) is the classic pattern.

**Q130. Find middle of a linked list (without knowing length).**

**A.** Two-pointer technique (Floyd's tortoise and hare):

```csharp
ListNode? FindMiddle(ListNode? head) {
    var slow = head;
    var fast = head;
    while (fast?.Next != null) {
        slow = slow!.Next;
        fast = fast.Next.Next;
    }
    return slow;
}
// Slow moves 1 step, fast 2 steps
// When fast reaches end, slow is at middle
// Same pattern detects cycles
```

### Section 9.3 — Real-Time Business Logic

**Q131. Implement an in-memory cache with TTL.**

**A.** Common interview question — tests concurrency, threading, expiration:

```csharp
public class TtlCache<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, (TValue Value, DateTime Expiry)> _store
        = new();
    private readonly TimeSpan _ttl;

    public TtlCache(TimeSpan ttl) => _ttl = ttl;

    public void Set(TKey key, TValue value)
    {
        _store[key] = (value, DateTime.UtcNow.Add(_ttl));
    }

    public bool TryGet(TKey key, out TValue? value)
    {
        if (_store.TryGetValue(key, out var entry))
        {
            if (entry.Expiry > DateTime.UtcNow)
            {
                value = entry.Value;
                return true;
            }
            _store.TryRemove(key, out _); // expired
        }
        value = default;
        return false;
    }
}
```

Talk about: thread safety (ConcurrentDictionary), lazy expiration (only on read), background cleanup of expired items as a trade-off. In production: use IMemoryCache.

**Q132. Implement a rate limiter (N calls per minute per user).**

```csharp
public class RateLimiter
{
    private readonly int _maxCalls;
    private readonly TimeSpan _window;
    private readonly ConcurrentDictionary<string, ConcurrentQueue<DateTime>> _calls
        = new();

    public RateLimiter(int maxCalls, TimeSpan window)
    {
        _maxCalls = maxCalls;
        _window = window;
    }

    public bool TryAcquire(string userId)
    {
        var queue = _calls.GetOrAdd(userId, _ => new ConcurrentQueue<DateTime>());
        var now = DateTime.UtcNow;
        var cutoff = now - _window;

        // Remove expired entries from front
        while (queue.TryPeek(out var oldest) && oldest < cutoff)
            queue.TryDequeue(out _);

        if (queue.Count >= _maxCalls) return false;
        queue.Enqueue(now);
        return true;
    }
}
// Production: System.Threading.RateLimiting (.NET 7+) or APIM-level
```

**Q133. Implement retry with exponential backoff.**

```csharp
public static async Task<T> RetryAsync<T>(
    Func<Task<T>> operation,
    int maxAttempts = 3,
    TimeSpan? initialDelay = null,
    CancellationToken ct = default)
{
    var delay = initialDelay ?? TimeSpan.FromMilliseconds(100);
    var random = new Random();

    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex) when (
            ex is HttpRequestException ||
            ex is TimeoutException)
        {
            if (attempt == maxAttempts) throw;
            // Exponential + jitter to avoid thundering herd
            var jitter = random.Next(0, 100);
            await Task.Delay(delay + TimeSpan.FromMilliseconds(jitter), ct);
            delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
        }
    }
    throw new InvalidOperationException("Unreachable");
}
// Production: use Polly. Interviewer wants to see you understand the pattern.
```

**Q134. Thread-safe singleton in C#.**

```csharp
// Best: Lazy<T> (thread-safe by default)
public sealed class Logger
{
    private static readonly Lazy<Logger> _instance =
        new(() => new Logger());
    public static Logger Instance => _instance.Value;
    private Logger() { }
    public void Log(string msg) =>
        Console.WriteLine($"{DateTime.UtcNow:o} {msg}");
}

// Modern alternative: just use DI
services.AddSingleton<ILogger, Logger>();
```

> ***Say it like this:** "I'd actually use DI in a real app — same lifetime guarantee, more testable. The Singleton class pattern is for when you can't use DI."*

**Q135. Find longest substring without repeating characters.**

**A.** Sliding window technique:

```csharp
int LongestUnique(string s)
{
    var seen = new Dictionary<char, int>(); // char -> last position
    int start = 0, maxLen = 0;
    for (int i = 0; i < s.Length; i++)
    {
        if (seen.TryGetValue(s[i], out int prev) && prev >= start)
        {
            start = prev + 1; // skip past previous occurrence
        }
        seen[s[i]] = i;
        maxLen = Math.Max(maxLen, i - start + 1);
    }
    return maxLen;
}
```

**Q136. Group anagrams together.**

```csharp
// LINQ approach
List<List<string>> GroupAnagrams(string[] words)
{
    return words
        .GroupBy(w => new string(w.OrderBy(c => c).ToArray()))
        .Select(g => g.ToList())
        .ToList();
}

// Manual approach
List<List<string>> GroupAnagramsManual(string[] words)
{
    var groups = new Dictionary<string, List<string>>();
    foreach (var word in words)
    {
        var key = new string(word.OrderBy(c => c).ToArray());
        if (!groups.ContainsKey(key)) groups[key] = new List<string>();
        groups[key].Add(word);
    }
    return groups.Values.ToList();
}
```

**Q137. Calculate moving average over last N values.**

**A.** Common in real-time analytics. Queue-based:

```csharp
public class MovingAverage
{
    private readonly Queue<double> _values = new();
    private readonly int _windowSize;
    private double _sum;

    public MovingAverage(int windowSize) => _windowSize = windowSize;

    public double Add(double value)
    {
        _values.Enqueue(value);
        _sum += value;
        if (_values.Count > _windowSize)
            _sum -= _values.Dequeue();
        return _sum / _values.Count;
    }
}
// O(1) per update — sum maintained incrementally
// Critical for high-frequency data (telemetry, financial)
```

### Section 9.4 — Code Review (Find the Bug)

**Q138. What's wrong with this code? (Async deadlock)**

```csharp
public string GetUserName(int id)
{
    var user = _userService.GetAsync(id).Result;
    return user.Name;
}
```

**A.** Bug: .Result blocks the thread. In ASP.NET classic / WinForms can deadlock — the awaited task tries to resume on the captured sync context, but the thread is blocked.

```csharp
// FIX: async all the way
public async Task<string> GetUserNameAsync(int id)
{
    var user = await _userService.GetAsync(id);
    return user.Name;
}
// Rule: never .Result, .Wait(), or .GetAwaiter().GetResult()
```

**Q139. What's wrong with this LINQ?**

```csharp
var orders = _db.Orders
    .AsEnumerable()
    .Where(o => o.Total > 100)
    .ToList();
```

**A.** Bug: AsEnumerable() forces ALL orders to load into memory FIRST. Filtering happens in C# instead of SQL. Kills performance with large tables.

```csharp
// FIX: keep IQueryable
var orders = _db.Orders
    .Where(o => o.Total > 100) // SQL: WHERE Total > 100
    .ToList();                 // Then loads matching rows only
```

Use AsEnumerable() ONLY for C#-side functions EF can't translate. Even then, filter as much as possible in SQL first.

**Q140. What's wrong with this DI?**

```csharp
services.AddSingleton<AppDbContext>();
services.AddSingleton<IOrderService, OrderService>();
```

**A.** Bug: DbContext as Singleton. Two problems:

- DbContext is NOT thread-safe. Concurrent requests will corrupt state.
- DbContext caches all tracked entities — singleton would leak memory forever.

```csharp
// FIX: Scoped (default for AddDbContext)
services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlServer(connStr)); // Scoped automatically
services.AddScoped<IOrderService, OrderService>();
```

**Q141. What's wrong with this concurrent code?**

```csharp
public class Counter
{
    private int _count = 0;
    public void Increment() => _count++;
    public int Value => _count;
}
```

**A.** Bug: _count++ is NOT atomic. It's read, increment, write. Two threads can read 5, both write 6 — you lose increments.

```csharp
// Option 1: Interlocked (cheapest)
private long _count = 0;
public void Increment() => Interlocked.Increment(ref _count);

// Option 2: lock (when more complex logic needed)
private readonly object _gate = new();
public void Increment()
{
    lock (_gate) { _count++; }
}
```

**Q142. What's wrong with this exception handling?**

```csharp
try {
    await DoWorkAsync();
} catch (Exception ex) {
    _logger.LogError("Error occurred");
}
```

**A.** Two bugs:

- Catching generic Exception swallows everything, including programmer errors.
- Logger doesn't get the exception — just a static string. Stack trace lost.

```csharp
try {
    await DoWorkAsync();
} catch (HttpRequestException ex) { // SPECIFIC exception
    _logger.LogError(ex, "Failed to call external API");
    throw; // or return error response, or fallback
}
```

**Q143. What's wrong with this caching code? (Cache stampede)**

```csharp
public async Task<User> GetUserAsync(int id)
{
    if (_cache.TryGetValue(id, out User cached))
        return cached;
    var user = await _db.Users.FindAsync(id);
    _cache.Set(id, user);
    return user;
}
```

**A.** Two bugs:

- Cache stampede: 1000 concurrent requests for same id all miss, all hit DB.
- No TTL — data cached forever, becomes stale.

```csharp
// FIX: GetOrCreateAsync (built-in single-flight)
public async Task<User> GetUserAsync(int id)
{
    return await _cache.GetOrCreateAsync($"user:{id}", async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
        return await _db.Users.FindAsync(id);
    });
}
```

**Q144. Spot the SQL injection.**

```csharp
public List<Order> Search(string customerName)
{
    var sql = $"SELECT * FROM Orders WHERE CustomerName = '{customerName}'";
    return _db.Orders.FromSqlRaw(sql).ToList();
}
```

**A.** Massive SQL injection. Input "' OR '1'='1" returns ALL orders. "'; DROP TABLE Orders; --" deletes everything.

```csharp
// FIX option 1: FromSqlInterpolated (parameters are SAFE)
var orders = _db.Orders
    .FromSqlInterpolated($"SELECT * FROM Orders WHERE CustomerName = {customerName}")
    .ToList();

// FIX option 2: LINQ (always safe)
var orders = _db.Orders
    .Where(o => o.CustomerName == customerName)
    .ToList();
```

**Q145. What's wrong with this disposable usage?**

```csharp
public string ReadFile(string path)
{
    var stream = new FileStream(path, FileMode.Open);
    var reader = new StreamReader(stream);
    return reader.ReadToEnd();
}
```

**A.** Stream and reader never disposed. File handle leaks until GC eventually finalizes.

```csharp
// FIX: using declarations (C# 8+)
public string ReadFile(string path)
{
    using var stream = new FileStream(path, FileMode.Open);
    using var reader = new StreamReader(stream);
    return reader.ReadToEnd();
}

// Even simpler: File.ReadAllText handles disposal
public string ReadFile(string path) => File.ReadAllText(path);
```

### Section 9.5 — System Design Coding

**Q146. Design a class to track API request metrics.**

**A.** Real-time tech-lead question. Show thread-safety, atomicity, observability awareness:

```csharp
public class RequestMetrics
{
    private long _totalCount = 0;
    private long _errorCount = 0;
    private long _totalDurationMs = 0;

    public void Record(TimeSpan duration, bool success)
    {
        Interlocked.Increment(ref _totalCount);
        Interlocked.Add(ref _totalDurationMs, (long)duration.TotalMilliseconds);
        if (!success) Interlocked.Increment(ref _errorCount);
    }

    public MetricsSnapshot GetSnapshot()
    {
        var total = Interlocked.Read(ref _totalCount);
        var errors = Interlocked.Read(ref _errorCount);
        var duration = Interlocked.Read(ref _totalDurationMs);
        return new MetricsSnapshot
        {
            TotalRequests = total,
            ErrorRate = total > 0 ? (double)errors / total : 0,
            AvgDurationMs = total > 0 ? (double)duration / total : 0
        };
    }
}
```

> ***Say it like this:** "In production, I wouldn't write this — Application Insights, Prometheus, OpenTelemetry handle it. But the interview wants to see I understand thread-safe counters and atomic operations."*

**Q147. Implement a URL shortener service.**

```csharp
public class UrlShortener
{
    private readonly ConcurrentDictionary<string, string> _store = new();
    private const string Chars =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private readonly Random _random = new();

    public string Shorten(string longUrl)
    {
        for (int attempt = 0; attempt < 5; attempt++)
        {
            var code = GenerateCode(6);
            if (_store.TryAdd(code, longUrl))
                return code;
        }
        throw new InvalidOperationException("Failed to generate unique code");
    }

    public string? Resolve(string code) =>
        _store.TryGetValue(code, out var url) ? url : null;

    private string GenerateCode(int length)
    {
        var chars = new char[length];
        for (int i = 0; i < length; i++)
            chars[i] = Chars[_random.Next(Chars.Length)];
        return new string(chars);
    }
}
// 62^6 = 56 billion combinations. Collision rare for low volume.
// Production: counter with base62 encoding for guaranteed unique.
```

**Q148. Implement a publish-subscribe event bus.**

```csharp
public interface IEventHandler<TEvent>
{
    Task HandleAsync(TEvent evt, CancellationToken ct);
}

public class EventBus
{
    private readonly ConcurrentDictionary<Type, List<object>> _handlers = new();

    public void Subscribe<TEvent>(IEventHandler<TEvent> handler)
    {
        var list = _handlers.GetOrAdd(typeof(TEvent), _ => new List<object>());
        lock (list) { list.Add(handler); }
    }

    public async Task PublishAsync<TEvent>(TEvent evt, CancellationToken ct = default)
    {
        if (!_handlers.TryGetValue(typeof(TEvent), out var list)) return;
        List<object> snapshot;
        lock (list) { snapshot = new List<object>(list); }
        var tasks = snapshot
            .Cast<IEventHandler<TEvent>>()
            .Select(h => h.HandleAsync(evt, ct));
        await Task.WhenAll(tasks);
    }
}
// Production: MediatR for in-process, Service Bus for distributed
```

**Q149. Top-K most frequent items in a stream.**

```csharp
public class TopKTracker<T> where T : notnull
{
    private readonly Dictionary<T, int> _counts = new();
    private readonly int _k;

    public TopKTracker(int k) => _k = k;

    public void Add(T item) =>
        _counts[item] = _counts.GetValueOrDefault(item, 0) + 1;

    public List<T> GetTopK() =>
        _counts.OrderByDescending(kv => kv.Value)
            .Take(_k)
            .Select(kv => kv.Key)
            .ToList();
}
// For TRUE streaming (millions/sec), use:
// - Count-Min Sketch (probabilistic, fixed memory)
// - PriorityQueue<T, TPriority> for top-K with heap
// - Production: real OLAP (Synapse, Druid)
```

**Q150. Implement a state machine for an order workflow.**

```csharp
public enum OrderState { Pending, Paid, Shipped, Delivered, Cancelled }

public class OrderStateMachine
{
    private static readonly Dictionary<OrderState, HashSet<OrderState>> Transitions
        = new()
    {
        [OrderState.Pending] = new() { OrderState.Paid, OrderState.Cancelled },
        [OrderState.Paid] = new() { OrderState.Shipped, OrderState.Cancelled },
        [OrderState.Shipped] = new() { OrderState.Delivered },
        [OrderState.Delivered] = new() { }, // terminal
        [OrderState.Cancelled] = new() { }  // terminal
    };

    public bool CanTransition(OrderState from, OrderState to) =>
        Transitions.TryGetValue(from, out var allowed) && allowed.Contains(to);

    public OrderState Transition(OrderState from, OrderState to)
    {
        if (!CanTransition(from, to))
            throw new InvalidOperationException($"Cannot transition {from} -> {to}");
        return to;
    }
}
// Production: use Stateless library or MassTransit Saga
```

### Section 9.6 — Tips for Coding Interviews

**Q151. How do I handle a problem I've never seen?**

**A.** Strategy:

- Simplify. Solve a smaller version (N=1, N=2).
- Look for patterns: graph? tree? sliding window? two pointers?
- Brute force first. Then ask: where am I doing redundant work?
- Common optimizations: hash for O(1) lookup, sort for ordering, two pointers for arrays.
- Talk through your reasoning. Interviewers may give hints.

**Q152. What if I run out of time?**

**A.** Don't panic. Show what you have:

> ***Say it like this:** "I haven't fully solved this in the time, but here's where I am: [your approach]. Given more time, I'd handle [edge case], optimize [part], and add tests for [scenarios]. Want me to talk through the rest verbally?"*

**Q153. LINQ or manual loops?**

**A.** Both. Show you know LINQ for production AND can implement manually for clarity.

> ***Say it like this:** "I'd write this with LINQ in production for readability. Now in case you want to see the manual implementation to verify I understand the algorithm... — Then write both."*

**Q154. Do I need to mention Big-O?**

**A.** Yes, always. After writing your solution:

> ***Say it like this:** "Time complexity is O(n log n) because of the sort. Space is O(n) for the hash map. Could we do better? With more space, yes — using a frequency map we drop to O(n) time. Trade-off is memory."*

## PART 10 — DESIGN PATTERNS DEEP DIVE

Tech leads must know patterns deeply — when to apply, how to implement in C#, what trade-offs come with each. Full working code for the patterns Keith is most likely to ask about.

### Section 10.1 — How to Talk About Patterns

**Q155. What's the right way to discuss patterns in interviews?**

**A.** Three rules:

- Name the pattern explicitly: 'I'd model this as a Strategy pattern.' Signals fluency.
- State the trigger sentence — when you'd use it. 'Strategy is for when you have multiple algorithms for the same task, selectable at runtime.'
- Mention the trade-off. Every pattern has a cost — flexibility, complexity, indirection.

> ***Say it like this:** "For this problem, I'd use the Decorator pattern — multiple cross-cutting concerns layered on a single core operation. Trade-off: more classes, more indirection. Worth it because we can swap each behavior independently and test them in isolation."*

### Section 10.2 — Creational Patterns

**Q156. Singleton with Lazy\<T> — show me the implementation.**

```csharp
public sealed class ConnectionPool
{
    private static readonly Lazy<ConnectionPool> _instance =
        new(() => new ConnectionPool());
    public static ConnectionPool Instance => _instance.Value;

    private readonly ConcurrentBag<DbConnection> _pool = new();

    private ConnectionPool() { /* expensive init */ }

    public DbConnection Rent() { /* ... */ }
    public void Return(DbConnection conn) { /* ... */ }
}

// Why Lazy<T>:
// 1. Thread-safe by default
// 2. Initialized only on first use
// 3. No double-check locking boilerplate

// Better still: use DI with Singleton lifetime
services.AddSingleton<ConnectionPool>();
```

Trade-off: Singleton class is global, hostile to testing. Modern preference: DI singleton lifetime.

**Q157. Factory pattern with DI integration.**

```csharp
public interface IPaymentProcessor
{
    Task<bool> ChargeAsync(decimal amount);
}

public class StripeProcessor : IPaymentProcessor { /* ... */ }
public class PayPalProcessor : IPaymentProcessor { /* ... */ }

public interface IPaymentProcessorFactory
{
    IPaymentProcessor Create(PaymentMethod method);
}

public class PaymentProcessorFactory : IPaymentProcessorFactory
{
    private readonly IServiceProvider _services;
    public PaymentProcessorFactory(IServiceProvider services) =>
        _services = services;

    public IPaymentProcessor Create(PaymentMethod method) => method switch
    {
        PaymentMethod.Card => _services.GetRequiredService<StripeProcessor>(),
        PaymentMethod.PayPal => _services.GetRequiredService<PayPalProcessor>(),
        _ => throw new NotSupportedException()
    };
}

// Registration
services.AddTransient<StripeProcessor>();
services.AddTransient<PayPalProcessor>();
services.AddSingleton<IPaymentProcessorFactory, PaymentProcessorFactory>();
```

Reach for Factory when: choice depends on RUNTIME data the DI container doesn't have at registration time.

**Q158. Builder pattern for complex object construction.**

```csharp
public class HttpRequestBuilder
{
    private readonly HttpRequestMessage _request = new();
    private readonly Dictionary<string, string> _query = new();

    public HttpRequestBuilder Method(HttpMethod method)
    {
        _request.Method = method;
        return this;
    }

    public HttpRequestBuilder Url(string url)
    {
        _request.RequestUri = new Uri(url);
        return this;
    }

    public HttpRequestBuilder Header(string key, string value)
    {
        _request.Headers.Add(key, value);
        return this;
    }

    public HttpRequestBuilder JsonBody<T>(T body)
    {
        _request.Content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8, "application/json");
        return this;
    }

    public HttpRequestMessage Build() => _request;
}

// Usage
var request = new HttpRequestBuilder()
    .Method(HttpMethod.Post)
    .Url("https://api.example.com/orders")
    .Header("Authorization", $"Bearer {token}")
    .JsonBody(new { customerId = 123 })
    .Build();
```

Builder shines when: many optional parts, validation at Build() time, fluent readability matters.

### Section 10.3 — Structural Patterns

**Q159. Decorator chaining for cross-cutting concerns.**

```csharp
public interface IOrderRepository
{
    Task<Order?> GetAsync(Guid id);
    Task SaveAsync(Order order);
}

// Core implementation
public class SqlOrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;
    public SqlOrderRepository(AppDbContext db) => _db = db;

    public Task<Order?> GetAsync(Guid id) => _db.Orders.FindAsync(id).AsTask();

    public async Task SaveAsync(Order order)
    {
        _db.Orders.Update(order);
        await _db.SaveChangesAsync();
    }
}

// Decorator: caching
public class CachedOrderRepository : IOrderRepository
{
    private readonly IOrderRepository _inner;
    private readonly IMemoryCache _cache;

    public CachedOrderRepository(IOrderRepository inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<Order?> GetAsync(Guid id) =>
        await _cache.GetOrCreateAsync($"order:{id}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await _inner.GetAsync(id);
        });

    public async Task SaveAsync(Order order)
    {
        await _inner.SaveAsync(order);
        _cache.Remove($"order:{order.Id}");
    }
}

// Decorator: logging
public class LoggingOrderRepository : IOrderRepository
{
    private readonly IOrderRepository _inner;
    private readonly ILogger _logger;

    public LoggingOrderRepository(IOrderRepository inner, ILogger<LoggingOrderRepository> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<Order?> GetAsync(Guid id)
    {
        var sw = Stopwatch.StartNew();
        var result = await _inner.GetAsync(id);
        _logger.LogInformation("GetAsync({Id}) took {Ms}ms", id, sw.ElapsedMilliseconds);
        return result;
    }

    public Task SaveAsync(Order order) => _inner.SaveAsync(order);
}

// Registration with chained decorators
services.AddScoped<SqlOrderRepository>();
services.AddScoped<IOrderRepository>(sp =>
    new LoggingOrderRepository(
        new CachedOrderRepository(
            sp.GetRequiredService<SqlOrderRepository>(),
            sp.GetRequiredService<IMemoryCache>()),
        sp.GetRequiredService<ILogger<LoggingOrderRepository>>()));

// Or use Scrutor: services.Decorate<IOrderRepository, CachedOrderRepository>();
```

This is exactly how ASP.NET Core middleware works internally. Each decorator handles one concern.

**Q160. Adapter pattern for legacy integration.**

```csharp
// Legacy COM API (you can't change this)
public class LegacyComPromotionApi
{
    public LegacyComPromotion GetPromotion(string id) { /* COM */ }
}

public class LegacyComPromotion
{
    public string PromoCode { get; set; }  // legacy naming
    public string ItemList { get; set; }   // pipe-delimited string
    public int DiscountPct { get; set; }   // integer 0-100
}

// Your clean domain model
public class Promotion
{
    public Guid Id { get; init; }
    public List<string> ApplicableItems { get; init; }
    public decimal Discount { get; init; } // 0.0-1.0
}

public interface IPromotionService
{
    Promotion Get(Guid id);
}

// The Adapter — contains all ugly translation
public class LegacyComAdapter : IPromotionService
{
    private readonly LegacyComPromotionApi _legacy;
    public LegacyComAdapter(LegacyComPromotionApi legacy) => _legacy = legacy;

    public Promotion Get(Guid id)
    {
        var legacyPromo = _legacy.GetPromotion(id.ToString());
        return new Promotion
        {
            Id = id,
            ApplicableItems = legacyPromo.ItemList.Split('|').ToList(),
            Discount = legacyPromo.DiscountPct / 100m
        };
    }
}
```

Same pattern as your VB6 → .NET migration. Adapter contains all the ugly translation.

**Q161. Facade pattern hiding subsystem complexity.**

```csharp
// Subsystems (each independent)
public class InventoryService { /* check stock, reserve */ }
public class PaymentService { /* charge card */ }
public class ShippingService { /* schedule pickup */ }
public class NotificationService { /* email customer */ }

// Facade — single interface for the common case
public class OrderFacade
{
    private readonly InventoryService _inventory;
    private readonly PaymentService _payment;
    private readonly ShippingService _shipping;
    private readonly NotificationService _notification;

    public OrderFacade(InventoryService inv, PaymentService pay,
        ShippingService ship, NotificationService notif)
    {
        _inventory = inv; _payment = pay;
        _shipping = ship; _notification = notif;
    }

    public async Task<OrderResult> PlaceOrderAsync(OrderRequest request)
    {
        var reservation = await _inventory.ReserveAsync(request.Items);
        try
        {
            var charge = await _payment.ChargeAsync(request.Card, request.Total);
            var shipment = await _shipping.ScheduleAsync(request.Address);
            await _notification.SendConfirmationAsync(request.Email);
            return new OrderResult { Success = true, OrderId = charge.Id };
        }
        catch
        {
            await _inventory.ReleaseAsync(reservation.Id);
            throw;
        }
    }
}
// Caller deals with one simple API instead of 4 subsystems
```

**Q162. Proxy pattern for lazy loading.**

```csharp
public interface IReportService
{
    Report GenerateReport();
}

public class ExpensiveReportService : IReportService
{
    public ExpensiveReportService() { /* loads 1GB of data */ }
    public Report GenerateReport() { /* ... */ }
}

// Lazy proxy — defers creation until first use
public class LazyReportProxy : IReportService
{
    private readonly Lazy<IReportService> _service;

    public LazyReportProxy(Func<IReportService> factory)
    {
        _service = new Lazy<IReportService>(factory);
    }

    public Report GenerateReport() => _service.Value.GenerateReport();
}

// Real-world: EF Core uses Lazy Loading proxies for navigation properties.
// EF generates a proxy class that loads related entities only when accessed.
```

### Section 10.4 — Behavioral Patterns

**Q163. Strategy pattern with DI for runtime selection.**

```csharp
public interface IDiscountStrategy
{
    string Name { get; }
    decimal Apply(Cart cart);
}

public class PercentageDiscount : IDiscountStrategy
{
    public string Name => "Percentage";
    public decimal Apply(Cart cart) => cart.Subtotal * 0.10m;
}

public class BogoDiscount : IDiscountStrategy
{
    public string Name => "BOGO";
    public decimal Apply(Cart cart) => cart.Items
        .GroupBy(i => i.Sku)
        .Sum(g => (g.Count() / 2) * g.First().Price);
}

// Selector — picks strategy by name
public class DiscountStrategySelector
{
    private readonly IDictionary<string, IDiscountStrategy> _strategies;

    public DiscountStrategySelector(IEnumerable<IDiscountStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(s => s.Name);
    }

    public IDiscountStrategy Get(string name) =>
        _strategies.TryGetValue(name, out var s) ? s :
            throw new ArgumentException($"Unknown strategy: {name}");
}

// Registration — all strategies auto-registered
services.AddScoped<IDiscountStrategy, PercentageDiscount>();
services.AddScoped<IDiscountStrategy, BogoDiscount>();
services.AddSingleton<DiscountStrategySelector>();
```

New discount type? Just add a class. Doesn't touch CheckoutService. Open/Closed in action.

**Q164. Observer pattern with C# events.**

```csharp
// Event arguments
public class OrderPlacedEventArgs : EventArgs
{
    public Guid OrderId { get; init; }
    public decimal Total { get; init; }
    public DateTime Timestamp { get; init; }
}

// Subject
public class OrderService
{
    public event EventHandler<OrderPlacedEventArgs>? OrderPlaced;

    public async Task<Guid> PlaceOrderAsync(OrderRequest request)
    {
        var order = await SaveOrderAsync(request);
        OrderPlaced?.Invoke(this, new OrderPlacedEventArgs
        {
            OrderId = order.Id,
            Total = order.Total,
            Timestamp = DateTime.UtcNow
        });
        return order.Id;
    }
}

// Observers
public class EmailNotifier
{
    public EmailNotifier(OrderService service)
    {
        service.OrderPlaced += OnOrderPlaced;
    }

    private async void OnOrderPlaced(object? sender, OrderPlacedEventArgs e)
    {
        await SendEmailAsync($"Order {e.OrderId} placed");
    }
}

// Modern alternative: MediatR domain events
// Cross-service: Service Bus topic with subscriptions
```

**Q165. Mediator pattern with MediatR (the standard).**

```csharp
// Install: dotnet add package MediatR

// Command
public record CreateOrderCommand(string CustomerId, List<OrderItem> Items)
    : IRequest<Guid>;

// Handler
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly AppDbContext _db;
    private readonly IMediator _mediator;

    public CreateOrderHandler(AppDbContext db, IMediator mediator)
    {
        _db = db;
        _mediator = mediator;
    }

    public async Task<Guid> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        var order = new Order(cmd.CustomerId, cmd.Items);
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);
        await _mediator.Publish(new OrderCreatedNotification(order.Id), ct);
        return order.Id;
    }
}

// Notification
public record OrderCreatedNotification(Guid OrderId) : INotification;

// Multiple handlers run in parallel
public class SendEmailHandler : INotificationHandler<OrderCreatedNotification>
{
    public Task Handle(OrderCreatedNotification n, CancellationToken ct) =>
        SendEmailAsync(n.OrderId);
}

public class UpdateAnalyticsHandler : INotificationHandler<OrderCreatedNotification>
{
    public Task Handle(OrderCreatedNotification n, CancellationToken ct) =>
        TrackEventAsync("OrderCreated", n.OrderId);
}

// Registration
services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

// In controller
var orderId = await _mediator.Send(new CreateOrderCommand(customerId, items));
```

Why MediatR is standard: clean separation, easy to test handlers in isolation, supports pipeline behaviors (logging, validation, transactions) as cross-cutting decorators.

**Q166. Chain of Responsibility (it's how middleware works).**

```csharp
// ASP.NET Core middleware IS Chain of Responsibility
public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimiter _limiter;

    public RateLimitMiddleware(RequestDelegate next, RateLimiter limiter)
    {
        _next = next;
        _limiter = limiter;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        var userId = ctx.User.FindFirst("sub")?.Value ?? "anonymous";
        if (!_limiter.TryAcquire(userId))
        {
            ctx.Response.StatusCode = 429;
            await ctx.Response.WriteAsync("Too Many Requests");
            return; // short-circuit
        }
        await _next(ctx); // pass to next handler
    }
}

// In Program.cs
app.UseAuthentication();
app.UseMiddleware<RateLimitMiddleware>();
app.UseAuthorization();
app.MapControllers();
// Each is a handler in the chain. Each calls next() or short-circuits.
```

**Q167. Command pattern for undo/redo.**

```csharp
public interface ICommand
{
    void Execute();
    void Undo();
}

public class AddTextCommand : ICommand
{
    private readonly TextEditor _editor;
    private readonly string _text;
    private readonly int _position;

    public AddTextCommand(TextEditor editor, string text, int position)
    {
        _editor = editor;
        _text = text;
        _position = position;
    }

    public void Execute() => _editor.InsertText(_position, _text);
    public void Undo() => _editor.RemoveText(_position, _text.Length);
}

public class CommandManager
{
    private readonly Stack<ICommand> _undoStack = new();
    private readonly Stack<ICommand> _redoStack = new();

    public void Execute(ICommand cmd)
    {
        cmd.Execute();
        _undoStack.Push(cmd);
        _redoStack.Clear(); // new action invalidates redo
    }

    public void Undo()
    {
        if (_undoStack.Count == 0) return;
        var cmd = _undoStack.Pop();
        cmd.Undo();
        _redoStack.Push(cmd);
    }

    public void Redo()
    {
        if (_redoStack.Count == 0) return;
        var cmd = _redoStack.Pop();
        cmd.Execute();
        _undoStack.Push(cmd);
    }
}
```

**Q168. State pattern for an order workflow.**

```csharp
public abstract class OrderState
{
    public abstract OrderState Pay(Order order);
    public abstract OrderState Ship(Order order);
    public abstract OrderState Cancel(Order order);
    public abstract string Name { get; }
}

public class PendingState : OrderState
{
    public override string Name => "Pending";

    public override OrderState Pay(Order order)
    {
        order.PaidAt = DateTime.UtcNow;
        return new PaidState();
    }

    public override OrderState Ship(Order order) =>
        throw new InvalidOperationException("Cannot ship unpaid order");

    public override OrderState Cancel(Order order) => new CancelledState();
}

public class PaidState : OrderState
{
    public override string Name => "Paid";

    public override OrderState Pay(Order order) =>
        throw new InvalidOperationException("Already paid");

    public override OrderState Ship(Order order)
    {
        order.ShippedAt = DateTime.UtcNow;
        return new ShippedState();
    }

    public override OrderState Cancel(Order order)
    {
        order.RefundedAt = DateTime.UtcNow;
        return new CancelledState();
    }
}

public class Order
{
    public OrderState State { get; private set; } = new PendingState();
    public DateTime? PaidAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? RefundedAt { get; set; }

    public void Pay() => State = State.Pay(this);
    public void Ship() => State = State.Ship(this);
    public void Cancel() => State = State.Cancel(this);
}
// Production: use Stateless library for state machines
```

### Section 10.5 — Enterprise Patterns

**Q169. CQRS implementation with separate read and write models.**

```csharp
// WRITE side - normalized domain model
public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderLine> Lines { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

// Command
public record PlaceOrderCommand(Guid CustomerId, List<OrderItem> Items)
    : IRequest<Guid>;

public class PlaceOrderHandler : IRequestHandler<PlaceOrderCommand, Guid>
{
    private readonly AppDbContext _writeDb;
    private readonly IMessageBus _bus;

    public async Task<Guid> Handle(PlaceOrderCommand cmd, CancellationToken ct)
    {
        var order = new Order { /* ... */ };
        _writeDb.Orders.Add(order);
        await _writeDb.SaveChangesAsync(ct);
        await _bus.PublishAsync(new OrderPlacedEvent(order));
        return order.Id;
    }
}

// READ side - denormalized for queries
public class OrderListView
{
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; }  // denormalized
    public string CustomerEmail { get; set; } // denormalized
    public decimal Total { get; set; }
    public string Status { get; set; }
    public int LineCount { get; set; }        // pre-computed
}

// Read store: same SQL different views, or Cosmos, or Redis
public class OrderQueryService
{
    public async Task<List<OrderListView>> SearchAsync(string customerName) =>
        await _readDb.OrderListViews
            .Where(v => v.CustomerName.Contains(customerName))
            .ToListAsync(); // fast, no joins
}

// Projection keeps read model in sync
public class OrderPlacedProjection : IEventHandler<OrderPlacedEvent>
{
    public async Task HandleAsync(OrderPlacedEvent evt, CancellationToken ct)
    {
        var view = new OrderListView { /* map from event */ };
        _readDb.OrderListViews.Add(view);
        await _readDb.SaveChangesAsync(ct);
    }
}
```

**Q170. Outbox pattern in full.**

```csharp
// Outbox table (in same DB as business data)
public class OutboxMessage
{
    public Guid Id { get; set; }
    public string MessageType { get; set; }
    public string Payload { get; set; } // JSON
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int AttemptCount { get; set; }
}

// Service writes BOTH order AND outbox in ONE transaction
public async Task<Guid> PlaceOrderAsync(OrderRequest request)
{
    await using var tx = await _db.Database.BeginTransactionAsync();
    var order = new Order { /* ... */ };
    _db.Orders.Add(order);

    var outboxMsg = new OutboxMessage
    {
        Id = Guid.NewGuid(),
        MessageType = nameof(OrderPlacedEvent),
        Payload = JsonSerializer.Serialize(new OrderPlacedEvent(order.Id)),
        CreatedAt = DateTime.UtcNow
    };
    _db.OutboxMessages.Add(outboxMsg);

    await _db.SaveChangesAsync();
    await tx.CommitAsync();
    return order.Id;
}

// Background service polls and publishes
public class OutboxPublisher : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ServiceBusClient _busClient;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var sender = _busClient.CreateSender("orders");
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var pending = await db.OutboxMessages
                .Where(m => m.PublishedAt == null && m.AttemptCount < 5)
                .OrderBy(m => m.CreatedAt)
                .Take(50)
                .ToListAsync(stoppingToken);

            foreach (var msg in pending)
            {
                try
                {
                    var sbMsg = new ServiceBusMessage(msg.Payload)
                    {
                        MessageId = msg.Id.ToString(),
                        ContentType = "application/json",
                        Subject = msg.MessageType
                    };
                    await sender.SendMessageAsync(sbMsg, stoppingToken);
                    msg.PublishedAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    msg.AttemptCount++;
                    _logger.LogError(ex, "Failed to publish {Id}", msg.Id);
                }
            }

            await db.SaveChangesAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}
```

**Q171. Saga orchestrator with compensating transactions.**

```csharp
public class OrderSagaOrchestrator
{
    private readonly IPaymentService _payment;
    private readonly IInventoryService _inventory;
    private readonly IShippingService _shipping;
    private readonly ILogger<OrderSagaOrchestrator> _logger;

    public async Task<bool> ExecuteAsync(OrderSagaContext ctx)
    {
        Guid? paymentId = null;
        Guid? reservationId = null;
        try
        {
            // Step 1: Charge payment
            paymentId = await _payment.ChargeAsync(ctx.OrderId, ctx.Total);
            _logger.LogInformation("Charged payment {Id}", paymentId);

            // Step 2: Reserve inventory
            reservationId = await _inventory.ReserveAsync(ctx.OrderId, ctx.Items);
            _logger.LogInformation("Reserved inventory {Id}", reservationId);

            // Step 3: Schedule shipping
            await _shipping.ScheduleAsync(ctx.OrderId, ctx.Address);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Saga failed, running compensations");
            // Compensate in REVERSE order
            if (reservationId.HasValue)
                await SafelyAsync(() => _inventory.ReleaseAsync(reservationId.Value));
            if (paymentId.HasValue)
                await SafelyAsync(() => _payment.RefundAsync(paymentId.Value));
            return false;
        }
    }

    // Compensations must always succeed eventually
    private async Task SafelyAsync(Func<Task> action)
    {
        for (int attempt = 0; attempt < 3; attempt++)
        {
            try { await action(); return; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Compensation attempt {N} failed", attempt + 1);
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
            }
        }
        // After all retries: dead-letter for manual intervention
    }
}
// Production: Durable Functions, MassTransit Saga, NServiceBus
```

**Q172. Specification pattern for reusable query logic.**

```csharp
public abstract class Specification<T>
{
    public abstract Expression<Func<T, bool>> ToExpression();
    public bool IsSatisfiedBy(T entity) => ToExpression().Compile()(entity);

    public Specification<T> And(Specification<T> other) =>
        new AndSpecification<T>(this, other);
}

public class ActiveOrdersSpec : Specification<Order>
{
    public override Expression<Func<Order, bool>> ToExpression() =>
        o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Paid;
}

public class HighValueOrdersSpec : Specification<Order>
{
    private readonly decimal _threshold;
    public HighValueOrdersSpec(decimal threshold) => _threshold = threshold;

    public override Expression<Func<Order, bool>> ToExpression() =>
        o => o.Total > _threshold;
}

// Usage with EF Core (SQL pushdown!)
public async Task<List<Order>> GetActiveHighValueAsync()
{
    var spec = new ActiveOrdersSpec().And(new HighValueOrdersSpec(1000));
    return await _db.Orders.Where(spec.ToExpression()).ToListAsync();
}
```

### Section 10.6 — SOLID in Real Code

**Q173. SRP violation and fix.**

```csharp
// VIOLATION: 3 reasons to change in one class
public class OrderService
{
    public void PlaceOrder(Order order) { /* ... */ }
    public void SendEmail(string to, string body) { /* SMTP */ }
    public void GeneratePdfInvoice(Order order) { /* PDF */ }
    public void LogToFile(string message) { /* file IO */ }
}

// FIX: split by responsibility
public class OrderService
{
    private readonly IEmailSender _email;
    private readonly IInvoiceGenerator _invoice;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IEmailSender email, IInvoiceGenerator invoice,
        ILogger<OrderService> logger)
    {
        _email = email; _invoice = invoice; _logger = logger;
    }

    public async Task PlaceOrderAsync(Order order)
    {
        _logger.LogInformation("Placing order {Id}", order.Id);
        await _invoice.GenerateAsync(order);
        await _email.SendAsync(order.CustomerEmail, "Order placed", "...");
    }
}
```

**Q174. OCP — open for extension, closed for modification.**

```csharp
// VIOLATION: every new shape requires editing this
public class AreaCalculator
{
    public double Calculate(object shape)
    {
        if (shape is Circle c) return Math.PI * c.Radius * c.Radius;
        if (shape is Square s) return s.Side * s.Side;
        // Adding a new shape? Must modify this method.
        throw new ArgumentException();
    }
}

// FIX: polymorphism. New shapes plug in.
public abstract class Shape
{
    public abstract double Area { get; }
}

public class Circle : Shape
{
    public double Radius { get; init; }
    public override double Area => Math.PI * Radius * Radius;
}

// AreaCalculator is closed for modification, open for extension
public class AreaCalculator
{
    public double Total(IEnumerable<Shape> shapes) => shapes.Sum(s => s.Area);
}
```

**Q175. LSP violation.**

```csharp
// VIOLATION: Square breaks Rectangle's contract
public class Rectangle
{
    public virtual int Width { get; set; }
    public virtual int Height { get; set; }
    public int Area => Width * Height;
}

public class Square : Rectangle
{
    public override int Width { set { base.Width = base.Height = value; } }
    public override int Height { set { base.Width = base.Height = value; } }
}

// Code expecting Rectangle behavior breaks:
// var r = (Rectangle)new Square();
// r.Width = 5; r.Height = 10;
// Console.WriteLine(r.Area); // expects 50, gets 100

// FIX: don't model Square as Rectangle
public abstract class Shape { public abstract int Area { get; } }

public class Rectangle : Shape
{
    public int Width { get; init; }
    public int Height { get; init; }
    public override int Area => Width * Height;
}

public class Square : Shape
{
    public int Side { get; init; }
    public override int Area => Side * Side;
}
```

**Q176. ISP violation.**

```csharp
// VIOLATION: god interface
public interface IUserService
{
    User Authenticate(string username, string password);
    User GetById(int id);
    void UpdateProfile(int id, ProfileData data);
    void DeleteUser(int id);
    void GrantAdminRole(int id);
    List<User> SearchUsers(string query);
    AuditLog GetAuditLog(int id);
    // 30+ methods total
}

// Login service has to mock every method even though it only uses one

// FIX: split by client need
public interface IUserAuthenticator
{
    User Authenticate(string username, string password);
}

public interface IUserProfile
{
    User GetById(int id);
    void UpdateProfile(int id, ProfileData data);
}

public interface IUserAdmin
{
    void DeleteUser(int id);
    void GrantAdminRole(int id);
}

// One impl can implement multiple — ISP is about CLIENT view
public class UserService : IUserAuthenticator, IUserProfile, IUserAdmin
```

**Q177. DIP — depend on abstractions.**

```csharp
// VIOLATION: depends on concrete implementations
public class OrderService
{
    private readonly EfOrderRepository _repo = new();
    private readonly SmtpEmailSender _email = new();
}
// Untestable. Locked into EF and SMTP.

// FIX: depend on abstractions, inject implementations
public class OrderService
{
    private readonly IOrderRepository _repo;
    private readonly IEmailSender _email;

    public OrderService(IOrderRepository repo, IEmailSender email)
    {
        _repo = repo;
        _email = email;
    }

    public async Task PlaceOrderAsync(Order o)
    {
        await _repo.SaveAsync(o);
        await _email.SendAsync(o.Email, "Order placed");
    }
}

// Test:
var mockRepo = new Mock<IOrderRepository>();
var mockEmail = new Mock<IEmailSender>();
var service = new OrderService(mockRepo.Object, mockEmail.Object);

// Production:
services.AddScoped<IOrderRepository, EfOrderRepository>();
services.AddScoped<IEmailSender, SmtpEmailSender>();
// Switch to SendGrid? services.AddScoped<IEmailSender, SendGridSender>();
// OrderService never changes.
```

## PART 11 — MICROSERVICES ON AZURE (DEEP CONFIG)

This is the part Keith is most likely to test deeply. Service Bus configuration, Azure Functions production setup, AKS deployment, APIM policies, distributed tracing. Real code, real configs, no hand-waving.

### Section 11.1 — Azure Service Bus Deep Configuration

**Q178. Walk me through Service Bus queues vs topics in production.**

**A.** Two messaging patterns:

**Queue:** Point-to-point. ONE message, ONE consumer. Multiple consumers competing for messages = horizontal scaling. Use for: work distribution, command processing.

**Topic + Subscriptions:** Pub-sub. ONE message, MANY subscribers (each gets own copy). Use for: events that multiple services react to.

```text
// Queue: workers compete
Producer -> Queue [order-processing] -> Worker1 OR Worker2 OR Worker3

// Topic: each subscription gets a copy
Producer -> Topic [order-events]
            |-> Subscription [email-service] -> EmailWorker
            |-> Subscription [analytics] -> AnalyticsWorker
            |-> Subscription [audit] -> AuditWorker
```

Architect rule: prefer topics for events (decouples future subscribers), queues for commands (one specific consumer).

**Q179. Show me a complete Service Bus consumer with sessions.**

**A.** Production consumer with FIFO-per-customer ordering, peek-lock, error handling, DLQ:

```csharp
public class OrderProcessor : BackgroundService
{
    private readonly ServiceBusClient _client;
    private readonly ILogger<OrderProcessor> _logger;
    private readonly IServiceProvider _services;
    private ServiceBusSessionProcessor _processor;

    public OrderProcessor(
        ServiceBusClient client,
        ILogger<OrderProcessor> logger,
        IServiceProvider services)
    {
        _client = client;
        _logger = logger;
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = new ServiceBusSessionProcessorOptions
        {
            AutoCompleteMessages = false,    // We control completion
            MaxConcurrentSessions = 10,      // 10 customers in parallel
            MaxConcurrentCallsPerSession = 1, // FIFO per customer
            PrefetchCount = 5,               // pre-fetch for throughput
            ReceiveMode = ServiceBusReceiveMode.PeekLock,
            SessionIdleTimeout = TimeSpan.FromMinutes(1)
        };

        _processor = _client.CreateSessionProcessor("orders", options);
        _processor.ProcessMessageAsync += HandleMessageAsync;
        _processor.ProcessErrorAsync += HandleErrorAsync;
        await _processor.StartProcessingAsync(stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleMessageAsync(ProcessSessionMessageEventArgs args)
    {
        try
        {
            var json = args.Message.Body.ToString();
            var orderEvent = JsonSerializer.Deserialize<OrderEvent>(json);

            using var scope = _services.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IOrderEventHandler>();

            // Idempotency check
            var msgId = args.Message.MessageId;
            if (await handler.AlreadyProcessedAsync(msgId))
            {
                await args.CompleteMessageAsync(args.Message);
                return;
            }

            await handler.HandleAsync(orderEvent);
            await args.CompleteMessageAsync(args.Message);
        }
        catch (PoisonMessageException ex)
        {
            // Bad message, send to DLQ immediately
            await args.DeadLetterMessageAsync(args.Message,
                deadLetterReason: "PoisonMessage",
                deadLetterErrorDescription: ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process {MessageId}, retrying",
                args.Message.MessageId);
            // Abandon = back to queue, retry counter increments
            // After max retries (configured at queue level), goes to DLQ
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private Task HandleErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception,
            "Service Bus error in {Source}", args.ErrorSource);
        return Task.CompletedTask;
    }
}

// Registration
services.AddSingleton(sp => new ServiceBusClient(
    "namespace.servicebus.windows.net",
    new DefaultAzureCredential()));
services.AddHostedService<OrderProcessor>();
```

> **Tip:** MaxConcurrentSessions controls parallelism across DIFFERENT sessions (customers). MaxConcurrentCallsPerSession controls within ONE session — usually keep at 1 for FIFO.

**Q180. What's PeekLock vs ReceiveAndDelete?**

**A.** Two receive modes with very different guarantees:

| Mode | Behavior |
| ------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| PeekLock (default) | Receive message, lock it for visibility timeout (default 30s). Consumer must Complete to remove or Abandon to release. If consumer crashes, lock expires, message returns. AT-LEAST-ONCE delivery. |
| ReceiveAndDelete | Atomically receive AND delete. Faster. If consumer crashes after receipt, message is LOST. AT-MOST-ONCE delivery. |

Always use PeekLock in production. ReceiveAndDelete only for telemetry where loss is acceptable.

**Q181. Configure max delivery count and DLQ properly.**

```text
// Bicep for queue with proper retry / DLQ config
resource ordersQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: serviceBusNamespace
  name: 'orders'
  properties: {
    maxDeliveryCount: 10,              // After 10 retries -> DLQ
    lockDuration: 'PT1M',              // 1 minute lock per message
    defaultMessageTimeToLive: 'P14D',  // 14 days
    deadLetteringOnMessageExpiration: true,
    duplicateDetectionHistoryTimeWindow: 'PT10M', // 10 min dedup window
    requiresDuplicateDetection: true,
    requiresSession: true,             // for FIFO per session
    enablePartitioning: false,         // Premium handles partitioning
    enableBatchedOperations: true
  }
}

// Alert on DLQ depth
resource dlqAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: 'orders-dlq-depth-alert'
  properties: {
    severity: 2,
    enabled: true,
    scopes: [serviceBusNamespace.id]
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [{
        name: 'DeadletteredMessages > 10'
        metricName: 'DeadletteredMessages'
        operator: 'GreaterThan'
        threshold: 10
        timeAggregation: 'Maximum'
      }]
    }
  }
}
```

**Q182. How do you reprocess messages from DLQ?**

**A.** After fixing the root cause, drain DLQ back to main queue:

```csharp
public class DlqDrainer
{
    private readonly ServiceBusClient _client;

    public async Task DrainAsync(string queueName, CancellationToken ct)
    {
        // Receive from DLQ
        var dlqReceiver = _client.CreateReceiver(queueName, new ServiceBusReceiverOptions
        {
            SubQueue = SubQueue.DeadLetter // <-- DLQ access
        });

        // Send back to main queue
        var sender = _client.CreateSender(queueName);

        while (!ct.IsCancellationRequested)
        {
            var batch = await dlqReceiver.ReceiveMessagesAsync(
                maxMessages: 50,
                maxWaitTime: TimeSpan.FromSeconds(5),
                cancellationToken: ct);

            if (batch.Count == 0) break;

            foreach (var dlqMsg in batch)
            {
                // Clone to a new message
                var newMsg = new ServiceBusMessage(dlqMsg.Body)
                {
                    MessageId = Guid.NewGuid().ToString(),
                    SessionId = dlqMsg.SessionId,
                    ContentType = dlqMsg.ContentType
                };
                foreach (var kv in dlqMsg.ApplicationProperties)
                    newMsg.ApplicationProperties[kv.Key] = kv.Value;

                await sender.SendMessageAsync(newMsg, ct);
                await dlqReceiver.CompleteMessageAsync(dlqMsg, ct);
            }
        }
    }
}
```

Important: don't blindly drain. Always investigate WHY messages went to DLQ first. Reprocessing without fixing the root cause floods DLQ again.

**Q183. Service Bus Standard vs Premium — which to pick?**

**A.** Production microservices: Premium. Here's why:

| Feature | Standard vs Premium |
| ------------------------------------- | ------------------------------------------------------------------------------------------------------------ |
| Cost | Standard: pay-per-message (~$10/M ops). Premium: dedicated capacity (~$675/mo minimum per messaging unit). |
| Predictable latency | Standard: shared multi-tenant, latency varies. Premium: dedicated, predictable. |
| VNet integration / Private Endpoints | Standard: limited. Premium: full support. |
| Geo-disaster recovery | Premium only. |
| Encryption with customer-managed keys | Premium only. |
| Throughput | Standard: ~1000s msg/sec. Premium: scales with messaging units (1-16). |
| Max message size | Standard: 256KB. Premium: 100MB. |

> ***Say it like this:** "For STERIS-style enterprise with HIPAA, Premium is non-negotiable. Standard is fine for dev/test or low-volume internal tools."*

**Q184. What's duplicate detection in Service Bus?**

**A.** Optional feature. Service Bus tracks MessageId of recent messages and rejects duplicates within a configurable window (10 min to 7 days).

```csharp
// Configure on queue
RequiresDuplicateDetection = true,
DuplicateDetectionHistoryTimeWindow = TimeSpan.FromMinutes(30)

// Producer sets MessageId
var msg = new ServiceBusMessage(payload)
{
    MessageId = orderId.ToString(), // critical — broker dedupes on this
    ContentType = "application/json"
};
await sender.SendMessageAsync(msg);
```

Use case: producer retries due to network blip. Without dedup, same message processed twice. With dedup, second send silently dropped at broker.

Trade-off: every message stores its MessageId in memory. Higher resource cost. Don't enable unless needed.

**Q185. How do you tune Service Bus throughput?**

**A.** Levers from cheapest to most expensive:

- Increase MaxConcurrentCalls (default 1, can go to ~100).
- Increase PrefetchCount (default 0, set to 2-5x MaxConcurrentCalls).
- Use batched operations: SendMessagesAsync(batch) and ReceiveMessagesAsync(batch).
- Scale out consumers: more pods/instances of the consumer.
- Premium tier with more messaging units.
- Partition the queue (Premium auto-partitions).

```csharp
// Tune for high throughput
var options = new ServiceBusProcessorOptions
{
    MaxConcurrentCalls = 32,  // 32 concurrent handlers
    PrefetchCount = 100,      // pre-fetch 100 messages
    AutoCompleteMessages = false,
    MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5)
};
```

### Section 11.2 — Azure Functions Production Configuration

**Q186. Walk me through hosting plans for Functions.**

**A.** Five hosting plans, each with different trade-offs:

| Plan | Use case |
| ---------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------ |
| Consumption | Pay-per-execution. Cold starts (1-10s for .NET). 10 min max timeout. Cheapest. Best for: bursty, infrequent workloads. |
| Premium | Pre-warmed instances, no cold start. VNet integration. Unlimited timeout. ~$165/mo minimum. Best for: production APIs, latency-sensitive. |
| Flex Consumption | Newer (2024). Per-instance scale, faster cold start (300ms-1s), VNet support. Default for new serverless workloads. |
| Dedicated (App Service Plan) | Run on existing App Service. Best when you have App Service capacity. Always-on. |
| Container Apps | Functions in containers. Newer option. Run alongside other containers. |

**Q187. Show me a Service Bus-triggered function with proper config.**

```csharp
// .NET 8 isolated worker model (recommended in 2026)
public class OrderProcessor
{
    private readonly ILogger<OrderProcessor> _logger;
    private readonly IOrderHandler _handler;

    public OrderProcessor(ILogger<OrderProcessor> logger, IOrderHandler handler)
    {
        _logger = logger;
        _handler = handler;
    }

    [Function("ProcessOrder")]
    public async Task RunAsync(
        [ServiceBusTrigger(
            queueName: "orders",
            Connection = "ServiceBus",
            IsSessionsEnabled = true,        // FIFO per session
            AutoCompleteMessages = false)]   // We control completion
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        FunctionContext context)
    {
        try
        {
            var json = message.Body.ToString();
            var order = JsonSerializer.Deserialize<OrderMessage>(json);

            // Idempotency
            if (await _handler.AlreadyProcessedAsync(message.MessageId))
            {
                await messageActions.CompleteMessageAsync(message);
                return;
            }

            await _handler.HandleAsync(order);
            await messageActions.CompleteMessageAsync(message);
        }
        catch (PoisonMessageException ex)
        {
            await messageActions.DeadLetterMessageAsync(
                message,
                deadLetterReason: "PoisonMessage",
                deadLetterErrorDescription: ex.Message);
        }
        catch (Exception)
        {
            await messageActions.AbandonMessageAsync(message);
            throw; // re-throw so Functions runtime logs failure
        }
    }
}

// Program.cs
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IOrderHandler, OrderHandler>();
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();
```

**Q188. Show me host.json for production tuning.**

```text
{
  "version": "2.0",
  "logging": {
    "logLevel": {
      "default": "Information",
      "Function": "Information",
      "Host.Aggregator": "Trace"
    },
    "applicationInsights": {
      "samplingSettings": {
        "isEnabled": true,
        "maxTelemetryItemsPerSecond": 20,
        "excludedTypes": "Request"
      }
    }
  },
  "extensions": {
    "serviceBus": {
      "prefetchCount": 100,
      "messageHandlerOptions": {
        "autoComplete": false,
        "maxConcurrentCalls": 32,
        "maxAutoRenewDuration": "00:05:00"
      },
      "sessionHandlerOptions": {
        "maxConcurrentSessions": 16
      }
    },
    "http": {
      "maxConcurrentRequests": 200,
      "maxOutstandingRequests": 1000
    }
  },
  "functionTimeout": "00:10:00",
  "healthMonitor": {
    "enabled": true,
    "healthCheckInterval": "00:00:10",
    "healthCheckWindow": "00:02:00",
    "healthCheckThreshold": 6
  }
}
```

**Q189. What's a Durable Function and when do I use it?**

**A.** Stateful workflow built on Functions. State persists automatically; orchestrator survives crashes/restarts. Code reads sequentially though it's distributed.

Use Durable Functions for:

- Saga orchestration.
- Long-running processes (minutes to days).
- Workflows requiring human approval (Wait pattern).
- Complex retries and timeouts that vanilla Functions can't easily express.

**Q190. Show me a complete Durable Function order saga.**

```csharp
// Orchestrator — defines the workflow
[Function("OrderSagaOrchestrator")]
public async Task<OrderSagaResult> RunOrchestrator(
    [OrchestrationTrigger] TaskOrchestrationContext context)
{
    var input = context.GetInput<OrderRequest>();
    var log = context.CreateReplaySafeLogger<OrderProcessor>();
    Guid? paymentId = null;
    Guid? reservationId = null;

    try
    {
        // Step 1: Charge payment with retry
        paymentId = await context.CallActivityAsync<Guid>(
            "ChargePayment",
            input,
            new TaskOptions(new TaskRetryOptions(
                new RetryPolicy(maxNumberOfAttempts: 3,
                    firstRetryInterval: TimeSpan.FromSeconds(5),
                    backoffCoefficient: 2.0))));
        log.LogInformation("Charged payment {Id}", paymentId);

        // Step 2: Reserve inventory
        reservationId = await context.CallActivityAsync<Guid>(
            "ReserveInventory", input);
        log.LogInformation("Reserved inventory {Id}", reservationId);

        // Step 3: Schedule shipping (with timeout)
        var deadline = context.CurrentUtcDateTime.AddMinutes(10);
        using var cts = new CancellationTokenSource();
        var shippingTask = context.CallActivityAsync<Guid>(
            "ScheduleShipping", input);
        var timeoutTask = context.CreateTimer(deadline, cts.Token);

        var winner = await Task.WhenAny(shippingTask, timeoutTask);
        if (winner == timeoutTask)
            throw new TimeoutException("Shipping scheduling timed out");

        cts.Cancel(); // cancel timer
        var shippingId = await shippingTask;
        log.LogInformation("Scheduled shipping {Id}", shippingId);

        return new OrderSagaResult { Success = true };
    }
    catch (Exception ex)
    {
        log.LogError(ex, "Saga failed, compensating");
        // Compensate in reverse order
        if (reservationId.HasValue)
            await context.CallActivityAsync(
                "ReleaseInventory", reservationId.Value);
        if (paymentId.HasValue)
            await context.CallActivityAsync(
                "RefundPayment", paymentId.Value);
        return new OrderSagaResult { Success = false, Error = ex.Message };
    }
}

// Activities — each is a normal Function
[Function("ChargePayment")]
public async Task<Guid> ChargePayment(
    [ActivityTrigger] OrderRequest order,
    FunctionContext ctx)
{
    return await _paymentService.ChargeAsync(order.Total);
}

// Trigger — kicks off the orchestration
[Function("PlaceOrder")]
public async Task<HttpResponseData> PlaceOrder(
    [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
    [DurableClient] DurableTaskClient client,
    FunctionContext ctx)
{
    var order = await req.ReadFromJsonAsync<OrderRequest>();
    string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
        "OrderSagaOrchestrator",
        order);
    return await client.CreateCheckStatusResponseAsync(req, instanceId);
}
```

**Q191. What are Durable Function patterns?**

**A.** Five canonical patterns:

**Function chaining:** Step 1 → Step 2 → Step 3, awaiting each.

**Fan-out / fan-in:** Run N parallel activities, then aggregate results.

**Async HTTP API:** Long-running work with status polling endpoint.

**Monitoring:** Periodic check loop with exit condition.

**Human interaction:** Wait for external event (approval) with timeout.

```csharp
// Fan-out / fan-in example
[Function("ProcessFiles")]
public async Task<int> ProcessFiles(
    [OrchestrationTrigger] TaskOrchestrationContext context)
{
    var files = await context.CallActivityAsync<List<string>>("ListFiles", null);

    // Parallel execution
    var tasks = files.Select(f =>
        context.CallActivityAsync<int>("ProcessFile", f)).ToList();
    var results = await Task.WhenAll(tasks);
    return results.Sum();
}

// Human approval pattern
[Function("ApprovalWorkflow")]
public async Task<bool> ApprovalWorkflow(
    [OrchestrationTrigger] TaskOrchestrationContext context)
{
    await context.CallActivityAsync("SendApprovalEmail", null);

    using var cts = new CancellationTokenSource();
    var approvalEvent = context.WaitForExternalEvent<bool>("ApprovalReceived");
    var timeoutEvent = context.CreateTimer(
        context.CurrentUtcDateTime.AddDays(3), cts.Token);

    var winner = await Task.WhenAny(approvalEvent, timeoutEvent);
    if (winner == timeoutEvent) return false;
    cts.Cancel();
    return await approvalEvent;
}
```

**Q192. How do you mitigate Functions cold start?**

**A.** Multiple strategies:

- Premium plan or Flex Consumption: pre-warmed instances, no cold start.
- Always-On in Premium: keeps host warm. ~$0 extra cost on Premium.
- Smaller deployments: fewer dependencies = faster startup.
- Avoid heavy startup: lazy-load expensive resources.
- AOT compilation (.NET 8+): native AOT eliminates JIT overhead.
- If latency-critical: use App Service or Container Apps instead. Functions adds cold-start risk.

### Section 11.3 — AKS Production Configuration

**Q193. Walk me through a production AKS cluster checklist.**

**A.** Things you should verify on any production AKS cluster:

- Multiple node pools (system / user / spot).
- Azure CNI networking (not kubenet) for VNet integration.
- Workload Identity enabled (replaces deprecated AAD Pod Identity).
- Private cluster (no public API endpoint) for production.
- Azure Container Registry with image scanning.
- Network policies (Calico or Azure Network Policy Manager).
- Pod-level resource requests and limits on every workload.
- Liveness, readiness, and startup probes.
- PodDisruptionBudgets for availability during node upgrades.
- Container Insights + Diagnostic settings → Log Analytics.
- Azure Policy add-on enforcing org rules.
- Cluster autoscaler enabled.
- Multi-AZ node pools.

**Q194. Show me a production K8s deployment YAML.**

```text
apiVersion: apps/v1
kind: Deployment
metadata:
  name: order-service
  namespace: orders
  labels:
    app: order-service
    version: v2.1.0
spec:
  replicas: 3
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1
      maxUnavailable: 0
  selector:
    matchLabels:
      app: order-service
  template:
    metadata:
      labels:
        app: order-service
        azure.workload.identity/use: "true" # Workload Identity
    spec:
      serviceAccountName: order-service-sa
      containers:
      - name: app
        image: myregistry.azurecr.io/order-service:v2.1.0
        ports:
        - containerPort: 8080
          name: http
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: Production
        - name: AzureAd__TenantId
          valueFrom:
            secretKeyRef:
              name: azure-ad
              key: tenantId
        resources:
          requests:
            cpu: 200m
            memory: 256Mi
          limits:
            cpu: 1000m
            memory: 512Mi
        livenessProbe:
          httpGet:
            path: /health/live
            port: http
          initialDelaySeconds: 30
          periodSeconds: 10
          failureThreshold: 3
        readinessProbe:
          httpGet:
            path: /health/ready
            port: http
          initialDelaySeconds: 10
          periodSeconds: 5
          failureThreshold: 3
        startupProbe:
          httpGet:
            path: /health/live
            port: http
          initialDelaySeconds: 5
          periodSeconds: 5
          failureThreshold: 30 # 150s for slow startup
      affinity:
        podAntiAffinity:
          preferredDuringSchedulingIgnoredDuringExecution:
          - weight: 100
            podAffinityTerm:
              labelSelector:
                matchLabels:
                  app: order-service
              topologyKey: kubernetes.io/hostname
---
apiVersion: policy/v1
kind: PodDisruptionBudget
metadata:
  name: order-service-pdb
  namespace: orders
spec:
  minAvailable: 2
  selector:
    matchLabels:
      app: order-service
---
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: order-service-hpa
  namespace: orders
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: order-service
  minReplicas: 3
  maxReplicas: 20
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
---
apiVersion: v1
kind: Service
metadata:
  name: order-service
  namespace: orders
spec:
  selector:
    app: order-service
  ports:
  - port: 80
    targetPort: http
```

**Q195. What's KEDA and when do you use it over HPA?**

**A.** HPA scales by CPU/memory. KEDA scales by external metrics — queue depth, Service Bus messages, custom metrics.

```text
# KEDA ScaledObject — scale on Service Bus queue depth
apiVersion: keda.sh/v1alpha1
kind: ScaledObject
metadata:
  name: order-processor-scaler
  namespace: orders
spec:
  scaleTargetRef:
    name: order-processor
  pollingInterval: 30
  cooldownPeriod: 300
  minReplicaCount: 1
  maxReplicaCount: 30
  triggers:
  - type: azure-servicebus
    metadata:
      queueName: orders
      messageCount: "10" # 1 pod per 10 messages
      namespace: my-namespace
    authenticationRef:
      name: workload-identity-auth
```

Use KEDA whenever your workload's load doesn't correlate with CPU. Service Bus consumers especially: many idle pods waste money; KEDA scales to zero when queue is empty.

**Q196. Workload Identity setup in AKS.**

**A.** Modern way to authenticate K8s pods to Azure resources without secrets:

```text
# 1. Enable on cluster (one-time)
az aks update -n my-cluster -g my-rg \
  --enable-oidc-issuer \
  --enable-workload-identity

# 2. Get OIDC issuer URL
OIDC_URL=$(az aks show -n my-cluster -g my-rg \
  --query "oidcIssuerProfile.issuerUrl" -o tsv)

# 3. Create User-Assigned Managed Identity
az identity create -n order-service-identity -g my-rg
CLIENT_ID=$(az identity show -n order-service-identity -g my-rg \
  --query clientId -o tsv)

# 4. Federate identity with K8s service account
az identity federated-credential create \
  -n order-service-fed \
  --identity-name order-service-identity \
  -g my-rg \
  --issuer $OIDC_URL \
  --subject system:serviceaccount:orders:order-service-sa

# 5. Grant identity permissions on Azure resources
az role assignment create \
  --assignee $CLIENT_ID \
  --role "Azure Service Bus Data Receiver" \
  --scope /subscriptions/.../servicebus/...

# 6. Create K8s service account with annotation
cat <<EOF | kubectl apply -f -
apiVersion: v1
kind: ServiceAccount
metadata:
  name: order-service-sa
  namespace: orders
  annotations:
    azure.workload.identity/client-id: $CLIENT_ID
EOF

# 7. Pod uses serviceAccountName + label
spec:
  serviceAccountName: order-service-sa
  labels:
    azure.workload.identity/use: "true"
```

Result: Pod gets a federated token; uses it to call Azure SQL, Service Bus, Key Vault. Zero secrets in the cluster.

### Section 11.4 — APIM Policies

**Q197. Show me APIM policy for JWT validation + rate limiting.**

```text
<policies>
  <inbound>
    <base />
    <!-- 1. Validate JWT from Entra ID -->
    <validate-jwt header-name="Authorization"
        failed-validation-httpcode="401"
        failed-validation-error-message="Unauthorized">
      <openid-config url="https://login.microsoftonline.com/{tenant}/v2.0/.well-known/openid-configuration" />
      <audiences>
        <audience>api://orders-api</audience>
      </audiences>
      <required-claims>
        <claim name="scp" match="any">
          <value>orders.read</value>
          <value>orders.write</value>
        </claim>
      </required-claims>
    </validate-jwt>
    <!-- 2. Rate limit per subscription key -->
    <rate-limit-by-key calls="100"
        renewal-period="60"
        counter-key="@(context.Subscription?.Key ?? "anonymous")" />
    <!-- 3. Quota per day -->
    <quota-by-key calls="10000"
        renewal-period="86400"
        counter-key="@(context.Subscription?.Key)" />
    <!-- 4. CORS for browser clients -->
    <cors allow-credentials="true">
      <allowed-origins>
        <origin>https://app.steris.com</origin>
      </allowed-origins>
      <allowed-methods>
        <method>GET</method>
        <method>POST</method>
      </allowed-methods>
      <allowed-headers>
        <header>Authorization</header>
        <header>Content-Type</header>
      </allowed-headers>
    </cors>
    <!-- 5. Set backend auth via Managed Identity -->
    <authentication-managed-identity resource="api://orders-backend" />
  </inbound>
  <backend>
    <forward-request timeout="30" />
  </backend>
  <outbound>
    <base />
    <!-- 6. Add response headers -->
    <set-header name="X-Powered-By" exists-action="delete" />
    <set-header name="X-Content-Type-Options" exists-action="override">
      <value>nosniff</value>
    </set-header>
  </outbound>
  <on-error>
    <base />
    <set-status code="@(context.Response?.StatusCode ?? 500)" reason="@(context.LastError.Reason)" />
  </on-error>
</policies>
```

**Q198. What APIM features do tech leads need to know?**

**A.** Top features that come up in interviews:

- Products: bundle APIs, attach policies, manage subscriptions.
- Subscriptions: API keys per consumer.
- Versioning: side-by-side v1 and v2 with separate policies.
- Rewrite-uri: change backend path without changing client URL.
- set-backend-service: route to different backend based on conditions.
- Caching: \<cache-lookup> and \<cache-store> for response caching.
- Mock responses: \<mock-response> for testing without backend.
- Developer portal: self-service API discovery + documentation.

### Section 11.5 — Distributed Tracing & Observability

**Q199. How do you set up distributed tracing across services?**

**A.** W3C Trace Context propagation. Built into .NET 8+ — trace IDs flow automatically across HTTP, Service Bus, SQL calls.

```csharp
// Program.cs — full observability setup
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(serviceName: "order-service"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation(opts =>
            opts.SetDbStatementForText = true)
        .AddSource("Azure.Messaging.ServiceBus")
        .AddAzureMonitorTraceExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddAzureMonitorMetricExporter());

// Custom activity in business code
using var activity = activitySource.StartActivity("PlaceOrder");
activity?.SetTag("order.id", order.Id);
activity?.SetTag("customer.id", order.CustomerId);
```

**Q200. Show me KQL queries for production debugging.**

```text
// Top 10 slowest endpoints last hour
requests
| where timestamp > ago(1h)
| summarize p95 = percentile(duration, 95), count() by name
| top 10 by p95 desc

// Error rate over time, 5-min buckets
requests
| where timestamp > ago(24h)
| summarize errors = countif(success == false), total = count()
    by bin(timestamp, 5m), name
| extend errorRate = todouble(errors) / total
| where errorRate > 0.01
| render timechart

// Slow dependencies
dependencies
| where timestamp > ago(1h) and duration > 1000
| summarize count() by target, name
| order by count_ desc

// Trace specific request across services
union requests, dependencies, exceptions, traces
| where operation_Id == "abc123"
| order by timestamp asc
| project timestamp, itemType, name, target, duration, message
```

**Q201. What alerts should I set up for production microservices?**

**A.** Alert on user impact, not internal metrics:

- Availability: % successful requests below SLO (99.9%) over 5 min.
- p95 latency above threshold (e.g., 500ms) for 10 min.
- Error rate above 1% for 5 min.
- DLQ depth growing — messages failing to process.
- Service Bus queue depth growing — consumer can't keep up.
- Pod restart rate above threshold.
- Memory/CPU saturation (>85%) for 10 min.
- Failed deployments / rollouts.

Use SLO burn rate alerts: page on fast burn (consuming budget at 14x normal); ticket on slow burn (3x). Focuses attention on real problems, not noise.

**Q202. How do you handle secrets across services?**

**A.** Layered approach using Key Vault + Managed Identity:

```csharp
// Program.cs
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddAzureAppConfiguration(opts =>
    {
        opts.Connect(new Uri("https://myconfig.azconfig.io"),
            new DefaultAzureCredential());
        opts.ConfigureKeyVault(kv =>
        {
            kv.SetCredential(new DefaultAzureCredential());
        });
        // Refresh on change
        opts.ConfigureRefresh(refresh =>
            refresh.Register("Sentinel", refreshAll: true)
                .SetCacheExpiration(TimeSpan.FromMinutes(5)));
    });

// In code — same as any config
var connStr = builder.Configuration["Database:ConnectionString"];
// App Configuration resolves Key Vault references automatically
```

## PART 12 — CONCURRENCY & THREADING DEEP

Concurrency is where senior engineers separate from staff. Tech leads must know how async/await actually works, when to use locks vs Interlocked, and how to handle high-concurrency scenarios without deadlocks or race conditions.

### Section 12.1 — async/await Deep

**Q203. How does async/await actually work under the hood?**

**A.** The compiler rewrites your async method into a state machine. Each 'await' becomes a state transition.

```csharp
// You write:
public async Task<int> GetAsync()
{
    var data = await _http.GetStringAsync(url);
    return data.Length;
}

// Compiler generates roughly:
// 1. State machine class with state field
// 2. Each await suspends the method, returns control to caller
// 3. When awaited task completes, state machine resumes
// 4. The thread isn't blocked while waiting
```

Key insight: 'await' doesn't create threads. It REUSES threads. While your async method is waiting on I/O, the thread can handle other work.

**Q204. What's a SynchronizationContext and why does it matter?**

**A.** Captures 'where to resume after await.' UI apps (WinForms/WPF) have one to ensure callbacks run on UI thread. Old ASP.NET (not Core) has request context.

Why it matters:

- If you call .Result on an async method that captured the context, and the context is single-threaded — DEADLOCK.
- ASP.NET Core has NO sync context — that's why .Result usually works there but is still bad practice.
- Library code should use ConfigureAwait(false) to skip context capture, avoiding deadlocks.

```csharp
// Library code — ALWAYS ConfigureAwait(false)
public async Task<string> FetchAsync(string url)
{
    var response = await _http.GetAsync(url).ConfigureAwait(false);
    return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
}

// App code (controllers, etc.) — usually fine without
public async Task<IActionResult> Get()
{
    var data = await _service.FetchAsync(); // ASP.NET Core has no context
    return Ok(data);
}
```

**Q205. Task vs Thread — what's the difference?**

**A.** Different abstractions:

| Aspect | Task vs Thread |
| ------------------ | ------------------------------------------------------------------------------------------------------ |
| Cost | Task: lightweight, thousands at once. Thread: ~1MB stack each. |
| What it is | Task: a future representing async work. Thread: an OS thread. |
| Threading | Task: scheduled on ThreadPool. Thread: dedicated OS thread. |
| When to use Task | Almost always. async/await, Task.Run, Parallel. |
| When to use Thread | Rare. Long-running work that can't be Task-based, or specific thread requirements (UI, COM apartment). |

**Q206. Task.Run vs Task.Factory.StartNew?**

**A.** Task.Run is the modern way. Task.Factory.StartNew has trickier defaults.

```csharp
// CPU-bound work — offload from request thread
var result = await Task.Run(() => HeavyComputation(input));

// DON'T use Task.Run for I/O — wastes threads
// BAD:
var data = await Task.Run(() => _db.Orders.ToListAsync());

// GOOD: I/O is already async
var data = await _db.Orders.ToListAsync();
```

> **Tip:** Common mistake: wrapping async I/O in Task.Run. Doesn't help — just allocates an extra thread. Use Task.Run only for CPU-bound work to keep request threads responsive.

**Q207. What's ValueTask and when do I use it?**

**A.** ValueTask avoids allocation when the async method completes synchronously (cache hit, fast path).

```csharp
// Task allocates always
public async Task<User> GetAsync(int id)
{
    if (_cache.TryGetValue(id, out User cached))
        return cached; // sync path, but still allocates Task
    return await _db.Users.FindAsync(id);
}

// ValueTask — no allocation on sync path
public async ValueTask<User> GetAsync(int id)
{
    if (_cache.TryGetValue(id, out User cached))
        return cached; // no allocation
    return await _db.Users.FindAsync(id);
}

// Use ValueTask when:
// - Method often completes synchronously
// - In hot paths where allocations matter
// - You won't await the same ValueTask multiple times
```

**Q208. What's an IAsyncEnumerable\<T>?**

**A.** Async streams. Process data incrementally without loading everything in memory.

```csharp
// Producer
public async IAsyncEnumerable<Order> StreamOrdersAsync(
    [EnumeratorCancellation] CancellationToken ct = default)
{
    await foreach (var batch in _db.Orders.AsAsyncEnumerable())
    {
        yield return batch;
    }
}

// Consumer
await foreach (var order in service.StreamOrdersAsync(ct))
{
    await ProcessAsync(order);
}

// Use cases:
// - Streaming large result sets without loading all in memory
// - SSE (Server-Sent Events) endpoints
// - Real-time event consumers
// - Pipelines processing data as it arrives
```

### Section 12.2 — Locking & Synchronization

**Q209. What are the .NET synchronization primitives?**

**A.** From cheapest to most expensive:

| Primitive | Use case |
| -------------------- | -------------------------------------------------------------------------------- |
| Interlocked | Atomic int/long operations. Cheapest. Increment, Add, Exchange, CompareExchange. |
| Volatile.Read/Write | Memory barriers without locks. Rarely needed. |
| lock (object) | Mutual exclusion within a process. Reentrant. Most common choice. |
| Monitor | Same as lock with extra features (TryEnter, Wait, Pulse). |
| SemaphoreSlim | Limit concurrency to N (e.g., 10 concurrent DB calls). Async-friendly. |
| Mutex | Cross-process locking. Slower. |
| ReaderWriterLockSlim | Many readers, one writer. Use when reads vastly outnumber writes. |

**Q210. When do I use Interlocked vs lock?**

**A.** Interlocked for single atomic operations on int/long. Lock for anything more complex.

```csharp
// Interlocked — single atomic op
private long _count = 0;
public void Increment() => Interlocked.Increment(ref _count);
public void Add(int n) => Interlocked.Add(ref _count, n);
public long Read() => Interlocked.Read(ref _count);

// CompareExchange — atomic compare-and-swap
long expected, newValue;
do
{
    expected = _count;
    newValue = expected * 2;
} while (Interlocked.CompareExchange(ref _count, newValue, expected) != expected);

// Use lock when you need multiple operations atomic together
private readonly object _gate = new();
private List<Order> _orders = new();
public void AddOrder(Order order)
{
    lock (_gate)
    {
        if (_orders.Count >= 100) _orders.RemoveAt(0);
        _orders.Add(order);
    } // Multiple ops protected together
}
```

**Q211. Show me SemaphoreSlim for limiting concurrency.**

```csharp
// Limit concurrent DB calls to 10
private readonly SemaphoreSlim _dbThrottle = new(10);

public async Task<Result> CallDbAsync()
{
    await _dbThrottle.WaitAsync();
    try
    {
        return await _db.SomeQueryAsync();
    }
    finally
    {
        _dbThrottle.Release(); // CRITICAL — always release
    }
}

// Use case: bulkhead pattern. Prevent one slow dependency
// from exhausting all threads.
```

**Q212. What's a deadlock and how do you avoid it?**

**A.** Two threads each hold a lock the other needs. Neither can proceed. Most common in code that takes multiple locks.

Avoidance rules:

- Take locks in the SAME ORDER everywhere. If always lock A then B, you can't deadlock.
- Don't call external code while holding a lock — you don't know what locks it takes.
- Keep critical sections short.
- Use lock-free data structures (ConcurrentDictionary) when possible.
- Use lock with timeout (Monitor.TryEnter) for non-critical paths.

### Section 12.3 — Concurrent Collections

**Q213. Walk me through the concurrent collections.**

**A.** Use these instead of locking ordinary collections:

| Collection | Use case |
| --------------------------- | ---------------------------------------------------------------------------------------------------------- |
| ConcurrentDictionary\<K,V> | Thread-safe dictionary. AddOrUpdate, GetOrAdd patterns. Most common. |
| ConcurrentQueue\<T> | FIFO queue. Lock-free. |
| ConcurrentStack\<T> | LIFO stack. |
| ConcurrentBag\<T> | Unordered. Fast for producer-consumer in same thread. |
| BlockingCollection\<T> | Bounded queue. Producer blocks when full, consumer blocks when empty. Replaced by Channels in modern .NET. |

**Q214. Show me ConcurrentDictionary patterns.**

```csharp
private readonly ConcurrentDictionary<string, int> _counters = new();

// Atomic increment (combines Get + Update)
public int Increment(string key) =>
    _counters.AddOrUpdate(key,
        addValue: 1,
        updateValueFactory: (_, current) => current + 1);

// Atomic get-or-create
var pool = _connectionPools.GetOrAdd(serverName,
    name => new ConnectionPool(name));

// Atomic remove (returns true if was present)
if (_counters.TryRemove(key, out var oldValue))
{
    Console.WriteLine($"Removed, was {oldValue}");
}
```

**Q215. What are Channels and when do I use them?**

**A.** Modern alternative to BlockingCollection. Producer-consumer queue with async support.

```csharp
// Bounded channel — producer waits when buffer full
var channel = Channel.CreateBounded<Order>(new BoundedChannelOptions(1000)
{
    FullMode = BoundedChannelFullMode.Wait,
    SingleReader = false,
    SingleWriter = false
});

// Producer
_ = Task.Run(async () =>
{
    foreach (var order in orderSource)
    {
        await channel.Writer.WriteAsync(order);
    }
    channel.Writer.Complete();
});

// Consumer
await foreach (var order in channel.Reader.ReadAllAsync(ct))
{
    await ProcessAsync(order);
}

// Use cases:
// - Producer-consumer pipelines
// - Background processing buffers
// - Backpressure (bounded channel slows producer when consumer is slow)
```

### Section 12.4 — Parallel Processing

**Q216. Parallel.ForEach vs Task.WhenAll — which when?**

**A.** Different tools for different work:

| Pattern | Use case |
| ------------------------------- | --------------------------------------------------------------- |
| Parallel.ForEach | CPU-bound work. Synchronous. Partitions across threads. |
| Parallel.ForEachAsync (.NET 6+) | Async work with parallelism limit. Modern choice for I/O bound. |
| Task.WhenAll | Async work. Run all in parallel, await all. Simple. |
| PLINQ | LINQ with parallel execution. .AsParallel(). Mostly historical. |

```csharp
// CPU-bound — Parallel.ForEach
Parallel.ForEach(items, item =>
{
    item.Hash = ComputeHash(item.Data);
});

// Async I/O with parallelism control — Parallel.ForEachAsync
await Parallel.ForEachAsync(urls,
    new ParallelOptions { MaxDegreeOfParallelism = 10 },
    async (url, ct) =>
    {
        var data = await _http.GetStringAsync(url, ct);
        await SaveAsync(data);
    });

// Async unbounded — Task.WhenAll (be careful!)
var tasks = items.Select(item => ProcessAsync(item));
var results = await Task.WhenAll(tasks);
// Risk: 1000 items = 1000 concurrent HTTP calls. Use semaphore to limit.
```

**Q217. How do I cancel async work?**

**A.** CancellationToken propagation. ASP.NET Core provides one per request that fires when client disconnects.

```csharp
// Always accept and propagate CancellationToken
public async Task<List<Order>> SearchAsync(
    string query,
    CancellationToken ct)
{
    return await _db.Orders
        .Where(o => o.Description.Contains(query))
        .ToListAsync(ct); // EF respects the token
}

// In controller — ASP.NET injects request's token
[HttpGet]
public async Task<IActionResult> Search(
    string q,
    CancellationToken ct)
{
    var results = await _service.SearchAsync(q, ct);
    return Ok(results);
}

// Linked tokens — combine multiple sources
using var cts = CancellationTokenSource.CreateLinkedTokenSource(
    requestCt, _shutdownToken);
cts.CancelAfter(TimeSpan.FromSeconds(30));
var data = await CallExternalAsync(cts.Token);
```

**Q218. Common async pitfalls to avoid.**

**A.** Top mistakes that cause production issues:

- .Result or .Wait() on async — deadlock or thread exhaustion.
- async void methods — exceptions can't be caught, crash the process.
- Fire-and-forget without try/catch — exceptions silently swallowed.
- Wrapping async I/O in Task.Run — wastes threads.
- Ignoring CancellationToken — work continues after client disconnects.
- Capturing DbContext in long-running tasks — DbContext isn't thread-safe.
- Unbounded Task.WhenAll — 10K concurrent operations exhausts resources.
- Re-awaiting same Task — works but wasteful. Cache the result.

## PART 13 — STERIS-SPECIFIC REAL-WORLD SCENARIOS

Healthcare-specific scenarios Keith may use as design questions. STERIS makes medical sterilization equipment, sells software to hospitals, must comply with HIPAA + FDA 21 CFR Part 11.

### Section 13.1 — Sterilization Telemetry Pipeline

**Q219. Design a system to ingest sterilization-cycle data from hospital devices, with real-time dashboards, alerts on anomalies, and 7-year retention.**

**A.** Walk through this in interview format. This is THE most STERIS-relevant possible question.

Step 1: Clarify

- Sources: medical devices in hospitals send cycle data (start, temp, pressure, end, outcome).
- Volume: 10K devices, 5 cycles/day, 100 events each = 5M events/day.
- Latency: dashboards near-real-time (<5 sec). Alerts <10 sec.
- Compliance: HIPAA, FDA 21 CFR Part 11 (audit trail integrity). 7-year retention.
- Multi-tenant: each hospital sees only its data.

Step 2: Estimate

- 5M events/day = ~60 events/sec average, ~300 peak.
- Each event ~1KB. 5GB/day × 365 = ~2TB/year × 7 = 14TB total.

Step 3: Architecture

```text
[Devices in hospitals]
  | (TLS, device cert auth)
  v
[IoT Hub / Event Hubs]
  |
  +-> [Stream Analytics] -> [Cosmos DB] (hot, 90 days)
  |                              |
  |                              v
  |        [App Service API] <- [Front Door + APIM] <- [Browser]
  |                              |
  |                              v
  |                         [SignalR for live updates]
  |
  +-> [Anomaly Detection] -> [Service Bus] -> [Alert Function] -> [SMS/Email]
  |
  +-> [Blob Storage with immutable policy] (cold, 7 years)
  |
  v
[Synapse for analytics]
```

Step 4: Key Decisions

- IoT Hub for ingestion: per-device cert auth, device twins for config.
- Cosmos for hot store: partition by hospitalId, multi-region writes.
- Immutable Blob for cold archive: 7-year retention, FDA-compliant.
- Stream Analytics for anomaly detection: built-in ML functions.
- All data services behind Private Endpoints, no public access.

Step 5: HIPAA Compliance

- BAA signed with Microsoft.
- All Azure services HIPAA-eligible.
- Encryption at rest (CMK in Key Vault) + in transit (TLS 1.2+).
- All services use Managed Identity — no connection strings.
- Audit log: every PHI access logged to Log Analytics with 7-year retention.
- RBAC: hospitals see only their data via row-level filters.

**Q220. How would you design the Service Bus topology for sterilization events?**

```text
Topic: sterilization-events
Subscriptions:
- dashboards: real-time UI updates (SignalR push)
- alerts: anomaly detection function
- audit: compliance logging to Log Analytics
- analytics: feed Synapse for reporting
- integration: send to hospital EHR via FHIR

Queue: alert-notifications
  Sessions enabled (FIFO per hospital)
  MaxDeliveryCount: 5
  DLQ alerted on depth > 10
  Duplicate detection: 30 min window

Queue: device-commands
  Sessions enabled (FIFO per device)
  Cloud-to-device commands (firmware updates, config changes)
  Critical: medical device commands MUST be ordered
```

**Q221. How do you handle FDA 21 CFR Part 11 audit trail requirements?**

**A.** Audit trails must be tamper-proof, attributable, contemporaneous. Concrete approach:

```csharp
// Every state-changing operation produces an audit entry
public class AuditEntry
{
    public Guid Id { get; init; }
    public DateTime Timestamp { get; init; }  // UTC
    public string UserId { get; init; }        // who
    public string Action { get; init; }        // what
    public string EntityType { get; init; }
    public string EntityId { get; init; }
    public string OldValue { get; init; }      // JSON
    public string NewValue { get; init; }      // JSON
    public string Hash { get; init; }          // SHA-256 of all fields
    public string PrevHash { get; init; }      // chain — tamper detection
    public string Signature { get; init; }     // electronic signature
}

// Storage: Immutable Blob with 7-year retention policy
// Cannot be modified or deleted — even by Owner
// Hash chain enables tamper detection
// Electronic signature ties entry to authenticated user
```

Combined with Azure Monitor + Log Analytics with long retention, you have the audit trail integrity FDA requires.

### Section 13.2 — Multi-Tenant Hospital SaaS

**Q222. Design a multi-tenant SaaS for surgical instrument tracking where each hospital is a tenant.**

**A.** Core question: tenant isolation strategy.

Tenant Isolation Options

| Strategy | Trade-off |
| ----------------------------------- | ------------------------------------------- |
| Shared everything (tenantId column) | Cheapest. Highest noisy-neighbor risk. |
| Shared DB, separate schemas | Better isolation. Harder schema migrations. |
| Separate DB per tenant | Strong isolation. Expensive at scale. |
| Separate everything (full stack) | Maximum isolation. Used for VIP. |

Recommended for STERIS

- Default: Cosmos DB with hospitalId as partition key. Each hospital has its own logical partition. Per-partition RU prevents noisy-neighbor.
- Premium hospitals: dedicated Cosmos container with reserved RUs.
- Authentication: Entra External ID (formerly B2C). Each hospital is a tenant.
- API enforces hospitalId from JWT — never trust the client.

**Q223. How do you enforce tenant isolation in API code?**

```csharp
// Middleware extracts tenantId from JWT and stores in context
public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public async Task InvokeAsync(HttpContext ctx, ITenantContext tenant)
    {
        var hospitalId = ctx.User.FindFirst("hospital_id")?.Value;
        if (string.IsNullOrEmpty(hospitalId))
        {
            ctx.Response.StatusCode = 403;
            return;
        }
        tenant.HospitalId = hospitalId;
        await _next(ctx);
    }
}

// Repository auto-filters by tenant
public class InstrumentRepository
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public async Task<List<Instrument>> GetAllAsync()
    {
        return await _db.Instruments
            .Where(i => i.HospitalId == _tenant.HospitalId) // ENFORCED
            .ToListAsync();
    }
}

// EF Global Query Filter — defense in depth
modelBuilder.Entity<Instrument>().HasQueryFilter(
    i => i.HospitalId == _tenant.HospitalId);
```

### Section 13.3 — Healthcare Integration

**Q224. What's FHIR and how would STERIS integrate with hospital EHRs?**

**A.** FHIR (Fast Healthcare Interoperability Resources) is the modern healthcare data standard. RESTful API for healthcare data.

Use Azure Health Data Services:

- Azure FHIR service: managed FHIR endpoint. RBAC via Entra.
- MedTech service: ingests IoT device data and converts to FHIR.
- DICOM service: medical imaging.

```text
// FHIR resources STERIS would create:
// - Device: representing a sterilizer
// - DeviceMetric: cycle parameters
// - Observation: cycle outcomes
// - Procedure: sterilization procedure
// - AuditEvent: compliance trail

// Example FHIR Observation for a cycle
{
  "resourceType": "Observation",
  "status": "final",
  "category": [{
    "coding": [{ "system": "sterilization", "code": "cycle-result" }]
  }],
  "code": { "text": "Sterilization Cycle" },
  "subject": { "reference": "Device/sterilizer-123" },
  "effectiveDateTime": "2026-05-12T10:00:00Z",
  "valueQuantity": { "value": 134, "unit": "Celsius" }
}
```

**Q225. How would STERIS use ML for predictive maintenance?**

**A.** ML.NET for in-app predictions; Azure ML for training pipelines:

```csharp
// Use case: predict device failures from telemetry
// Input features: cycle duration trends, temperature variance,
// pressure deviations, error counts, age of unit
// Output: probability of failure in next 30 days

// Training (Azure ML pipeline):
// 1. Pull historical telemetry + maintenance records from Synapse
// 2. Feature engineering
// 3. Train classifier (gradient boosting works well for tabular)
// 4. Evaluate (precision/recall — false positives are expensive too)
// 5. Export to ONNX

// Inference (in app, via ML.NET):
var prediction = predictionEnginePool.Predict("failure-model", new DeviceFeatures
{
    AvgCycleDurationLast30 = telemetry.AvgDuration,
    TempVarianceLast30 = telemetry.TempVariance,
    ErrorCountLast30 = telemetry.ErrorCount,
    AgeMonths = device.AgeMonths,
    CyclesSinceLastService = device.CyclesSinceService
});

if (prediction.FailureProbability > 0.7)
{
    await _maintenance.ScheduleProactiveAsync(device.Id);
    await _alerts.NotifyAsync(
        $"Device {device.Id} predicted to fail in 30 days");
}
```

Business value: prevent failures = avoid hospital downtime + service contract upsell.

### Section 13.4 — Honest Framings for Healthcare Questions

**Q226. What if Keith asks about FHIR and you haven't worked with it?**

**A.** Be honest:

> ***Say it like this:** "I haven't built a FHIR integration in production. I know Azure Health Data Services manages a FHIR endpoint, that FHIR is the modern healthcare data standard with resources like Patient, Observation, Device. I'd want to spend time with the spec and one of Microsoft's reference implementations before designing a real integration. STERIS likely has expertise on this internally — I'd partner with them for the first project."*

**Q227. What if Keith asks about HIPAA implementation specifics you don't know?**

> ***Say it like this:** "My HIPAA experience is at the architecture-pattern level — what HIPAA-eligible services to pick, why we encrypt at rest with CMK, why audit logs must be retained, why every access path has Entra-based RBAC. The compliance audit specifics — exact attestation processes, BAA review, breach notification timelines — that's where I'd partner with your compliance team. I know the engineering side; I don't pretend to know the legal side."*

**Q228. What's your closing answer if it gets really deep on healthcare specifics?**

> ***Say it like this:** "STERIS is a healthcare company; you have deep healthcare expertise that I don't yet have. What I bring is the engineering discipline — secure design, performance, quality, AI tooling savvy — that travels across domains. Healthcare-specific knowledge I'd build in the first 90 days. Engineering judgment I bring on day one."*
