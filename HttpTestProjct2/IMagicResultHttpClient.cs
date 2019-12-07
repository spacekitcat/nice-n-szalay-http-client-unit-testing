using System.Threading.Tasks;

namespace HttpTestProjct2
{
    public interface IMagicResultHttpClient
    {
        public Task<MagicNumber> FetchMagicNumber();
    }
}
