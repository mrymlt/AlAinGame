using UnityEngine;

public class JebelCrumbleRock : MonoBehaviour
{
    public JebelMission mission;
    public float delay=.4f,respawnTime=2;
    Collider2D surface;SpriteRenderer image;bool triggered;
    void Awake(){surface=GetComponent<Collider2D>();image=GetComponent<SpriteRenderer>();}
    void Update()
    {
        // Only a landing starts the timer; jumping through the underside is safe.
        if(!triggered && mission.Support==surface){triggered=true;Invoke(nameof(Crumble),delay);}
    }
    void Crumble(){surface.enabled=false;image.enabled=false;Invoke(nameof(Respawn),respawnTime);}
    void Respawn(){surface.enabled=true;image.enabled=true;triggered=false;}
}
