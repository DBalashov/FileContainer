$CD = $PSScriptRoot

cd $CD\FileContainer

& "dotnet" @("build", "--configuration", "Release")

cd $CD

nuget pack FileContainer.nuspec