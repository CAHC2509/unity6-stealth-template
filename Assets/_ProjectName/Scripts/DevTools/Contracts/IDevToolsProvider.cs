using System.Collections.Generic;
using System.Text;

public interface IDevToolsProvider
{
    string DevToolsSectionName { get; }

    void WriteDevToolsState(DevToolsStateSectionData section);
    void WriteDevToolsOverlaySummary(StringBuilder builder);
    void CollectDevToolsCommands(List<DevToolsCommandData> commands);
    void OnDevToolsPanelVisibilityChanged(bool isVisible);
}
