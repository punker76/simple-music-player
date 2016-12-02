using System.Windows;

namespace SimpleMusicPlayer.FMODStudio
{
    public static class FMODExtensions
    {
        public static bool ERRCHECK(this FMOD.RESULT result, FMOD.RESULT ignoredResults = FMOD.RESULT.OK)
        {
            if (result != FMOD.RESULT.OK && !ignoredResults.HasFlag(result))
            {
                MessageBox.Show("FMOD error! " + result + " - " + FMOD.Error.String(result));
                // todo : show better error info dialog
                return false;
            }
            return true;
        }
    }
}