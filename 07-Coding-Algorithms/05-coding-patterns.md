# Coding Patterns (The 15+ Interview Patterns)

> **This is the most important file in the section.** ~90% of FAANG coding questions are variations of ~18 recurring patterns. Once you can *recognize* the pattern from the problem statement, the solution writes itself. This is exactly how a senior engineer should prepare: learn the templates, drill the recognition triggers, and stop treating every problem as new.

---

## How to Use This File

For each pattern you get:
- **When to recognize it** — the verbal/structural triggers in the problem statement.
- **The template** — a reusable C# skeleton you can adapt under pressure.
- **Example problems** — 2–3 canonical LeetCode problems to drill it.

> **Recognition is the skill.** Read the trigger lists until you can pattern-match a fresh problem within 2–3 minutes. The code is the easy part once you've named the pattern.

---

## Table of Contents

1. [Two Pointers](#1-two-pointers)
2. [Sliding Window](#2-sliding-window)
3. [Fast & Slow Pointers](#3-fast--slow-pointers)
4. [Merge Intervals](#4-merge-intervals)
5. [Cyclic Sort](#5-cyclic-sort)
6. [In-place Linked List Reversal](#6-in-place-linked-list-reversal)
7. [Breadth-First Search (BFS)](#7-breadth-first-search-bfs)
8. [Depth-First Search (DFS)](#8-depth-first-search-dfs)
9. [Two Heaps](#9-two-heaps)
10. [Subsets / Backtracking](#10-subsets--backtracking)
11. [Modified Binary Search](#11-modified-binary-search)
12. [Top-K Elements (Heap)](#12-top-k-elements-heap)
13. [K-way Merge](#13-k-way-merge)
14. [Topological Sort](#14-topological-sort)
15. [Dynamic Programming](#15-dynamic-programming)
16. [Trie](#16-trie)
17. [Union-Find (Disjoint Set)](#17-union-find-disjoint-set)
18. [Quick Pattern-Recognition Cheat Sheet](#quick-pattern-recognition-cheat-sheet)

---

## 1. Two Pointers

### When to recognize it
- Input is a **sorted array** (or you can sort it) and you need a **pair / triplet** summing to a target.
- Comparing elements from **both ends** (palindrome, container).
- Removing duplicates / partitioning **in place**.
- Keywords: "sorted," "pair," "two values," "palindrome," "in-place," "without extra space."

**Time:** O(n) (or O(n²) for triplets). **Space:** O(1).

### Template

```csharp
// Opposite-direction pointers on a sorted array
public int[] TwoSumSorted(int[] nums, int target) {
    int left = 0, right = nums.Length - 1;
    while (left < right) {
        int sum = nums[left] + nums[right];
        if (sum == target) return new[] { left, right };
        if (sum < target) left++;     // need bigger → move left up
        else right--;                 // need smaller → move right down
    }
    return Array.Empty<int>();
}
```

```csharp
// Same-direction (slow/fast) for in-place filtering
public int RemoveDuplicates(int[] nums) {
    if (nums.Length == 0) return 0;
    int slow = 0;
    for (int fast = 1; fast < nums.Length; fast++) {
        if (nums[fast] != nums[slow]) {
            slow++;
            nums[slow] = nums[fast];
        }
    }
    return slow + 1;
}
```

### Example problems
- **167. Two Sum II – Input Array Is Sorted** (Easy)
- **15. 3Sum** (Medium) — fix one pointer, two-pointer the rest
- **11. Container With Most Water** (Medium)

---

## 2. Sliding Window

### When to recognize it
- **Contiguous subarray / substring** with a constraint (longest, shortest, sum, at most K distinct).
- You'd otherwise nest two loops over a range — collapse them into a window.
- Keywords: "substring," "subarray," "contiguous," "longest/shortest," "window of size K," "at most/exactly K."

**Time:** O(n) — each element enters and leaves the window once. **Space:** O(k) for the window's bookkeeping.

### Template

```csharp
// Variable-size window: longest substring without repeating characters
public int LengthOfLongestSubstring(string s) {
    var lastSeen = new Dictionary<char, int>();
    int left = 0, best = 0;
    for (int right = 0; right < s.Length; right++) {
        char c = s[right];
        if (lastSeen.TryGetValue(c, out int idx) && idx >= left) {
            left = idx + 1;            // shrink window past the duplicate
        }
        lastSeen[c] = right;
        best = Math.Max(best, right - left + 1);
    }
    return best;
}
```

```csharp
// Fixed-size window: max sum of any subarray of size k
public int MaxSumWindow(int[] nums, int k) {
    int sum = 0, best;
    for (int i = 0; i < k; i++) sum += nums[i];
    best = sum;
    for (int i = k; i < nums.Length; i++) {
        sum += nums[i] - nums[i - k];   // slide: add new, drop old
        best = Math.Max(best, sum);
    }
    return best;
}
```

### Example problems
- **3. Longest Substring Without Repeating Characters** (Medium)
- **76. Minimum Window Substring** (Hard)
- **424. Longest Repeating Character Replacement** (Medium)

---

## 3. Fast & Slow Pointers

### When to recognize it
- **Linked list** with a possible **cycle**, or you need the **middle** node.
- Detecting cycles in a sequence defined by a function (Happy Number).
- Keywords: "cycle," "loop," "middle," "without extra space," "Floyd."

**Time:** O(n). **Space:** O(1).

### Template

```csharp
// Floyd's cycle detection + find cycle start
public ListNode DetectCycle(ListNode head) {
    ListNode slow = head, fast = head;
    while (fast != null && fast.next != null) {
        slow = slow.next;
        fast = fast.next.next;
        if (slow == fast) {            // cycle confirmed
            ListNode p = head;
            while (p != slow) { p = p.next; slow = slow.next; }
            return p;                  // cycle entry point
        }
    }
    return null;                       // no cycle
}
```

```csharp
// Find the middle node (slow lands on middle when fast hits the end)
public ListNode FindMiddle(ListNode head) {
    ListNode slow = head, fast = head;
    while (fast != null && fast.next != null) {
        slow = slow.next;
        fast = fast.next.next;
    }
    return slow;
}
```

### Example problems
- **141 / 142. Linked List Cycle (I & II)** (Easy / Medium)
- **876. Middle of the Linked List** (Easy)
- **202. Happy Number** (Easy)

---

## 4. Merge Intervals

### When to recognize it
- Input is a **list of intervals** `[start, end]` and you need to **merge, insert, or count overlaps**.
- Scheduling / meeting-room / calendar problems.
- Keywords: "intervals," "overlapping," "merge," "meeting rooms," "ranges."

**Time:** O(n log n) for the sort. **Space:** O(n) for output.

### Template

```csharp
// Merge all overlapping intervals
public int[][] Merge(int[][] intervals) {
    Array.Sort(intervals, (a, b) => a[0].CompareTo(b[0]));   // sort by start
    var result = new List<int[]>();
    foreach (var cur in intervals) {
        if (result.Count == 0 || result[^1][1] < cur[0]) {
            result.Add(cur);                          // no overlap → new interval
        } else {
            result[^1][1] = Math.Max(result[^1][1], cur[1]); // overlap → extend end
        }
    }
    return result.ToArray();
}
```

```csharp
// Meeting Rooms II: minimum rooms = max concurrent overlaps (min-heap of end times)
public int MinMeetingRooms(int[][] intervals) {
    Array.Sort(intervals, (a, b) => a[0].CompareTo(b[0]));
    var endTimes = new PriorityQueue<int, int>();   // min-heap of meeting ends
    foreach (var m in intervals) {
        if (endTimes.Count > 0 && endTimes.Peek() <= m[0])
            endTimes.Dequeue();                     // a room freed up
        endTimes.Enqueue(m[1], m[1]);
    }
    return endTimes.Count;
}
```

### Example problems
- **56. Merge Intervals** (Medium)
- **57. Insert Interval** (Medium)
- **253. Meeting Rooms II** (Medium)

---

## 5. Cyclic Sort

### When to recognize it
- Array contains numbers in a **known range**, typically **1..n** or **0..n**.
- Asked to find **missing / duplicate / misplaced** numbers, ideally in **O(1) extra space**.
- Keywords: "range 1 to n," "n distinct numbers," "find missing," "find duplicate."

**Time:** O(n). **Space:** O(1).

### Template

```csharp
// Place each value v at index v-1, then scan for the mismatch
public int FindMissingNumber(int[] nums) {
    int i = 0;
    while (i < nums.Length) {
        int correct = nums[i] - 1;                  // value v belongs at index v-1
        if (nums[i] > 0 && nums[i] <= nums.Length && nums[i] != nums[correct]) {
            (nums[i], nums[correct]) = (nums[correct], nums[i]);  // swap into place
        } else {
            i++;
        }
    }
    for (int j = 0; j < nums.Length; j++)
        if (nums[j] != j + 1) return j + 1;         // first wrong slot = missing
    return nums.Length + 1;
}
```

### Example problems
- **268. Missing Number** (Easy)
- **448. Find All Numbers Disappeared in an Array** (Easy)
- **287. Find the Duplicate Number** (Medium)

---

## 6. In-place Linked List Reversal

### When to recognize it
- Reverse a **linked list** or a **sub-section** of one, **without extra space**.
- Reverse in **groups of k**.
- Keywords: "reverse," "in place," "O(1) space," "k-group."

**Time:** O(n). **Space:** O(1).

### Template

```csharp
// Reverse an entire singly linked list
public ListNode Reverse(ListNode head) {
    ListNode prev = null, cur = head;
    while (cur != null) {
        ListNode next = cur.next;   // save
        cur.next = prev;            // reverse pointer
        prev = cur;                 // advance prev
        cur = next;                 // advance cur
    }
    return prev;                    // new head
}
```

```csharp
// Reverse the sublist between positions left..right (1-indexed)
public ListNode ReverseBetween(ListNode head, int left, int right) {
    var dummy = new ListNode(0, head);
    ListNode prev = dummy;
    for (int i = 0; i < left - 1; i++) prev = prev.next;  // node before sublist
    ListNode cur = prev.next;
    for (int i = 0; i < right - left; i++) {              // front-insert the next node
        ListNode next = cur.next;
        cur.next = next.next;
        next.next = prev.next;
        prev.next = next;
    }
    return dummy.next;
}
```

### Example problems
- **206. Reverse Linked List** (Easy)
- **92. Reverse Linked List II** (Medium)
- **25. Reverse Nodes in k-Group** (Hard)

---

## 7. Breadth-First Search (BFS)

### When to recognize it
- **Shortest path / minimum steps** in an **unweighted** graph or grid.
- **Level-by-level** tree traversal.
- "Spreading" simulations (rotting oranges, infection).
- Keywords: "shortest," "minimum number of steps," "level," "nearest," "fewest moves."

**Time:** O(V + E) (or O(rows·cols) for grids). **Space:** O(V) for the queue.

### Template

```csharp
// Tree level-order traversal
public IList<IList<int>> LevelOrder(TreeNode root) {
    var result = new List<IList<int>>();
    if (root == null) return result;
    var queue = new Queue<TreeNode>();
    queue.Enqueue(root);
    while (queue.Count > 0) {
        int size = queue.Count;            // freeze count = one level
        var level = new List<int>();
        for (int i = 0; i < size; i++) {
            var node = queue.Dequeue();
            level.Add(node.val);
            if (node.left != null) queue.Enqueue(node.left);
            if (node.right != null) queue.Enqueue(node.right);
        }
        result.Add(level);
    }
    return result;
}
```

```csharp
// Grid BFS shortest path (multi-source capable)
public int ShortestSpread(int[][] grid) {
    int rows = grid.Length, cols = grid[0].Length;
    var queue = new Queue<(int r, int c)>();
    // ... enqueue sources, mark visited ...
    int[][] dirs = { new[]{1,0}, new[]{-1,0}, new[]{0,1}, new[]{0,-1} };
    int steps = 0;
    while (queue.Count > 0) {
        int size = queue.Count;
        for (int i = 0; i < size; i++) {
            var (r, c) = queue.Dequeue();
            foreach (var d in dirs) {
                int nr = r + d[0], nc = c + d[1];
                if (nr >= 0 && nr < rows && nc >= 0 && nc < cols && /* valid & unvisited */ true) {
                    // mark visited, enqueue (nr, nc)
                }
            }
        }
        steps++;
    }
    return steps;
}
```

### Example problems
- **102. Binary Tree Level Order Traversal** (Medium)
- **994. Rotting Oranges** (Medium) — multi-source BFS
- **127. Word Ladder** (Hard)

---

## 8. Depth-First Search (DFS)

### When to recognize it
- **Explore all paths**, connected components, or reachability.
- **Tree recursion** (path sums, depth, validation).
- **Flood fill** / island counting on a grid.
- Keywords: "all paths," "connected," "islands," "explore," "reachable," "number of components."

**Time:** O(V + E). **Space:** O(V) for visited + O(height) recursion stack.

### Template

```csharp
// Grid DFS: count islands (flood fill)
public int NumIslands(char[][] grid) {
    int count = 0;
    for (int r = 0; r < grid.Length; r++)
        for (int c = 0; c < grid[0].Length; c++)
            if (grid[r][c] == '1') { Flood(grid, r, c); count++; }
    return count;
}

private void Flood(char[][] grid, int r, int c) {
    if (r < 0 || r >= grid.Length || c < 0 || c >= grid[0].Length || grid[r][c] != '1')
        return;                       // out of bounds or water → stop
    grid[r][c] = '0';                 // mark visited (sink the land)
    Flood(grid, r + 1, c);
    Flood(grid, r - 1, c);
    Flood(grid, r, c + 1);
    Flood(grid, r, c - 1);
}
```

```csharp
// Graph DFS with visited set + cycle detection (directed)
private bool HasCycle(int node, Dictionary<int, List<int>> graph,
                      int[] state) { // 0=unvisited, 1=in-stack, 2=done
    state[node] = 1;
    foreach (var nei in graph.GetValueOrDefault(node, new List<int>())) {
        if (state[nei] == 1) return true;                  // back edge → cycle
        if (state[nei] == 0 && HasCycle(nei, graph, state)) return true;
    }
    state[node] = 2;
    return false;
}
```

### Example problems
- **200. Number of Islands** (Medium)
- **133. Clone Graph** (Medium)
- **417. Pacific Atlantic Water Flow** (Medium)

---

## 9. Two Heaps

### When to recognize it
- Need the **median** or balance of a **stream** of numbers.
- Continuously split data into a **smaller half** and a **larger half**.
- Keywords: "median," "stream," "balance," "smaller/larger half."

**Time:** O(log n) per insert, O(1) median. **Space:** O(n).

### Template

```csharp
// Median of a data stream: maxHeap (lower half) + minHeap (upper half)
public class MedianFinder {
    private readonly PriorityQueue<int, int> _low =   // max-heap (invert priority)
        new(Comparer<int>.Create((a, b) => b.CompareTo(a)));
    private readonly PriorityQueue<int, int> _high =  // min-heap
        new();

    public void AddNum(int num) {
        _low.Enqueue(num, num);
        _high.Enqueue(_low.Peek(), _low.Dequeue());   // funnel top of low into high
        if (_high.Count > _low.Count) {               // rebalance: low holds the extra
            _low.Enqueue(_high.Peek(), _high.Dequeue());
        }
    }

    public double FindMedian() =>
        _low.Count > _high.Count ? _low.Peek()
                                 : (_low.Peek() + _high.Peek()) / 2.0;
}
```

### Example problems
- **295. Find Median from Data Stream** (Hard)
- **480. Sliding Window Median** (Hard)
- **502. IPO** (Hard)

---

## 10. Subsets / Backtracking

### When to recognize it
- Generate **all** combinations, permutations, subsets, or partitions.
- Constraint-satisfaction (N-Queens, Sudoku).
- Keywords: "all possible," "every combination/permutation," "generate," "enumerate."

**Time:** exponential — O(2ⁿ) subsets, O(n!) permutations. **Space:** O(n) recursion depth.

### Template

```csharp
// Generic backtracking skeleton
public IList<IList<int>> Subsets(int[] nums) {
    var result = new List<IList<int>>();
    Backtrack(nums, 0, new List<int>(), result);
    return result;
}

private void Backtrack(int[] nums, int start, List<int> path, IList<IList<int>> result) {
    result.Add(new List<int>(path));        // record current subset (every node)
    for (int i = start; i < nums.Length; i++) {
        path.Add(nums[i]);                  // choose
        Backtrack(nums, i + 1, path, result); // explore
        path.RemoveAt(path.Count - 1);      // un-choose (backtrack)
    }
}
```

```csharp
// Permutations variant (uses a "used" set instead of a start index)
private void Permute(int[] nums, bool[] used, List<int> path, IList<IList<int>> result) {
    if (path.Count == nums.Length) { result.Add(new List<int>(path)); return; }
    for (int i = 0; i < nums.Length; i++) {
        if (used[i]) continue;
        used[i] = true; path.Add(nums[i]);
        Permute(nums, used, path, result);
        used[i] = false; path.RemoveAt(path.Count - 1);
    }
}
```

> **The mantra:** *choose → explore → un-choose.* For "II" variants with duplicates, sort first and skip `nums[i] == nums[i-1]` at the same recursion level.

### Example problems
- **78. Subsets** (Medium)
- **46. Permutations** (Medium)
- **39. Combination Sum** (Medium)

---

## 11. Modified Binary Search

### When to recognize it
- **Sorted** (or rotated-sorted) input and you need O(log n).
- **Binary search on the answer** — minimize/maximize a value where feasibility is monotonic ("Koko eating bananas," "ship within D days").
- Keywords: "sorted," "rotated," "find peak," "minimum capacity such that," "smallest value that works."

**Time:** O(log n). **Space:** O(1).

### Template

```csharp
// Search in a rotated sorted array
public int Search(int[] nums, int target) {
    int lo = 0, hi = nums.Length - 1;
    while (lo <= hi) {
        int mid = lo + (hi - lo) / 2;
        if (nums[mid] == target) return mid;
        if (nums[lo] <= nums[mid]) {                 // left half is sorted
            if (nums[lo] <= target && target < nums[mid]) hi = mid - 1;
            else lo = mid + 1;
        } else {                                     // right half is sorted
            if (nums[mid] < target && target <= nums[hi]) lo = mid + 1;
            else hi = mid - 1;
        }
    }
    return -1;
}
```

```csharp
// Binary search on the answer: smallest eating speed to finish in h hours
public int MinEatingSpeed(int[] piles, int h) {
    int lo = 1, hi = piles.Max();
    while (lo < hi) {
        int mid = lo + (hi - lo) / 2;
        long hours = 0;
        foreach (int p in piles) hours += (p + mid - 1) / mid;  // ceil division
        if (hours <= h) hi = mid;     // feasible → try slower
        else lo = mid + 1;            // too slow → go faster
    }
    return lo;
}
```

### Example problems
- **33. Search in Rotated Sorted Array** (Medium)
- **153. Find Minimum in Rotated Sorted Array** (Medium)
- **875. Koko Eating Bananas** (Medium) — binary search on the answer

---

## 12. Top-K Elements (Heap)

### When to recognize it
- Find the **K largest / smallest / most frequent** elements.
- You **don't** need full sorting — just the top K.
- Keywords: "top K," "K largest/smallest," "K most frequent," "Kth largest."

**Time:** O(n log k). **Space:** O(k).

### Template

```csharp
// Kth largest element using a min-heap of size k
public int FindKthLargest(int[] nums, int k) {
    var minHeap = new PriorityQueue<int, int>();      // min-heap
    foreach (int n in nums) {
        minHeap.Enqueue(n, n);
        if (minHeap.Count > k) minHeap.Dequeue();     // evict smallest → keep k largest
    }
    return minHeap.Peek();                            // root = kth largest
}
```

```csharp
// Top K frequent elements: count, then heap of size k
public int[] TopKFrequent(int[] nums, int k) {
    var freq = new Dictionary<int, int>();
    foreach (int n in nums) freq[n] = freq.GetValueOrDefault(n) + 1;
    var heap = new PriorityQueue<int, int>();         // min-heap keyed by frequency
    foreach (var kv in freq) {
        heap.Enqueue(kv.Key, kv.Value);
        if (heap.Count > k) heap.Dequeue();
    }
    var result = new int[k];
    for (int i = k - 1; i >= 0; i--) result[i] = heap.Dequeue();
    return result;
}
```

> **Heap-direction trick:** to keep the K *largest*, use a **min-heap** of size k (evict the smallest). To keep the K *smallest*, use a **max-heap**.

### Example problems
- **215. Kth Largest Element in an Array** (Medium)
- **347. Top K Frequent Elements** (Medium)
- **973. K Closest Points to Origin** (Medium)

---

## 13. K-way Merge

### When to recognize it
- Merge **K sorted lists / arrays** into one sorted output.
- Find the smallest/largest across multiple sorted sources.
- Keywords: "K sorted lists," "merge K," "smallest range covering K lists."

**Time:** O(N log k) where N = total elements. **Space:** O(k) for the heap.

### Template

```csharp
// Merge k sorted linked lists with a min-heap of current heads
public ListNode MergeKLists(ListNode[] lists) {
    var heap = new PriorityQueue<ListNode, int>();
    foreach (var node in lists)
        if (node != null) heap.Enqueue(node, node.val);

    var dummy = new ListNode(0);
    var tail = dummy;
    while (heap.Count > 0) {
        var smallest = heap.Dequeue();
        tail.next = smallest;
        tail = tail.next;
        if (smallest.next != null)
            heap.Enqueue(smallest.next, smallest.next.val); // push the next from that list
    }
    return dummy.next;
}
```

> **Core idea:** the heap always holds one "frontier" element from each of the k sources. Pop the global minimum, then refill from whichever source it came from.

### Example problems
- **23. Merge k Sorted Lists** (Hard)
- **378. Kth Smallest Element in a Sorted Matrix** (Medium)
- **632. Smallest Range Covering Elements from K Lists** (Hard)

---

## 14. Topological Sort

### When to recognize it
- **Ordering** with **dependencies / prerequisites** (DAG).
- Detecting whether a valid ordering exists (i.e., no cycle).
- Keywords: "prerequisites," "build order," "dependencies," "course schedule," "can you finish."

**Time:** O(V + E). **Space:** O(V + E).

### Template (Kahn's algorithm — BFS on in-degrees)

```csharp
// Course Schedule II: return a valid order, or empty if a cycle exists
public int[] FindOrder(int numCourses, int[][] prerequisites) {
    var graph = new List<int>[numCourses];
    var indegree = new int[numCourses];
    for (int i = 0; i < numCourses; i++) graph[i] = new List<int>();
    foreach (var p in prerequisites) {        // p[1] -> p[0]
        graph[p[1]].Add(p[0]);
        indegree[p[0]]++;
    }

    var queue = new Queue<int>();
    for (int i = 0; i < numCourses; i++)
        if (indegree[i] == 0) queue.Enqueue(i);  // start with no-dependency nodes

    var order = new List<int>();
    while (queue.Count > 0) {
        int node = queue.Dequeue();
        order.Add(node);
        foreach (int next in graph[node])
            if (--indegree[next] == 0) queue.Enqueue(next);  // freed a dependency
    }
    return order.Count == numCourses ? order.ToArray() : Array.Empty<int>(); // cycle?
}
```

### Example problems
- **207. Course Schedule** (Medium)
- **210. Course Schedule II** (Medium)
- **269. Alien Dictionary** (Hard)

---

## 15. Dynamic Programming

DP is the largest pattern family. The trigger is always: **optimal substructure** (the answer is built from answers to subproblems) plus **overlapping subproblems** (the same subproblem recurs). Keywords: "minimum/maximum cost," "number of ways," "longest/shortest," "can you reach," "optimal."

> **Senior strategy:** define the state precisely (`dp[i]` = "the best answer considering the first i items"), write the recurrence, then code it — top-down memoized first (easier to reason about), bottom-up if you need to optimize space.

### 15a. 0/1 Knapsack (each item used at most once)

**When:** choose a subset under a capacity/budget to maximize value. **Time:** O(n·W). **Space:** O(W) optimized.

```csharp
// Maximize value within capacity; each item taken 0 or 1 times
public int Knapsack(int[] weights, int[] values, int capacity) {
    var dp = new int[capacity + 1];        // dp[w] = best value for capacity w
    for (int i = 0; i < weights.Length; i++)
        for (int w = capacity; w >= weights[i]; w--)   // iterate w DOWNWARD for 0/1
            dp[w] = Math.Max(dp[w], values[i] + dp[w - weights[i]]);
    return dp[capacity];
}
```

**Family:** Partition Equal Subset Sum (416), Target Sum (494), Last Stone Weight II (1049).

### 15b. Unbounded Knapsack (each item reused freely)

**When:** unlimited copies of each item (coins, cutting). The only difference from 0/1: iterate capacity **upward** so an item can be reused.

```csharp
// Coin Change: fewest coins to make `amount` (coins reusable)
public int CoinChange(int[] coins, int amount) {
    var dp = new int[amount + 1];
    Array.Fill(dp, amount + 1);            // "infinity" sentinel
    dp[0] = 0;
    foreach (int coin in coins)
        for (int a = coin; a <= amount; a++)         // iterate a UPWARD for unbounded
            dp[a] = Math.Min(dp[a], dp[a - coin] + 1);
    return dp[amount] > amount ? -1 : dp[amount];
}
```

**Family:** Coin Change II (518, count ways), Combination Sum IV (377), Word Break (139).

### 15c. LCS / LIS (sequence DP)

**When:** comparing/aligning two sequences, or finding an increasing subsequence.

```csharp
// Longest Common Subsequence — 2-D DP
public int LongestCommonSubsequence(string a, string b) {
    int m = a.Length, n = b.Length;
    var dp = new int[m + 1, n + 1];
    for (int i = 1; i <= m; i++)
        for (int j = 1; j <= n; j++)
            dp[i, j] = a[i - 1] == b[j - 1]
                ? dp[i - 1, j - 1] + 1                 // match → extend diagonal
                : Math.Max(dp[i - 1, j], dp[i, j - 1]); // skip one char
    return dp[m, n];
}
```

```csharp
// Longest Increasing Subsequence — O(n log n) with patience sorting
public int LengthOfLIS(int[] nums) {
    var tails = new List<int>();           // tails[i] = smallest tail of an LIS of length i+1
    foreach (int x in nums) {
        int lo = 0, hi = tails.Count;
        while (lo < hi) {                  // binary search for first tail >= x
            int mid = (lo + hi) / 2;
            if (tails[mid] < x) lo = mid + 1; else hi = mid;
        }
        if (lo == tails.Count) tails.Add(x);
        else tails[lo] = x;
    }
    return tails.Count;
}
```

**Family:** Edit Distance (72), Longest Palindromic Subsequence (516), Russian Doll Envelopes (354).

### Example problems (DP overall)
- **322. Coin Change** (Medium) — unbounded knapsack
- **1143. Longest Common Subsequence** (Medium)
- **300. Longest Increasing Subsequence** (Medium)

---

## 16. Trie

### When to recognize it
- Many operations involving **string prefixes** — autocomplete, dictionary lookup, prefix matching.
- Searching a board/grid for **multiple words** at once (Word Search II).
- Keywords: "prefix," "starts with," "dictionary," "autocomplete," "search words."

**Time:** O(L) per insert/search where L = word length. **Space:** O(total characters).

### Template

```csharp
public class TrieNode {
    public TrieNode[] Children = new TrieNode[26];
    public bool IsWord;
}

public class Trie {
    private readonly TrieNode _root = new();

    public void Insert(string word) {
        var node = _root;
        foreach (char c in word) {
            int i = c - 'a';
            node.Children[i] ??= new TrieNode();   // create branch if missing
            node = node.Children[i];
        }
        node.IsWord = true;                        // mark end of a full word
    }

    public bool Search(string word) => Find(word)?.IsWord == true;
    public bool StartsWith(string prefix) => Find(prefix) != null;

    private TrieNode Find(string s) {
        var node = _root;
        foreach (char c in s) {
            node = node.Children[c - 'a'];
            if (node == null) return null;
        }
        return node;
    }
}
```

### Example problems
- **208. Implement Trie (Prefix Tree)** (Medium)
- **211. Design Add and Search Words Data Structure** (Medium) — wildcard `.`
- **212. Word Search II** (Hard) — Trie + DFS on a grid

---

## 17. Union-Find (Disjoint Set)

### When to recognize it
- **Connectivity** queries: are two elements in the same group? Count groups.
- **Dynamically** merging sets (edges added over time).
- Cycle detection in an **undirected** graph.
- Keywords: "connected," "groups," "components," "redundant connection," "merge accounts."

**Time:** ~O(α(n)) ≈ O(1) per op with path compression + union by rank. **Space:** O(n).

### Template

```csharp
public class UnionFind {
    private readonly int[] _parent;
    private readonly int[] _rank;
    public int Count { get; private set; }

    public UnionFind(int n) {
        _parent = new int[n];
        _rank = new int[n];
        Count = n;
        for (int i = 0; i < n; i++) _parent[i] = i;   // each node is its own root
    }

    public int Find(int x) {
        if (_parent[x] != x)
            _parent[x] = Find(_parent[x]);            // path compression
        return _parent[x];
    }

    public bool Union(int a, int b) {
        int ra = Find(a), rb = Find(b);
        if (ra == rb) return false;                   // already connected (cycle if undirected)
        if (_rank[ra] < _rank[rb]) (ra, rb) = (rb, ra); // union by rank
        _parent[rb] = ra;
        if (_rank[ra] == _rank[rb]) _rank[ra]++;
        Count--;                                      // two groups became one
        return true;
    }
}
```

### Example problems
- **323. Number of Connected Components in an Undirected Graph** (Medium)
- **684. Redundant Connection** (Medium) — first edge that forms a cycle
- **721. Accounts Merge** (Medium)

---

## Quick Pattern-Recognition Cheat Sheet

Train yourself to map a phrase in the problem to a pattern in seconds.

| If the problem says... | Reach for... |
|------------------------|--------------|
| "sorted array" + "find a pair/triplet" | Two Pointers |
| "longest/shortest contiguous subarray/substring" | Sliding Window |
| "linked list" + "cycle" or "middle" | Fast & Slow Pointers |
| "intervals" / "overlapping" / "meeting rooms" | Merge Intervals |
| "numbers 1..n" + "find missing/duplicate" | Cyclic Sort |
| "reverse a linked list (segment / k-group)" | In-place LL Reversal |
| "shortest path / fewest steps" (unweighted) | BFS |
| "level by level" tree traversal | BFS |
| "all paths / connected components / islands" | DFS |
| "median of a stream" / "balance two halves" | Two Heaps |
| "all combinations / permutations / subsets" | Backtracking |
| "sorted or rotated" + O(log n), or "min value such that..." | Modified Binary Search |
| "top K / K most frequent / Kth largest" | Top-K Heap |
| "merge K sorted lists/arrays" | K-way Merge |
| "prerequisites / build order / dependencies" | Topological Sort |
| "min/max cost," "number of ways," "can you reach" | Dynamic Programming |
| "prefix / starts-with / dictionary" | Trie |
| "groups / connected / merge sets" | Union-Find |

---

## Summary

| Skill | Senior-level expectation |
|-------|--------------------------|
| Recognize the pattern from the prompt | Within 2–3 minutes of reading |
| Recall the template | Write the skeleton from memory |
| Adapt, not memorize | Bend the template to the specific twist |
| Justify the choice aloud | "This is a sliding-window problem because..." |
| Know the complexity instantly | State time & space without prompting |

Master these ~18 patterns and the ~150 problems in [04-practice-strategy.md](04-practice-strategy.md), and the overwhelming majority of FAANG coding rounds become exercises in recognition rather than invention.

*"Amateurs memorize solutions. Professionals recognize patterns."*
