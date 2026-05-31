module Program

open System

// ==========================================
// GAME CONSTANTS & CONFIGURATION
// ==========================================
let screenWidth = 80
let screenHeight = 24

// Screen buffer double-buffering variables to eliminate console flicker
let currentBuffer = Array2D.create screenHeight screenWidth ' '
let currentColor = Array2D.create screenHeight screenWidth ConsoleColor.Gray
let prevBuffer = Array2D.create screenHeight screenWidth ' '
let prevColor = Array2D.create screenHeight screenWidth ConsoleColor.Gray

let wordPool = [
    // Core F# concepts
    "fsharp"; "dotnet"; "lambda"; "currying"; "recursion";
    "immutable"; "mutable"; "pattern"; "matching"; "signature";
    "functor"; "pipeline"; "composition"; "option"; "result";
    "discriminated"; "union"; "record"; "tuple"; "sequence";
    "async"; "computation"; "expression"; "inference"; "generic";
    "algebraic"; "application"; "fold"; "bind"; "closure";
    "module"; "namespace"; "workflow"; "map"; "observable";
    "typeclass"; "polymorphism"; "function"; "quotation"; "active";
    "array"; "string"; "boolean"; "integer"; "lazy";
    "evaluation"; "higher"; "partial"; "printf"; "sprintf";
    "filter"; "reduce"; "collect"; "purity"; "effect";
    "agent"; "reactive"; "equality"; "binding"; "scope";
    "syntax"; "compiler"; "runtime"; "parallel"; "delegate";
    "abstract"; "interface"; "inherit"; "override"; "exception";
    "casting"; "assembly"; "threading"; "callback"; "monoid";
    "value"; "parameter"; "iteration"; "conditional"; "declaration";
]

let random = Random()

// Pure helper functions (testable)
let calcSpeed elapsed = 3.0 + (elapsed / 10.0) * 0.5
let calcSpawnRate elapsed = 0.5 + (elapsed / 20.0) * 0.5
let calcScore wordLength spawnRate = int (Math.Round(float wordLength * spawnRate))

// ==========================================
// GAME DATA TYPES
// ==========================================
type Word = {
    Id: Guid
    Text: string
    mutable X: float
    SpawnY: float
    mutable IsTargeted: bool
}

type Projectile = {
    mutable X: float
    mutable Y: int
    TargetId: Guid
    Speed: float
}

type Particle = {
    mutable X: float
    mutable Y: float
    Vx: float
    Vy: float
    mutable Age: float
    MaxAge: float
    Char: char
    Color: ConsoleColor
}

type GameState = {
    StartTime: DateTime
    mutable Score: int
    mutable Words: list<Word>
    mutable Projectiles: list<Projectile>
    mutable Particles: list<Particle>
    mutable InputBuffer: string
    mutable LastSpawnTime: float
    mutable GameOver: bool
    mutable Quit: bool
}

// ==========================================
// HELPER METHODS
// ==========================================
let safeSetCursorPosition (x: int) (y: int) =
    let clampedX = max 0 (min x (Console.BufferWidth - 1))
    let clampedY = max 0 (min y (Console.BufferHeight - 1))
    try
        Console.SetCursorPosition(clampedX, clampedY)
    with
    | _ -> ()

let drawChar (x: int) (y: int) (c: char) (color: ConsoleColor) =
    if x >= 0 && x < screenWidth && y >= 0 && y < screenHeight then
        currentBuffer.[y, x] <- c
        currentColor.[y, x] <- color

let render () =
    for y in 0 .. screenHeight - 1 do
        for x in 0 .. screenWidth - 1 do
            // Only draw to screen if character or color changed (Double Buffering)
            if currentBuffer.[y, x] <> prevBuffer.[y, x] || currentColor.[y, x] <> prevColor.[y, x] then
                safeSetCursorPosition x y
                Console.ForegroundColor <- currentColor.[y, x]
                Console.Write(currentBuffer.[y, x])
                prevBuffer.[y, x] <- currentBuffer.[y, x]
                prevColor.[y, x] <- currentColor.[y, x]

// ==========================================
// CORE GAME LOOPS & RENDERING
// ==========================================
let processInput (state: GameState) =
    while Console.KeyAvailable do
        let keyInfo = Console.ReadKey(true)
        match keyInfo.Key with
        | ConsoleKey.Escape ->
            state.Quit <- true
        | ConsoleKey.Backspace ->
            if state.InputBuffer.Length > 0 then
                state.InputBuffer <- state.InputBuffer.Substring(0, state.InputBuffer.Length - 1)
        | ConsoleKey.Enter ->
            // Find closest matching untargeted word
            let matchedWordOpt =
                state.Words
                |> List.filter (fun w -> not w.IsTargeted && w.Text.Equals(state.InputBuffer, StringComparison.OrdinalIgnoreCase))
                |> List.sortBy (fun w -> w.X)
                |> List.tryHead
            
            match matchedWordOpt with
            | Some matchedWord ->
                matchedWord.IsTargeted <- true
                let elapsed = (DateTime.UtcNow - state.StartTime).TotalSeconds
                let v = calcSpeed elapsed
                let startProjY = int (Math.Round(matchedWord.SpawnY))
                let proj = {
                    X = 5.0
                    Y = startProjY
                    TargetId = matchedWord.Id
                    Speed = 2.0 * v
                }
                state.Projectiles <- proj :: state.Projectiles
                state.InputBuffer <- ""
            | None ->
                state.InputBuffer <- "" // Clear buffer on wrong word typing
        | _ ->
            let c = keyInfo.KeyChar
            // Allow only letters, digits, dots, hashes, dashes
            if Char.IsLetterOrDigit(c) || c = '#' || c = '.' || c = '-' then
                if state.InputBuffer.Length < 20 then
                    state.InputBuffer <- state.InputBuffer + string c

let selectSafeSpawnY (activeWords: list<Word>) =
    // Candidate Y rows: from 4 to 16 inclusive to keep well above the input bar at 22
    let candidates = [ 4.0 .. 16.0 ]
    
    // Filter out Y rows that have any active word close to the right boundary (X >= 55.0)
    let safeCandidates =
        candidates
        |> List.filter (fun y ->
            not (activeWords |> List.exists (fun w -> Math.Abs(w.SpawnY - y) < 0.1 && w.X >= 55.0))
        )
        
    if not (List.isEmpty safeCandidates) then
        safeCandidates.[random.Next(safeCandidates.Length)]
    else
        // Fallback: pick the row where the closest word is furthest to the left (i.e. smallest X)
        let yWithMinX =
            candidates
            |> List.sortBy (fun y ->
                let matchingWords = activeWords |> List.filter (fun w -> Math.Abs(w.SpawnY - y) < 0.1)
                if List.isEmpty matchingWords then 0.0 else matchingWords |> List.map (fun w -> w.X) |> List.max
            )
            |> List.tryHead
        match yWithMinX with
        | Some y -> y
        | None -> float (random.Next(4, 17))

let updateGame (state: GameState) (dt: float) =
    let elapsed = (DateTime.UtcNow - state.StartTime).TotalSeconds
    let spawnRate = calcSpawnRate elapsed
    let v = calcSpeed elapsed

    // 1. Spawning words dynamically
    if elapsed - state.LastSpawnTime >= 1.0 / spawnRate then
        let newWordText = wordPool.[random.Next(wordPool.Length)]
        let spawnY = selectSafeSpawnY state.Words
        let newWord = {
            Id = Guid.NewGuid()
            Text = newWordText
            X = 80.0
            SpawnY = spawnY
            IsTargeted = false
        }
        state.Words <- state.Words @ [newWord]
        state.LastSpawnTime <- elapsed

    // 2. Update word horizontal positions & check for Game Over boundary crossing
    for w in state.Words do
        w.X <- w.X - v * dt
        // Game Over: If left-most character coordinates <= 5
        if w.X <= 5.0 then
            state.GameOver <- true

    // 3. Update active projectiles
    for p in state.Projectiles do
        p.X <- p.X + p.Speed * dt
        // Update projectile height (Y) to follow target word's straight path
        let targetWordOpt = state.Words |> List.tryFind (fun w -> w.Id = p.TargetId)
        match targetWordOpt with
        | Some target ->
            p.Y <- int (Math.Round(target.SpawnY))
        | None ->
            ()

    // 4. Collision checking
    let mutable collisions = []
    let mutable remainingProjectiles = []
    
    for p in state.Projectiles do
        let targetWordOpt = state.Words |> List.tryFind (fun w -> w.Id = p.TargetId)
        match targetWordOpt with
        | Some target ->
            // Collision happens when projectile reaches or passes the target word
            if p.X >= target.X then
                collisions <- (target.Id, p.X, float p.Y) :: collisions
            else
                remainingProjectiles <- p :: remainingProjectiles
        | None ->
            () // Word was removed or targeted by another source
    
    state.Projectiles <- remainingProjectiles

    // 5. Handle collisions: remove words, trigger explosion, update score
    for (wordId, cx, cy) in collisions do
        let wordOpt = state.Words |> List.tryFind (fun w -> w.Id = wordId)
        match wordOpt with
        | Some word ->
            // Score formula: Length * SpawnRate
            let points = calcScore word.Text.Length spawnRate
            state.Score <- state.Score + points
            
            // Remove word from active list
            state.Words <- state.Words |> List.filter (fun w -> w.Id <> wordId)
            
            // Trigger 12 expanding particles in circular fashion
            for i in 0 .. 11 do
                let angle = float i * (2.0 * Math.PI / 12.0)
                let speed = 6.0 + random.NextDouble() * 6.0
                let p = {
                    X = cx
                    Y = cy
                    Vx = cos angle * speed
                    Vy = sin angle * speed * 0.4 // Squished vertically to fit terminal spacing
                    Age = 0.0
                    MaxAge = 0.3 + random.NextDouble() * 0.3
                    Char = match random.Next(3) with | 0 -> '*' | 1 -> '+' | _ -> '.'
                    Color = match random.Next(3) with | 0 -> ConsoleColor.Yellow | 1 -> ConsoleColor.Red | _ -> ConsoleColor.DarkYellow
                }
                state.Particles <- p :: state.Particles
        | None -> ()

    // 6. Update explosion particles
    for p in state.Particles do
        p.Age <- p.Age + dt
        p.X <- p.X + p.Vx * dt
        p.Y <- p.Y + p.Vy * dt
        
    state.Particles <- state.Particles |> List.filter (fun p -> p.Age < p.MaxAge)

let renderGame (state: GameState) =
    // Clear rendering buffer
    for y in 0 .. screenHeight - 1 do
        for x in 0 .. screenWidth - 1 do
            currentBuffer.[y, x] <- ' '
            currentColor.[y, x] <- ConsoleColor.Gray

    // Render visual boundaries
    for x in 0 .. screenWidth - 1 do
        currentBuffer.[2, x] <- '═'
        currentColor.[2, x] <- ConsoleColor.DarkGray
        currentBuffer.[21, x] <- '═'
        currentColor.[21, x] <- ConsoleColor.DarkGray

    // Visual deadline at x = 5 (Red vertical line)
    for y in 3 .. 20 do
        currentBuffer.[y, 5] <- '║'
        currentColor.[y, 5] <- ConsoleColor.Red

    // Status bar stats computation
    let elapsed = (DateTime.UtcNow - state.StartTime).TotalSeconds
    let spawnRate = calcSpawnRate elapsed
    let v = calcSpeed elapsed

    let statsLine1 = sprintf " TIME: %6.1fs   SCORE: %05d   SPEED: %4.1fc/s   RATE: %4.2fw/s" elapsed state.Score v spawnRate
    let statsLine2 = " ═════════════════════════════ TYPE WORD WORM ═════════════════════════════"

    for i in 0 .. min (statsLine1.Length - 1) (screenWidth - 1) do
        currentBuffer.[0, i] <- statsLine1.[i]
        currentColor.[0, i] <- ConsoleColor.Cyan

    for i in 0 .. min (statsLine2.Length - 1) (screenWidth - 1) do
        currentBuffer.[1, i] <- statsLine2.[i]
        currentColor.[1, i] <- ConsoleColor.DarkCyan

    // Input buffer rendering
    let inputStr = sprintf " INPUT: %s_" state.InputBuffer
    for i in 0 .. min (inputStr.Length - 1) (screenWidth - 1) do
        currentBuffer.[22, i] <- inputStr.[i]
        currentColor.[22, i] <- ConsoleColor.White

    let footerStr = " [ESC] Quit   [Backspace] Delete   [Enter] Shoot Projectile"
    for i in 0 .. min (footerStr.Length - 1) (screenWidth - 1) do
        currentBuffer.[23, i] <- footerStr.[i]
        currentColor.[23, i] <- ConsoleColor.DarkGray

    // Render arrow projectiles: ---->
    for p in state.Projectiles do
        let px = int (Math.Round(p.X))
        let py = p.Y
        let chars = [|'-'; '-'; '-'; '>'|]
        for i in 0 .. 3 do
            let cx = px - (3 - i)
            if cx >= 0 && cx < screenWidth && py >= 3 && py <= 20 then
                currentBuffer.[py, cx] <- chars.[i]
                currentColor.[py, cx] <- ConsoleColor.Green

    // Render active words (each character moves in a straight horizontal line!)
    for w in state.Words do
        let len = w.Text.Length
        for i in 0 .. len - 1 do
            let cx_float = w.X + float i
            let cx = int (Math.Round(cx_float))
            let cy = int (Math.Round(w.SpawnY))
            
            if cx >= 0 && cx < screenWidth && cy >= 3 && cy <= 20 then
                currentBuffer.[cy, cx] <- w.Text.[i]
                if w.IsTargeted then
                    currentColor.[cy, cx] <- ConsoleColor.Yellow
                else
                    currentColor.[cy, cx] <- ConsoleColor.Magenta

    // Render explosion particles
    for p in state.Particles do
        let px = int (Math.Round(p.X))
        let py = int (Math.Round(p.Y))
        if px >= 0 && px < screenWidth && py >= 3 && py <= 20 then
            currentBuffer.[py, px] <- p.Char
            currentColor.[py, px] <- p.Color

    // Render to terminal window using delta comparison
    render()

let showGameOverScreen (score: int) (elapsed: float) =
    Console.Clear()
    Console.ForegroundColor <- ConsoleColor.Red

    let gameOverArt = [
        """  ___   _   __  __ ___     ___  _   _ ___ ___ """
        """ / __| /_\ |  \/  | __|   / _ \| | | | __| _ \"""
        """| (_ |/ _ \| |\/| | _|   | (_) | |_| | _||   /"""
        """ \___/_/ \_\_|  |_|___|   \___/ \___/|___|_|_\"""
    ]

    let startY = 5
    for i in 0 .. gameOverArt.Length - 1 do
        safeSetCursorPosition ((screenWidth - gameOverArt.[i].Length) / 2) (startY + i)
        Console.Write(gameOverArt.[i])

    Console.ForegroundColor <- ConsoleColor.Yellow
    safeSetCursorPosition 25 13
    Console.Write(sprintf "FINAL SCORE: %d points" score)

    safeSetCursorPosition 25 15
    Console.Write(sprintf "SURVIVED TIME: %.1f seconds" elapsed)

    Console.ForegroundColor <- ConsoleColor.Gray
    safeSetCursorPosition 25 18
    Console.Write("Press ESC to quit...")

    // Clear key buffer and wait for ESC
    while Console.KeyAvailable do
        Console.ReadKey(true) |> ignore
    let mutable waiting = true
    while waiting do
        let key = Console.ReadKey(true)
        if key.Key = ConsoleKey.Escape then
            waiting <- false

// ==========================================
// GAME ENTRY POINT
// ==========================================
[<EntryPoint>]
let main argv =
    // Initialize Console UI parameters
    Console.Title <- "Type Word Worm"
    Console.Clear()
    Console.CursorVisible <- false

    let state = {
        StartTime = DateTime.UtcNow
        Score = 0
        Words = []
        Projectiles = []
        Particles = []
        InputBuffer = ""
        LastSpawnTime = 0.0
        GameOver = false
        Quit = false
    }

    let mutable lastTime = DateTime.UtcNow

    // Game loop
    while not state.GameOver && not state.Quit do
        let now = DateTime.UtcNow
        let dt = (now - lastTime).TotalSeconds
        lastTime <- now

        processInput state
        updateGame state dt
        renderGame state

        // Sleep to limit frame rate ~30 FPS
        System.Threading.Thread.Sleep(33)

    // Game Over display or regular exits
    Console.CursorVisible <- true
    Console.ResetColor()

    if state.GameOver then
        let elapsed = (DateTime.UtcNow - state.StartTime).TotalSeconds
        showGameOverScreen state.Score elapsed

    Console.Clear()
    0
