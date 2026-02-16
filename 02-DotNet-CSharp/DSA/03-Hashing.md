# 🔐 Hashing in C# — Complete DSA Guide

A comprehensive guide to hashing, hash tables, dictionaries, and hash-based data structures in C#, covering fundamentals, algorithms, and real-world applications for technical interviews and development.

---

## 🚀 Quick Overview
- **Hashing** transforms data into a fixed-size value (hash code) for fast lookup
- **Dictionary<TKey, TValue>** is C#'s primary hash table implementation
- **HashSet<T>** provides O(1) average-case operations for unique elements
- Hash functions should be **fast**, **deterministic**, and **uniformly distributed**
- **Collision resolution** is crucial for hash table performance

---

## 🧠 Hashing Fundamentals

### What is Hashing?

Hashing is a technique that maps data of arbitrary size to fixed-size values (hash codes) for efficient storage and retrieval.

```
Input Data → Hash Function → Hash Code (Integer)
"John" → HashFunction("John") → 2547896321
```

**Key Properties:**
- **Deterministic**: Same input always produces same hash
- **Fast**: O(1) computation time
- **Uniform Distribution**: Minimizes collisions
- **One-way**: Hard to reverse (for cryptographic hashes)

### Time/Space Complexity

| Operation | Average Case | Worst Case | Space |
|-----------|-------------|------------|-------|
| Insert | O(1) | O(n) | O(n) |
| Search | O(1) | O(n) | O(1) |
| Delete | O(1) | O(n) | O(1) |
| Contains | O(1) | O(n) | O(1) |

**Worst case** occurs when all keys hash to the same bucket (collision).

---

## 📚 Dictionary<TKey, TValue>

### Basic Operations

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

### Performance Characteristics

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

### Advanced Usage

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

**Real-world scenario**: Caching user sessions
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

---

## 🎯 HashSet<T>

### Basic Operations

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
var set2 = new HashSet<int> { 3, 4, 5, 6 };

// Union (all unique elements)
var union = new HashSet<int>(set1);
union.UnionWith(set2);  // {1, 2, 3, 4, 5, 6}

// Intersection (common elements)
var intersection = new HashSet<int>(set1);
intersection.IntersectWith(set2);  // {3, 4}

// Difference (in set1 but not set2)
var difference = new HashSet<int>(set1);
difference.ExceptWith(set2);  // {1, 2}

// Symmetric difference (XOR - in either but not both)
var symmetricDiff = new HashSet<int>(set1);
symmetricDiff.SymmetricExceptWith(set2);  // {1, 2, 5, 6}

// Subset/Superset checks
bool isSubset = set1.IsSubsetOf(set2);
bool isSuperset = set1.IsSupersetOf(set2);
bool overlaps = set1.Overlaps(set2);
```

### Practical Examples

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

**Real-world scenario**: Email deduplication and validation
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

---

## 🔑 Hash Functions

### How Hash Functions Work

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

### Hash Code Guidelines

**Best Practices:**
1. **Consistency**: Equal objects must have equal hash codes
2. **Distribution**: Spread values uniformly across int range
3. **Performance**: Fast to compute
4. **Immutability**: Hash code shouldn't change (use immutable fields)

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

---

## ⚔️ Collision Resolution

### Separate Chaining (C# Dictionary Implementation)

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

### Load Factor and Performance

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

---

## 🎯 Common Hashing Problems

### Problem 1: Two Sum (Hash Map Approach)

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

### Problem 2: Subarray Sum Equals K

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

**Real-world scenario**: Analyzing financial transactions
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

### Problem 3: Longest Consecutive Sequence

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

### Problem 4: Group Anagrams

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

### Problem 5: LRU Cache

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
cache.Put(3, 3);              // Evicts key 2
int val2 = cache.Get(2);     // Returns -1 (not found)
```

**Real-world scenario**: Database query result caching
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

### Problem 6: First Non-Repeating Character

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

---

## 📈 Real-World Examples

### Example 1: Frequency Counter

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

### Example 2: Phone Directory

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

### Example 3: Product Inventory System

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

### Example 4: Rate Limiter

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

---

## ✅ Best Practices

### Do's ✓
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

### Don'ts ✗
1. **Don't use mutable objects as Dictionary keys**
2. **Don't forget to override both GetHashCode AND Equals**
3. **Don't call GetHashCode repeatedly** (cache if needed)
4. **Don't use Dictionary for ordered data** (use SortedDictionary)
5. **Don't ignore capacity planning** for large collections
6. **Don't use == for Dictionary value comparison** (use Equals)
7. **Don't modify collection while iterating** (use ToList() first)

```csharp
// Bad: Mutable key
var dict = new Dictionary<List<int>, string>();
var key = new List<int> { 1, 2 };
dict[key] = "value";
key.Add(3); // Now can't find the entry!

// Good: Immutable key
var dict = new Dictionary<string, string>();
```

---

## ⚠️ Common Pitfalls

### Pitfall 1: Modifying Keys After Insertion
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

### Pitfall 2: Not Overriding Equals with GetHashCode
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

### Pitfall 3: KeyNotFoundException
```csharp
// Wrong: Direct access throws if key missing
int value = dict[key]; // Throws KeyNotFoundException!

// Correct: Use TryGetValue
if (dict.TryGetValue(key, out int value))
{
    // Use value safely
}
```

### Pitfall 4: ConcurrentModificationException Equivalent
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

---

## 🎓 Interview Tips

### Common Questions
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

### Performance Considerations
- **Capacity planning**: Set initial capacity to avoid resizing
- **Hash function quality**: Good distribution reduces collisions
- **Load factor**: Lower = faster but more memory
- **Key type**: Value types as keys avoid allocation
- **Concurrent access**: Use ConcurrentDictionary if needed

---

## 🧩 Summary

| Concept | Key Points |
|---------|------------|
| **Dictionary** | Key-value pairs, O(1) average operations |
| **HashSet** | Unique elements, set operations, O(1) lookups |
| **Hash Function** | Deterministic, uniform distribution, fast |
| **Collisions** | Separate chaining (linked lists) in C# |
| **Load Factor** | 0.75 threshold triggers resize |
| **Best Practices** | Immutable keys, override Equals+GetHashCode |
| **Use Cases** | Caching, frequency counting, deduplication |

---

## 📚 Related Topics
- **SortedDictionary**: Ordered key-value pairs (Red-Black tree)
- **ConcurrentDictionary**: Thread-safe dictionary
- **ImmutableDictionary**: Immutable hash table
- **Bloom Filters**: Probabilistic membership testing
- **Consistent Hashing**: Distributed systems

---

*This guide covers essential hashing concepts and Dictionary/HashSet usage in C# for technical interviews and practical development. Master these patterns for efficient data structure selection and implementation.*
