namespace SpaceSimFramework
{
public abstract class Order
{
    public string Name;

    public abstract void UpdateState(ShipAI controller);
    public abstract void Destroy();
}
}