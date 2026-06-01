module GameRender

open System
open GameConfig
open GameTypes
open GameInput

let renderGame (state: GameState) =
    for y in 0 .. screenHeight - 1 do
        for x in 0 .. screenWidth - 1 do
            currentBuffer.[y, x] <- ' '
            currentColor.[y, x] <- ConsoleColor.Gray

    for x in 0 .. screenWidth - 1 do
        currentBuffer.[2, x] <- '═'
        currentColor.[2, x] <- ConsoleColor.DarkGray
        currentBuffer.[21, x] <- '═'
        currentColor.[21, x] <- ConsoleColor.DarkGray

    for y in 3 .. 20 do
        currentBuffer.[y, 5] <- '║'
        currentColor.[y, 5] <- ConsoleColor.Red

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

    let inputStr = sprintf " INPUT: %s_" state.InputBuffer
    for i in 0 .. min (inputStr.Length - 1) (screenWidth - 1) do
        currentBuffer.[22, i] <- inputStr.[i]
        currentColor.[22, i] <- ConsoleColor.White

    let footerStr = " [ESC] Quit   [Backspace] Delete   [Enter] Shoot Projectile"
    for i in 0 .. min (footerStr.Length - 1) (screenWidth - 1) do
        currentBuffer.[23, i] <- footerStr.[i]
        currentColor.[23, i] <- ConsoleColor.DarkGray

    for p in state.Projectiles do
        let px = int (Math.Round(p.X))
        let py = p.Y
        let chars = [|'-'; '-'; '-'; '>'|]
        for i in 0 .. 3 do
            let cx = px - (3 - i)
            if cx >= 0 && cx < screenWidth && py >= 3 && py <= 20 then
                currentBuffer.[py, cx] <- chars.[i]
                currentColor.[py, cx] <- ConsoleColor.Green

    for w in state.Words do
        let len = w.Text.Length
        for i in 0 .. len - 1 do
            let cx_float = w.X + float i
            let cx = int (Math.Round(cx_float))
            let cy = int (Math.Round(w.SpawnY))
            if cx >= 0 && cx < screenWidth && cy >= 3 && cy <= 20 then
                currentBuffer.[cy, cx] <- w.Text.[i]
                currentColor.[cy, cx] <- if w.IsTargeted then ConsoleColor.Yellow else ConsoleColor.Magenta

    for p in state.Particles do
        let px = int (Math.Round(p.X))
        let py = int (Math.Round(p.Y))
        if px >= 0 && px < screenWidth && py >= 3 && py <= 20 then
            currentBuffer.[py, px] <- p.Char
            currentColor.[py, px] <- p.Color

    render()

let showStartScreen () =
    Console.Clear()
    Console.ForegroundColor <- ConsoleColor.Cyan

    let titleArt = [
        """ _____ _   _ ____  _____   __        _____  ____  ____     __        _____  ____  __  __ """
        """| ____| | | |  _ \| ____|  \ \      / / _ \|  _ \|  _ \   \ \      / / _ \|  _ \|  \/  |"""
        """|  _| | | | | |_) |  _|    \ \ /\ / / | | | |_) | | | |   \ \ /\ / / | | | |_) | |\/| |"""
        """| |___| |_| |  _ <| |___    \ V  V /| |_| |  _ <| |_| |    \ V  V /| |_| |  _ <| |  | |"""
        """|_____|\___/|_| \_\_____|    \_/\_/  \___/|_| \_\____/      \_/\_/  \___/|_| \_\_|  |_|"""
    ]

    let startY = 3
    for i in 0 .. titleArt.Length - 1 do
        safeSetCursorPosition (max 0 ((Console.WindowWidth - titleArt.[i].Length) / 2)) (startY + i)
        Console.Write(titleArt.[i])

    Console.ForegroundColor <- ConsoleColor.Yellow
    safeSetCursorPosition 20 10
    Console.Write("=== How to Play ===")

    Console.ForegroundColor <- ConsoleColor.White
    let rules = [
        "  Words spawn at the right side and move left."
        "  Type the exact word and press [Enter] to fire an arrow."
        "  The arrow travels right and destroys the word on contact."
        "  If any word reaches the Dead Line (x=5), it's GAME OVER."
        "  Score = word_length x spawn_rate at time of elimination."
    ]
    for i in 0 .. rules.Length - 1 do
        safeSetCursorPosition 15 (12 + i)
        Console.Write(rules.[i])

    Console.ForegroundColor <- ConsoleColor.DarkGray
    safeSetCursorPosition 15 19
    Console.Write("[Backspace] Delete last character    [ESC] Quit during game")

    Console.ForegroundColor <- ConsoleColor.Green
    safeSetCursorPosition 28 22
    Console.Write("Press any key to start...")

    Console.ResetColor()
    Console.ReadKey(true) |> ignore
    Console.Clear()

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

    Console.ForegroundColor <- ConsoleColor.Green
    safeSetCursorPosition 22 17
    Console.Write("Press [Y] to play again or [ESC] to quit")

    while Console.KeyAvailable do
        Console.ReadKey(true) |> ignore

    let mutable result = false
    let mutable waiting = true
    while waiting do
        let key = Console.ReadKey(true)
        if key.Key = ConsoleKey.Escape then
            waiting <- false
        elif key.KeyChar = 'y' || key.KeyChar = 'Y' then
            result <- true
            waiting <- false
    result
