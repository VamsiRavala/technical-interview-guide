# 🔤 Strings in C# — Complete DSA Guide

A comprehensive guide to string manipulation, algorithms, and patterns in C#, covering fundamentals, efficient techniques, and real-world applications for technical interviews and development.

---

## 🚀 Quick Overview
- **Strings** are **immutable** sequences of Unicode characters in C#
- Strings are **reference types** but have value-type semantics for equality
- **StringBuilder** should be used for multiple concatenations (mutable)
- String operations often have **O(n)** time complexity
- String interning can optimize memory for duplicate strings

---

## 🧠 String Fundamentals

### Declaration and Initialization

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

### Key Properties and Methods

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

### Time/Space Complexity

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

---

## 🔧 String Immutability

### Understanding Immutability

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

### Performance Impact

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

---

## 🛠️ StringBuilder - Mutable Strings

### When to Use StringBuilder

**Use StringBuilder when:**
- Performing multiple concatenations (3+ operations)
- Building strings in loops
- Frequent modifications needed
- Performance is critical

**Use regular strings when:**
- Few concatenations (1-2 operations)
- String interpolation suffices
- Readability is priority over performance

### StringBuilder Operations

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

### StringBuilder Performance Example

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

**Real-world scenario**: Building HTML dynamically
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

---

## 🎯 String Manipulation Techniques

### 1. Reversing a String

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

**Real-world scenario**: Reversing domain name for sorting
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

### 2. Checking Palindrome

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

**Real-world scenario**: Validating license plate palindromes
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

### 3. Checking Anagrams

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

**Real-world scenario**: Finding similar usernames for fraud detection
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

### 4. String Compression

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

**Real-world scenario**: Compressing repetitive log data
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

---

## 🔍 Pattern Matching Algorithms

### 1. Naive String Search

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

### 2. KMP (Knuth-Morris-Pratt) Algorithm

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

**Real-world scenario**: Finding all occurrences of a keyword in a document
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

### 3. Rabin-Karp Algorithm (Rolling Hash)

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

**Real-world scenario**: Plagiarism detection
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

---

## 🎯 Common String Problems

### Problem 1: Longest Substring Without Repeating Characters

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

### Problem 2: Valid Parentheses

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

**Real-world scenario**: Validating JSON/XML syntax
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

### Problem 3: Group Anagrams

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

### Problem 4: Longest Palindromic Substring

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

---

## 📈 Real-World Examples

### Example 1: Email Validation and Parsing

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

### Example 2: URL Parsing and Manipulation

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
var params = UrlParser.ParseQueryString(url); // { "q": "test", "page": "2" }
string domain = UrlParser.ExtractDomain(url);  // "example.com"
```

### Example 3: CSV Parser

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

---

## ✅ Best Practices

### Do's ✓
1. **Use StringBuilder for multiple concatenations** (3+ operations)
2. **Use string.IsNullOrEmpty()** or **string.IsNullOrWhiteSpace()** for validation
3. **Use StringComparison enum** for culture-aware comparisons
4. **Cache string.Length** in loops to avoid repeated property access
5. **Use Span<T>** for high-performance scenarios without allocations
6. **Use string interpolation** ($"") for readability (few concatenations)
7. **Consider ReadOnlySpan<char>** for substring operations without allocation
8. **Use const for string literals** that won't change

```csharp
// Good: Use StringComparison
bool equal = str1.Equals(str2, StringComparison.OrdinalIgnoreCase);

// Good: Use Span for parsing without allocation
ReadOnlySpan<char> span = text.AsSpan(startIndex, length);
```

### Don'ts ✗
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

---

## ⚠️ Common Pitfalls

### Pitfall 1: String Concatenation in Loops
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

### Pitfall 2: String Comparison Issues
```csharp
// Wrong: Case-sensitive comparison
if (userInput == "Yes") { } // Fails for "yes", "YES"

// Correct: Case-insensitive comparison
if (userInput.Equals("Yes", StringComparison.OrdinalIgnoreCase)) { }
```

### Pitfall 3: Null Reference Exceptions
```csharp
// Wrong: Null reference exception if str is null
if (str.Length > 0) { }

// Correct: Check for null/empty
if (!string.IsNullOrEmpty(str)) { }
```

### Pitfall 4: Substring Index Errors
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

---

## 🎓 Interview Tips

### Common Questions
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

### Performance Considerations
- **String concatenation**: O(n) for each operation
- **StringBuilder.Append**: O(1) amortized
- **String.Contains**: O(n*m) worst case
- **String.IndexOf**: O(n*m) naive, optimized in .NET
- **String interning**: Saves memory but costs time for lookup

---

## 🧩 Summary

| Concept | Key Points |
|---------|------------|
| **Immutability** | Strings never change; operations create new strings |
| **StringBuilder** | Use for multiple concatenations, mutable |
| **Searching** | KMP O(n+m), Rabin-Karp O(n) average |
| **Common Patterns** | Two-pointer, sliding window, hash maps |
| **Performance** | Avoid concatenation in loops, use Span<T> |
| **Best Practices** | Use StringComparison, null checks, StringBuilder |

---

## 📚 Related Topics
- **Regular Expressions**: Pattern matching with Regex class
- **String Encoding**: UTF-8, UTF-16, ASCII conversions
- **Text Processing**: Parsing, tokenization, lexical analysis
- **Cryptography**: Hashing, encoding strings
- **Globalization**: Culture-aware string operations

---

*This guide provides comprehensive coverage of string manipulation in C# for technical interviews and practical development. Master these concepts and patterns for efficient string processing.*
