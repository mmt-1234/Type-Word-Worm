module Program

open System
open GameConfig
open GameTypes
open GameLogic
open GameRender

let createState () = {
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

let runGame () =
    let state = createState ()
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
    state

[<EntryPoint>]
let main _ =
    Console.Title <- "Type Word Worm"
    Console.CursorVisible <- false

    let rec playLoop () =
        showStartScreen ()

        let state = runGame ()

        if state.GameOver then
            let elapsed = (DateTime.UtcNow - state.StartTime).TotalSeconds
            let playAgain = showGameOverScreen state.Score elapsed
            if playAgain then
                Console.CursorVisible <- false
                playLoop ()

    playLoop ()
    Console.Clear()
    0
