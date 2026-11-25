using System;
using System.Runtime.InteropServices;
using System.Threading;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;

namespace Necrosmith2MemoryFix
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class MemoryFixPlugin : BasePlugin
    {
        [DllImport("GameAssembly.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void il2cpp_gc_collect(int maxGenerationCollected);

        [DllImport("GameAssembly.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr il2cpp_domain_get();

        // Returns a thread handle; must be detached when done
        [DllImport("GameAssembly.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr il2cpp_thread_attach(IntPtr domain);

        [DllImport("GameAssembly.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void il2cpp_thread_detach(IntPtr thread);

        private Timer? _gcTimer;
        private const int GcIntervalMs = 60_000;

        public override void Load()
        {
            Log.LogInfo($"[MemoryFix] Loaded - GC will run every {GcIntervalMs / 1000}s");
            _gcTimer = new Timer(RunGC, null, GcIntervalMs, GcIntervalMs);
        }

        private void RunGC(object? _)
        {
            // Must attach to IL2CPP domain before calling il2cpp_gc_collect,
            // otherwise Boehm GC fatals with "Collecting from unknown thread"
            var domain = il2cpp_domain_get();
            var thread = il2cpp_thread_attach(domain);
            try
            {
                var before = GC.GetTotalMemory(false) / 1048576L;
                il2cpp_gc_collect(-1);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                var after = GC.GetTotalMemory(true) / 1048576L;
                Log.LogInfo($"[MemoryFix] GC complete: {before} MB → {after} MB");
            }
            catch (Exception ex)
            {
                Log.LogError($"[MemoryFix] GC error: {ex}");
            }
            finally
            {
                il2cpp_thread_detach(thread);
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
