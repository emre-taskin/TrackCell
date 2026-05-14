namespace TrackCell.UI.Models;

public class ApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string HubPath { get; set; } = "/dashboardHub";
    public string OperationHistoryPath { get; set; } = "/OperationHistory";
    public string MasterDataPath { get; set; } = "/MasterData";
    public string PartPath { get; set; } = "/Part";

    public string HubUrl              => BaseUrl.TrimEnd('/') + HubPath;
    public string OperationHistoryUrl => BaseUrl.TrimEnd('/') + OperationHistoryPath;
    public string MasterUrl           => BaseUrl.TrimEnd('/') + MasterDataPath;
    public string PartUrl             => BaseUrl.TrimEnd('/') + PartPath;
}
