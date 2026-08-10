---
name: token-optimization
description: "Use when: reading large files, exploring any codebase, generating long documents, or operating inside any AI-assisted development session. Minimizes token usage through strategic tool selection, parallel operations, context consolidation, and disciplined output generation."
---

# Skill: Token Optimization

**Context window ≈ 200K tokens — budget it like memory. Every token spent on input is a token unavailable for reasoning and output.**

---

## Why This Matters in AI Development

Every AI coding session operates inside a finite context window. Poor token habits cause:
- 🔴 **Context overflow** — critical earlier content gets dropped mid-session
- 🔴 **Slower responses** — larger prompts take longer to process
- 🔴 **Higher cost** — token usage directly drives API billing
- 🔴 **Degraded quality** — bloated context dilutes model attention on what matters

Applying these principles keeps sessions sharp, fast, and accurate from start to finish.

---

## Tool Selection: Pick the Right Tool First

Before reading anything, choose the most targeted tool available:

| Goal | Right Tool | Wrong Tool |
|------|-----------|------------|
| Find where a symbol is defined | `grep_search("SymbolName")` | `read_file` on multiple files |
| Find files matching a pattern | `file_search("**/*.ext")` | `list_dir` recursively |
| Read a known small file (<500 lines) | `read_file` (full) | read in chunks |
| Explore an unknown large file | `grep_search` for structure → targeted `read_file` | read from line 1 |
| Find all usages of a symbol | `list_code_usages("SymbolName")` | manual grep across files |
| Explore directory structure | `list_dir` (parallel for multiple dirs) | read every file inside |
| Check build/lint errors | `get_errors` | read log files manually |
| Understand a changed file | `get_changed_files` | re-read entire file |

---

## Principle 1: Search Before Read

Always locate the exact target before reading.

❌ BAD — reads the entire file hunting for one method
```
read_file(LargeService.cs)   → 2000 lines = ~6000 tokens
```

✅ GOOD — find it first, then read only the relevant section
```
grep_search("ProcessOrder") → line 312 in OrderService.cs
read_file(OrderService.cs, offset: 308, limit: 40)
Cost: ~200 tokens
```

**Universal search-first pattern:**
```
grep_search("TargetMethod|TargetClass|TargetInterface", isRegexp: true)
→ Identify the 1–2 relevant files
→ read_file only those, only the relevant ranges
```

---

## Principle 2: Read Large Ranges, Not Micro-Chunks

❌ BAD — many small reads multiply round-trip overhead
```
read_file(Service.cs, 1, 50)    → 150 tokens
read_file(Service.cs, 51, 100)  → 150 tokens
read_file(Service.cs, 101, 150) → 150 tokens
Total: 3 calls + 450 tokens
```

✅ GOOD — one large read is cheaper in both tokens and latency
```
read_file(Service.cs, 1, 200)
Total: 1 call + ~380 tokens
```

**Reading strategy by file size:**

| File Size | Strategy |
|-----------|----------|
| < 500 lines | `read_file` entire file (no offset/limit needed) |
| 500–2000 lines | `grep_search` for structure → `read_file` targeted ranges (200+ lines each) |
| > 2000 lines | `grep_search` first → 2–3 large targeted reads only |

---

## Principle 3: Parallelize Independent Operations

Never make sequential calls when operations are independent.

❌ BAD — sequential tool calls waste wall-clock time and context overhead
```
1. list_dir(src/)
2. list_dir(tests/)
3. list_dir(docs/)
4. read_file(config.json)
```

✅ GOOD — fire all independent calls in a single block
```
list_dir(src/)         ─┐
list_dir(tests/)        ├─ all at once
list_dir(docs/)         │
read_file(config.json) ─┘
Cost: same tokens, 1 round trip
```

**Always parallelize:**
- Multiple `list_dir` calls across different directories
- Multiple `read_file` calls on different files
- `grep_search` + `read_file` when the grep target file is already known
- Reading a spec/doc file alongside its corresponding source file

---

## Principle 4: Consolidate Context — Read Once, Reference Often

❌ BAD — reading the same file twice doubles its token cost
```
[Step A] read_file(UserService.cs)
[Step B] read_file(UserService.cs)   ← forgot it was already read
```

✅ GOOD — retain and reference prior reads
```
[Step A] read_file(UserService.cs) — note key line numbers
[Step B] "Per earlier read of UserService.cs, line 87 shows..."
```

**Before reading any file**, ask: *"Did I already read this in this session?"*
If yes → reference it. If the context is too old → re-read only the relevant range.

---

## Principle 5: Use grep_search for File Overviews

Avoid reading entire files just to understand their structure.

```
# Get all public methods in a source file
grep_search("public (async Task|void|bool|string|int)", isRegexp: true, includePattern: "src/MyService.cs")

# Find all class/interface declarations in a directory
grep_search("^(public|internal) (class|interface|abstract)", isRegexp: true, includePattern: "src/")

# Find all test methods in a test project
grep_search("\\[Test\\]|\\[Fact\\]|\\[TestMethod\\]", isRegexp: true, includePattern: "tests/")

# Find dependency injection registrations
grep_search("services\\.Add|builder\\.Services", isRegexp: true, includePattern: "src/")

# Find all configuration keys used
grep_search("Configuration\\[|GetSection\\(|GetValue<", isRegexp: true, includePattern: "src/")
```

---

## Principle 6: Load Only Relevant Context Files

Projects often contain many spec, doc, or skill files. **Never load all of them upfront.**

**Token cost reality:**
- One 200-line document ≈ 600 tokens
- Ten documents loaded unnecessarily = 6,000+ tokens wasted

**Decision framework — which files to load:**

| Task Type | Load |
|-----------|------|
| Bug fix in a module | Source file for that module + its test file |
| New feature | Design doc for that feature area + 1 related existing implementation |
| Writing tests | 1 existing test file as template + the source under test |
| API / integration work | API contract/spec file + the integration handler |
| Infrastructure / CI | Deployment or config files relevant to the change |
| Cross-cutting concern | Architecture doc for that concern only |

**Rule**: Always find a "selector" or index file first (e.g., `README.md`, `spec-selector.md`, `docs/index.md`) — it's small and points to the 1–2 relevant documents to load.

---

## Principle 7: Skill / Agent Files — Load on Demand

Skill and agent definition files are large. Never load all of them at session start.

| Scenario | Skills to load |
|----------|---------------|
| Bug fix | Skill specific to that bug domain |
| Feature implementation | Skill for the feature area |
| Writing tests | Testing skill only |
| Source control operations | VCS / git skill |
| Work item tracking | `azure-devops` skill |
| Token-heavy session | `token-optimization` (this file) |

**Never pre-load all skills** — load each skill only when that phase of work begins.

---

## Principle 8: Trim Output — Be Precise, Not Verbose

Every word generated also consumes context for subsequent turns.

✅ DO
- Use bullet points instead of long prose paragraphs
- Reference file paths and line numbers instead of copying file content
- Summarize what was found; don't echo entire tool outputs back
- Generate only the code sections that changed, not the entire file

❌ DON'T
- Inline entire existing files in responses
- Repeat the same boilerplate pattern multiple times verbatim
- Narrate every tool call with lengthy preamble
- Copy large spec or doc sections into the chat output

**Example — generating multiple test methods:**
```
# GOOD: Read 1 existing test as structural template
read_file(tests/ExistingFeatureTests.cs)
→ Apply the pattern to all new test methods
→ Don't re-read the spec before writing each individual test
```

---

## Principle 9: Delegate Broad Research to Subagents

When a task requires searching across many unknown files, delegate it.

✅ Good subagent task (uncertain scope, many files)
```
"Search all source files under src/ and return:
 - All classes that implement IRepository
 - The file path and line number of each implementation
 - Any implementation that overrides SaveChangesAsync"
```

❌ Bad subagent task (simple, known target — do it yourself)
```
"Read UserRepository.cs and find the GetByIdAsync method"
```

**Delegate when:**
- Searching > 10 files with uncertain locations
- Finding patterns across an entire codebase
- Research tasks where the answer location is unknown

---

## Parallel Exploration Template

Standard pattern for understanding any unfamiliar area quickly:

```
Step 1 — Orient (all parallel):
  list_dir(src/)
  list_dir(tests/)
  grep_search("TargetFeature|RelatedKeyword", isRegexp: true, includePattern: "src/")

Step 2 — Focus (parallel read of 2–3 key files found in Step 1):
  read_file(src/RelevantModule.cs)
  read_file(tests/RelevantModuleTests.cs)

Step 3 — Deep dive (only if needed after Step 2):
  read_file(src/RelevantModule.cs, offset: <specific range>)
```

---

## Token Budget Reference

| Content Type | Approximate Tokens |
|-------------|-------------------|
| 100-line source file | ~300 |
| 500-line source file | ~1,500 |
| 1,000-line source file | ~3,000 |
| 2,000-line source file | ~6,000 |
| Spec / doc file (200 lines) | ~600 |
| `grep_search` result (10 matches) | ~500 |
| `git diff` (50 lines changed) | ~400 |
| Skill file (~200 lines) | ~600 |
| `list_dir` result (30 entries) | ~200 |
| Typical AI tool call + response overhead | ~100–300 |

**Total context window**: ~200K tokens
**Practical working budget**: ~80K tokens input (reserve remainder for reasoning + output)
**Warning threshold**: If input context exceeds ~100K tokens, aggressively prune and summarize.

---

## Anti-Patterns: Never Do These

| Anti-Pattern | Why It Hurts | Fix |
|-------------|-------------|-----|
| Read a large file line-by-line | 2,000 lines = 6K tokens | `grep_search` to locate, then targeted read |
| Load all spec/doc files upfront | 10+ files = 10K+ wasted tokens | Load only what the current task needs |
| Load all skills at session start | 6K+ tokens before any work begins | Load skills on demand |
| Re-read a file already in context | Doubles its token cost | Reference prior read by line number |
| Sequential `list_dir` calls | Slow and wasteful | Parallelize all independent calls |
| `read_file` to search for a symbol | Wrong tool entirely | Use `grep_search` or `list_code_usages` |
| Echo entire tool output in chat | Inflates context unnecessarily | Summarize the relevant finding only |
| Inline entire file in generated output | Massive unnecessary tokens | Reference path + show only changed sections |
| Repeat boilerplate N times verbatim | N× token waste | Parameterize; generate once and apply pattern |

---

## Session Start Checklist

Before starting any task:

- [ ] Do I know which files are involved? → `grep_search` first if not
- [ ] Do I need a doc or spec? → Find index/selector file, load 1–2 max
- [ ] Do I need a skill? → Load only the skill relevant to the current step
- [ ] Can I parallelize my first reads? → Yes — always do it
- [ ] Have I already read these files this session? → Reference, don't re-read
- [ ] Am I about to generate a large output? → Use templates and references, not verbatim copies

---

## Universal grep_search Patterns (Any Codebase)

```
# Find class and interface declarations
grep_search("^(public|internal|private).*(class|interface|abstract class|record)", isRegexp: true)

# Find method definitions (C# / Java style)
grep_search("(public|private|protected|internal).*(async )?(Task|void|bool|string|int|List)", isRegexp: true)

# Find dependency injection registrations
grep_search("services\\.Add|AddSingleton|AddScoped|AddTransient", isRegexp: true)

# Find configuration/environment variable access
grep_search("Environment\\.GetEnvironmentVariable|Configuration\\[|GetSection", isRegexp: true)

# Find test methods (xUnit / NUnit / MSTest)
grep_search("\\[Fact\\]|\\[Theory\\]|\\[Test\\]|\\[TestMethod\\]", isRegexp: true)

# Find API route declarations
grep_search("\\[Route|\\[HttpGet|\\[HttpPost|\\[HttpPut|\\[HttpDelete", isRegexp: true)

# Find TODO / FIXME markers
grep_search("TODO|FIXME|HACK|BUG", isRegexp: true)

# Find imports / using statements for a specific namespace
grep_search("using MyNamespace|import { MyModule", isRegexp: true)

# Find all files that reference a specific type or symbol
grep_search("IMyInterface|MyBaseClass|MyEnum", isRegexp: true)
```
