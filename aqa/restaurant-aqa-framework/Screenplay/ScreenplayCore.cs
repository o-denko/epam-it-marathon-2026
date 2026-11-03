using Microsoft.Playwright;

namespace RestaurantAqaFramework.Screenplay;

public interface IAbility { }

public class BrowseTheWeb : IAbility
{
    public IPage Page { get; }
    public IBrowserContext Context { get; }

    public BrowseTheWeb(IPage page, IBrowserContext context)
    {
        Page = page;
        Context = context;
    }

    public static BrowseTheWeb With(IPage page, IBrowserContext context) => new(page, context);
}

public class CallAnApi : IAbility
{
    public IAPIRequestContext ApiContext { get; }

    public CallAnApi(IAPIRequestContext apiContext)
    {
        ApiContext = apiContext;
    }

    public static CallAnApi At(IAPIRequestContext apiContext) => new(apiContext);
}

public interface ITask
{
    Task PerformAsAsync(Actor actor);
}

public interface IQuestion<T>
{
    Task<T> AnsweredByAsync(Actor actor);
}

public class Actor
{
    public string Name { get; }
    private readonly Dictionary<Type, IAbility> _abilities = new();

    public Actor(string name)
    {
        Name = name;
    }

    public static Actor Named(string name) => new(name);

    public Actor WhoCan(IAbility ability)
    {
        _abilities[ability.GetType()] = ability;
        return this;
    }

    public T AbilityTo<T>() where T : class, IAbility
    {
        if (_abilities.TryGetValue(typeof(T), out var ability))
        {
            return (T)ability;
        }
        throw new InvalidOperationException($"Actor {Name} does not have ability {typeof(T).Name}");
    }

    public async Task AttemptsToAsync(params ITask[] tasks)
    {
        foreach (var task in tasks)
        {
            await task.PerformAsAsync(this);
        }
    }

    public async Task<T> AsksForAsync<T>(IQuestion<T> question)
    {
        return await question.AnsweredByAsync(this);
    }
}
