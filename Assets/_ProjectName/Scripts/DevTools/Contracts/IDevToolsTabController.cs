public interface IDevToolsTabController
{
    DevToolsTab Tab { get; }
    bool IsOpen { get; }

    void Initialize(params object[] parameters);
    void Conclude();
    void Open();
    void Close();
    void SelectFirstItem();
}
