# Data Structures & Algorithms (C#)

A comprehensive C#-centric guide to three foundational data-structure topics — Arrays, Strings, and Hashing — with fundamentals, operations, common interview problems, real-world examples, best practices, pitfalls, and complexity analysis.

> Note: broader DSA and coding-pattern preparation (sorting, trees, graphs, dynamic programming, coding patterns) lives in section `07-Coding-Algorithms`.

---

## Arrays

A comprehensive guide to arrays in C#, covering fundamentals, operations, patterns, and real-world applications for technical interviews and practical development.

### Quick Overview
- **Arrays** are fixed-size, contiguous memory structures that store elements of the same type
- Arrays provide **O(1)** access time using index-based lookup
- Support for **multi-dimensional** and **jagged arrays** in C#
- Arrays are **reference types** in C# (stored on heap)
- **Zero-indexed**: first element is at index 0

### Array Fundamentals

#### Declaration and Initialization
```csharp
// Declaration with initialization
int[] numbers = new int[5];              // Array of size 5, initialized to 0
int[] primes = { 2, 3, 5, 7, 11 };       // Array literal
string[] names = new string[3];          // Array of strings, initialized to null

// Using Array.Empty for empty arrays (more efficient)
int[] empty = Array.Empty<int>();

// Creating with initial values
int[] sequence = Enumerable.Range(1, 10).ToArray(); // [1,2,3,...,10]

// Implicit typing
var scores = new[] { 95, 87, 92, 78 };   // Type inferred as int[]
```

#### Time/Space Complexity

| Operation | Time Complexity | Space Complexity | Notes |
|-----------|----------------|------------------|-------|
| Access by index | O(1) | O(1) | Direct memory access |
| Search (unsorted) | O(n) | O(1) | Linear scan required |
| Search (sorted) | O(log n) | O(1) | Binary search |
| Insert at end | O(1)* | O(n) | *If space available; else O(n) |
| Insert at index | O(n) | O(n) | Shift elements |
| Delete at index | O(n) | O(1) | Shift elements |
| Resize | O(n) | O(n) | Copy to new array |

### Basic Operations

#### 1. Traversal
```csharp
// Method 1: Traditional for loop
int[] numbers = { 10, 20, 30, 40, 50 };
for (int i = 0; i < numbers.Length; i++)
{
    Console.WriteLine($"Index {i}: {numbers[i]}");
}

// Method 2: foreach loop (read-only)
foreach (int num in numbers)
{
    Console.WriteLine(num);
}

// Method 3: Using LINQ
numbers.ToList().ForEach(n => Console.WriteLine(n));

// Method 4: Array.ForEach (functional style)
Array.ForEach(numbers, n => Console.WriteLine(n));
```

**Real-world scenario:** Processing sensor readings from IoT devices
```csharp
// Process temperature readings from multiple sensors
double[] temperatures = { 72.5, 68.3, 75.1, 71.8, 69.9 };
double average = temperatures.Average();
double max = temperatures.Max();
Console.WriteLine($"Average: {average}°F, Max: {max}°F");
```

#### 2. Insertion
```csharp
// Insertion at end (requires resizing)
public static int[] InsertAtEnd(int[] arr, int element)
{
    int[] newArr = new int[arr.Length + 1];
    Array.Copy(arr, newArr, arr.Length);
    newArr[arr.Length] = element;
    return newArr;

    // Time: O(n), Space: O(n)
}

// Insertion at specific index
public static int[] InsertAtIndex(int[] arr, int index, int element)
{
    if (index < 0 || index > arr.Length)
        throw new ArgumentOutOfRangeException(nameof(index));

    int[] newArr = new int[arr.Length + 1];

    // Copy elements before index
    Array.Copy(arr, 0, newArr, 0, index);

    // Insert new element
    newArr[index] = element;

    // Copy elements after index
    Array.Copy(arr, index, newArr, index + 1, arr.Length - index);

    return newArr;

    // Time: O(n), Space: O(n)
}

// Usage
int[] original = { 1, 2, 3, 5 };
int[] modified = InsertAtIndex(original, 3, 4); // [1, 2, 3, 4, 5]
```

**Real-world scenario:** Adding a new product to inventory
```csharp
public class InventoryManager
{
    private string[] products;

    public void AddProduct(string productName, int position)
    {
        // Insert product at specified position in inventory
        products = InsertAtIndex(products, position, productName);
    }
}
```

#### 3. Deletion
```csharp
// Delete element at specific index
public static int[] DeleteAtIndex(int[] arr, int index)
{
    if (index < 0 || index >= arr.Length)
        throw new ArgumentOutOfRangeException(nameof(index));

    int[] newArr = new int[arr.Length - 1];

    // Copy elements before index
    Array.Copy(arr, 0, newArr, 0, index);

    // Copy elements after index
    Array.Copy(arr, index + 1, newArr, index, arr.Length - index - 1);

    return newArr;

    // Time: O(n), Space: O(n)
}

// Delete by value (first occurrence)
public static int[] DeleteByValue(int[] arr, int value)
{
    int index = Array.IndexOf(arr, value);
    if (index == -1)
        return arr; // Value not found

    return DeleteAtIndex(arr, index);

    // Time: O(n), Space: O(n)
}

// Usage
int[] numbers = { 10, 20, 30, 40, 50 };
int[] afterDelete = DeleteAtIndex(numbers, 2); // [10, 20, 40, 50]
```

#### 4. Searching
```csharp
// Linear search (unsorted array)
public static int LinearSearch(int[] arr, int target)
{
    for (int i = 0; i < arr.Length; i++)
    {
        if (arr[i] == target)
            return i;
    }
    return -1; // Not found

    // Time: O(n), Space: O(1)
}

// Binary search (sorted array)
public static int BinarySearch(int[] arr, int target)
{
    int left = 0;
    int right = arr.Length - 1;

    while (left <= right)
    {
        int mid = left + (right - left) / 2; // Avoid overflow

        if (arr[mid] == target)
            return mid;
        else if (arr[mid] < target)
            left = mid + 1;
        else
            right = mid - 1;
    }

    return -1; // Not found

    // Time: O(log n), Space: O(1)
}

// Using built-in binary search
int[] sorted = { 1, 3, 5, 7, 9, 11, 13 };
int index = Array.BinarySearch(sorted, 7); // Returns index 3
```

**Real-world scenario:** Finding a user by ID in a sorted database export
```csharp
public class UserRepository
{
    private int[] userIds; // Sorted array of user IDs

    public int FindUserIndex(int userId)
    {
        return Array.BinarySearch(userIds, userId);
        // O(log n) - much faster than database query for cached data
    }
}
```

### Multi-Dimensional Arrays

#### 2D Arrays (Rectangular)
```csharp
// Declaration and initialization
int[,] matrix = new int[3, 4]; // 3 rows, 4 columns

// Initialize with values
int[,] grid =
{
    { 1, 2, 3, 4 },
    { 5, 6, 7, 8 },
    { 9, 10, 11, 12 }
};

// Accessing elements
int element = grid[1, 2]; // Row 1, Column 2 = 7

// Traversing 2D array
for (int i = 0; i < grid.GetLength(0); i++) // Rows
{
    for (int j = 0; j < grid.GetLength(1); j++) // Columns
    {
        Console.Write($"{grid[i, j]} ");
    }
    Console.WriteLine();
}

// Total elements
int totalElements = grid.Length; // 12
int rows = grid.GetLength(0);    // 3
int cols = grid.GetLength(1);    // 4
```

**Real-world scenario:** Representing a game board or seating chart
```csharp
public class ChessBoard
{
    private string[,] board = new string[8, 8];

    public void PlacePiece(int row, int col, string piece)
    {
        if (row >= 0 && row < 8 && col >= 0 && col < 8)
            board[row, col] = piece;
    }

    public void DisplayBoard()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Console.Write($"{board[i, j] ?? "."} ");
            }
            Console.WriteLine();
        }
    }
}
```

#### Jagged Arrays (Array of Arrays)
```csharp
// Declaration - each row can have different length
int[][] jaggedArray = new int[3][];

// Initialize each row separately
jaggedArray[0] = new int[] { 1, 2, 3 };
jaggedArray[1] = new int[] { 4, 5 };
jaggedArray[2] = new int[] { 6, 7, 8, 9 };

// Initialize in one statement
int[][] jagged = new int[][]
{
    new int[] { 1, 2, 3 },
    new int[] { 4, 5 },
    new int[] { 6, 7, 8, 9 }
};

// Accessing elements
int value = jagged[1][0]; // 4

// Traversing jagged array
for (int i = 0; i < jagged.Length; i++)
{
    Console.Write($"Row {i}: ");
    for (int j = 0; j < jagged[i].Length; j++)
    {
        Console.Write($"{jagged[i][j]} ");
    }
    Console.WriteLine();
}
```

**Real-world scenario:** Storing student grades (different number of assignments per student)
```csharp
public class GradeBook
{
    private int[][] studentGrades; // Each student may have different number of grades

    public GradeBook(int numberOfStudents)
    {
        studentGrades = new int[numberOfStudents][];
    }

    public void AddStudentGrades(int studentIndex, int[] grades)
    {
        studentGrades[studentIndex] = grades;
    }

    public double CalculateAverage(int studentIndex)
    {
        if (studentGrades[studentIndex] == null || studentGrades[studentIndex].Length == 0)
            return 0;

        return studentGrades[studentIndex].Average();
    }
}
```

**When to use Jagged vs Multi-dimensional:**
- **Jagged Array:** Use when rows have different lengths (memory efficient)
- **Multi-dimensional Array:** Use for fixed rectangular structures (slightly faster access)

### Common Patterns and Problems

#### Pattern 1: Two Sum Problem
**Problem:** Find two numbers in an array that sum to a target value.
```csharp
// Approach 1: Brute Force
// Time: O(n²), Space: O(1)
public static int[] TwoSum_BruteForce(int[] nums, int target)
{
    for (int i = 0; i < nums.Length; i++)
    {
        for (int j = i + 1; j < nums.Length; j++)
        {
            if (nums[i] + nums[j] == target)
                return new int[] { i, j };
        }
    }
    return new int[] { -1, -1 };
}

// Approach 2: Hash Map (Optimal)
// Time: O(n), Space: O(n)
public static int[] TwoSum_HashMap(int[] nums, int target)
{
    var map = new Dictionary<int, int>(); // value -> index

    for (int i = 0; i < nums.Length; i++)
    {
        int complement = target - nums[i];

        if (map.ContainsKey(complement))
            return new int[] { map[complement], i };

        if (!map.ContainsKey(nums[i]))
            map[nums[i]] = i;
    }

    return new int[] { -1, -1 };
}

// Usage
int[] numbers = { 2, 7, 11, 15 };
int target = 9;
int[] result = TwoSum_HashMap(numbers, target); // [0, 1] because 2 + 7 = 9
```

**Real-world scenario:** Finding matching transactions that balance out
```csharp
// Find two expenses that match a budget
public int[] FindMatchingExpenses(decimal[] expenses, decimal budget)
{
    var map = new Dictionary<decimal, int>();

    for (int i = 0; i < expenses.Length; i++)
    {
        decimal needed = budget - expenses[i];
        if (map.ContainsKey(needed))
            return new int[] { map[needed], i };

        if (!map.ContainsKey(expenses[i]))
            map[expenses[i]] = i;
    }

    return new int[] { -1, -1 };
}
```

#### Pattern 2: Maximum Subarray (Kadane's Algorithm)
**Problem:** Find the contiguous subarray with the largest sum.
```csharp
// Kadane's Algorithm
// Time: O(n), Space: O(1)
public static int MaxSubArray(int[] nums)
{
    if (nums == null || nums.Length == 0)
        throw new ArgumentException("Array cannot be null or empty");

    int maxSoFar = nums[0];
    int maxEndingHere = nums[0];

    for (int i = 1; i < nums.Length; i++)
    {
        // Either extend the existing subarray or start new from current element
        maxEndingHere = Math.Max(nums[i], maxEndingHere + nums[i]);
        maxSoFar = Math.Max(maxSoFar, maxEndingHere);
    }

    return maxSoFar;
}

// With subarray indices
public static (int maxSum, int start, int end) MaxSubArrayWithIndices(int[] nums)
{
    int maxSoFar = nums[0];
    int maxEndingHere = nums[0];
    int start = 0, end = 0, tempStart = 0;

    for (int i = 1; i < nums.Length; i++)
    {
        if (nums[i] > maxEndingHere + nums[i])
        {
            maxEndingHere = nums[i];
            tempStart = i;
        }
        else
        {
            maxEndingHere += nums[i];
        }

        if (maxEndingHere > maxSoFar)
        {
            maxSoFar = maxEndingHere;
            start = tempStart;
            end = i;
        }
    }

    return (maxSoFar, start, end);
}

// Usage
int[] profits = { -2, 1, -3, 4, -1, 2, 1, -5, 4 };
int maxProfit = MaxSubArray(profits); // 6 (subarray [4, -1, 2, 1])
```

**Real-world scenario:** Finding the best period of sales performance
```csharp
public class SalesAnalyzer
{
    public (decimal maxProfit, DateTime start, DateTime end) FindBestPeriod(
        decimal[] dailyProfits, DateTime[] dates)
    {
        var result = MaxSubArrayWithIndices(dailyProfits.Select(p => (int)p).ToArray());
        return (result.maxSum, dates[result.start], dates[result.end]);
    }
}
```

#### Pattern 3: Rotate Array
**Problem:** Rotate an array to the right by k steps.
```csharp
// Approach 1: Using extra space
// Time: O(n), Space: O(n)
public static void Rotate_ExtraSpace(int[] nums, int k)
{
    int n = nums.Length;
    k = k % n; // Handle k > n

    int[] result = new int[n];

    for (int i = 0; i < n; i++)
    {
        result[(i + k) % n] = nums[i];
    }

    Array.Copy(result, nums, n);
}

// Approach 2: Reverse method (optimal, in-place)
// Time: O(n), Space: O(1)
public static void Rotate_Reverse(int[] nums, int k)
{
    int n = nums.Length;
    k = k % n; // Handle k > n

    // Reverse entire array
    Reverse(nums, 0, n - 1);
    // Reverse first k elements
    Reverse(nums, 0, k - 1);
    // Reverse remaining elements
    Reverse(nums, k, n - 1);
}

private static void Reverse(int[] arr, int start, int end)
{
    while (start < end)
    {
        int temp = arr[start];
        arr[start] = arr[end];
        arr[end] = temp;
        start++;
        end--;
    }
}

// Usage
int[] numbers = { 1, 2, 3, 4, 5, 6, 7 };
Rotate_Reverse(numbers, 3); // [5, 6, 7, 1, 2, 3, 4]
```

**Real-world scenario:** Implementing a circular buffer for log rotation
```csharp
public class CircularBuffer<T>
{
    private T[] buffer;
    private int writePosition = 0;

    public CircularBuffer(int size)
    {
        buffer = new T[size];
    }

    public void Add(T item)
    {
        buffer[writePosition] = item;
        writePosition = (writePosition + 1) % buffer.Length;
    }

    public T[] GetAll()
    {
        // Rotate to get items in insertion order
        var result = new T[buffer.Length];
        for (int i = 0; i < buffer.Length; i++)
        {
            result[i] = buffer[(writePosition + i) % buffer.Length];
        }
        return result;
    }
}
```

#### Pattern 4: Sliding Window
**Problem:** Find maximum sum of k consecutive elements.
```csharp
// Time: O(n), Space: O(1)
public static int MaxSumSubarray(int[] arr, int k)
{
    if (arr.Length < k)
        throw new ArgumentException("Array size must be >= k");

    // Calculate sum of first window
    int windowSum = 0;
    for (int i = 0; i < k; i++)
    {
        windowSum += arr[i];
    }

    int maxSum = windowSum;

    // Slide the window
    for (int i = k; i < arr.Length; i++)
    {
        windowSum = windowSum - arr[i - k] + arr[i]; // Remove left, add right
        maxSum = Math.Max(maxSum, windowSum);
    }

    return maxSum;
}

// Usage
int[] scores = { 100, 200, 300, 400, 500 };
int maxConsecutive = MaxSumSubarray(scores, 3); // 1200 (300+400+500)
```

**Real-world scenario:** Finding peak load in time-series data
```csharp
public class LoadAnalyzer
{
    // Find peak average load over any 5-minute window
    public double FindPeakLoad(double[] requestsPerSecond, int windowSeconds)
    {
        return MaxSumSubarray(
            requestsPerSecond.Select(r => (int)r).ToArray(),
            windowSeconds
        ) / (double)windowSeconds;
    }
}
```

### Real-World Examples

#### Example 1: Inventory Management System
```csharp
public class InventorySystem
{
    private string[] productIds;
    private int[] quantities;
    private decimal[] prices;

    public InventorySystem(int capacity)
    {
        productIds = new string[capacity];
        quantities = new int[capacity];
        prices = new decimal[capacity];
    }

    // Add product - O(1) if index known
    public void AddProduct(int index, string id, int quantity, decimal price)
    {
        productIds[index] = id;
        quantities[index] = quantity;
        prices[index] = price;
    }

    // Find product by ID - O(n)
    public int FindProduct(string productId)
    {
        return Array.IndexOf(productIds, productId);
    }

    // Calculate total inventory value - O(n)
    public decimal CalculateTotalValue()
    {
        decimal total = 0;
        for (int i = 0; i < productIds.Length; i++)
        {
            if (productIds[i] != null)
                total += quantities[i] * prices[i];
        }
        return total;
    }

    // Get low stock items - O(n)
    public string[] GetLowStockItems(int threshold)
    {
        var lowStock = new List<string>();
        for (int i = 0; i < quantities.Length; i++)
        {
            if (quantities[i] < threshold && productIds[i] != null)
                lowStock.Add(productIds[i]);
        }
        return lowStock.ToArray();
    }
}
```

#### Example 2: Student Score Processing
```csharp
public class ScoreProcessor
{
    private int[] scores;

    public ScoreProcessor(int[] studentScores)
    {
        scores = studentScores;
    }

    // Calculate statistics - O(n)
    public (double average, int min, int max, double median) GetStatistics()
    {
        double average = scores.Average();
        int min = scores.Min();
        int max = scores.Max();

        // Calculate median
        int[] sorted = (int[])scores.Clone();
        Array.Sort(sorted);
        double median = sorted.Length % 2 == 0
            ? (sorted[sorted.Length / 2 - 1] + sorted[sorted.Length / 2]) / 2.0
            : sorted[sorted.Length / 2];

        return (average, min, max, median);
    }

    // Find grade distribution - O(n)
    public Dictionary<char, int> GetGradeDistribution()
    {
        var distribution = new Dictionary<char, int>
        {
            { 'A', 0 }, { 'B', 0 }, { 'C', 0 }, { 'D', 0 }, { 'F', 0 }
        };

        foreach (int score in scores)
        {
            char grade = score >= 90 ? 'A' :
                        score >= 80 ? 'B' :
                        score >= 70 ? 'C' :
                        score >= 60 ? 'D' : 'F';
            distribution[grade]++;
        }

        return distribution;
    }

    // Curve scores (add points to all) - O(n)
    public void ApplyCurve(int points)
    {
        for (int i = 0; i < scores.Length; i++)
        {
            scores[i] = Math.Min(100, scores[i] + points);
        }
    }
}
```

#### Example 3: Time Series Data Analysis
```csharp
public class TimeSeriesAnalyzer
{
    private double[] values;
    private DateTime[] timestamps;

    public TimeSeriesAnalyzer(double[] data, DateTime[] times)
    {
        if (data.Length != times.Length)
            throw new ArgumentException("Data and timestamps must have same length");

        values = data;
        timestamps = times;
    }

    // Find anomalies (values beyond threshold from moving average) - O(n)
    public List<(DateTime time, double value, double deviation)> DetectAnomalies(
        int windowSize, double threshold)
    {
        var anomalies = new List<(DateTime, double, double)>();

        for (int i = windowSize; i < values.Length; i++)
        {
            // Calculate moving average
            double sum = 0;
            for (int j = i - windowSize; j < i; j++)
            {
                sum += values[j];
            }
            double average = sum / windowSize;

            // Check if current value is anomalous
            double deviation = Math.Abs(values[i] - average);
            if (deviation > threshold)
            {
                anomalies.Add((timestamps[i], values[i], deviation));
            }
        }

        return anomalies;
    }

    // Calculate rate of change - O(n)
    public double[] CalculateRateOfChange()
    {
        double[] rates = new double[values.Length - 1];

        for (int i = 1; i < values.Length; i++)
        {
            TimeSpan timeDiff = timestamps[i] - timestamps[i - 1];
            rates[i - 1] = (values[i] - values[i - 1]) / timeDiff.TotalSeconds;
        }

        return rates;
    }
}
```

### Best Practices

**Do's**
1. **Use `Array.Empty<T>()`** for empty arrays instead of `new T[0]` (more efficient)
2. **Check bounds** before accessing array elements to avoid `IndexOutOfRangeException`
3. **Use `Array.BinarySearch()`** for sorted arrays instead of linear search
4. **Consider `List<T>`** for dynamic sizing needs (better than manual array resizing)
5. **Use `Array.Copy()`** for efficient array copying instead of loops
6. **Initialize arrays with appropriate size** to minimize resizing
7. **Use `Span<T>` and `Memory<T>`** for high-performance scenarios to avoid allocations
8. **Cache `Length` property** in loops if it won't change

```csharp
// Good: Cache length
int length = array.Length;
for (int i = 0; i < length; i++) { }

// Bad: Recalculate each iteration
for (int i = 0; i < array.Length; i++) { }
```

**Don'ts**
1. **Don't use arrays for frequently changing sizes** (use `List<T>` instead)
2. **Don't forget to check for null** before accessing array elements
3. **Don't use magic numbers** for array sizes (use constants)
4. **Don't ignore array bounds** when calculating indices
5. **Don't use `Array.Copy` with overlapping regions** in the same array
6. **Don't allocate large arrays on stack** (use heap)
7. **Don't modify collection while iterating with foreach**

```csharp
// Bad: Will throw exception
foreach (var item in items)
{
    if (condition)
        items = InsertAtEnd(items, newItem); // Modifying during iteration
}

// Good: Use for loop with index
for (int i = 0; i < items.Length; i++)
{
    if (condition)
        items = InsertAtEnd(items, newItem);
}
```

### Common Pitfalls

**Pitfall 1: Off-by-One Errors**
```csharp
// Wrong: Will throw IndexOutOfRangeException
for (int i = 0; i <= arr.Length; i++) // Should be i < arr.Length
{
    Console.WriteLine(arr[i]);
}

// Correct
for (int i = 0; i < arr.Length; i++)
{
    Console.WriteLine(arr[i]);
}
```

**Pitfall 2: Array Copying vs Reference Assignment**
```csharp
int[] original = { 1, 2, 3 };

// Wrong: Both variables point to same array
int[] copy1 = original; // Reference copy
copy1[0] = 99;
Console.WriteLine(original[0]); // 99 - original modified!

// Correct: Create independent copy
int[] copy2 = (int[])original.Clone(); // Or Array.Copy()
copy2[0] = 99;
Console.WriteLine(original[0]); // 1 - original unchanged
```

**Pitfall 3: Integer Overflow in Index Calculation**
```csharp
// Wrong: Can cause integer overflow for large arrays
int mid = (left + right) / 2;

// Correct: Prevents overflow
int mid = left + (right - left) / 2;
```

**Pitfall 4: Modifying Array During Foreach**
```csharp
// Wrong: Cannot modify array size during foreach
foreach (var item in array)
{
    // array = InsertAtEnd(array, newItem); // Will fail
}

// Correct: Use for loop or create new array
for (int i = 0; i < array.Length; i++)
{
    // Can modify array here
}
```

### Interview Tips

**Common Questions**
1. **Difference between array and List<T>?**
   - Array: Fixed size, slightly faster access, lower-level
   - List<T>: Dynamic size, more flexible, built on arrays
2. **Why are arrays faster than List<T>?**
   - Direct memory access, no bounds checking overhead in some cases
   - Less memory overhead (no capacity management)
3. **When to use jagged vs multidimensional arrays?**
   - Jagged: Variable row lengths, more memory efficient
   - Multidimensional: Fixed rectangular structure, slightly faster
4. **How to handle large arrays efficiently?**
   - Use `Span<T>` for stack allocation
   - Consider memory-mapped files for very large data
   - Use streaming/chunking for processing

**Performance Considerations**
- **Array access:** O(1) - use when random access is needed
- **Array search:** O(n) unsorted, O(log n) sorted
- **Array sort:** O(n log n) with `Array.Sort()`
- **Memory:** Contiguous allocation - good cache locality
- **Resize:** O(n) - avoid frequent resizing

#### Pattern 5: Subarrays with Product Less Than K

Count contiguous subarrays whose product of all elements is strictly less than `k`. A classic **sliding window** problem — the window approach works because all elements are positive, so extending the window can only increase the product.

```csharp
// O(n) time, O(1) space. Assumes every nums[i] > 0.
public static int NumSubarrayProductLessThanK(int[] nums, int k)
{
    if (k <= 1) return 0;              // nothing can have product < 1
    int product = 1, left = 0, count = 0;
    for (int right = 0; right < nums.Length; right++)
    {
        product *= nums[right];
        while (product >= k)           // shrink window until product < k
            product /= nums[left++];
        count += right - left + 1;     // every subarray ending at 'right' that is valid
    }
    return count;
}
```

Key insight: when the window `[left..right]` has product `< k`, **every** subarray that ends at `right` and starts anywhere in `[left..right]` is also valid — that contributes `right - left + 1` new subarrays. (LeetCode 713.)

### Arrays Summary

| Concept | Key Points |
|---------|------------|
| **Declaration** | Fixed size, type-safe, zero-indexed |
| **Access** | O(1) time via index |
| **Search** | O(n) linear, O(log n) binary (sorted) |
| **Insert/Delete** | O(n) due to shifting |
| **Memory** | Contiguous, reference type, heap allocation |
| **Multi-dimensional** | 2D arrays (rectangular) and jagged arrays |
| **Common Patterns** | Two Sum, Max Subarray, Rotate, Sliding Window |
| **Best Use Cases** | Fixed-size collections, high-performance access, mathematical operations |

**Related Topics:** Lists and Collections (dynamic arrays, `LinkedList<T>`), Sorting Algorithms (QuickSort, MergeSort), Searching Algorithms (binary, interpolation), `Span<T>`/`Memory<T>`, LINQ.

---

## Strings

A comprehensive guide to string manipulation, algorithms, and patterns in C#, covering fundamentals, efficient techniques, and real-world applications for technical interviews and development.

### Quick Overview
- **Strings** are **immutable** sequences of Unicode characters in C#
- Strings are **reference types** but have value-type semantics for equality
- **StringBuilder** should be used for multiple concatenations (mutable)
- String operations often have **O(n)** time complexity
- String interning can optimize memory for duplicate strings

### String Fundamentals

#### Declaration and Initialization
```csharp
// Various ways to declare strings
string str1 = "Hello, World!";              // String literal
string str2 = new string('a', 5);           // "aaaaa" - repeat character
string str3 = String.Empty;                 // Empty string (preferred over "")
string str4 = string.Concat("Hello", " ", "World");
string str5 = $"Value: {42}";               // String interpolation (C# 6+)

// Verbatim strings (ignore escape sequences)
string path = @"C:\Users\Documents\file.txt";

// Raw string literals (C# 11+)
string json = """
{
    "name": "John",
    "age": 30
}
""";

// Multi-line strings
string multiline = @"Line 1
Line 2
Line 3";
```

#### Key Properties and Methods
```csharp
string text = "Hello, World!";

// Properties
int length = text.Length;                   // 13
char firstChar = text[0];                   // 'H'

// Common methods
bool isEmpty = string.IsNullOrEmpty(text);  // false
bool isWhitespace = string.IsNullOrWhiteSpace(text); // false
string upper = text.ToUpper();              // "HELLO, WORLD!"
string lower = text.ToLower();              // "hello, world!"
string trimmed = "  test  ".Trim();         // "test"
bool contains = text.Contains("World");     // true
bool starts = text.StartsWith("Hello");     // true
bool ends = text.EndsWith("!");             // true
int index = text.IndexOf("World");          // 7
string sub = text.Substring(0, 5);          // "Hello"
```

#### Time/Space Complexity

| Operation | Time Complexity | Space Complexity | Notes |
|-----------|----------------|------------------|-------|
| Access by index | O(1) | O(1) | Direct character access |
| Length | O(1) | O(1) | Cached property |
| Concatenation (+) | O(n + m) | O(n + m) | Creates new string |
| Substring | O(n) | O(n) | Creates new string |
| IndexOf/Contains | O(n*m) | O(1) | Naive search; m = pattern length |
| Replace | O(n) | O(n) | Creates new string |
| Split | O(n) | O(n) | Creates array |
| ToUpper/ToLower | O(n) | O(n) | Creates new string |
| StringBuilder.Append | O(1)* | O(1) | *Amortized |
| String.Compare | O(n) | O(1) | Character comparison |

### String Immutability

#### Understanding Immutability
```csharp
// Strings are immutable - any "modification" creates a new string
string original = "Hello";
string modified = original.ToUpper(); // Creates new string

Console.WriteLine(original);  // "Hello" - unchanged
Console.WriteLine(modified);  // "HELLO" - new string

// What happens internally with concatenation
string s1 = "A";
string s2 = "B";
string s3 = s1 + s2; // Creates new string "AB" in memory
// s1 and s2 remain unchanged
```

#### Performance Impact
```csharp
// BAD: Creates n intermediate strings - O(n²)
string result = "";
for (int i = 0; i < 1000; i++)
{
    result += i.ToString(); // Creates new string each iteration
}

// GOOD: Use StringBuilder - O(n)
var sb = new StringBuilder();
for (int i = 0; i < 1000; i++)
{
    sb.Append(i);
}
string result = sb.ToString();
```

### StringBuilder — Mutable Strings

#### When to Use StringBuilder
**Use StringBuilder when:**
- Performing multiple concatenations (3+ operations)
- Building strings in loops
- Frequent modifications needed
- Performance is critical

**Use regular strings when:**
- Few concatenations (1-2 operations)
- String interpolation suffices
- Readability is priority over performance

#### StringBuilder Operations
```csharp
// Creating StringBuilder
var sb = new StringBuilder();                    // Default capacity 16
var sb2 = new StringBuilder(100);                // Initial capacity 100
var sb3 = new StringBuilder("Initial", 100);     // Initial value + capacity

// Common operations
sb.Append("Hello");                              // Add text
sb.AppendLine("World");                          // Add text with newline
sb.Insert(5, ", ");                              // Insert at position
sb.Replace("World", "C#");                       // Replace all occurrences
sb.Remove(5, 2);                                 // Remove characters
sb.Clear();                                      // Clear all content

// Get final string
string result = sb.ToString();

// Capacity management
int capacity = sb.Capacity;                      // Current capacity
sb.EnsureCapacity(200);                          // Ensure minimum capacity
```

#### StringBuilder Performance Example
```csharp
using System.Diagnostics;

// Benchmark: String concatenation vs StringBuilder
public class StringPerformanceTest
{
    public static void ComparePerformance(int iterations)
    {
        // Test 1: String concatenation
        var sw1 = Stopwatch.StartNew();
        string result1 = "";
        for (int i = 0; i < iterations; i++)
        {
            result1 += i.ToString();
        }
        sw1.Stop();

        // Test 2: StringBuilder
        var sw2 = Stopwatch.StartNew();
        var sb = new StringBuilder();
        for (int i = 0; i < iterations; i++)
        {
            sb.Append(i);
        }
        string result2 = sb.ToString();
        sw2.Stop();

        Console.WriteLine($"String concat: {sw1.ElapsedMilliseconds}ms");
        Console.WriteLine($"StringBuilder: {sw2.ElapsedMilliseconds}ms");
        // For 10000 iterations:
        // String concat: ~500ms (O(n²))
        // StringBuilder: ~2ms (O(n))
    }
}
```

**Real-world scenario:** Building HTML dynamically
```csharp
public class HtmlBuilder
{
    private StringBuilder sb = new StringBuilder();

    public HtmlBuilder AddElement(string tag, string content)
    {
        sb.AppendLine($"<{tag}>{content}</{tag}>");
        return this; // Fluent interface
    }

    public HtmlBuilder AddDiv(string cssClass, string content)
    {
        sb.AppendLine($"<div class='{cssClass}'>{content}</div>");
        return this;
    }

    public string Build() => sb.ToString();
}

// Usage
var html = new HtmlBuilder()
    .AddElement("h1", "Welcome")
    .AddDiv("content", "Hello World")
    .AddElement("p", "This is a paragraph")
    .Build();
```

### String Manipulation Techniques

#### 1. Reversing a String
```csharp
// Method 1: Using Array.Reverse
public static string ReverseString_Array(string s)
{
    char[] chars = s.ToCharArray();
    Array.Reverse(chars);
    return new string(chars);

    // Time: O(n), Space: O(n)
}

// Method 2: Using StringBuilder
public static string ReverseString_StringBuilder(string s)
{
    var sb = new StringBuilder();
    for (int i = s.Length - 1; i >= 0; i--)
    {
        sb.Append(s[i]);
    }
    return sb.ToString();

    // Time: O(n), Space: O(n)
}

// Method 3: Using LINQ
public static string ReverseString_Linq(string s)
{
    return new string(s.Reverse().ToArray());

    // Time: O(n), Space: O(n)
}

// Method 4: Two-pointer approach (in-place for char array)
public static void ReverseCharArray(char[] s)
{
    int left = 0, right = s.Length - 1;

    while (left < right)
    {
        // Swap
        char temp = s[left];
        s[left] = s[right];
        s[right] = temp;

        left++;
        right--;
    }

    // Time: O(n), Space: O(1)
}
```

**Real-world scenario:** Reversing domain name for sorting
```csharp
public class DomainSorter
{
    // Reverse domains for better sorting (com.example.www instead of www.example.com)
    public static string[] SortDomainsByTLD(string[] domains)
    {
        return domains
            .Select(d => ReverseString_Array(d))
            .OrderBy(d => d)
            .Select(d => ReverseString_Array(d))
            .ToArray();
    }
}
```

#### 2. Checking Palindrome
```csharp
// Method 1: Using string reversal
public static bool IsPalindrome_Reverse(string s)
{
    string cleaned = new string(s.Where(char.IsLetterOrDigit)
                                  .Select(char.ToLower)
                                  .ToArray());

    string reversed = new string(cleaned.Reverse().ToArray());
    return cleaned == reversed;

    // Time: O(n), Space: O(n)
}

// Method 2: Two-pointer approach (optimal)
public static bool IsPalindrome_TwoPointer(string s)
{
    int left = 0, right = s.Length - 1;

    while (left < right)
    {
        // Skip non-alphanumeric characters
        while (left < right && !char.IsLetterOrDigit(s[left]))
            left++;
        while (left < right && !char.IsLetterOrDigit(s[right]))
            right--;

        // Compare characters (case-insensitive)
        if (char.ToLower(s[left]) != char.ToLower(s[right]))
            return false;

        left++;
        right--;
    }

    return true;

    // Time: O(n), Space: O(1)
}

// Method 3: Using Span<T> (high performance)
public static bool IsPalindrome_Span(string s)
{
    ReadOnlySpan<char> span = s.AsSpan();
    int left = 0, right = span.Length - 1;

    while (left < right)
    {
        while (left < right && !char.IsLetterOrDigit(span[left]))
            left++;
        while (left < right && !char.IsLetterOrDigit(span[right]))
            right--;

        if (char.ToLower(span[left]) != char.ToLower(span[right]))
            return false;

        left++;
        right--;
    }

    return true;

    // Time: O(n), Space: O(1) - no allocations
}
```

**Real-world scenario:** Validating license plate palindromes
```csharp
public class LicensePlateValidator
{
    public static bool IsSpecialPlate(string plate)
    {
        // Some jurisdictions offer special palindromic plates
        return IsPalindrome_TwoPointer(plate);
    }
}
```

#### 3. Checking Anagrams
```csharp
// Method 1: Sorting approach
public static bool AreAnagrams_Sort(string s1, string s2)
{
    if (s1.Length != s2.Length)
        return false;

    char[] arr1 = s1.ToLower().ToCharArray();
    char[] arr2 = s2.ToLower().ToCharArray();

    Array.Sort(arr1);
    Array.Sort(arr2);

    return new string(arr1) == new string(arr2);

    // Time: O(n log n), Space: O(n)
}

// Method 2: Character frequency (optimal)
public static bool AreAnagrams_Frequency(string s1, string s2)
{
    if (s1.Length != s2.Length)
        return false;

    var charCount = new Dictionary<char, int>();

    // Count characters in first string
    foreach (char c in s1.ToLower())
    {
        if (!charCount.ContainsKey(c))
            charCount[c] = 0;
        charCount[c]++;
    }

    // Decrement count for second string
    foreach (char c in s2.ToLower())
    {
        if (!charCount.ContainsKey(c))
            return false;

        charCount[c]--;
        if (charCount[c] < 0)
            return false;
    }

    return charCount.Values.All(count => count == 0);

    // Time: O(n), Space: O(k) where k is unique characters
}

// Method 3: Using array for lowercase letters only
public static bool AreAnagrams_Array(string s1, string s2)
{
    if (s1.Length != s2.Length)
        return false;

    int[] charCount = new int[26]; // Only lowercase a-z

    foreach (char c in s1.ToLower())
    {
        if (char.IsLetter(c))
            charCount[c - 'a']++;
    }

    foreach (char c in s2.ToLower())
    {
        if (char.IsLetter(c))
            charCount[c - 'a']--;
    }

    return charCount.All(count => count == 0);

    // Time: O(n), Space: O(1) - fixed size array
}
```

**Real-world scenario:** Finding similar usernames for fraud detection
```csharp
public class UsernameFraudDetector
{
    public static List<string> FindSuspiciousAccounts(string[] usernames, string targetUser)
    {
        var suspicious = new List<string>();

        foreach (var username in usernames)
        {
            if (username != targetUser && AreAnagrams_Frequency(username, targetUser))
            {
                suspicious.Add(username);
            }
        }

        return suspicious;
    }
}
```

#### 4. String Compression
```csharp
// Run-length encoding
public static string CompressString(string s)
{
    if (string.IsNullOrEmpty(s))
        return s;

    var sb = new StringBuilder();
    int count = 1;

    for (int i = 1; i <= s.Length; i++)
    {
        // Check if we're at the end or character changed
        if (i == s.Length || s[i] != s[i - 1])
        {
            sb.Append(s[i - 1]);
            if (count > 1)
                sb.Append(count);

            count = 1;
        }
        else
        {
            count++;
        }
    }

    string compressed = sb.ToString();

    // Return original if compression doesn't reduce size
    return compressed.Length < s.Length ? compressed : s;

    // Time: O(n), Space: O(n)
}

// Decompression
public static string DecompressString(string compressed)
{
    var sb = new StringBuilder();
    int i = 0;

    while (i < compressed.Length)
    {
        char c = compressed[i];
        i++;

        // Read count if present
        int count = 0;
        while (i < compressed.Length && char.IsDigit(compressed[i]))
        {
            count = count * 10 + (compressed[i] - '0');
            i++;
        }

        // Append character (count times, or once if no count)
        sb.Append(c, count == 0 ? 1 : count);
    }

    return sb.ToString();
}

// Usage
string original = "aaabbbcccc";
string compressed = CompressString(original);    // "a3b3c4"
string decompressed = DecompressString(compressed); // "aaabbbcccc"
```

**Real-world scenario:** Compressing repetitive log data
```csharp
public class LogCompressor
{
    public static string CompressRepeatedMessages(string[] logMessages)
    {
        if (logMessages.Length == 0)
            return string.Empty;

        var sb = new StringBuilder();
        string currentMsg = logMessages[0];
        int count = 1;

        for (int i = 1; i < logMessages.Length; i++)
        {
            if (logMessages[i] == currentMsg)
            {
                count++;
            }
            else
            {
                sb.AppendLine($"[{count}x] {currentMsg}");
                currentMsg = logMessages[i];
                count = 1;
            }
        }

        sb.AppendLine($"[{count}x] {currentMsg}");
        return sb.ToString();
    }
}
```

### Pattern Matching Algorithms

#### 1. Naive String Search
```csharp
// Simple pattern matching
// Time: O(n*m) where n = text length, m = pattern length
public static List<int> NaiveSearch(string text, string pattern)
{
    var matches = new List<int>();
    int n = text.Length;
    int m = pattern.Length;

    for (int i = 0; i <= n - m; i++)
    {
        int j;
        for (j = 0; j < m; j++)
        {
            if (text[i + j] != pattern[j])
                break;
        }

        if (j == m) // Pattern found
            matches.Add(i);
    }

    return matches;
}
```

#### 2. KMP (Knuth-Morris-Pratt) Algorithm
```csharp
// KMP pattern matching - optimal for single pattern
// Time: O(n + m), Space: O(m)
public class KMPMatcher
{
    // Build LPS (Longest Proper Prefix which is also Suffix) array
    private static int[] ComputeLPSArray(string pattern)
    {
        int m = pattern.Length;
        int[] lps = new int[m];
        int length = 0; // Length of previous longest prefix suffix
        int i = 1;

        while (i < m)
        {
            if (pattern[i] == pattern[length])
            {
                length++;
                lps[i] = length;
                i++;
            }
            else
            {
                if (length != 0)
                {
                    length = lps[length - 1];
                }
                else
                {
                    lps[i] = 0;
                    i++;
                }
            }
        }

        return lps;
    }

    // KMP search
    public static List<int> KMPSearch(string text, string pattern)
    {
        var matches = new List<int>();
        int n = text.Length;
        int m = pattern.Length;

        if (m == 0)
            return matches;

        int[] lps = ComputeLPSArray(pattern);
        int i = 0; // Index for text
        int j = 0; // Index for pattern

        while (i < n)
        {
            if (pattern[j] == text[i])
            {
                i++;
                j++;
            }

            if (j == m)
            {
                matches.Add(i - j);
                j = lps[j - 1];
            }
            else if (i < n && pattern[j] != text[i])
            {
                if (j != 0)
                    j = lps[j - 1];
                else
                    i++;
            }
        }

        return matches;
    }
}
```

**Real-world scenario:** Finding all occurrences of a keyword in a document
```csharp
public class DocumentSearcher
{
    public static Dictionary<string, List<int>> FindKeywords(
        string document, string[] keywords)
    {
        var results = new Dictionary<string, List<int>>();

        foreach (var keyword in keywords)
        {
            results[keyword] = KMPMatcher.KMPSearch(document, keyword);
        }

        return results;
    }
}
```

#### 3. Rabin-Karp Algorithm (Rolling Hash)
```csharp
// Rabin-Karp for pattern matching using rolling hash
// Time: O(n + m) average, O(n*m) worst case
// Space: O(1)
public class RabinKarp
{
    private const int Prime = 101; // Prime number for hash calculation

    public static List<int> Search(string text, string pattern)
    {
        var matches = new List<int>();
        int n = text.Length;
        int m = pattern.Length;

        if (m > n)
            return matches;

        // Calculate hash for pattern and first window of text
        int patternHash = 0;
        int textHash = 0;
        int h = 1;

        // h = pow(d, m-1) % Prime
        for (int i = 0; i < m - 1; i++)
            h = (h * 256) % Prime;

        // Calculate initial hashes
        for (int i = 0; i < m; i++)
        {
            patternHash = (256 * patternHash + pattern[i]) % Prime;
            textHash = (256 * textHash + text[i]) % Prime;
        }

        // Slide pattern over text
        for (int i = 0; i <= n - m; i++)
        {
            // Check if hashes match
            if (patternHash == textHash)
            {
                // Verify character by character
                bool match = true;
                for (int j = 0; j < m; j++)
                {
                    if (text[i + j] != pattern[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                    matches.Add(i);
            }

            // Calculate hash for next window
            if (i < n - m)
            {
                textHash = (256 * (textHash - text[i] * h) + text[i + m]) % Prime;

                // Handle negative hash
                if (textHash < 0)
                    textHash += Prime;
            }
        }

        return matches;
    }
}
```

**Real-world scenario:** Plagiarism detection
```csharp
public class PlagiarismDetector
{
    public static double CalculateSimilarity(string document1, string document2, int chunkSize)
    {
        // Break documents into chunks and find matching segments
        int matches = 0;
        int totalChunks = document1.Length - chunkSize + 1;

        for (int i = 0; i <= document1.Length - chunkSize; i++)
        {
            string chunk = document1.Substring(i, chunkSize);
            var positions = RabinKarp.Search(document2, chunk);
            if (positions.Count > 0)
                matches++;
        }

        return (double)matches / totalChunks * 100;
    }
}
```

### Common String Problems

#### Problem 1: Longest Substring Without Repeating Characters
```csharp
// Sliding window approach
// Time: O(n), Space: O(min(n, m)) where m is charset size
public static int LengthOfLongestSubstring(string s)
{
    var charSet = new HashSet<char>();
    int maxLength = 0;
    int left = 0;

    for (int right = 0; right < s.Length; right++)
    {
        // Remove characters from left until no duplicates
        while (charSet.Contains(s[right]))
        {
            charSet.Remove(s[left]);
            left++;
        }

        charSet.Add(s[right]);
        maxLength = Math.Max(maxLength, right - left + 1);
    }

    return maxLength;
}

// Using dictionary to track last seen index
public static int LengthOfLongestSubstring_Optimized(string s)
{
    var lastSeen = new Dictionary<char, int>();
    int maxLength = 0;
    int left = 0;

    for (int right = 0; right < s.Length; right++)
    {
        if (lastSeen.ContainsKey(s[right]))
        {
            // Move left pointer to position after last occurrence
            left = Math.Max(left, lastSeen[s[right]] + 1);
        }

        lastSeen[s[right]] = right;
        maxLength = Math.Max(maxLength, right - left + 1);
    }

    return maxLength;
}

// Usage
string test = "abcabcbb";
int length = LengthOfLongestSubstring(test); // 3 ("abc")
```

#### Problem 2: Valid Parentheses
```csharp
// Check if parentheses/brackets are balanced
// Time: O(n), Space: O(n)
public static bool IsValidParentheses(string s)
{
    var stack = new Stack<char>();
    var pairs = new Dictionary<char, char>
    {
        { ')', '(' },
        { '}', '{' },
        { ']', '[' }
    };

    foreach (char c in s)
    {
        if (c == '(' || c == '{' || c == '[')
        {
            stack.Push(c);
        }
        else if (c == ')' || c == '}' || c == ']')
        {
            if (stack.Count == 0 || stack.Pop() != pairs[c])
                return false;
        }
    }

    return stack.Count == 0;
}

// Usage
bool valid1 = IsValidParentheses("()[]{}");    // true
bool valid2 = IsValidParentheses("([)]");      // false
bool valid3 = IsValidParentheses("{[]}");      // true
```

**Real-world scenario:** Validating JSON/XML syntax
```csharp
public class SyntaxValidator
{
    public static bool ValidateJsonStructure(string json)
    {
        // Simplified validation focusing on brackets
        return IsValidParentheses(json);
    }
}
```

#### Problem 3: Group Anagrams
```csharp
// Group strings that are anagrams
// Time: O(n * k log k) where n = number of strings, k = max string length
// Space: O(n * k)
public static List<List<string>> GroupAnagrams(string[] strs)
{
    var groups = new Dictionary<string, List<string>>();

    foreach (string str in strs)
    {
        // Create sorted key
        char[] chars = str.ToCharArray();
        Array.Sort(chars);
        string key = new string(chars);

        if (!groups.ContainsKey(key))
            groups[key] = new List<string>();

        groups[key].Add(str);
    }

    return groups.Values.ToList();
}

// Alternative: Using character count as key (faster for long strings)
public static List<List<string>> GroupAnagrams_CharCount(string[] strs)
{
    var groups = new Dictionary<string, List<string>>();

    foreach (string str in strs)
    {
        // Create character count key
        int[] count = new int[26];
        foreach (char c in str)
        {
            count[c - 'a']++;
        }

        string key = string.Join(',', count);

        if (!groups.ContainsKey(key))
            groups[key] = new List<string>();

        groups[key].Add(str);
    }

    return groups.Values.ToList();
}

// Usage
string[] words = { "eat", "tea", "tan", "ate", "nat", "bat" };
var groups = GroupAnagrams(words);
// Result: [["eat","tea","ate"], ["tan","nat"], ["bat"]]
```

#### Problem 4: Longest Palindromic Substring
```csharp
// Expand around center approach
// Time: O(n²), Space: O(1)
public static string LongestPalindrome(string s)
{
    if (string.IsNullOrEmpty(s))
        return string.Empty;

    int start = 0, maxLen = 0;

    for (int i = 0; i < s.Length; i++)
    {
        // Check for odd-length palindromes (center at i)
        int len1 = ExpandAroundCenter(s, i, i);

        // Check for even-length palindromes (center between i and i+1)
        int len2 = ExpandAroundCenter(s, i, i + 1);

        int len = Math.Max(len1, len2);

        if (len > maxLen)
        {
            maxLen = len;
            start = i - (len - 1) / 2;
        }
    }

    return s.Substring(start, maxLen);
}

private static int ExpandAroundCenter(string s, int left, int right)
{
    while (left >= 0 && right < s.Length && s[left] == s[right])
    {
        left--;
        right++;
    }

    return right - left - 1;
}

// Usage
string text = "babad";
string longest = LongestPalindrome(text); // "bab" or "aba"
```

### Real-World Examples (Strings)

#### Example 1: Email Validation and Parsing
```csharp
public class EmailProcessor
{
    // Simple email validation
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        int atIndex = email.IndexOf('@');
        if (atIndex <= 0 || atIndex == email.Length - 1)
            return false;

        int dotIndex = email.LastIndexOf('.');
        if (dotIndex <= atIndex + 1 || dotIndex == email.Length - 1)
            return false;

        return true;
    }

    // Parse email into components
    public static (string username, string domain) ParseEmail(string email)
    {
        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format");

        int atIndex = email.IndexOf('@');
        string username = email.Substring(0, atIndex);
        string domain = email.Substring(atIndex + 1);

        return (username, domain);
    }

    // Mask email for privacy
    public static string MaskEmail(string email)
    {
        var (username, domain) = ParseEmail(email);

        if (username.Length <= 2)
            return $"{username[0]}***@{domain}";

        string masked = $"{username[0]}***{username[username.Length - 1]}@{domain}";
        return masked;
    }
}

// Usage
string email = "john.doe@example.com";
var (user, domain) = EmailProcessor.ParseEmail(email); // ("john.doe", "example.com")
string masked = EmailProcessor.MaskEmail(email);        // "j***e@example.com"
```

#### Example 2: URL Parsing and Manipulation
```csharp
public class UrlParser
{
    // Extract query parameters
    public static Dictionary<string, string> ParseQueryString(string url)
    {
        var parameters = new Dictionary<string, string>();

        int queryIndex = url.IndexOf('?');
        if (queryIndex == -1)
            return parameters;

        string query = url.Substring(queryIndex + 1);
        string[] pairs = query.Split('&');

        foreach (string pair in pairs)
        {
            string[] keyValue = pair.Split('=');
            if (keyValue.Length == 2)
            {
                parameters[keyValue[0]] = Uri.UnescapeDataString(keyValue[1]);
            }
        }

        return parameters;
    }

    // Build URL with query parameters
    public static string BuildUrl(string baseUrl, Dictionary<string, string> parameters)
    {
        if (parameters == null || parameters.Count == 0)
            return baseUrl;

        var sb = new StringBuilder(baseUrl);
        sb.Append('?');

        bool first = true;
        foreach (var kvp in parameters)
        {
            if (!first)
                sb.Append('&');

            sb.Append($"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}");
            first = false;
        }

        return sb.ToString();
    }

    // Extract domain from URL
    public static string ExtractDomain(string url)
    {
        // Remove protocol
        int protocolEnd = url.IndexOf("://");
        string withoutProtocol = protocolEnd >= 0
            ? url.Substring(protocolEnd + 3)
            : url;

        // Extract domain (before first / or ?)
        int pathStart = withoutProtocol.IndexOfAny(new[] { '/', '?' });
        string domain = pathStart >= 0
            ? withoutProtocol.Substring(0, pathStart)
            : withoutProtocol;

        return domain;
    }
}

// Usage
string url = "https://example.com/search?q=test&page=2";
var queryParams = UrlParser.ParseQueryString(url); // { "q": "test", "page": "2" }
string domain = UrlParser.ExtractDomain(url);  // "example.com"
```

#### Example 3: CSV Parser
```csharp
public class CsvParser
{
    // Parse CSV line handling quoted fields
    public static string[] ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var currentField = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    // Escaped quote
                    currentField.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                // Field delimiter
                fields.Add(currentField.ToString());
                currentField.Clear();
            }
            else
            {
                currentField.Append(c);
            }
        }

        fields.Add(currentField.ToString());
        return fields.ToArray();
    }

    // Parse entire CSV file
    public static List<Dictionary<string, string>> ParseCsv(string csvContent)
    {
        var lines = csvContent.Split(new[] { '\r', '\n' },
            StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length == 0)
            return new List<Dictionary<string, string>>();

        // First line is header
        string[] headers = ParseCsvLine(lines[0]);
        var records = new List<Dictionary<string, string>>();

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = ParseCsvLine(lines[i]);
            var record = new Dictionary<string, string>();

            for (int j = 0; j < Math.Min(headers.Length, values.Length); j++)
            {
                record[headers[j]] = values[j];
            }

            records.Add(record);
        }

        return records;
    }
}
```

### Best Practices (Strings)

**Do's**
1. **Use StringBuilder for multiple concatenations** (3+ operations)
2. **Use `string.IsNullOrEmpty()`** or **`string.IsNullOrWhiteSpace()`** for validation
3. **Use StringComparison enum** for culture-aware comparisons
4. **Cache `string.Length`** in loops to avoid repeated property access
5. **Use `Span<T>`** for high-performance scenarios without allocations
6. **Use string interpolation** (`$""`) for readability (few concatenations)
7. **Consider `ReadOnlySpan<char>`** for substring operations without allocation
8. **Use const for string literals** that won't change

```csharp
// Good: Use StringComparison
bool equal = str1.Equals(str2, StringComparison.OrdinalIgnoreCase);

// Good: Use Span for parsing without allocation
ReadOnlySpan<char> span = text.AsSpan(startIndex, length);
```

**Don'ts**
1. **Don't concatenate strings in loops** with + operator
2. **Don't use == for case-insensitive comparison** (use Equals with StringComparison)
3. **Don't call ToLower()/ToUpper() for comparison** (inefficient)
4. **Don't use Substring excessively** (creates new strings)
5. **Don't forget null checks** before string operations
6. **Don't use string.Format() when string interpolation suffices**
7. **Don't compare strings with ==** for non-ordinal comparison

```csharp
// Bad: Case-insensitive comparison
if (str1.ToLower() == str2.ToLower()) { } // Creates 2 new strings!

// Good: Use Equals with comparison
if (str1.Equals(str2, StringComparison.OrdinalIgnoreCase)) { }
```

### Common Pitfalls (Strings)

**Pitfall 1: String Concatenation in Loops**
```csharp
// Wrong: O(n²) complexity
string result = "";
for (int i = 0; i < 1000; i++)
{
    result += i.ToString(); // Creates new string each time!
}

// Correct: O(n) complexity
var sb = new StringBuilder();
for (int i = 0; i < 1000; i++)
{
    sb.Append(i);
}
string result = sb.ToString();
```

**Pitfall 2: String Comparison Issues**
```csharp
// Wrong: Case-sensitive comparison
if (userInput == "Yes") { } // Fails for "yes", "YES"

// Correct: Case-insensitive comparison
if (userInput.Equals("Yes", StringComparison.OrdinalIgnoreCase)) { }
```

**Pitfall 3: Null Reference Exceptions**
```csharp
// Wrong: Null reference exception if str is null
if (str.Length > 0) { }

// Correct: Check for null/empty
if (!string.IsNullOrEmpty(str)) { }
```

**Pitfall 4: Substring Index Errors**
```csharp
string text = "Hello";

// Wrong: May throw ArgumentOutOfRangeException
string sub = text.Substring(10); // Index out of range!

// Correct: Check bounds
if (index < text.Length)
{
    string sub = text.Substring(index);
}

// Or use Span for bounds-checked slicing
ReadOnlySpan<char> span = text.AsSpan();
if (index < span.Length)
{
    ReadOnlySpan<char> slice = span.Slice(index);
}
```

### Interview Tips (Strings)

**Common Questions**
1. **Why are strings immutable in C#?**
   - Thread safety, security, optimization (string interning), hash code caching
2. **When to use StringBuilder vs String?**
   - StringBuilder: Multiple concatenations, loops, performance-critical
   - String: Few operations, readability priority, immutability needed
3. **What is string interning?**
   - Runtime maintains a pool of unique string literals to save memory
   - Same literal used multiple times references same memory
4. **How to reverse a string efficiently?**
   - Use char array with Array.Reverse() or two-pointer approach
5. **Difference between String and string?**
   - string is C# keyword (alias), String is .NET type
   - Functionally identical, prefer lowercase 'string' by convention

**Performance Considerations**
- **String concatenation:** O(n) for each operation
- **StringBuilder.Append:** O(1) amortized
- **String.Contains:** O(n*m) worst case
- **String.IndexOf:** O(n*m) naive, optimized in .NET
- **String interning:** Saves memory but costs time for lookup

#### Longest Palindromic Substring

Return the longest palindromic (symmetric) **contiguous** substring. The clean interview answer is **expand around center**: every palindrome has a center — a single character (odd length) or the gap between two characters (even length) — so test all `2n - 1` centers.

```csharp
// O(n^2) time, O(1) extra space. (Manacher's gives O(n) but is rarely required.)
public static string LongestPalindrome(string s)
{
    if (string.IsNullOrEmpty(s)) return "";
    int start = 0, maxLen = 1;
    for (int center = 0; center < s.Length; center++)
    {
        Expand(s, center, center,     ref start, ref maxLen); // odd length
        Expand(s, center, center + 1, ref start, ref maxLen); // even length
    }
    return s.Substring(start, maxLen);
}

private static void Expand(string s, int l, int r, ref int start, ref int maxLen)
{
    while (l >= 0 && r < s.Length && s[l] == s[r]) { l--; r++; }
    int len = r - l - 1;              // loop overshoots by one on each side
    if (len > maxLen) { maxLen = len; start = l + 1; }
}
```

Clarify the wording: longest palindromic **substring** (contiguous, shown here, LeetCode 5) is different from longest palindromic **subsequence** (non-contiguous, a separate DP problem). Ask which one the interviewer means.

### Strings Summary

| Concept | Key Points |
|---------|------------|
| **Immutability** | Strings never change; operations create new strings |
| **StringBuilder** | Use for multiple concatenations, mutable |
| **Searching** | KMP O(n+m), Rabin-Karp O(n) average |
| **Common Patterns** | Two-pointer, sliding window, hash maps |
| **Performance** | Avoid concatenation in loops, use Span<T> |
| **Best Practices** | Use StringComparison, null checks, StringBuilder |

**Related Topics:** Regular Expressions (Regex class), String Encoding (UTF-8/16, ASCII), Text Processing (parsing, tokenization), Cryptography (hashing, encoding), Globalization (culture-aware operations).

---

## Hashing

A comprehensive guide to hashing, hash tables, dictionaries, and hash-based data structures in C#, covering fundamentals, algorithms, and real-world applications for technical interviews and development.

### Quick Overview
- **Hashing** transforms data into a fixed-size value (hash code) for fast lookup
- **Dictionary<TKey, TValue>** is C#'s primary hash table implementation
- **HashSet<T>** provides O(1) average-case operations for unique elements
- Hash functions should be **fast**, **deterministic**, and **uniformly distributed**
- **Collision resolution** is crucial for hash table performance

### Hashing Fundamentals

#### What is Hashing?
Hashing is a technique that maps data of arbitrary size to fixed-size values (hash codes) for efficient storage and retrieval.
```text
Input Data → Hash Function → Hash Code (Integer)
"John" → HashFunction("John") → 2547896321
```

**Key Properties:**
- **Deterministic:** Same input always produces same hash
- **Fast:** O(1) computation time
- **Uniform Distribution:** Minimizes collisions
- **One-way:** Hard to reverse (for cryptographic hashes)

#### Time/Space Complexity

| Operation | Average Case | Worst Case | Space |
|-----------|-------------|------------|-------|
| Insert | O(1) | O(n) | O(n) |
| Search | O(1) | O(n) | O(1) |
| Delete | O(1) | O(n) | O(1) |
| Contains | O(1) | O(n) | O(1) |

**Worst case** occurs when all keys hash to the same bucket (collision).

### Dictionary<TKey, TValue>

#### Basic Operations
```csharp
// Creating dictionaries
var dict = new Dictionary<string, int>();
var dict2 = new Dictionary<string, int>(100);  // Initial capacity
var dict3 = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase); // Custom comparer

// Adding elements
dict.Add("apple", 5);
dict["banana"] = 3;  // Adds or updates

// Accessing elements
int value = dict["apple"];  // Throws if key doesn't exist

// Safe access
if (dict.TryGetValue("apple", out int val))
{
    Console.WriteLine($"Value: {val}");
}

// Checking existence
bool exists = dict.ContainsKey("apple");
bool hasValue = dict.ContainsValue(5);

// Removing elements
dict.Remove("apple");
dict.Clear();  // Remove all

// Iterating
foreach (var kvp in dict)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
}

// Keys and Values collections
var keys = dict.Keys;      // ICollection<string>
var values = dict.Values;  // ICollection<int>
```

#### Performance Characteristics
```csharp
// Dictionary internally uses arrays and linked lists
// Capacity grows when load factor exceeds threshold (~0.75)

var dict = new Dictionary<string, int>();

// Initial capacity: 0
// After first add: capacity becomes 3
// Growth pattern: 3 → 7 → 17 → 37 → 79 → 163 → ...
// New capacity ≈ 2 × old capacity + 1

// Pre-allocate if size known
var optimized = new Dictionary<string, int>(10000); // Avoids resizing
```

#### Advanced Usage
```csharp
// Custom equality comparer
public class CaseInsensitiveComparer : IEqualityComparer<string>
{
    public bool Equals(string x, string y)
    {
        return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(string obj)
    {
        return obj.ToLowerInvariant().GetHashCode();
    }
}

var dict = new Dictionary<string, int>(new CaseInsensitiveComparer());
dict["Hello"] = 1;
Console.WriteLine(dict["hello"]); // 1 - case insensitive

// Using complex keys
var personDict = new Dictionary<(string firstName, string lastName), int>();
personDict[("John", "Doe")] = 12345;

// Dictionary with default values
var dictWithDefault = new Dictionary<string, int>();
int count = dictWithDefault.GetValueOrDefault("missing", 0); // Returns 0
```

**Real-world scenario:** Caching user sessions
```csharp
public class SessionManager
{
    private Dictionary<string, UserSession> sessions = new();

    public void CreateSession(string sessionId, UserSession session)
    {
        sessions[sessionId] = session;
    }

    public UserSession GetSession(string sessionId)
    {
        return sessions.TryGetValue(sessionId, out var session)
            ? session
            : null;
    }

    public void RemoveSession(string sessionId)
    {
        sessions.Remove(sessionId);
    }

    public void CleanupExpiredSessions()
    {
        var expired = sessions
            .Where(kvp => kvp.Value.ExpiresAt < DateTime.UtcNow)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var id in expired)
        {
            sessions.Remove(id);
        }
    }
}

public class UserSession
{
    public string UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public Dictionary<string, object> Data { get; set; }
}
```

### HashSet<T>

#### Basic Operations
```csharp
// Creating HashSet
var set = new HashSet<int>();
var set2 = new HashSet<int>(new[] { 1, 2, 3, 4, 5 });
var set3 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

// Adding elements
set.Add(1);
set.Add(2);
set.Add(1);  // Duplicate - won't be added
Console.WriteLine(set.Count);  // 2

// Checking membership
bool contains = set.Contains(1);  // O(1)

// Removing elements
set.Remove(1);
set.RemoveWhere(x => x > 10);  // Remove by condition

// Set operations
var set1 = new HashSet<int> { 1, 2, 3, 4 };
var setB = new HashSet<int> { 3, 4, 5, 6 };

// Union (all unique elements)
var union = new HashSet<int>(set1);
union.UnionWith(setB);  // {1, 2, 3, 4, 5, 6}

// Intersection (common elements)
var intersection = new HashSet<int>(set1);
intersection.IntersectWith(setB);  // {3, 4}

// Difference (in set1 but not setB)
var difference = new HashSet<int>(set1);
difference.ExceptWith(setB);  // {1, 2}

// Symmetric difference (XOR - in either but not both)
var symmetricDiff = new HashSet<int>(set1);
symmetricDiff.SymmetricExceptWith(setB);  // {1, 2, 5, 6}

// Subset/Superset checks
bool isSubset = set1.IsSubsetOf(setB);
bool isSuperset = set1.IsSupersetOf(setB);
bool overlaps = set1.Overlaps(setB);
```

#### Practical Examples
```csharp
// Remove duplicates from array
public static int[] RemoveDuplicates(int[] arr)
{
    var set = new HashSet<int>(arr);
    return set.ToArray();

    // Time: O(n), Space: O(n)
}

// Find unique elements
public static List<int> FindUniqueElements(int[] arr)
{
    var counts = new Dictionary<int, int>();

    foreach (int num in arr)
    {
        counts[num] = counts.GetValueOrDefault(num, 0) + 1;
    }

    return counts.Where(kvp => kvp.Value == 1)
                 .Select(kvp => kvp.Key)
                 .ToList();
}

// Find missing number in sequence
public static int FindMissingNumber(int[] nums, int n)
{
    var set = new HashSet<int>(nums);

    for (int i = 1; i <= n; i++)
    {
        if (!set.Contains(i))
            return i;
    }

    return -1;
}
```

**Real-world scenario:** Email deduplication and validation
```csharp
public class EmailService
{
    private HashSet<string> sentEmails = new(StringComparer.OrdinalIgnoreCase);
    private HashSet<string> blacklist = new(StringComparer.OrdinalIgnoreCase);

    public bool CanSendEmail(string email)
    {
        return !blacklist.Contains(email) && !sentEmails.Contains(email);
    }

    public void MarkAsSent(string email)
    {
        sentEmails.Add(email);
    }

    public void AddToBlacklist(string email)
    {
        blacklist.Add(email);
    }

    public List<string> GetUniqueRecipients(List<string> emails)
    {
        return new HashSet<string>(emails, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
```

### Hash Functions

#### How Hash Functions Work
```csharp
// Simple hash function example
public static int SimpleHash(string str)
{
    int hash = 0;
    foreach (char c in str)
    {
        hash = (hash * 31 + c) % int.MaxValue;
    }
    return hash;
}

// C# built-in GetHashCode
string text = "Hello";
int hashCode = text.GetHashCode();  // Uses optimized algorithm

// For custom objects
public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    // Override GetHashCode for proper Dictionary/HashSet usage
    public override int GetHashCode()
    {
        // Combine hash codes
        return HashCode.Combine(FirstName, LastName);

        // Or manually (older approach)
        // unchecked
        // {
        //     int hash = 17;
        //     hash = hash * 23 + (FirstName?.GetHashCode() ?? 0);
        //     hash = hash * 23 + (LastName?.GetHashCode() ?? 0);
        //     return hash;
        // }
    }

    // MUST override Equals when overriding GetHashCode
    public override bool Equals(object obj)
    {
        if (obj is not Person other)
            return false;

        return FirstName == other.FirstName && LastName == other.LastName;
    }
}
```

#### Hash Code Guidelines
**Best Practices:**
1. **Consistency:** Equal objects must have equal hash codes
2. **Distribution:** Spread values uniformly across int range
3. **Performance:** Fast to compute
4. **Immutability:** Hash code shouldn't change (use immutable fields)

```csharp
// Good: Using immutable properties
public class ImmutablePerson
{
    public string Name { get; }
    public int Age { get; }

    public ImmutablePerson(string name, int age)
    {
        Name = name;
        Age = age;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Age);
    }

    public override bool Equals(object obj)
    {
        return obj is ImmutablePerson other &&
               Name == other.Name &&
               Age == other.Age;
    }
}

// Bad: Using mutable properties (dangerous!)
public class MutablePerson
{
    public string Name { get; set; }  // Mutable!

    public override int GetHashCode()
    {
        return Name.GetHashCode();  // Changes if Name changes!
    }

    // Problem: If Name changes after adding to Dictionary/HashSet,
    // the object becomes "lost" - can't be found or removed
}
```

### Collision Resolution

#### Separate Chaining (C# Dictionary Implementation)
```csharp
// Conceptual implementation of separate chaining
public class SimpleHashTable<TKey, TValue>
{
    private class Entry
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
        public Entry Next { get; set; }  // Linked list for collisions
    }

    private Entry[] buckets;
    private int count;

    public SimpleHashTable(int capacity = 16)
    {
        buckets = new Entry[capacity];
    }

    private int GetBucketIndex(TKey key)
    {
        int hash = key.GetHashCode();
        return Math.Abs(hash) % buckets.Length;
    }

    public void Add(TKey key, TValue value)
    {
        int index = GetBucketIndex(key);

        // Check if key exists
        Entry current = buckets[index];
        while (current != null)
        {
            if (current.Key.Equals(key))
                throw new ArgumentException("Key already exists");
            current = current.Next;
        }

        // Add to beginning of chain
        var newEntry = new Entry
        {
            Key = key,
            Value = value,
            Next = buckets[index]
        };
        buckets[index] = newEntry;
        count++;

        // Resize if load factor too high
        if ((double)count / buckets.Length > 0.75)
            Resize();
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        int index = GetBucketIndex(key);
        Entry current = buckets[index];

        while (current != null)
        {
            if (current.Key.Equals(key))
            {
                value = current.Value;
                return true;
            }
            current = current.Next;
        }

        value = default;
        return false;
    }

    private void Resize()
    {
        var oldBuckets = buckets;
        buckets = new Entry[oldBuckets.Length * 2];
        count = 0;

        // Rehash all entries
        foreach (var entry in oldBuckets)
        {
            Entry current = entry;
            while (current != null)
            {
                Add(current.Key, current.Value);
                current = current.Next;
            }
        }
    }
}
```

#### Load Factor and Performance
```csharp
// Load factor = number of entries / number of buckets
// Ideal load factor: 0.5 - 0.75

public class LoadFactorDemo
{
    public static void DemonstrateLoadFactor()
    {
        // Low load factor: more memory, fewer collisions, faster
        var sparse = new Dictionary<int, int>(1000);
        for (int i = 0; i < 100; i++)
            sparse[i] = i;
        // Load factor: 100/1000 = 0.1

        // High load factor: less memory, more collisions, slower
        var dense = new Dictionary<int, int>(10);
        for (int i = 0; i < 100; i++)
            dense[i] = i;
        // Load factor starts at 0, grows and triggers resizes

        // Optimal: pre-allocate if size known
        var optimal = new Dictionary<int, int>(100);
        for (int i = 0; i < 100; i++)
            optimal[i] = i;
        // Load factor: ~1.0, no resizes needed
    }
}
```

### Common Hashing Problems

#### Problem 1: Two Sum (Hash Map Approach)
```csharp
// Find indices of two numbers that sum to target
// Time: O(n), Space: O(n)
public static int[] TwoSum(int[] nums, int target)
{
    var map = new Dictionary<int, int>(); // value -> index

    for (int i = 0; i < nums.Length; i++)
    {
        int complement = target - nums[i];

        if (map.ContainsKey(complement))
            return new[] { map[complement], i };

        map[nums[i]] = i;  // Store current number
    }

    return new[] { -1, -1 };
}

// Usage
int[] nums = { 2, 7, 11, 15 };
int[] result = TwoSum(nums, 9);  // [0, 1]
```

#### Problem 2: Subarray Sum Equals K
```csharp
// Count subarrays with sum equal to k
// Time: O(n), Space: O(n)
public static int SubarraySum(int[] nums, int k)
{
    var sumCount = new Dictionary<int, int>();
    sumCount[0] = 1;  // Empty subarray

    int count = 0;
    int sum = 0;

    foreach (int num in nums)
    {
        sum += num;

        // Check if (sum - k) exists
        if (sumCount.ContainsKey(sum - k))
            count += sumCount[sum - k];

        // Update sum frequency
        sumCount[sum] = sumCount.GetValueOrDefault(sum, 0) + 1;
    }

    return count;
}

// Usage
int[] arr = { 1, 1, 1 };
int count = SubarraySum(arr, 2);  // 2 subarrays: [1,1] appears twice
```

**Real-world scenario:** Analyzing financial transactions
```csharp
public class TransactionAnalyzer
{
    // Find periods where total transactions equal target amount
    public static List<(int start, int end)> FindTransactionPeriods(
        decimal[] amounts, decimal target)
    {
        var periods = new List<(int, int)>();
        var sumIndices = new Dictionary<decimal, List<int>>();
        sumIndices[0] = new List<int> { -1 };

        decimal sum = 0;
        for (int i = 0; i < amounts.Length; i++)
        {
            sum += amounts[i];

            if (sumIndices.ContainsKey(sum - target))
            {
                foreach (int startIndex in sumIndices[sum - target])
                {
                    periods.Add((startIndex + 1, i));
                }
            }

            if (!sumIndices.ContainsKey(sum))
                sumIndices[sum] = new List<int>();

            sumIndices[sum].Add(i);
        }

        return periods;
    }
}
```

#### Problem 3: Longest Consecutive Sequence
```csharp
// Find length of longest consecutive sequence
// Time: O(n), Space: O(n)
public static int LongestConsecutive(int[] nums)
{
    var numSet = new HashSet<int>(nums);
    int maxLength = 0;

    foreach (int num in numSet)
    {
        // Only start counting from sequence beginning
        if (!numSet.Contains(num - 1))
        {
            int currentNum = num;
            int currentLength = 1;

            // Count consecutive numbers
            while (numSet.Contains(currentNum + 1))
            {
                currentNum++;
                currentLength++;
            }

            maxLength = Math.Max(maxLength, currentLength);
        }
    }

    return maxLength;
}

// Usage
int[] nums = { 100, 4, 200, 1, 3, 2 };
int length = LongestConsecutive(nums);  // 4 (sequence: 1,2,3,4)
```

#### Problem 4: Group Anagrams
```csharp
// Group strings that are anagrams of each other
// Time: O(n * k log k) where k is max string length
// Space: O(n * k)
public static List<List<string>> GroupAnagrams(string[] strs)
{
    var groups = new Dictionary<string, List<string>>();

    foreach (string str in strs)
    {
        // Sort characters as key
        char[] chars = str.ToCharArray();
        Array.Sort(chars);
        string key = new string(chars);

        if (!groups.ContainsKey(key))
            groups[key] = new List<string>();

        groups[key].Add(str);
    }

    return groups.Values.ToList();
}

// Alternative: Character frequency as key (faster for long strings)
public static List<List<string>> GroupAnagrams_Frequency(string[] strs)
{
    var groups = new Dictionary<string, List<string>>();

    foreach (string str in strs)
    {
        // Create frequency signature
        int[] freq = new int[26];
        foreach (char c in str)
            freq[c - 'a']++;

        string key = string.Join(',', freq);

        if (!groups.ContainsKey(key))
            groups[key] = new List<string>();

        groups[key].Add(str);
    }

    return groups.Values.ToList();
}
```

#### Problem 5: LRU Cache
```csharp
// Least Recently Used Cache using Dictionary + Doubly Linked List
// Time: O(1) for get and put operations
public class LRUCache
{
    private class Node
    {
        public int Key { get; set; }
        public int Value { get; set; }
        public Node Prev { get; set; }
        public Node Next { get; set; }
    }

    private readonly Dictionary<int, Node> cache;
    private readonly int capacity;
    private Node head, tail;

    public LRUCache(int capacity)
    {
        this.capacity = capacity;
        cache = new Dictionary<int, Node>(capacity);

        // Dummy head and tail
        head = new Node();
        tail = new Node();
        head.Next = tail;
        tail.Prev = head;
    }

    public int Get(int key)
    {
        if (!cache.TryGetValue(key, out Node node))
            return -1;

        // Move to front (most recently used)
        MoveToFront(node);
        return node.Value;
    }

    public void Put(int key, int value)
    {
        if (cache.TryGetValue(key, out Node node))
        {
            // Update existing
            node.Value = value;
            MoveToFront(node);
        }
        else
        {
            // Add new node
            if (cache.Count >= capacity)
            {
                // Remove least recently used (tail.Prev)
                Node lru = tail.Prev;
                RemoveNode(lru);
                cache.Remove(lru.Key);
            }

            Node newNode = new Node { Key = key, Value = value };
            cache[key] = newNode;
            AddToFront(newNode);
        }
    }

    private void MoveToFront(Node node)
    {
        RemoveNode(node);
        AddToFront(node);
    }

    private void AddToFront(Node node)
    {
        node.Next = head.Next;
        node.Prev = head;
        head.Next.Prev = node;
        head.Next = node;
    }

    private void RemoveNode(Node node)
    {
        node.Prev.Next = node.Next;
        node.Next.Prev = node.Prev;
    }
}

// Usage
var cache = new LRUCache(2);
cache.Put(1, 1);
cache.Put(2, 2);
int val1 = cache.Get(1);     // Returns 1
cache.Put(3, 3);             // Evicts key 2
int val2 = cache.Get(2);     // Returns -1 (not found)
```

**Real-world scenario:** Database query result caching
```csharp
public class QueryCache
{
    private readonly LRUCache cache;

    public QueryCache(int maxQueries)
    {
        cache = new LRUCache(maxQueries);
    }

    public object ExecuteQuery(string query, Func<object> queryFunc)
    {
        int queryHash = query.GetHashCode();

        // Try to get from cache
        var cached = cache.Get(queryHash);
        if (cached != -1)
            return cached;

        // Execute query and cache result
        var result = queryFunc();
        cache.Put(queryHash, (int)result); // Simplified

        return result;
    }
}
```

#### Problem 6: First Non-Repeating Character
```csharp
// Find first character that appears only once
// Time: O(n), Space: O(k) where k is unique characters
public static char FirstNonRepeating(string s)
{
    var charCount = new Dictionary<char, int>();

    // Count frequencies
    foreach (char c in s)
    {
        charCount[c] = charCount.GetValueOrDefault(c, 0) + 1;
    }

    // Find first with count 1
    foreach (char c in s)
    {
        if (charCount[c] == 1)
            return c;
    }

    return '\0'; // Not found
}

// Usage
char result = FirstNonRepeating("leetcode");  // 'l'
```

### Real-World Examples (Hashing)

#### Example 1: Frequency Counter
```csharp
public class FrequencyAnalyzer
{
    // Count word frequencies in text
    public static Dictionary<string, int> CountWords(string text)
    {
        var frequencies = new Dictionary<string, int>(
            StringComparer.OrdinalIgnoreCase);

        // Split into words (simple tokenization)
        var words = text.Split(
            new[] { ' ', '.', ',', '!', '?', ';', ':' },
            StringSplitOptions.RemoveEmptyEntries);

        foreach (string word in words)
        {
            frequencies[word] = frequencies.GetValueOrDefault(word, 0) + 1;
        }

        return frequencies;
    }

    // Get top N most frequent words
    public static List<(string word, int count)> GetTopWords(
        Dictionary<string, int> frequencies, int n)
    {
        return frequencies
            .OrderByDescending(kvp => kvp.Value)
            .Take(n)
            .Select(kvp => (kvp.Key, kvp.Value))
            .ToList();
    }

    // Find words appearing exactly k times
    public static List<string> FindWordsWithFrequency(
        Dictionary<string, int> frequencies, int k)
    {
        return frequencies
            .Where(kvp => kvp.Value == k)
            .Select(kvp => kvp.Key)
            .ToList();
    }
}

// Usage
string document = "The quick brown fox jumps over the lazy dog. The dog sleeps.";
var frequencies = FrequencyAnalyzer.CountWords(document);
var topWords = FrequencyAnalyzer.GetTopWords(frequencies, 3);
// Result: [("the", 3), ("dog", 2), ...]
```

#### Example 2: Phone Directory
```csharp
public class PhoneDirectory
{
    private Dictionary<string, List<string>> nameToNumbers;
    private Dictionary<string, string> numberToName;

    public PhoneDirectory()
    {
        nameToNumbers = new Dictionary<string, List<string>>(
            StringComparer.OrdinalIgnoreCase);
        numberToName = new Dictionary<string, string>();
    }

    // Add contact (one name can have multiple numbers)
    public void AddContact(string name, string phoneNumber)
    {
        // Add to name->numbers mapping
        if (!nameToNumbers.ContainsKey(name))
            nameToNumbers[name] = new List<string>();

        nameToNumbers[name].Add(phoneNumber);

        // Add to number->name mapping
        numberToName[phoneNumber] = name;
    }

    // Find numbers by name
    public List<string> FindNumbersByName(string name)
    {
        return nameToNumbers.TryGetValue(name, out var numbers)
            ? numbers
            : new List<string>();
    }

    // Find name by number (reverse lookup)
    public string FindNameByNumber(string phoneNumber)
    {
        return numberToName.TryGetValue(phoneNumber, out var name)
            ? name
            : "Unknown";
    }

    // Find contacts with multiple numbers
    public List<string> FindContactsWithMultipleNumbers()
    {
        return nameToNumbers
            .Where(kvp => kvp.Value.Count > 1)
            .Select(kvp => kvp.Key)
            .ToList();
    }

    // Remove contact
    public void RemoveContact(string name)
    {
        if (nameToNumbers.TryGetValue(name, out var numbers))
        {
            // Remove from number->name mapping
            foreach (var number in numbers)
            {
                numberToName.Remove(number);
            }

            // Remove from name->numbers mapping
            nameToNumbers.Remove(name);
        }
    }
}
```

#### Example 3: Product Inventory System
```csharp
public class InventorySystem
{
    private Dictionary<string, Product> productById;
    private Dictionary<string, HashSet<string>> productsByCategory;
    private Dictionary<string, HashSet<string>> productsBySupplier;

    public InventorySystem()
    {
        productById = new Dictionary<string, Product>();
        productsByCategory = new Dictionary<string, HashSet<string>>();
        productsBySupplier = new Dictionary<string, HashSet<string>>();
    }

    public void AddProduct(Product product)
    {
        // Add to main inventory
        productById[product.Id] = product;

        // Index by category
        if (!productsByCategory.ContainsKey(product.Category))
            productsByCategory[product.Category] = new HashSet<string>();
        productsByCategory[product.Category].Add(product.Id);

        // Index by supplier
        if (!productsBySupplier.ContainsKey(product.Supplier))
            productsBySupplier[product.Supplier] = new HashSet<string>();
        productsBySupplier[product.Supplier].Add(product.Id);
    }

    public Product GetProduct(string productId)
    {
        return productById.TryGetValue(productId, out var product)
            ? product
            : null;
    }

    public List<Product> GetProductsByCategory(string category)
    {
        if (!productsByCategory.TryGetValue(category, out var productIds))
            return new List<Product>();

        return productIds
            .Select(id => productById[id])
            .ToList();
    }

    public List<Product> GetProductsBySupplier(string supplier)
    {
        if (!productsBySupplier.TryGetValue(supplier, out var productIds))
            return new List<Product>();

        return productIds
            .Select(id => productById[id])
            .ToList();
    }

    public List<Product> GetLowStockProducts(int threshold)
    {
        return productById.Values
            .Where(p => p.Quantity < threshold)
            .ToList();
    }

    public decimal CalculateTotalValue()
    {
        return productById.Values
            .Sum(p => p.Price * p.Quantity);
    }
}

public class Product
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public string Supplier { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
```

#### Example 4: Rate Limiter
```csharp
public class RateLimiter
{
    private class RateLimitInfo
    {
        public Queue<DateTime> Requests { get; set; } = new();
        public int MaxRequests { get; set; }
        public TimeSpan Window { get; set; }
    }

    private Dictionary<string, RateLimitInfo> limits;

    public RateLimiter()
    {
        limits = new Dictionary<string, RateLimitInfo>();
    }

    public void SetLimit(string userId, int maxRequests, TimeSpan window)
    {
        limits[userId] = new RateLimitInfo
        {
            MaxRequests = maxRequests,
            Window = window
        };
    }

    public bool AllowRequest(string userId)
    {
        if (!limits.TryGetValue(userId, out var info))
            return true; // No limit set

        var now = DateTime.UtcNow;
        var windowStart = now - info.Window;

        // Remove expired requests
        while (info.Requests.Count > 0 && info.Requests.Peek() < windowStart)
        {
            info.Requests.Dequeue();
        }

        // Check if under limit
        if (info.Requests.Count < info.MaxRequests)
        {
            info.Requests.Enqueue(now);
            return true;
        }

        return false; // Rate limit exceeded
    }

    public int GetRemainingRequests(string userId)
    {
        if (!limits.TryGetValue(userId, out var info))
            return int.MaxValue;

        var now = DateTime.UtcNow;
        var windowStart = now - info.Window;

        // Remove expired requests
        while (info.Requests.Count > 0 && info.Requests.Peek() < windowStart)
        {
            info.Requests.Dequeue();
        }

        return info.MaxRequests - info.Requests.Count;
    }
}

// Usage
var limiter = new RateLimiter();
limiter.SetLimit("user123", 10, TimeSpan.FromMinutes(1)); // 10 requests per minute

for (int i = 0; i < 15; i++)
{
    bool allowed = limiter.AllowRequest("user123");
    Console.WriteLine($"Request {i + 1}: {(allowed ? "Allowed" : "Denied")}");
}
```

### Best Practices (Hashing)

**Do's**
1. **Pre-allocate Dictionary capacity** if size is known
2. **Override GetHashCode and Equals together** for custom keys
3. **Use ImmutableRecord or readonly fields** for hash key properties
4. **Use StringComparer** for case-insensitive string keys
5. **Use TryGetValue** instead of ContainsKey + indexer
6. **Use GetValueOrDefault** for cleaner default value handling
7. **Consider HashSet** for uniqueness checks instead of Dictionary
8. **Use concurrent collections** for thread-safe scenarios

```csharp
// Good: Efficient pattern
if (dict.TryGetValue(key, out var value))
{
    // Use value
}

// Bad: Two lookups
if (dict.ContainsKey(key))
{
    var value = dict[key];
}
```

**Don'ts**
1. **Don't use mutable objects as Dictionary keys**
2. **Don't forget to override both GetHashCode AND Equals**
3. **Don't call GetHashCode repeatedly** (cache if needed)
4. **Don't use Dictionary for ordered data** (use SortedDictionary)
5. **Don't ignore capacity planning** for large collections
6. **Don't use == for Dictionary value comparison** (use Equals)
7. **Don't modify collection while iterating** (use ToList() first)

```csharp
// Bad: Mutable key
var dictBad = new Dictionary<List<int>, string>();
var keyBad = new List<int> { 1, 2 };
dictBad[keyBad] = "value";
keyBad.Add(3); // Now can't find the entry!

// Good: Immutable key
var dictGood = new Dictionary<string, string>();
```

### Common Pitfalls (Hashing)

**Pitfall 1: Modifying Keys After Insertion**
```csharp
// Wrong: Key modified after insertion
var person = new Person { Name = "John" };
dict[person] = "Employee";
person.Name = "Jane"; // Hash code changes!
var result = dict[person]; // Won't find it!

// Correct: Use immutable keys
public class ImmutablePerson
{
    public string Name { get; init; } // Immutable property
}
```

**Pitfall 2: Not Overriding Equals with GetHashCode**
```csharp
// Wrong: Override only GetHashCode
public class BadKey
{
    public int Id { get; set; }

    public override int GetHashCode() => Id.GetHashCode();
    // Missing Equals override!
}

// Correct: Override both
public class GoodKey
{
    public int Id { get; set; }

    public override int GetHashCode() => Id.GetHashCode();

    public override bool Equals(object obj)
    {
        return obj is GoodKey other && Id == other.Id;
    }
}
```

**Pitfall 3: KeyNotFoundException**
```csharp
// Wrong: Direct access throws if key missing
int value = dict[key]; // Throws KeyNotFoundException!

// Correct: Use TryGetValue
if (dict.TryGetValue(key, out int value))
{
    // Use value safely
}
```

**Pitfall 4: ConcurrentModificationException Equivalent**
```csharp
// Wrong: Modifying while iterating
foreach (var key in dict.Keys)
{
    dict.Remove(key); // InvalidOperationException!
}

// Correct: Create list first
foreach (var key in dict.Keys.ToList())
{
    dict.Remove(key);
}
```

### Interview Tips (Hashing)

**Common Questions**
1. **How does Dictionary work internally?**
   - Uses array of buckets, each bucket can have linked list for collisions
   - Hash function determines bucket, then linear search in bucket
   - Resizes when load factor exceeds threshold
2. **What's the difference between Dictionary and Hashtable?**
   - Dictionary: Generic, type-safe, faster, newer
   - Hashtable: Non-generic, thread-safe (synchronized), legacy
3. **When to use Dictionary vs HashSet?**
   - Dictionary: Key-value pairs, need associated data
   - HashSet: Unique items only, set operations needed
4. **What if two objects have same hash code?**
   - Collision handled by separate chaining or open addressing
   - Equals() method used to distinguish between colliding objects
5. **How to make Dictionary thread-safe?**
   - Use ConcurrentDictionary<TKey, TValue>
   - Or lock around Dictionary operations
   - Or use ImmutableDictionary for read-heavy scenarios

**Performance Considerations**
- **Capacity planning:** Set initial capacity to avoid resizing
- **Hash function quality:** Good distribution reduces collisions
- **Load factor:** Lower = faster but more memory
- **Key type:** Value types as keys avoid allocation
- **Concurrent access:** Use ConcurrentDictionary if needed

### Hashing Summary

| Concept | Key Points |
|---------|------------|
| **Dictionary** | Key-value pairs, O(1) average operations |
| **HashSet** | Unique elements, set operations, O(1) lookups |
| **Hash Function** | Deterministic, uniform distribution, fast |
| **Collisions** | Separate chaining (linked lists) in C# |
| **Load Factor** | 0.75 threshold triggers resize |
| **Best Practices** | Immutable keys, override Equals+GetHashCode |
| **Use Cases** | Caching, frequency counting, deduplication |

**Related Topics:** SortedDictionary (Red-Black tree), ConcurrentDictionary (thread-safe), ImmutableDictionary, Bloom Filters (probabilistic membership testing), Consistent Hashing (distributed systems).
