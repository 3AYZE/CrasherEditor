# Crasher Editor V1.4

A free, offline **Castle Crashers save editor** for Windows. Edit your own local Steam save; this is **not** a live-game trainer or multiplayer cheat.

### Features
- Edit character level, XP, gold, combat stats, and consumables.
- Change weapons, Animal Orbs, and Normal/Insane Mode progress.
- Edit 10 custom Workshop-character save slots; local slot names do not change save data.
- Manage profile unlocks and restore automatic backups.
- Dark interface and one **Apply Changes** action.

### Save location
```text
C:\Program Files (x86)\Steam\userdata\<SteamAccountID>\204360\remote\cc_save.dat
```
Example: `C:\Program Files (x86)\Steam\userdata\123456789\204360\remote\cc_save.dat`

### Use
1. **Close Castle Crashers.**
2. Open Crasher Editor and select **Load Save**.
3. Choose your `cc_save.dat`, edit values, then select **Apply Changes**.
4. A backup is created before writing. Use **Backups** to restore.

**Supported ranges:** Level 1–99, stats 1–25, consumables 0–9.

**Creator:** 3AZYE

### Build
Run `BUILD_CRASHER_EDITOR_V1_4.cmd` on Windows with .NET Framework 4.x installed. GitHub Actions builds the release and publishes the EXE with a SHA-256 checksum.

*Workshop names are local labels, not automatically verified Steam assignments. Empty/unsubscribed custom slots retain their stored progression.*
