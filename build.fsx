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

Target "Angular" (fun _ ->
    Npm (fun p -> 
        {
            p with 
                Command = Run "tsc"
                WorkingDirectory = "./Designer.Web/www"
        })
)

Target "Webserver" (fun _ ->
    Npm (fun p -> 
        {
            p with 
                Command = Run "lite"
                WorkingDirectory = "./Designer.Web/www"
        })
)

Target "Build" (fun _ ->
    // compile all projects below src/app/
    MSBuildDebug buildDir "Build" appReferences
    |> Log "AppBuild-Output: "
)

// Target "Deploy" (fun _ ->
//     !! (buildDir + "/**/*.*")
//     -- "*.zip"
//     |> Zip buildDir (deployDir + "ApplicationName." + version + ".zip")
// )

Target "Finish" DoNothing

// Build order
"Clean"
  ==> "NpmInstall"
  ==> "Angular"
  ==> "Build"
  // ==> "Deploy"
  ==> "Finish" 

"Build"
  ==> "Webserver"

// start build
RunTargetOrDefault "Build"
