using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

public static class TestModelValidator
{
    public static void ValidateModel(object model, ControllerBase controller)
    {
        var validationContext = new ValidationContext(model, null, null);
        var validationResults = new List<ValidationResult>();

        Validator.TryValidateObject(
            model,
            validationContext,
            validationResults,
            validateAllProperties: true
        );

        foreach (var validationResult in validationResults)
        {
            foreach (var memberName in validationResult.MemberNames)
            {
                controller.ModelState.AddModelError(memberName, validationResult.ErrorMessage);
            }
        }
    }
}
