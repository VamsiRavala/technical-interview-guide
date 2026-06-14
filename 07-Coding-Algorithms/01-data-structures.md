# 📚 Core Data Structures Deep Dive

> **Mastering data structures is the foundation of algorithm problem-solving.** Understanding when and how to use each data structure is critical for efficient solutions.

---

## 📋 Table of Contents

1. [Arrays & Strings](#1-arrays--strings)
2. [HashMaps & HashSets](#2-hashmaps--hashsets)
3. [Linked Lists](#3-linked-lists)
4. [Trees](#4-trees)
5. [Graphs](#5-graphs)
6. [Heaps](#6-heaps)
7. [Stacks & Queues](#7-stacks--queues)

---

## 1. Arrays & Strings

### Overview
Arrays and strings are the most fundamental data structures. They appear in 40-50% of coding interviews.

### Key Concepts

#### **Array Characteristics**
- **Contiguous memory allocation**
- **O(1) access time** by index
- **O(n) search time** (unsorted)
- **Fixed size** (in most languages) or dynamic (like C# List, JavaScript Array)

#### **String Characteristics**
- **Immutable** in many languages (C#, Java)
- **Array of characters** under the hood
- **O(n) concatenation cost** due to immutability

---

### Common Operations & Complexity

| Operation | Array | String |
|-----------|-------|--------|
| Access by index | O(1) | O(1) |
| Search | O(n) | O(n) |
| Insert at end | O(1) amortized | O(n) |
| Insert at position | O(n) | O(n) |
| Delete | O(n) | O(n) |
| Concatenation | O(1) | O(n) |

---

### Common Patterns & Tricks

#### **1. Two Pointers Pattern**
Used for: Finding pairs, reversing, partitioning

```csharp
// C# Example: Remove duplicates from sorted array
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

```javascript
// JavaScript Example: Reverse string
function reverseString(s) {
    let left = 0, right = s.length - 1;
    while (left < right) {
        [s[left], s[right]] = [s[right], s[left]]; // Swap
        left++;
        right--;
    }
    return s;
}
```

#### **2. Sliding Window Pattern**
Used for: Subarray/substring problems

```csharp
// C# Example: Maximum sum of k consecutive elements
public int MaxSumSubarray(int[] arr, int k) {
    int maxSum = 0, windowSum = 0;
    
    // Initial window
    for (int i = 0; i < k; i++) {
        windowSum += arr[i];
    }
    maxSum = windowSum;
    
    // Slide window
    for (int i = k; i < arr.Length; i++) {
        windowSum += arr[i] - arr[i - k];
        maxSum = Math.Max(maxSum, windowSum);
    }
    return maxSum;
}
```

#### **3. Prefix Sum Pattern**
Used for: Range queries, subarray sums

```javascript
// JavaScript Example: Prefix sum array
function buildPrefixSum(arr) {
    const prefix = [arr[0]];
    for (let i = 1; i < arr.length; i++) {
        prefix[i] = prefix[i - 1] + arr[i];
    }
    return prefix;
}

// Get sum of elements from index i to j: O(1)
function rangeSum(prefix, i, j) {
    return i === 0 ? prefix[j] : prefix[j] - prefix[i - 1];
}
```

#### **4. In-Place Manipulation**
Used for: Space optimization

```csharp
// C# Example: Rotate array right by k steps
public void Rotate(int[] nums, int k) {
    k %= nums.Length;
    Reverse(nums, 0, nums.Length - 1);
    Reverse(nums, 0, k - 1);
    Reverse(nums, k, nums.Length - 1);
}

private void Reverse(int[] nums, int start, int end) {
    while (start < end) {
        int temp = nums[start];
        nums[start] = nums[end];
        nums[end] = temp;
        start++;
        end--;
    }
}
```

---

### Time/Space Complexity Analysis

**Key Insights:**
- **Access**: Always O(1) with index
- **Search**: O(n) unsorted, O(log n) sorted with binary search
- **Sorting**: O(n log n) with efficient algorithms
- **Space**: O(1) for in-place operations, O(n) for creating new arrays

**Common Pitfalls:**
- String concatenation in loops: O(n²) due to immutability
- Use StringBuilder in C# or array join in JavaScript
- Not considering edge cases (empty array, single element)

---

### Interview Tips

✅ **Always clarify:**
- Can array be empty?
- Can it have duplicates?
- Is it sorted?
- Can I modify in-place?

✅ **Common patterns to recognize:**
- Two pointers for sorted arrays
- Sliding window for subarrays
- Hash map for frequency counting
- Binary search for sorted arrays

---

## 2. HashMaps & HashSets

### Overview
Hash-based structures provide O(1) average-case lookups, making them essential for optimization.

### Key Concepts

#### **HashMap (Dictionary in C#)**
- **Key-value pairs**
- **O(1) average insertion, deletion, lookup**
- **Unordered** (insertion order not guaranteed)
- **Use for**: Frequency counting, caching, lookups

#### **HashSet**
- **Unique elements only**
- **O(1) average insertion, deletion, lookup**
- **Use for**: Deduplication, membership checks

---

### Common Operations & Complexity

| Operation | HashMap | HashSet |
|-----------|---------|---------|
| Insert | O(1) avg | O(1) avg |
| Delete | O(1) avg | O(1) avg |
| Lookup | O(1) avg | O(1) avg |
| Space | O(n) | O(n) |

**Worst case**: O(n) if many collisions (rare with good hash function)

---

### Common Patterns

#### **1. Frequency Counting**

```csharp
// C# Example: Find first non-repeating character
public char FirstUniqChar(string s) {
    var freq = new Dictionary<char, int>();
    
    // Count frequencies
    foreach (char c in s) {
        freq[c] = freq.GetValueOrDefault(c, 0) + 1;
    }
    
    // Find first with frequency 1
    foreach (char c in s) {
        if (freq[c] == 1) return c;
    }
    return '\0';
}
```

```javascript
// JavaScript Example: Using Map
function firstUniqChar(s) {
    const freq = new Map();
    
    for (let c of s) {
        freq.set(c, (freq.get(c) || 0) + 1);
    }
    
    for (let c of s) {
        if (freq.get(c) === 1) return c;
    }
    return null;
}
```

#### **2. Two Sum Pattern**

```csharp
// C# Example: Two Sum
public int[] TwoSum(int[] nums, int target) {
    var map = new Dictionary<int, int>();
    
    for (int i = 0; i < nums.Length; i++) {
        int complement = target - nums[i];
        if (map.ContainsKey(complement)) {
            return new int[] { map[complement], i };
        }
        map[nums[i]] = i;
    }
    return new int[0];
}
```

#### **3. Caching/Memoization**

```javascript
// JavaScript Example: Fibonacci with memoization
function fib(n, memo = {}) {
    if (n in memo) return memo[n];
    if (n <= 2) return 1;
    
    memo[n] = fib(n - 1, memo) + fib(n - 2, memo);
    return memo[n];
}
```

---

### Collision Handling

**Two main approaches:**

1. **Chaining**: Each bucket has a linked list
   - Simple implementation
   - Performance degrades gracefully
   
2. **Open Addressing**: Find next empty slot
   - Better cache locality
   - More complex implementation

**In interviews**: Just know that good hash functions minimize collisions. Don't need to implement custom hash tables.

---

### Real-World Use Cases

- **Caching**: Store computed results for quick retrieval
- **Database indexing**: Fast lookups by key
- **Deduplication**: Remove duplicates efficiently
- **Frequency analysis**: Count occurrences
- **Anagram grouping**: Group strings by sorted characters

---

### Interview Tips

✅ **When to use HashMap:**
- Need O(1) lookup
- Frequency counting
- Caching results
- Two Sum style problems

✅ **When to use HashSet:**
- Check for duplicates
- Membership testing
- Deduplication

❌ **Don't use when:**
- Need ordering (use TreeMap/TreeSet)
- Need range queries
- Memory is very limited

---

## 3. Linked Lists

### Overview
Linked lists are linear data structures where elements are stored in nodes with pointers to next (and sometimes previous) nodes.

### Types of Linked Lists

1. **Singly Linked List**: One pointer (next)
2. **Doubly Linked List**: Two pointers (next, prev)
3. **Circular Linked List**: Last node points to first

---

### Common Operations & Complexity

| Operation | Singly | Doubly |
|-----------|--------|--------|
| Insert at head | O(1) | O(1) |
| Insert at tail | O(n) or O(1) with tail pointer | O(1) |
| Delete node | O(n) | O(1) if have reference |
| Search | O(n) | O(n) |
| Access by index | O(n) | O(n) |

---

### Node Definition

```csharp
// C# Node Definition
public class ListNode {
    public int val;
    public ListNode next;
    public ListNode(int val = 0, ListNode next = null) {
        this.val = val;
        this.next = next;
    }
}
```

```javascript
// JavaScript Node Definition
class ListNode {
    constructor(val = 0, next = null) {
        this.val = val;
        this.next = next;
    }
}
```

---

### Common Patterns

#### **1. Fast & Slow Pointer (Floyd's Cycle Detection)**

```csharp
// C# Example: Detect cycle
public bool HasCycle(ListNode head) {
    if (head == null) return false;
    
    ListNode slow = head, fast = head;
    
    while (fast != null && fast.next != null) {
        slow = slow.next;
        fast = fast.next.next;
        
        if (slow == fast) return true;
    }
    return false;
}

// Find middle of linked list
public ListNode FindMiddle(ListNode head) {
    ListNode slow = head, fast = head;
    
    while (fast != null && fast.next != null) {
        slow = slow.next;
        fast = fast.next.next;
    }
    return slow;
}
```

#### **2. Reversal Pattern**

```javascript
// JavaScript Example: Reverse linked list
function reverseList(head) {
    let prev = null, current = head;
    
    while (current) {
        let nextTemp = current.next;
        current.next = prev;
        prev = current;
        current = nextTemp;
    }
    return prev;
}
```

#### **3. Dummy Node Pattern**

```csharp
// C# Example: Merge two sorted lists
public ListNode MergeTwoLists(ListNode l1, ListNode l2) {
    ListNode dummy = new ListNode(0);
    ListNode current = dummy;
    
    while (l1 != null && l2 != null) {
        if (l1.val < l2.val) {
            current.next = l1;
            l1 = l1.next;
        } else {
            current.next = l2;
            l2 = l2.next;
        }
        current = current.next;
    }
    
    current.next = l1 ?? l2;
    return dummy.next;
}
```

#### **4. Runner Technique**

```javascript
// JavaScript Example: Remove Nth node from end
function removeNthFromEnd(head, n) {
    const dummy = new ListNode(0, head);
    let first = dummy, second = dummy;
    
    // Move first n+1 steps ahead
    for (let i = 0; i <= n; i++) {
        first = first.next;
    }
    
    // Move both until first reaches end
    while (first) {
        first = first.next;
        second = second.next;
    }
    
    // Remove nth node
    second.next = second.next.next;
    return dummy.next;
}
```

---

### Interview Tips

✅ **Always consider:**
- Empty list (null head)
- Single node
- Two nodes
- Cycle detection needs

✅ **Common patterns:**
- Use dummy node for easier head handling
- Fast/slow pointers for finding middle, detecting cycles
- Reverse in-place for space optimization

❌ **Common mistakes:**
- Not checking for null before accessing .next
- Losing reference to head
- Off-by-one errors in loops

---

## 4. Trees

### Overview
Trees are hierarchical data structures with a root and child nodes. They're fundamental for many algorithms and appear in 30-40% of interviews.

### Types of Trees

1. **Binary Tree**: Each node has at most 2 children
2. **Binary Search Tree (BST)**: Left < Parent < Right
3. **AVL Tree**: Self-balancing BST
4. **Red-Black Tree**: Self-balancing BST with color property
5. **B-Tree**: Multi-way search tree (used in databases)
6. **Trie**: Prefix tree for strings

---

### Tree Node Definition

```csharp
// C# Tree Node
public class TreeNode {
    public int val;
    public TreeNode left;
    public TreeNode right;
    public TreeNode(int val = 0, TreeNode left = null, TreeNode right = null) {
        this.val = val;
        this.left = left;
        this.right = right;
    }
}
```

---

### Tree Traversals

#### **1. In-Order (Left, Root, Right)**
- **BST**: Produces sorted order
- **Use case**: Get elements in sorted order

```csharp
// C# Recursive In-Order
public void InOrder(TreeNode root) {
    if (root == null) return;
    InOrder(root.left);
    Console.WriteLine(root.val);
    InOrder(root.right);
}

// Iterative In-Order
public List<int> InOrderIterative(TreeNode root) {
    var result = new List<int>();
    var stack = new Stack<TreeNode>();
    TreeNode current = root;
    
    while (current != null || stack.Count > 0) {
        while (current != null) {
            stack.Push(current);
            current = current.left;
        }
        current = stack.Pop();
        result.Add(current.val);
        current = current.right;
    }
    return result;
}
```

#### **2. Pre-Order (Root, Left, Right)**
- **Use case**: Copy tree, prefix expression

```javascript
// JavaScript Recursive Pre-Order
function preOrder(root) {
    if (!root) return [];
    return [root.val, ...preOrder(root.left), ...preOrder(root.right)];
}

// Iterative Pre-Order
function preOrderIterative(root) {
    if (!root) return [];
    const result = [], stack = [root];
    
    while (stack.length > 0) {
        const node = stack.pop();
        result.push(node.val);
        if (node.right) stack.push(node.right);
        if (node.left) stack.push(node.left);
    }
    return result;
}
```

#### **3. Post-Order (Left, Right, Root)**
- **Use case**: Delete tree, postfix expression

```csharp
// C# Recursive Post-Order
public void PostOrder(TreeNode root) {
    if (root == null) return;
    PostOrder(root.left);
    PostOrder(root.right);
    Console.WriteLine(root.val);
}
```

#### **4. Level-Order (BFS)**
- **Use case**: Level by level processing

```javascript
// JavaScript Level-Order Traversal
function levelOrder(root) {
    if (!root) return [];
    
    const result = [];
    const queue = [root];
    
    while (queue.length > 0) {
        const levelSize = queue.length;
        const currentLevel = [];
        
        for (let i = 0; i < levelSize; i++) {
            const node = queue.shift();
            currentLevel.push(node.val);
            
            if (node.left) queue.push(node.left);
            if (node.right) queue.push(node.right);
        }
        result.push(currentLevel);
    }
    return result;
}
```

---

### Binary Search Tree Operations

#### **Search**

```csharp
// C# BST Search
public TreeNode Search(TreeNode root, int val) {
    if (root == null || root.val == val) return root;
    return val < root.val ? Search(root.left, val) : Search(root.right, val);
}
// Time: O(h) where h is height, O(log n) balanced, O(n) worst case
```

#### **Insert**

```javascript
// JavaScript BST Insert
function insert(root, val) {
    if (!root) return new TreeNode(val);
    
    if (val < root.val) {
        root.left = insert(root.left, val);
    } else {
        root.right = insert(root.right, val);
    }
    return root;
}
```

#### **Validate BST**

```csharp
// C# Validate BST
public bool IsValidBST(TreeNode root) {
    return Validate(root, null, null);
}

private bool Validate(TreeNode node, int? min, int? max) {
    if (node == null) return true;
    
    if ((min.HasValue && node.val <= min.Value) ||
        (max.HasValue && node.val >= max.Value)) {
        return false;
    }
    
    return Validate(node.left, min, node.val) &&
           Validate(node.right, node.val, max);
}
```

---

### Tree Properties & Patterns

#### **Height/Depth**

```javascript
// Maximum depth
function maxDepth(root) {
    if (!root) return 0;
    return 1 + Math.max(maxDepth(root.left), maxDepth(root.right));
}

// Check if balanced
function isBalanced(root) {
    function height(node) {
        if (!node) return 0;
        
        const left = height(node.left);
        if (left === -1) return -1;
        
        const right = height(node.right);
        if (right === -1) return -1;
        
        if (Math.abs(left - right) > 1) return -1;
        return 1 + Math.max(left, right);
    }
    
    return height(root) !== -1;
}
```

#### **Lowest Common Ancestor**

```csharp
// C# LCA in BST
public TreeNode LowestCommonAncestor(TreeNode root, TreeNode p, TreeNode q) {
    if (root.val > p.val && root.val > q.val) {
        return LowestCommonAncestor(root.left, p, q);
    }
    if (root.val < p.val && root.val < q.val) {
        return LowestCommonAncestor(root.right, p, q);
    }
    return root;
}
```

---

### Interview Tips

✅ **Tree traversal choices:**
- In-order for BST sorted output
- Pre-order for copying
- Post-order for deletion
- Level-order for level-wise processing

✅ **Recursion tips:**
- Base case: null node
- Recursive case: process left, right, or both
- Return value: accumulate results

✅ **Iterative approach:**
- Use stack for DFS (pre/in/post order)
- Use queue for BFS (level order)

---

## 5. Graphs

### Overview
Graphs represent relationships between objects. They're essential for many real-world problems.

### Graph Representation

#### **1. Adjacency List (Most Common)**

```csharp
// C# Adjacency List using Dictionary
Dictionary<int, List<int>> graph = new Dictionary<int, List<int>>();

// Add edge
void AddEdge(int u, int v) {
    if (!graph.ContainsKey(u)) graph[u] = new List<int>();
    if (!graph.ContainsKey(v)) graph[v] = new List<int>();
    graph[u].Add(v);
    // For undirected graph: graph[v].Add(u);
}
```

```javascript
// JavaScript Adjacency List using Map
const graph = new Map();

function addEdge(u, v) {
    if (!graph.has(u)) graph.set(u, []);
    if (!graph.has(v)) graph.set(v, []);
    graph.get(u).push(v);
    // For undirected: graph.get(v).push(u);
}
```

**Space**: O(V + E) where V = vertices, E = edges
**Best for**: Sparse graphs (E << V²)

#### **2. Adjacency Matrix**

```csharp
// C# Adjacency Matrix
int[,] graph = new int[n, n];

// Add edge
void AddEdge(int u, int v) {
    graph[u, v] = 1;
    // For undirected: graph[v, u] = 1;
}
```

**Space**: O(V²)
**Best for**: Dense graphs, quick edge lookup O(1)

---

### Graph Traversal Algorithms

#### **1. Depth-First Search (DFS)**

```csharp
// C# DFS - Recursive
HashSet<int> visited = new HashSet<int>();

void DFS(int node) {
    visited.Add(node);
    Console.WriteLine(node);
    
    foreach (var neighbor in graph[node]) {
        if (!visited.Contains(neighbor)) {
            DFS(neighbor);
        }
    }
}

// DFS - Iterative
void DFSIterative(int start) {
    var visited = new HashSet<int>();
    var stack = new Stack<int>();
    stack.Push(start);
    
    while (stack.Count > 0) {
        int node = stack.Pop();
        if (visited.Contains(node)) continue;
        
        visited.Add(node);
        Console.WriteLine(node);
        
        foreach (var neighbor in graph[node]) {
            if (!visited.Contains(neighbor)) {
                stack.Push(neighbor);
            }
        }
    }
}
```

**Time**: O(V + E)
**Space**: O(V) for visited set + recursion stack

**Use DFS for:**
- Cycle detection
- Topological sorting
- Connected components
- Path finding
- Backtracking problems

#### **2. Breadth-First Search (BFS)**

```javascript
// JavaScript BFS
function BFS(start) {
    const visited = new Set();
    const queue = [start];
    visited.add(start);
    
    while (queue.length > 0) {
        const node = queue.shift();
        console.log(node);
        
        for (const neighbor of graph.get(node)) {
            if (!visited.has(neighbor)) {
                visited.add(neighbor);
                queue.push(neighbor);
            }
        }
    }
}
```

**Time**: O(V + E)
**Space**: O(V) for visited set + queue

**Use BFS for:**
- Shortest path (unweighted)
- Level-order traversal
- Finding connected components
- Minimum spanning tree

---

### Common Graph Algorithms

#### **1. Connected Components**

```csharp
// C# Count Connected Components
public int CountComponents(int n, int[][] edges) {
    // Build adjacency list
    var graph = new Dictionary<int, List<int>>();
    for (int i = 0; i < n; i++) {
        graph[i] = new List<int>();
    }
    foreach (var edge in edges) {
        graph[edge[0]].Add(edge[1]);
        graph[edge[1]].Add(edge[0]);
    }
    
    var visited = new HashSet<int>();
    int components = 0;
    
    for (int i = 0; i < n; i++) {
        if (!visited.Contains(i)) {
            DFS(i, graph, visited);
            components++;
        }
    }
    return components;
}

void DFS(int node, Dictionary<int, List<int>> graph, HashSet<int> visited) {
    visited.Add(node);
    foreach (var neighbor in graph[node]) {
        if (!visited.Contains(neighbor)) {
            DFS(neighbor, graph, visited);
        }
    }
}
```

#### **2. Cycle Detection**

```javascript
// JavaScript Cycle Detection in Undirected Graph
function hasCycle(n, edges) {
    const graph = buildGraph(n, edges);
    const visited = new Set();
    
    function dfs(node, parent) {
        visited.add(node);
        
        for (const neighbor of graph.get(node)) {
            if (!visited.has(neighbor)) {
                if (dfs(neighbor, node)) return true;
            } else if (neighbor !== parent) {
                return true; // Back edge found
            }
        }
        return false;
    }
    
    for (let i = 0; i < n; i++) {
        if (!visited.has(i)) {
            if (dfs(i, -1)) return true;
        }
    }
    return false;
}
```

#### **3. Shortest Path (Dijkstra's Algorithm)**

```csharp
// C# Dijkstra's Shortest Path
public int[] Dijkstra(Dictionary<int, List<(int node, int weight)>> graph, int start, int n) {
    var dist = new int[n];
    Array.Fill(dist, int.MaxValue);
    dist[start] = 0;
    
    var pq = new PriorityQueue<int, int>();
    pq.Enqueue(start, 0);
    
    while (pq.Count > 0) {
        var current = pq.Dequeue();
        
        if (!graph.ContainsKey(current)) continue;
        
        foreach (var (neighbor, weight) in graph[current]) {
            int newDist = dist[current] + weight;
            if (newDist < dist[neighbor]) {
                dist[neighbor] = newDist;
                pq.Enqueue(neighbor, newDist);
            }
        }
    }
    return dist;
}
```

**Time**: O((V + E) log V) with binary heap
**Use for**: Weighted graphs, shortest path from single source

---

### Interview Tips

✅ **Graph representation:**
- Use adjacency list for sparse graphs (most interview problems)
- Adjacency matrix if graph is dense or need O(1) edge lookup

✅ **DFS vs BFS:**
- DFS: Recursion/stack, better for detecting cycles, topological sort
- BFS: Queue, better for shortest path (unweighted)

✅ **Common patterns:**
- Connected components: DFS/BFS with visited set
- Cycle detection: DFS with parent tracking (undirected) or recursion stack (directed)
- Shortest path: BFS (unweighted), Dijkstra (weighted)

---

## 6. Heaps

### Overview
Heaps are complete binary trees that maintain heap property: parent ≥ children (max heap) or parent ≤ children (min heap).

### Key Concepts

**Heap Properties:**
- **Complete binary tree**: All levels full except possibly last
- **Heap property**: Max heap or min heap
- **Array representation**: Parent at i, children at 2i+1 and 2i+2

**Operations:**
- Insert: O(log n)
- Extract min/max: O(log n)
- Peek min/max: O(1)
- Heapify: O(n)

---

### Priority Queue

Most languages provide heap as priority queue:

```csharp
// C# PriorityQueue (min heap by default)
var minHeap = new PriorityQueue<int, int>();

// Add element with priority
minHeap.Enqueue(5, 5);
minHeap.Enqueue(3, 3);
minHeap.Enqueue(7, 7);

// Get minimum
int min = minHeap.Dequeue(); // 3

// Max heap: negate values or use custom comparer
var maxHeap = new PriorityQueue<int, int>(Comparer<int>.Create((a, b) => b.CompareTo(a)));
```

```javascript
// JavaScript: No built-in heap, use library or implement
class MinHeap {
    constructor() {
        this.heap = [];
    }
    
    insert(val) {
        this.heap.push(val);
        this.bubbleUp(this.heap.length - 1);
    }
    
    extractMin() {
        if (this.heap.length === 0) return null;
        if (this.heap.length === 1) return this.heap.pop();
        
        const min = this.heap[0];
        this.heap[0] = this.heap.pop();
        this.bubbleDown(0);
        return min;
    }
    
    bubbleUp(index) {
        while (index > 0) {
            const parentIndex = Math.floor((index - 1) / 2);
            if (this.heap[index] >= this.heap[parentIndex]) break;
            [this.heap[index], this.heap[parentIndex]] = [this.heap[parentIndex], this.heap[index]];
            index = parentIndex;
        }
    }
    
    bubbleDown(index) {
        while (true) {
            let smallest = index;
            const left = 2 * index + 1;
            const right = 2 * index + 2;
            
            if (left < this.heap.length && this.heap[left] < this.heap[smallest]) {
                smallest = left;
            }
            if (right < this.heap.length && this.heap[right] < this.heap[smallest]) {
                smallest = right;
            }
            if (smallest === index) break;
            
            [this.heap[index], this.heap[smallest]] = [this.heap[smallest], this.heap[index]];
            index = smallest;
        }
    }
}
```

---

### Common Heap Problems

#### **1. Top K Elements**

```csharp
// C# Find K largest elements
public int[] TopKElements(int[] nums, int k) {
    var minHeap = new PriorityQueue<int, int>();
    
    foreach (int num in nums) {
        minHeap.Enqueue(num, num);
        if (minHeap.Count > k) {
            minHeap.Dequeue();
        }
    }
    
    int[] result = new int[k];
    for (int i = 0; i < k; i++) {
        result[i] = minHeap.Dequeue();
    }
    return result;
}
```

**Time**: O(n log k)
**Space**: O(k)

#### **2. Merge K Sorted Lists**

```javascript
// JavaScript Merge K sorted lists using heap
function mergeKLists(lists) {
    const minHeap = new MinHeap();
    
    // Add first element from each list
    for (const list of lists) {
        if (list) {
            minHeap.insert({ val: list.val, node: list });
        }
    }
    
    const dummy = new ListNode(0);
    let current = dummy;
    
    while (minHeap.size() > 0) {
        const { val, node } = minHeap.extractMin();
        current.next = node;
        current = current.next;
        
        if (node.next) {
            minHeap.insert({ val: node.next.val, node: node.next });
        }
    }
    
    return dummy.next;
}
```

**Time**: O(N log k) where N = total nodes, k = number of lists

#### **3. Median from Data Stream**

```csharp
// C# Median using two heaps
public class MedianFinder {
    PriorityQueue<int, int> maxHeap; // Lower half (negated for max heap)
    PriorityQueue<int, int> minHeap; // Upper half
    
    public MedianFinder() {
        maxHeap = new PriorityQueue<int, int>(Comparer<int>.Create((a, b) => b.CompareTo(a)));
        minHeap = new PriorityQueue<int, int>();
    }
    
    public void AddNum(int num) {
        maxHeap.Enqueue(num, num);
        minHeap.Enqueue(maxHeap.Dequeue(), maxHeap.Dequeue());
        
        if (minHeap.Count > maxHeap.Count) {
            maxHeap.Enqueue(minHeap.Dequeue(), minHeap.Dequeue());
        }
    }
    
    public double FindMedian() {
        if (maxHeap.Count > minHeap.Count) {
            return maxHeap.Peek();
        }
        return (maxHeap.Peek() + minHeap.Peek()) / 2.0;
    }
}
```

---

### Interview Tips

✅ **When to use heap:**
- Top K problems
- Median/percentile tracking
- Merging sorted sequences
- Scheduling problems

✅ **Heap choice:**
- Min heap for finding K largest (keep K largest in heap)
- Max heap for finding K smallest (keep K smallest in heap)

✅ **Common pattern:**
- Maintain heap of size K for O(n log k) solution

---

## 7. Stacks & Queues

### Overview
Linear data structures with restricted access patterns.

### Stack (LIFO - Last In First Out)

**Operations:**
- Push: O(1)
- Pop: O(1)
- Peek: O(1)

```csharp
// C# Stack
Stack<int> stack = new Stack<int>();
stack.Push(1);
stack.Push(2);
int top = stack.Peek(); // 2
int popped = stack.Pop(); // 2
```

```javascript
// JavaScript Stack (using array)
const stack = [];
stack.push(1);
stack.push(2);
const top = stack[stack.length - 1]; // 2
const popped = stack.pop(); // 2
```

---

### Queue (FIFO - First In First Out)

**Operations:**
- Enqueue: O(1)
- Dequeue: O(1)
- Peek: O(1)

```csharp
// C# Queue
Queue<int> queue = new Queue<int>();
queue.Enqueue(1);
queue.Enqueue(2);
int front = queue.Peek(); // 1
int dequeued = queue.Dequeue(); // 1
```

```javascript
// JavaScript Queue (use array with caution - shift() is O(n))
const queue = [];
queue.push(1);
queue.push(2);
const front = queue[0]; // 1
const dequeued = queue.shift(); // 1 (expensive O(n))

// Better: Use linked list or circular buffer for true O(1) dequeue
```

---

### Common Stack Patterns

#### **1. Valid Parentheses**

```csharp
// C# Valid parentheses
public bool IsValid(string s) {
    var stack = new Stack<char>();
    var map = new Dictionary<char, char> {
        {')', '('}, {']', '['}, {'}', '{'}
    };
    
    foreach (char c in s) {
        if (map.ContainsKey(c)) {
            if (stack.Count == 0 || stack.Pop() != map[c]) {
                return false;
            }
        } else {
            stack.Push(c);
        }
    }
    return stack.Count == 0;
}
```

#### **2. Monotonic Stack**

```javascript
// JavaScript Next greater element
function nextGreaterElement(nums) {
    const result = new Array(nums.length).fill(-1);
    const stack = [];
    
    for (let i = 0; i < nums.length; i++) {
        while (stack.length > 0 && nums[i] > nums[stack[stack.length - 1]]) {
            const index = stack.pop();
            result[index] = nums[i];
        }
        stack.push(i);
    }
    return result;
}
```

**Monotonic stack pattern:**
- Maintain increasing/decreasing order in stack
- When violated, pop and process
- Used for "next greater/smaller element" problems

#### **3. Evaluate Expression**

```csharp
// C# Evaluate postfix expression
public int EvalRPN(string[] tokens) {
    var stack = new Stack<int>();
    
    foreach (string token in tokens) {
        if (int.TryParse(token, out int num)) {
            stack.Push(num);
        } else {
            int b = stack.Pop();
            int a = stack.Pop();
            int result = token switch {
                "+" => a + b,
                "-" => a - b,
                "*" => a * b,
                "/" => a / b,
                _ => 0
            };
            stack.Push(result);
        }
    }
    return stack.Pop();
}
```

---

### Common Queue Patterns

#### **1. BFS (Level-Order Traversal)**

```javascript
// Already covered in Trees and Graphs sections
```

#### **2. Sliding Window Maximum**

```csharp
// C# Sliding window maximum using deque
public int[] MaxSlidingWindow(int[] nums, int k) {
    var deque = new LinkedList<int>(); // Store indices
    var result = new int[nums.Length - k + 1];
    
    for (int i = 0; i < nums.Length; i++) {
        // Remove elements outside window
        while (deque.Count > 0 && deque.First.Value < i - k + 1) {
            deque.RemoveFirst();
        }
        
        // Remove smaller elements (they'll never be max)
        while (deque.Count > 0 && nums[deque.Last.Value] < nums[i]) {
            deque.RemoveLast();
        }
        
        deque.AddLast(i);
        
        // Add to result once window is full
        if (i >= k - 1) {
            result[i - k + 1] = nums[deque.First.Value];
        }
    }
    return result;
}
```

---

### Applications

**Stack Applications:**
- Function call stack
- Undo/redo functionality
- Expression evaluation
- Backtracking algorithms
- DFS traversal

**Queue Applications:**
- Task scheduling
- BFS traversal
- Print queue
- Message queues
- Level-order processing

---

### Interview Tips

✅ **Stack usage:**
- Matching/balancing problems (parentheses)
- Monotonic stack for next greater/smaller
- Expression evaluation
- DFS implementation

✅ **Queue usage:**
- BFS traversal
- Level-order processing
- Sliding window problems (deque)
- FIFO ordering requirements

✅ **Deque (double-ended queue):**
- Can act as both stack and queue
- Useful for sliding window problems
- Maintain monotonic ordering

---

## 🎯 Summary

### Data Structure Selection Guide

| Problem Type | Data Structure | Time Complexity |
|-------------|----------------|-----------------|
| Fast lookups, frequency counting | HashMap | O(1) average |
| Ordered data with range queries | TreeMap | O(log n) |
| Find min/max continuously | Heap | O(log n) |
| Most recent/LIFO | Stack | O(1) |
| FIFO, level-order | Queue | O(1) |
| Parent-child relationships | Tree | O(log n) avg |
| Connections between entities | Graph | O(V + E) |
| Sequential with pointer manipulation | Linked List | O(n) |

### Master These Patterns

1. **Two Pointers**: Arrays, linked lists, strings
2. **Fast/Slow Pointers**: Cycle detection, finding middle
3. **Sliding Window**: Substring/subarray problems
4. **Hash Table**: O(1) lookups, frequency counting
5. **Tree Traversals**: DFS (recursion/stack), BFS (queue)
6. **Graph Traversals**: DFS for cycles/components, BFS for shortest path
7. **Monotonic Stack**: Next greater/smaller element
8. **Heap**: Top K, median tracking, merge K sorted

---

**Next Steps:**
- Review [02-algorithms.md](02-algorithms.md) for algorithm-specific techniques
- Study [03-big-o-analysis.md](03-big-o-analysis.md) for complexity mastery
- Practice with [04-practice-strategy.md](04-practice-strategy.md)

*"Understanding data structures deeply is the key to solving any algorithm problem."*
