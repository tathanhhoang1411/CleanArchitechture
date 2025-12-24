using System;
using System.Collections.Generic;

namespace CleanArchitecture.Application.Utilities
{
    public class ValidationException : Exception
    {
        public List<string> Errors { get; }

        public ValidationException(IEnumerable<string> errors)
            : base("One or more validation errors occurred.")
        {
            Errors = new List<string>(errors);
        }
    }
}
