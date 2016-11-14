// include Fake libs
#r "./packages/FAKE/tools/FakeLib.dll"

open Fake
open NpmHelper
open ProcessHelper

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

Target "Run" (fun _ ->
    let status = ExecProcessElevated "/usr/local/bin/mono" (buildDir </> "Designer.Web.exe") System.TimeSpan.MaxValue
    printfn "Status: %d" status
)

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
  ==> "Run"

// start angular app standalone without backend
"BuildAngularApp"
  ==> "Start"

// start build
RunTargetOrDefault "Build"
