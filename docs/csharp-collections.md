# C# Collections Overview

Essential C# collections you must know for technical interviews.

---

## 📋 List<T>

**Description**: Dynamic array that grows automatically.

**Time Complexity**:
- Access: O(1)
- Add: O(1) amortized
- Insert: O(n)
- Remove: O(n)

### Example 1: Basic Operations
```csharp
List<int> numbers = new List<int>();
numbers.Add(1);
numbers.Add(2);
numbers.Add(3);
Console.WriteLine(numbers[0]); // 1
numbers.RemoveAt(1); // Remove at index 1
Console.WriteLine(string.Join(", ", numbers)); // 1, 3
```

### Example 2: Finding Elements
```csharp
List<string> names = new List<string> { "Alice", "Bob", "Charlie" };
int index = names.IndexOf("Bob"); // 1
bool exists = names.Contains("David"); // false
```

### Example 3: Sorting
```csharp
List<int> nums = new List<int> { 5, 2, 8, 1, 9 };
nums.Sort(); // [1, 2, 5, 8, 9]
nums.Sort((a, b) => b.CompareTo(a)); // Descending: [9, 8, 5, 2, 1]
```

---

## 🗂️ Dictionary<TKey, TValue>

**Description**: Hash table for key-value pairs.

**Time Complexity**:
- Add: O(1) average
- Lookup: O(1) average
- Remove: O(1) average

### Example 1: Frequency Counter
```csharp
string text = "hello";
Dictionary<char, int> freq = new Dictionary<char, int>();
foreach (char c in text)
{
    if (!freq.ContainsKey(c))
        freq[c] = 0;
    freq[c]++;
}
// freq: {'h': 1, 'e': 1, 'l': 2, 'o': 1}
```

### Example 2: Two Sum Problem
```csharp
public int[] TwoSum(int[] nums, int target)
{
    Dictionary<int, int> map = new Dictionary<int, int>();
    for (int i = 0; i < nums.Length; i++)
    {
        int complement = target - nums[i];
        if (map.ContainsKey(complement))
            return new int[] { map[complement], i };
        map[nums[i]] = i;
    }
    return new int[] { -1, -1 };
}
```

### Example 3: Get or Default
```csharp
Dictionary<string, int> scores = new Dictionary<string, int>
{
    ["Alice"] = 95,
    ["Bob"] = 87
};

int aliceScore = scores.GetValueOrDefault("Alice", 0); // 95
int charlieScore = scores.GetValueOrDefault("Charlie", 0); // 0
```

---

## 🎯 HashSet<T>

**Description**: Unordered collection of unique elements.

**Time Complexity**:
- Add: O(1) average
- Contains: O(1) average
- Remove: O(1) average

### Example 1: Remove Duplicates
```csharp
int[] nums = { 1, 2, 2, 3, 4, 4, 5 };
HashSet<int> unique = new HashSet<int>(nums);
Console.WriteLine(string.Join(", ", unique)); // 1, 2, 3, 4, 5
```

### Example 2: Find Intersection
```csharp
HashSet<int> set1 = new HashSet<int> { 1, 2, 3, 4 };
HashSet<int> set2 = new HashSet<int> { 3, 4, 5, 6 };
set1.IntersectWith(set2);
Console.WriteLine(string.Join(", ", set1)); // 3, 4
```

### Example 3: Detect Duplicates
```csharp
public bool ContainsDuplicate(int[] nums)
{
    HashSet<int> seen = new HashSet<int>();
    foreach (int num in nums)
    {
        if (!seen.Add(num)) // Add returns false if already exists
            return true;
    }
    return false;
}
```

---

## 📥 Queue<T>

**Description**: FIFO (First-In-First-Out) data structure.

**Time Complexity**:
- Enqueue: O(1)
- Dequeue: O(1)
- Peek: O(1)

### Example 1: Basic Queue Operations
```csharp
Queue<int> queue = new Queue<int>();
queue.Enqueue(1);
queue.Enqueue(2);
queue.Enqueue(3);
Console.WriteLine(queue.Peek()); // 1 (doesn't remove)
Console.WriteLine(queue.Dequeue()); // 1 (removes and returns)
Console.WriteLine(queue.Count); // 2
```

### Example 2: BFS Level Order Traversal
```csharp
public class TreeNode
{
    public int val;
    public TreeNode left, right;
    public TreeNode(int val) { this.val = val; }
}

public List<List<int>> LevelOrder(TreeNode root)
{
    List<List<int>> result = new List<List<int>>();
    if (root == null) return result;
    
    Queue<TreeNode> queue = new Queue<TreeNode>();
    queue.Enqueue(root);
    
    while (queue.Count > 0)
    {
        int levelSize = queue.Count;
        List<int> currentLevel = new List<int>();
        
        for (int i = 0; i < levelSize; i++)
        {
            TreeNode node = queue.Dequeue();
            currentLevel.Add(node.val);
            
            if (node.left != null) queue.Enqueue(node.left);
            if (node.right != null) queue.Enqueue(node.right);
        }
        result.Add(currentLevel);
    }
    return result;
}
```

### Example 3: Moving Average
```csharp
public class MovingAverage
{
    private Queue<int> queue;
    private int size;
    private double sum;
    
    public MovingAverage(int size)
    {
        this.queue = new Queue<int>();
        this.size = size;
        this.sum = 0;
    }
    
    public double Next(int val)
    {
        queue.Enqueue(val);
        sum += val;
        
        if (queue.Count > size)
        {
            sum -= queue.Dequeue();
        }
        
        return sum / queue.Count;
    }
}
```

---

## 📤 Stack<T>

**Description**: LIFO (Last-In-First-Out) data structure.

**Time Complexity**:
- Push: O(1)
- Pop: O(1)
- Peek: O(1)

### Example 1: Valid Parentheses
```csharp
public bool IsValid(string s)
{
    Stack<char> stack = new Stack<char>();
    Dictionary<char, char> pairs = new Dictionary<char, char>
    {
        ['('] = ')', ['['] = ']', ['{'] = '}'
    };
    
    foreach (char c in s)
    {
        if (pairs.ContainsKey(c))
        {
            stack.Push(c);
        }
        else
        {
            if (stack.Count == 0 || pairs[stack.Pop()] != c)
                return false;
        }
    }
    return stack.Count == 0;
}
```

### Example 2: Reverse Polish Notation
```csharp
public int EvalRPN(string[] tokens)
{
    Stack<int> stack = new Stack<int>();
    
    foreach (string token in tokens)
    {
        if (token == "+" || token == "-" || token == "*" || token == "/")
        {
            int b = stack.Pop();
            int a = stack.Pop();
            
            int result = token switch
            {
                "+" => a + b,
                "-" => a - b,
                "*" => a * b,
                "/" => a / b,
                _ => 0
            };
            stack.Push(result);
        }
        else
        {
            stack.Push(int.Parse(token));
        }
    }
    return stack.Pop();
}
```

### Example 3: Daily Temperatures (Monotonic Stack)
```csharp
public int[] DailyTemperatures(int[] temperatures)
{
    int n = temperatures.Length;
    int[] result = new int[n];
    Stack<int> stack = new Stack<int>(); // Store indices
    
    for (int i = 0; i < n; i++)
    {
        while (stack.Count > 0 && temperatures[i] > temperatures[stack.Peek()])
        {
            int prevIndex = stack.Pop();
            result[prevIndex] = i - prevIndex;
        }
        stack.Push(i);
    }
    return result;
}
```

---

## 🔗 LinkedList<T>

**Description**: Doubly-linked list.

**Time Complexity**:
- Add First/Last: O(1)
- Remove First/Last: O(1)
- Find: O(n)

### Example 1: Basic Operations
```csharp
LinkedList<int> list = new LinkedList<int>();
list.AddLast(1);
list.AddLast(2);
list.AddFirst(0);
// List: 0 -> 1 -> 2

LinkedListNode<int> node = list.Find(1);
list.AddAfter(node, 10);
// List: 0 -> 1 -> 10 -> 2
```

### Example 2: LRU Cache
```csharp
public class LRUCache
{
    private Dictionary<int, LinkedListNode<(int key, int value)>> cache;
    private LinkedList<(int key, int value)> list;
    private int capacity;
    
    public LRUCache(int capacity)
    {
        this.capacity = capacity;
        cache = new Dictionary<int, LinkedListNode<(int, int)>>();
        list = new LinkedList<(int, int)>();
    }
    
    public int Get(int key)
    {
        if (!cache.ContainsKey(key)) return -1;
        
        var node = cache[key];
        list.Remove(node);
        list.AddFirst(node);
        return node.Value.value;
    }
    
    public void Put(int key, int value)
    {
        if (cache.ContainsKey(key))
        {
            list.Remove(cache[key]);
            cache.Remove(key);
        }
        else if (cache.Count >= capacity)
        {
            var last = list.Last;
            cache.Remove(last.Value.key);
            list.RemoveLast();
        }
        
        var newNode = list.AddFirst((key, value));
        cache[key] = newNode;
    }
}
```

---

## 🔢 SortedSet<T>

**Description**: Balanced binary search tree (Red-Black Tree).

**Time Complexity**:
- Add: O(log n)
- Remove: O(log n)
- Contains: O(log n)

### Example 1: Maintain Sorted Order
```csharp
SortedSet<int> set = new SortedSet<int>();
set.Add(5);
set.Add(2);
set.Add(8);
set.Add(1);
Console.WriteLine(string.Join(", ", set)); // 1, 2, 5, 8
```

### Example 2: Range Queries
```csharp
SortedSet<int> nums = new SortedSet<int> { 1, 3, 5, 7, 9, 11 };

// Get elements >= 5
var greaterThan5 = nums.GetViewBetween(5, int.MaxValue);
Console.WriteLine(string.Join(", ", greaterThan5)); // 5, 7, 9, 11

// Get elements in range [3, 8]
var range = nums.GetViewBetween(3, 8);
Console.WriteLine(string.Join(", ", range)); // 3, 5, 7
```

### Example 3: Sliding Window Median
```csharp
public double[] MedianSlidingWindow(int[] nums, int k)
{
    double[] result = new double[nums.Length - k + 1];
    SortedSet<(int value, int index)> window = new SortedSet<(int, int)>();
    
    for (int i = 0; i < nums.Length; i++)
    {
        window.Add((nums[i], i));
        
        if (window.Count > k)
        {
            window.Remove((nums[i - k], i - k));
        }
        
        if (window.Count == k)
        {
            var sorted = window.ToList();
            result[i - k + 1] = k % 2 == 0 
                ? ((long)sorted[k/2 - 1].value + (long)sorted[k/2].value) / 2.0
                : sorted[k/2].value;
        }
    }
    return result;
}
```

---

## 🎯 PriorityQueue<TElement, TPriority>

**Description**: Min-heap or max-heap (.NET 6+).

**Time Complexity**:
- Enqueue: O(log n)
- Dequeue: O(log n)
- Peek: O(1)

### Example 1: Kth Largest Element
```csharp
public int FindKthLargest(int[] nums, int k)
{
    PriorityQueue<int, int> minHeap = new PriorityQueue<int, int>();
    
    foreach (int num in nums)
    {
        minHeap.Enqueue(num, num);
        if (minHeap.Count > k)
            minHeap.Dequeue();
    }
    
    return minHeap.Peek();
}
```

### Example 2: Merge K Sorted Lists
```csharp
public ListNode MergeKLists(ListNode[] lists)
{
    PriorityQueue<ListNode, int> pq = new PriorityQueue<ListNode, int>();
    
    foreach (var list in lists)
    {
        if (list != null)
            pq.Enqueue(list, list.val);
    }
    
    ListNode dummy = new ListNode(0);
    ListNode current = dummy;
    
    while (pq.Count > 0)
    {
        ListNode node = pq.Dequeue();
        current.next = node;
        current = current.next;
        
        if (node.next != null)
            pq.Enqueue(node.next, node.next.val);
    }
    
    return dummy.next;
}
```

### Example 3: Task Scheduler
```csharp
public int LeastInterval(char[] tasks, int n)
{
    Dictionary<char, int> freq = new Dictionary<char, int>();
    foreach (char task in tasks)
    {
        freq[task] = freq.GetValueOrDefault(task, 0) + 1;
    }
    
    PriorityQueue<int, int> pq = new PriorityQueue<int, int>(
        Comparer<int>.Create((a, b) => b.CompareTo(a)) // Max heap
    );
    
    foreach (int count in freq.Values)
    {
        pq.Enqueue(count, count);
    }
    
    int time = 0;
    Queue<(int count, int availableAt)> cooldown = new Queue<(int, int)>();
    
    while (pq.Count > 0 || cooldown.Count > 0)
    {
        time++;
        
        if (cooldown.Count > 0 && cooldown.Peek().availableAt == time)
        {
            var (count, _) = cooldown.Dequeue();
            pq.Enqueue(count, count);
        }
        
        if (pq.Count > 0)
        {
            int count = pq.Dequeue();
            count--;
            if (count > 0)
            {
                cooldown.Enqueue((count, time + n + 1));
            }
        }
    }
    
    return time;
}
```

---

## 🔄 Value vs Reference Types

### Value Types
- `int`, `double`, `bool`, `char`, `struct`, `enum`
- Stored on stack
- Copied by value

```csharp
int a = 10;
int b = a;
b = 20;
Console.WriteLine(a); // 10 (unchanged)
```

### Reference Types
- `class`, `string`, `array`, collections
- Stored on heap (reference on stack)
- Copied by reference

```csharp
int[] arr1 = { 1, 2, 3 };
int[] arr2 = arr1;
arr2[0] = 99;
Console.WriteLine(arr1[0]); // 99 (changed!)
```

---

## 🎛️ Delegates & Comparers

### Custom Comparers for Sorting
```csharp
// Sort by absolute value
List<int> nums = new List<int> { -5, 2, -8, 1, 9 };
nums.Sort((a, b) => Math.Abs(a).CompareTo(Math.Abs(b)));

// Sort strings by length
List<string> words = new List<string> { "apple", "pie", "banana" };
words.Sort((a, b) => a.Length.CompareTo(b.Length));

// Sort 2D array
int[][] intervals = { new[] {1, 3}, new[] {2, 6}, new[] {8, 10} };
Array.Sort(intervals, (a, b) => a[0].CompareTo(b[0]));
```

---

## 🎨 LINQ (Light Usage)

**Warning**: Avoid heavy LINQ in interviews. Use sparingly.

```csharp
// Acceptable LINQ usage
int[] nums = { 1, 2, 3, 4, 5 };
int sum = nums.Sum();
int max = nums.Max();
int[] doubled = nums.Select(x => x * 2).ToArray();

// Avoid complex LINQ chains in interviews
// Bad: nums.Where(x => x > 2).OrderBy(x => x).GroupBy(x => x % 2).Select(...)
```

---

## 🎯 Key Takeaways

1. **Know time complexities** for all operations
2. **Practice implementing** data structures from scratch
3. **Understand when to use** each collection
4. **Master Dictionary and HashSet** - they solve 30% of problems
5. **Be comfortable with** custom comparers and delegates