# CosmicMachine

## Galaxy compiler

Galaxy was written in C#-ish language. All source code is in [CSharpGalaxy](CSharpGalaxy) directory.

Main code of the C# to alien language compiler is located in [Cs2SkiCompiler.cs](Cs2SkiCompiler.cs) and [CompilerPrimitives.cs](Cs2SkiCompiler.cs).

Galaxy consists of Stages and Stage management module OSModule.cs

Stages:

* GalaxyCounterModule
* ShooterModule
* RacesModule
* TicTacToeModule
* MatchingPuzzleModule
* GameManagementModule
* PlanetWarsModule


Use `../_compile-galaxy.sh` to compile galaxy from sources.

### C#-ish language

Do support:

* unary, binary and ternary operators
* arithmetics and boolean opearations
* method declarations
* local function declarations
* recursion
* variable declaration
* if-return statements
* if-return-else-return statements
* types (not used during actual compilation of the galaxy, but are checked by IDE and during CosmicMachine project compilation)
* classes (object is equivalent to list)
* class field reassignment (sic!)
* null for nil
* macros for image rendering
* tuples
* list deconstruction

Do not support:

* arbitrarty if statements
* loops
* instance methods
* namespaces
* and almost all other functions of C# 

## Message rendering

Source code for the message is located in [message.txt](message.txt)

Use `../_render-messages.sh` to render message images from source.