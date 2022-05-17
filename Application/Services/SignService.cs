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

        public async Task<SignDto> CreateSign(SignContract contract)
        {
            if (string.IsNullOrEmpty(contract.Name))
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(contract.SignTypeId))
                throw new ArgumentNullException("signtype_id");

            contract.SequenceNumber = 1;
            Tuple<string, Sign> tuple = await UpdateProperties(contract);
            var prefix = tuple.Item1;
            var entity = tuple.Item2; 

            // If 'name' exists, discard the sign and increase the sequence number
            var spec = new GetSignsSpecification(new QueryParameters()
            {
                name = prefix,
                organization_id = entity.OrganizationId.ToString()
            });
            IEnumerable<Sign> signs = await _repository.Find(spec);

            int raceDay = 1;
            foreach (var sign in signs)
            {
                sign.State = SignState.Discarded;
                if (sign.RaceDay == 1 || sign.RaceId != null)
                    await _repository.Update(sign.Id.ToString(), sign);
                else
                    await _repository.Remove(sign.Id.ToString());

                entity.SequenceNumber = ++sign.SequenceNumber;
                entity.RaceDay = raceDay++;
                Tuple<string, string> t = ComposeName(entity.Name, (int)entity.SequenceNumber);
                entity.Name = t.Item2;
            }

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
            int numberOfRaceDays = 4;
            try { numberOfRaceDays = int.Parse(_config["NumberOfRaceDays"]); } catch { }
            for (raceDay = 1; raceDay <= numberOfRaceDays; raceDay++)
            {
                var sign = new Sign();
                sign = entity;
                sign.Id = null;
                sign.RaceDay = raceDay;
                sign = await _repository.Add(entity);
                if (raceDay == 1)
                    entity = sign;
            }
            var result = _mapper.Map<Sign, SignDto>(entity);

            return result;
        }

        public async Task<bool> UpdateSign(string id, SignContract contract)
        {
            Tuple<string, Sign> tuple = await UpdateProperties(contract);

            var result = await _repository.Update(id, tuple.Item2);

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
            string name = str;

            var re = new Regex(@"(.*?)([\$\+\-].*)", RegexOptions.IgnoreCase);
            var m = re.Match(str);
            string prefix = str;
            if (m.Success)
            {
                prefix = m.Groups[1].Value;
            }
            //var s = string.Format("{0}-{1:000}", name, sequenceNumber);
            name = prefix + "-" + sequenceNumber.ToString("D3");
            return new Tuple<string, string>(prefix, name);
        }

        private async Task<Tuple<string, Sign>> UpdateProperties(SignContract contract)
        {
            QueryParameters parameters = new QueryParameters();
            var tenantValidation = new TenantValidation(_tenantAccessService, _multitenancy);
            await tenantValidation.Validate(parameters);

            var entity = _mapper.Map<SignContract, Sign>(contract);

            if (entity.TenantId == null)
            {
                if (Guid.TryParse(parameters.tenant_id, out Guid tid))
                    entity.TenantId = tid;
            }
            if (entity.OrganizationId == null)
            {
                if (Guid.TryParse(parameters.organization_id, out Guid oid))
                    entity.OrganizationId = oid;
            }

            Tuple<string, string> res = ComposeName(entity.Name, (int)entity.SequenceNumber);
            entity.Name = res.Item2;

            return new Tuple<string, Sign>(res.Item1, entity);
        }
    }
}