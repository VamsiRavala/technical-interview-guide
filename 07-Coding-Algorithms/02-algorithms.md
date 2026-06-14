# Essential Algorithms Mastery

## Table of Contents
1. [Sorting Algorithms](#sorting-algorithms)
2. [Searching Algorithms](#searching-algorithms)
3. [Graph Algorithms](#graph-algorithms)
4. [Dynamic Programming](#dynamic-programming)
5. [Greedy Algorithms](#greedy-algorithms)
6. [Divide and Conquer](#divide-and-conquer)
7. [Backtracking](#backtracking)
8. [String Algorithms](#string-algorithms)

---

## Sorting Algorithms

### Quick Sort
**Time Complexity**: O(n log n) average, O(n²) worst case  
**Space Complexity**: O(log n)  
**When to Use**: General-purpose sorting, average case performance matters

**C# Implementation**:
```csharp
public class QuickSort
{
    public void Sort(int[] arr, int low, int high)
    {
        if (low < high)
        {
            int pi = Partition(arr, low, high);
            Sort(arr, low, pi - 1);
            Sort(arr, pi + 1, high);
        }
    }

    private int Partition(int[] arr, int low, int high)
    {
        int pivot = arr[high];
        int i = low - 1;

        for (int j = low; j < high; j++)
        {
            if (arr[j] < pivot)
            {
                i++;
                Swap(arr, i, j);
            }
        }
        Swap(arr, i + 1, high);
        return i + 1;
    }

    private void Swap(int[] arr, int i, int j)
    {
        int temp = arr[i];
        arr[i] = arr[j];
        arr[j] = temp;
    }
}
```

**JavaScript Implementation**:
```javascript
function quickSort(arr, low = 0, high = arr.length - 1) {
    if (low < high) {
        const pi = partition(arr, low, high);
        quickSort(arr, low, pi - 1);
        quickSort(arr, pi + 1, high);
    }
    return arr;
}

function partition(arr, low, high) {
    const pivot = arr[high];
    let i = low - 1;

    for (let j = low; j < high; j++) {
        if (arr[j] < pivot) {
            i++;
            [arr[i], arr[j]] = [arr[j], arr[i]];
        }
    }
    [arr[i + 1], arr[high]] = [arr[high], arr[i + 1]];
    return i + 1;
}
```

### Merge Sort
**Time Complexity**: O(n log n) all cases  
**Space Complexity**: O(n)  
**When to Use**: Stable sort needed, worst-case guarantees required

**C# Implementation**:
```csharp
public class MergeSort
{
    public void Sort(int[] arr, int left, int right)
    {
        if (left < right)
        {
            int mid = left + (right - left) / 2;
            Sort(arr, left, mid);
            Sort(arr, mid + 1, right);
            Merge(arr, left, mid, right);
        }
    }

    private void Merge(int[] arr, int left, int mid, int right)
    {
        int n1 = mid - left + 1;
        int n2 = right - mid;

        int[] L = new int[n1];
        int[] R = new int[n2];

        Array.Copy(arr, left, L, 0, n1);
        Array.Copy(arr, mid + 1, R, 0, n2);

        int i = 0, j = 0, k = left;

        while (i < n1 && j < n2)
        {
            if (L[i] <= R[j])
                arr[k++] = L[i++];
            else
                arr[k++] = R[j++];
        }

        while (i < n1) arr[k++] = L[i++];
        while (j < n2) arr[k++] = R[j++];
    }
}
```

### Heap Sort
**Time Complexity**: O(n log n)  
**Space Complexity**: O(1)  
**When to Use**: In-place sorting with guaranteed performance

---

## Searching Algorithms

### Binary Search
**Time Complexity**: O(log n)  
**Space Complexity**: O(1)  
**Prerequisites**: Sorted array

**C# Implementation**:
```csharp
public class BinarySearch
{
    public int Search(int[] arr, int target)
    {
        int left = 0, right = arr.Length - 1;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;

            if (arr[mid] == target)
                return mid;
            else if (arr[mid] < target)
                left = mid + 1;
            else
                right = mid - 1;
        }

        return -1; // Not found
    }

    // Find first occurrence
    public int SearchFirst(int[] arr, int target)
    {
        int left = 0, right = arr.Length - 1;
        int result = -1;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;

            if (arr[mid] == target)
            {
                result = mid;
                right = mid - 1; // Continue searching left
            }
            else if (arr[mid] < target)
                left = mid + 1;
            else
                right = mid - 1;
        }

        return result;
    }
}
```

**JavaScript Implementation**:
```javascript
function binarySearch(arr, target) {
    let left = 0, right = arr.length - 1;

    while (left <= right) {
        const mid = Math.floor(left + (right - left) / 2);

        if (arr[mid] === target) return mid;
        else if (arr[mid] < target) left = mid + 1;
        else right = mid - 1;
    }

    return -1;
}
```

---

## Graph Algorithms

### Depth-First Search (DFS)
**Time Complexity**: O(V + E)  
**Space Complexity**: O(V)

**C# Implementation**:
```csharp
public class GraphDFS
{
    private Dictionary<int, List<int>> adjacencyList;

    public GraphDFS()
    {
        adjacencyList = new Dictionary<int, List<int>>();
    }

    public void AddEdge(int u, int v)
    {
        if (!adjacencyList.ContainsKey(u))
            adjacencyList[u] = new List<int>();
        adjacencyList[u].Add(v);
    }

    public void DFS(int start)
    {
        HashSet<int> visited = new HashSet<int>();
        DFSHelper(start, visited);
    }

    private void DFSHelper(int vertex, HashSet<int> visited)
    {
        visited.Add(vertex);
        Console.WriteLine(vertex);

        if (adjacencyList.ContainsKey(vertex))
        {
            foreach (int neighbor in adjacencyList[vertex])
            {
                if (!visited.Contains(neighbor))
                    DFSHelper(neighbor, visited);
            }
        }
    }
}
```

### Breadth-First Search (BFS)
**Time Complexity**: O(V + E)  
**Space Complexity**: O(V)

**C# Implementation**:
```csharp
public class GraphBFS
{
    private Dictionary<int, List<int>> adjacencyList;

    public void BFS(int start)
    {
        HashSet<int> visited = new HashSet<int>();
        Queue<int> queue = new Queue<int>();

        visited.Add(start);
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            int vertex = queue.Dequeue();
            Console.WriteLine(vertex);

            if (adjacencyList.ContainsKey(vertex))
            {
                foreach (int neighbor in adjacencyList[vertex])
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }
    }
}
```

### Dijkstra's Algorithm
**Time Complexity**: O((V + E) log V) with priority queue  
**Use Case**: Shortest path in weighted graphs

**C# Implementation**:
```csharp
public class Dijkstra
{
    public Dictionary<int, int> FindShortestPath(
        Dictionary<int, List<(int node, int weight)>> graph, 
        int start)
    {
        var distances = new Dictionary<int, int>();
        var priorityQueue = new PriorityQueue<int, int>();
        var visited = new HashSet<int>();

        // Initialize distances
        foreach (var node in graph.Keys)
            distances[node] = int.MaxValue;
        distances[start] = 0;

        priorityQueue.Enqueue(start, 0);

        while (priorityQueue.Count > 0)
        {
            var current = priorityQueue.Dequeue();

            if (visited.Contains(current)) continue;
            visited.Add(current);

            if (!graph.ContainsKey(current)) continue;

            foreach (var (neighbor, weight) in graph[current])
            {
                int newDist = distances[current] + weight;
                if (newDist < distances[neighbor])
                {
                    distances[neighbor] = newDist;
                    priorityQueue.Enqueue(neighbor, newDist);
                }
            }
        }

        return distances;
    }
}
```

---

## Dynamic Programming

### Fibonacci (Memoization)
**Time Complexity**: O(n)  
**Space Complexity**: O(n)

**C# Implementation**:
```csharp
public class DynamicProgramming
{
    // Top-down approach (Memoization)
    public long Fibonacci(int n, Dictionary<int, long> memo = null)
    {
        if (memo == null) memo = new Dictionary<int, long>();
        
        if (n <= 1) return n;
        if (memo.ContainsKey(n)) return memo[n];

        memo[n] = Fibonacci(n - 1, memo) + Fibonacci(n - 2, memo);
        return memo[n];
    }

    // Bottom-up approach (Tabulation)
    public long FibonacciDP(int n)
    {
        if (n <= 1) return n;

        long[] dp = new long[n + 1];
        dp[0] = 0;
        dp[1] = 1;

        for (int i = 2; i <= n; i++)
            dp[i] = dp[i - 1] + dp[i - 2];

        return dp[n];
    }
}
```

### Longest Common Subsequence
**Time Complexity**: O(m × n)  
**Space Complexity**: O(m × n)

**C# Implementation**:
```csharp
public class LCS
{
    public int LongestCommonSubsequence(string text1, string text2)
    {
        int m = text1.Length, n = text2.Length;
        int[,] dp = new int[m + 1, n + 1];

        for (int i = 1; i <= m; i++)
        {
            for (int j = 1; j <= n; j++)
            {
                if (text1[i - 1] == text2[j - 1])
                    dp[i, j] = dp[i - 1, j - 1] + 1;
                else
                    dp[i, j] = Math.Max(dp[i - 1, j], dp[i, j - 1]);
            }
        }

        return dp[m, n];
    }
}
```

### 0/1 Knapsack Problem
**Time Complexity**: O(n × W)  
**Space Complexity**: O(n × W)

**C# Implementation**:
```csharp
public class Knapsack
{
    public int KnapsackDP(int[] weights, int[] values, int capacity)
    {
        int n = weights.Length;
        int[,] dp = new int[n + 1, capacity + 1];

        for (int i = 1; i <= n; i++)
        {
            for (int w = 1; w <= capacity; w++)
            {
                if (weights[i - 1] <= w)
                {
                    dp[i, w] = Math.Max(
                        values[i - 1] + dp[i - 1, w - weights[i - 1]],
                        dp[i - 1, w]
                    );
                }
                else
                {
                    dp[i, w] = dp[i - 1, w];
                }
            }
        }

        return dp[n, capacity];
    }
}
```

---

## Greedy Algorithms

### Activity Selection Problem
**Time Complexity**: O(n log n)  
**Space Complexity**: O(1)

**C# Implementation**:
```csharp
public class GreedyAlgorithms
{
    public class Activity
    {
        public int Start { get; set; }
        public int End { get; set; }
    }

    public List<Activity> SelectActivities(List<Activity> activities)
    {
        // Sort by finish time
        activities.Sort((a, b) => a.End.CompareTo(b.End));

        List<Activity> selected = new List<Activity>();
        selected.Add(activities[0]);

        int lastEnd = activities[0].End;

        for (int i = 1; i < activities.Count; i++)
        {
            if (activities[i].Start >= lastEnd)
            {
                selected.Add(activities[i]);
                lastEnd = activities[i].End;
            }
        }

        return selected;
    }
}
```

---

## Backtracking

### N-Queens Problem
**Time Complexity**: O(N!)  
**Space Complexity**: O(N²)

**C# Implementation**:
```csharp
public class NQueens
{
    public IList<IList<string>> SolveNQueens(int n)
    {
        var result = new List<IList<string>>();
        var board = new char[n][];
        
        for (int i = 0; i < n; i++)
        {
            board[i] = new char[n];
            Array.Fill(board[i], '.');
        }

        Backtrack(board, 0, result);
        return result;
    }

    private void Backtrack(char[][] board, int row, IList<IList<string>> result)
    {
        if (row == board.Length)
        {
            result.Add(board.Select(r => new string(r)).ToList());
            return;
        }

        for (int col = 0; col < board.Length; col++)
        {
            if (IsValid(board, row, col))
            {
                board[row][col] = 'Q';
                Backtrack(board, row + 1, result);
                board[row][col] = '.';
            }
        }
    }

    private bool IsValid(char[][] board, int row, int col)
    {
        // Check column
        for (int i = 0; i < row; i++)
            if (board[i][col] == 'Q') return false;

        // Check diagonal
        for (int i = row - 1, j = col - 1; i >= 0 && j >= 0; i--, j--)
            if (board[i][j] == 'Q') return false;

        // Check anti-diagonal
        for (int i = row - 1, j = col + 1; i >= 0 && j < board.Length; i--, j++)
            if (board[i][j] == 'Q') return false;

        return true;
    }
}
```

---

## String Algorithms

### KMP Pattern Matching
**Time Complexity**: O(n + m)  
**Space Complexity**: O(m)

**C# Implementation**:
```csharp
public class KMP
{
    public int[] ComputeLPS(string pattern)
    {
        int[] lps = new int[pattern.Length];
        int len = 0, i = 1;

        while (i < pattern.Length)
        {
            if (pattern[i] == pattern[len])
            {
                len++;
                lps[i] = len;
                i++;
            }
            else
            {
                if (len != 0)
                    len = lps[len - 1];
                else
                {
                    lps[i] = 0;
                    i++;
                }
            }
        }

        return lps;
    }

    public List<int> Search(string text, string pattern)
    {
        List<int> result = new List<int>();
        int[] lps = ComputeLPS(pattern);
        int i = 0, j = 0;

        while (i < text.Length)
        {
            if (text[i] == pattern[j])
            {
                i++;
                j++;
            }

            if (j == pattern.Length)
            {
                result.Add(i - j);
                j = lps[j - 1];
            }
            else if (i < text.Length && text[i] != pattern[j])
            {
                if (j != 0)
                    j = lps[j - 1];
                else
                    i++;
            }
        }

        return result;
    }
}
```

---

## Interview Tips

### Algorithm Selection Guide
1. **Sorting needed?** → Quick Sort, Merge Sort
2. **Shortest path?** → Dijkstra, BFS
3. **Optimization problem?** → Dynamic Programming, Greedy
4. **All combinations?** → Backtracking
5. **Pattern matching?** → KMP, Rabin-Karp

### Common Mistakes to Avoid
- Not considering edge cases (empty arrays, single elements)
- Forgetting to handle duplicates
- Off-by-one errors in loops
- Not optimizing space complexity when possible
- Missing base cases in recursion

### Practice Resources
- LeetCode: Top Interview Questions
- HackerRank: Algorithm Challenges
- Project Euler: Mathematical algorithms

---

**Next**: [03-big-o-analysis.md](./03-big-o-analysis.md) - Understanding time and space complexity analysis