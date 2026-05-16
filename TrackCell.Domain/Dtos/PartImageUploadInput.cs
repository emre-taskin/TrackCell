using System.IO;

namespace TrackCell.Domain.Dtos
{
    public class PartImageUploadInput
    {
        public int PartDefinitionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Length { get; set; }
        public Stream Content { get; set; } = Stream.Null;
    }
}
