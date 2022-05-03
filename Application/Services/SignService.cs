using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Application.Interfaces;
using Domain.Dtos;
using Domain.Queries.Helpers;
using Application.Helpers;
using Domain.Contracts;
using Domain.Interfaces;
using Domain.Models;
using Domain.Multitenant;
using Domain.Specifications;
using Microsoft.Extensions.Configuration;
using IronBarCode;
using Net.Codecrete;
using shortid;
using System.Text.RegularExpressions;

namespace Application.Services
{
    public class SignService : ISignService
    {
        private readonly TenantAccessService<Tenant> _tenantAccessService;
        private readonly ISignRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<SignService> _logger;
        private readonly bool _multitenancy = false;


        public SignService(TenantAccessService<Tenant> tenantAccessService, IRepository<Sign> repository,
            IConfiguration config, IMapper mapper, ILogger<SignService> logger)
        {
            _tenantAccessService = tenantAccessService;
            _repository = (ISignRepository) repository;
            _mapper = mapper;
            _logger = logger;
            var value = config["Multitenancy:Enabled"];
            bool.TryParse(value, out _multitenancy);
        }

        public async Task<IEnumerable<SignDto>> GetSigns(QueryParameters queryParameters)
        {
            var tenantValidation = new TenantValidation(_tenantAccessService, _multitenancy);
            await tenantValidation.Validate(queryParameters);

            var result = await _repository.Find(new GetSignsSpecification(queryParameters));
            _logger.LogTrace($"GetEquipment: Making query {result}");

            var response = _mapper.Map<IEnumerable<Sign>, IEnumerable<SignDto>>(result);
            return response;
        }

        public async Task<SignDto> GetSignById(string id)
        {
            Guid.TryParse(id, out Guid guid);
            var result = await _repository.FindById(guid);

            var response = _mapper.Map<Sign, SignDto>(result);
            return response;
        }

        public Task<string> GetSignWithQrCode(string qrCode)
        {
            throw new NotImplementedException();
        }

        public async Task<SignDto> CreateSign(SignContract contract)
        {
            //var isAdmin = await _tenantAccessService.IsAdministrator();
            //if (_multitenancy && !isAdmin)
            //    throw new UnauthorizedAccessException(
            //        "Unauthorized. You are missing the necessary permissions to issue this request.");

            QueryParameters parameters = new QueryParameters();
            var tenantValidation = new TenantValidation(_tenantAccessService, _multitenancy);
            await tenantValidation.Validate(parameters);

            var entity = _mapper.Map<SignContract, Sign>(contract);

            if (entity.SignType == null || entity.SignType.Id == null)
                throw new ArgumentException("Property 'sign_type.id' must be set");
                
            if (entity.TenantId == null)
            {
                if (Guid.TryParse(parameters.tenant_id, out Guid tid))
                    entity.TenantId = tid;
            }
            if (entity.OrganizationId == null)
            {
                if (Guid.TryParse(parameters.organization_id, out Guid oid))
                    entity.TenantId = oid;
            }

            entity.Name = ComposeName(entity.Name, (int)entity.SequenceNumber);

            // If 'name' exists, discard the sign and increase the sequence number
            var existingEntity = await _repository.FindById(entity.Name);
            if (existingEntity != null)
            {
                existingEntity.State = SignState.Discarded;
                await _repository.Update((Guid)existingEntity.Id, existingEntity);

                entity.SequenceNumber++;
                entity.Name = ComposeName(entity.Name, (int)entity.SequenceNumber);
            }

            if (string.IsNullOrEmpty(entity.QrCode))
            {
                // Use ticks to generate unique id
                //var ticks = new DateTime(2016, 1, 1).Ticks;
                //var ans = DateTime.Now.Ticks - ticks;
                //var uniqueId = ans.ToString("x");
                // or
                //Guid.NewGuid().ToString("n")
                // or
                var options = new shortid.Configuration.GenerationOptions(true, false, 12);
                entity.QrCode = ShortId.Generate(options).ToUpper();
            }
            entity.State = SignState.Inactive;
            entity = await _repository.Add(entity);

            var result = _mapper.Map<Sign, SignDto>(entity);
            return result;
        }

        public async Task<bool> UpdateSign(string id, SignContract contract)
        {
            //var isAdmin = await _tenantAccessService.IsAdministrator();
            //if (_multitenancy && !isAdmin)
            //    throw new UnauthorizedAccessException(
            //        "Unauthorized. You are missing the necessary permissions to issue this request.");

            var entity = _mapper.Map<SignContract, Sign>(contract);
            Guid.TryParse(id, out Guid guid);
            var result = await _repository.Update(guid, entity);
            return result;
        }

        public async Task<bool> DeleteSign(string id)
        {
            //var isAdmin = await _tenantAccessService.IsAdministrator();
            //if (_multitenancy && !isAdmin)
            //    throw new UnauthorizedAccessException(
            //        "Unauthorized. You are missing the necessary permissions to issue this request.");

            Guid.TryParse(id, out Guid guid);
            var result = await _repository.Remove(guid);

            return result;
        }

        public async Task<int> GetCount(QueryParameters queryParameters)
        {
            var tenantValidation = new TenantValidation(_tenantAccessService, _multitenancy);
            await tenantValidation.Validate(queryParameters);
            queryParameters.page_size = 0;

            var result = _repository.Count(new GetSignsSpecification(queryParameters));
            return result;
        }


        public List<KeyValuePair<int, string>> GetSignStates()
        {
            var result = _repository.GetSignStates();
            return result;
        }

        private string ComposeName(string str, int sequenceNumber)
        {
            string name = "";

            var re = new Regex("^([a-z ]+)(.+)", RegexOptions.IgnoreCase);
            var m = re.Match(str);
            if (m.Success)
            {
                name = m.Groups[1].Value;
            }
            else
                name = str;
            var s = string.Format("{0}-{1:000}", name, sequenceNumber);
            name += "-" + sequenceNumber.ToString("D3");
            return name;
        }
    }
}