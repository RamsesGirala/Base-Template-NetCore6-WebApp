using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebUI.Common
{
    /// <summary>
    /// Esta clase se encarga de que todos los endopoing POST sean tratados con 
    /// una Cultura Generica, para evitar problemas con los formatos numericos y de Fecha
    /// </summary>
    public class InvariantFormValueProviderFactory : IValueProviderFactory
    {
        public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.ActionContext.HttpContext.Request.Method.ToUpper() == "POST")
            {
                var contentType = context.ActionContext.HttpContext.Request.ContentType;
                if (context.ActionContext.HttpContext.Request.HasFormContentType)
                {
                    if (contentType == "application/x-www-form-urlencoded")
                    {
                        var form = context.ActionContext.HttpContext.Request.Form;
                        if (form != null)
                        {
                            var valueProvider = new FormValueProvider(BindingSource.Form,
                                                                form,
                                                                CultureInfo.InvariantCulture);
                            context.ValueProviders.Add(valueProvider);
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}