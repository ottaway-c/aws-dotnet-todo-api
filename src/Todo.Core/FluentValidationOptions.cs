using FluentValidation;
using FluentValidation.Internal;

namespace Todo.Core;

public static class FluentValidationOptions
{
    public static void Configure()
    {
        ValidatorOptions.Global.DisplayNameResolver = ValidatorOptions.Global.PropertyNameResolver;
        ValidatorOptions.Global.MessageFormatterFactory = () => new CustomMessageFormatter();
    }
}

public class CustomMessageFormatter : MessageFormatter
{
    public override string BuildMessage(string messageTemplate)
    {
        var template = messageTemplate.Replace("'", "");
        var message = base.BuildMessage(template);
        return message;
    }
}
