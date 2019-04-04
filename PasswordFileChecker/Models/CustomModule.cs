using Ninject.Modules;
using PasswordCore.Interfaces;
using PasswordCore.Repositories;
using PasswordCore.Utilities;

namespace PasswordFileChecker.Models
{
    public class CustomModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IDbConnector>().To<DbConnector>();
            Bind<IAccountRepository>().To<AccountRepository>();
            Bind<IPasswordRepository>().To<PasswordRepository>();
            Bind<IEncryptor>().To<Encryptor>();
            Bind<ITextFile>().To<TextFileRepository>();
            Bind<IFileTransferTimer>().To<FileTransferTimer>();
        }
    }
}
