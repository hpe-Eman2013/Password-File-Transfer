using Ninject;

namespace PasswordFileChecker.Models
{
    public static class NinjectConfig
    {
        private static IKernel _kernel;

        public static void CreateKernel()
        {
            _kernel = new StandardKernel();
            _kernel.Load(new CustomModule());
        }

        public static IKernel Kernel => _kernel;
    }
}
