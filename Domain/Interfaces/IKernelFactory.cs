using Microsoft.SemanticKernel;

namespace Domain.Interfaces;

public interface IKernelFactory
{
    Kernel CreateKernel();
}
