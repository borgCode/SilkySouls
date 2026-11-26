// 

namespace SilkySouls.Interfaces;

public interface IUtilityService
{
    void ShowMenu(int offset, int val);
    void ToggleNoClip(bool isEnabled);
    void WriteNoClipSpeed(float speedScale);
    void ToggleDeathCamera(bool isEnabled);
    void ToggleWarpHomePose(bool isEnabled);
    bool HasTemporalAntiAliasing();
    void SetGuaranteedBkhDrop(bool isEnabled);
    void ToggleFilter(bool isEnabled);
}