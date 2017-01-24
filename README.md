# Apex Gallery
Aspnetcore based image gallery. JSON backend with a jQuery frontend. Supports interface and folder name localisation. Tested on CentOS 7.2, default thumbnail command will not function on Windows.

A live version can be viewed [here](https://portfolio.helifreak.club/gallery/).

# Install
- Copy appsettings.default.json to appsettings.json and edit.
- Copy Views/Home/Index.default.cshtml and Contact.default.cshtml to Index.cshtml and Contact.cshtml respectively and edit.
- Run dotnet publish in src\Gallery
- Server will require image magick installed to generate thumbnails

## Systemd Unit
```
[Unit]
Description=Gallery
After=network.target
Wants=network.target

[Service]
User=<user>
Group=<group>
KillMode=control-group
SuccessExitStatus=0 1

NoNewPrivileges=true
ReadWriteDirectories=/var/www/html/gallery
WorkingDirectory=/var/www/html/gallery/bin
ExecStart=/usr/bin/dotnet Gallery.dll

[Install]
WantedBy=multi-user.target
```

Or use `screen dotnet Gallery.dll`