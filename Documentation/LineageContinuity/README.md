# Lineage continuity — evidence and review

Use **Tab → Chronicle** after transferring control. The header says whose descendants/relationships are shown. Siblings from an earlier controlled parent's family remain visible and locatable, but their cards cannot take control and Locate does not change care eligibility. Founders now have deterministic names beside ID and Gen 0. Normal survival and reproduction are unchanged.

- [Verification and test results](../../VERIFICATION.md)
- [Final suite breakdown](test-results.md)
- [Exact twelve-file asset inventory and reconciliation](ASSET_REVIEW.md)
- [Baseline natural birth/death/control trace](baseline-natural-events.jsonl)
- [Baseline state after taking control](baseline-after-control.json)

The natural baseline used real-time, unmodified gameplay with autonomous AI and a native F player courtship, Skip, descendant-card click, Tab, Chronicle and paging. The temporary observer only recorded run state and disabled focus-loss pause; it did not refill resources, move creatures, create births or accelerate time. The idle founder eventually starved; its first child survived and became the controlled creature. This is agent-operated native input, not a human playthrough.

Controlled edge cases use explicitly assisted co-location, refills, frozen AI and accelerated cooldown/growth to make several siblings available together. Any deliberately induced death is a fixture, not evidence of premature natural death. The temporary helper was removed before final tests.

## Rendered evidence (native journal clicks, assisted setup)

- [Named founders and a living sibling](journal-founders.png): #001 Asha and #002 Sela, Gen 0, are parents of controlled #017. #014 Mira 1 is a sibling, not a descendant.
- [Living sibling and child, page 3/3](journal-living-family.png): #018 has Locate only; #019 can be located or controlled. The left HUD says #017 has one child born, matching its own life rather than the previous player's total.
- [Recorded child death after corpse cleanup](journal-recorded-death.png): forced fixture starvation, not natural mortality evidence. Stable #019 retains name, parents, cause and lived age. #018 stays locatable.
- [Assisted birth/control/death trace](fixture-events.jsonl), [the exact #017/#019 state before death](fixture-017-019.json), [state after death and cleanup](fixture-after-death-cleanup.json).

Each trace row records stable ID, parent IDs, current controller (top-level), life state, death cause or none, actor registration, archive-alive state, relationship and both list memberships. A living sibling's `livingList: false` is correct: it is not a descendant of the current controller. `chronicle: true` now keeps it discoverable from the earlier controlled parent's history. Unrelated founders are excluded, rather than falsely labeled as ancestors in the UI. After an actual recorded death the child leaves Living descendants but stays in Chronicle. Actor absence alone never supplies a death cause.

In the natural baseline the first player child #018 survived its founder's death and became controlled; it was therefore **You**, not a missing descendant. The user's earlier ephemeral session is not recoverable, so these results cannot establish whether their original first child died.
