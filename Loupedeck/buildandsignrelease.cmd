msbuild /t:Clean,Build /property:Configuration=Release MixItUp3Plugin\MixItUp3Plugin.csproj

signtool.exe sign /fd sha256 /n "Blazing Cacti LLC" /tr http://ts.ssl.com /td sha256 /v ".\bin\Release\win\MixItUp.API.dll" ".\bin\Release\win\MixItUp.LoupedeckPlugin.dll"
signtool.exe verify /pa ".\bin\Release\win\MixItUp.API.dll" ".\bin\Release\win\MixItUp.LoupedeckPlugin.dll"

del ".\MixItUp_Loupedeck.zip"
powershell -NoLogo -NoProfile -Command Compress-Archive -Path ".\bin\Release\*" -DestinationPath ".\MixItUp_Loupedeck.zip"
move ".\MixItUp_Loupedeck.zip" ".\MixItUp_Loupedeck.lplug4"