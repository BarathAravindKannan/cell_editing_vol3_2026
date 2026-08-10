# Feature Impact Analysis Skill

**Purpose**  
Cross-feature dependency analyzer for the Blazor Grid. Ensures zero regression by mapping interactions before any code change.

**When to Use**  
Before implementing or modifying any feature, invoke this skill to:
- Identify all features that could be affected
- Map exact behavioral dependencies
- Generate a pre-change verification checklist

**Analysis Protocol**  

1. **Identify the Target Feature**  
   Read the feature's requirements folder and training docs to understand its scope.

2. **Scan All Other Features**  
   For each existing feature in `/docs/requirements/features/`:
   - Read its requirements and training materials
   - Check if it shares data models, services, or UI space
   - Look for explicit interaction rules in training docs

3. **Build Interaction Matrix**  
   For each combination found, document:
   - What behavior must be preserved
   - Risk level (Critical/High/Medium)
   - Files/components shared between features

4. **Generate Verification Checklist**  
   Output a test scenario for each high-risk interaction.

**Output Format**  

```markdown
## Impact Analysis for {Feature}

### Direct Dependencies
- Feature A: [shared component/service/state]
- Feature B: [shared data model]

### Interaction Matrix
| Affected Feature | Must Preserve | Risk | Verification Test |
|------------------|---------------|------|-------------------|
| Sorting | Sort order after {action} | Critical | Apply filter → sort → verify order |
| Grouping | Group structure remains | High | Group → {action} → verify groups |

### Files to Review Before Change
- [exact file paths that are shared]

### Recommended Test Sequence
1. Test {Feature} in isolation
2. Test {Feature} + Sorting
3. Test {Feature} + Filtering
...
```

**Integration with Other Skills**  
Every feature skill's Interaction Matrix is derived from this analysis process.
Use this skill before generating or updating any feature skill.
