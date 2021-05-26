@ECHO off

DEL /q build
dotnet clean source\DefaultEcs.Analyzer\DefaultEcs.Analyzer.csproj -c Release
dotnet clean source\DefaultEcs.Analyzer.Test\DefaultEcs.Analyzer.Test.csproj -c Release

dotnet test source\DefaultEcs.Analyzer.Test\DefaultEcs.Analyzer.Test.csproj -c Release -r build -l trx

IF %ERRORLEVEL% GTR 0 GOTO :end

dotnet pack source\DefaultEcs.Analyzer\DefaultEcs.Analyzer.csproj -c Release -o build /p:LOCAL_VERSION=true

:end