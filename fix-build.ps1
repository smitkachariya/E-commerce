# Stop all E-commerce related processes
Write-Host "Stopping all E-commerce related processes..." -ForegroundColor Yellow

# Stop IIS Express processes
Get-Process -Name "iisexpress*" -ErrorAction SilentlyContinue | ForEach-Object {
    Write-Host "Stopping IIS Express process: $($_.Id)" -ForegroundColor Red
    Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
}

# Stop E-commerce.exe processes
Get-Process -Name "E-commerce*" -ErrorAction SilentlyContinue | ForEach-Object {
    Write-Host "Stopping E-commerce process: $($_.Id)" -ForegroundColor Red
    Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
}

# Stop dotnet processes that might be running the app
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | ForEach-Object {
    if ($_.MainWindowTitle -like "*E-commerce*" -or $_.CommandLine -like "*E-commerce*") {
        Write-Host "Stopping dotnet process: $($_.Id)" -ForegroundColor Red
        Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "Cleaning build artifacts..." -ForegroundColor Yellow
dotnet clean

Write-Host "Removing bin and obj folders..." -ForegroundColor Yellow
Remove-Item -Path "bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "obj" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Building the project..." -ForegroundColor Green
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build successful! You can now run the application." -ForegroundColor Green
} else {
    Write-Host "Build failed. Please check the error messages above." -ForegroundColor Red
}