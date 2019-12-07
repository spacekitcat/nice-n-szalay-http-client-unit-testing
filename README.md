# Nice 'N Szalay

This repository represents some exploration into the use of [Richard Szalay's Mock HTTP library](https://github.com/richardszalay/mockhttp) to create fully unit tested HTTP Clients that are specific to a particular service your application needs to talk to. The abstraction client can then be mocked itself further up your application's dependency tree to seperate the concerns of your test suites.

## Setup
You need to provide an HTTP endpoint with a straightforward GET method at `http://localhost:3000/answer` and it should return a JSON string with the following structure:

```
{ "code": "4284-2033-7359-1983" }
```

I recommend using [Mockoon](https://mockoon.com/) to create a fake server in a few minutes. Just add a GET request (`/answer`) and make it return OK (200) with the payload in the example snippet above. 

## Commentary and notes

Pretend we have a web service at `http://localhost:3000/answer` and we need to send it a `GET` request to acquire a special code that our application then needs to authenticate the launch of a nuclear missle from the dark side of the Moon. 

### Code
I implement an abstraction client for talking to the webservice, which is essentially the \* GoF Bridge pattern (*Decouple an abstraction from its implementation so that the two can vary independently*). The project is in this Git repository.

I define an interface, `IMagicResultHttpClient`, for the abstraction client.

```C#
using System.Threading.Tasks;

namespace HttpTestProjct2
{
    public interface IMagicResultHttpClient
    {
        public Task<MagicNumber> FetchMagicNumber();
    }
}

```

I define an implementation, `MagicResultHttpClient`, for the abstraction client interface. Notice how it takes an instance of `HttpClient` in the first argument of the constructor, this is a vital detail of this experiment because it provides a clear way to inject a mock instance. 

```C#
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpTestProjct2
{
    public class MagicResultHttpClient : IMagicResultHttpClient
    {
        private HttpClient HTTPClient;
        private string SpecifiedBaseURL;

        public MagicResultHttpClient(HttpClient _httpClient, string _specifiedBaseURL)
        {
            this.HTTPClient = _httpClient;
            this.SpecifiedBaseURL = _specifiedBaseURL;
        }

        public Task<MagicNumber> FetchMagicNumber()
        {
            if (this.HTTPClient == null)
            {
                throw new ArgumentNullException();
            }

            return this.HTTPClient.GetStringAsync(this.SpecifiedBaseURL)
                .ContinueWith(_ => {
                    var magicNumber = JsonConvert.DeserializeObject<MagicNumber>(_.Result);
                    return magicNumber;
                    }
                );
        }
    }
}
```

I implement a dependant class of the abstraction client. Notice how it takes an instance of our client as the first argument, this is for the exact same reasons that we pass an instance of HttpClient into the client itself.  

```C#
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
```

### Unit tests
The unit tests are defined in a seperate project within the same solution, which I've already decided is just a strange and mysterious social more of the .net world that I might as well just move past unless I want to be prosecuted for metaphorical social order offences against the high order of Anders Hejlsberg. The unit test project is in this Git repository and you can take a look at that for more details.

I define a unit test for the abstraction client. As you can see, it uses mock HTTP client mentioned earlier instead of a real one. The tests verify the invokation of HttpClient, the response parsing and the ultimate result. We define a couple of tests to verify it's ability to enforce its requirements to operate correctly.

```C#
using HttpTestProjct2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichardSzalay.MockHttp;
using System;

namespace UnitTestProject2
{
    [TestClass]
    public class MagicResultHttpClientTest
    {
        public const string SpecifiedBaseURL = "http://localhost:3000/answer";

        [TestMethod]
        public void HappyPathTestMathod()
        {
            var mockHttp = new MockHttpMessageHandler();
            var sut = new MagicResultHttpClient(mockHttp.ToHttpClient(), SpecifiedBaseURL);

            var assertion = mockHttp
                .When(SpecifiedBaseURL)
                .Respond("application/json", @"{ 'code': '4284-2033-7359-1983'}");

            var asyncTask = sut.FetchMagicNumber();
            asyncTask.Wait();

            Assert.AreEqual(1, mockHttp.GetMatchCount(assertion));
            Assert.AreEqual("4284-2033-7359-1983", asyncTask.Result.Code);
        }

        [TestMethod]
        public void HappyPathTestMethodAltData()
        {
            const string AltSpecifiedBaseURL = "https://mmymymymymymymymyyssssserver.com/answer";
            var mockHttp = new MockHttpMessageHandler();
            var sut = new MagicResultHttpClient(mockHttp.ToHttpClient(), AltSpecifiedBaseURL);

            var assertion = mockHttp
                .When(AltSpecifiedBaseURL)
                .Respond("application/json", @"{ 'code': '4644-8749-1098-2675'}");

            var asyncTask = sut.FetchMagicNumber();
            asyncTask.Wait();

            Assert.AreEqual(1, mockHttp.GetMatchCount(assertion));
            Assert.AreEqual("4644-8749-1098-2675", asyncTask.Result.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AngryPathTestMethod()
        {
            var mockHttp = new MockHttpMessageHandler();
            var sut = new MagicResultHttpClient(null, SpecifiedBaseURL);

            sut.FetchMagicNumber();
        }
    }
}
```

I define a unit test for the code where the abstration client is used. It mocks out the client and only goes as far as verifying the client's invokation. We already have a test suite for the abstraction client, so we can treat the assertions verified by those tests as facts. \*Injecting mocks in this scenario \*seperates the concerns of each test suite, which keeps a handle on the amount of state each unit test has to be concerned with, and I think this is \*vital for writing well focused tests.

A test suite should set out to verify only the code associated with the unit of code it's associated with, and every responsibility within your application should have its own test suite.

```C#
using HttpTestProjct2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RichardSzalay.MockHttp;
using System;
using System.Threading.Tasks;

namespace UnitTestProject2
{
    [TestClass]
    public class ProgramTest
    {
        [TestMethod]
        public void HappyPathTestMathod()
        {
            var specifiedMagicNumber = new MagicNumber();
            specifiedMagicNumber.Code = "4873-9876-3762-8487";
            var specifiedMagicNumberResponse = new Task<MagicNumber>(() => specifiedMagicNumber);

            var mockMagicResultHttpClient = new Mock<IMagicResultHttpClient>();
            var sut = new Program(mockMagicResultHttpClient.Object);

            var assertion = mockMagicResultHttpClient.Setup(_ => _.FetchMagicNumber()).Returns(Task.FromResult(specifiedMagicNumber));

            var result = sut.Execute();

            Assert.AreEqual(specifiedMagicNumber.Code, result);
            mockMagicResultHttpClient.Verify(_ => _.FetchMagicNumber(), Times.Once());
        }
    }
}
```

### Conclusion

It's a straightforward example, but the basic principles should be applicable to most HTTP calls you'd need to make in a C# application. I think the test setup is fairly clean and the test coverage should be virtually complete. I'd love to discuss the topic of webservice unit testing further and invite rational scrutiny towards my views.

## Footnotes

\* I'd love to have conversations disputing this, I might have a kernel of doubt towards this statement in my heart.