using System.Collections.Generic;

namespace TrackCell.Api.Models
{
    public class OperatorDto
    {
        public string BadgeNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class PartDefinitionDto
    {
        public string PartNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class OperationDefinitionDto
    {
        public string PartNumber { get; set; } = string.Empty;
        public string OpNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class ResultDto<T>
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public T Data { get; set; }
    }
}
