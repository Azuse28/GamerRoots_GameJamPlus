# Portal

16-bit style animated effect sprites.

- **Multiple cell sizes: 32x32 px, 64x64 px, 128x128 px**, Standard tier: 32x32 px, Premium tier(s): 64x64, 128x128 px
- **Anchor:** center - the same point in every frame, so the
  sprite never slides inside its cell
- **Perspective:** effect
- **Palette:** 37 colours, locked across every frame
- **Transparency:** binary alpha only (0 or 255), safe for point filtering

## Animations

| Clip | Frames | FPS | Loops | Location in combined sheet |
|------|--------|-----|-------|----------------------------|
| `rift_normal_32` | 16 | 12 | yes | row 0 |
| `rift_red_32` | 16 | 12 | yes | row 0 |
| `rift_blue_32` | 16 | 12 | yes | row 0 |
| `rift_green_32` | 16 | 12 | yes | row 0 |
| `gate_normal_32` | 16 | 12 | yes | row 0 |
| `gate_normal_32_nofloor` | 16 | 12 | yes | row 0 |
| `gate_red_32` | 16 | 12 | yes | row 0 |
| `gate_red_32_nofloor` | 16 | 12 | yes | row 0 |
| `gate_blue_32` | 16 | 12 | yes | row 0 |
| `gate_blue_32_nofloor` | 16 | 12 | yes | row 0 |
| `gate_green_32` | 16 | 12 | yes | row 0 |
| `gate_green_32_nofloor` | 16 | 12 | yes | row 0 |
| `weave_normal_32` | 16 | 12 | yes | row 0 |
| `weave_red_32` | 16 | 12 | yes | row 0 |
| `weave_blue_32` | 16 | 12 | yes | row 0 |
| `weave_green_32` | 16 | 12 | yes | row 0 |

## Files

```
sheets/portal_sheet.png            all clips, one row per clip
sheets/portal_sheet.json           grid + clip metadata
sheets/portal_sheet.aseprite.json  Aseprite array format, with frameTags
sheets/portal_<clip>.png           one sheet per clip, single row
sheets/portal_*_n.png              normal maps, if the pack ships them:
                                              same grid and same cell as the sheet
                                              they sit beside, so one importer
                                              setting covers both
frames/<clip>/000.png                         individual frames
preview/                                      gifs and contact sheet
```

See `LICENSE.txt` for usage terms.

This is the **Standard** edition. Held back for Premium: gate_blue_128, gate_blue_128_nofloor, gate_blue_64, gate_blue_64_nofloor, gate_green_128, gate_green_128_nofloor, gate_green_64, gate_green_64_nofloor, gate_normal_128, gate_normal_128_nofloor, gate_normal_64, gate_normal_64_nofloor, gate_red_128, gate_red_128_nofloor, gate_red_64, gate_red_64_nofloor, rift_blue_128, rift_blue_64, rift_green_128, rift_green_64, rift_normal_128, rift_normal_64, rift_red_128, rift_red_64, weave_blue_128, weave_blue_64, weave_green_128, weave_green_64, weave_normal_128, weave_normal_64, weave_red_128, weave_red_64.
