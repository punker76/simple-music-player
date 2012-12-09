using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleMusicPlayer.Common
{
  public class PlayerEngine
  {
    public bool Configure() {
      return false;
    }

    private static PlayerEngine instance;

    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    static PlayerEngine() {
    }

    private PlayerEngine() {
    }

    public static PlayerEngine Instance {
      get { return instance ?? (instance = new PlayerEngine()); }
    }
  }
}