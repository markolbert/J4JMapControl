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

using J4JSoftware.J4JMapLibrary;
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
            UpdateCaching();
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
            SetValue( FileSystemCacheEntriesProperty, value );
            UpdateCaching();
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
            SetValue( FileSystemCacheRetentionProperty, value );
            UpdateCaching();
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
            SetValue( FileSystemCacheSizeProperty, value );
            UpdateCaching();
        }
    }
}
