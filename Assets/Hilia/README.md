# HiliaScene — Hili Grand Tomb

Open `Assets/Scenes/HiliaScene.unity` in Unity and press Play. This is a separate, playable Hili mission using Maryam's complete MissionOne layout: the original tomb, palms, shrubs, full sandy route, puzzle chamber, lever and exit gate.

## Play

- Read Abu Rashid and Shamma's introduction, followed by two short puzzle instructions.
- Walk with A/D or the arrow keys. Jump with Space. On a phone, use the left, right and Jump buttons.
- Approach the lever at the tomb. Press **E**, click/tap the lever, or tap **Use lever**.
- Each press changes all four carvings. The correct order is **oryx — person — person — oryx**. From the starting arrangement, two presses solve it.
- The gate rises, the Carved Stone counter becomes **1 / 1**, and the completion conversation opens.
- **How to play** repeats the instructions. **Hint** gives the clue. Click or tap Shamma to replay the introduction or completed conversation.

Movement and puzzle input pause while dialogue is open. Close the dialogue with **X**, or continue through its pages.

## Edit text and portraits

Stop Play Mode first, then select **02 Shamma → MainPlayer** in the Hierarchy. In **Hilia Dialogue**, expand:

| Field | Used for |
| --- | --- |
| Introduction | The five story lines from your Hili table |
| How To Play | The instructions shown after the introduction and from the button |
| Hint | The clue about two people and an oryx on each side |
| Solved | The final conversation and Carved Stone message |

Each element has **Speaker**, **Text**, and **Portrait**. Change the words or drag a Sprite into Portrait. To add a page, increase the array size and fill the new element. A blank Portrait displays a small Hili seal. Turn off **Open Introduction On Start** if you only want dialogue when Shamma is tapped.

The instructions are stored once: changing **How To Play** updates both the opening explanation and the replay button. Save the scene after editing.

## Scene organization

- **00 Systems:** camera, sky and input EventSystem.
- **01 MissionOne - Hili Grand Tomb:** the whole mission, grouped into Ground, Scenery, Puzzle Chamber and Puzzle Controls. Scenery contains the original tomb, grouped date palms and grouped shrubs. Puzzle Controls contains four evenly spaced carving slots, the lever, exit gate and mission controller.
- **02 Shamma:** the existing character, movement and animation controller, plus editable dialogue.
- **03 Interface:** objective, token counter, dialogue and touch controls.

All 45 sprite objects from Maryam's MissionOne were brought across. The tomb artwork, size and placement are preserved. The four interactive carvings use tighter copies of the same images to keep each figure readable. A continuous collider supports the sandy route; the copied decorative ground colliders are disabled. The camera follows the longer level and the lever retains its original press sound.

The new scene reuses the project's Hili artwork, Shamma character, Abu Rashid/Shamma portraits, StateCycler and DoorUnlock. The four carving images have separate copies with tighter Unity sprite crops under `Assets/Hilia/Art`; the original assets are unchanged.

## Scope and verification

MaryamScene and SaraScene were left unchanged. The new scene and supporting runtime files are under `Assets/Scenes/HiliaScene.unity` and `Assets/Hilia`.

This scene contains **one Hili mission**. The Carved Stone is tracked for the current play session. Saving progress, travelling to the next scene, and adding this scene to a build profile are not connected yet.

Unity 6000.3.5f1 play-mode checks covered the full walking route from tomb to chamber, camera follow, dialogue paging and portraits, keyboard/touch movement, nearby interaction, blocking puzzle input during dialogue, the winning order, gate opening, one reward only, and replaying completed dialogue. Desktop and phone previews were rendered in Unity. Validation ran in a separate copy of the project. Unity's existing Search database exception appeared during editor startup; the Hili checks completed successfully.
