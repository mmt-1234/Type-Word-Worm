module Program

open System
open GameConfig
open GameTypes
open GameLogic
open GameRender

[<EntryPoint>]
let main argv =
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

    while not state.GameOver && not state.Quit do
        let now = DateTime.UtcNow
        let dt = (now - lastTime).TotalSeconds
        lastTime <- now

        processInput state
        updateGame state dt
        renderGame state

        System.Threading.Thread.Sleep(33)

    Console.CursorVisible <- true
    Console.ResetColor()

    if state.GameOver then
        let elapsed = (DateTime.UtcNow - state.StartTime).TotalSeconds
        showGameOverScreen state.Score elapsed

    Console.Clear()
    0
