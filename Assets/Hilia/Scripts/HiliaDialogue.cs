using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class HiliaLine
{
    public string speaker;
    [TextArea(2,4)] public string text;
    public Sprite portrait;
    public HiliaLine(string who,string words,Sprite image){speaker=who;text=words;portrait=image;}
}

// Edit conversations on MainPlayer. Shared by HiliaScene and JebelHafeetScene.
public class HiliaDialogue : MonoBehaviour, IPointerClickHandler
{
    public bool openIntroductionOnStart=true;
    public HiliaLine[] introduction, howToPlay, hint, solved;
    [HideInInspector] public GameObject panel;
    [HideInInspector] public TMP_Text speakerLabel, wordsLabel, pageLabel, nextLabel, initialsLabel;
    [HideInInspector] public Image portrait;
    [HideInInspector] public HiliaMission mission;
    public bool IsOpen {get;private set;}
    public int CurrentPage {get;private set;}
    HiliaLine[] pages;
    PlayerMovement player;
    Rigidbody2D body;
    Animator animator;
    bool previousPause,previousSimulation;
    float previousAnimationSpeed;
    Vector2 previousVelocity;
    void Start(){if(openIntroductionOnStart)ShowIntroduction();else Close();}
    public void OnPointerClick(PointerEventData data){if(data.button==PointerEventData.InputButton.Left)ShowIntroduction();}
    public void ShowIntroduction()=>Open(mission!=null && mission.IsSolved?solved:introduction);
    public void ShowHint()=>Open(mission!=null && mission.IsSolved?solved:hint);
    public void ShowHowToPlay()=>Open(howToPlay);
    public void ShowSolved()=>Open(solved);
    public void ShowLines(HiliaLine[] lines)=>Open(lines);
    void Open(HiliaLine[] lines)
    {
        if(!isActiveAndEnabled || panel==null || lines==null || lines.Length==0)return;
        if(!IsOpen)
        {
            player=GetComponent<PlayerMovement>();body=GetComponent<Rigidbody2D>();animator=GetComponent<Animator>();
            previousPause=player.isPaused;player.ClearInteractionInput();player.isPaused=true;
            previousSimulation=body.simulated;previousVelocity=body.linearVelocity;body.simulated=false;
            if(animator!=null){previousAnimationSpeed=animator.speed;animator.speed=0;}
        }
        IsOpen=true;pages=lines;CurrentPage=0;panel.SetActive(true);Display();
    }
    void Display()
    {
        var line=pages[CurrentPage];speakerLabel.text=line.speaker;wordsLabel.text=line.text;
        portrait.sprite=line.portrait;portrait.gameObject.SetActive(line.portrait!=null);
        initialsLabel.gameObject.SetActive(line.portrait==null);initialsLabel.text="H";
        pageLabel.text=$"{CurrentPage+1} / {pages.Length}";
        nextLabel.text=CurrentPage==pages.Length-1?"Close":"Next  >";
        Canvas.ForceUpdateCanvases();
        var scroll=wordsLabel.GetComponentInParent<ScrollRect>();if(scroll!=null)scroll.verticalNormalizedPosition=1;
    }
    public void Next()
    {
        if(!IsOpen)return;
        if(++CurrentPage<pages.Length){Display();return;}
        // The introduction leads into the same instructions used by How to play.
        if(pages==introduction && howToPlay!=null && howToPlay.Length>0)Open(howToPlay);
        else Close();
    }
    public void Close()
    {
        if(IsOpen)
        {
            if(player!=null){player.ClearInteractionInput();player.isPaused=previousPause;}
            if(body!=null){body.simulated=previousSimulation;if(previousSimulation)body.linearVelocity=new Vector2(0,previousVelocity.y);}
            if(animator!=null)animator.speed=previousAnimationSpeed;
        }
        IsOpen=false;if(panel!=null)panel.SetActive(false);
    }
    void OnDisable()=>Close();
}

