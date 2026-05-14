namespace TrackCell.CollectorService.Models;

public class MachineConfig
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string CommunicationType { get; set; } = "HttpXml";
}
