using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleScuffolding;

internal static class ConsoleHelp {
  #region Algorithm

  private static bool IsHelp() {
    if (Environment.GetCommandLineArgs().Length <= 1)
      return true;

    HashSet<string> helps = new(StringComparer.OrdinalIgnoreCase) {
      "help",
      "hlp",
      "?"
    };

    return Environment
      .GetCommandLineArgs()
      .Any(item => helps.Contains(item.TrimStart('/', '\\', '-')));
  }

  private static string GetName() {
    string? name = Assembly
       .GetEntryAssembly()
      ?.GetCustomAttribute<AssemblyDescriptionAttribute>()
      ?.Description;

    if (string.IsNullOrWhiteSpace(name))
      name = Assembly
        .GetEntryAssembly()
       ?.GetName()
       ?.Name ?? "???";

    string version = Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString() ?? "1.0.0.0";

    return $"{name} vesrion {version}";
  }
  
  private static string GetHelp() {
    return string.Join(Environment.NewLine, 
      $"{GetName()}"
    );
  }

  #endregion Algorithm

  #region Public

  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  public static bool Help() {
    if (!IsHelp())
      return false;

    Console.WriteLine(GetHelp());

    return true;
  }

  #endregion Public
}

