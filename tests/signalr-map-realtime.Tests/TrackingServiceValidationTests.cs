using System;
using System.Collections.Generic;
using FluentAssertions;
using SignalRMapRealtime.Services;
using Xunit;

namespace signalr_map_realtime.Tests
{
    public class TrackingServiceValidationTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public void ValidateVehicleId_PositiveId_ReturnsNoErrors(int vehicleId)
        {
            TrackingServiceValidation.ValidateVehicleId(vehicleId).Should().BeEmpty();
            TrackingServiceValidation.ValidateSessionId(vehicleId).Should().BeEmpty();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ValidateVehicleId_NonPositiveId_ReturnsSingleErrorWithValue(int vehicleId)
        {
            var errors = TrackingServiceValidation.ValidateVehicleId(vehicleId);

            errors.Should().ContainSingle()
                .Which.Should().Contain($"was {vehicleId}");
        }

        [Fact]
        public void ValidateSessionName_NullName_ThrowsArgumentNullException()
        {
            Action act = () => TrackingServiceValidation.ValidateSessionName(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ValidateSessionName_BoundaryLengths_AcceptsUpTo200AndRejectsAbove()
        {
            TrackingServiceValidation.ValidateSessionName(string.Empty).Should().BeEmpty();
            TrackingServiceValidation.ValidateSessionName(new string('a', 200)).Should().BeEmpty();

            TrackingServiceValidation.ValidateSessionName(new string('a', 201))
                .Should().ContainSingle()
                .Which.Should().Be("Session name cannot exceed 200 characters.");
        }

        [Fact]
        public void ValidateRouteId_NullOrPositive_ReturnsNoErrors()
        {
            TrackingServiceValidation.ValidateRouteId(null).Should().BeEmpty();
            TrackingServiceValidation.ValidateRouteId(1).Should().BeEmpty();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void ValidateRouteId_NonPositiveValue_ReturnsSingleError(int routeId)
        {
            TrackingServiceValidation.ValidateRouteId(routeId)
                .Should().ContainSingle()
                .Which.Should().Contain($"was {routeId}.");
        }

        [Fact]
        public void ValidateCancellationReason_NullReason_ThrowsArgumentNullException()
        {
            Action act = () => TrackingServiceValidation.ValidateCancellationReason(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ValidateCancellationReason_BoundaryLengths_AcceptsUpTo500AndRejectsAbove()
        {
            TrackingServiceValidation.ValidateCancellationReason(new string('r', 500)).Should().BeEmpty();

            TrackingServiceValidation.ValidateCancellationReason(new string('r', 501))
                .Should().ContainSingle()
                .Which.Should().Be("Cancellation reason cannot exceed 500 characters.");
        }

        [Fact]
        public void ValidateStartSessionParameters_DefaultsAndValidInput_ReturnsNoErrors()
        {
            TrackingServiceValidation.ValidateStartSessionParameters(7, "Morning run").Should().BeEmpty();
            TrackingServiceValidation.ValidateStartSessionParameters(7, "Morning run", 3).Should().BeEmpty();
        }

        [Fact]
        public void ValidateStartSessionParameters_AllInvalid_CollectsEveryError()
        {
            var errors = TrackingServiceValidation.ValidateStartSessionParameters(
                vehicleId: 0,
                sessionName: new string('n', 201),
                routeId: -1);

            errors.Should().HaveCount(3);
        }

        [Fact]
        public void ValidateStartSessionParameters_NullSessionName_ThrowsInsteadOfReturningError()
        {
            Action act = () => TrackingServiceValidation.ValidateStartSessionParameters(1, null, null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void SessionIdOnlyValidators_ShareSessionIdRules()
        {
            Func<int, IReadOnlyList<string>>[] validators =
            {
                TrackingServiceValidation.ValidatePauseSessionParameters,
                TrackingServiceValidation.ValidateResumeSessionParameters,
                TrackingServiceValidation.ValidateCompleteSessionParameters,
                TrackingServiceValidation.ValidateSessionStatusParameters,
                TrackingServiceValidation.ValidateSessionDistanceParameters,
                TrackingServiceValidation.ValidateSessionSpeedParameters,
            };

            foreach (var validate in validators)
            {
                validate(1).Should().BeEmpty();
                validate(0).Should().ContainSingle()
                    .Which.Should().Contain("Session ID must be a positive integer");
            }
        }

        [Fact]
        public void ValidateCancelSessionParameters_InvalidSessionAndReason_CollectsBothErrors()
        {
            TrackingServiceValidation.ValidateCancelSessionParameters(5, "Vehicle serviced").Should().BeEmpty();

            TrackingServiceValidation.ValidateCancelSessionParameters(0, new string('r', 501))
                .Should().HaveCount(2);
        }
    }
}
