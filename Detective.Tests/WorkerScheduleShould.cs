using AutoFixture;
using Detective.Level;
using Detective.Players;
using Detective.Tests.Utils;
using Detective.Utils;
using FluentAssertions;
using NSubstitute;
using System.Numerics;

namespace Detective.Tests;

public class WorkerScheduleShould
{
    [Theory]
    [InlineData(true, true, true, true, true, false)]
    [InlineData(true, true, true, true, false, false)]
    [InlineData(true, true, true, false, true, false)]
    [InlineData(true, true, true, false, false, false)]
    [InlineData(true, true, false, true, true, true)]
    [InlineData(true, true, false, true, false, false)]
    [InlineData(true, true, false, false, true, true)]
    [InlineData(true, true, false, false, false, false)]
    [InlineData(true, false, true, true, true, false)]
    [InlineData(true, false, true, true, false, false)]
    [InlineData(true, false, true, false, true, false)]
    [InlineData(true, false, true, false, false, false)]
    [InlineData(true, false, false, true, true, true)]
    [InlineData(true, false, false, true, false, false)]
    [InlineData(true, false, false, false, true, true)]
    [InlineData(true, false, false, false, false, false)]
    [InlineData(false, true, true, true, true, false)]
    [InlineData(false, true, true, true, false, false)]
    [InlineData(false, true, true, false, true, false)]
    [InlineData(false, true, true, false, false, false)]
    [InlineData(false, true, false, true, true, true)]
    [InlineData(false, true, false, true, false, false)]
    [InlineData(false, true, false, false, true, true)]
    [InlineData(false, true, false, false, false, false)]
    [InlineData(false, false, true, true, true, false)]
    [InlineData(false, false, true, true, false, false)]
    [InlineData(false, false, true, false, true, false)]
    [InlineData(false, false, true, false, false, false)]
    [InlineData(false, false, false, true, true, true)]
    [InlineData(false, false, false, true, false, false)]
    [InlineData(false, false, false, false, true, true)]
    [InlineData(false, false, false, false, false, false)]

    public void Be_At_Work_When_Time_To_Work_Except_When_Time_To_Sleep(bool isFirstDay, bool isNightShift, bool isDayOff, bool isTimeToSleep, bool isTimeToWork, bool shouldBeAtWork)
    {
        // Arrange
        var fixture = new Fixture();

        // Set up schedule.
        var actualRandom = new Random();

        WorkSchedule scheduleTemplate;
        if (isDayOff)
        {
            scheduleTemplate = new WorkSchedule(isNightShift);
        }
        else
        {
            var start = isNightShift ? actualRandom.Next(18, 22) : actualRandom.Next(7, 11);
            var end = isNightShift ? actualRandom.Next(2, 6) : actualRandom.Next(15, 18);

            scheduleTemplate = new WorkSchedule(
                isNightShift: isNightShift,
                startHour: start,
                endHour: end
            );
        }

        // Set up random

        var mockRandom = Substitute.For<IRandom>();
        var timeToSleep = !isDayOff && isNightShift ? actualRandom.Next(scheduleTemplate.EndHour + 1, 7) : actualRandom.Next(21, 24);
        var timeToWakeUp = !isDayOff && isNightShift ? actualRandom.Next(12, scheduleTemplate.StartHour - 1) : actualRandom.Next(6, isDayOff ? 12 : scheduleTemplate.StartHour);
        mockRandom.Next(Arg.Any<int>(), Arg.Any<int>()).ReturnsForAnyArgs(timeToSleep, timeToWakeUp);

        // Set up level
        var mockLevelService = Substitute.For<ILevelService>();

        var home = fixture
            .Build<Place>()
            .With(x => x.Information, fixture.Build<PlaceInformation>().With(x => x.Type, PlaceType.Houses).Create())
            .Create();

        var places = fixture
            .Build<Place>()
            .With(x => x.Information, fixture.Build<PlaceInformation>().With(x => x.Type, PlaceType.Other).Create())
            .CreateMany()
            .ToList();

        var workPlace = places[actualRandom.Next(places.Count)];

        places.Add(home);

        mockLevelService.Places.Returns(places);

        var levelInformation = fixture.Create<LevelInformation>();

        mockLevelService.Information.Returns(levelInformation);

        // Set up mockLevelPath

        var mockLevelPath = Substitute.For<ILevelPathFinding>();

        var startPoint = fixture.Create<Vector2>();
        var endPoint = isTimeToSleep ? home.Information.EntrancePosition : isTimeToWork ? workPlace.Information.EntrancePosition : fixture.Create<Vector2>();

        // Ensure levelservice always pick endpoint, in case it's being called.
        mockLevelService.PickPointOrPlace().Returns(new LevelChoice(endPoint, null));

        mockLevelPath.GenerateMoves(
            startPoint: startPoint,
            target: endPoint,
            levelWidth: (int)levelInformation.LevelSize.X,
            levelHeight: (int)levelInformation.LevelSize.Y,
            invalidPoints: levelInformation.InvalidPositions
        ).Returns(fixture.CreateMany<DummyMove>());

        WorkSchedule[] schedules = [scheduleTemplate, scheduleTemplate];

        // Create worker

        var mockClock = Substitute.For<IClock>();

        var SUT = new WorkerSchedule(mockLevelService, mockClock, workPlace, schedules, mockRandom, mockLevelPath);

        // Generate current hours

        var currentDay = isFirstDay ? 1 : 0;
        int currentHour;

        if (isTimeToWork)
        {
            currentHour = scheduleTemplate.StartHour;
        }
        else if (isTimeToSleep)
        {
            currentHour = timeToSleep;
        }
        else if (isDayOff)
        {
            currentHour = actualRandom.Next(12, 18);
        }
        else
        {
            var possibleTime = new List<int>();

            for (int i = 0; i < 24; i++)
            {
                // Sleep conditions
                if ((isNightShift && (i >= timeToSleep && i < timeToWakeUp)) ||
                    (!isNightShift && (i >= timeToSleep || i < timeToWakeUp)))
                {
                    continue;
                }

                // work conditions
                if ((isNightShift && (i >= scheduleTemplate.StartHour || i <= scheduleTemplate.EndHour))
                    || (!isNightShift && (i >= scheduleTemplate.StartHour && i <= scheduleTemplate.EndHour)))
                {
                    continue;
                }

                possibleTime.Add(i);
            }

            currentHour = possibleTime[actualRandom.Next(possibleTime.Count)];
        }

        // Act
        mockClock.HourChanged += Raise.Event<ClockTickEventHandler>(new object(), new ClockTickEventArgs(Day: currentDay, Hour: currentHour, Minute: fixture.Create<int>()));

        foreach (var move in SUT.GenerateMoves(startPoint, null))
        {
            move.Execute(fixture.Create<float>(), startPoint, fixture.Create<bool>());
        }

        // Assert

        if (shouldBeAtWork)
        {
            SUT.CurrentPlace.Should().Be(workPlace.Information);
        }
        else
        {
            SUT.CurrentPlace.Should().NotBe(workPlace.Information);
        }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Leave_Work_When_Time_Is_Past_End(bool isNightShift, bool isPastEnd)
    {
        // Arrange
        var fixture = new Fixture();

        // Set up schedule.
        var actualRandom = new Random();
       
        var start = isNightShift ? actualRandom.Next(18, 22) : actualRandom.Next(7, 11);
        var end = isNightShift ? actualRandom.Next(2, 6) : actualRandom.Next(15, 18);

        var scheduleTemplate = new WorkSchedule(
            isNightShift: isNightShift,
            startHour: start,
            endHour: end
        );

        // Set up random
        var mockRandom = Substitute.For<IRandom>();
        var timeToSleep = isNightShift ? actualRandom.Next(scheduleTemplate.EndHour + 1, 7) : actualRandom.Next(21, 24);
        var timeToWakeUp = isNightShift ? actualRandom.Next(12, scheduleTemplate.StartHour - 1) : actualRandom.Next(6, scheduleTemplate.StartHour);
        mockRandom.Next(Arg.Any<int>(), Arg.Any<int>()).ReturnsForAnyArgs(timeToSleep, timeToWakeUp);

        // Set up level
        var mockLevelService = Substitute.For<ILevelService>();

        var home = fixture
            .Build<Place>()
            .With(x => x.Information, fixture.Build<PlaceInformation>().With(x => x.Type, PlaceType.Houses).Create())
            .Create();

        var places = fixture
            .Build<Place>()
            .With(x => x.Information, fixture.Build<PlaceInformation>().With(x => x.Type, PlaceType.Other).Create())
            .CreateMany()
            .ToList();

        var workPlace = places[actualRandom.Next(places.Count)];

        places.Add(home);

        mockLevelService.Places.Returns(places);

        var levelInformation = fixture.Create<LevelInformation>();

        mockLevelService.Information.Returns(levelInformation);

        // Set up mockLevelPath

        var mockLevelPath = Substitute.For<ILevelPathFinding>();

        var startPoint = fixture.Create<Vector2>();
        var endPoint = fixture.Create<Vector2>();

        // Ensure levelservice always pick endpoint, in case it's being called.
        mockLevelService.PickPointOrPlace().Returns(new LevelChoice(endPoint, null));

        mockLevelPath.GenerateMoves(
            startPoint: startPoint,
            target: endPoint,
            levelWidth: (int)levelInformation.LevelSize.X,
            levelHeight: (int)levelInformation.LevelSize.Y,
            invalidPoints: levelInformation.InvalidPositions
        ).Returns(fixture.CreateMany<DummyMove>());

        WorkSchedule[] schedules = [scheduleTemplate, scheduleTemplate];

        // Create worker

        var mockClock = Substitute.For<IClock>();

        var SUT = new WorkerSchedule(mockLevelService, mockClock, workPlace, schedules, mockRandom, mockLevelPath);

        // Generate current hours
        mockClock.HourChanged += Raise.Event<ClockTickEventHandler>(new object(), new ClockTickEventArgs(Day: actualRandom.Next(schedules.Length), Hour: scheduleTemplate.StartHour, Minute: fixture.Create<int>()));

        foreach (var move in SUT.GenerateMoves(startPoint, null))
        {
            move.Execute(fixture.Create<float>(), startPoint, fixture.Create<bool>());
        }

        // Act
        var currentHour = isPastEnd ? scheduleTemplate.EndHour 
            : scheduleTemplate.StartHour + actualRandom.Next(Math.Abs(scheduleTemplate.EndHour - scheduleTemplate.StartHour)) % 24;
        mockClock.HourChanged += Raise.Event<ClockTickEventHandler>(new object(), new ClockTickEventArgs(Day: actualRandom.Next(schedules.Length), Hour: currentHour, Minute: fixture.Create<int>()));

        foreach (var move in SUT.GenerateMoves(startPoint, workPlace.Information))
        {
            move.Execute(fixture.Create<float>(), startPoint, fixture.Create<bool>());
        }

        // Assert
        if (isPastEnd)
        {
            SUT.CurrentPlace.Should().NotBe(workPlace.Information);
        }
        else
        {
            SUT.CurrentPlace.Should().Be(workPlace.Information);
        }
    }
}
