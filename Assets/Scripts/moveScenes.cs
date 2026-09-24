using UnityEngine;
using UnityEngine.SceneManagement;


public class moveScenes : MonoBehaviour
{
    public bool changeScene = false;
    private BoxCollider2D boxCollider2D;
    public string sceneName = "";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onTriggerEnter2d(Collider2D other)
    {
        if (changeScene)
        {
            if (other.tag == "player")
            {
                LoadScene(sceneName);
            }
        }
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
