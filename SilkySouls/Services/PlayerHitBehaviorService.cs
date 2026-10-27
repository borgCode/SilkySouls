using System;
using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.Memory;
using SilkySouls.Utilities;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Services;

public class PlayerHitBehaviorService(IMemoryService memoryService, HookManager hookManager)
    : IPlayerHitBehaviorService
{
    private const int MaxChrCount = 1024;

    // The second Bell Gargoyle is the only enemy that spawns below its max health.
    private const int SecondBellGargoyleChrId = 535001;
    private const int SecondBellGargoyleSpawnHp = 480;

    private bool _isNegateEnemyHitsEnabled;
    private bool _isHealEnemiesOnHitEnabled;

    public void ToggleNegateEnemyHits(bool isEnabled)
    {
        _isNegateEnemyHitsEnabled = isEnabled;
        memoryService.Write(CodeCaveOffsets.Base + CodeCaveOffsets.NegateEnemyHitsFlag, isEnabled);
        UpdateHitHook();
    }

    public void ToggleHealEnemiesOnHit(bool isEnabled)
    {
        _isHealEnemiesOnHitEnabled = isEnabled;
        memoryService.Write(CodeCaveOffsets.Base + CodeCaveOffsets.LastAttackerPtr, IntPtr.Zero);
        UpdateHitHook();
    }

    public bool ConsumeEnemyHit()
    {
        var lastAttacker = CodeCaveOffsets.Base + CodeCaveOffsets.LastAttackerPtr;
        if (memoryService.Read<nint>(lastAttacker) == IntPtr.Zero)
            return false;

        memoryService.Write(lastAttacker, IntPtr.Zero);
        return true;
    }

    public void HealEnemies()
    {
        var worldChrMan = memoryService.Read<nint>(WorldChrMan.Base);
        var player = memoryService.Read<nint>(worldChrMan + WorldChrMan.PlayerIns);

        for (int slot = 0; slot < WorldChrMan.ChrSetSlots; slot++)
        {
            var chrSet = worldChrMan + WorldChrMan.ChrSets + slot * WorldChrMan.ChrSetSize;
            var count = memoryService.Read<int>(chrSet + WorldChrMan.ChrSetCount);
            var entries = memoryService.Read<nint>(chrSet + WorldChrMan.ChrSetEntries);
            if (count <= 0 || count > MaxChrCount)
                continue;

            var raw = memoryService.ReadBytes(entries, count * WorldChrMan.ChrSetEntrySize);
            for (int i = 0; i < count; i++)
            {
                var chr = (nint)BitConverter.ToInt64(raw, i * WorldChrMan.ChrSetEntrySize);
                if (chr != IntPtr.Zero && chr != player)
                    HealEnemy(chr);
            }
        }
    }

    private void HealEnemy(nint chr)
    {
        // Only touch real game objects: their vtable pointer lies inside the game module.
        var vtable = (long)memoryService.Read<nint>(chr);
        var moduleStart = (long)memoryService.BaseAddress;
        if (vtable < moduleStart || vtable >= moduleStart + memoryService.ModuleMemorySize)
            return;

        var hp = memoryService.Read<int>(chr + ChrIns.Health);
        var maxHp = memoryService.Read<int>(chr + ChrIns.MaxHealth);
        if (hp <= 0 || maxHp <= 1)
            return;

        var chrId = memoryService.Read<int>(chr + ChrIns.ChrId);
        var restoreHp = chrId == SecondBellGargoyleChrId
            ? Math.Min(maxHp, SecondBellGargoyleSpawnHp)
            : maxHp;

        if (hp < restoreHp)
            memoryService.Write(chr + ChrIns.Health, restoreHp);
    }

    private void UpdateHitHook()
    {
        // The address is unset for a game version the offset tables do not cover.
        if (Hooks.Hit == IntPtr.Zero || Hooks.Hit == memoryService.BaseAddress)
            return;

        var code = CodeCaveOffsets.Base + CodeCaveOffsets.HitCode;
        if (_isNegateEnemyHitsEnabled || _isHealEnemiesOnHitEnabled)
        {
            var bytes = AsmLoader.GetAsmBytes(AsmScript.DSRHit);
            var lastAttacker = CodeCaveOffsets.Base + CodeCaveOffsets.LastAttackerPtr;
            var negateFlag = CodeCaveOffsets.Base + CodeCaveOffsets.NegateEnemyHitsFlag;
            var originalBytes = GetHitOriginalBytes();
            var displacedBytes = RelocateJump(originalBytes, Hooks.Hit, code + 0xC7);

            // The offsets of ChrIns.Health (the player's alive check) and ChrIns.ChrId (Seath's exemption).
            BitConverter.GetBytes(ChrIns.Health).CopyTo(bytes, 0x20);
            BitConverter.GetBytes(ChrIns.ChrId).CopyTo(bytes, 0x48);
            Array.Copy(displacedBytes, 0, bytes, 0xC7, displacedBytes.Length);
            AsmHelper.WriteRelativeOffsets(bytes, [
                (code + 0x1, WorldChrMan.Base, 7, 0x1 + 3),
                (code + 0x97, lastAttacker, 7, 0x97 + 3),
                (code + 0x9E, negateFlag, 7, 0x9E + 2),
                (code + 0xCC, Hooks.Hit + 5, 5, 0xCC + 1),
            ]);

            memoryService.WriteBytes(code, bytes);
            hookManager.InstallHook(code, Hooks.Hit, originalBytes);
        }
        else
        {
            hookManager.UninstallHook(code);
        }
    }

    private static byte[] GetHitOriginalBytes()
    {
        return SilkySouls.memory.Offsets.Version switch
        {
            // Relative jmps, which have to be re-aimed to work from the code cave.
            GameVersion.Version1_0_1_0 => [0xE9, 0xD7, 0xB7, 0xEC, 0x03],
            GameVersion.Version1_0_1_1 => [0xE9, 0x4D, 0xF8, 0x81, 0x02],
            GameVersion.Version1_0_1_2 => [0xE9, 0x76, 0x1E, 0xDF, 0x02],
            GameVersion.Version1_0_3_0 => [0xE9, 0xE9, 0x6D, 0xE6, 0x02],
            _ => [0x48, 0x89, 0x5C, 0x24, 0x08],
        };
    }

    // Instructions copied out of their original place keep working unless they are relative; a jmp rel32
    // has to be re-aimed at its original target from the new location.
    private static byte[] RelocateJump(byte[] original, nint from, nint to)
    {
        if (original[0] != 0xE9)
            return original;

        var target = from + 5 + BitConverter.ToInt32(original, 1);
        var relocated = (byte[])original.Clone();
        BitConverter.GetBytes((int)(target - (to + 5))).CopyTo(relocated, 1);
        return relocated;
    }
}
