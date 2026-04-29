using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrackCell.Api.Models;

namespace TrackCell.Api.Services
{
    public class MasterDataService
    {
        private readonly OperationDefinitionRepository _operationRepository;

        public MasterDataService(OperationDefinitionRepository operationRepository)
        {
            _operationRepository = operationRepository;
        }

        public async Task<IEnumerable<OperationDefinitionDto>> GetOperationsByPartAsync(string partNumber)
        {
            var predicate = PredicateBuilder.True<OperationDefinition>();

            if (!string.IsNullOrEmpty(partNumber))
            {
                predicate = predicate.And(x => x.PartNumber == partNumber);
            }

            var operations = await _operationRepository.FindAsync(predicate);

            return operations.Select(o => new OperationDefinitionDto
            {
                PartNumber = o.PartNumber,
                OpNumber = o.OpNumber,
                Description = o.Description
            });
        }
    }
}
