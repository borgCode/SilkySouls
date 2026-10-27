namespace SilkySouls.Interfaces;

public interface IPlayerHitBehaviorService
{
    void ToggleNegateEnemyHits(bool isEnabled);
    void ToggleHealEnemiesOnHit(bool isEnabled);
    bool ConsumeEnemyHit();
    void HealEnemies();
}
