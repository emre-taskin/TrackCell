namespace TrackCell.UI.Models;

public class ApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string HubPath { get; set; } = "/dashboardHub";
    public string WorkItemsPath { get; set; } = "/api/workitems";
    public string MasterDataPath { get; set; } = "/api/masterdata";

    public string HubUrl       => BaseUrl.TrimEnd('/') + HubPath;
    public string WorkItemsUrl => BaseUrl.TrimEnd('/') + WorkItemsPath;
    public string MasterUrl    => BaseUrl.TrimEnd('/') + MasterDataPath;
}
