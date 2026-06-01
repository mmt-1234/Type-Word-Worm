module GameTypes

open System

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
