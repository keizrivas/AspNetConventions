using Microsoft.AspNetCore.Mvc;

namespace AspNetConventions.ExceptionHandling.Abstractions
{
    public interface IInvalidModelStateFactory
    {
        IActionResult Create(ActionContext context);
    }
}
