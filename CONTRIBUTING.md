# Contributing to Technical Interview Guide

First off, thank you for considering contributing to the Technical Interview Guide! It's people like you that make this resource valuable for the entire community.

## 🎯 How Can I Contribute?

### 📝 Content Contributions

#### Adding New Content
- New interview questions with detailed answers
- Code examples and explanations
- Study guides and tutorials
- System design patterns
- Best practices and tips

#### Improving Existing Content
- Fixing typos and grammatical errors
- Improving explanations
- Adding missing information
- Updating outdated content
- Adding visual diagrams

#### Creating New Sections
- Propose new technology sections
- Create learning paths
- Develop practice exercises
- Write case studies

---

## 🔍 Before You Start

### Check Existing Content
1. Search existing [issues](https://github.com/VamsiRavala/technical-interview-guide/issues)
2. Check [pull requests](https://github.com/VamsiRavala/technical-interview-guide/pulls)
3. Review the [roadmap](README.md#roadmap)
4. Read this contribution guide completely

### Discuss Major Changes
For significant changes, please:
1. Open an issue first
2. Describe your proposal
3. Wait for feedback
4. Proceed after approval

---

## 🛠️ Getting Started

### 1. Fork & Clone

```bash
# Fork the repository on GitHub, then clone your fork
git clone https://github.com/YOUR-USERNAME/technical-interview-guide.git

# Add upstream remote
cd technical-interview-guide
git remote add upstream https://github.com/VamsiRavala/technical-interview-guide.git

# Verify remotes
git remote -v
```

### 2. Create a Branch

```bash
# Update your fork
git fetch upstream
git checkout main
git merge upstream/main

# Create a feature branch
git checkout -b feature/your-feature-name
# or
git checkout -b fix/your-fix-name
```

### 3. Make Your Changes

Follow our content guidelines (see below).

### 4. Commit Your Changes

```bash
# Stage your changes
git add .

# Commit with a descriptive message
git commit -m "Add: Detailed explanation of React Context API"
# or
git commit -m "Fix: Typo in Azure Functions guide"
# or
git commit -m "Update: React Hooks examples with TypeScript"
```

#### Commit Message Guidelines
- Use present tense ("Add feature" not "Added feature")
- Use imperative mood ("Move cursor to..." not "Moves cursor to...")
- Start with a verb (Add, Update, Fix, Remove, Refactor, etc.)
- Be specific and descriptive
- Reference issues and pull requests when relevant

Examples:
```
Add: JWT authentication guide for .NET
Fix: Broken links in microservices section
Update: React 18 concurrent features
Remove: Outdated Azure pricing information
Docs: Improve contributing guidelines
```

### 5. Push to Your Fork

```bash
git push origin feature/your-feature-name
```

### 6. Create a Pull Request

1. Go to your fork on GitHub
2. Click "Pull Request"
3. Select your feature branch
4. Fill out the PR template
5. Submit for review

---

## 📋 Content Guidelines

### Quality Standards

#### Accuracy
- ✅ Content must be technically accurate
- ✅ Verify all code examples work
- ✅ Test code with latest versions
- ✅ Cite sources when applicable
- ✅ Update version-specific content

#### Clarity
- ✅ Write clear, concise explanations
- ✅ Use simple language
- ✅ Define technical terms
- ✅ Include examples
- ✅ Use proper formatting

#### Structure
- ✅ Follow existing file structure
- ✅ Use consistent markdown formatting
- ✅ Include table of contents for long documents
- ✅ Add meaningful headings
- ✅ Use bullet points and lists

---

## 📝 Markdown Style Guide

### Headings

```markdown
# H1 - Main Title (One per file)
## H2 - Major Section
### H3 - Subsection
#### H4 - Sub-subsection
```

### Code Blocks

Use language-specific syntax highlighting:

```markdown
\```javascript
const greeting = "Hello, World!";
console.log(greeting);
\```

\```csharp
public class Program
{
    public static void Main()
    {
        Console.WriteLine("Hello, World!");
    }
}
\```
```

### Lists

```markdown
#### Unordered Lists
- Item 1
- Item 2
  - Sub-item 2.1
  - Sub-item 2.2
- Item 3

#### Ordered Lists
1. First item
2. Second item
3. Third item
```

### Tables

```markdown
| Column 1 | Column 2 | Column 3 |
|----------|----------|----------|
| Data 1   | Data 2   | Data 3   |
| Data 4   | Data 5   | Data 6   |
```

### Links

```markdown
[Link Text](URL)
[Internal Link](../path/to/file.md)
[Section Link](#section-heading)
```

### Images

```markdown
![Alt Text](path/to/image.png)
```

### Emphasis

```markdown
**Bold text**
*Italic text*
`Inline code`
```

---

## 📂 File Structure

### Naming Conventions

#### Files
- Use kebab-case: `my-new-file.md`
- Be descriptive: `react-context-api.md` not `context.md`
- Use numbers for ordered content: `01-basics.md`, `02-advanced.md`

#### Directories
- Use kebab-case: `my-new-section`
- Group related content logically
- Keep hierarchy shallow (max 3-4 levels)

### Organization

```
technology-section/
├── README.md                 # Section overview
├── 01-fundamentals.md        # Numbered files for ordered content
├── 02-advanced-topics.md
├── subsection/               # Group related files
│   ├── topic-1.md
│   └── topic-2.md
└── interview/                # Special purpose folders
    ├── questions.md
    └── examples.md
```

---

## 🧪 Code Examples

### Guidelines

1. **Working Code**: All examples must run without errors
2. **Comments**: Explain complex logic
3. **Best Practices**: Follow language conventions
4. **Complete**: Include necessary imports/setup
5. **Modern**: Use current syntax and features

### Example Template

```javascript
// Good Example:

/**
 * Fetches user data from an API
 * @param {string} userId - The user's unique identifier
 * @returns {Promise<Object>} User object
 */
async function fetchUser(userId) {
  try {
    const response = await fetch(`/api/users/${userId}`);
    
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    
    const user = await response.json();
    return user;
  } catch (error) {
    console.error('Error fetching user:', error);
    throw error;
  }
}

// Usage
const user = await fetchUser('123');
console.log(user);
```

### What to Avoid

```javascript
// Bad Example:

// No explanation
// No error handling
// No comments
function f(x) {
  return fetch('/api/' + x).then(r => r.json());
}
```

---

## 🎯 Interview Questions

### Format

```markdown
### Question: [Question Text]

**Difficulty:** [Easy/Medium/Hard]

**Topic:** [e.g., React Hooks, Async Programming, Azure Functions]

#### Answer:

[Detailed explanation]

#### Example:

\```language
[Code example if applicable]
\```

#### Key Points:
- Point 1
- Point 2
- Point 3

#### Follow-up Questions:
- Related question 1
- Related question 2
```

### Quality Checklist

- [ ] Question is clear and specific
- [ ] Answer is comprehensive
- [ ] Includes practical example
- [ ] Covers edge cases
- [ ] Mentions trade-offs
- [ ] Provides context
- [ ] Lists related concepts

---

## 🔍 Review Process

### What We Look For

1. **Accuracy**: Technical correctness
2. **Clarity**: Easy to understand
3. **Completeness**: All necessary information
4. **Consistency**: Matches existing style
5. **Value**: Adds meaningful content

### Review Timeline

- Initial review: Within 3-5 days
- Feedback and iteration: Ongoing
- Approval and merge: When all requirements met

### Getting Feedback

- Reviewers may request changes
- Address feedback promptly
- Ask questions if unclear
- Be patient and collaborative

---

## ❌ Common Mistakes to Avoid

### Content
- ❌ Copying content without attribution
- ❌ Unverified or incorrect information
- ❌ Poorly formatted code
- ❌ Broken links
- ❌ Missing context or explanation

### Process
- ❌ Not creating an issue first (for major changes)
- ❌ Large PRs with multiple unrelated changes
- ❌ Unclear commit messages
- ❌ Not responding to review feedback
- ❌ Force pushing after review started

### Code
- ❌ Code that doesn't run
- ❌ Missing dependencies
- ❌ No error handling
- ❌ Poor variable naming
- ❌ No comments for complex logic

---

## 🎨 Style Preferences

### Language
- Use clear, simple English
- Define acronyms on first use: "JWT (JSON Web Token)"
- Use active voice: "React updates the DOM" not "The DOM is updated by React"
- Be concise but complete

### Code Style

#### JavaScript/TypeScript
```javascript
// Use const by default, let when reassignment needed
const userName = "John";
let counter = 0;

// Use arrow functions for callbacks
items.map(item => item.value);

// Use template literals
console.log(`User: ${userName}`);
```

#### C#
```csharp
// Use PascalCase for public members
public class UserService { }

// Use camelCase for private fields
private readonly ILogger _logger;

// Use async suffix for async methods
public async Task<User> GetUserAsync(string id) { }
```

---

## 🏷️ Labels

We use labels to organize issues and PRs:

- `good first issue` - Good for newcomers
- `help wanted` - Extra attention needed
- `documentation` - Documentation improvements
- `bug` - Something isn't working
- `enhancement` - New feature or request
- `question` - Further information requested
- `duplicate` - Already exists
- `wontfix` - Won't be worked on

---

## 🤝 Community

### Code of Conduct

Be respectful, inclusive, and professional:

- ✅ Be welcoming and friendly
- ✅ Be respectful of differing opinions
- ✅ Accept constructive criticism gracefully
- ✅ Focus on what is best for the community
- ✅ Show empathy towards others

### Communication

- Be clear and concise
- Provide context
- Be patient
- Ask questions
- Help others

---

## 📚 Resources

### Markdown
- [Markdown Guide](https://www.markdownguide.org/)
- [GitHub Markdown](https://guides.github.com/features/mastering-markdown/)

### Git
- [Git Handbook](https://guides.github.com/introduction/git-handbook/)
- [Pro Git Book](https://git-scm.com/book/en/v2)

### Open Source
- [How to Contribute to Open Source](https://opensource.guide/how-to-contribute/)
- [First Contributions](https://github.com/firstcontributions/first-contributions)

---

## 📞 Questions?

- Open an [issue](https://github.com/VamsiRavala/technical-interview-guide/issues)
- Start a [discussion](https://github.com/VamsiRavala/technical-interview-guide/discussions)

---

## 🙏 Thank You!

Your contributions make this resource better for everyone. We appreciate your time and effort!

---

**Happy Contributing!** 🎉
