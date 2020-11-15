@ECHO off

DEL /q package
dotnet clean source\DefaultEcs.Analyzer\DefaultEcs.Analyzer.csproj -c Release
dotnet clean source\DefaultEcs.Analyzer.Test\DefaultEcs.Analyzer.Test.csproj -c Release

dotnet test source\DefaultEcs.Analyzer.Test\DefaultEcs.Analyzer.Test.csproj -c Release -r package -l trx

IF %ERRORLEVEL% GTR 0 GOTO :end

dotnet pack source\DefaultEcs.Analyzer\DefaultEcs.Analyzer.csproj -c Release -o package /p:LOCAL_VERSION=true

:end