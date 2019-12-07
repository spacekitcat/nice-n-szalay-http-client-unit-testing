using System;
using System.Net.Http;

namespace HttpTestProjct2
{
    public class Program
    {
        private static IMagicResultHttpClient client;

        public Program(IMagicResultHttpClient _client)
        {
            client = _client;
        }

        public string Execute()
        {
            Console.WriteLine("Fetching the Cinco Magic Number (TM) from remote server...");
            try
            {
                var asyncTask = client.FetchMagicNumber();
                asyncTask.Wait();
                Console.WriteLine("Acquired Cinco Magic Number (TM)");                
                return asyncTask.Result.Code;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }

        static void Main(string[] args)
        {
            var program = new Program(new MagicResultHttpClient(new HttpClient(), "http://localhost:3000/answer"));
            Console.WriteLine("Cinco Magic Number " + 
                program.Execute()
           );
        }
    }
}
