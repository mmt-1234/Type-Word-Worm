module Tests

open System
open Xunit
open GameTypes
open GameConfig
open GameLogic

// ===== Speed Formula: v = 3.0 + (elapsed / 10.0) * 0.5 =====

[<Fact>]
let ``speed at t=0 is 3.0`` () =
    Assert.Equal(3.0, calcSpeed 0.0)

[<Fact>]
let ``speed at t=10 is 3.5`` () =
    Assert.Equal(3.5, calcSpeed 10.0)

[<Fact>]
let ``speed at t=20 is 4.0`` () =
    Assert.Equal(4.0, calcSpeed 20.0)

[<Fact>]
let ``speed increases over time`` () =
    Assert.True(calcSpeed 30.0 > calcSpeed 20.0)

// ===== Spawn Rate Formula: spawnRate = 0.5 + (elapsed / 20.0) * 0.5 =====

[<Fact>]
let ``spawn rate at t=0 is 0.5`` () =
    Assert.Equal(0.5, calcSpawnRate 0.0)

[<Fact>]
let ``spawn rate at t=20 is 1.0`` () =
    Assert.Equal(1.0, calcSpawnRate 20.0)

[<Fact>]
let ``spawn rate at t=40 is 1.5`` () =
    Assert.Equal(1.5, calcSpawnRate 40.0)

[<Fact>]
let ``spawn rate increases over time`` () =
    Assert.True(calcSpawnRate 40.0 > calcSpawnRate 20.0)

// ===== Score Formula: points = round(wordLength * spawnRate) =====

[<Fact>]
let ``score: 6-char word at rate 0.5 is 3`` () =
    Assert.Equal(3, calcScore 6 0.5)

[<Fact>]
let ``score: 10-char word at rate 1.0 is 10`` () =
    Assert.Equal(10, calcScore 10 1.0)

[<Fact>]
let ``score: 7-char word at rate 0.75 rounds correctly`` () =
    // round(7 * 0.75) = round(5.25) = 5
    Assert.Equal(5, calcScore 7 0.75)

[<Fact>]
let ``score scales with longer words`` () =
    Assert.True(calcScore 10 1.0 > calcScore 5 1.0)

[<Fact>]
let ``score scales with higher spawn rate`` () =
    Assert.True(calcScore 6 2.0 > calcScore 6 1.0)

// ===== Spawn Y Selection =====

[<Fact>]
let ``spawn Y with no active words is within valid range`` () =
    let y = selectSafeSpawnY []
    Assert.True(y >= 4.0 && y <= 16.0, sprintf "Expected 4..16 but got %f" y)

[<Fact>]
let ``spawn Y avoids rows with words near right edge`` () =
    // Block rows 4..14 with words at X=60 (>= 55 threshold)
    let blockedWords =
        [ 4.0 .. 14.0 ]
        |> List.map (fun y ->
            { Id = Guid.NewGuid(); Text = "x"; X = 60.0; SpawnY = y; IsTargeted = false })
    // Run multiple times to rule out lucky random picks
    for _ in 1 .. 30 do
        let y = selectSafeSpawnY blockedWords
        Assert.True(y >= 15.0 && y <= 16.0, sprintf "Expected 15..16 but got %f" y)

[<Fact>]
let ``spawn Y with all rows blocked falls back to leftmost word row`` () =
    // Fill ALL rows (4..16) with words at X=60
    let allBlockedWords =
        [ 4.0 .. 16.0 ]
        |> List.map (fun y ->
            { Id = Guid.NewGuid(); Text = "x"; X = 60.0; SpawnY = y; IsTargeted = false })
    let y = selectSafeSpawnY allBlockedWords
    Assert.True(y >= 4.0 && y <= 16.0, sprintf "Expected 4..16 but got %f" y)
