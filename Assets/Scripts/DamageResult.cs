public readonly struct DamageResult
{
    public int Amount { get; }
    public bool IsCritical { get; }

    public DamageResult(int amount, bool isCritical)
    {
        Amount = amount;
        IsCritical = isCritical;
    }
}
