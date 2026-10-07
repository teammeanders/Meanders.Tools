@echo off
setlocal

title Meanders.Tools - Component Data Generator

echo.
echo ==================================================
echo   Meanders.Tools Component Data Generator
echo ==================================================
echo.
echo Press any key to start generation...
pause >nul

echo.
echo Generating component data...
echo.

python "%~dp0generate-component-data.py"

if errorlevel 1 (
    echo.
    echo ==================================================
    echo   GENERATION FAILED
    echo ==================================================
    echo.
    echo Fix the error above and run the generator again.
    echo.
    echo Press any key to exit...
    pause >nul
    exit /b 1
)

echo.
echo ==================================================
echo   GENERATION COMPLETE
echo ==================================================
echo.
echo data/components.json has been generated successfully.
echo.
echo Press any key to exit...
pause >nul

endlocal