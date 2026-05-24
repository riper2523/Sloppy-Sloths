public interface IPartModifier
{
    void ResetModifier(PartLogic coreLogic);
    void ActivateEffects(PartLogic coreLogic);
    void ApplyModifiers(PartLogic coreLogic);
}