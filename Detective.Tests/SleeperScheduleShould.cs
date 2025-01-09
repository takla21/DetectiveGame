using AutoFixture;
using Detective.Level;
using Detective.Players;
using Detective.Tests.Utils;
using Detective.Utils;
using FluentAssertions;
using FluentAssertions.Execution;
using NSubstitute;
using System.Numerics;

namespace Detective.Tests;

public class SleeperScheduleShould
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Set_IsTimeToSleep_To_True_If_Current_Time_Is_Past_By_TimeToSleep(bool shouldSleep)
    {
        // Arrange
        var fixture = new Fixture();

        var mockClock = Substitute.For<IClock>();
        var mockRandom = Substitute.For<IRandom>();

        var actualRandom = new Random();
        var timeToSleep = actualRandom.Next(21, 24);
        var timeToWakeUp = actualRandom.Next(6, 12);
        mockRandom.Next(Arg.Any<int>(), Arg.Any<int>()).ReturnsForAnyArgs(timeToSleep, timeToWakeUp);

        using var SUT = new SleeperSchedulerTestWrapper(
            home: fixture.Create<Place>(),
            levelInformation: fixture.Create<LevelInformation>(),
            mockClock,
            mockRandom,
            levelPathFinding: Substitute.For<ILevelPathFinding>());

        // Act
        var firstHour = shouldSleep ? timeToSleep : actualRandom.Next(timeToWakeUp + 1, timeToSleep);
        mockClock.HourChanged += Raise.Event<ClockTickEventHandler>(new object(), new ClockTickEventArgs(Day: fixture.Create<int>(), Hour: firstHour, Minute: fixture.Create<int>()));

        // Assert
        SUT.IsTimeToSleepResult.Should().Be(shouldSleep);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Set_IsTimeToSleep_To_False_Once_We_Are_Past_TimeToWakeUp(bool shouldWakeUp)
    {
        // Arrange
        var fixture = new Fixture();

        var mockClock = Substitute.For<IClock>();
        var mockRandom = Substitute.For<IRandom>();

        var actualRandom = new Random();
        var timeToSleep = actualRandom.Next(21, 24);
        var timeToWakeUp = actualRandom.Next(6, 12);
        mockRandom.Next(Arg.Any<int>(), Arg.Any<int>()).ReturnsForAnyArgs(timeToSleep, timeToWakeUp);

        using var SUT = new SleeperSchedulerTestWrapper(
            home: fixture.Create<Place>(),
            levelInformation: fixture.Create<LevelInformation>(),
            mockClock,
            mockRandom,
            levelPathFinding: Substitute.For<ILevelPathFinding>());
        mockClock.HourChanged += Raise.Event<ClockTickEventHandler>(new object(), new ClockTickEventArgs(Day: fixture.Create<int>(), Hour: timeToSleep, Minute: fixture.Create<int>()));

        // Act
        var secondHour = shouldWakeUp ? timeToWakeUp : actualRandom.Next(0, timeToWakeUp);
        mockClock.HourChanged += Raise.Event<ClockTickEventHandler>(new object(), new ClockTickEventArgs(Day: fixture.Create<int>(), Hour: secondHour, Minute: fixture.Create<int>()));

        // Assert
        SUT.IsTimeToSleepResult.Should().NotBe(shouldWakeUp);
    }

    [Fact]
    public void Set_Initial_Times_On_First_ClockUpdate()
    {
        // Arrange
        var fixture = new Fixture();

        var mockClock = Substitute.For<IClock>();
        var mockRandom = Substitute.For<IRandom>();

        var actualRandom = new Random();
        var timeToSleep = actualRandom.Next(0, 24);
        var timeToWakeUp = actualRandom.Next(0, 24);
        mockRandom.Next(Arg.Any<int>(), Arg.Any<int>()).ReturnsForAnyArgs(timeToSleep, timeToWakeUp);

        using var SUT = new SleeperSchedulerTestWrapper(
            home: fixture.Create<Place>(),
            levelInformation: fixture.Create<LevelInformation>(),
            mockClock,
            mockRandom,
            levelPathFinding: Substitute.For<ILevelPathFinding>());

        // Act
        var currentHour = actualRandom.Next(0, 24);
        mockClock.HourChanged += Raise.Event<ClockTickEventHandler>(new object(), new ClockTickEventArgs(Day: fixture.Create<int>(), Hour: currentHour, Minute: fixture.Create<int>()));

        // Assert
        using (new AssertionScope())
        {
            SUT.TimeToSleepResult.Should().Be(timeToSleep);
            SUT.TimeToWakeUpResult.Should().Be(timeToWakeUp);
        }
    }

    [Fact]
    public void Set_New_Times_After_WakeUp()
    {
        // Arrange
        var fixture = new Fixture();

        var mockClock = Substitute.For<IClock>();
        var mockRandom = Substitute.For<IRandom>();

        var actualRandom = new Random();
        var firstTimeToSleep = actualRandom.Next(21, 24);
        var secondTimeToSleep = actualRandom.Next(21, 24);
        var firstTimeToWakeUp = actualRandom.Next(6, 12);
        var secondTimeToWakeUp = actualRandom.Next(6, 12);
        mockRandom.Next(Arg.Any<int>(), Arg.Any<int>()).ReturnsForAnyArgs(firstTimeToSleep, firstTimeToWakeUp, secondTimeToSleep, secondTimeToWakeUp);

        using var SUT = new SleeperSchedulerTestWrapper(
            home: fixture.Create<Place>(),
            levelInformation: fixture.Create<LevelInformation>(),
            mockClock,
            mockRandom,
            levelPathFinding: Substitute.For<ILevelPathFinding>());

        // Act
        mockClock.HourChanged += Raise.Event<ClockTickEventHandler>(new object(), new ClockTickEventArgs(Day: fixture.Create<int>(), Hour: firstTimeToSleep, Minute: fixture.Create<int>()));
        mockClock.HourChanged += Raise.Event<ClockTickEventHandler>(new object(), new ClockTickEventArgs(Day: fixture.Create<int>(), Hour: firstTimeToWakeUp, Minute: fixture.Create<int>()));

        // Assert
        using (new AssertionScope())
        {
            SUT.TimeToSleepResult.Should().Be(secondTimeToSleep);
            SUT.TimeToWakeUpResult.Should().Be(secondTimeToWakeUp);
        }
    }

    [Fact]
    public async Task ClearMoves_Be_Raised_OnSleep()
    {
        // Arrange
        var fixture = new Fixture();

        var mockClock = Substitute.For<IClock>();
        var mockRandom = Substitute.For<IRandom>();

        var actualRandom = new Random();
        var timeToSleep = actualRandom.Next(21, 24);
        var timeToWakeUp = actualRandom.Next(6, 12);
        mockRandom.Next(Arg.Any<int>(), Arg.Any<int>()).ReturnsForAnyArgs(timeToSleep, timeToWakeUp);

        using var SUT = new SleeperSchedulerTestWrapper(
            home: fixture.Create<Place>(),
            levelInformation: fixture.Create<LevelInformation>(),
            mockClock,
            mockRandom,
            levelPathFinding: Substitute.For<ILevelPathFinding>());

        var tcs = new TaskCompletionSource<bool>();
        using var disposable = new ActionDisposable(() => SUT.OnClearMoves -= OnClearMoves);
        SUT.OnClearMoves += OnClearMoves;

        // Act
        mockClock.HourChanged += Raise.Event<ClockTickEventHandler>(new object(), new ClockTickEventArgs(Day: fixture.Create<int>(), Hour: timeToSleep, Minute: fixture.Create<int>()));

        var timeoutTask = Task.Delay(1000);
        await Task.WhenAny(tcs.Task, timeoutTask);

        if (timeoutTask.IsCompleted)
        {
            Assert.Fail("ClearMoves event was not raised.");
        }

        var result = await tcs.Task;

        // Assert
        result.Should().BeTrue();

        void OnClearMoves()
        {
            tcs.SetResult(true);
        }
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    public void GenerateMoves_When_Player_Is_Not_Located_At_House_Including_Enter_And_Exit_Places(bool isOutside, bool isAtHouse, bool shouldBeEmpty)
    {
        // Arrange
        var fixture = new Fixture();

        var home = fixture
            .Build<Place>()
            .With(x => x.Information, fixture.Build<PlaceInformation>().With(x => x.Type, PlaceType.Houses).Create())
            .Create();

        var levelInformation = fixture.Create<LevelInformation>();

        var mockLevelPath = Substitute.For<ILevelPathFinding>();

        var startPoint = fixture.Create<Vector2>();

        var pathMoves = fixture.CreateMany<DummyMove>();

        mockLevelPath.GenerateMoves(
            startPoint: startPoint,
            target: home.Information.EntrancePosition,
            levelWidth: (int)levelInformation.LevelSize.X,
            levelHeight: (int)levelInformation.LevelSize.Y,
            invalidPoints: levelInformation.InvalidPositions
        ).Returns(pathMoves);

        var currentPlace = isOutside ? default
            : isAtHouse ? home.Information
            : fixture.Build<PlaceInformation>().With(x => x.Type, PlaceType.Other).Create();

        using var SUT = new SleeperSchedulerTestWrapper(
            home: home,
            levelInformation: levelInformation,
            clock: Substitute.For<IClock>(),
            random: Substitute.For<IRandom>(),
            mockLevelPath,
            initialPlace: currentPlace,
            initialShouldLeaveOnNextIteration: isOutside ? false : !isAtHouse);

        // Act
        var moves = SUT.GenerateMoves(startPoint, currentPlace);

        // Arrange
        if (shouldBeEmpty)
        {
            moves.Should().BeEmpty();
        }
        else
        {
            using (new AssertionScope())
            {
                moves.Should().NotBeEmpty();
                moves.Should().Contain(pathMoves);

                moves.First().Should().BeOfType<ExecuteAction>();

                if (!isOutside)
                {
                    moves.Last().Should().BeOfType<ExecuteAction>();
                }

                var hasEnteredPlace = false;
                var hasExitedPlace = false;
                using var disposable = new ActionDisposable(() =>
                {
                    SUT.OnPlaceEntered -= OnPlaceEntered;
                    SUT.OnPlaceExited -= OnPlaceExited;
                });
                SUT.OnPlaceEntered += OnPlaceEntered;
                SUT.OnPlaceExited += OnPlaceExited;

                foreach (var move in moves)
                {
                    var result = move.Execute(fixture.Create<float>(), startPoint, fixture.Create<bool>());
                }

                hasEnteredPlace.Should().BeTrue();
                hasExitedPlace.Should().Be(!isOutside);

                void OnPlaceEntered(object sender, PlaceUpdateArgs e)
                {
                    hasEnteredPlace = true;
                }

                void OnPlaceExited(object sender, PlaceUpdateArgs e)
                {
                    hasExitedPlace = true;
                }
            }
        }
    }

    private class SleeperSchedulerTestWrapper : SleeperSchedule
    {
        public SleeperSchedulerTestWrapper(Place home, LevelInformation levelInformation, IClock clock, IRandom random, ILevelPathFinding levelPathFinding, PlaceInformation? initialPlace = null, bool initialShouldLeaveOnNextIteration = false) : base(home, levelInformation, clock, random, levelPathFinding)
        {
            CurrentPlace = initialPlace;
            ShouldLeaveOnNextIteration = initialShouldLeaveOnNextIteration;
        }

        public override double CalculateSuspiciousProbability()
        {
            return 0d;
        }

        public int TimeToSleepResult => TimeToSleep;

        public int TimeToWakeUpResult => TimeToWakeUp;

        public bool IsTimeToSleepResult => IsTimeToSleep;

        public bool ShouldLeaveOnNextIterationResult => ShouldLeaveOnNextIteration;
    }
}
