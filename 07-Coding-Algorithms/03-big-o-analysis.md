# Big-O & Complexity Analysis

> **Complexity analysis is non-negotiable in FAANG interviews.** Even a correct solution loses points if you cannot state and justify its time and space complexity. At senior level, interviewers expect you to analyze complexity *instantly* and to reason about trade-offs out loud.

---

## Table of Contents

1. [Asymptotic Notation (O, Θ, Ω)](#1-asymptotic-notation-o-θ-ω)
2. [Analyzing Loops](#2-analyzing-loops)
3. [Analyzing Recursion & Divide-and-Conquer](#3-analyzing-recursion--divide-and-conquer)
4. [Recurrence Relations & the Master Theorem](#4-recurrence-relations--the-master-theorem)
5. [Amortized Analysis](#5-amortized-analysis)
6. [Space Complexity (incl. Recursion Stack)](#6-space-complexity-incl-recursion-stack)
7. [Complexity of Common Data-Structure Operations](#7-complexity-of-common-data-structure-operations)
8. [How to State Complexity in Interviews](#8-how-to-state-complexity-in-interviews)
9. [Common Mistakes](#9-common-mistakes)

---

## 1. Asymptotic Notation (O, Θ, Ω)

Asymptotic notation describes how an algorithm's cost grows as the input size `n` grows toward infinity. We drop constants and lower-order terms because they're irrelevant at scale.

| Notation | Name | Meaning | Informal |
|----------|------|---------|----------|
| **O(f(n))** | Big-O | **Upper bound** | "grows no faster than" |
| **Ω(f(n))** | Big-Omega | **Lower bound** | "grows no slower than" |
| **Θ(f(n))** | Big-Theta | **Tight bound** (both O and Ω) | "grows exactly like" |
| **o(f(n))** | little-o | Strict upper bound | "grows strictly slower than" |
| **ω(f(n))** | little-omega | Strict lower bound | "grows strictly faster than" |

### What interviewers actually want

In practice, when an interviewer asks "what's the complexity?" they almost always mean the **worst-case Big-O**. Technically many of our "O" statements are really Θ (tight), but the convention is to say "O".

- **Worst case** (most common): the maximum cost over all inputs of size `n`.
- **Average case**: expected cost over a distribution of inputs (e.g., quicksort is Θ(n log n) average, O(n²) worst).
- **Best case**: rarely useful; cite it only to contrast (e.g., "best case O(n) if already sorted").

### The growth hierarchy (memorize this)

```
O(1) < O(log n) < O(√n) < O(n) < O(n log n) < O(n²) < O(n³) < O(2ⁿ) < O(n!) < O(nⁿ)
```

| Complexity | Name | n=10 | n=1,000 | n=1,000,000 | Typical source |
|------------|------|------|---------|-------------|----------------|
| O(1) | Constant | 1 | 1 | 1 | Hash lookup, array index |
| O(log n) | Logarithmic | ~3 | ~10 | ~20 | Binary search, balanced tree |
| O(n) | Linear | 10 | 1,000 | 1,000,000 | Single pass |
| O(n log n) | Linearithmic | ~33 | ~10,000 | ~20,000,000 | Efficient sorts |
| O(n²) | Quadratic | 100 | 1,000,000 | 10¹² | Nested loops |
| O(2ⁿ) | Exponential | 1,024 | astronomical | — | Naive recursion, subsets |
| O(n!) | Factorial | 3.6M | — | — | Permutations |

### Rule of thumb for interview input sizes

Use the constraint `n` to *reverse-engineer* the expected complexity:

| Input size `n` | Likely target complexity |
|----------------|--------------------------|
| n ≤ 10–12 | O(n!) or O(2ⁿ) (backtracking/brute force is fine) |
| n ≤ 20–25 | O(2ⁿ) (subsets, bitmask DP) |
| n ≤ 500 | O(n³) |
| n ≤ 5,000 | O(n²) |
| n ≤ 10⁶ | O(n) or O(n log n) |
| n ≥ 10⁸ | O(log n) or O(1) |

> **Interview tip:** If the problem says `n ≤ 10⁵`, an O(n²) solution (10¹⁰ operations) will time out. State this out loud — "n is up to 100k, so I need at least O(n log n)" — it signals seniority.

---

## 2. Analyzing Loops

### Single loop → O(n)

```csharp
// O(n): the loop body runs n times, each O(1)
public int Sum(int[] arr) {
    int total = 0;
    for (int i = 0; i < arr.Length; i++) {  // n iterations
        total += arr[i];                     // O(1)
    }
    return total;
}
```

### Nested loops → multiply

```csharp
// O(n * m). If both bound by n, it's O(n²)
public bool HasDuplicatePair(int[] arr) {
    for (int i = 0; i < arr.Length; i++) {          // n
        for (int j = i + 1; j < arr.Length; j++) {  // up to n
            if (arr[i] == arr[j]) return true;
        }
    }
    return false;
}
```

> The inner loop runs n + (n-1) + ... + 1 = n(n-1)/2 times. Drop the constant → **O(n²)**.

### Sequential loops → add (then take the max)

```csharp
// O(n) + O(m) = O(n + m). If m ≤ n, simplifies to O(n)
public void Process(int[] a, int[] b) {
    foreach (var x in a) { /* O(1) */ }  // O(n)
    foreach (var y in b) { /* O(1) */ }  // O(m)
}
```

### Loop with multiplicative index → O(log n)

```csharp
// O(log n): i doubles each iteration, so ~log₂(n) iterations
public void HalveDown(int n) {
    for (int i = 1; i < n; i *= 2) {
        // O(1) work
    }
}
```

### The classic trap: loop bound depends on a variable

```csharp
// O(n²), NOT O(n). Inner loop runs i times → total = n(n+1)/2
for (int i = 0; i < n; i++) {
    for (int j = 0; j < i; j++) { /* O(1) */ }
}
```

### Hidden cost inside the loop body

```csharp
// O(n²) — string concatenation is O(n) due to immutability!
string result = "";
for (int i = 0; i < n; i++) {
    result += chars[i];  // O(current length) each time
}
// Fix: use StringBuilder for amortized O(1) appends → O(n) total
```

---

## 3. Analyzing Recursion & Divide-and-Conquer

The cost of a recursive function = **(number of calls) × (work per call)**. Express it as a recurrence, then solve.

### Linear recursion → O(n)

```csharp
// T(n) = T(n-1) + O(1)  →  O(n)
public int Factorial(int n) {
    if (n <= 1) return 1;
    return n * Factorial(n - 1);  // one recursive call, O(1) work
}
```

### Binary recursion (naive Fibonacci) → O(2ⁿ)

```csharp
// T(n) = T(n-1) + T(n-2) + O(1)  →  O(2ⁿ) (actually O(φⁿ))
public int Fib(int n) {
    if (n <= 1) return n;
    return Fib(n - 1) + Fib(n - 2);  // two calls
}
```

> Each call branches into two; the recursion tree has ~2ⁿ nodes. Memoization collapses this to **O(n)** by caching subproblems.

### Divide-and-conquer → use the recurrence

```csharp
// Merge sort: split in half, recurse twice, merge in O(n)
// T(n) = 2·T(n/2) + O(n)  →  O(n log n)
public void MergeSort(int[] a, int lo, int hi) {
    if (lo >= hi) return;
    int mid = lo + (hi - lo) / 2;
    MergeSort(a, lo, mid);       // T(n/2)
    MergeSort(a, mid + 1, hi);   // T(n/2)
    Merge(a, lo, mid, hi);       // O(n)
}
```

### Counting subsets / permutations

```csharp
// Generate all subsets: T(n) = 2·T(n-1) + O(1), and we copy each subset
// 2ⁿ subsets; total work O(n · 2ⁿ) when including the copy cost
public void Subsets(int[] nums, int i, List<int> path, IList<IList<int>> res) {
    if (i == nums.Length) { res.Add(new List<int>(path)); return; }
    Subsets(nums, i + 1, path, res);           // exclude
    path.Add(nums[i]);
    Subsets(nums, i + 1, path, res);           // include
    path.RemoveAt(path.Count - 1);
}
```

---

## 4. Recurrence Relations & the Master Theorem

For divide-and-conquer recurrences of the form:

```
T(n) = a · T(n / b) + f(n)
```

where `a` = number of subproblems, `n/b` = subproblem size, `f(n)` = work to split and combine.

Compare `f(n)` against `n^(log_b a)`:

| Case | Condition | Result |
|------|-----------|--------|
| **1** | f(n) grows slower than n^(log_b a) | T(n) = Θ(n^(log_b a)) |
| **2** | f(n) ≈ n^(log_b a) | T(n) = Θ(n^(log_b a) · log n) |
| **3** | f(n) grows faster than n^(log_b a) | T(n) = Θ(f(n)) |

### Practical examples (the ones that actually show up)

| Recurrence | a | b | n^(log_b a) | f(n) | Result | Example |
|------------|---|---|-------------|------|--------|---------|
| T(n)=2T(n/2)+O(n) | 2 | 2 | n | n | **O(n log n)** | Merge sort |
| T(n)=2T(n/2)+O(1) | 2 | 2 | n | 1 | **O(n)** | Tree traversal |
| T(n)=T(n/2)+O(1) | 1 | 2 | 1 | 1 | **O(log n)** | Binary search |
| T(n)=T(n/2)+O(n) | 1 | 2 | 1 | n | **O(n)** | Quickselect (avg) |
| T(n)=2T(n/2)+O(n²) | 2 | 2 | n | n² | **O(n²)** | dominated by combine |
| T(n)=4T(n/2)+O(n) | 4 | 2 | n² | n | **O(n²)** | naive matrix-ish |

> **Interview tip:** You rarely need to *invoke* the Master Theorem by name. Just be able to say "I split into two halves and do linear merge work, that's the merge-sort recurrence, so O(n log n)." Recognizing the *shape* is what matters.

### When the Master Theorem doesn't apply

Subtract-and-conquer recurrences (`T(n) = T(n-1) + f(n)`) aren't covered by the Master Theorem. Solve by expansion:
- `T(n) = T(n-1) + O(1)` → O(n)
- `T(n) = T(n-1) + O(n)` → O(n²)
- `T(n) = 2T(n-1) + O(1)` → O(2ⁿ)

---

## 5. Amortized Analysis

Amortized analysis gives the **average cost per operation over a sequence**, even when individual operations occasionally spike. It's not the same as average-case — it's a guarantee over the worst-case sequence.

### Classic example: dynamic array (List<T>) append

```csharp
var list = new List<int>();
for (int i = 0; i < n; i++) {
    list.Add(i);  // Usually O(1). When full, resize = O(current size)
}
```

- Most `Add` calls are O(1).
- Occasionally the backing array is full → allocate a 2× array and copy everything (O(n) for that one call).
- But resizes happen at sizes 1, 2, 4, 8, ... The total copy cost across all n appends is `1 + 2 + 4 + ... + n ≈ 2n = O(n)`.
- **Amortized cost per Add = O(n) / n = O(1).**

### Other amortized-O(1) operations to know

| Structure / operation | Amortized | Worst case (single op) |
|------------------------|-----------|------------------------|
| `List<T>.Add` (append) | O(1) | O(n) on resize |
| `Dictionary<K,V>` insert | O(1) | O(n) on rehash / collisions |
| `StringBuilder.Append` | O(1) | O(n) on internal grow |
| Monotonic stack push/pop in a single pass | O(1) | each element pushed/popped once → O(n) total |
| Two-pointer / sliding window pass | O(1) per step | window expands & contracts ≤ 2n moves total |

> **Interview tip:** When asked "isn't the inner while-loop O(n), making it O(n²)?" for a monotonic stack or sliding window — answer: "Each element is pushed and popped at most once across the *entire* run, so the total is O(n) amortized, not per-iteration." This is a frequent senior-level gotcha.

---

## 6. Space Complexity (incl. Recursion Stack)

Space complexity counts **extra** memory allocated beyond the input (unless the problem counts the output, which you should clarify).

### What counts

- Auxiliary data structures (arrays, hash maps, heaps you allocate).
- The **recursion call stack** — one frame per active call.
- Output is usually *excluded* unless asked (e.g., "return all subsets" inherently needs O(2ⁿ) output space).

### Recursion stack depth = space

```csharp
// Space: O(n) — the call stack reaches depth n before unwinding
public int SumTo(int n) {
    if (n == 0) return 0;
    return n + SumTo(n - 1);  // n nested frames
}
```

```csharp
// Tree DFS recursion: O(h) space where h = tree height
// Balanced tree → O(log n); skewed/linked-list tree → O(n)
public int MaxDepth(TreeNode root) {
    if (root == null) return 0;
    return 1 + Math.Max(MaxDepth(root.left), MaxDepth(root.right));
}
```

> **Common mistake:** Calling a recursive solution "O(1) space" because it allocates no data structures. The call stack is real memory — a recursion of depth n is **O(n) space**.

### In-place vs. extra space

| Approach | Space | Note |
|----------|-------|------|
| Reverse array with two pointers | O(1) | swaps in place |
| Reverse linked list iteratively | O(1) | pointer rewiring |
| Reverse linked list recursively | O(n) | stack depth = list length |
| Merge sort | O(n) | needs a merge buffer |
| Quick sort | O(log n) | recursion stack only (in-place partition) |
| BFS | O(width) | queue can hold a full level |
| DFS (recursive) | O(height) | call stack |

### Iterative vs. recursive trade-off

Converting recursion to an explicit stack moves the cost from the call stack to the heap but the asymptotic space is the same. The reason to do it in production (and to mention in interviews) is to **avoid stack overflow** on deep inputs — a real-world senior consideration.

---

## 7. Complexity of Common Data-Structure Operations

### C# collections cheat sheet

| Structure (C# type) | Access | Search | Insert | Delete | Notes |
|---------------------|--------|--------|--------|--------|-------|
| `T[]` (array) | O(1) | O(n) | — | — | fixed size |
| `List<T>` | O(1) | O(n) | O(1)* end / O(n) middle | O(n) | *amortized |
| `Dictionary<K,V>` | — | O(1)* | O(1)* | O(1)* | *avg; O(n) worst |
| `HashSet<T>` | — | O(1)* | O(1)* | O(1)* | *avg |
| `SortedDictionary<K,V>` | — | O(log n) | O(log n) | O(log n) | red-black tree |
| `SortedSet<T>` | — | O(log n) | O(log n) | O(log n) | ordered |
| `Stack<T>` | O(1) top | — | O(1) push | O(1) pop | LIFO |
| `Queue<T>` | O(1) front | — | O(1) enqueue | O(1) dequeue | FIFO |
| `LinkedList<T>` | O(n) | O(n) | O(1) at node | O(1) at node | needs node ref |
| `PriorityQueue<T,P>` | O(1) peek | — | O(log n) enqueue | O(log n) dequeue | binary heap |

### Algorithm-level complexity

| Operation | Average | Worst | Space |
|-----------|---------|-------|-------|
| Binary search (sorted array) | O(log n) | O(log n) | O(1) |
| Quick sort | O(n log n) | O(n²) | O(log n) |
| Merge sort | O(n log n) | O(n log n) | O(n) |
| Heap sort | O(n log n) | O(n log n) | O(1) |
| Build heap (heapify) | O(n) | O(n) | O(1) |
| BFS / DFS on graph | O(V + E) | O(V + E) | O(V) |
| Dijkstra (binary heap) | O((V+E) log V) | O((V+E) log V) | O(V) |
| Topological sort | O(V + E) | O(V + E) | O(V) |

---

## 8. How to State Complexity in Interviews

A clean, senior-level complexity statement has three parts:

1. **State both** time and space — interviewers often forget to ask for space; volunteering it is a signal.
2. **Define your variables** — "n is the number of nodes, e is the number of edges."
3. **Justify briefly** — point to the dominant operation.

### Template phrasing

> "This runs in **O(n log n)** time — I sort the array, which dominates the linear scan afterward — and **O(n)** space for the hash map. If the input were already sorted I could drop the sort and get O(n) time."

### A worked example narration

```csharp
public int[] TwoSum(int[] nums, int target) {
    var seen = new Dictionary<int, int>();   // O(n) space
    for (int i = 0; i < nums.Length; i++) {  // single pass: O(n) time
        int need = target - nums[i];
        if (seen.TryGetValue(need, out int j)) return new[] { j, i };
        seen[nums[i]] = i;
    }
    return Array.Empty<int>();
}
```

> "One pass over n elements, each doing an O(1) average hash lookup, so **O(n) time**. The dictionary can hold up to n entries, so **O(n) space**. The brute-force alternative is O(n²) time but O(1) space — this trades space for time."

### Always discuss the trade-off

At senior level, the interviewer wants to hear you reason about **why** you picked this point on the time/space curve, and what the alternative would cost. "I could do this in O(1) space with a nested loop, but that's O(n²) time; with n up to 10⁵ I'll spend O(n) memory to stay linear."

---

## 9. Common Mistakes

❌ **Saying "O(1) space" for a recursive solution.** The call stack is O(depth) space.

❌ **Forgetting hidden costs.** `string += ` in a loop is O(n²). `List.Contains` inside a loop is O(n²). `queue.Dequeue` on a List via `RemoveAt(0)` is O(n).

❌ **Confusing amortized with worst-case.** A single `List.Add` *can* be O(n); the *amortized* cost is O(1). Be precise about which you mean.

❌ **Multiplying when you should add (or vice-versa).** Sequential loops add; nested loops multiply.

❌ **Ignoring the dominant term wrong.** O(n + n²) is O(n²), not O(n² + n). Drop lower-order terms and constants — but keep the *largest*.

❌ **Treating `log` base as significant.** O(log₂ n) = O(log₁₀ n) = O(log n). Bases differ by a constant factor only.

❌ **Counting input in space when the problem doesn't.** Clarify whether output counts. "Return all permutations" is unavoidably O(n!) output — don't apologize for it, but separate it from your *auxiliary* space.

❌ **Hand-waving graph complexity as O(n²).** For a graph, state it in terms of V and E: O(V + E) for traversal. Only use O(V²) for an adjacency-matrix representation or a dense graph.

❌ **Forgetting that `Dictionary`/`HashSet` are O(1) *average*, not guaranteed.** Mention the O(n) worst case if the interviewer probes (adversarial hash collisions).

---

## Summary

| Skill | Why it matters at senior level |
|-------|-------------------------------|
| Reverse-engineer target complexity from `n` constraints | Shows you design before coding |
| Distinguish worst / average / amortized | Precision separates senior from junior |
| Account for recursion-stack space | Most-missed space detail |
| State complexity unprompted, with justification | Demonstrates communication & rigor |
| Reason about time/space trade-offs aloud | Core senior signal |

**Next:** [04-practice-strategy.md](04-practice-strategy.md) — a structured, week-by-week plan to build this fluency.

*"You don't truly understand a solution until you can state its complexity and defend it."*
