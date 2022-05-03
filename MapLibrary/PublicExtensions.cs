using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.MapLibrary
{
    public static class PublicExtensions
    {
        // thanx to 3dGrabber for this
        // https://stackoverflow.com/questions/383587/how-do-you-do-integer-exponentiation-in-c
        public static int Pow( this int bas, int exp ) =>
            Enumerable
               .Repeat( bas, Math.Abs(exp) )
               .Aggregate( 1, ( a, b ) => exp < 0 ? a / b : a * b );
    }
}
