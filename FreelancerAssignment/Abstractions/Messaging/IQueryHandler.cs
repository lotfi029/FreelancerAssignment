using MediatR;

namespace FreelancerAssignment.Abstractions.Messaging;

public interface IQueryHandler<in TQuery, TResponse> 
    : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;