using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class CarvingState
{
    public Sprite[] slotSprites;
}

public class StateCycler : MonoBehaviour
{
    public SpriteRenderer[] slots;
    public CarvingState[] states;
    public int targetStateIndex;
    public UnityEvent onTargetReached;

    private int currentIndex;

    void Start()
    {
        ApplyState(currentIndex);
    }

    public void Advance()
    {
        currentIndex = (currentIndex + 1) % states.Length;
        ApplyState(currentIndex);

        if (currentIndex == targetStateIndex)
        {
            onTargetReached.Invoke();
        }
    }

    private void ApplyState(int index)
    {
        CarvingState state = states[index];
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].sprite = state.slotSprites[i];
        }
    }
}
