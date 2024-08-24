# Playlists
A mod for Muse Dash that allows making custom playlists.

## Installation

- Download and install MelonLoader v0.6.1 (**any other version will NOT work!**)
- Download the [latest release](https://github.com/MDMods/Playlists/releases) and place it in your `Mods` folder.

## Usage

- Playlists are located at `Muse Dash/UserData/Playlists`.
- Use any text editor to edit the files.
- To create a new playlist, simply create a new file.

### File Format

| Field    | Description                                                |
|----------|------------------------------------------------------------|
| name     | The displayed name of the playlist.                        |
| icon     | The icon of the playlist. Can be a local file path or URL. |
| position | The position this playlist appears at.                     |
| albums   | The Albums/Songs in this playlist. Uses the game's UIDs.   |

> If you want to find a UID you can use https://musedash.moe. The UID will be in the URL. (https://musedash.moe/music/<uid\>)

Note that the playlists will only update once you restart the game.
