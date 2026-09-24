# JebelHafeetScene — Mission Three at night

Open `Assets/Scenes/JebelHafeetScene.unity` and press Play. This is a new, separate scene using the mountain, rock platforms, shrubs and camel from MaryamScene's MissionThree, with your attached blue night background.

## Play

- Read Shamma and Abu Rashid's opening conversation, followed by the climbing instructions.
- Move with A/D or the arrow keys. Press Space to jump; press it again in the air for a double jump.
- On a phone, use the left, right and Jump buttons. Tap Jump again for the second jump.
- Keep moving on crumbling rocks. Ride moving platforms to reach the higher ledges.
- Stable ledges save a checkpoint for the current play session. If Shamma falls, she returns to the last checkpoint.
- On the **first fall only**, Abu Rashid says: “Yalla, get up. Every climber falls once.”
- Reach the summit ledge to finish the mission.

Tap Shamma to replay the opening dialogue. **How to play** repeats the instructions; **Hint** explains the double jump. Platforms pause while any dialogue is open.

## Edit dialogue and portraits

Stop Play Mode, then select **02 Shamma → MainPlayer → Hilia Dialogue**. This is the same dialogue script and UI used in HiliaScene.

Each page contains **Speaker**, **Text**, and **Portrait**. Edit these under Introduction, How To Play, Hint or Solved. The three opening story lines match your table. The Solved page is a short summit-completion message.

To edit the first-fall message, select **01 MissionThree - Jebel Hafeet → 04 Mission controller → Jebel Mission**. Change **First Fall Text** or **Abu Rashid Portrait** there.

## Organization

- **00 Systems:** camera, supplied night background and EventSystem.
- **01 MissionThree - Jebel Hafeet:** Terrain, Climb, Mountain Scenery and Mission Controller.
- **02 Shamma:** character, animation, movement and dialogue.
- **03 Interface:** objective, checkpoint counter, dialogue and touch controls.

Inside Climb, fixed ledges, crumbling rocks, moving platforms and their endpoints are grouped separately. Moving-platform endpoints stay fixed in the level. The supplied sprites are reused; no replacement character or mountain art was generated.

The new scene uses one-way platforms so Shamma can jump up through them. Fragile rocks start their timer on landing, then return after two seconds. Touch jumps use the full jump height. The moving-platform logic is reused with a small helper to carry the character. Original mission scripts and artwork are left intact; the shared dialogue script gains one public method for showing the first-fall conversation.

## Scope

MaryamScene, HiliaScene and SaraScene are not edited. The scene is independent; checkpoint progress resets when Play Mode restarts. The scene has not been connected to the main menu or added to a build profile.

Previews were rendered in Unity. Play-mode checks in a separate validation copy covered keyboard jumping onto a fragile rock, its collapse and return, automatic first-fall dialogue, one-time delivery, checkpoint recovery, moving-platform travel and carrying, dialogue replay, summit completion and touch jumping. Later checkpoints and the summit were tested by placing the character there; this was not an automated end-to-end climb.

Unity's existing Search database exception appeared during editor startup; the scene's gameplay checks completed successfully.
