# Crasher Editor V1.4

A free, offline **Castle Crashers save editor** for Windows, by **3AZYE**.

## Features
- Edit levels, XP, gold, stats, and consumables.
- Change weapons, Animal Orbs, profile unlocks, and Normal/Insane progress.
- Edit all 10 Workshop character slots and give them optional local names.
- Restore automatic backups. Light and dark themes are included.

## How to use
1. **Close Castle Crashers**, then select **Load Save**.
2. Open your `cc_save.dat`. Example location:

   ```text
   C:\Program Files (x86)\Steam\userdata\123456789\204360\remote\cc_save.dat
   ```

   Replace `123456789` with your own Steam account folder number.
3. Make your edits and click **Apply Changes**. The editor backs up the save before writing.

**Limits:** Levels 1–99, combat stats 1–25, consumables 0–9.

**Workshop note:** Names are local labels, not automatic Steam assignments. Empty or unsubscribed custom slots retain their saved data.

**Build from source:** Run `BUILD_CRASHER_EDITOR_V1_4.cmd` on Windows with .NET Framework 4.x.
