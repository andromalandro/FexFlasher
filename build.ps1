# build.ps1
param (
    [string]$Runtime = "win-x64"
)

Write-Host "🚀 Building FexFlasher for $Runtime ..."

# Clean old builds
if (Test-Path "dist") { Remove-Item "dist" -Recurse -Force }

# Run publish
dotnet publish FexFlasher.csproj `
    -c Release `
    -r $Runtime `
    -p:PublishSingleFile=true `
    -p:SelfContained=true `
    -p:EnableCompressionInSingleFile=true `
    -o "dist"

Write-Host "`n✅ Build complete!"
Write-Host "Your single EXE is here: dist\FexFlasher.exe"
