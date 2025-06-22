namespace Domain.ValueObjects;

public abstract record Option<T>
{
    public static Option<T> Some(T value) => new Some<T>(value);

    public static Option<T> None => new None<T>();

    public abstract bool IsSome { get; }
    public abstract bool IsNone { get; }
    public abstract T Value { get; }

    public T GetValueOrDefault(T defaultValue = default!)
    {
        // Treat null or default(Option<T>) as None
        if (this is null || this is None<T>)
            return defaultValue;
        return IsSome ? Value : defaultValue;
    }
}

public record Some<T> : Option<T>
{
    public Some(T value) => Value = value;

    public override bool IsSome => true;
    public override bool IsNone => false;
    public override T Value { get; }
}

public record None<T> : Option<T>
{
    public override bool IsSome => false;
    public override bool IsNone => true;
    public override T Value => throw new InvalidOperationException("Cannot get value from None");
}
