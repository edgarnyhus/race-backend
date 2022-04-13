using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using ReenWise.Domain.Core.Bus;
using ReenWise.Domain.Core.Commands;

namespace ReenWise.Infrastructure.Bus
{
    public sealed class InMemoryBus : IMediatorHandler
    {
        private readonly IMediator _mediator;

        public InMemoryBus(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task SendCommand<T>(IRequest<T> command)
        {
            var result = _mediator.Send<T>(command);
            return Task.FromResult(result);
        }
    }
}
