#!/bin/bash

killall MuseDash.exe
mv ./Playlists/bin/Debug/net6.0/Playlists.dll "$MD_DIRECTORY/Mods"
xdg-open "steam://rungameid/774171"
