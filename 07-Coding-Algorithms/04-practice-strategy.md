# Practice Strategy & Study Plan

> **For a 13+ year senior engineer, the goal isn't to "learn to code" — it's to rebuild interview reflexes you haven't used in a decade.** This is a deliberate-practice problem, not a knowledge problem. The plan below is structured, measurable, and pattern-driven, because pattern recognition is what actually transfers to interview day.

---

## Table of Contents

1. [The Senior Engineer's Mindset](#1-the-senior-engineers-mindset)
2. [How Many Problems & At What Difficulty](#2-how-many-problems--at-what-difficulty)
3. [The ~150 Core Problems by Pattern](#3-the-150-core-problems-by-pattern)
4. [The Week-by-Week Plan](#4-the-week-by-week-plan)
5. [How to Study a Problem You Can't Solve](#5-how-to-study-a-problem-you-cant-solve)
6. [Spaced Repetition & Retention](#6-spaced-repetition--retention)
7. [Timed Practice](#7-timed-practice)
8. [Mock Interviews](#8-mock-interviews)
9. [Tracking Progress](#9-tracking-progress)
10. [Weekly Cadence (Working Professional)](#10-weekly-cadence-working-professional)

---

## 1. The Senior Engineer's Mindset

You have advantages and liabilities. Be honest about both.

| Your advantage | Your liability |
|----------------|----------------|
| You read and reason about code fast | You haven't hand-written a heap or DFS in years |
| You debug systematically | You over-engineer simple problems |
| You communicate trade-offs naturally | You under-practice the "boring" fundamentals |
| You recognize real-world analogues | Rust on recursion, pointer manipulation, edge cases |

**Three rules that matter more for you than for a new grad:**

1. **Patterns over problems.** You don't have time to grind 500 problems. Master ~150 problems that cover every pattern, and you can solve the other 1,500 by recognition.
2. **Speed is the gap, not capability.** You can solve most mediums — but can you solve them *clean, bug-free, in 30 minutes, while talking*? That's the skill to build.
3. **Don't skip Easy.** Seniors skip Easy out of ego, then fumble a trivial pointer bug in the real interview. Easy problems build the muscle memory that lets your brain spend its budget on the hard part.

---

## 2. How Many Problems & At What Difficulty

### Target volume

| Profile | Total problems | Easy | Medium | Hard |
|---------|---------------|------|--------|------|
| **Minimum viable** | 150 | 30 (20%) | 90 (60%) | 30 (20%) |
| **Solid (recommended)** | 200 | 40 (20%) | 120 (60%) | 40 (20%) |
| **Thorough** | 300 | 60 (20%) | 180 (60%) | 60 (20%) |

> **Medium is your bread and butter.** ~80% of FAANG coding-round questions are medium. Spend the majority of your time there. Hards are for stretching and for the occasional Google/Meta E6+ round — don't over-invest until mediums are automatic.

### Quality bar — a problem isn't "done" until

- [ ] You solved it (or studied it) and understand *why* the approach works
- [ ] You can re-derive the approach from the pattern, not from memory
- [ ] You wrote clean, compiling C# with good names
- [ ] You handled edge cases (empty, single, duplicates, overflow, null)
- [ ] You stated time **and** space complexity
- [ ] You scheduled a re-review (see [spaced repetition](#6-spaced-repetition--retention))

**200 truly-mastered problems beats 500 superficially-touched ones.**

---

## 3. The ~150 Core Problems by Pattern

These map 1:1 to the patterns in [05-coding-patterns.md](05-coding-patterns.md). Solve them in pattern blocks — your brain generalizes far better when problems cluster.

| Pattern | # | Representative problems (LeetCode) |
|---------|---|-----------------------------------|
| **Two Pointers** | 8 | Two Sum II, 3Sum, Container With Most Water, Trapping Rain Water, Valid Palindrome, Remove Duplicates, Sort Colors, 4Sum |
| **Sliding Window** | 8 | Longest Substring Without Repeating, Min Window Substring, Longest Repeating Char Replacement, Permutation in String, Max Sum Subarray of Size K, Fruit Into Baskets, Sliding Window Maximum, Subarrays w/ K Distinct |
| **Fast & Slow Pointers** | 5 | Linked List Cycle, Cycle II, Happy Number, Middle of Linked List, Palindrome Linked List |
| **Merge Intervals** | 5 | Merge Intervals, Insert Interval, Non-overlapping Intervals, Meeting Rooms II, Interval List Intersections |
| **Cyclic Sort** | 4 | Missing Number, Find All Numbers Disappeared, Find the Duplicate, First Missing Positive |
| **In-place Linked List Reversal** | 5 | Reverse Linked List, Reverse Linked List II, Reverse Nodes in k-Group, Swap Nodes in Pairs, Rotate List |
| **BFS (Trees/Graphs)** | 8 | Level Order Traversal, Zigzag Level Order, Right Side View, Min Depth, Rotting Oranges, Word Ladder, 01 Matrix, Min Knight Moves |
| **DFS (Trees/Graphs)** | 10 | Max Depth, Path Sum, Diameter, Number of Islands, Clone Graph, Course Schedule, Pacific Atlantic, All Paths Source→Target, Surrounded Regions, Validate BST |
| **Two Heaps** | 4 | Find Median from Data Stream, Sliding Window Median, IPO, Find Right Interval |
| **Subsets / Backtracking** | 10 | Subsets, Subsets II, Permutations, Permutations II, Combinations, Combination Sum, Combination Sum II, Letter Combinations, Generate Parentheses, Palindrome Partitioning |
| **Modified Binary Search** | 8 | Search in Rotated Sorted Array, Find Min in Rotated, Search a 2D Matrix, Find Peak Element, First Bad Version, Koko Eating Bananas, Find First/Last Position, Median of Two Sorted Arrays |
| **Top-K (Heap)** | 6 | Kth Largest Element, Top K Frequent Elements, K Closest Points to Origin, Sort Characters by Frequency, Kth Largest in Stream, Reorganize String |
| **K-way Merge** | 4 | Merge k Sorted Lists, Kth Smallest in Sorted Matrix, Smallest Range Covering K Lists, Find K Pairs Smallest Sums |
| **Topological Sort** | 5 | Course Schedule, Course Schedule II, Alien Dictionary, Minimum Height Trees, Sequence Reconstruction |
| **Dynamic Programming** | 20 | Climbing Stairs, House Robber, House Robber II, Coin Change, Coin Change II, Longest Increasing Subsequence, LCS, Edit Distance, 0/1 Knapsack, Partition Equal Subset Sum, Word Break, Decode Ways, Unique Paths, Min Path Sum, Max Product Subarray, Longest Palindromic Substring, Maximal Square, Best Time to Buy/Sell Stock (II/III), Burst Balloons |
| **Trie** | 4 | Implement Trie, Add & Search Word, Word Search II, Replace Words |
| **Union-Find** | 5 | Number of Connected Components, Redundant Connection, Accounts Merge, Number of Islands II, Most Stones Removed |
| **Greedy / Misc** | 6 | Jump Game, Jump Game II, Gas Station, Task Scheduler, Partition Labels, Merge Triplets |
| **Bit Manipulation** | 5 | Single Number, Number of 1 Bits, Counting Bits, Missing Number, Reverse Bits |
| **Matrix / Simulation** | 5 | Set Matrix Zeroes, Spiral Matrix, Rotate Image, Game of Life, Word Search |

**Total: ~149 problems** spanning every interview pattern. This is the spine of your preparation.

---

## 4. The Week-by-Week Plan

This is the 12-week plan referenced by the [section README](README.md), expanded into actionable weekly goals. (For limited time, stretch each week into ~1.5 weeks for a 16–20 week pace.)

### Phase 1 — Foundations (Weeks 1–4): topic-focused

| Week | Focus | Patterns | Problems | Mix |
|------|-------|----------|----------|-----|
| **1** | Arrays, Strings, Hashing | Two Pointers, Sliding Window | 20–25 | 60E/30M/10H |
| **2** | Linked Lists, Stacks, Queues | Fast/Slow, Reversal, Monotonic Stack | 20–25 | 50E/40M/10H |
| **3** | Trees, BST | BFS, DFS, traversals | 20–25 | 40E/50M/10H |
| **4** | Binary Search, Sorting | Modified Binary Search, Cyclic Sort | 20–25 | 40E/50M/10H |

**Phase 1 goal:** rebuild muscle memory. By end of week 4 you should write a clean BFS, DFS, binary search, and linked-list reversal from memory without hesitation.

### Phase 2 — Advanced Patterns (Weeks 5–8): pattern-focused

| Week | Focus | Patterns | Problems | Mix |
|------|-------|----------|----------|-----|
| **5** | Graphs | BFS/DFS on graphs, Topological Sort, Union-Find | 20–25 | 20E/60M/20H |
| **6** | Dynamic Programming I | 1-D DP, knapsack, climbing-stairs family | 20–25 | 10E/60M/30H |
| **7** | Dynamic Programming II + Backtracking | 2-D DP (LCS/edit distance), subsets/permutations | 20–25 | 10E/60M/30H |
| **8** | Heaps, Tries, Intervals | Top-K, Two Heaps, K-way Merge, Merge Intervals, Trie | 20–25 | 20E/60M/20H |

**Phase 2 goal:** master complex patterns. By end of week 8 you should *recognize* which pattern a problem wants within 2–3 minutes of reading it.

### Phase 3 — Polish & Simulation (Weeks 9–12): mixed + company-focused

| Week | Focus | Activity | Problems | Mix |
|------|-------|----------|----------|-----|
| **9** | Mixed random sets | No category hints; force recognition | 15–20 | 10E/50M/40H |
| **10** | Company top-50 lists | Target your real companies | 15–20 | 10E/50M/40H |
| **11** | Hard problems + weak areas | Attack lowest-confidence patterns | 12–18 | 0E/40M/60H |
| **12** | Full mock simulations | 2–3 mocks; re-do failed problems | 10–15 | 10E/40M/50H |

**Phase 3 goal:** interview-ready speed, accuracy, and composure under time pressure.

### Readiness checkpoints

- [ ] **End of Week 4:** core data-structure operations are automatic
- [ ] **End of Week 8:** can name the pattern for any medium in <3 min
- [ ] **End of Week 12:** solve a fresh medium clean in ≤30 min, talking the whole time

---

## 5. How to Study a Problem You Can't Solve

The single highest-leverage skill in prep. Most people either give up too early or look at the solution too early. Use this protocol:

```
1. ATTEMPT (20–30 min, hard cap)
   - Brute force first. A working brute force beats a broken "optimal."
   - Write down every idea, even bad ones. Note WHERE you got stuck.

2. HINT, DON'T SOLVE (5 min)
   - Read ONLY the topic tag or the first hint. Not the full solution.
   - Ask: "What pattern is this?" Try again with that lens.

3. STUDY THE SOLUTION (deliberately)
   - Read the editorial / top solution. Understand WHY, not just WHAT.
   - Identify the KEY INSIGHT — the one observation that unlocks it.
   - Ask: "What signal in the problem should have told me to use this?"

4. CLOSE THE BOOK & RE-IMPLEMENT
   - Solve it again from scratch, no reference. If you can't, you didn't
     understand it — repeat step 3.

5. WRITE A ONE-LINE TAKEAWAY
   - e.g., "When asked for kth largest, reach for a min-heap of size k."
   - This line is what you'll review during spaced repetition.

6. SCHEDULE A REVISIT (1 day, 1 week, 1 month)
```

> **The "key insight" log is gold.** Keep a running list of one-line insights ("subarray sum → prefix sums + hashmap"). Reviewing 150 of these the night before an interview is worth more than re-solving 10 problems.

---

## 6. Spaced Repetition & Retention

Solving a problem once buys you almost nothing two weeks later. Schedule re-reviews at expanding intervals.

| Review | Timing after first solve | What to do |
|--------|--------------------------|-----------|
| **R1** | +1 day | Re-solve from scratch (or recall the insight) |
| **R2** | +1 week | Re-solve; if smooth, mark "confident" |
| **R3** | +1 month | Quick recall; re-solve only if shaky |

### A simple confidence-tagging system

| Tag | Meaning | Action |
|-----|---------|--------|
| 🔴 Red | Couldn't solve, needed full solution | Re-attempt in 1 day |
| 🟡 Yellow | Solved with hints / slow / buggy | Re-attempt in 3 days |
| 🟢 Green | Solved clean within time | Review in 1 week, then 1 month |

Only problems that hit 🟢 twice in a row "graduate." Each week, your first 30–45 minutes should be clearing the day's due re-reviews *before* touching new problems.

---

## 7. Timed Practice

Capability without speed fails real interviews. Introduce the clock progressively.

| Phase | Timing rule |
|-------|-------------|
| Weeks 1–4 | Untimed for new problems; build correctness first |
| Weeks 5–8 | Soft timer: 35 min medium, 50 min hard. Note overruns. |
| Weeks 9–12 | Hard timer: if you blow the cap, stop, study, redo timed |

### The interview clock (45–60 min round)

| Phase | Time | What you're doing |
|-------|------|-------------------|
| Clarify | 2–3 min | Questions, constraints, examples |
| Approach | 5–7 min | Brute force → optimal, get buy-in |
| Code | 15–25 min | Clean C#, talking aloud |
| Test | 5–7 min | Trace examples, edge cases |
| Optimize / discuss | 5–10 min | Complexity, trade-offs, follow-ups |

> **Practice the *narration*, not just the code.** Solving silently in 20 minutes won't prepare you to solve while explaining in 30. Talk out loud even when practicing alone.

---

## 8. Mock Interviews

Mocks are where prep becomes performance. (The [section README](README.md) covers platforms — Pramp free, interviewing.io paid — in depth.) Cadence:

| Phase | Mocks/week | Purpose |
|-------|-----------|---------|
| Weeks 1–4 | 0–1 | Light; get comfortable talking |
| Weeks 5–8 | 1–2 | Build the clarify→code→test loop |
| Weeks 9–12 | 2–3 | Full pressure, fresh problems, real feedback |

**After every mock, log:** the problem, what went well, where you stalled, communication notes, and one concrete fix for next time. Re-implement any problem you stumbled on the same day.

---

## 9. Tracking Progress

Use a simple spreadsheet. The act of tracking is itself a forcing function.

| Column | Example | Why |
|--------|---------|-----|
| # / Name | 53. Max Subarray | identify |
| Pattern | Kadane / DP | cluster by pattern |
| Difficulty | Medium | balance your mix |
| Date solved | 2026-06-14 | schedule reviews |
| Time taken | 22 min | track speed |
| Confidence | 🟢 / 🟡 / 🔴 | spaced repetition |
| Next review | 2026-06-21 | retention |
| Key insight | "running sum, reset at <0" | night-before review |

### Weekly self-review checklist

- [ ] Did I hit my problem count and difficulty mix?
- [ ] Are my due re-reviews cleared (no growing backlog of 🔴)?
- [ ] Which pattern is my weakest? (Schedule extra next week.)
- [ ] Is my median solve time trending down?
- [ ] Did I do my mock(s) and log feedback?

### You're ready when

- ✅ Medium problems solved clean in 25–35 min, consistently
- ✅ Can code on a whiteboard / shared doc without IDE autocomplete
- ✅ State time & space complexity instantly with justification
- ✅ Mock feedback is consistently positive
- ✅ 150+ problems mastered, 50+ revisited to 🟢
- ✅ You recognize the pattern of a fresh medium within 3 minutes

---

## 10. Weekly Cadence (Working Professional)

A realistic rhythm for someone with a full-time senior job and family obligations.

| Day | Time | Focus |
|-----|------|-------|
| Mon | 1.5–2 hr | Clear due re-reviews + 2 new (1 medium, 1 easy warm-up) |
| Tue | 1.5–2 hr | 2 new mediums (current week's pattern) |
| Wed | 1.5–2 hr | 1 new medium + study 1 problem you couldn't solve |
| Thu | 1.5–2 hr | 1 mock OR 2 mediums |
| Fri | 1 hr | Light: review key-insight log, 1 easy |
| Sat | 3–4 hr | Deep block: 3–4 problems incl. 1 hard; weekly self-review |
| Sun | rest / 1 hr | Optional light recall; plan next week |

**Total: ~12–15 hours/week.** Sustainable, compounding, and enough to be interview-ready in 12–16 weeks.

> **Consistency beats intensity.** Two focused hours daily for 12 weeks crushes weekend cramming. Treat it like the gym — show up even on low-energy days, even if it's just clearing reviews.

---

## Summary

| Principle | The senior-engineer version |
|-----------|----------------------------|
| Patterns over volume | ~150 pattern-clustered problems, not 500 random |
| Quality over quantity | A problem isn't done until you can re-derive it |
| Speed is the real gap | Time everything from week 5 |
| Retention is engineered | Spaced repetition with confidence tags |
| Performance ≠ capability | Mocks + narration, not silent solving |
| Track to stay honest | A spreadsheet is a forcing function |

**Next:** [05-coding-patterns.md](05-coding-patterns.md) — the canonical interview patterns with C# templates and example problems.

*"Don't practice until you get it right. Practice until you can't get it wrong."*
