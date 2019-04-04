using Ninject;
using PasswordFileChecker.Models;

namespace PasswordFileChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            NinjectConfig.CreateKernel();
            var kernel = NinjectConfig.Kernel;
            var fileTransfer = kernel.Get<IFileTransferTimer>();
            fileTransfer.StartFileChecker();
        }
    }
}
