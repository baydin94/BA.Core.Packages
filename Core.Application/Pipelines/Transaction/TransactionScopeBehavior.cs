using Core.Abstractions.Applications.Transaction;
using Core.Abstractions.Dependencies;
using MediatR;
using System.Transactions;

namespace Core.Application.Pipelines.Transaction
{
    public class TransactionScopeBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionScopeBehavior(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is not ITransactionRequest)
                return await next();

            using TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled);

            TResponse response;

            try
            {
                response = await next();
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                transactionScope.Complete();
                return response;
            }
            catch
            {
                throw;
            }
        }
    }
}
