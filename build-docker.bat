DEL /S /Q Server\wwwroot
DEL /S /Q Server\data
xcopy UI\* Server\wwwroot
docker build -t my-bookmarks .
timeout 3