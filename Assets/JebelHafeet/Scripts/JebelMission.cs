using System;
using TMPro;
using UnityEngine;

public class JebelMission : MonoBehaviour
{
    public PlayerMovement player;
    public HiliaDialogue dialogue;
    public Collider2D[] safeLedges;
    public Collider2D summit;
    [TextArea(2,4)] public string firstFallText="Yalla, get up. Every climber falls once.";
    public Sprite abuRashidPortrait;
    public TMP_Text objectiveLabel, checkpointLabel, instructionLabel;
    public bool IsComplete {get;private set;}
    public int FallCount {get;private set;}
    public int Checkpoint {get;private set;}
    public Collider2D Support {get;private set;}
    public Vector2 RespawnPoint {get;private set;}
    Rigidbody2D body;Collider2D playerCollider;
    float lastPlatformTop, previousTimeScale=1;
    bool paused;
    void Start()
    {
        body=player.GetComponent<Rigidbody2D>();playerCollider=player.GetComponent<Collider2D>();
        RespawnPoint=body.position;lastPlatformTop=safeLedges[0].bounds.max.y;Refresh();
    }
    Vector2 StandingPoint(Collider2D ledge)=>new Vector2(ledge.bounds.center.x,ledge.bounds.max.y+playerCollider.bounds.extents.y-(playerCollider.bounds.center.y-body.position.y)+.05f);
    void Update()
    {
        if(dialogue.IsOpen){PauseWorld(true);return;}PauseWorld(false);
        if((body.position.y<lastPlatformTop-1.6f && body.linearVelocity.y<-.2f) || body.position.y<-5){Recover();return;}
        Support=null;
        if(body.linearVelocity.y<=.1f)
        {
            var bounds=playerCollider.bounds;
            var hit=Physics2D.OverlapBox(new Vector2(bounds.center.x,bounds.min.y-.035f),new Vector2(bounds.size.x*.7f,.12f),0,player.groundLayer);
            if(hit!=null && !hit.isTrigger)
            {
                Support=hit;lastPlatformTop=hit.bounds.max.y;
                int index=Array.IndexOf(safeLedges,hit);
                if(index>Checkpoint){Checkpoint=index;RespawnPoint=StandingPoint(hit);Refresh();}
                if(hit==summit && !IsComplete){IsComplete=true;Refresh();dialogue.ShowSolved();}
            }
        }
    }
    void LateUpdate(){PauseWorld(dialogue.IsOpen);}
    void PauseWorld(bool value)
    {
        if(value && !paused){previousTimeScale=Time.timeScale;Time.timeScale=0;paused=true;}
        else if(!value && paused){Time.timeScale=previousTimeScale;paused=false;}
    }
    public void Recover()
    {
        if(dialogue.IsOpen)return;
        FallCount++;player.ClearInteractionInput();body.linearVelocity=Vector2.zero;body.position=RespawnPoint;player.transform.position=RespawnPoint;
        lastPlatformTop=safeLedges[Checkpoint].bounds.max.y;Support=null;Physics2D.SyncTransforms();
        if(FallCount==1)dialogue.ShowLines(new[]{new HiliaLine("Abu Rashid (walkie)",firstFallText,abuRashidPortrait)});
    }
    void Refresh()
    {
        objectiveLabel.text=IsComplete?"JEBEL HAFEET AT NIGHT\nSummit reached!":"JEBEL HAFEET AT NIGHT\nKeep climbing to the summit.";
        checkpointLabel.text=$"CHECKPOINT  {Checkpoint+1} / {safeLedges.Length}";
        instructionLabel.text=IsComplete?"You reached the top of Jebel Hafeet.":"Keep moving on fragile rocks. Tap Shamma to talk.";
    }
    void OnDisable(){PauseWorld(false);}
}

