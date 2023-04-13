#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// file-cache.cs
//
// This file is part of JumpForJoy Software's J4JMapWinLibrary.
// 
// J4JMapWinLibrary is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// J4JMapWinLibrary is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with J4JMapWinLibrary. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    public DependencyProperty FileSystemCachePathProperty = DependencyProperty.Register( nameof( FileSystemCachePath ),
        typeof( string ),
        typeof( J4JMapControl ),
        new PropertyMetadata( string.Empty ) );

    public string FileSystemCachePath
    {
        get => (string) GetValue( FileSystemCachePathProperty );

        set
        {
            SetValue( FileSystemCachePathProperty, value );
            _logger?.LogTrace("File system caching {text}", string.IsNullOrEmpty(value) ? "disabled": $"Enabled at {value}");

            InitializeCaching();
        }
    }

    public DependencyProperty FileSystemCacheEntriesProperty = DependencyProperty.Register(
        nameof( FileSystemCacheEntries ),
        typeof( int ),
        typeof( J4JMapControl ),
        new PropertyMetadata( DefaultFileSystemCacheEntries ) );

    public int FileSystemCacheEntries
    {
        get => (int) GetValue( FileSystemCacheEntriesProperty );

        set
        {
            if (value <= 0)
            {
                _logger?.LogWarning("Invalid file system cache entries limit {limit}, defaulting to {default}",
                                    value,
                                    DefaultFileSystemCacheEntries);

                value = DefaultFileSystemCacheEntries;
            }

            SetValue( FileSystemCacheEntriesProperty, value );
            _logger?.LogTrace("File system cache limited to {entries} entries", value);

            InitializeCaching();
        }
    }

    public DependencyProperty FileSystemCacheRetentionProperty = DependencyProperty.Register(
        nameof( FileSystemCacheRetention ),
        typeof( string ),
        typeof( J4JMapControl ),
        new PropertyMetadata( DefaultFileSystemCacheRetention.ToString() ) );

    public string FileSystemCacheRetention
    {
        get => (string) GetValue( FileSystemCacheRetentionProperty );

        set
        {
            if (!TimeSpan.TryParse(value, out var retention))
            {
                _logger?.LogWarning(
                    "Invalid file system cache retention period '{period}', defaulting to {default}",
                    value,
                    DefaultFileSystemCacheRetention);

                value = DefaultFileSystemCacheRetention.ToString();
            }
            
            SetValue( FileSystemCacheRetentionProperty, value );
            _logger?.LogTrace("File system cache entries retained for up to {retention}", value);

            InitializeCaching();
        }
    }

    public DependencyProperty FileSystemCacheSizeProperty = DependencyProperty.Register( nameof( FileSystemCacheSize ),
        typeof( int ),
        typeof( J4JMapControl ),
        new PropertyMetadata( DefaultFileSystemCacheSize ) );

    public int FileSystemCacheSize
    {
        get => (int) GetValue( FileSystemCacheSizeProperty );

        set
        {
            if (value <= 0)
            {
                _logger?.LogWarning("Invalid file system cache size limit {limit}, defaulting to {default}",
                                    value,
                                    DefaultFileSystemCacheSize);

                value = DefaultFileSystemCacheSize;
            }

            SetValue( FileSystemCacheSizeProperty, value );
            _logger?.LogTrace("File system cache limited to {bytes} bytes", value);

            InitializeCaching();
        }
    }
}
