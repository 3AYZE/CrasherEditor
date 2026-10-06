# Castle Editor [Castle Crasher]

## Crasher Editor V1.3

Offline Castle Crashers save editor for your own local Steam profile.

### Features

- Character unlocks
- Level, XP, Gold, and supported stats
- Weapons and Animal Orbs
- Consumables and equipment
- Story/progression editing
- Profile unlocks
- Character balancing and MAX options
- Automatic backups and restore
- Light and Dark themes

### How to use

1. Completely close Castle Crashers.
2. Open Crasher Editor and click **Load Save**.
3. Select your original `cc_save.dat` from:
   `Steam\userdata\<account>\204360\remote\cc_save.dat`
4. Make your changes.
5. Press **Apply Changes**. A backup is created before the save is written.

Supported limits are Level 1–99 and combat stats 1–25.

### V1.3 trust / compatibility cleanup

V1.3 uses a user-selected save instead of scanning Steam accounts. It does not scan the Windows registry, enumerate running processes, launch Explorer, inject into Castle Crashers, modify process memory, or use native Windows theme APIs. The editor only reads/writes the save file the user selects and creates local backups beside it.

If a save fails validation, check:
`%TEMP%\Crasher_Editor_V1.3.log`

### Build from source

Run `BUILD_CRASHER_EDITOR_V1_3.cmd` on Windows with .NET Framework 4.x available.

Output:

`Crasher Editor V1.3.exe`

GitHub Actions also builds the same public source and uploads the EXE as a workflow artifact. Tagged builds can publish the EXE to GitHub Releases automatically.
