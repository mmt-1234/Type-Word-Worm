module GameLogic

open System
open GameConfig
open GameTypes

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
                state.InputBuffer <- ""
        | _ ->
            let c = keyInfo.KeyChar
            if Char.IsLetterOrDigit(c) || c = '#' || c = '.' || c = '-' then
                if state.InputBuffer.Length < 20 then
                    state.InputBuffer <- state.InputBuffer + string c

let selectSafeSpawnY (activeWords: list<Word>) =
    let candidates = [ 4.0 .. 16.0 ]
    let safeCandidates =
        candidates
        |> List.filter (fun y ->
            not (activeWords |> List.exists (fun w -> Math.Abs(w.SpawnY - y) < 0.1 && w.X >= 55.0))
        )

    if not (List.isEmpty safeCandidates) then
        safeCandidates.[random.Next(safeCandidates.Length)]
    else
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

    for w in state.Words do
        w.X <- w.X - v * dt
        if w.X <= 5.0 then
            state.GameOver <- true

    for p in state.Projectiles do
        p.X <- p.X + p.Speed * dt
        let targetWordOpt = state.Words |> List.tryFind (fun w -> w.Id = p.TargetId)
        match targetWordOpt with
        | Some target -> p.Y <- int (Math.Round(target.SpawnY))
        | None -> ()

    let mutable collisions = []
    let mutable remainingProjectiles = []

    for p in state.Projectiles do
        let targetWordOpt = state.Words |> List.tryFind (fun w -> w.Id = p.TargetId)
        match targetWordOpt with
        | Some target ->
            if p.X >= target.X then
                collisions <- (target.Id, p.X, float p.Y) :: collisions
            else
                remainingProjectiles <- p :: remainingProjectiles
        | None -> ()

    state.Projectiles <- remainingProjectiles

    for (wordId, cx, cy) in collisions do
        let wordOpt = state.Words |> List.tryFind (fun w -> w.Id = wordId)
        match wordOpt with
        | Some word ->
            let points = calcScore word.Text.Length spawnRate
            state.Score <- state.Score + points
            state.Words <- state.Words |> List.filter (fun w -> w.Id <> wordId)

            for i in 0 .. 11 do
                let angle = float i * (2.0 * Math.PI / 12.0)
                let speed = 6.0 + random.NextDouble() * 6.0
                let p = {
                    X = cx
                    Y = cy
                    Vx = cos angle * speed
                    Vy = sin angle * speed * 0.4
                    Age = 0.0
                    MaxAge = 0.3 + random.NextDouble() * 0.3
                    Char = match random.Next(3) with | 0 -> '*' | 1 -> '+' | _ -> '.'
                    Color = match random.Next(3) with | 0 -> ConsoleColor.Yellow | 1 -> ConsoleColor.Red | _ -> ConsoleColor.DarkYellow
                }
                state.Particles <- p :: state.Particles
        | None -> ()

    for p in state.Particles do
        p.Age <- p.Age + dt
        p.X <- p.X + p.Vx * dt
        p.Y <- p.Y + p.Vy * dt

    state.Particles <- state.Particles |> List.filter (fun p -> p.Age < p.MaxAge)
