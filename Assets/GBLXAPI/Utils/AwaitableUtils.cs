// taken from: https://discussions.unity.com/t/awaitable-fromresult/943659/8
using UnityEngine;

/// <summary>
/// Create an Awaitable object from non-Awaitable value
/// </summary>
public static class AwaitableUtils
{
    public static Awaitable<TResult> FromResult<TResult>(TResult result)
        => Result<TResult>.From(result);

    static class Result<TResult>
    {
        static readonly AwaitableCompletionSource<TResult> completionSource = new();

        public static Awaitable<TResult> From(TResult result)
        {
            completionSource.SetResult(result);
            var awaitable = completionSource.Awaitable;
            completionSource.Reset();
            return awaitable;
        }
    }
}
