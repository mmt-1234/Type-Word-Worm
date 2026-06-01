module GameInput

open System
open GameConfig

let safeSetCursorPosition (x: int) (y: int) =
    let clampedX = max 0 (min x (Console.BufferWidth - 1))
    let clampedY = max 0 (min y (Console.BufferHeight - 1))
    try
        Console.SetCursorPosition(clampedX, clampedY)
    with
    | _ -> ()

let render () =
    for y in 0 .. screenHeight - 1 do
        for x in 0 .. screenWidth - 1 do
            if currentBuffer.[y, x] <> prevBuffer.[y, x] || currentColor.[y, x] <> prevColor.[y, x] then
                safeSetCursorPosition x y
                Console.ForegroundColor <- currentColor.[y, x]
                Console.Write(currentBuffer.[y, x])
                prevBuffer.[y, x] <- currentBuffer.[y, x]
                prevColor.[y, x] <- currentColor.[y, x]
