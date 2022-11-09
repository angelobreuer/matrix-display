abstract class Material
{
    public virtual bool IsSolid { get; }

    public virtual void HandleJump(MaterialPhysicsContext context) { }
}
