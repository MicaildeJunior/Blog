using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Blog.Extensions;

public static class ModelStateExtension
{
    public static List<string> GetErrors(this ModelStateDictionary modelState)
    {
        var result = new List<string>();
        foreach (var item in modelState.Values)
        {
            // Poderia fazer esse Foreach pra trazer os erros, mas tem um jeito mais facil, usando menos linhas de código, usando LINQ
            /*foreach (var error in item.Errors)
            {
                result.Add(error.ErrorMessage);
            }*/
            result.AddRange(item.Errors.Select(erorr => erorr.ErrorMessage));
        }

        return result;
    }
}
