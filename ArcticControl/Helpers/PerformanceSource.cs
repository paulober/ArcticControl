using System.Diagnostics;

namespace ArcticControl.Helpers;

internal enum PerformanceSourceType
{
    PerformanceCounter,
    ValueOffsetCallback
}

internal struct PerformanceSourceArgs
{
    internal PerformanceSourceType Type;
    internal string[]? PerformanceCounterArgs;
    internal object? ArcNativeArgs;

    /// <summary>
    /// Will only be used if no ValueOffsetCallback has been provided.
    /// </summary>
    internal string Format;
    /// <summary>
    /// Callback to modify value before return.
    /// </summary>
    internal Func<float, object?, string>? ValueOffsetCallback;
}

internal class PerformanceSource
{
    private readonly PerformanceSourceType _type;
    private PerformanceCounter? _perfCounter;
    private readonly string _format;
    private readonly Func<float, object?, string> _valueOffsetCallback;
    private readonly object? _arcNativeArgs;
    private readonly bool _deferPerfCounterSetup;
    private readonly string[]? _deferedPerfCounterArgs;

    internal PerformanceSource(PerformanceSourceArgs args, bool deferPerfCounterSetup = false)
    {
        _type = args.Type;
        _format = args.Format;
        _valueOffsetCallback = args.ValueOffsetCallback ?? ((float arg, object? arcNativeArgs) => arg.ToString(_format));
        _arcNativeArgs = args.ArcNativeArgs;

        switch (_type)
        {
            case PerformanceSourceType.PerformanceCounter:
                if (args.PerformanceCounterArgs == null || args.PerformanceCounterArgs.Length < 2)
                {
                    throw new ArgumentException("PerformanceCounterArgs not set or less than 2");
                }
                
                if (args.PerformanceCounterArgs?.Length == 2)
                {
                    if (deferPerfCounterSetup)
                    {
                        _deferPerfCounterSetup = true;
                        _deferedPerfCounterArgs = args.PerformanceCounterArgs;
                    }
                    else
                    {
                        _perfCounter = new PerformanceCounter(args.PerformanceCounterArgs[0], args.PerformanceCounterArgs[1]);
                    }
                }
                else if (args.PerformanceCounterArgs?.Length == 3)
                {
                    if (deferPerfCounterSetup)
                    {
                        _deferPerfCounterSetup = true;
                        _deferedPerfCounterArgs = args.PerformanceCounterArgs;
                    }
                    else
                    {
                        _perfCounter = new PerformanceCounter(
                            args.PerformanceCounterArgs[0], 
                            args.PerformanceCounterArgs[1], 
                            args.PerformanceCounterArgs[2]);
                    }
                }
                else
                {
                    throw new ArgumentException("PerformanceCounterArgs must contain 2 or 3 arguments for PerformanceSourceType.PerformanceCounter!");
                }

                break;

            case PerformanceSourceType.ValueOffsetCallback:
                if (_valueOffsetCallback == null)
                {
                    Debug.WriteLine("ValueOffsetCallback must be provided if " +
                                    "type is set to PerformanceSourceType.ValueOffsetCallback!");
                    throw new ArgumentNullException(nameof(args.ValueOffsetCallback));
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void SetupPerfCounterLate()
    {
        if (_deferedPerfCounterArgs?.Length == 2)
        {
            _perfCounter = new PerformanceCounter(_deferedPerfCounterArgs[0], _deferedPerfCounterArgs[1]);
        }
        else if (_deferedPerfCounterArgs?.Length == 3)
        {
            _perfCounter = new PerformanceCounter(
                _deferedPerfCounterArgs[0], 
                _deferedPerfCounterArgs[1], 
                _deferedPerfCounterArgs[2]);
        }
    }
    
    /// <summary>
    /// Returns next value.
    /// </summary>
    /// <returns>Value or string.Empty if nothing has been found.</returns>
    internal string NextValue()
    {
        if (_deferPerfCounterSetup && _perfCounter == null)
        {
            SetupPerfCounterLate();
        }
        
        return _valueOffsetCallback(_type switch
        {
            PerformanceSourceType.PerformanceCounter => _perfCounter?.NextValue() ?? 0.0f,
            _ => 0.0f,
        }, _arcNativeArgs);
    }
}
