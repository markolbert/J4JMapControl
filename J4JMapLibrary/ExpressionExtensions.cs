using System.Linq.Expressions;
using System.Reflection;

namespace J4JSoftware.J4JMapLibrary;

internal static class ExpressionExtensions
{
    public static PropertyInfo GetPropertyInfo<TContainer, TProp>(
        this Expression<Func<TContainer, TProp>> expression
    ) =>
        expression.Body switch
        {
            null => throw new ArgumentNullException( nameof( expression ) ),
            UnaryExpression { Operand: MemberExpression me } => (PropertyInfo) me.Member,
            MemberExpression me => (PropertyInfo) me.Member,
            _ => throw new ArgumentException( $"The expression isn't a valid property. [ {expression} ]" )
        };

    // thanx to https://gist.github.com/jrgcubano/6e4df87913411ee9db0c68efc5fc41a3
    // for these next methods
    public static Action<TEntity, TProperty> CreateSetter<TEntity, TProperty>(
        this PropertyInfo propertyInfo
    )
    {
        var setMethod = propertyInfo.GetSetMethod()
         ?? throw new ArgumentNullException($"Property {propertyInfo.Name} does not have a public setter");

        var instance = Expression.Parameter(typeof(TEntity), "instance");
        var parameter = Expression.Parameter(typeof(TProperty), "param");

        var body = Expression.Call(instance, setMethod, parameter);
        var parameters = new[] { instance, parameter };

        return Expression.Lambda<Action<TEntity, TProperty>>(body, parameters).Compile();
    }

    public static Action<TEntity, TProperty> CreateSetter<TEntity, TProperty>(
        this Expression<Func<TEntity, TProperty>> property
    ) =>
        property.GetPropertyInfo().CreateSetter<TEntity, TProperty>();

    public static Func<TEntity, TProperty> CreateGetter<TEntity, TProperty>(
        this PropertyInfo propertyInfo
    )
    {
        var getMethod = propertyInfo.GetGetMethod()
         ?? throw new ArgumentNullException( $"Property {propertyInfo.Name} does not have a public getter" );

        var instance = Expression.Parameter( typeof( TEntity ), "instance" );

        var body = Expression.Call( instance, getMethod );
        var parameters = new[] { instance };

        return Expression.Lambda<Func<TEntity, TProperty>>( body, parameters ).Compile();
    }

    public static Func<TEntity, TProperty> CreateGetter<TEntity, TProperty>(
        this Expression<Func<TEntity, TProperty>> property
    ) =>
        property.GetPropertyInfo().CreateGetter<TEntity, TProperty>();
}