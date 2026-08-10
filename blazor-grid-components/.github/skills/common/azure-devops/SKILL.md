---
name: azure-devops
description: Runs Azure DevOps CRUD-style operations from chat by securely prompting for an Azure DevOps Personal Access Token (PAT) and executing a PowerShell script. Use when you need read, retrieve-all, create, update, or delete operations similar to MCP behavior.
metadata:
  author: Syncfusion Inc
  version: "1.1"
compatibility: Requires PowerShell 5.1+ (powershell.exe on Windows, pwsh on macOS/Linux) and network access to dev.azure.com.
---

# Azure DevOps Skill

This skill provides an MCP-like workflow using a PowerShell script that calls Azure DevOps REST APIs.

## What This Skill Does

- Prompts for context values (organization, project, operation)
- Uses a secure PAT input flow (environment variable preferred)
- Runs a PowerShell script for CRUD-style work item operations
- Returns structured JSON output suitable for agent summarization

## Supported Operations

| Operation | Behavior |
|-----------|----------|
| `list` | Retrieve all work items from a query (default: top 50) |
| `get` | Retrieve one work item by ID |
| `create` | Create a work item with title and optional description |
| `update` | Update title, description, and/or state. Auto-posts acknowledgment when state changes |
| `delete` | Delete a work item by ID |
| `post-comment` | Post a discussion comment on a work item |

## Security Rules

1. Never echo the PAT to output
2. Prefer PAT in env var `AZDO_PAT`
3. Do not store PAT in files or commit history
4. Require explicit confirmation before `delete`

## Inputs

| Input | Required | Notes |
|------|----------|-------|
| Organization | Conditionally | Example: `contoso` (from `https://dev.azure.com/contoso`). Default: `EssentialStudio` |
| Project | Conditionally | Azure DevOps project name. Default: `Ej2-Web` |
| OrganizationUrl | Optional | Can resolve org/project from URL like `https://dev.azure.com/contoso/MyProject` |
| Operation | Yes | `list`, `get`, `create`, `update`, `delete`, `post-comment` |
| Work Item Type | list/create | Default `Bug` |
| Work Item ID | get/update/delete/post-comment | Numeric ID (must be > 0) |
| Title | create/update | Required for create |
| Description | create/update | HTML/plain text accepted |
| NewState | update | New state value (e.g., `Active`, `Resolved`, `Validated`, `Closed`) |
| Comment | post-comment | Required for post-comment operation |
| AreaPath | list | Filter by area path (e.g., `Ej2-Web` or a sub-path like `Ej2-Web\Forms Team`) |
| State | list | Filter by state (e.g., `Active`, `New`, `Resolved`) |
| Top | list | Default `50` |
| Destroy | delete | Switch flag; if omitted, delete is soft (recycle bin). If set, permanent destruction |

## Execution Steps

1. Ask the user for organization, project, operation, and required fields.
2. If PAT is missing, check whether the environment variable is set:

```bash
# Check if the variable is set
printenv AZDO_PAT
```

If the variable is **not set**, do NOT ask the user to paste the token in the chat or terminal. Instead, instruct them to add it as a **persistent machine environment variable**:

**macOS / Linux:**
1. Open your shell profile (`~/.zshrc` for zsh or `~/.bashrc` for bash)
2. Add the line: `export AZDO_PAT='<your-azure-devops-pat>'`
3. Save the file and run `source ~/.zshrc` (or `source ~/.bashrc`)
4. Restart VS Code to pick up the new variable

**Windows (Environment Variables UI):**
1. Open **Settings → System → About → Advanced system settings → Environment Variables**
2. Under **User variables**, click **New**
3. Set Variable name: `AZDO_PAT` and Variable value: `<your-azure-devops-pat>`
4. Click OK and restart VS Code to pick up the new variable

**Windows (PowerShell — persistent):**
```powershell
[System.Environment]::SetEnvironmentVariable('AZDO_PAT', '<your-azure-devops-pat>', 'User')
```
Then restart VS Code.

> ⚠️ **Security**: Never pass tokens directly in chat messages, agent prompts, or inline terminal commands — they may be logged or accidentally committed.

Optionally set reusable context values:

```bash
export AZDO_ORGANIZATION='EssentialStudio'
export AZDO_PROJECT='Ej2-Web'
```

3. Run the script with `powershell` (Windows) or `pwsh` (macOS/Linux).
4. For `delete`, ask for explicit confirmation before execution.
5. Summarize result from JSON output.

## Command Templates

**Note:** Use `powershell` on Windows, `pwsh` on macOS/Linux.

```powershell
# List bugs (default WorkItemType is Bug)
powershell .github/skills/azure-devops/scripts/ado.ps1 -Organization EssentialStudio -Project "Ej2-Web" -Operation list -Top 50

# List bugs filtered by area path and state
powershell .github/skills/azure-devops/scripts/ado.ps1 -Organization EssentialStudio -Project "Ej2-Web" -Operation list -AreaPath "Ej2-Web\Forms%20Team" -State Active -Top 20

# List using URL details (org + project inferred)
powershell .github/skills/azure-devops/scripts/ado.ps1 -OrganizationUrl "https://dev.azure.com/EssentialStudio/Ej2-Web" -Operation list -Top 50

# Read one work item
powershell .github/skills/azure-devops/scripts/ado.ps1 -Organization EssentialStudio -Project "Ej2-Web" -Operation get -WorkItemId 123

# Create a bug
powershell .github/skills/azure-devops/scripts/ado.ps1 -Organization EssentialStudio -Project "Ej2-Web" -Operation create -WorkItemType Bug -Title "New bug" -Description "Created from skill"

# Create a task
powershell .github/skills/azure-devops/scripts/ado.ps1 -Organization EssentialStudio -Project "Ej2-Web" -Operation create -WorkItemType Task -Title "New task"

# Update
powershell .github/skills/azure-devops/scripts/ado.ps1 -Operation update -WorkItemId 123 -Title "Updated title"

# Update state
powershell .github/skills/azure-devops/scripts/ado.ps1 -Operation update -WorkItemId 123 -NewState "Active"

# Update state and title
powershell .github/skills/azure-devops/scripts/ado.ps1 -Operation update -WorkItemId 123 -Title "Updated title" -NewState "Resolved"

# Delete (soft — moves to recycle bin)
powershell .github/skills/azure-devops/scripts/ado.ps1 -Organization EssentialStudio -Project "Ej2-Web" -Operation delete -WorkItemId 123

# Delete (permanent — use with caution)
powershell .github/skills/azure-devops/scripts/ado.ps1 -Organization EssentialStudio -Project "Ej2-Web" -Operation delete -WorkItemId 123 -Destroy

# Post a comment
powershell .github/skills/azure-devops/scripts/ado.ps1 -Operation post-comment -WorkItemId 1000780 -Comment "testing"

# Post a longer comment from file (HTML FORMATTED)
$commentText = [System.IO.File]::ReadAllText('comment.html'); .github/skills/azure-devops/scripts/ado.ps1 -Operation 'post-comment' -WorkItemId 976209 -Comment $commentText
```

## Comment Formatting Best Practice

**IMPORTANT:** Azure DevOps comments should be posted as **HTML**, NOT plain text or markdown.

**Why?**
- Plain text appears as one unformatted block (all line breaks removed)
- Markdown formatting (**, ##, `, etc.) displays as HTML entities (&quot;, &amp;, etc.)
- HTML renders properly with headings, lists, code blocks, and formatting

**Best Practice - Use HTML:**

```html
<h2>Title</h2>
<h3>Subtitle</h3>
<p>Regular paragraph text.</p>

<ul>
  <li>Bullet item 1</li>
  <li>Bullet item 2</li>
</ul>

<p><strong>Bold text:</strong> Regular text</p>
<p><code>inline code</code></p>

<pre>Multi-line code block
Second line of code</pre>

<hr/>
```

**HTML Tags Supported:**
- Headings: `<h2>`, `<h3>`, `<h4>`
- Paragraphs: `<p>`
- Lists: `<ul>`, `<ol>`, `<li>`
- Emphasis: `<strong>`, `<em>`
- Code: `<code>`, `<pre>`
- Links: `<a href="...">`
- Line breaks: `<br/>`, `<hr/>`

**Example:**
```powershell
# Create HTML formatted comment
$htmlComment = @"
<h3>Fix Complete</h3>
<p><strong>Status:</strong> Ready for review</p>
<ul>
<li>Build passing</li>
<li>Tests added</li>
<li>Documentation complete</li>
</ul>
<p>PR: <a href="https://gitea.com/repo/pulls/8">PR #8</a></p>
"@

.github/skills/azure-devops/scripts/ado.ps1 -Operation post-comment -WorkItemId 976209 -Comment $htmlComment
```

**Example Plain Text Format:**
```
BUG-12345 FIX COMPLETE

SUMMARY
Fixed the issue by implementing XYZ

========================================

ROOT CAUSE
- Problem A occurred
- Led to Problem B
- Result: Feature broken

========================================

SOLUTION
Modified: src/Component.cs (lines 10-20)
✓ Added validation
✓ Fixed edge case
✓ Updated tests
```

## Automatic Acknowledgments

When state changes occur via the `update` operation, the skill automatically posts an acknowledgment comment to the work item:

**Example acknowledgment:**
```
[OK] State changed to 'Active', Title updated - via AI Driven Development from CodeStudio
```

This provides visibility into changes made through the skill and helps track automated updates.

## Notes

- **Auto-acknowledgments**: When state is updated via the skill, an automatic comment is posted to document the change.
- **Comment Format**: Always use plain text, never markdown (see "Comment Formatting Best Practice" above)
- This skill is script-driven and not a true typed MCP tool.
- Delete is soft by default (recycle bin). Use `-Destroy` for permanent deletion.
- For broad API coverage (pipelines, repos, PR comments, etc.), extend the script with additional resource routes.
- The script path from the repo root is `.github/skills/azure-devops/scripts/ado.ps1`.
- **Configured for Syncfusion Blazor Grid component development** with default organization: `EssentialStudio` and project: `Ej2-Web`.
