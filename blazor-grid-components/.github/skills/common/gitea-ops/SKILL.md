---
name: gitea-ops
description: Runs Gitea CRUD-style operations from chat by securely using a Gitea token and executing a PowerShell script. Use when you need read, retrieve-all, create, update, close, or delete operations similar to MCP behavior.
metadata:
  author: Syncfusion Inc
  version: "1.1"
compatibility: Requires PowerShell 5.1+ (powershell.exe on Windows, pwsh on macOS/Linux) and network access to your Gitea instance.
---

# Gitea Ops Skill

This skill provides an MCP-like workflow using a PowerShell script that calls Gitea REST APIs.

## What This Skill Does

- Prompts for context values (base URL, owner, repository, operation)
- Uses a secure token input flow (environment variable preferred)
- Runs a PowerShell script for issue CRUD-style operations
- Returns structured JSON output suitable for agent summarization

## Supported Operations

### Issue Operations

| Operation | Behavior |
|-----------|----------|
| `list` | Retrieve all issues from a repository (default: top 50) |
| `get` | Retrieve one issue by number |
| `create` | Create an issue with title and optional body |
| `update` | Update issue title/body/state |
| `close` | Close an issue |
| `delete` | Delete an issue by number |

### Pull Request Operations

| Operation | Behavior |
|-----------|----------|
| `list-pr` | Retrieve all PRs (default: open/closed/all, limit 50) with optional state filter |
| `get-pr` | Retrieve PR metadata (title, description, author, branches, status) |
| `list-pr-files` | List changed files in a PR |
| `pr-diff` | Get the raw unified diff for a PR |
| `create-pr` | Create a new pull request from a head branch to a base branch |
| `create-review-comment` | Post an inline review comment on a PR file/line |
| `post-pr-comment` | Post a general comment on a PR |

## Security Rules

1. Never echo the token to output
2. Prefer token in env var `GITEA_TOKEN`
3. Do not store tokens in files or commit history
4. Require explicit confirmation before `delete`

## Inputs

| Input | Required | Notes |
|------|----------|-------|
| GiteaBaseUrl | Conditionally | Default: `https://gitea.syncfusion.com` |
| Owner | Conditionally | Default: `essential-studio` |
| Repository | Conditionally | Repository name. Default: `blazor-grid-components` |
| Operation | Yes | `list`, `get`, `create`, `update`, `close`, `delete`, `get-pr`, `list-pr-files`, `pr-diff`, `create-pr`, `create-review-comment`, `post-pr-comment` |
| IssueNumber | get/update/close/delete | Numeric issue number (must be > 0) |
| PullNumber | PR operations | Numeric PR number (must be > 0) |
| Title | create/update | Required for create |
| Body | Optional | Issue/comment description text |
| State | update/list-pr | `open`, `closed`, or `all` (default: `all` for list-pr) |
| FilePath | create-review-comment | File path for inline review comment |
| Line | create-review-comment | Line number for inline review comment |
| ReviewAction | create-review-comment | `APPROVED`, `REQUEST_CHANGES`, or `COMMENT` (default: `COMMENT`) |
| HeadBranch | create-pr | Source branch name (e.g., `fix/12345-null-crash`) |
| BaseBranch | create-pr | Target branch name (default: `development`) |
| Top | list/list-pr-files | Default `50` |

## Execution Steps

1. Ask for base URL, owner, repo, operation, and required fields.
2. If token is missing, check whether the environment variable is set:

```bash
# Check if the variable is set
printenv GITEA_TOKEN
```

If the variable is **not set**, do NOT ask the user to paste the token in the chat or terminal. Instead, instruct them to add it as a **persistent machine environment variable**:

**macOS / Linux:**
1. Open your shell profile (`~/.zshrc` for zsh or `~/.bashrc` for bash)
2. Add the line: `export GITEA_TOKEN='<your-gitea-token>'`
3. Save the file and run `source ~/.zshrc` (or `source ~/.bashrc`)
4. Restart VS Code to pick up the new variable

**Windows (Environment Variables UI):**
1. Open **Settings → System → About → Advanced system settings → Environment Variables**
2. Under **User variables**, click **New**
3. Set Variable name: `GITEA_TOKEN` and Variable value: `<your-gitea-token>`
4. Click OK and restart VS Code to pick up the new variable

**Windows (PowerShell — persistent):**
```powershell
[System.Environment]::SetEnvironmentVariable('GITEA_TOKEN', '<your-gitea-token>', 'User')
```
Then restart VS Code.

> ⚠️ **Security**: Never pass tokens directly in chat messages, agent prompts, or inline terminal commands — they may be logged or accidentally committed.

3. Optionally set reusable defaults:

```bash
export GITEA_BASE_URL='https://gitea.syncfusion.com'
export GITEA_OWNER='essential-studio'
export GITEA_REPO='blazor-grid-components'
```

4. Run the script with `powershell` (Windows) or `pwsh` (macOS/Linux).
5. For `delete`, ask for explicit confirmation before execution.
6. Summarize result from JSON output.

## Command Templates

**Note:** Use `powershell` on Windows, `pwsh` on macOS/Linux.

### Issue Operations

```powershell
# List issues
powershell .github/skills/gitea-ops/scripts/gitea.ps1 -Operation list -Top 50

# Read one issue
powershell .github/skills/gitea-ops/scripts/gitea.ps1 -Operation get -IssueNumber 123

# Create an issue
powershell .github/skills/gitea-ops/scripts/gitea.ps1 -Operation create -Title "New issue" -Body "Created from skill"

# Update an issue
powershell .github/skills/gitea-ops/scripts/gitea.ps1 -Operation update -IssueNumber 123 -Title "Updated title" -Body "Updated body"

# Close an issue
powershell .github/skills/gitea-ops/scripts/gitea.ps1 -Operation close -IssueNumber 123

# Delete an issue
powershell .github/skills/gitea-ops/scripts/gitea.ps1 -Operation delete -IssueNumber 123
```

### Pull Request Operations

```powershell
# List all PRs (all states, limit 50)
powershell .github/skills/gitea-ops/scripts/gitea.ps1 -Operation list-pr

# List only open PRs
powershell .github/skills/gitea-ops/scripts/gitea.ps1 -Operation list-pr -State open

# List only closed PRs (limit 100)
powershell .github/skills/gitea-ops/scripts/gitea.ps1 -Operation list-pr -State closed -Top 100

# Get PR metadata
powershell .github/skills/gitea-ops/scripts/gitea.ps1 -Operation get-pr -PullNumber 42

# List changed files in a PR
powershell .github/skills/gitea-ops/scripts/gitea.ps1 -Operation list-pr-files -PullNumber 42

# Get raw diff for a PR
powershell .github/skills/gitea-ops/scripts/gitea.ps1 -Operation pr-diff -PullNumber 42

# Post an inline review comment on a specific file and line
powershell .github/skills/gitea-ops/scripts/gitea.ps1 -Operation create-review-comment -PullNumber 42 -FilePath "src/drop-down-list/drop-down-list.ts" -Line 150 -Body "Consider using a guard clause here."

# Post a general comment on a PR
powershell .github/skills/gitea-ops/scripts/gitea.ps1 -Operation post-pr-comment -PullNumber 42 -Body "Overall the changes look good. See inline comments."

# Post a PR comment using the Bug template (RECOMMENDED)
powershell -Command "& { .github/skills/gitea-ops/scripts/gitea.ps1 -Operation post-pr-comment -PullNumber 42 -Body (Get-Content '.github/PULL_REQUEST_TEMPLATE/Bug.md' -Raw) }"

# Submit a review with APPROVED / REQUEST_CHANGES
powershell .github/skills/gitea-ops/scripts/gitea.ps1 -Operation create-review-comment -PullNumber 42 -Body "Approved with minor suggestions." -ReviewAction APPROVED

# Create a new pull request
powershell .github/skills/gitea-ops/scripts/gitea.ps1 -Operation create-pr -Title "Fix: Null crash on tab selection" -Body "Fixes bug #12345" -HeadBranch "fix/12345-null-crash" -BaseBranch "development"
```

## Using PR Templates

**Location:** `.github/PULL_REQUEST_TEMPLATE/`
- **Bug.md** - Template for bug fix PRs
- **Feature.md** - Template for feature implementation PRs

**Best Practice:**
1. Use appropriate template (Bug.md or Feature.md)
2. Fill template using PowerShell here-string (preserves formatting)
3. Complete all mandatory sections and checklists
4. Post to PR using `-Body` parameter

**Example Workflow (Bug Fix):**
```powershell
# Fill Bug template using here-string
$prDescription = @'
### Bug description
Timeline Month/Year views fail to render scrollbar when HeaderRows are dynamically configured.

### Root cause
ScheduleHeaderRows UpdateChildProperties was not calling RenderLayout() after updates.

Internal discussion: BUG-976209 (Azure DevOps)

### Solution description
Modified src/ScheduleHeaderRows.razor.cs: Added await RenderLayout().ConfigureAwait(true)

### Code Studio usage(Mandatory)
* Code Studio used: Yes
* Primary use: Tests + Bug fix / debugging help
* Outcome: Saved time

[... continue with all sections ...]
'@

# Post to PR
.github/skills/gitea-ops/scripts/gitea.ps1 -Operation 'post-pr-comment' -PullNumber 8 -Body $prDescription
```

**Template Sections (All PRs):**
- PR Type (Bug Fix, Feature, Documentation, Refactoring, Performance, Other)
- Reference (Task ID, Azure DevOps link, OpenSpec change path)
- Summary (1-3 sentence description)
- Changes made (bulleted list)
- Root cause (for bugs) / Solution approach (for features)
- Testing (checklist, test files, coverage percentages)
- Validation (comprehensive checklist including commit message length ≤70 chars)
- Files changed (source, tests, demos)
- Screenshots/Recordings (if applicable)
- Breaking changes (list or "None")
- Additional notes
- Reviewer checklist

**Mandatory Sections** (must complete):
- ✅ Reference section (Task ID required)
- ✅ Testing section (test files + coverage required)
- ✅ Validation checklist (all items checked)
- ✅ Files changed (list all modified files)

**Real Example**: See PR #8 for BUG-976209 in blazor-grid-components
- Posted detailed template covering all sections
- Included root cause analysis
- Listed all 3 commits with descriptions
- Documented skill improvements as bonus work
- Complete validation checklist

---

## Adding Labels to PRs

**Purpose**: Mark PRs with labels like 'cs:used' to indicate Code Studio/AI was used

**Add 'cs:used' label** (MANDATORY for all AI-assisted PRs):

```powershell
# Add cs:used label to indicate Code Studio was used
$token = $env:GITEA_TOKEN
$headers = @{ 
    Authorization = "token $token"
    Accept = "application/json"
    "Content-Type" = "application/json"
}
$body = '{"labels": ["cs:used"]}'
Invoke-RestMethod -Uri "https://gitea.syncfusion.com/api/v1/repos/essential-studio/blazor-grid-components/issues/<PR#>/labels" -Method Post -Headers $headers -Body $body
```

**Available labels**:
- `cs:used` (ID: 43280) - AI was used in this PR (color: #009800 green)
- Add other labels as needed for categorization

**Example**:
```powershell
# Add cs:used label to PR #8
$token = $env:GITEA_TOKEN
$headers = @{ 
    Authorization = "token $token"
    Accept = "application/json"
    "Content-Type" = "application/json"
}
$body = '{"labels": ["cs:used"]}'
Invoke-RestMethod -Uri "https://gitea.syncfusion.com/api/v1/repos/essential-studio/blazor-grid-components/issues/8/labels" -Method Post -Headers $headers -Body $body
```

**Result**: Label appears on PR with green color indicating Code Studio assistance

---

## Notes

- This skill is script-driven and not a true typed MCP tool.
- Supports both issue and pull request operations.
- The script path from the repo root is `.github/skills/gitea-ops/scripts/gitea.ps1`.
- **Default repository is `blazor-grid-components`** under `essential-studio` on `gitea.syncfusion.com`.
- Configured for Syncfusion Blazor Grid component development and PR workflow automation.
- You can extend it to releases, branches, and users with additional routes.
