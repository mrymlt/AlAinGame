using UnityEngine;

[RequireComponent(typeof(Camera))]
public class HiliaCamera : MonoBehaviour
{
    public Transform player;
    public float levelLeft=-12,levelRight=12,height=1.1f,lookAhead=1.6f;
    Camera view;
    void Awake(){view=GetComponent<Camera>();}
    void LateUpdate()
    {
        view.orthographicSize=view.aspect<1?6.4f:5.8f;
        float halfWidth=view.orthographicSize*view.aspect;
        float halfLevel=(levelRight-levelLeft)*.5f;
        float x=halfWidth>=halfLevel?(levelLeft+levelRight)*.5f:Mathf.Clamp(player.position.x+lookAhead,levelLeft+halfWidth,levelRight-halfWidth);
        transform.position=Vector3.Lerp(transform.position,new Vector3(x,height,-10),1-Mathf.Exp(-6*Time.deltaTime));
    }
}


