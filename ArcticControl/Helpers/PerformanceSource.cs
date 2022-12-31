using System.Diagnostics;

namespace ArcticControl.Helpers;

internal enum PerformanceSourceType
{
    PerformanceCounter,
    ArcNative,
    ValueOffsetCallback
}

internal struct PerformanceSourceArgs
{
    internal PerformanceSourceType Type;
    internal string[]? PerformanceCounterArgs;
    internal uint[]? ArcNativeArgs;

    /// <summary>
    /// Will only be used if no ValueOffsetCallback has been provided.
    /// </summary>
    internal string Format;
    /// <summary>
    /// Callback to modify value before return.
    /// </summary>
    internal Func<float, string>? ValueOffsetCallback;
}

internal class PerformanceSource
{
    private readonly PerformanceSourceType _type;
    private readonly PerformanceCounter? _perfCounter;
    private readonly string _format;
    private readonly Func<float, string> _valueOffsetCallback;

    internal PerformanceSource(PerformanceSourceArgs args)
    {
        _type = args.Type;
        _format = args.Format;
        _valueOffsetCallback = args.ValueOffsetCallback ?? ((float arg) => arg.ToString(_format));

        switch (_type)
        {
            case PerformanceSourceType.ArcNative:
                throw new NotImplementedException();

            case PerformanceSourceType.PerformanceCounter:
                if (args.PerformanceCounterArgs == null || args.PerformanceCounterArgs.Length < 2)
                {
                    throw new ArgumentException("PerformanceCounterArgs not set or less than 2");
                }
                
                if (args.PerformanceCounterArgs?.Length == 2)
                {
                    _perfCounter = new PerformanceCounter(args.PerformanceCounterArgs[0], args.PerformanceCounterArgs[1]);
                }
                else if (args.PerformanceCounterArgs?.Length == 3)
                {
                    _perfCounter = new PerformanceCounter(
                        args.PerformanceCounterArgs[0], 
                        args.PerformanceCounterArgs[1], 
                        args.PerformanceCounterArgs[2]);
                }
                else
                {
                    throw new ArgumentException("PerformanceCounterArgs must contain 2 or 3 arguments for PerformanceSourceType.PerformanceCounter!");
                }

                break;

            case PerformanceSourceType.ValueOffsetCallback:
                if (_valueOffsetCallback == null)
                {
                    throw new ArgumentNullException("ValueOffsetCallback must be provided if type is set to PerformanceSourceType.ValueOffsetCallback!");
                }

                break;
        }
    }

    /// <summary>
    /// Returns next value.
    /// </summary>
    /// <returns>Value or string.Empty if nothing has been found.</returns>
    internal string NextValue()
    {
        return _valueOffsetCallback(_type switch
        {
            PerformanceSourceType.ArcNative => 0.0f,
            PerformanceSourceType.PerformanceCounter => _perfCounter?.NextValue() ?? 0.0f,
            _ => 0.0f,
        });
    }
}
