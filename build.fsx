// include Fake libs
#r "./packages/FAKE/tools/FakeLib.dll"

open Fake
open NpmHelper

// Directories
let buildDir  = "./build/"
let deployDir = "./deploy/"


// Filesets
let appReferences  =
    !! "/**/*.csproj"
    // ++ "/**/*.fsproj"

// version info
let version = "0.1"  // or retrieve from CI server

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; deployDir]
)

Target "NpmInstall" (fun _ ->
    Npm (fun p -> 
        {
            p with 
                Command = Install Standard
                WorkingDirectory = "./Designer.Web/www"
        })
)

Target "BuildAngularApp" (fun _ ->
    Npm (fun p -> 
        {
            p with 
                Command = Run "tsc"
                WorkingDirectory = "./Designer.Web/www"
        })
)

Target "Start" (fun _ ->
    Npm (fun p -> 
        {
            p with 
                Command = Run "start"
                WorkingDirectory = "./Designer.Web/www"
        })
)

Target "BuildBackend" (fun _ ->
    // compile all projects below src/app/
    MSBuildDebug buildDir "Build" appReferences
    |> Log "AppBuild-Output: "
)

Target "Build" DoNothing

// Build order backend
"Clean"
  ==> "BuildBackend"

// build order frontend
"NpmInstall"
  ==> "BuildAngularApp"

// build frontend and backend
"BuildBackend"
  ==> "BuildAngularApp"
  ==> "Build"

// start angular app standalone without backend
"BuildAngularApp"
  ==> "Start"

// start build
RunTargetOrDefault "Build"
