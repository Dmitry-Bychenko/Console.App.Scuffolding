using ConsoleScuffolding;

namespace Scuffolding;

// Entry Point class
internal static class EntryPoint {

  // Entry Point
  private static async Task Main() {
    if (ConsoleHelp.Help())
      return;

    await StartUp.Run();
  }
}
