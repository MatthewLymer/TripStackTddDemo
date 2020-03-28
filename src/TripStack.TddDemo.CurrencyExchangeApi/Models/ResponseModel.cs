using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TripStack.TddDemo.CurrencyExchangeApi.Models
{
    public sealed class ResponseModel
    {
        private ResponseModel()
        {
        }

        public string Status { get; private set; }
        public object Data { get; private set; }
        public IEnumerable<object> Errors { get; private set; }

        public static ResponseModel FromSuccess(object data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return new ResponseModel
            {
                Status = "Success",
                Data = data
            };
        }

        public static ResponseModel FromErrors(IEnumerable<object> errors)
        {
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            return new ResponseModel
            {
                Status = "Failure",
                Errors = errors
            };
        }
    }
}