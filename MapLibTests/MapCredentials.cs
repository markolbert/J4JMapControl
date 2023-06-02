#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// MapCredentials.cs
//
// This file is part of JumpForJoy Software's WinAppTest.
// 
// WinAppTest is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// WinAppTest is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with WinAppTest. If not, see <https://www.gnu.org/licenses/>.

#endregion

using J4JSoftware.J4JMapLibrary;

namespace MapLibTests;

public class MapCredentials
{
    public BingCredentials? BingCredentials { get; set; }
    public GoogleCredentials? GoogleCredentials { get; set; }
    public OpenStreetCredentials? OpenStreetCredentials { get; set; }
    public OpenTopoCredentials? OpenTopoCredentials { get; set; }
}
