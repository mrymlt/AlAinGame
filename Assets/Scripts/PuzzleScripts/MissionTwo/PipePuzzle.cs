using UnityEngine;
using UnityEngine.Events;

public class PipePuzzle : MonoBehaviour
{
    public PipeTile[] tiles;
    public UnityEvent onSolved;

    private bool solved;

    void Start()
    {
        foreach (PipeTile tile in tiles)
        {
            tile.Randomize();
        }
    }

    public void Recheck()
    {
        if (solved) return;

        foreach (PipeTile tile in tiles)
        {
            if (!tile.IsCorrect) return;
        }

        solved = true;
        onSolved.Invoke();
    }
}
