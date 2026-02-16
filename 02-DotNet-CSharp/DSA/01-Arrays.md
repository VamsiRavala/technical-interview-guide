# 📊 Arrays in C# — Complete DSA Guide

A comprehensive guide to arrays in C#, covering fundamentals, operations, patterns, and real-world applications for technical interviews and practical development.

---

## 🚀 Quick Overview
- **Arrays** are fixed-size, contiguous memory structures that store elements of the same type
- Arrays provide **O(1)** access time using index-based lookup
- Support for **multi-dimensional** and **jagged arrays** in C#
- Arrays are **reference types** in C# (stored on heap)
- **Zero-indexed**: first element is at index 0

---

## 🧠 Array Fundamentals

### Declaration and Initialization

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

### Time/Space Complexity

| Operation | Time Complexity | Space Complexity | Notes |
|-----------|----------------|------------------|-------|
| Access by index | O(1) | O(1) | Direct memory access |
| Search (unsorted) | O(n) | O(1) | Linear scan required |
| Search (sorted) | O(log n) | O(1) | Binary search |
| Insert at end | O(1)* | O(n) | *If space available; else O(n) |
| Insert at index | O(n) | O(n) | Shift elements |
| Delete at index | O(n) | O(1) | Shift elements |
| Resize | O(n) | O(n) | Copy to new array |

---

## 🔧 Basic Operations

### 1. Traversal

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

**Real-world scenario**: Processing sensor readings from IoT devices
```csharp
// Process temperature readings from multiple sensors
double[] temperatures = { 72.5, 68.3, 75.1, 71.8, 69.9 };
double average = temperatures.Average();
double max = temperatures.Max();
Console.WriteLine($"Average: {average}°F, Max: {max}°F");
```

### 2. Insertion

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

**Real-world scenario**: Adding a new product to inventory
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

### 3. Deletion

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

### 4. Searching

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

**Real-world scenario**: Finding a user by ID in a sorted database export
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

---

## 🧩 Multi-Dimensional Arrays

### 2D Arrays (Rectangular)

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

**Real-world scenario**: Representing a game board or seating chart
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

### Jagged Arrays (Array of Arrays)

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

**Real-world scenario**: Storing student grades (different number of assignments per student)
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
- **Jagged Array**: Use when rows have different lengths (memory efficient)
- **Multi-dimensional Array**: Use for fixed rectangular structures (slightly faster access)

---

## 🎯 Common Patterns and Problems

### Pattern 1: Two Sum Problem

**Problem**: Find two numbers in an array that sum to a target value.

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

**Real-world scenario**: Finding matching transactions that balance out
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

### Pattern 2: Maximum Subarray (Kadane's Algorithm)

**Problem**: Find the contiguous subarray with the largest sum.

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

**Real-world scenario**: Finding the best period of sales performance
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

### Pattern 3: Rotate Array

**Problem**: Rotate an array to the right by k steps.

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

**Real-world scenario**: Implementing a circular buffer for log rotation
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

### Pattern 4: Sliding Window

**Problem**: Find maximum sum of k consecutive elements.

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

**Real-world scenario**: Finding peak load in time-series data
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

---

## 📈 Real-World Examples

### Example 1: Inventory Management System

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

### Example 2: Student Score Processing

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

### Example 3: Time Series Data Analysis

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

---

## ✅ Best Practices

### Do's ✓
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

### Don'ts ✗
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

---

## ⚠️ Common Pitfalls

### Pitfall 1: Off-by-One Errors
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

### Pitfall 2: Array Copying vs Reference Assignment
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

### Pitfall 3: Integer Overflow in Index Calculation
```csharp
// Wrong: Can cause integer overflow for large arrays
int mid = (left + right) / 2;

// Correct: Prevents overflow
int mid = left + (right - left) / 2;
```

### Pitfall 4: Modifying Array During Foreach
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

---

## 🎓 Interview Tips

### Common Questions
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

### Performance Considerations
- **Array access**: O(1) - use when random access is needed
- **Array search**: O(n) unsorted, O(log n) sorted
- **Array sort**: O(n log n) with `Array.Sort()`
- **Memory**: Contiguous allocation - good cache locality
- **Resize**: O(n) - avoid frequent resizing

---

## 🧩 Summary

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

---

## 📚 Related Topics
- **Lists and Collections**: Dynamic arrays, LinkedList<T>
- **Sorting Algorithms**: QuickSort, MergeSort for arrays
- **Searching Algorithms**: Binary search, interpolation search
- **Span<T> and Memory<T>**: Modern high-performance alternatives
- **LINQ**: Functional operations on arrays

---

*This guide covers the essential array concepts needed for technical interviews and practical C# development. Practice these patterns and understand their complexities for optimal performance.*
