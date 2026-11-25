using System;
using System.Runtime.InteropServices;
using System.Threading;
using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;

namespace Necrosmith2MemoryFix
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class MemoryFixPlugin : BasePlugin
    {
        // Call the IL2CPP runtime GC directly — no game-specific stubs needed
        [DllImport("GameAssembly.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void il2cpp_gc_collect(int maxGenerationCollected);

        private Timer? _gcTimer;
        private const int GcIntervalMs = 60_000; // 60 seconds

        public override void Load()
        {
            Log.LogInfo($"[MemoryFix] Loaded — GC will run every {GcIntervalMs / 1000}s");
            _gcTimer = new Timer(RunGC, null, GcIntervalMs, GcIntervalMs);
        }

        private void RunGC(object? _)
        {
            try
            {
                var before = GC.GetTotalMemory(false) / 1048576L;
                il2cpp_gc_collect(-1); // full IL2CPP GC collection
                GC.Collect();
                GC.WaitForPendingFinalizers();
                var after = GC.GetTotalMemory(true) / 1048576L;
                Log.LogInfo($"[MemoryFix] GC complete: {before} MB → {after} MB");
            }
            catch (Exception ex)
            {
                Log.LogError($"[MemoryFix] GC error: {ex}");
            }
        }

        public override bool Unload()
        {
            _gcTimer?.Dispose();
            return true;
        }
    }

    internal static class PluginInfo
    {
        internal const string GUID = "com.floopdapig.necrosmith2memoryfix";
        internal const string Name = "Necrosmith 2 Memory Fix";
        internal const string Version = "1.0.0";
    }
}
