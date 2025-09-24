public class Health
{
    public ReactiveProperty<float> Current = new();
    public ReactiveProperty<float> Max = new();
    public Health() { }
    public Health(float max)
    {
        Max.Value = max;
        Current.Value = max;
    }
    public void Increment(float increment = 1)
    {
        var incremented = Current.Value + increment;
        if (incremented > Max.Value) return;
        Current.Value = incremented;
    }
    public void Decrement(float decrement = 1)
    {
        var decremented = Current.Value - decrement;
        if (decremented < 0) return;
        Current.Value = decremented;
    }
}
