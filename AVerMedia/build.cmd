del ".\MixItUp_AVerMedia.zip"
powershell -NoLogo -NoProfile -Command Compress-Archive -Path ".\MixItUpPlugin\*" -DestinationPath ".\MixItUp_AVerMedia.zip"
move ".\MixItUp_AVerMedia.zip" ".\MixItUp_AVerMedia.creatorCentral"