#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// CacheStats.cs
//
// This file is part of JumpForJoy Software's J4JMapLibrary.
// 
// J4JMapLibrary is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// J4JMapLibrary is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with J4JMapLibrary. If not, see <https://www.gnu.org/licenses/>.
#endregion

namespace J4JSoftware.J4JMapLibrary;

public class CacheStats
{
    public CacheStats(
        string name
    )
    {
        Name = name;
    }

    public string Name { get; }
    public long Bytes { get; protected set; }
    public int Entries { get; protected set; }

    // these DateTimes are in UTC
    public DateTime Earliest { get; protected set; } = DateTime.MaxValue;
    public DateTime MostRecent { get; protected set; } = DateTime.MinValue;

    public void Reload( CacheBase cache )
    {
        Bytes = 0;
        Entries = 0;
        Earliest = DateTime.MaxValue;
        MostRecent = DateTime.MinValue;

        foreach( var entry in cache )
        {
            Bytes += entry.ImageBytes;
            Entries++;

            if( entry.LastAccessedUtc > MostRecent )
                MostRecent = entry.LastAccessedUtc;
            {
                if( entry.LastAccessedUtc < Earliest )
                    Earliest = entry.LastAccessedUtc;
            }
        }
    }

    public void Reload( IEnumerable<FileInfo> files )
    {
        Bytes = 0;
        Entries = 0;
        Earliest = DateTime.MaxValue;
        MostRecent = DateTime.MinValue;

        foreach( var file in files )
        {
            Bytes += file.Length;
            Entries++;

            if( file.LastAccessTimeUtc > MostRecent )
                MostRecent = file.LastAccessTimeUtc;
            {
                if( file.LastAccessTimeUtc < Earliest )
                    Earliest = file.LastAccessTimeUtc;
            }
        }
    }

    public void RecordEntry( byte[]? imageData )
    {
        Entries++;

        Bytes += imageData?.Length ?? 0;

        var utcNow = DateTime.UtcNow;
        if( utcNow < Earliest )
            Earliest = utcNow;
        else
        {
            if( utcNow > MostRecent )
                MostRecent = utcNow;
        }
    }
}
