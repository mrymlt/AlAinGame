# Simple MainPlayer dialogue

1. Switch back to Unity and stop Play Mode. Let the new scripts compile.
2. The setup removes the separate Oasis Guide image and adds **Simple Player Dialogue** to **MainPlayer** in SampleScene.
3. Select **MainPlayer**. In **Simple Player Dialogue**, edit **Character Name**, **Dialogue Text**, and **Dialogue Image**.
4. Press Play. Left-click or tap MainPlayer. Click/tap **Close** or **X** to return to the game.

**Dialogue Image** is only the portrait inside the dialogue box. It does not change the character's image in the game. Leave it empty to hide the portrait. Import a PNG as **Sprite (2D and UI)**, then drag it into this field.

**Dialogue Text** is a single editable text box directly on MainPlayer. Text appears immediately. Long text can be scrolled. Movement pauses while the box is open and resumes when it closes. No interaction radius, E key, separate guide object, or dialogue-page list is needed.

The runtime script is **Assets/Scripts/SimplePlayerDialogue.cs**. UI references are connected by the setup tool and hidden from the normal Inspector to keep editing simple. The existing canvas supplies the green-and-gold design.

If automatic setup has not run, open SampleScene outside Play Mode and choose **Al Ain > Dialogue > Use simple MainPlayer dialogue** once. Let Unity finish compiling first.

The click/tap system uses MainPlayer's existing Collider2D, a Physics2DRaycaster added to Main Camera, and the existing EventSystem with its Input System UI module.
