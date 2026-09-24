# HiliaScene — Hili Grand Tomb

Open `Assets/Scenes/HiliaScene.unity` in Unity and press Play. This is a separate, playable Hili mission using Maryam's complete MissionOne layout: the original tomb, palms, shrubs, full sandy route, puzzle chamber, lever and exit gate.

## Play

- Read Abu Rashid and Shamma's introduction, followed by puzzle and coin instructions.
- Walk with A/D or the arrow keys. Jump with Space. On a phone, use the left, right and Jump buttons.
- Approach the lever at the tomb. Press **E**, click/tap the lever, or tap **Use lever**.
- Each press changes all four carvings. The correct order is **oryx — person — person — oryx**. From the starting arrangement, two presses solve it.
- The gate rises, the Carved Stone counter becomes **1 / 1**, and the completion conversation opens.
- **How to play** repeats the instructions. **Hint** gives the clue. Click or tap Shamma to replay the introduction or completed conversation.
- Walk or jump through the 18 golden dallah coins along the path. Four coins are beyond the puzzle gate. Coins are optional and have a separate counter from the Carved Stone reward.

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

## Coins

Select **01 MissionOne - Hili Grand Tomb > 05 Coins** in the Hierarchy. Each numbered coin is a linked instance of **Assets/Prefabs/Coin.prefab**, using the existing two-frame dallah artwork, trigger, bobbing and pickup sound. Move a coin with the Move tool, or duplicate one with Ctrl+D. The displayed total is calculated automatically when Play starts.

The existing root **TokenManager** tracks collected coins across scene changes during Play. **00 Systems > Coin collection - GameManager and counter** supplies the GameManager required by the existing pickup script and the small HiliaCoinCounter display script. **03 Interface > Safe area > Coins counter** displays coins collected during this visit to Hili. Reloading Hili restores its coins and resets that visit's display; it does not erase TokenManager's shared total. Stopping Play ends this temporary progress.

The original Coin prefab, CoinBehavior, SpriteLoop and TokenManager scripts are unchanged. Scene instances override coin sorting order to keep the coins visible in front of the environment. Keep TokenManager at the scene root because it uses DontDestroyOnLoad.

Coin checks in Unity covered real walking pickups, keyboard and touch jumps, paused dialogue, one count per pickup, all 18 placements, the original puzzle/gate, separate Carved Stone rewards, and reloading with a surviving TokenManager. Desktop and phone previews were checked.

## Scope and verification

MaryamScene and SaraScene were left unchanged. The new scene and supporting runtime files are under `Assets/Scenes/HiliaScene.unity` and `Assets/Hilia`.

This scene contains **one Hili mission**. The Carved Stone is tracked for the current play session. Saving progress, travelling to the next scene, and adding this scene to a build profile are not connected yet.

Unity 6000.3.5f1 play-mode checks covered the full walking route from tomb to chamber, camera follow, dialogue paging and portraits, keyboard/touch movement, nearby interaction, blocking puzzle input during dialogue, the winning order, gate opening, one reward only, and replaying completed dialogue. Desktop and phone previews were rendered in Unity. Validation ran in a separate copy of the project. Unity's existing Search database exception appeared during editor startup; the Hili checks completed successfully.
