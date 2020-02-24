@ECHO off

DEL /q package
dotnet clean source\DefaultEcs.Analyzer\DefaultEcs.Analyzer.csproj -c Release
dotnet clean source\DefaultEcs.Analyzer.Test\DefaultEcs.Analyzer.Test.csproj -c Release

dotnet test source\DefaultEcs.Analyzer.Test\DefaultEcs.Analyzer.Test.csproj -c Release -r package -l trx

IF %ERRORLEVEL% GTR 0 GOTO :end

dotnet clean source\DefaultEcs\DefaultEcs.Analyzer.csproj -c Release
dotnet clean source\DefaultEcs\DefaultEcs.Analyzer.Release.csproj -c Release

dotnet pack source\DefaultEcs\DefaultEcs.Analyzer.Release.csproj -c Release -o package

:end