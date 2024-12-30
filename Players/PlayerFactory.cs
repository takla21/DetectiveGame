using Detective.Level;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Detective.Players;

public interface IPlayerFactory
{
    IEnumerable<Player> Create(int playerCount, int playerSize, Clock clock);
}

public class PlayerFactory : IPlayerFactory
{
    private readonly string _namesFilePath;
    private readonly Random _random;
    private readonly ILevelService _levelService;

    public PlayerFactory(Random random, string namesFilePath, ILevelService levelService)
    {
        _random = random;
        _namesFilePath = namesFilePath;
        _levelService = levelService;
    }

    public IEnumerable<Player> Create(int playerCount, int playerSize, Clock clock)
    {
        var players = new List<Player>();

        var killer = _random.Next(playerCount);

        var availableNames = new List<string>();
        using (var stream = Microsoft.Xna.Framework.TitleContainer.OpenStream(_namesFilePath))
        {
            using (var reader = new StreamReader(stream))
            {
                availableNames.AddRange(reader.ReadToEnd().Split("\r\n"));
            }
        }

        for (int i = 0; i < playerCount; i++)
        {
            var nameChoice = _random.Next(availableNames.Count);

            var p = new Player(
                new PlayerProfile(
                    availableNames[nameChoice],
                    _random.Next(18, 99)
                ),
                playerSize
            );

            availableNames.RemoveAt(nameChoice);

            PlayerRoleBase role;

            var scheduleChoice = _random.Next(2);
            IPlayerSchedule schedule;
            if (scheduleChoice == 1)
            {
                schedule = new UnemployedSchedule(_levelService, clock);
            }
            else
            {
                var workPlace = _levelService.Places.First(x => x.Information.Type == PlaceType.Other);

                var isNightShift = _random.Next(2) == 1;
                var start = isNightShift ? _random.Next(18, 22) : _random.Next(7, 11);
                var end = isNightShift ? _random.Next(2, 6) : _random.Next(15, 18);
                var defaultShift = new WorkSchedule(isNightShift, start, end);
                var shifts = Enumerable.Range(0, 7).Select(x => (x == 0 && !isNightShift) || (x == 7 && isNightShift) ? new WorkSchedule(isNightShift) : defaultShift);

                schedule = new WorkerSchedule(_levelService, clock, workPlace, shifts, p.Name);
            }

            // Calculate player position so they all start with a different position while being put in a circle.
            var radialPosition = (i / (playerCount * 1.0)) * 2 * Math.PI;

            // Cast positions into integer to convert back to pixels which improves performance.
            var x = (int)(playerSize * Math.Cos(radialPosition)) + 1000;
            var y = (int)(playerSize * Math.Sin(radialPosition)) + 500;

            if (i == killer)
            {
                role = new Killer(p.Id, new Vector2(x, y), _levelService, schedule);
            }
            else
            {
                role = new Innocent(p.Name, new Vector2(x, y), schedule);
            }

            p.AssignRole(role);

            players.Add(p);
        }

        return players;
    }
}
