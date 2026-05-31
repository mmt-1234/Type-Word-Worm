# CS20200 Term Project Proposal

**Project Title:** Type Word Worm  
**Submitted by:** 20250318 박준형  

---

## 1. Overview
This project is a CLI-based typing game implemented in **F#** and **.NET 10**. 
English words appear from the right side of the screen and move toward the left. The player earns points and eliminates the "worm" (word) by typing the word accurately before it reaches the left boundary.

---

## 2. Requirements
The following requirements are designed to be objective and testable.

### 2.1. Dynamic Word Generation
* Words are spawned at the right boundary ($x = 80$).
* The spawn rate ($R$) starts at 1 word per second and increases by 0.5 every 10 seconds.
  $$\text{Spawn Rate } (R) = 1 + \frac{\text{Time}}{10} \times 0.5$$

### 2.2. Movement and Pathing
* Words move from right to left with a constant horizontal velocity $v$:
  $$v = 5 + \frac{\text{Time}}{10} \times 0.5 \text{ (characters per second)}$$
* The vertical position ($y$) is randomly decided when a word spawns.

### 2.3. Scoring and Particle Effects
* **Arrow Projectile:** Upon correctly typing all characters of a target word and pressing **Enter**, an arrow projectile (`---->`) is spawned at the left boundary ($x = 5$). The projectile travels rightward at a constant velocity of $2v$ (twice the speed of the word) until it reaches the target word.
* **Word Removal:** The target word is removed from the screen only when the arrow projectile makes contact with it, triggering a brief circular particle effect.
* **Score Calculation ($\Delta S$):** Points are awarded at the moment of elimination based on the formula:
  $$\Delta S = L \times R$$
  *(where $L$ is the number of characters in the word, and $R$ is the current spawn rate)*

### 2.4. Termination Condition (Game Over)
* A "Dead Line" is established at the left boundary ($x = 5$).
* If the $x$-coordinate of any character in a word becomes less than or equal to 5, the game immediately terminates.

---

## 3. Example Interaction
* **System:** A word `coding` spawns at $x = 80$ and moves leftward in a wave pattern.
* **User:** Types `c`, `o`, `d`, `i`, `n`, `g` and presses `Enter`.
* **System:** The word `coding` disappears with a circular particle effect, and the score increases by 10.
* **System:** As time passes, new words spawn more frequently.
* **System:** A word `fsharp` reaches $x = 5$. The game stops and shows a slanted **GAME OVER** screen.

---

## 4. Technical Specifications
* **Language:** F#
* **Runtime:** .NET 10
* **UI Mode:** CLI