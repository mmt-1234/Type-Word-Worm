# Type Word Worm
CS-20200 Programming Principles — Term Project, Spring 2026
**Student ID:** 20250318 Junhyoung Park

A CLI-based typing game written in F# and .NET 10. Words crawl in from the right side of the screen. Type each word exactly and press Enter to blast it with an arrow before it crosses the deadline on the left.

---

## Getting Started

**Prerequisites:** .NET 10.0 SDK — verify with `dotnet --version`

```bash
git clone https://github.com/mmt-1234/Type-Word-Worm
cd Type-Word-Worm
dotnet run
```


> Expand your terminal to at least **85 columns × 25 rows** before launching.

---

## How to Play

When the game starts, a title screen shows the rules and key bindings. Press any key to begin.

| Key | Action |
|---|---|
| Letters | Add to input buffer |
| Backspace | Delete last character |
| Enter | Fire arrow at the matched word |
| ESC | Quit |

Type a word shown on screen exactly, then press **Enter**. An arrow (`---->`) launches from the left and travels right until it hits the word, destroying it. If any word reaches the **Dead Line** on the left edge, the game ends immediately.

After a game over, press **Y** to play again or **ESC** to exit.

---

## Example Interaction

The game shows a start screen. The player presses any key.

A word `fsharp` appears at the right side of the screen and drifts left. The player types `f`, `s`, `h`, `a`, `r`, `p` — the input buffer at the bottom shows `INPUT: fsharp_` — then presses **Enter**. An arrow launches from the left at twice the word's speed, flies across the screen, and destroys `fsharp` in a particle explosion. The score increases.

As time passes, words spawn faster and move quicker. A word `lambda` slips through untouched and reaches the dead line. The game halts and displays:

```
  GAME OVER
  FINAL SCORE: 42 points
  SURVIVED TIME: 38.2 seconds
  Press [Y] to play again or [ESC] to quit
```

---

## Requirements

The following requirements define the observable, testable behavior of the game. Each item can be verified by running the game.

1. Words appear at the right boundary (x = 80) at a random vertical position between y = 4 and y = 17.
2. Each word moves left at velocity v = 3 + (t / 10) × 0.5 chars/sec, where t is elapsed seconds. A word's vertical position never changes after spawning.
3. Words spawn at rate R = 0.5 + (t / 20) × 0.5 words/sec — that is, one word every 1/R seconds.
4. The player types into an input buffer shown at the bottom of the screen. Pressing **Enter** submits the buffer.
5. If the submitted buffer exactly matches an active word, an arrow (`---->`) fires from x = 5 at the same row as the matched word and the buffer clears.
6. The arrow travels right at 2v (double the current word speed).
7. When the arrow's x-coordinate reaches the word's x-coordinate, the word is eliminated and a circular particle explosion plays at that position.
8. Score increases by ΔS = L × R on each elimination, where L is the word's character count and R is the current spawn rate.
9. A Dead Line is drawn at x = 5. If any word's leftmost character reaches x ≤ 5, the game ends immediately and a GAME OVER screen shows the final score and time survived.
10. Current score and elapsed time are displayed on screen throughout the game.
11. On launch, a start screen shows the game title, rules, and key bindings. The game begins only after the player presses any key.
12. After GAME OVER, pressing **Y** restarts the game from the beginning; pressing **ESC** exits the program.

---

## Requirement Changes

As permitted under Section 4.2 of the project specification, the following requirements were changed from the initial proposal, each with justification.

| Planned (Proposal) | Implemented (Final) | Justification |
|:---|:---|:---|
| Sine-wave movement — words move left in a wavy vertical pattern | Linear movement — words move left in a straight horizontal line | In testing, the wave motion made characters shift vertically too fast on standard console refresh rates, making words nearly impossible to read mid-motion. Straight movement preserves full playability and typing focus. |
| Word speed: v = 5 + (t / 10) × 0.5 | v = 3 + (t / 10) × 0.5 | A base speed of 5 chars/sec left almost no reaction time at the start. Lowering the base to 3 provides a fair early game while keeping the same scaling curve. Projectile speed remains 2v. |
| Spawn rate: R = 1 + (t / 10) × 0.5 | R = 0.5 + (t / 20) × 0.5 | A starting rate of 1 word/sec combined with the higher speed flooded the screen too quickly. A lower base rate and slower ramp gives the player time to clear words before new ones pile up. |

---

## Use of Large Language Models

In accordance with Section 7 of the project specification:

**What the LLM was used for:**
- Designing the game loop architecture using non-blocking input reads (`Console.KeyAvailable`) and a fixed-interval update-render cycle.
- Building a double-buffered terminal renderer to eliminate screen flicker.
- Generating the particle explosion system and ASCII art for the GAME OVER and start screens.
- Assembling the word pool and writing unit tests for the core formulas (`calcSpeed`, `calcSpawnRate`, `calcScore`) and spawn placement logic (`selectSafeSpawnY`).

**What had to be manually changed or reprompted:**
- **Spawn height bounds:** The LLM initially let words spawn at any row, causing overlap with the status bars. The valid range had to be clamped to y ∈ [4, 17] manually.
- **Collision check:** The first version used an exact x-coordinate match, which caused fast-moving words and arrows to pass through each other between frames. It was changed to a threshold check (x_proj ≥ x_word).

**What the LLM was not able to do correctly:**
- **Terminal buffer sizing on macOS:** The LLM assumed a standard resize event API would work, but .NET console resizing behaves differently on macOS versus Windows. The grid had to be hardcoded to 80 × 24 and all cursor writes clamped manually to prevent overflow crashes.


