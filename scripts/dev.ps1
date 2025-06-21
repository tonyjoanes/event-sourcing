param(
    [Parameter(Position=0)]
    [string]$Command = "help"
)

function Show-Help {
    Write-Host "Event Sourcing Banking Demo - Development Script" -ForegroundColor Green
    Write-Host ""
    Write-Host "Usage: .\scripts\dev.ps1 [command]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Commands:" -ForegroundColor Cyan
    Write-Host "  build     - Build the entire solution" -ForegroundColor White
    Write-Host "  test      - Run all tests" -ForegroundColor White
    Write-Host "  test-unit - Run unit tests only" -ForegroundColor White
    Write-Host "  console   - Run console application" -ForegroundColor White
    Write-Host "  api       - Run web API" -ForegroundColor White
    Write-Host "  ravendb   - Start RavenDB (Docker)" -ForegroundColor White
    Write-Host "  stop-db   - Stop RavenDB (Docker)" -ForegroundColor White
    Write-Host "  clean     - Clean build artifacts" -ForegroundColor White
    Write-Host "  watch     - Watch for changes and rebuild" -ForegroundColor White
    Write-Host ""
}

function Start-RavenDB {
    Write-Host "Starting RavenDB..." -ForegroundColor Green
    docker-compose -f docker/docker-compose.yml up -d ravendb
    Write-Host "RavenDB started! Studio available at: http://localhost:8080" -ForegroundColor Green
}

function Stop-RavenDB {
    Write-Host "Stopping RavenDB..." -ForegroundColor Yellow
    docker-compose -f docker/docker-compose.yml down
    Write-Host "RavenDB stopped!" -ForegroundColor Green
}

function Invoke-Build {
    Write-Host "Building solution..." -ForegroundColor Green
    dotnet build
}

function Invoke-Test {
    Write-Host "Running all tests..." -ForegroundColor Green
    dotnet test
}

function Invoke-UnitTest {
    Write-Host "Running unit tests..." -ForegroundColor Green
    dotnet test --filter Category=Unit
}

function Start-Console {
    Write-Host "Starting console application..." -ForegroundColor Green
    dotnet run --project src/ConsoleApp
}

function Start-API {
    Write-Host "Starting web API..." -ForegroundColor Green
    dotnet run --project src/WebApi
}

function Invoke-Clean {
    Write-Host "Cleaning build artifacts..." -ForegroundColor Yellow
    dotnet clean
    Get-ChildItem -Path . -Include bin,obj -Recurse -Directory | Remove-Item -Recurse -Force
    Write-Host "Clean complete!" -ForegroundColor Green
}

function Start-Watch {
    Write-Host "Watching for changes..." -ForegroundColor Green
    dotnet watch --project src/ConsoleApp
}

# Main script logic
switch ($Command.ToLower()) {
    "help" { Show-Help }
    "build" { Invoke-Build }
    "test" { Invoke-Test }
    "test-unit" { Invoke-UnitTest }
    "console" { Start-Console }
    "api" { Start-API }
    "ravendb" { Start-RavenDB }
    "stop-db" { Stop-RavenDB }
    "clean" { Invoke-Clean }
    "watch" { Start-Watch }
    default {
        Write-Host "Unknown command: $Command" -ForegroundColor Red
        Show-Help
    }
} 