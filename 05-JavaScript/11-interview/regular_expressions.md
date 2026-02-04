# Regular Expressions (Regex) in JavaScript

## What are Regular Expressions?

**Regular Expressions** (regex or regexp) are patterns used to match character combinations in strings. They're powerful tools for:
- Searching text
- Validating input
- Extracting data
- Replacing text
- Parsing strings

---

## Creating Regular Expressions

### Two Ways to Create Regex

```javascript
// 1. Literal notation (preferred for static patterns)
const regex1 = /pattern/flags;

// 2. Constructor (useful for dynamic patterns)
const regex2 = new RegExp('pattern', 'flags');

// Examples
const literal = /hello/i;
const constructor = new RegExp('hello', 'i');
```

---

## Regex Flags

| Flag | Name | Description |
|------|------|-------------|
| `g` | Global | Find all matches (not just first) |
| `i` | Ignore case | Case-insensitive matching |
| `m` | Multiline | `^` and `$` match line starts/ends |
| `s` | Dot all | `.` matches newlines too |
| `u` | Unicode | Treat pattern as Unicode |
| `y` | Sticky | Match from `lastIndex` position |

```javascript
const text = 'Hello World, hello universe';

// No flags: first match only, case-sensitive
console.log(text.match(/hello/));    // ["hello"]

// g flag: all matches
console.log(text.match(/hello/g));   // ["hello"]

// i flag: case-insensitive
console.log(text.match(/hello/i));   // ["Hello"]

// gi flags: all matches, case-insensitive
console.log(text.match(/hello/gi));  // ["Hello", "hello"]
```

---

## Basic Patterns

### Literal Characters

```javascript
const regex = /cat/;

console.log(regex.test('cat'));        // true
console.log(regex.test('catch'));      // true
console.log(regex.test('dog'));        // false
```

### Special Characters (Need Escaping)

Special characters: `. * + ? ^ $ { } [ ] ( ) | \`

```javascript
// Escape with backslash
const dotRegex = /\./;
console.log(dotRegex.test('file.txt')); // true

const dollarRegex = /\$100/;
console.log(dollarRegex.test('$100'));  // true
```

---

## Character Classes

### Basic Character Classes

```javascript
// [abc] - Match any character in brackets
const vowels = /[aeiou]/;
console.log(vowels.test('hello')); // true

// [^abc] - Match any character NOT in brackets
const consonants = /[^aeiou]/i;
console.log(consonants.test('hello')); // true (h, l, l)

// [a-z] - Match range
const lowercase = /[a-z]/;
console.log(lowercase.test('Hello')); // true (e, l, l, o)

// [A-Za-z0-9] - Multiple ranges
const alphanumeric = /[A-Za-z0-9]/;
console.log(alphanumeric.test('test123')); // true
```

### Predefined Character Classes

| Pattern | Equivalent | Description |
|---------|------------|-------------|
| `\d` | `[0-9]` | Any digit |
| `\D` | `[^0-9]` | Any non-digit |
| `\w` | `[A-Za-z0-9_]` | Word character |
| `\W` | `[^A-Za-z0-9_]` | Non-word character |
| `\s` | `[ \t\n\r\f\v]` | Whitespace |
| `\S` | `[^ \t\n\r\f\v]` | Non-whitespace |
| `.` | Any character | Except newline (unless `s` flag) |

```javascript
// \d - digits
const hasDigit = /\d/;
console.log(hasDigit.test('abc123')); // true

// \w - word characters
const word = /\w+/;
console.log(word.test('hello_world')); // true

// \s - whitespace
const hasSpace = /\s/;
console.log(hasSpace.test('hello world')); // true

// . - any character
const anyChar = /.at/;
console.log(anyChar.test('cat')); // true
console.log(anyChar.test('bat')); // true
console.log(anyChar.test('mat')); // true
```

---

## Quantifiers

| Quantifier | Description |
|------------|-------------|
| `*` | 0 or more |
| `+` | 1 or more |
| `?` | 0 or 1 (optional) |
| `{n}` | Exactly n |
| `{n,}` | n or more |
| `{n,m}` | Between n and m |

```javascript
// * - 0 or more
const colors = /colou*r/;
console.log(colors.test('color'));   // true
console.log(colors.test('colour'));  // true
console.log(colors.test('colouur')); // true

// + - 1 or more
const digits = /\d+/;
console.log(digits.test('123'));     // true
console.log(digits.test('abc'));     // false

// ? - optional
const optional = /colou?r/;
console.log(optional.test('color'));  // true
console.log(optional.test('colour')); // true

// {n} - exactly n
const threeDigits = /\d{3}/;
console.log(threeDigits.test('12'));  // false
console.log(threeDigits.test('123')); // true

// {n,m} - range
const zipCode = /\d{5,9}/;
console.log(zipCode.test('12345'));    // true
console.log(zipCode.test('123456789')); // true
```

### Greedy vs Lazy Quantifiers

```javascript
const text = '<div>Hello</div><div>World</div>';

// Greedy (default): matches as much as possible
const greedy = /<div>.*<\/div>/;
console.log(text.match(greedy));
// ["<div>Hello</div><div>World</div>"]

// Lazy (add ?): matches as little as possible
const lazy = /<div>.*?<\/div>/;
console.log(text.match(lazy));
// ["<div>Hello</div>"]

// With global flag
console.log(text.match(/<div>.*?<\/div>/g));
// ["<div>Hello</div>", "<div>World</div>"]
```

---

## Anchors

| Anchor | Description |
|--------|-------------|
| `^` | Start of string (or line with `m` flag) |
| `$` | End of string (or line with `m` flag) |
| `\b` | Word boundary |
| `\B` | Not a word boundary |

```javascript
// ^ - start
const startsWithHello = /^Hello/;
console.log(startsWithHello.test('Hello World')); // true
console.log(startsWithHello.test('Say Hello'));   // false

// $ - end
const endsWithWorld = /World$/;
console.log(endsWithWorld.test('Hello World')); // true
console.log(endsWithWorld.test('World Hello')); // false

// \b - word boundary
const exactWord = /\bcat\b/;
console.log(exactWord.test('cat'));      // true
console.log(exactWord.test('catch'));    // false
console.log(exactWord.test('the cat')); // true

// Exact match (start and end)
const exactEmail = /^[a-z]+@[a-z]+\.[a-z]+$/;
console.log(exactEmail.test('user@example.com')); // true
console.log(exactEmail.test('user@example.com extra')); // false
```

---

## Groups and Capturing

### Capturing Groups `()`

```javascript
const dateRegex = /(\d{4})-(\d{2})-(\d{2})/;
const match = '2024-03-15'.match(dateRegex);

console.log(match[0]); // "2024-03-15" (full match)
console.log(match[1]); // "2024" (first group)
console.log(match[2]); // "03" (second group)
console.log(match[3]); // "15" (third group)
```

### Non-Capturing Groups `(?:)`

```javascript
// Capture
const withCapture = /(hello) (world)/;
console.log('hello world'.match(withCapture));
// ["hello world", "hello", "world"]

// Non-capture (for grouping without capturing)
const noCapture = /(?:hello) (world)/;
console.log('hello world'.match(noCapture));
// ["hello world", "world"]
```

### Named Capturing Groups `(?<name>)`

```javascript
const dateRegex = /(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})/;
const match = '2024-03-15'.match(dateRegex);

console.log(match.groups.year);  // "2024"
console.log(match.groups.month); // "03"
console.log(match.groups.day);   // "15"
```

---

## Alternation

### OR Operator `|`

```javascript
const fileExt = /\.(jpg|png|gif)$/;

console.log(fileExt.test('photo.jpg')); // true
console.log(fileExt.test('image.png')); // true
console.log(fileExt.test('file.txt'));  // false
```

---

## Lookahead and Lookbehind

### Positive Lookahead `(?=)`

Match if followed by pattern (doesn't include in match).

```javascript
// Match number followed by "px"
const pxValue = /\d+(?=px)/;

console.log('10px'.match(pxValue));  // ["10"]
console.log('10em'.match(pxValue));  // null
```

### Negative Lookahead `(?!)`

Match if NOT followed by pattern.

```javascript
// Match number NOT followed by "px"
const notPx = /\d+(?!px)/;

console.log('10em'.match(notPx));  // ["10"]
console.log('10px'.match(notPx));  // null
```

### Positive Lookbehind `(?<=)`

Match if preceded by pattern.

```javascript
// Match number preceded by "$"
const price = /(?<=\$)\d+/;

console.log('$100'.match(price));  // ["100"]
console.log('100'.match(price));   // null
```

### Negative Lookbehind `(?<!)`

Match if NOT preceded by pattern.

```javascript
// Match number NOT preceded by "$"
const notPrice = /(?<!\$)\d+/;

console.log('100'.match(notPrice));  // ["100"]
console.log('$100'.match(notPrice)); // null
```

---

## String Methods with Regex

### `test()` - Boolean Check

```javascript
const regex = /hello/i;

console.log(regex.test('Hello World')); // true
console.log(regex.test('Goodbye'));     // false
```

### `match()` - Find Matches

```javascript
const text = 'Contact: john@email.com and jane@email.com';
const emailRegex = /\w+@\w+\.\w+/g;

const matches = text.match(emailRegex);
console.log(matches);
// ["john@email.com", "jane@email.com"]
```

### `matchAll()` - Get All Match Details

```javascript
const text = 'test1 test2 test3';
const regex = /test(\d)/g;

const matches = [...text.matchAll(regex)];
matches.forEach(match => {
  console.log(`Full: ${match[0]}, Group: ${match[1]}`);
});
// Full: test1, Group: 1
// Full: test2, Group: 2
// Full: test3, Group: 3
```

### `search()` - Find Index

```javascript
const text = 'Hello World';

console.log(text.search(/World/));  // 6
console.log(text.search(/Goodbye/)); // -1
```

### `replace()` - Replace Matches

```javascript
const text = 'Hello World';

// Simple replacement
console.log(text.replace(/World/, 'Universe'));
// "Hello Universe"

// With function
console.log(text.replace(/\w+/g, match => match.toUpperCase()));
// "HELLO WORLD"

// With groups
const date = '2024-03-15';
const formatted = date.replace(/(\d{4})-(\d{2})-(\d{2})/, '$2/$3/$1');
console.log(formatted); // "03/15/2024"
```

### `replaceAll()` - Replace All Matches

```javascript
const text = 'cat cat cat';

console.log(text.replace(/cat/, 'dog'));     // "dog cat cat"
console.log(text.replaceAll(/cat/g, 'dog')); // "dog dog dog"
```

### `split()` - Split by Pattern

```javascript
const text = 'one,two;three:four';

console.log(text.split(/[,;:]/));
// ["one", "two", "three", "four"]

const words = 'hello   world    test';
console.log(words.split(/\s+/));
// ["hello", "world", "test"]
```

---

## Common Patterns

### Email Validation

```javascript
const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;

console.log(emailRegex.test('user@example.com'));    // true
console.log(emailRegex.test('invalid.email'));       // false
```

### Phone Number

```javascript
// US phone: (123) 456-7890 or 123-456-7890
const phoneRegex = /^\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}$/;

console.log(phoneRegex.test('(123) 456-7890')); // true
console.log(phoneRegex.test('123-456-7890'));   // true
console.log(phoneRegex.test('1234567890'));     // true
```

### URL

```javascript
const urlRegex = /^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b/;

console.log(urlRegex.test('https://example.com'));     // true
console.log(urlRegex.test('http://www.example.com'));  // true
```

### Password Strength

```javascript
// At least 8 chars, 1 uppercase, 1 lowercase, 1 number, 1 special char
const strongPassword = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/;

console.log(strongPassword.test('Password123!'));  // true
console.log(strongPassword.test('weak'));          // false
```

### Credit Card

```javascript
// Basic pattern (spaces optional)
const ccRegex = /^\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}$/;

console.log(ccRegex.test('1234 5678 9012 3456')); // true
console.log(ccRegex.test('1234-5678-9012-3456')); // true
```

### Hex Color

```javascript
const hexColor = /^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$/;

console.log(hexColor.test('#FFF'));      // true
console.log(hexColor.test('#FF5733'));   // true
console.log(hexColor.test('#GGG'));      // false
```

### IPv4 Address

```javascript
const ipv4 = /^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$/;

console.log(ipv4.test('192.168.1.1'));   // true
console.log(ipv4.test('256.1.1.1'));     // false
```

---

## Performance Tips

### 1. Compile Once, Use Many Times

```javascript
// ❌ Bad: Compiles regex every iteration
for (let i = 0; i < 1000; i++) {
  if (/test/.test(strings[i])) {
    // ...
  }
}

// ✅ Good: Compile once
const regex = /test/;
for (let i = 0; i < 1000; i++) {
  if (regex.test(strings[i])) {
    // ...
  }
}
```

### 2. Use Specific Patterns

```javascript
// ❌ Slower: Too general
const slow = /.*@.*/;

// ✅ Faster: More specific
const fast = /\w+@\w+\.\w+/;
```

### 3. Avoid Catastrophic Backtracking

```javascript
// ❌ Dangerous: Can hang on long strings
const dangerous = /(a+)+b/;

// ✅ Better: More efficient
const safe = /a+b/;
```

---

## Interview Questions

### Q1: Extract all numbers from a string

```javascript
function extractNumbers(str) {
  return str.match(/\d+/g) || [];
}

console.log(extractNumbers('Price: $123.45, Qty: 10'));
// ["123", "45", "10"]
```

### Q2: Validate username (alphanumeric, 3-16 chars, underscore allowed)

```javascript
const usernameRegex = /^[a-zA-Z0-9_]{3,16}$/;

console.log(usernameRegex.test('user_123'));  // true
console.log(usernameRegex.test('ab'));        // false (too short)
console.log(usernameRegex.test('user-name')); // false (dash not allowed)
```

### Q3: Replace multiple spaces with single space

```javascript
const text = 'hello    world     test';
console.log(text.replace(/\s+/g, ' '));
// "hello world test"
```

### Q4: Check if string contains only digits

```javascript
const onlyDigits = /^\d+$/;

console.log(onlyDigits.test('12345'));   // true
console.log(onlyDigits.test('123a45'));  // false
```

### Q5: Extract domain from email

```javascript
function getDomain(email) {
  const match = email.match(/@(.+)$/);
  return match ? match[1] : null;
}

console.log(getDomain('user@example.com')); // "example.com"
```

---

## Summary

### Basic Components
- **Literals**: Match exact characters
- **Character classes**: `[abc]`, `\d`, `\w`, `\s`
- **Quantifiers**: `*`, `+`, `?`, `{n,m}`
- **Anchors**: `^`, `$`, `\b`

### Advanced Features
- **Groups**: `()`, `(?:)`, `(?<name>)`
- **Lookahead/Lookbehind**: `(?=)`, `(?!)`, `(?<=)`, `(?<!)`
- **Alternation**: `|`
- **Flags**: `g`, `i`, `m`, `s`, `u`, `y`

### Methods
- `test()` - Check match
- `match()` - Get matches
- `matchAll()` - Get all match details
- `search()` - Find position
- `replace()` - Replace matches
- `split()` - Split string

### Best Practices
- ✅ Escape special characters
- ✅ Use specific patterns
- ✅ Compile regex once
- ✅ Test edge cases
- ❌ Avoid catastrophic backtracking
- ❌ Don't use regex for complex parsing (HTML, JSON)

### Interview Tips
- Know common patterns (email, phone, URL)
- Understand greedy vs lazy matching
- Be familiar with lookahead/lookbehind
- Practice with real-world validation scenarios
- Remember: Regex is powerful but can be slow

Master regex to become a string manipulation expert! 🚀
