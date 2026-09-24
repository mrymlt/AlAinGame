using UnityEngine;

[RequireComponent(typeof(Camera))]
public class JebelCamera : MonoBehaviour
{
    public Transform player;
    public SpriteRenderer nightBackground;
    public Vector2 horizontalLimits=new Vector2(-5,68);
    public Vector2 verticalLimits=new Vector2(2,25);
    Camera view;
    void Awake(){view=GetComponent<Camera>();}
    void LateUpdate()
    {
        view.orthographicSize=view.aspect<1?6.8f:6.5f;
        float half=view.orthographicSize*view.aspect;
        float x=half*2>=horizontalLimits.y-horizontalLimits.x?(horizontalLimits.x+horizontalLimits.y)*.5f:Mathf.Clamp(player.position.x+2,horizontalLimits.x+half,horizontalLimits.y-half);
        var target=new Vector3(x,Mathf.Clamp(player.position.y+2,verticalLimits.x,verticalLimits.y),-10);
        transform.position=Vector3.Lerp(transform.position,target,1-Mathf.Exp(-6*Time.unscaledDeltaTime));FitBackground();
    }
    public void FitBackground()
    {
        if(view==null)view=GetComponent<Camera>();
        float scale=Mathf.Max(view.orthographicSize*2/nightBackground.sprite.bounds.size.y,view.orthographicSize*2*view.aspect/nightBackground.sprite.bounds.size.x)*1.02f;
        nightBackground.transform.localScale=Vector3.one*scale;nightBackground.transform.position=new Vector3(transform.position.x,transform.position.y,10);
    }
}
