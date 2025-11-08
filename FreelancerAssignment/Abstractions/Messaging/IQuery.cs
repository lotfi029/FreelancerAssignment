using MediatR;

namespace FreelancerAssignment.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
