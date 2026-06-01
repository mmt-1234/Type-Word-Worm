module GameConfig

open System

let screenWidth = 80
let screenHeight = 24

let currentBuffer = Array2D.create screenHeight screenWidth ' '
let currentColor = Array2D.create screenHeight screenWidth ConsoleColor.Gray
let prevBuffer = Array2D.create screenHeight screenWidth ' '
let prevColor = Array2D.create screenHeight screenWidth ConsoleColor.Gray

let wordPool = [
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

let calcSpeed elapsed = 3.0 + (elapsed / 10.0) * 0.5
let calcSpawnRate elapsed = 0.5 + (elapsed / 20.0) * 0.5
let calcScore wordLength spawnRate = int (Math.Round(float wordLength * spawnRate))
