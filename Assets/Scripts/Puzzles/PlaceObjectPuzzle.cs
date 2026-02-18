using UnityEngine;

public sealed class PlaceObjectPuzzle : PuzzleBase
{
    [Header("Condition")]
    public bool slotFilled;

    public void SetSlotFilled(bool filled)
    {
        slotFilled = filled;
        NotifyProgress();
    }

    protected override bool CanComplete()
    {
        return slotFilled;
    }
}