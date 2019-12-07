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
