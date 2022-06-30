using System;
using System.Linq;
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
using shortid;
using System.Text.RegularExpressions;

namespace Application.Services
{
    public class SignService : ISignService
    {
        private readonly TenantAccessService<Tenant> _tenantAccessService;
        private readonly ISignRepository _repository;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly ILogger<SignService> _logger;
        private readonly bool _multitenancy = false;


        public SignService(TenantAccessService<Tenant> tenantAccessService, IRepository<Sign> repository,
            IConfiguration config, IMapper mapper, ILogger<SignService> logger)
        {
            _tenantAccessService = tenantAccessService;
            _repository = (ISignRepository) repository;
            _config = config;
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
            var response = _mapper.Map<IEnumerable<Sign>, IEnumerable<SignDto>>(result);

            return response;
        }

        public async Task<SignDto> GetSignById(string id)
        {
            var result = await _repository.FindById(id);
            var response = _mapper.Map<Sign, SignDto>(result);

            return response;
        }

        enum CreateOrUpdate
        {
            create = 1,
            update
        };

        public async Task<SignDto> CreateSign(SignContract contract)
        {
            if (string.IsNullOrEmpty(contract.Name))
                throw new ArgumentNullException("name");
            if (contract.RaceDay == 0)
                throw new ArgumentNullException("race_day");
            if (string.IsNullOrEmpty(contract.SignTypeId))
                throw new ArgumentNullException("signtype_id");

            var entity = await CheckIfDiscard(contract, CreateOrUpdate.create);

            // Create an unique QR Code
            if (string.IsNullOrEmpty(entity.QrCode))
            {
                var length = 12;
                try { length = int.Parse(_config["QrCodeLength"]); } catch {  }
                var options = new shortid.Configuration.GenerationOptions(true, false, length);
                entity.QrCode = ShortId.Generate(options).ToUpper();
            }
            entity.State = SignState.Inactive;

            // Create the sign plus 'numberOfRaceDays' shadow signs
            var signtypeId = entity.SignTypeId;
            SignDto result = null;
            int numberOfRaceDays = 4;
            try { numberOfRaceDays = int.Parse(_config["NumberOfRaceDays"]); } catch { }
            for (var raceDay = 1; raceDay <= numberOfRaceDays; raceDay++)
            {
                entity.Id = null;
                entity.RaceDay = raceDay;
                entity.SignType = null;
                entity.SignTypeId = signtypeId;
                entity = await _repository.Add(entity);
                if (entity.SignTypeId == null)
                {
                    // For some reason the SignTypeId property is occationally not set...
                    entity.SignTypeId = signtypeId;
                    await _repository.Update(entity.Id.ToString(), entity);
                }
                if (raceDay == 1)
                    result = _mapper.Map<Sign, SignDto>(entity);
                if (entity.SignType != null && !entity.SignType.Reuseable)
                    break;
            }

            return result;
        }

        public async Task<bool> UpdateSign(string id, SignContract contract)
        {
            contract.Id = id;
            var entity = await CheckIfDiscard(contract, CreateOrUpdate.update);
            if (entity.State == SignState.Discarded)
                return true;

            var result = await _repository.Update(id, entity);

            return result;
        }

        public async Task<bool> DeleteSign(string id)
        {
            bool result = true;
            QueryParameters queryParameters = new QueryParameters();
            var tenantValidation = new TenantValidation(_tenantAccessService, _multitenancy);
            await tenantValidation.Validate(queryParameters);

            // Delete the sign plus shadow signs with the same QR Code
            if (Guid.TryParse(id, out Guid guid))
            {
                var sign = await _repository.FindById(id);
                if (sign == null)
                    return false;
                queryParameters.qr_code = sign.QrCode;
            }

            var spec = new GetSignsSpecification(queryParameters);
            var signs = await _repository.Find(spec);

            foreach (var item in signs)
            {
                result = await _repository.Remove(item.Id.ToString());
            }

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


        //
        // Misc routines
        //

        private Tuple<string, string> ComposeName(string str, int sequenceNumber)
        {
            if (string.IsNullOrEmpty(str))
                return new Tuple<string, string>(null, null);

            var re = new Regex(@"(.*?)([\$\+\-].*)", RegexOptions.IgnoreCase);
            var m = re.Match(str);
            string prefix = str;
            if (m.Success)
            {
                prefix = m.Groups[1].Value;
            }
            if (!prefix.EndsWith("-"))
                prefix += "-";
            //var s = string.Format("{0}-{1:000}", name, sequenceNumber);
            var name = prefix + sequenceNumber.ToString("D3");
            return new Tuple<string, string>(prefix, name);
        }

        private async Task<Tuple<string, Sign>> UpdateProperties(SignContract contract)
        {
            QueryParameters parameters = new QueryParameters();
            var tenantValidation = new TenantValidation(_tenantAccessService, _multitenancy);
            await tenantValidation.Validate(parameters);

            var entity = _mapper.Map<SignContract, Sign>(contract);

            if (entity.TenantId == null && Guid.TryParse(parameters.tenant_id, out Guid tid))
                entity.TenantId = tid;
            if (entity.OrganizationId == null && Guid.TryParse(parameters.organization_id, out Guid oid))
                entity.OrganizationId = oid;

            if (entity.Location != null && entity.Location.Timestamp == null)
                entity.Location.Timestamp = DateTime.UtcNow;

            string prefix = "";
            if (!string.IsNullOrEmpty(entity.Name))
            {
                Tuple<string, string> res = ComposeName(entity.Name, (int)entity.SequenceNumber);
                entity.Name = res.Item2;
                prefix = res.Item1;
            }

            return new Tuple<string, Sign>(prefix, entity);
        }

        private async Task<Sign> CheckIfDiscard(SignContract contract, CreateOrUpdate mode)
        {
            if (mode == CreateOrUpdate.update)
            {
                var sign = await _repository.FindById(contract.Id);
                if (sign == null)
                    throw new ArgumentException($"Sign with ID {contract.Id} not found");
                contract.Name = sign.Name;
                contract.SequenceNumber = sign.SequenceNumber;
                contract.QrCode = sign.QrCode;
            }

            Tuple<string, Sign> tuple = await UpdateProperties(contract);
            var prefix = tuple.Item1;
            var entity = tuple.Item2;

            if ((mode == CreateOrUpdate.update && entity.State == SignState.Discarded) ||
                (mode == CreateOrUpdate.create))
            {
                var spec = new GetSignsSpecification(new QueryParameters()
                {
                    name = prefix,
                    organization_id = entity.OrganizationId.ToString()
                });
                IEnumerable<Sign> signs = await _repository.Find(spec);

                int raceDay = 1;
                foreach (var item in signs)
                {
                    if (item.RaceDay == 1)
                    {
                        entity.SequenceNumber = ++item.SequenceNumber;
                        item.SequenceNumber = entity.SequenceNumber;
                        item.State = SignState.Discarded;
                        await _repository.Update(item.Id.ToString(), item);
                        Tuple<string, string> t = ComposeName(entity.Name, (int)entity.SequenceNumber);
                        entity.Name = t.Item2;
                        raceDay++;
                    }
                    else
                        await _repository.Remove(item.Id.ToString());
                }
            }

            return entity;
        }
    }
}