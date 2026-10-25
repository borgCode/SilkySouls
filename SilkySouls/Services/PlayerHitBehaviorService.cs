using System;
using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.Memory;
using SilkySouls.Utilities;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Services;

public class PlayerHitBehaviorService : IPlayerHitBehaviorService
{
    private const int MaxChrCount = 1024;

    // The second Bell Gargoyle is the only enemy that spawns below its max health.
    private const int SecondBellGargoyleChrId = 535001;
    private const int SecondBellGargoyleSpawnHp = 480;

    private readonly IMemoryService _memoryService;
    private readonly HookManager _hookManager;

    private bool _isNegateEnemyHitsEnabled;
    private bool _isHealEnemiesOnPlayerHitEnabled;

    public bool IsNegateEnemyHitsEnabled => _isNegateEnemyHitsEnabled;
    public bool IsHealEnemiesOnPlayerHitEnabled => _isHealEnemiesOnPlayerHitEnabled;

    public PlayerHitBehaviorService(
        IMemoryService memoryService,
        HookManager hookManager,
        IGameTickService gameTickService,
        IStateService stateService)
    {
        _memoryService = memoryService;
        _hookManager = hookManager;

        stateService.Subscribe(State.Loaded, OnLoaded);
        gameTickService.Subscribe(OnTick);
    }

    public void SetNegateEnemyHitsEnabled(bool isEnabled)
    {
        if (_isNegateEnemyHitsEnabled == isEnabled)
            return;

        _isNegateEnemyHitsEnabled = isEnabled;
        if (isEnabled)
            Enable();
        WriteNegateFlag();
    }

    public void SetHealEnemiesOnPlayerHitEnabled(bool isEnabled)
    {
        if (_isHealEnemiesOnPlayerHitEnabled == isEnabled)
            return;

        _isHealEnemiesOnPlayerHitEnabled = isEnabled;
        if (isEnabled)
            Enable();
    }

    private void Enable()
    {
        WriteLastAttacker(IntPtr.Zero);
        InstallHitHook();
    }

    private void OnLoaded()
    {
        WriteLastAttacker(IntPtr.Zero);
        if (_isNegateEnemyHitsEnabled || _isHealEnemiesOnPlayerHitEnabled)
            InstallHitHook();
        WriteNegateFlag();
    }

    private void OnTick()
    {
        if (!_isNegateEnemyHitsEnabled && !_isHealEnemiesOnPlayerHitEnabled)
            return;

        if (!IsHitHookInstalled())
            InstallHitHook();

        if (!_isHealEnemiesOnPlayerHitEnabled)
            return;

        if (ReadLastAttacker() == IntPtr.Zero)
            return;
        WriteLastAttacker(IntPtr.Zero);

        HealEnemies();
    }

    private void HealEnemies()
    {
        try
        {
            var worldChrMan = _memoryService.Read<nint>(WorldChrMan.Base);
            if (worldChrMan == IntPtr.Zero)
                return;

            var player = _memoryService.Read<nint>(worldChrMan + WorldChrMan.PlayerIns);
            var chrSet = _memoryService.Read<nint>(worldChrMan + WorldChrMan.ChrSet);
            if (chrSet == IntPtr.Zero)
                return;

            var count = _memoryService.Read<int>(chrSet + WorldChrMan.ChrSetCount);
            var entries = _memoryService.Read<nint>(chrSet + WorldChrMan.ChrSetEntries);
            if (entries == IntPtr.Zero || count <= 0 || count > MaxChrCount)
                return;

            var raw = _memoryService.ReadBytes(entries, count * WorldChrMan.ChrSetEntrySize);
            for (int i = 0; i < count; i++)
            {
                var chr = (nint)BitConverter.ToInt64(raw, i * WorldChrMan.ChrSetEntrySize);
                if (chr != IntPtr.Zero && chr != player)
                    HealEnemy(chr);
            }
        }
        catch
        {
        }
    }

    private void HealEnemy(nint chr)
    {
        try
        {
            var hp = _memoryService.Read<int>(chr + ChrIns.Health);
            var maxHp = _memoryService.Read<int>(chr + ChrIns.MaxHealth);
            if (hp <= 0 || maxHp <= 1)
                return;

            var chrId = _memoryService.Read<int>(chr + ChrIns.ChrId);
            var restoreHp = chrId == SecondBellGargoyleChrId
                ? Math.Min(maxHp, SecondBellGargoyleSpawnHp)
                : maxHp;

            if (hp < restoreHp)
                _memoryService.Write(chr + ChrIns.Health, restoreHp);
        }
        catch
        {
        }
    }

    private nint ReadLastAttacker()
    {
        try
        {
            return _memoryService.Read<nint>(CodeCaveOffsets.Base + CodeCaveOffsets.LastAttackerPtr);
        }
        catch
        {
            return IntPtr.Zero;
        }
    }

    private void WriteLastAttacker(nint value)
    {
        try
        {
            _memoryService.Write(CodeCaveOffsets.Base + CodeCaveOffsets.LastAttackerPtr, value);
        }
        catch
        {
        }
    }

    private void WriteNegateFlag()
    {
        try
        {
            _memoryService.Write<byte>(CodeCaveOffsets.Base + CodeCaveOffsets.NegateEnemyHitsFlag,
                (byte)(_isNegateEnemyHitsEnabled ? 1 : 0));
        }
        catch
        {
        }
    }

    private bool IsHitHookInstalled()
    {
        try
        {
            return _memoryService.Read<byte>(Hooks.Hit) == 0xE9;
        }
        catch
        {
            return false;
        }
    }

    private void InstallHitHook()
    {
        try
        {
            var bytes = AsmLoader.GetAsmBytes(AsmScript.DSRHit);
            var lastAttacker = CodeCaveOffsets.Base + CodeCaveOffsets.LastAttackerPtr;
            var negateFlag = CodeCaveOffsets.Base + CodeCaveOffsets.NegateEnemyHitsFlag;
            var code = CodeCaveOffsets.Base + CodeCaveOffsets.HitCode;
            var originalBytes = GetHitOriginalBytes();

            Array.Copy(originalBytes, 0, bytes, 0xC7, originalBytes.Length);
            AsmHelper.WriteRelativeOffsets(bytes, [
                (code + 0x1, WorldChrMan.Base, 7, 0x1 + 3),
                (code + 0x97, lastAttacker, 7, 0x97 + 3),
                (code + 0x9E, negateFlag, 7, 0x9E + 2),
                (code + 0xCC, Hooks.Hit + 5, 5, 0xCC + 1),
            ]);

            _memoryService.WriteBytes(code, bytes);
            _hookManager.InstallHook(code, Hooks.Hit, originalBytes);
        }
        catch
        {
        }
    }

    private static byte[] GetHitOriginalBytes()
    {
        return SilkySouls.memory.Offsets.Version switch
        {
            GameVersion.Version1_0_1_2 or GameVersion.Version1_0_3_1 => [0x48, 0x89, 0x5C, 0x24, 0x08],
            _ => [0x48, 0x89, 0x6C, 0x24, 0x10],
        };
    }
}
