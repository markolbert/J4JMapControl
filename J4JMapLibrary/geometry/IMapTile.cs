﻿namespace J4JMapLibrary;

public interface IMapTile
{
    IMapServer MapServer { get; }
    int MaxRequestLatency { get; }
    long ImageBytes { get; }
    event EventHandler? ImageChanged;
    Task<byte[]?> GetImageAsync( bool forceRetrieval = false, CancellationToken ctx = default );

    MapScope GetScope();
}

public interface IMapTile<out TScope> : IMapTile
    where TScope : MapScope
{
    TScope Scope { get; }
}