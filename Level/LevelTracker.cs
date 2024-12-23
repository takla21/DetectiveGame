using System;
using System.Collections.Generic;

namespace Detective;

public class LevelTracker
{
    private readonly Dictionary<PlaceInformation, IEnumerable<Player>> _placeCrowd;

    public LevelTracker()
    {
        _placeCrowd = new Dictionary<PlaceInformation, IEnumerable<Player>>();
    }

    public void Update(PlaceInformation place, IEnumerable<Player> players)
    {
        _placeCrowd[place] = players;
    }

    public IEnumerable<Player> GetPlayersFromPlace(PlaceInformation place)
    {
        if (_placeCrowd.ContainsKey(place))
        {
            return _placeCrowd[place];
        }
        else
        {
            return Array.Empty<Player>();
        }
    }
}
