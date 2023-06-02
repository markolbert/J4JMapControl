#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// BingImageryMetadata.cs
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

public class BingImageryMetadata
{
    public string Copyright { get; set; } = string.Empty;
    public string BrandLogoUri { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string StatusDescription { get; set; } = string.Empty;
    public string AuthenticationResultCode { get; set; } = string.Empty;
    public string[] ErrorDetails { get; set; } = Array.Empty<string>();
    public string TraceId { get; set; } = string.Empty;
    public BingResourceSet[] ResourceSets { get; set; } = Array.Empty<BingResourceSet>();

    public bool IsValid => ResourceSets is [{ Resources.Length: 1 }];

    public BingResource? PrimaryResource => IsValid ? ResourceSets[ 0 ].Resources[ 0 ] : null;
}
