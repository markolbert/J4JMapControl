#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// GoogleCredentials.cs
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

using Microsoft.AspNetCore.DataProtection;

namespace J4JSoftware.J4JMapLibrary;

public class GoogleCredentials : Credentials
{
    private string _apiKey = string.Empty;
    private string _sigSecret = string.Empty;

    public GoogleCredentials()
        : base( typeof( GoogleMapsProjection ) )
    {
    }

    [ CredentialProperty ]
    public string ApiKey
    {
        get => _apiKey;
        set => SetField( ref _apiKey, value );
    }

    [ CredentialProperty ]
    public string SignatureSecret
    {
        get => _sigSecret;
        set => SetField( ref _sigSecret, value );
    }


    public override ICredentials Encrypt( IDataProtector protector ) =>
        new GoogleCredentials()
        {
            ApiKey = protector.Protect( ApiKey ), SignatureSecret = protector.Protect( SignatureSecret )
        };

    public override ICredentials Decrypt( IDataProtector protector ) =>
        new GoogleCredentials
        {
            ApiKey = protector.Unprotect( ApiKey ), SignatureSecret = protector.Unprotect( ApiKey )
        };
}
