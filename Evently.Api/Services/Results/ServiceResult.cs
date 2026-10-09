namespace Evently.Api.Services.Results
{
    public class ServiceResult<T> where T : class
    {
        public bool Success { get; init; }

        public bool NotFound { get; init; }

        public string? Message { get; init; }

        public T? Data { get; init; }

        public static ServiceResult<T> Ok(T data)
        {
            return new ServiceResult<T>
            {
                Success = true,
                Data = data
            };
        }

        public static ServiceResult<T> Failure(string message)
        {
            return new ServiceResult<T>
            {
                Success = false,
                Message = message
            };
        }

        public static ServiceResult<T> Missing(string message)
        {
            return new ServiceResult<T>
            {
                Success = false,
                NotFound = true,
                Message = message
            };
        }
    }
}