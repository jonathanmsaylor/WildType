# Survival pressure — actual Unity Editor evidence

Unedited 1920x1080 Game-view captures from the saved Creature Stage, seed 917430. Native Editor inspection plus temporary virtual gamepad input through the existing input bridge; not human playtesting. No staged food depletion, healing, teleport, kill, birth or genome change. The helper was removed before final verification.

1. [Depleted meadow](01-depleted-meadow.png): resting player at 61 energy, empty local stems, 24 population/11 births. Repeated AI harvests are recorded in the inspection log.
2. [Walked into woodland](02-woodland-arrival.png): approximately 49 m travelled normally from the meadow to a fernberry. Movement consumed energy and crossed the region boundary.
3. [Normal woodland meal](03-woodland-meal.png): virtual South-button input restores 22 energy; depleted fernberry remains visible, and energy is 40/100. This is not an injected refill.
4. [Natural starvation and retained turnover](04-death-and-history.png): after returning to depleted meadow and missing a competing harvest, the player eventually died while arranging captures. The ordinary no-descendant menu appears; #001 starvation, #028 generation-four birth, and #007 old-age death remain in the bounded history. No silent replacement.

The player death is not evidence of autonomous trait selection. The three-seed before/after survey and matched-genome causal tests are distinguished in [VERIFICATION.md](../../VERIFICATION.md). Meadow #0's live recovery (ripe at 246.4 s, stored delay falling from 119.8 to 101.8 by 282.5 s) is recorded in the inspection log; no still screenshot alone proves a timer trajectory. Native restart and paused reseed were also inspected successfully.
