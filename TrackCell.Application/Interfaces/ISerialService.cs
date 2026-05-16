using System.Collections.Generic;
using System.Threading.Tasks;
using TrackCell.Domain.Dtos;
using TrackCell.Domain.Entities;

namespace TrackCell.Application.Interfaces
{
    public interface ISerialService
    {
        Task<List<PartSerial>> GetByPartAsync(int partId);
        Task<(SerialLookupDto? Result, string? Error)> LookupAsync(string serialNumber);
        Task<(PartSerial? Serial, string? Error)> AddAsync(CreatePartSerialDto dto);
    }
}
