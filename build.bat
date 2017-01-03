REM optional clean
REM rm -rf artifacts CoreTests/bin CoreTests/obj CoreIntegrationTests/bin CoreIntegrationTests/obj Microsoft.AspNetCore.Identity.MongoDB/bin Microsoft.AspNetCore.Identity.DocumentDB/obj

dotnet restore
dotnet test -c Release test/CoreTests
dotnet test -c Release test/CoreIntegrationTests
dotnet pack -c Release -o artifacts src/AspNetCore.Identity.DocumentDB

REM nuget add artifacts\X.nupkg -Source C:\Code\scratch\localnugetfeedtesting
REM nuget publish artifacts\X.nupkg
