using System;
using UnityEngine;

[Serializable]
public struct PuzzleReward
{
    public bool grantOnComplete;
    public string itemId;
}


public abstract class PuzzleBase : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string puzzleId = "Puzzle_01";

    [Header("Optional Gate")]
    [Tooltip("Leave empty for puzzles that do not require an item.")]
    [SerializeField] private string requiredItemId = "";

    [Header("State")]
    [SerializeField] private bool autoStartOnEnable = false;
    
    [Header("Optional Reward")]
    [SerializeField] private PuzzleReward reward;
    
    public string PuzzleId => puzzleId;
    public string RequiredItemId => requiredItemId;
    public PuzzleReward Reward => reward;
    public bool HasStarted { get; private set; }
    public bool IsCompleted { get; private set; }

    public event Action<PuzzleBase> OnPuzzleStarted;
    public event Action<PuzzleBase> OnPuzzleProgressed;
    public event Action<PuzzleBase> OnPuzzleCompleted;
    public event Action<PuzzleBase, string> OnPuzzleFailed;
    public event Action<PuzzleBase> OnPuzzleReset;
    
    // Inventory listens to this
    public static event Action<string> OnRewardItemGranted;
    protected virtual void OnEnable()
    {
        if (autoStartOnEnable && !HasStarted && !IsCompleted)
        {
            StartPuzzle();
        }
    }

    public void StartPuzzle()
    {
        if (IsCompleted) return;
        if (HasStarted) return;

        HasStarted = true;
        OnPuzzleStarted?.Invoke(this);
        HandleStarted();
    }

    /// <summary>
    /// Try to complete the puzzle.
    /// - If RequiredItemId is empty, completes if puzzle conditions allow.
    /// - If RequiredItemId is set, the provided itemId must match.
    /// </summary>
    public void TryComplete(string providedItemId = "")
    {
        if (IsCompleted) return;
        if (!HasStarted) StartPuzzle();

        if (!string.IsNullOrWhiteSpace(requiredItemId))
        {
            if (!string.Equals(requiredItemId, providedItemId, StringComparison.Ordinal))
            {
                Fail("Missing required item: " + requiredItemId);
                return;
            }
        }

        // For puzzles like "move A to slot A", the puzzle decides when it is actually solvable.
        if (!CanComplete())
        {
            Fail("Puzzle conditions not met");
            return;
        }

        Complete();
    }

    /// <summary>
    /// Call this whenever internal progress changes.
    /// Example: object moved, lever pulled, slot filled.
    /// </summary>
    protected void NotifyProgress()
    {
        if (IsCompleted) return;
        if (!HasStarted) StartPuzzle();

        OnPuzzleProgressed?.Invoke(this);
        HandleProgressed();

        // Optional auto-check: if conditions become true, complete (with no item gate).
        if (CanAutoCompleteOnProgress() && string.IsNullOrWhiteSpace(requiredItemId) && CanComplete())
        {
            Complete();
        }
    }

    public void Complete()
    {
        if (IsCompleted) return;

        IsCompleted = true;
        OnPuzzleCompleted?.Invoke(this);
        HandleCompleted();
        if (reward.grantOnComplete && !string.IsNullOrWhiteSpace(reward.itemId))
        {
            OnRewardItemGranted?.Invoke(reward.itemId);
        }
    }

    public void Fail(string reason)
    {
        if (IsCompleted) return;
        OnPuzzleFailed?.Invoke(this, reason);
        HandleFailed(reason);
    }

    public void ResetPuzzle()
    {
        HasStarted = false;
        IsCompleted = false;

        OnPuzzleReset?.Invoke(this);
        HandleReset();
    }

    /// <summary>
    /// Override for puzzles that have logic-based completion.
    /// Default true means "no extra conditions".
    /// </summary>
    protected virtual bool CanComplete() => true;

    /// <summary>
    /// If true, NotifyProgress can auto-complete when CanComplete becomes true and no item gate exists.
    /// </summary>
    protected virtual bool CanAutoCompleteOnProgress() => true;

    protected virtual void HandleStarted() { }
    protected virtual void HandleProgressed() { }
    protected virtual void HandleCompleted() { }
    protected virtual void HandleFailed(string reason) { }
    protected virtual void HandleReset() { }
}
