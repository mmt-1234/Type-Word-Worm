# CS20200 Programming Principles - Term Project
# Project Title: Type Word Worm

A premium, CLI-based typing game implemented in **F#** and **.NET 10**. English words spawn from the right side of the screen and travel leftward. Your objective is to type the words accurately and press Enter to shoot arrow projectiles that eliminate them before they reach the deadline!

---

## 🎮 Game Overview

In **Type Word Worm**, you must defend your boundary from the invading worms (words). 

- **Spawn Point:** Words appear at the right boundary ($x = 80$) at a random vertical position.
- **Movement:** Words move right-to-left
- **Scoring:** Accurately typing a word and pressing **Enter** fires an arrow projectile (`---->`) from the left boundary ($x = 5$). When the projectile reaches the word, the word is eliminated in a circular particle explosion, and points are awarded!
- **Game Over:** A "Dead Line" is established at the left boundary ($x = 5$). If any character of an active word crosses this deadline, the game immediately terminates.

---

## 🚀 How to Build and Run

### Prerequisites
- **.NET 10.0 SDK** must be installed on your machine. You can verify your installation by running:
  ```bash
  dotnet --version
  ```

### Steps to Run
1. Clone my Github Repository:
   ```bash
   git clone https://github.com/mmt-1234/Type-Word-Worm
   ```
2. Build the project:
   ```bash
   dotnet build
   ```
3. Run the game:
   ```bash
   dotnet run
   ```
4. To play, make sure your terminal window is expanded to at least **85 columns** and **25 rows** for the best visual experience.

---

## 🛠️ Key Game Mechanics & Technical Specifications

### 1. Dynamic Word Generation & Difficulty Scaling
- **Spawning:** Words are selected randomly from a predefined dictionary of common English words and programming terms.
- **Spawn Rate ($R$):** Starts at 0.5 words per second and increases over time.
  $$R = 0.5 + \frac{\text{Time}}{20} \times 0.5$$
- **Spawn Interval:** A new word spawns every $1 / R$ seconds.

### 2. Movement and Pathing (Linear)
- **Horizontal Velocity ($v$):** Words move at a constant horizontal velocity $v$ which scales with time:
  $$v = 3 + \frac{\text{Time}}{10} \times 0.5 \text{ (characters per second)}$$
- **Linear Pathing:** The vertical position ($y$) of each word remains constant at its initial random spawn height:
  $$y = y_{\text{spawn}}$$

### 3. Projectiles & Particle Effects
- **Arrow Projectiles (`---->`):** When you successfully type a word and press **Enter**, a projectile spawns at the left boundary ($x = 5$) at the same height as the target word.
- **Velocity:** Projectiles travel rightward at $2v$ (double the word's speed).
- **Collision:** When the projectile reaches the word, a beautiful, multi-frame circular particle explosion triggers at the collision coordinates.
- **Score Calculation ($\Delta S$):** Points awarded upon elimination:
  $$\Delta S = L \times R$$
  *(where $L$ is the character length of the word, and $R$ is the current spawn rate)*

### 4. Termination
- If any word's left-most character's $x$-coordinate becomes $\le 5$, the game immediately halts and renders a stylized, slanted **GAME OVER** screen.

---

## 🤖 Section 7: Use of Large Language Models (LLM)

In accordance with Section 7 of the Term Project Specification, here is the required experience report on working with the LLM:

### 1. What the LLM was used for:
- Designing the game loop architecture in F#, utilizing non-blocking asynchronous console input reads (`Console.KeyAvailable`).
- Devising a double-buffered custom text rendering engine to eliminate console flicker, drawing directly to specific cursor positions.
- Crafting dynamic particle systems for the circular explosion effect and formatting this comprehensive `README.md`.
- Making a Word Pool for Game.
- Making a Game Over Screen art.

### 2. What had to be manually changed or reprompted:
- **Wave Boundary Collision:** Initially, the wave pattern amplitude caused some words to move too close to the upper/lower status bars. We had to adjust the spawn height bounds ($y \in [4, 17]$) and damp the sine wave amplitude to $2.0$ to ensure perfect visibility.
- **Collision Precision:** The projectile collision check required tuning to ensure that fast-moving words and projectiles didn't pass through each other between frames. We changed it from an exact coordinate match to a threshold check ($x_{\text{proj}} \ge x_{\text{word}}$).

### 3. The main point that the LLM was not able to do correctly:
- **CLI Terminal Sizing & Buffer Sync:** The LLM initially assumed a standard terminal resize event listener would work, but standard .NET CLI resizing behaves differently across macOS and Windows. We had to enforce hardcoded grid boundaries ($80 \times 24$) and handle out-of-bounds rendering manually to prevent terminal cursor overflow crashes on standard macOS terminals.

---

## 📝 Requirement Changes and Justification

As permitted under **Section 4.2 (Final Submission)** of the Project Specification, we have recorded the following requirement change from the initial proposal:

| Planned Feature (Proposal) | Implemented Feature (Final) | Justification |
|:---|:---|:---|
| **Sine-wave Movement Pathing** (Words move leftward in a wavy pattern) | **Linear Movement Pathing** (Words move leftward in a straight horizontal line) | In playtesting, the sine-wave wiggle made characters fluctuate too rapidly in standard low-refresh console terminals. This made words extremely difficult to read and type accurately in a fast-paced environment, severely degrading playability. Changing to a clean, straight horizontal path ensures the game is highly playable, fair, and focuses on core typing skill while still retaining full graphical integrity. |
| **Word Speed** $v = 5 + \frac{\text{Time}}{10} \times 0.5$ | $v = 3 + \frac{\text{Time}}{10} \times 0.5$ | The initial speed of 5 chars/sec made the game start too difficult, leaving the player almost no reaction time before words crossed the deadline. Reducing the base speed to 3 chars/sec provides a more accessible early game while preserving the same difficulty scaling curve. Projectile speed remains 2v (consistent with the requirement). |
| **Spawn Rate** $R = 1 + \frac{\text{Time}}{10} \times 0.5$ | $R = 0.5 + \frac{\text{Time}}{20} \times 0.5$ | A starting rate of 1 word/sec combined with the revised word speed created too much simultaneous clutter, especially at higher word lengths. Reducing the base rate to 0.5 words/sec and slowing the ramp gives the player time to clear existing words before new ones pile up, resulting in a fairer and more enjoyable progression. |
