using System;
using System.Threading.Tasks;
using UnityEngine;

public static class AnimationUtility
{

    public delegate T InterpolateDelegate<T>(T start, T end, float progress);

    public static async Task AnimateOverTime(int durationMilliseconds, Action<float> progressCallback)
    {
        progressCallback(0f);

        try
        {
            var startTime = DateTime.UtcNow;
            while (Application.isPlaying)
            {
                var elapsed = DateTime.UtcNow - startTime;
                var progress = elapsed.TotalMilliseconds / durationMilliseconds;
                progressCallback((float)progress);
                if (progress >= 1)
                {
                    break;
                }

                await Task.Yield();
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (ObjectDisposedException)
        {
        }

        progressCallback(1f);
    }

}
