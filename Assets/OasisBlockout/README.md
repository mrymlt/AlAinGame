# SaraScene — Al Ain oasis blockout

The environment uses the artwork in your supplied ZIP: sand-and-stone tiles, six date-palm variants, vegetation, water, a mountain backdrop, a fort and a lookout tower.

## Layout

- **Palm entrance:** MainPlayer starts on the left terrace.
- **Falaj crossing:** Short jumps and stepping stones lead between shallow irrigation basins.
- **Garden terraces:** Raised ground leaves space for future objectives and puzzles.
- **Upper route:** Nine additional ledges create an optional exploration path.
- **Fort courtyard:** The right terrace is the visual destination.

The water is decorative for this blockout. Solid channel beds and map-edge colliders help keep the player within the environment. No swimming, damage, ladders, quest completion or new animation behaviour is implemented.

## Edit the map

Open **Assets → Scenes → SaraScene**. In the Hierarchy, expand **SARA - AL AIN OASIS BLOCKOUT**.

| Group | What to edit |
|---|---|
| 01 Walkable terrain | Move entire named platform groups with the Move tool. Each group has one BoxCollider2D and its tile images. |
| 02 Falaj water | Move or scale the water graphics. They do not have hazard scripts. |
| 03 Date palms and oasis planting | Move, scale or duplicate palms and bushes. |
| 04 Background and landmarks | Adjust the sky, distant mountain, fort and lookout tower. |
| 05 Design markers | Empty editor-only objects marking start, destination and future puzzle space. |
| 06 Map edge colliders | Invisible walls at the left and right ends. |

Stop Play Mode before saving changes. Select the blockout root and press **F** in Scene view to frame the map. The Game camera starts at the player and uses the existing follow code.

The existing environment and mission objects are retained under **Previous layout - disabled backup**. Keep that group disabled while using the oasis layout. Character movement, dialogue and animation scripts are unchanged.

Imported art is in **Assets/OasisBlockout/Art**. The palms are sliced into six sprites. You can expand the palm atlas in the Project panel to choose a different variant.

The editor tool **Al Ain → Oasis → Build SaraScene blockout** installs the initial layout if it is missing. It does not rebuild over an existing blockout. **Al Ain → Oasis → Export overview preview** renders overview and entrance images into the project's **Logs/OasisPreviews** folder.

SaraScene retains the GUID of the renamed SampleScene; the existing build-settings entry is updated to its new path.
