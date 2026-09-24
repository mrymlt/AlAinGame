using UnityEngine;

// Original PatrolPlatform handles travel; this carries a standing character with it.
[DefaultExecutionOrder(200)]
public class JebelPlatformCarry : MonoBehaviour
{
    public JebelMission mission;
    Vector2 previous;
    Collider2D surface;
    void Start(){previous=transform.position;surface=GetComponent<Collider2D>();}
    void LateUpdate()
    {
        Vector2 current=transform.position;
        if(mission.Support==surface && !mission.dialogue.IsOpen)
        {
            var body=mission.player.GetComponent<Rigidbody2D>();
            if(body.linearVelocity.y<=.1f)body.position+=current-previous;
        }
        previous=current;
    }
}
