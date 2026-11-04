@echo off
setlocal
cd /d "%~dp0"
title Zlora - ASP.NET Core MVC 9
echo Compilando o projeto...
dotnet build Zlora.csproj --nologo
if errorlevel 1 goto erro
echo.
echo Abra http://localhost:5080 no navegador.
echo Cadastre o estabelecimento para criar sua conta de administrador.
echo Pressione Ctrl+C para encerrar.
if exist ".runtime\dotnet.exe" (
    ".runtime\dotnet.exe" "bin\Debug\net9.0\Zlora.dll" --urls http://localhost:5080
) else (
    dotnet run --no-build --no-launch-profile --urls http://localhost:5080
)
if errorlevel 1 goto erro
exit /b 0
:erro
echo.
echo Verifique o MongoDB e a instalacao do SDK e runtime .NET 9. Consulte README.md.
pause
exit /b 1
