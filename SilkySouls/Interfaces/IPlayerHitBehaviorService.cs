namespace SilkySouls.Interfaces;

public interface IPlayerHitBehaviorService
{
    bool IsNegateEnemyHitsEnabled { get; }
    bool IsHealEnemiesOnPlayerHitEnabled { get; }
    void SetNegateEnemyHitsEnabled(bool isEnabled);
    void SetHealEnemiesOnPlayerHitEnabled(bool isEnabled);
}
