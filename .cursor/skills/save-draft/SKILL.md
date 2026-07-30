---
name: save-draft
description: Save the project's current implementation state and a focused handoff for tomorrow. Use automatically whenever the user says "save draft".
---

# Save Draft

Trigger: when i say "save draft", you will save what have we implemented so far, and what is left to be finished for tomorrow.

## Workflow

1. Read the existing `docs/DRAFT_HANDOFF.md` if present. Note its date and its unfinished items.
2. Rotate before writing, so the current file only ever describes now:
   - If the existing draft's date is **older than today**, move it to `docs/drafts/YYYY-MM-DD.md` using that draft's own date, then write the new one.
   - If the existing draft is **already dated today**, overwrite it in place — repeated saves within one day must not create extra files.
3. Inspect the current git status, staged and unstaged diffs, and recent commits.
4. Read `docs/SCHEDULE.md` and the implementation files relevant to the current day's work.
5. Run only quick, relevant verification if needed to distinguish completed work from unverified work.
6. Carry forward unfinished items from the previous draft: re-check each against the repository, drop the ones now done, and keep the ones still open so nothing silently disappears between days.
7. Write `docs/DRAFT_HANDOFF.md` with:
   - date and current schedule day/milestone;
   - implemented so far, based on repository evidence;
   - verification completed and its result;
   - incomplete or partially implemented work, including still-open carryover;
   - a short, ordered plan for tomorrow;
   - blockers, risks, and important working-tree state.
8. Keep the handoff concise and actionable. Do not claim work is complete without evidence.
9. Do not change confirmed product decisions, mark schedule items complete, commit, or push unless the user separately requests it.

## Keep the draft clean

- State each item once. Do not repeat the same item under both "Still unfinished" and "Tomorrow" — put the fact in one section and reference it briefly in the other.
- Do not narrate the save-draft pass itself, or explain which files this skill did or did not touch. The reader wants project state, not process commentary.
- Write for someone starting tomorrow morning with no memory of today's session.

## Handoff Template

```markdown
# Draft Handoff — YYYY-MM-DD

## Implemented
- ...

## Verification
- ...

## Still unfinished
- ...

## Tomorrow
1. ...

## Blockers / notes
- ...
```
