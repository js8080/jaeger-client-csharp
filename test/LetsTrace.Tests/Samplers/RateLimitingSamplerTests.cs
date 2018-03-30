using System;
using System.Collections.Generic;
using Jaeger.Core.Samplers;
using Jaeger.Core.Util;
using NSubstitute;
using Xunit;

namespace Jaeger.Core.Tests.Samplers
{
    public class RateLimitingSamplerTests
    {
        [Fact]
        public void RateLimitingSampler_Constructor_ThrowsIfRateLimiterIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new RateLimitingSampler(1.0, null));
            Assert.Equal("rateLimiter", ex.ParamName);
        }

        [Fact]
        public void RateLimitingSampler_UsesTheRateLimiter()
        {
            var maxTracesPerSecond = 3.6;
            var traceId = new TraceId(1);
            var expectedTags = new Dictionary<string, object> {
                { SamplerConstants.SamplerTypeTagKey, SamplerConstants.SamplerTypeRateLimiting },
                { SamplerConstants.SamplerParamTagKey, maxTracesPerSecond }
            };
            var rateLimiter = Substitute.For<IRateLimiter>();
            double calledWith = 0;
            var shouldReturn = false;
            rateLimiter.CheckCredit(Arg.Any<double>()).Returns(x => {
                calledWith = (double)x[0];
                return shouldReturn;
            });

            var sampler = new RateLimitingSampler(maxTracesPerSecond, rateLimiter);
            var isSampled = sampler.IsSampled(traceId, "op");

            Assert.False(isSampled.Sampled);
            Assert.Equal(1.0, calledWith);

            shouldReturn = true;
            isSampled = sampler.IsSampled(traceId, "op");

            Assert.True(isSampled.Sampled);
            Assert.Equal(1.0, calledWith);
            Assert.Equal(expectedTags, isSampled.Tags);
        }

        [Fact]
        public void RateLimitingSampler_UsesDefaultRateLimiter()
        {
            var maxTracesPerSecond = 5.4;
            var sampler = new RateLimitingSampler(maxTracesPerSecond);

            Assert.Equal(maxTracesPerSecond, sampler.MaxTracesPerSecond);
            Assert.IsType<RateLimiter>(sampler._rateLimiter);
            sampler.Dispose();
        }
    }
}
