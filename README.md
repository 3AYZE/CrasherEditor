# Castle Editor [Castle Crasher]

## Crasher Editor V1.3

**Crasher Editor V1.3** is a Windows save editor for **Castle Crashers** that lets you modify your own local Steam save while the game is closed.

### What it can edit

- Character unlocks
- Level, XP, Gold, and combat stats
- Potions, Bombs, and Sandwiches
- Weapons and Animal Orbs
- Normal and Insane Mode progression
- Profile unlocks and key items
- Character balance and MAX options
- Automatic backups and restore
- Light and Dark themes

The editor only changes the save file you select. It does not inject into Castle Crashers, modify other players, or require the game to be running.

### How to use

1. Completely close Castle Crashers.
2. Open Crasher Editor and click **Load Save**.
3. Select your original `cc_save.dat` from:
   `Steam\userdata\<account>\204360\remote\cc_save.dat`
4. Make your changes.
5. Press **Apply Changes** when finished.
6. A backup is automatically created before the save is written.

### Supported limits

- Level: 1–99
- Strength: 1–25
- Defense: 1–25
- Magic: 1–25
- Agility: 1–25

### V1.3

V1.3 improves save loading, error messages, compatibility, and keeps the editor focused on the selected local save file.

If a save fails to load, check:

`%TEMP%\Crasher_Editor_V1.3.log`

### Build from source

Run `BUILD_CRASHER_EDITOR_V1_3.cmd` on Windows with .NET Framework 4.x available.

Output:

`Crasher Editor V1.3.exe`

GitHub Actions builds the public source and publishes the release EXE together with a SHA-256 checksum.
