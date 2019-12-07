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
