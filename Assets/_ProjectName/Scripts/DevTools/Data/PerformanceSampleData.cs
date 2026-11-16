public struct PerformanceSampleData
{
    public float Time { get; set; }
    public float FrameTimeMs { get; set; }
    public float MainThreadMs { get; set; }
    public float RenderThreadMs { get; set; }
    public long GcAllocatedBytes { get; set; }
    public long TotalUsedMemoryBytes { get; set; }
    public long GcReservedMemoryBytes { get; set; }
    public long DrawCalls { get; set; }
    public long SetPassCalls { get; set; }
    public long Triangles { get; set; }
    public long Vertices { get; set; }
}
