using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common
{

    [DataContract]
    public class Result
    {
        [DataMember]
        public bool IsSuccess { get; set; }

        // Sprečava DataContract serijalizator da traži set za izračunato polje
        [IgnoreDataMember]
        [JsonIgnore]
        public bool IsFailure => !IsSuccess;

        [DataMember]
        public string Error { get; set; } = string.Empty;

        public Result() { } // Prazan konstruktor za deserializaciju

        protected Result(bool isSuccess, string error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new(true, string.Empty);
        public static Result Failure(string error) => new(false, error);

        public static Result<T> Success<T>(T value) => new(value, true, string.Empty);
        public static Result<T> Failure<T>(string error) => new(default!, false, error);
    }

    [DataContract]
    public class Result<T> : Result
    {
        [DataMember]
        public T? Value { get; set; }

        public Result() { } // Prazan konstruktor za deserializaciju

        internal Result(T? value, bool isSuccess, string error) : base(isSuccess, error)
        {
            Value = value;
        }
    }
}
