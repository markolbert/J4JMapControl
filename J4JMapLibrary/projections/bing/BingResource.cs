#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// BingResource.cs
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

using System.Text.Json.Serialization;

namespace J4JSoftware.J4JMapLibrary;

public class BingResource
{
    [ JsonPropertyName( "__type" ) ]
    public string Type { get; set; } = string.Empty;

    public float[] BoundingBox { get; set; } = Array.Empty<float>();
    public int ImageHeight { get; set; }
    public int ImageWidth { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string[] ImageUrlSubdomains { get; set; } = Array.Empty<string>();
    public string VintageEnd { get; set; } = string.Empty;
    public string VintageStart { get; set; } = string.Empty;
    public int ZoomMax { get; set; }
    public int ZoomMin { get; set; }
}
