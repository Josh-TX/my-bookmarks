DEL /S /Q Server\wwwroot
xcopy UI\* Server\wwwroot
dotnet run --project Server
timeout 3