using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Reuses Maryam's StateCycler; shared scripts and MaryamScene remain unchanged.
public class HiliaMission : MonoBehaviour
{
    public PlayerMovement player;
    public HiliaDialogue dialogue;
    public StateCycler carvings;
    public Transform lever;
    public DoorUnlock exitDoor;
    public SpriteRenderer leverImage;
    public Sprite leverReleased,leverPressed;
    public AudioSource leverAudio;
    public float interactionDistance=2.4f;
    public float hintDelay=20f;
    public GameObject restoredMarker;
    public TMP_Text objectiveLabel,tokenLabel,instructionLabel;
    public Button useButton;
    public bool IsSolved {get;private set;}
    public int CarvedStoneCount {get;private set;}
    public bool IsNear=>player!=null && Vector2.Distance(player.transform.position,lever.position)<=interactionDistance;
    float nextUse,releaseTime,thinkingTime;
    bool hinted;
    void Start(){Refresh();}
    void Update()
    {
        bool canUse=!IsSolved && IsNear && !dialogue.IsOpen;
        useButton.gameObject.SetActive(canUse);
        if(leverImage!=null && Time.unscaledTime>=releaseTime)leverImage.sprite=leverReleased;
        if(canUse)
        {
            thinkingTime+=Time.deltaTime;
            if(!hinted && thinkingTime>=hintDelay){hinted=true;dialogue.ShowHint();}
            if(Keyboard.current!=null && Keyboard.current.eKey.wasPressedThisFrame)UseLever();
        }
        instructionLabel.text=IsSolved?"Carvings restored. The path is open.":IsNear?"Tap the lever or press E to change the carvings.":"Walk right to the tomb. Tap Shamma to talk.";
    }
    public void UseLever()
    {
        if(IsSolved || !IsNear || dialogue.IsOpen || Time.unscaledTime<nextUse)return;
        nextUse=Time.unscaledTime+.3f;releaseTime=Time.unscaledTime+.15f;
        if(leverImage!=null)leverImage.sprite=leverPressed;
        if(leverAudio!=null)leverAudio.Play();
        player.TriggerInteract();carvings.Advance();
    }
    public void Complete()
    {
        if(IsSolved)return;
        IsSolved=true;CarvedStoneCount=1;
        exitDoor.Unlock();restoredMarker.SetActive(true);
        Refresh();dialogue.ShowSolved();
    }
    void Refresh()
    {
        objectiveLabel.text=IsSolved?"HILI GRAND TOMB\nThe carvings are restored.":"HILI GRAND TOMB\nRestore the carved stones.";
        tokenLabel.text=IsSolved?"CARVED STONE  1 / 1":"CARVED STONE  0 / 1";
    }
}

