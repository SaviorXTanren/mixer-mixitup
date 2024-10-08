msbuild /t:Clean,Build /property:Configuration=Release mixer-mixitup.sln

signtool.exe sign /fd sha256 /n "Blazing Cacti LLC" /tr http://ts.ssl.com /td sha256 /v ".\MixItUp.Installer\bin\Release\MixItUp-Setup.exe" ".\MixItUp.WPF\bin\Release\MixItUp.exe" ".\MixItUp.WPF\bin\Release\MixItUp.Reporter.exe" ".\MixItUp.WPF\bin\Release\MixItUp.API.dll" ".\MixItUp.WPF\bin\Release\MixItUp.Base.dll" ".\MixItUp.WPF\bin\Release\MixItUp.SignalR.Client.dll"
signtool.exe verify /pa ".\MixItUp.Installer\bin\Release\MixItUp-Setup.exe" ".\MixItUp.WPF\bin\Release\MixItUp.exe" ".\MixItUp.WPF\bin\Release\MixItUp.Reporter.exe" ".\MixItUp.WPF\bin\Release\MixItUp.API.dll" ".\MixItUp.WPF\bin\Release\MixItUp.Base.dll" ".\MixItUp.WPF\bin\Release\MixItUp.SignalR.Client.dll"

signtool.exe sign /fd sha256 /n "Blazing Cacti LLC" /tr http://ts.ssl.com /td sha256 /v ".\MixItUp.Installer\bin\Debug\MixItUp-Setup.exe" ".\MixItUp.WPF\bin\Debug\MixItUp.exe" ".\MixItUp.WPF\bin\Debug\MixItUp.Reporter.exe" ".\MixItUp.WPF\bin\Debug\MixItUp.API.dll" ".\MixItUp.WPF\bin\Debug\MixItUp.Base.dll" ".\MixItUp.WPF\bin\Debug\MixItUp.SignalR.Client.dll"
signtool.exe verify /pa ".\MixItUp.Installer\bin\Debug\MixItUp-Setup.exe" ".\MixItUp.WPF\bin\Debug\MixItUp.exe" ".\MixItUp.WPF\bin\Debug\MixItUp.Reporter.exe" ".\MixItUp.WPF\bin\Debug\MixItUp.API.dll" ".\MixItUp.WPF\bin\Debug\MixItUp.Base.dll" ".\MixItUp.WPF\bin\Debug\MixItUp.SignalR.Client.dll"

powershell -NoLogo -NoProfile -Command Compress-Archive -Path ".\MixItUp.WPF\bin\Release\*" -DestinationPath "..\MixItUp.zip"
copy .\MixItUp.WPF\Changelog.html ..\