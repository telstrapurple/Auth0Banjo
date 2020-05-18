using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Banjo.CLI.Model;
using Banjo.CLI.Services;
using FakeItEasy;
using Shouldly;
using Xunit;

namespace Banjo.CLI.Tests.Services
{
    public class TransformingOverridesSourceTests
    {
        [Fact]
        public async Task TestTransformersAreCalledInCorrectOrder()
        {
            var fakeSource = A.Fake<IOverridesSource>();
            var fakeTransformer1 = A.Fake<IOverridesTransformer>();
            var fakeTransformer2 = A.Fake<IOverridesTransformer>();
            var overrides = new Overrides();

            var listOfTransformers = new List<IOverridesTransformer> { fakeTransformer1, fakeTransformer2 };
            A.CallTo(() => fakeSource.GetOverridesAsync()).Returns(overrides);

            var sut = new TransformingOverridesSource(fakeSource, listOfTransformers);
            await sut.GetOverridesAsync();

            A.CallTo(() => fakeSource.GetOverridesAsync())
                .MustHaveHappened();
            A.CallTo(() => fakeTransformer1.TransformAsync(A<Overrides>.That.IsSameAs(overrides))).MustHaveHappened()
                .Then(A.CallTo(() => fakeTransformer2.TransformAsync(A<Overrides>.That.IsSameAs(overrides))).MustHaveHappened());
        }

        [Theory]
        [MemberData(nameof(DifferentLists))]
        public async Task TestDifferentTransformerLists(IEnumerable<IOverridesTransformer> transformers)
        {
            var fakeSource = A.Fake<IOverridesSource>();
            var overrides = new Overrides();
            A.CallTo(() => fakeSource.GetOverridesAsync()).Returns(overrides);
            
            var sut = new TransformingOverridesSource(fakeSource, transformers);
            await sut.GetOverridesAsync(); //no exceptions is a pass
        }

        public static IEnumerable<object[]> DifferentLists => new List<object[]>
        {
            new object[] { null },
            new object[] { new List<IOverridesTransformer>() }
        };
        
        [Fact]
        public async Task TestNullWrappedInstanceThrowsArgNullException()
        {
            Should.Throw<ArgumentNullException>(() => new TransformingOverridesSource(null, null));
        }
    }
}