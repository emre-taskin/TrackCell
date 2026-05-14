namespace TrackCell.CollectorService.Data;

public interface IMessageRepository
{
    Task InsertMessageAsync(string payload);
    Task<(long Id, string Payload)?> GetNextMessageAsync();
    Task DeleteMessageAsync(long id);
}
