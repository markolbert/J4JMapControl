using J4JSoftware.Logging;

namespace J4JMapLibrary;

public partial class MapProjectionFactory
{
    private record TypeInfoBase
    {
        protected TypeInfoBase(
            Type type
        )
        {
            ProcessType( type );
        }

        protected List<List<ParameterInfo>> Constructors { get; }= new();

        private void ProcessType( Type type )
        {
            foreach( var ctor in type.GetConstructors() )
            {
                var argList = new List<ParameterInfo>();

                var ctorParams = ctor.GetParameters();

                for( var argIdx = 0; argIdx < ctorParams.Length; argIdx++ )
                {
                    var argAliases = ctorParams[ argIdx ].ParameterType.GetInterfaces().ToList();
                    argAliases.Add( ctorParams[ argIdx ].ParameterType );

                    var optional = ctorParams[ argIdx ].IsOptional;

                    if( argAliases.Any( x => x.FullName == null ) )
                    {
                        argList.Add( new ParameterInfo( argIdx, ParameterType.Other, optional ) );
                        continue;
                    }

                    if( argAliases.Any( x => x.FullName!.Equals( typeof( IJ4JLogger ).FullName ) ) )
                    {
                        argList.Add( new ParameterInfo( argIdx, ParameterType.Logger, optional ) );
                        continue;
                    }

                    if( argAliases.Any( x => x.FullName!.Equals( typeof( IMapServer ).FullName ) ) )
                    {
                        argList.Add( new ParameterInfo( argIdx, ParameterType.MapServer, optional ) );
                        continue;
                    }

                    if( argAliases.Any( x => x.FullName!.Equals( typeof( IProjectionCredentials ).FullName ) ) )
                    {
                        argList.Add( new ParameterInfo( argIdx, ParameterType.Credentials, optional ) );
                        continue;
                    }

                    argList.Add( argAliases.Any( x => x.FullName!.Equals( typeof( ITileCache ).FullName ) )
                                     ? new ParameterInfo( argIdx, ParameterType.TileCache, optional )
                                     : new ParameterInfo( argIdx, ParameterType.Other, optional ) );
                }

                Constructors.Add( argList );
            }
        }

        private bool ConstructorMatches( ParameterType[] requiredTypes, List<ParameterInfo> paramList )
        {
            var requiredArgs = paramList
                              .Where( x => requiredTypes.Any( y => y == x.Type ) )
                              .Distinct()
                              .ToList();

            var otherArgs = paramList
                           .Where( x => requiredTypes.All( y => y != x.Type ) )
                           .Distinct()
                           .ToList();

            if( requiredArgs.Count != requiredTypes.Length )
                return false;

            if( !otherArgs.Any() || otherArgs.All( x => x.Optional ) )
                return true;

            return false;
        }

        public List<ParameterInfo>? GetConstructor( ParameterType[] requiredTypes )
        {
            foreach( var paramList in Constructors )
            {
                if( !ConstructorMatches( requiredTypes, paramList ) )
                    continue;

                return paramList;
            }

            return null;
        }
    }
}
