using Avalonia;
using BinaryDifference;

AppBuilder
    .Configure<App>()
    .UsePlatformDetect()
    .LogToTrace()
    .StartWithClassicDesktopLifetime(args);
