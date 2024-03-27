namespace TaskManager.Domain.Common;

public class Result<TData>
{
    public TData Response { get; set; }
    public Exception Exception { get; set; }
    public bool IsSuccess { get; set; }
    
    public Result(TData response)
    {
        Response = response;
        IsSuccess = true;
    }

    public Result(Exception exception)
    {
        Exception = exception;
        IsSuccess = false;
    }

    public static Result<TData> Ok(TData data)
    {
        return data;
    }

    public static implicit operator Result<TData>(TData response)
    {
        return new Result<TData>(response);
    }

    public static implicit operator Result<TData>(Exception exception)
    {
        return new Result<TData>(exception);
    }
}